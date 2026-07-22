param(
    [Parameter(Mandatory = $true)][string]$DatabasePath,
    [string]$SqlCeAssemblyPath = ''
)

$ErrorActionPreference = 'Stop'
$sourceDatabase = (Resolve-Path $DatabasePath).ProviderPath
$temporaryDatabase = Join-Path ([IO.Path]::GetTempPath()) ('ssw-sdf-probe-' + [Guid]::NewGuid().ToString('N') + '.sdf')
Copy-Item -LiteralPath $sourceDatabase -Destination $temporaryDatabase
$database = $temporaryDatabase
$buildDirectory = Split-Path (Split-Path $sourceDatabase -Parent) -Parent
$assemblyCandidates = @(
    $SqlCeAssemblyPath,
    (Join-Path $buildDirectory 'System.Data.SqlServerCe.dll'),
    (Join-Path (Split-Path $PSScriptRoot -Parent) 'SSW\bin\x86\AV\System.Data.SqlServerCe.dll'),
    (Join-Path (Split-Path $PSScriptRoot -Parent) 'SSWLib\bin\x86\Release\System.Data.SqlServerCe.dll')
)
$sqlCeAssembly = $assemblyCandidates | Where-Object { $_ -and (Test-Path -LiteralPath $_) } | Select-Object -First 1
if (-not $sqlCeAssembly) { throw 'System.Data.SqlServerCe.dll was not found.' }
Add-Type -Path $sqlCeAssembly

$connectionString = 'Data Source="{0}"; Password="{1}"' -f $database, '@D3C1L4T2%'
$connection = New-Object System.Data.SqlServerCe.SqlCeConnection($connectionString)
try {
    $connection.Open()
    $command = $connection.CreateCommand()
    $command.CommandText = 'SELECT SchemaVersion, DataVersion, ExporterVersion, CustomerCode, MinimumSSWVersion, ContentHash FROM CLDatabaseMetadata'
    $reader = $command.ExecuteReader()
    try {
        if (-not $reader.Read()) { throw 'CLDatabaseMetadata is empty.' }
        $result = [ordered]@{
            schema_version = [int]$reader['SchemaVersion']
            data_version = [string]$reader['DataVersion']
            exporter_version = [string]$reader['ExporterVersion']
            customer_code = [string]$reader['CustomerCode']
            minimum_ssw_version = [string]$reader['MinimumSSWVersion']
            content_hash = [string]$reader['ContentHash']
        }
        if ($reader.Read()) { throw 'CLDatabaseMetadata contains multiple rows.' }
    } finally { $reader.Dispose() }

    $featureCommand = $connection.CreateCommand()
    $featureCommand.CommandText = 'SELECT FeatureCode, FeatureVersion, IsEnabled FROM CLDatabaseFeatures ORDER BY FeatureCode'
    $featureReader = $featureCommand.ExecuteReader()
    try {
        $features = @()
        while ($featureReader.Read()) {
            $features += [ordered]@{
                code = [string]$featureReader['FeatureCode']
                version = [int]$featureReader['FeatureVersion']
                enabled = [bool]$featureReader['IsEnabled']
            }
        }
        $result.features = $features
    } finally { $featureReader.Dispose() }
    $result | ConvertTo-Json -Depth 4 -Compress
} finally {
    $connection.Dispose()
    if ($temporaryDatabase -and (Test-Path -LiteralPath $temporaryDatabase)) {
        Remove-Item -LiteralPath $temporaryDatabase -Force
    }
}
