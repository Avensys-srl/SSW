param(
    [Parameter(Mandatory = $true)][string]$InstallerPath,
    [Parameter(Mandatory = $true)][string]$ManifestPath,
    [Parameter(Mandatory = $true)][string]$PublishDirectory
)

$ErrorActionPreference = 'Stop'

$installer = Get-Item -LiteralPath $InstallerPath
$manifestFile = Get-Item -LiteralPath $ManifestPath
$manifest = Get-Content -LiteralPath $manifestFile.FullName -Raw | ConvertFrom-Json
$expectedInstaller = $manifest.release.installer
$sourceHash = (Get-FileHash -LiteralPath $installer.FullName -Algorithm SHA256).Hash

if ([string]$expectedInstaller.filename -ne $installer.Name) {
    throw 'Installer filename does not match the update manifest.'
}
if ([int64]$expectedInstaller.size_bytes -ne $installer.Length) {
    throw 'Installer size does not match the update manifest.'
}
if ([string]$expectedInstaller.sha256 -ne $sourceHash) {
    throw 'Installer SHA-256 does not match the update manifest.'
}

New-Item -ItemType Directory -Path $PublishDirectory -Force | Out-Null
$publishedInstaller = Join-Path $PublishDirectory $installer.Name
$publishedManifest = Join-Path $PublishDirectory $manifestFile.Name
$pendingSuffix = '.pending-' + [Guid]::NewGuid().ToString('N')
$pendingInstaller = $publishedInstaller + $pendingSuffix
$pendingManifest = $publishedManifest + $pendingSuffix

try {
    Copy-Item -LiteralPath $installer.FullName -Destination $pendingInstaller -Force
    Copy-Item -LiteralPath $manifestFile.FullName -Destination $pendingManifest -Force

    $pendingHash = (Get-FileHash -LiteralPath $pendingInstaller -Algorithm SHA256).Hash
    if ($pendingHash -ne $sourceHash) {
        throw 'Published installer verification failed before activation.'
    }

    Move-Item -LiteralPath $pendingInstaller -Destination $publishedInstaller -Force
    Move-Item -LiteralPath $pendingManifest -Destination $publishedManifest -Force
} finally {
    Remove-Item -LiteralPath $pendingInstaller, $pendingManifest -Force -ErrorAction SilentlyContinue
}

$publishedHash = (Get-FileHash -LiteralPath $publishedInstaller -Algorithm SHA256).Hash
$publishedManifestData = Get-Content -LiteralPath $publishedManifest -Raw | ConvertFrom-Json
if ($publishedHash -ne $sourceHash -or
    [string]$publishedManifestData.release.installer.sha256 -ne $sourceHash) {
    throw 'Published release verification failed after activation; previous releases were retained.'
}

$currentFiles = @($installer.Name, $manifestFile.Name)
$obsoleteFiles = @(
    Get-ChildItem -LiteralPath $PublishDirectory -File |
        Where-Object {
            ($_.Name -like 'SSW_Setup_*.exe' -or $_.Name -like 'SSW_Setup_*.manifest.json') -and
            $_.Name -notin $currentFiles
        }
)

foreach ($obsoleteFile in $obsoleteFiles) {
    Remove-Item -LiteralPath $obsoleteFile.FullName -Force
}

Write-Output "Published release $($manifest.release.version): $publishedInstaller"
Write-Output "Removed obsolete public artifacts: $($obsoleteFiles.Count)"
