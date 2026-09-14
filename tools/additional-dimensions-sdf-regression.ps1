param(
    [Parameter(Mandatory = $true)]
    [string]$DatabasePath,
    [string]$ReleaseDirectory = 'D:\mdev\SSW\SSW\bin\x86\AV'
)

$ErrorActionPreference = 'Stop'
$provider = Join-Path $ReleaseDirectory 'System.Data.SqlServerCe.dll'
if (!(Test-Path -LiteralPath $DatabasePath)) { throw "SDF non trovato: $DatabasePath" }
if (!(Test-Path -LiteralPath $provider)) { throw "Provider SQL CE non trovato: $provider" }

Set-Location $ReleaseDirectory
Add-Type -Path $provider
$connection = New-Object System.Data.SqlServerCe.SqlCeConnection(
    "Data Source=$DatabasePath;Password=@D3C1L4T2%")
$connection.Open()
try {
    function Scalar([string]$sql) {
        $command = $connection.CreateCommand()
        $command.CommandText = $sql
        return [int]$command.ExecuteScalar()
    }

    $schema = Scalar 'SELECT SchemaVersion FROM CLDatabaseMetadata'
    $feature = Scalar "SELECT COUNT(*) FROM CLDatabaseFeatures WHERE FeatureCode='AdditionalModelDimensions' AND IsEnabled=1"
    $dimensions = Scalar 'SELECT COUNT(*) FROM CLHeatRecoveryModelDimensions'
    $packaging = Scalar 'SELECT COUNT(*) FROM CLHeatRecoveryModelPackaging'
    $duplicateDimensions = Scalar 'SELECT COUNT(*) FROM (SELECT IdHeatRecoveryModel,OrientationScope FROM CLHeatRecoveryModelDimensions GROUP BY IdHeatRecoveryModel,OrientationScope HAVING COUNT(*)>1) d'
    $duplicatePackaging = Scalar 'SELECT COUNT(*) FROM (SELECT IdHeatRecoveryModel,Orientation FROM CLHeatRecoveryModelPackaging GROUP BY IdHeatRecoveryModel,Orientation HAVING COUNT(*)>1) p'
    $orphanDimensions = Scalar 'SELECT COUNT(*) FROM CLHeatRecoveryModelDimensions d LEFT JOIN CLHeatRecoveryModels m ON m.Id=d.IdHeatRecoveryModel WHERE m.Id IS NULL'
    $orphanPackaging = Scalar 'SELECT COUNT(*) FROM CLHeatRecoveryModelPackaging p LEFT JOIN CLHeatRecoveryModels m ON m.Id=p.IdHeatRecoveryModel WHERE m.Id IS NULL'

    if ($schema -lt 6 -or $feature -ne 1 -or $dimensions -eq 0 -or $packaging -eq 0 -or
        $duplicateDimensions -ne 0 -or $duplicatePackaging -ne 0 -or
        $orphanDimensions -ne 0 -or $orphanPackaging -ne 0) {
        throw "SDF dimensionale non valido: schema=$schema feature=$feature dimensions=$dimensions packaging=$packaging duplicates=$duplicateDimensions/$duplicatePackaging orphans=$orphanDimensions/$orphanPackaging"
    }
    Write-Output "SDF dimensionale: OK (schema=$schema, quote=$dimensions, imballaggi=$packaging)"
}
finally {
    $connection.Dispose()
}
