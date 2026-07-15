param(
    [Parameter(Mandatory = $true)][string]$InstallerPath,
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [string]$HealthUrl = 'https://www.avensys-srl.com/api/health.php',
    [string]$UpdateUrl = 'https://www.avensys-srl.com/api/ssw_check_update.php'
)

$ErrorActionPreference = 'Stop'
$installer = Get-Item -LiteralPath $InstallerPath
$manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
if ($manifest.manifest_version -lt 1) { throw 'Invalid release manifest version.' }
if ($manifest.release.installer.filename -ne $installer.Name) { throw 'Installer filename does not match manifest.' }
if ([int64]$manifest.release.installer.size_bytes -ne $installer.Length) { throw 'Installer size does not match manifest.' }
$hash = (Get-FileHash -LiteralPath $installer.FullName -Algorithm SHA256).Hash
if ($hash -ne [string]$manifest.release.installer.sha256) { throw 'Installer SHA-256 does not match manifest.' }

$health = Invoke-RestMethod -Uri $HealthUrl -TimeoutSec 20
if ($health.status -ne 'ok' -or -not $health.checks.database -or -not $health.checks.geoip_local) {
    throw 'Technical-selection API health check is not healthy.'
}
$update = Invoke-RestMethod -Uri $UpdateUrl -TimeoutSec 20
if ([string]$update.latest_version -eq [string]$manifest.release.version -and -not $update.verified_manifest) {
    throw 'Published update is missing its verified manifest.'
}

Write-Output "Release readiness passed: version=$($manifest.release.version) sha256=$hash api=$($health.status)"
