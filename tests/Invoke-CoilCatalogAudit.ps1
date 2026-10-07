# Default: catalog-wide pressure-feedback regression. FullCoilCalculation additionally
# exercises thermal/coil routines and can take substantially longer.
param([string]$BinaryDirectory=(Join-Path $PSScriptRoot '..\SSW\bin\x86\AV'), [string]$OutputPath=(Join-Path $env:TEMP 'ssw-coil-catalog-audit.csv'), [switch]$FullCoilCalculation)
$ErrorActionPreference='Stop'
if ([Environment]::Is64BitProcess) {
    $extra=@()
    if($FullCoilCalculation) { $extra+='-FullCoilCalculation' }
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -BinaryDirectory $BinaryDirectory -OutputPath $OutputPath @extra
    exit $LASTEXITCODE
}
$binary=(Resolve-Path $BinaryDirectory).Path
$assemblyPath=Join-Path $env:TEMP ('coil-audit-'+[Guid]::NewGuid().ToString('N')+'.dll')
$source=@'
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using SSW;
public sealed class CatalogProbe : MarshalByRefObject {
    static MethodInfo Private(string name) { return typeof(CLNextUiApplicationService).GetMethod(name,BindingFlags.Static|BindingFlags.NonPublic); }
    static CLNextUiCalculationResult Core(CLNextUiCalculationInput input,object raw,List<CLCoilDefinition> coils,MethodInfo solver) {
        double q=input.SupplyAirflowM3h, p=input.PressurePa, reg=input.RegulationPercent;
        var season=Private("CalculateSeason");
        var treatment=Private("CalculateAirTreatment");
        var winter=season.Invoke(null,new object[]{raw,"Winter",q,p,reg,-10.0,80.0,20.0,60.0,0.0,0.0});
        var summer=season.Invoke(null,new object[]{raw,"Summer",q,p,reg,32.0,50.0,26.0,50.0,0.0,0.0});
        var water=new List<CLWaterCoilResult>(); var electric=new List<CLElectricHeaterResult>();
        double drop=(double)treatment.Invoke(null,new object[]{input,raw,q,coils,null,null,winter,summer,water,electric});
        bool exceeded=false;
        if(drop>0) {
            var point=solver.Invoke(null,new object[]{raw,q,p+drop,reg});
            exceeded=point==null; reg=exceeded?100:Convert.ToDouble(Get(point,"RegulationPercent"));
            winter=season.Invoke(null,new object[]{raw,"Winter",q,p,reg,-10.0,80.0,20.0,60.0,drop,0.0});
            summer=season.Invoke(null,new object[]{raw,"Summer",q,p,reg,32.0,50.0,26.0,50.0,drop,0.0});
            water.Clear(); electric.Clear();
            double effectiveFlow=Math.Max(1,Convert.ToDouble(Get(Get(winter,"Curves"),"WorkingPointAirflow")));
            treatment.Invoke(null,new object[]{input,raw,effectiveFlow,coils,null,null,winter,summer,water,electric});
        }
        return new CLNextUiCalculationResult { EffectiveRegulationPercent=reg,PressureCapacityExceeded=exceeded,WaterCoilResults=water,AdditionalPressureDropPa=drop };
    }
    static object Get(object o,string p) { return o.GetType().GetProperty(p).GetValue(o,null); }
    static string Csv(object o) { return "\""+Convert.ToString(o,System.Globalization.CultureInfo.InvariantCulture).Replace("\"","\"\"")+"\""; }
    static void Row(StreamWriter w,params object[] fields) { w.WriteLine(string.Join(",",Array.ConvertAll(fields,Csv))); w.Flush(); }
    public void Run(string output,bool full) {
        string root=AppDomain.CurrentDomain.BaseDirectory;
        Directory.SetCurrentDirectory(root);
        var t=Assembly.LoadFrom(Path.Combine(root,"SSW.exe")).GetType("SSW.CLSSWInfo_AV",true);
        CLEnvironment.Current=new CLEnvironment(Path.Combine(root,"data","DataCentral.sdf"),(CLSSWInfo)Activator.CreateInstance(t));
        var rawModels=new Dictionary<string,object>();
        foreach(object raw in (IEnumerable)Get(Get(CLEnvironment.Current,"DCContext"),"CLDCHeatRecoveryModels")) rawModels[Convert.ToString(Get(raw,"Code"))]=raw;
        var solver=typeof(CLSelectionApplicationService).GetMethod("FindCompatibleFanOperatingPoint",BindingFlags.Static|BindingFlags.NonPublic);
        var coilsMethod=typeof(CLCoilPerformanceCalculator).GetMethod("GetAvailableCoils");
        using(var w=new StreamWriter(output,false)) {
            Row(w,"Model","Airflow","Pressure","Coil","Mode","Outcome","Attempts","Regulation","CoilStatuses","Detail");
            foreach(var model in CLNextUiApplicationService.GetModels()) {
                Console.WriteLine("Checking "+model.Code);
                try {
                    object raw=rawModels[model.Code];
                    if(!full) {
                        foreach(double fraction in new[]{0.6,0.8,1.0}) foreach(double p in new[]{100.0,200.0}) {
                            double flow=Math.Round(model.NominalAirflowM3h*fraction);
                            if(flow<=0) { Row(w,model.Code,flow,p,"","","NO_NOMINAL_FLOW",0,0,"",""); continue; }
                            var basePoint=solver.Invoke(null,new object[]{raw,flow,p,70.0});
                            if(basePoint==null) { Row(w,model.Code,flow,p,"","","NO_BASE_DUTY",0,0,"",""); continue; }
                            foreach(double drop in new[]{10.0,42.0,55.0,100.0}) {
                                double reg=Convert.ToDouble(Get(basePoint,"RegulationPercent"));
                                bool stable=false,capacity=false; int attempt=0;
                                for(;attempt<6;attempt++) {
                                    var nextPoint=solver.Invoke(null,new object[]{raw,flow,p+drop,reg});
                                    capacity=nextPoint==null;
                                    double next=capacity?100:Convert.ToDouble(Get(nextPoint,"RegulationPercent"));
                                    if(next==reg) { stable=true; attempt++; break; }
                                    reg=Math.Max(reg,next);
                                }
                                Row(w,model.Code,flow,p,"Synthetic drop "+drop,"SOLVER",!stable?"NONCONVERGENT":capacity?"STABLE_CAPACITY_LIMIT":"STABLE",attempt,reg,"","Pressure solver only; not a full coil calculation");
                            }
                        }
                        continue;
                    }
                    var coils=(List<CLCoilDefinition>)coilsMethod.Invoke(null,new[]{raw});
                    bool hasCoils=coils.Count>0;
                    if(!hasCoils) { Row(w,model.Code,0,0,"","","NO_COILS",0,0,"",""); continue; }
                    foreach(double fraction in new[]{0.6,1.0}) {
                        double q=Math.Round(model.NominalAirflowM3h*fraction);
                        double pressure=100;
                        if(q<=0) { Row(w,model.Code,q,pressure,"","","NO_NOMINAL_FLOW",0,0,"",""); continue; }
                        var point=solver.Invoke(null,new object[]{raw,q,pressure,70.0});
                        if(point==null) { Row(w,model.Code,q,pressure,"","","NO_BASE_DUTY",0,0,"",""); continue; }
                        double start=Convert.ToDouble(Get(point,"RegulationPercent"));
                        var input=new CLNextUiCalculationInput { LanguageCode="en",ModelCode=model.Code,SupplyAirflowM3h=q,ExtractAirflowM3h=q,PressurePa=pressure,MinimumRegulationPercent=70,RegulationPercent=start };
                        foreach(var coil in coils) foreach(string mode in new[]{"HCD","CWD","HWD"}) {
                            input.WaterCoilEnabled=true; input.WaterCoilId=coil.Id; input.WaterCoilMode=mode; input.RegulationPercent=start;
                            try {
                                bool stable=false; CLNextUiCalculationResult result=null; int attempts=0;
                                for(;attempts<6;attempts++) {
                                    result=Core(input,raw,coils,solver);
                                    double next=Math.Max(input.RegulationPercent,result.EffectiveRegulationPercent);
                                    if(next==input.RegulationPercent) { stable=true; attempts++; break; }
                                    input.RegulationPercent=next;
                                }
                                var statuses=new List<string>();
                                foreach(var r in result.WaterCoilResults) statuses.Add(r.ScenarioCode+":"+r.StatusCode);
                                string outcome=!stable?"NONCONVERGENT":result.WaterCoilResults.Count==0?"NO_RESULTS":result.PressureCapacityExceeded?"STABLE_CAPACITY_LIMIT":"STABLE";
                                Row(w,model.Code,q,pressure,coil.Name,mode,outcome,attempts,input.RegulationPercent,string.Join(";",statuses),"");
                            } catch(Exception e) { Row(w,model.Code,q,pressure,coil.Name,mode,"ERROR",0,0,"",e.GetBaseException().Message); }
                        }
                    }
                } catch(Exception e) { Row(w,model.Code,0,0,"","","MODEL_ERROR",0,0,"",e.GetBaseException().Message); }
            }
        }
    }
}
'@
$domain=$null
try {
    Add-Type -TypeDefinition $source -OutputAssembly $assemblyPath -OutputType Library -ReferencedAssemblies @((Join-Path $binary 'SSWLib.dll'),'System.Core.dll','System.Windows.Forms.dll')
    [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes($assemblyPath))
    $setup=New-Object AppDomainSetup
    $setup.ApplicationBase=$binary
    $setup.ConfigurationFile=Join-Path $binary 'SSW.exe.config'
    $domain=[AppDomain]::CreateDomain('CoilCatalogAudit',$null,$setup)
    $probe=$domain.CreateInstanceFromAndUnwrap($assemblyPath,'CatalogProbe')
    $probe.Run($OutputPath,[bool]$FullCoilCalculation)
    Import-Csv $OutputPath | Group-Object Outcome | Select-Object Count,Name | Format-Table
    $failures=@(Import-Csv $OutputPath | Where-Object Outcome -in @('NONCONVERGENT','NO_RESULTS','ERROR','MODEL_ERROR'))
    if($failures.Count) { throw "$($failures.Count) audit failures; see $OutputPath" }
} finally {
    if($domain) { [AppDomain]::Unload($domain) }
    if(Test-Path -LiteralPath $assemblyPath) { Remove-Item -LiteralPath $assemblyPath }
}
