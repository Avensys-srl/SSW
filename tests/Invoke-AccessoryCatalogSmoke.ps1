param(
    [string]$DatabasePath = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV\data\DataCentral.sdf'),
    [string]$BinaryDirectory = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV')
)

$ErrorActionPreference = 'Stop'

if ([Environment]::Is64BitProcess) {
    $x86PowerShell = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
    & $x86PowerShell -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath `
        -DatabasePath $DatabasePath -BinaryDirectory $BinaryDirectory
    exit $LASTEXITCODE
}

$database = (Resolve-Path -LiteralPath $DatabasePath).Path
$binaryDirectory = (Resolve-Path -LiteralPath $BinaryDirectory).Path
$sqlCeAssembly = Join-Path $binaryDirectory 'System.Data.SqlServerCe.dll'
$sswAssembly = Join-Path $binaryDirectory 'SSWLib.dll'

[void][Reflection.Assembly]::LoadFrom($sqlCeAssembly)
[void][Reflection.Assembly]::LoadFrom($sswAssembly)

$connectionString = 'Data Source="{0}"; Password="{1}"' -f $database, '@D3C1L4T2%'
$connection = New-Object System.Data.SqlServerCe.SqlCeConnection($connectionString)
$connection.Open()
try {
    $command = $connection.CreateCommand()
    $command.CommandText = @'
SELECT TOP (1) IdHeatRecoveryModel, COUNT(*) AS RelationCount
FROM CLHeatRecoveryModelSelectionItems
GROUP BY IdHeatRecoveryModel
'@
    $reader = $command.ExecuteReader()
    if (-not $reader.Read()) { throw 'No effective accessory relations found in the SDF.' }
    $modelId = [int]$reader['IdHeatRecoveryModel']
    $expectedCount = [int]$reader['RelationCount']
    $reader.Close()
}
finally {
    $connection.Dispose()
}

$items = [SSW.CLSelectionCatalogRepository]::GetEffectiveItems($database, $modelId, 'it')
if ($items.Count -ne $expectedCount) {
    throw "Repository returned $($items.Count) items; expected $expectedCount for model $modelId."
}
if (($items | Where-Object { $_.DefaultQuantity -lt 1 -or $_.MaxQuantity -lt $_.DefaultQuantity }).Count -gt 0) {
    throw 'Invalid quantity bounds returned by the repository.'
}
if (($items | Where-Object { $_.IsStandard -and -not $_.DefaultSelected }).Count -gt 0) {
    throw 'A standard item is not selected by default.'
}

$functionCount = ($items | Where-Object { $_.FunctionNames.Count -gt 0 }).Count
Write-Host "Accessory catalog smoke passed: model=$modelId items=$($items.Count) linkedAccessories=$functionCount"
