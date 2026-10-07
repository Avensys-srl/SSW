param(
    [string]$BinPath = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV')
)

$ErrorActionPreference = 'Stop'

if (-not [Environment]::Is64BitProcess) {
    $resolvedBin = (Resolve-Path $BinPath).Path
} else {
    $x86PowerShell = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
    & $x86PowerShell -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -BinPath $BinPath
    exit $LASTEXITCODE
}

Add-Type -Path (Join-Path $resolvedBin 'System.Data.SqlServerCe.dll')
$sswAssembly = [Reflection.Assembly]::LoadFrom((Join-Path $resolvedBin 'SSWLib.dll'))
$moduleType = $sswAssembly.GetType('SSW.CLModule', $true)
$thermoMethod = $moduleType.GetMethod('termo_calc', [Reflection.BindingFlags]'Public,Static')

$databasePath = Join-Path $resolvedBin 'data\DataCentral.sdf'
$connection = New-Object System.Data.SqlServerCe.SqlCeConnection(
    "Data Source=$databasePath;Password=@D3C1L4T2%;Persist Security Info=False;"
)
$connection.Open()

$command = $connection.CreateCommand()
$command.CommandText = @'
SELECT Id, Code, ModRec, LenRec, NominalAirflow, Airflows,
       Efficiency, HeatRecovered_Winter, HeatRecovered_Summer
FROM CLHeatRecoveryModels
ORDER BY Code
'@
$reader = $command.ExecuteReader()

$supportedExact = @('MODEL 1', 'MODEL 2', 'MODEL 3', 'MODEL NL180')
$supportedRotors = @('500', '600', '700', '1000', '1200', '1300', '1316', '1700')
$issues = New-Object System.Collections.Generic.List[object]
$rows = New-Object System.Collections.Generic.List[object]
$calculatedEfficiencies = New-Object System.Collections.Generic.List[double]

function Add-Issue([object]$row, [string]$kind, [string]$detail) {
    $issues.Add([pscustomobject]@{
        Code = $row.Code
        Model = $row.Model
        Kind = $kind
        Detail = $detail
    })
}

while ($reader.Read()) {
    $row = [pscustomobject]@{
        Id = [int]$reader['Id']
        Code = [string]$reader['Code']
        Model = ([string]$reader['ModRec']).Trim().ToUpperInvariant()
        Length = if ($reader['LenRec'] -is [DBNull]) { [double]::NaN } else { [double]$reader['LenRec'] }
        NominalAirflow = if ($reader['NominalAirflow'] -is [DBNull]) { [double]::NaN } else { [double]$reader['NominalAirflow'] }
        AirflowsRaw = if ($reader['Airflows'] -is [DBNull]) { '' } else { [string]$reader['Airflows'] }
        CatalogEfficiency = if ($reader['Efficiency'] -is [DBNull]) { $null } else { [double]$reader['Efficiency'] }
        CatalogWinterHeat = if ($reader['HeatRecovered_Winter'] -is [DBNull]) { $null } else { [double]$reader['HeatRecovered_Winter'] }
        CatalogSummerHeat = if ($reader['HeatRecovered_Summer'] -is [DBNull]) { $null } else { [double]$reader['HeatRecovered_Summer'] }
    }
    $rows.Add($row)

    $isSupported = $supportedExact -contains $row.Model
    $isSupported = $isSupported -or $row.Model.Contains('MODEL EN366') -or $row.Model.Contains('MODEL ENRFC27')
    if ($row.Model.Contains('MODEL R')) {
        $isSupported = ($supportedRotors | Where-Object { $row.Model.Contains($_) }).Count -gt 0
    }
    if (-not $isSupported) {
        Add-Issue $row 'Unsupported model' "No matching efficiency formula for '$($row.Model)'"
        continue
    }
    if ([double]::IsNaN($row.Length) -or $row.Length -le 0.008) {
        Add-Issue $row 'Invalid length' "LenRec=$($row.Length); formula requires > 0.008"
        continue
    }

    $airflows = @($row.AirflowsRaw -split '\|' | ForEach-Object {
        $value = 0.0
        if ([double]::TryParse($_.Trim().Replace(',', '.'), [Globalization.NumberStyles]::Float,
            [Globalization.CultureInfo]::InvariantCulture, [ref]$value)) { $value }
    })
    if ($airflows.Count -eq 0) {
        Add-Issue $row 'Invalid airflow table' 'Airflows has no parseable values'
        continue
    }

    $sampleAirflows = $airflows | Select-Object -Unique
    foreach ($airflow in $sampleAirflows) {
        foreach ($scenario in @(
            @{ Name = 'Winter'; Tin = 20.0; RHin = 0.50; Tout = -5.0; RHout = 0.80 },
            @{ Name = 'Summer'; Tin = 26.0; RHin = 0.50; Tout = 35.0; RHout = 0.50 }
        )) {
            try {
                $result = $thermoMethod.Invoke($null, @(
                    $scenario.Tin, $scenario.RHin, $scenario.Tout, $scenario.RHout,
                    [double]$airflow, $row.Model, $row.Length, $false
                ))
                $efficiencyPercent = 100.0 * [double]$result.efficiency
                $calculatedEfficiencies.Add($efficiencyPercent)
                if ([double]::IsNaN($efficiencyPercent) -or [double]::IsInfinity($efficiencyPercent)) {
                    Add-Issue $row 'Non-finite efficiency' "$($scenario.Name), airflow=${airflow}: $efficiencyPercent"
                } elseif ($efficiencyPercent -lt 0 -or $efficiencyPercent -gt 100) {
                    Add-Issue $row 'Efficiency outside range' "$($scenario.Name), airflow=${airflow}: $([math]::Round($efficiencyPercent, 3))%"
                }
            } catch {
                Add-Issue $row 'Calculation exception' "$($scenario.Name), airflow=${airflow}: $($_.Exception.InnerException.Message)"
            }
        }
    }
}

$reader.Close()
$connection.Close()

$summary = [pscustomobject]@{
    Database = $databasePath
    CatalogRows = $rows.Count
    FormulaFamilies = ($rows.Model | Sort-Object -Unique).Count
    RowsWithLegacyEfficiency = @($rows | Where-Object { $null -ne $_.CatalogEfficiency }).Count
    RowsWithLegacyWinterHeat = @($rows | Where-Object { $null -ne $_.CatalogWinterHeat }).Count
    RowsWithLegacySummerHeat = @($rows | Where-Object { $null -ne $_.CatalogSummerHeat }).Count
    CalculatedPoints = $calculatedEfficiencies.Count
    CalculatedEfficiencyMinPercent = [math]::Round(($calculatedEfficiencies | Measure-Object -Minimum).Minimum, 3)
    CalculatedEfficiencyMaxPercent = [math]::Round(($calculatedEfficiencies | Measure-Object -Maximum).Maximum, 3)
    Issues = $issues.Count
}

$summary | Format-List
$rows | Group-Object Model | Sort-Object Name | Select-Object Name, Count | Format-Table -AutoSize
if ($issues.Count -gt 0) {
    $issues | Format-Table -AutoSize -Wrap
    exit 1
}

Write-Host 'PASS: all catalog rows map to a formula and all catalog airflow points produce finite efficiencies within 0-100%.'
