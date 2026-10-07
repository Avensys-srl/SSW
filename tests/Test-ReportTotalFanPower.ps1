param([string]$BuildDirectory = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV'))
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -BuildDirectory $BuildDirectory
    exit $LASTEXITCODE
}
try {
    $assembly = [Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'SSWLib.dll'))
    $service = $assembly.GetType('SSW.CLNextUiReportService', $true)
    $flags = [Reflection.BindingFlags]'NonPublic,Static'
    $total = $service.GetMethod('CalculateTotalFanPower', $flags)
    foreach ($season in @('Winter','Summer')) {
        foreach ($case in @(@(16,24,40),@(0,0,0),@(16,$null,$null))) {
            $result = [Activator]::CreateInstance($assembly.GetType('SSW.CLSeasonCalculationResult', $true))
            $result.SupplyBranch.AbsorbedPowerW = $case[0]
            $result.ExtractBranch.AbsorbedPowerW = $case[1]
            $result.ScenarioCode = $season
            $actual = $total.Invoke($null,@($result))
            if ($actual -ne $case[2]) { throw 'Total must sum both branches, including zero; missing data must stay blank' }
            if ($result.SupplyBranch.AbsorbedPowerW -ne $case[0]) { throw 'Branch power not preserved' }
        }
    }
    $repo = Split-Path $PSScriptRoot -Parent
    foreach ($language in @('en','it','fr','de','nl','da','sv','pl','bg','cs','hu','is','no','ro','sl')) {
        [xml]$resource = Get-Content (Join-Path $repo "SSWLib\Resources.$language.resx") -Raw
        $labels = @($resource.root.data | Where-Object { $_.name -eq 'PDF_TotalUnitPower' })
        if ($labels.Count -ne 1 -or !$labels[0].value.EndsWith('[W]')) { throw "Missing/duplicate localized power caption: $language" }
    }
    'PASS: winter/summer, unequal branches, zero/missing power and 15 localized captions.'
} catch { throw }
