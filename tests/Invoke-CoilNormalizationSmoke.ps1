param([string]$BinaryDirectory = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV'))
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -BinaryDirectory $BinaryDirectory
    exit $LASTEXITCODE
}
$binary = (Resolve-Path -LiteralPath $BinaryDirectory).Path
$assemblyPath = Join-Path $env:TEMP ('ssw-coil-normalization-' + [Guid]::NewGuid().ToString('N') + '.dll')
$source = @'
using System;
using System.IO;
using System.Reflection;
using System.Text;
using SSW;
public sealed class CoilNormalizationProbe : MarshalByRefObject
{
    public string Run()
    {
        string root = AppDomain.CurrentDomain.BaseDirectory;
        Directory.SetCurrentDirectory(root);
        var infoType = Assembly.LoadFrom(Path.Combine(root, "SSW.exe")).GetType("SSW.CLSSWInfo_AV", true);
        var info = (CLSSWInfo)Activator.CreateInstance(infoType);
        CLEnvironment.Current = new CLEnvironment(Path.Combine(root, "data", "DataCentral.sdf"), info);
        var input = new CLNextUiCalculationInput {
            LanguageCode = "en", ModelCode = "CLRC 223 SSC",
            SupplyAirflowM3h = 2000, ExtractAirflowM3h = 2000,
            PressurePa = 100, MinimumRegulationPercent = 70,
            LayoutCode = "A2", WaterCoilMode = "HCD"
        };
        var candidate = CLNextUiApplicationService.Preselect(input).Find(x => x.Model.Code == input.ModelCode);
        if (candidate == null) throw new Exception("Reproduction unit missing from preselection.");
        double startingRegulation = candidate.RequiredRegulationPercent;
        input.RegulationPercent = startingRegulation;
        var initial = CLNextUiApplicationService.Calculate(input);
        var coil = initial.AvailableWaterCoils.Find(x => x.Name == "CWD 163");
        if (coil == null) throw new Exception("Reproduction coil CWD 163 missing.");
        input.WaterCoilId = coil.Id;
        input.WaterCoilEnabled = true;
        var trace = new StringBuilder();
        foreach (string mode in new[] { "HCD", "CWD", "HWD" })
        {
            input.WaterCoilMode = mode;
            input.RegulationPercent = startingRegulation;
            bool converged = false;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                var result = CLNextUiApplicationService.Calculate(input);
                trace.AppendLine(mode + " " + attempt + ": " + input.RegulationPercent.ToString("R") + " -> " + result.EffectiveRegulationPercent.ToString("R") + "; coil drop=" + result.AdditionalPressureDropPa);
                if (result.WaterCoilResults.Count == 0) throw new Exception("Coil results missing.");
                if (input.MinimumRegulationPercent != 70) throw new Exception("Search threshold changed.");
                if (result.PressureCapacityExceeded) throw new Exception("Unexpected pressure capacity failure.");
                if (result.Winter.Curves.WorkingPointPressurePa + 0.5 < 100) throw new Exception("Requested pressure not met.");
                if (result.EffectiveRegulationPercent == input.RegulationPercent)
                {
                    converged = true;
                    break;
                }
                input.RegulationPercent = Math.Max(input.RegulationPercent, result.EffectiveRegulationPercent);
            }
            if (!converged) throw new Exception("Normalization did not converge:\n" + trace);
        }
        return trace.ToString();
    }
}
'@
$domain = $null
try {
    Add-Type -TypeDefinition $source -OutputAssembly $assemblyPath -OutputType Library `
        -ReferencedAssemblies @((Join-Path $binary 'SSWLib.dll'), 'System.Core.dll', 'System.Windows.Forms.dll')
    [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes($assemblyPath))
    $setup = New-Object AppDomainSetup
    $setup.ApplicationBase = $binary
    $setup.ConfigurationFile = Join-Path $binary 'SSW.exe.config'
    $domain = [AppDomain]::CreateDomain('CoilNormalizationSmoke', $null, $setup)
    $probe = $domain.CreateInstanceFromAndUnwrap($assemblyPath, 'CoilNormalizationProbe')
    Write-Host ($probe.Run())
    Write-Host 'CLRC 223 SSC / CWD 163: HCD, CWD and HWD normalization passed.'
} finally {
    if ($domain) { [AppDomain]::Unload($domain) }
    if (Test-Path -LiteralPath $assemblyPath) { Remove-Item -LiteralPath $assemblyPath -Force }
}
