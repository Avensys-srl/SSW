param(
    [Parameter(Mandatory = $true)][string]$BuildOutputDir,
    [Parameter(Mandatory = $true)][string]$InstallerPath,
    [Parameter(Mandatory = $true)][string]$OutputPath,
    [string]$Channel = 'stable',
    [string]$PublishedDirectory = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 2.0

function Get-Artifact([string]$Name, [string]$Path, [string]$Version = '') {
    if (-not (Test-Path -LiteralPath $Path)) { throw "Required release artifact not found: $Path" }
    $file = Get-Item -LiteralPath $Path
    return [ordered]@{
        name = $Name
        filename = $file.Name
        version = $Version
        size_bytes = $file.Length
        sha256 = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
    }
}

$build = (Resolve-Path $BuildOutputDir).Path
$installer = (Resolve-Path $InstallerPath).Path
$sswExe = Join-Path $build 'SSW.exe'
$sswLib = Join-Path $build 'SSWLib.dll'
$softwareVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($sswLib).FileVersion
if ([string]::IsNullOrWhiteSpace($softwareVersion)) { throw 'SSWLib.dll has no FileVersion.' }
$applicationVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($sswExe).FileVersion
if ([string]::IsNullOrWhiteSpace($applicationVersion)) { throw 'SSW.exe has no FileVersion.' }
if (([Version]$applicationVersion) -ne ([Version]$softwareVersion)) {
    throw "Release binaries are misaligned: SSW.exe=$applicationVersion, SSWLib.dll=$softwareVersion."
}

$databasePath = Join-Path $build 'data\DataCentral.sdf'
$powershell32 = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
$probe = Join-Path $PSScriptRoot 'Get-SdfManifest.ps1'
$databaseMetadata = (& $powershell32 -NoProfile -ExecutionPolicy Bypass -File $probe -DatabasePath $databasePath | ConvertFrom-Json)
if ($LASTEXITCODE -ne 0 -or $null -eq $databaseMetadata) { throw 'Unable to inspect the SDF manifest.' }

$artifacts = @()
$artifacts += Get-Artifact 'application' $sswExe $softwareVersion
$artifacts += Get-Artifact 'calculation-engine' $sswLib $softwareVersion
$artifacts += Get-Artifact 'database' $databasePath ([string]$databaseMetadata.data_version)
foreach ($report in @('CLMainReport.rdlc','CLMainReportWithCO2.rdlc','CLMainReport_Coil.rdlc','CLMainReportWithCO2_Coil.rdlc')) {
    $artifacts += Get-Artifact 'report-template' (Join-Path $build $report) '2'
}
$coilEngine = Join-Path $build 'COILcalc.dll'
if (Test-Path -LiteralPath $coilEngine) {
    $coilVersion = [Diagnostics.FileVersionInfo]::GetVersionInfo($coilEngine).FileVersion
    $artifacts += Get-Artifact 'water-coil-engine' $coilEngine $coilVersion
}

$installerArtifact = Get-Artifact 'installer' $installer $softwareVersion
$previous = $null
if ($PublishedDirectory -and (Test-Path -LiteralPath $PublishedDirectory)) {
    $candidates = @()
    foreach ($file in Get-ChildItem -LiteralPath $PublishedDirectory -Filter 'SSW_Setup_*.manifest.json' -File) {
        try {
            $candidate = Get-Content -LiteralPath $file.FullName -Raw | ConvertFrom-Json
            $candidateVersion = [Version]$candidate.release.version
            if ($candidateVersion -lt [Version]$softwareVersion) { $candidates += $candidate }
        } catch { }
    }
    if ($candidates.Count -gt 0) {
        $previous = $candidates | Sort-Object { [Version]$_.release.version } -Descending | Select-Object -First 1
    }
}

$manifest = [ordered]@{
    manifest_version = 1
    channel = $Channel
    release = [ordered]@{
        version = $softwareVersion
        published_at_utc = [DateTime]::UtcNow.ToString('o')
        installer = $installerArtifact
    }
    compatibility = [ordered]@{
        minimum_ssw_version = [string]$databaseMetadata.minimum_ssw_version
        database_schema_version = [int]$databaseMetadata.schema_version
        selection_format_version = 1
        report_template_version = 2
        api_contract_version = 1
    }
    database = [ordered]@{
        data_version = [string]$databaseMetadata.data_version
        exporter_version = [string]$databaseMetadata.exporter_version
        content_hash = [string]$databaseMetadata.content_hash
        customer_code = [string]$databaseMetadata.customer_code
    }
    components = $artifacts
    rollback = if ($null -eq $previous) { $null } else { [ordered]@{
        version = [string]$previous.release.version
        filename = [string]$previous.release.installer.filename
        sha256 = [string]$previous.release.installer.sha256
    }}
}

$outputDirectory = Split-Path ([IO.Path]::GetFullPath($OutputPath)) -Parent
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$json = $manifest | ConvertTo-Json -Depth 8
[IO.File]::WriteAllText($OutputPath, $json, (New-Object Text.UTF8Encoding($false)))
Write-Output "Update manifest ready: $OutputPath"
