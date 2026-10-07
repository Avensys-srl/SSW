param([string]$BinaryDirectory=(Join-Path $PSScriptRoot '..\SSW\bin\x86\AV'))
$ErrorActionPreference='Stop'
if([Environment]::Is64BitProcess){
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -BinaryDirectory $BinaryDirectory
    exit $LASTEXITCODE
}
$binary=(Resolve-Path $BinaryDirectory).Path
$assemblyPath=Join-Path $env:TEMP ('ssw-accessory-language-'+[Guid]::NewGuid().ToString('N')+'.dll')
$source=@'
using System;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using SSW;
public sealed class AccessoryLanguageProbe : MarshalByRefObject {
    public void Run() {
        string root=AppDomain.CurrentDomain.BaseDirectory;
        Directory.SetCurrentDirectory(root);
        var info=Assembly.LoadFrom(Path.Combine(root,"SSW.exe")).GetType("SSW.CLSSWInfo_AV",true);
        CLEnvironment.Current=new CLEnvironment(Path.Combine(root,"data","DataCentral.sdf"),(CLSSWInfo)Activator.CreateInstance(info));
        var context=typeof(CLEnvironment).GetProperty("DCContext").GetValue(CLEnvironment.Current,null);
        object model=null;
        foreach(object item in (IEnumerable)context.GetType().GetProperty("CLDCHeatRecoveryModels").GetValue(context,null)) {
            if((string)item.GetType().GetProperty("Code").GetValue(item,null)=="PRIME 020DL EN") { model=item; break; }
        }
        if(model==null) throw new Exception("PRIME 020DL EN missing");
        int id=(int)model.GetType().GetProperty("Id").GetValue(model,null);
        var getter=typeof(CLNextUiApplicationService).GetMethod("GetAccessories",BindingFlags.NonPublic|BindingFlags.Static);
        System.Threading.Thread.CurrentThread.CurrentUICulture=new CultureInfo("it-IT");
        foreach(string language in new[]{"fr","en","it","fr"}) {
            CLNextUiApplicationService.ApplyLanguage(language);
            var expected=CLSelectionCatalogRepository.GetEffectiveItems(Path.Combine(root,"data","DataCentral.sdf"),id,language);
            var actual=(List<CLNextUiAccessorySummary>)getter.Invoke(null,new object[]{model,new string[0],false});
            if(actual.Count==0) throw new Exception("Accessory result empty");
            foreach(var item in actual) {
                var reference=expected.Find(x=>x.Code==item.Code);
                if(reference==null || item.Name!=reference.Name || item.Description!=reference.Description || item.Category!=reference.CategoryName)
                    throw new Exception(language+" accessory language mismatch: "+item.Code);
            }
            var sensor=actual.Find(x=>x.Code=="APC");
            Console.WriteLine(language+": "+sensor.Name+" / "+sensor.Category+"; "+actual.Count+" accessories matched catalog");
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
    $domain=[AppDomain]::CreateDomain('AccessoryLanguageSmoke',$null,$setup)
    $probe=$domain.CreateInstanceFromAndUnwrap($assemblyPath,'AccessoryLanguageProbe')
    $probe.Run()
} finally {
    if($domain){[AppDomain]::Unload($domain)}
    if(Test-Path -LiteralPath $assemblyPath){Remove-Item -LiteralPath $assemblyPath}
}
