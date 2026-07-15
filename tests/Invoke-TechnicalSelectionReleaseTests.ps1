param(
    [string]$Configuration = 'AV',
    [string]$Platform = 'x86',
    [switch]$SkipBuild,
    [switch]$SkipApiTests
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe'
if (-not (Test-Path $msbuild)) { throw 'Visual Studio 2019 MSBuild not found.' }

if (-not $SkipBuild) {
    & $msbuild (Join-Path $repo 'SSW.sln') "/p:Configuration=$Configuration" "/p:Platform=$Platform" /m /v:minimal
    if ($LASTEXITCODE -ne 0) { throw 'SSW build failed.' }
}

$versionProps = Get-Content -LiteralPath (Join-Path $repo 'SSWVersion.props') -Raw
$versionMatch = [regex]::Match($versionProps, '<SSWVersion>\s*([^<]+?)\s*</SSWVersion>')
if (-not $versionMatch.Success) { throw 'SSWVersion.props does not define SSWVersion.' }
$expectedVersion = [Version]$versionMatch.Groups[1].Value
$releaseDirectory = Join-Path $repo "SSW\bin\x86\$Configuration"
foreach ($name in @('SSW.exe', 'SSWLib.dll')) {
    $path = Join-Path $releaseDirectory $name
    if (-not (Test-Path -LiteralPath $path)) { throw "Release binary not found: $path" }
    $actualVersion = [Version]([Diagnostics.FileVersionInfo]::GetVersionInfo($path).FileVersion)
    if ($actualVersion -ne $expectedVersion) {
        throw "$name version $actualVersion does not match SSWVersion $expectedVersion."
    }
}

$smokeProject = Join-Path $repo 'tests\SelectionIdentitySmoke\SelectionIdentitySmoke.csproj'
& $msbuild $smokeProject /p:Configuration=Release /p:Platform=x86 /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'SelectionIdentitySmoke build failed.' }
$smoke = Join-Path $repo 'SSWLib\bin\x86\Release\SelectionIdentitySmoke.exe'
& $smoke
if ($LASTEXITCODE -ne 0) { throw 'SelectionIdentitySmoke failed.' }

$x86PowerShell = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
& $x86PowerShell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Invoke-AlternativeReferenceSmoke.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Alternative reference smoke test failed.' }

& (Join-Path $PSScriptRoot 'Invoke-PublishedReleaseRetentionSmoke.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Published release retention smoke test failed.' }

$resourceFiles = Get-ChildItem (Join-Path $repo 'SSWLib') -Filter 'Resources.*.resx'
if ($resourceFiles.Count -ne 12) { throw "Expected 12 localized RESX files, found $($resourceFiles.Count)." }
$requiredKeys = @('Update_Title','Update_CheckFailed','Update_PackageIntegrityFailed',
    'MainForm_SelectionRegistration_Title','Water',
    'MainForm_CoilPerformance_ResultAirOut',
    'MainForm_CoilPerformance_InvalidCoolingTemperatures',
    'MainForm_CoilPerformance_InvalidHeatingTemperatures',
    'MainForm_CoilPerformance_LowWaterDeltaT',
    'MainForm_CoilPerformance_CriticalWaterDeltaT',
    'MainForm_CoilPerformance_WaterPressureDropWarning',
    'MainForm_Project_CreateAlternative',
    'MainForm_CoilPerformance_DimensionsNote',
    'MainForm_CoilPerformance_CustomDisclaimerTitle',
    'MainForm_CoilPerformance_CustomDisclaimer',
    'MainForm_CoilPerformance_CustomDisclaimerAccepted')
foreach ($file in $resourceFiles) {
    [xml]$document = Get-Content -LiteralPath $file.FullName -Raw
    $names = @($document.root.data | ForEach-Object { $_.name })
    foreach ($key in $requiredKeys) {
        if ($names -notcontains $key) { throw "$($file.Name) is missing resource $key." }
    }
}

foreach ($report in @('CLMainReport.rdlc','CLMainReportWithCO2.rdlc','CLMainReport_Coil.rdlc','CLMainReportWithCO2_Coil.rdlc')) {
    [xml](Get-Content -LiteralPath (Join-Path $repo "SSWLib\$report") -Raw) | Out-Null
}

if (-not $SkipApiTests) {
    $php = 'C:\xampp\php\php.exe'
    $api = 'A:\webavensys\api'
    if (-not (Test-Path $php) -or -not (Test-Path $api)) { throw 'PHP runtime or deployed API not found.' }
    & $php (Join-Path $api 'tests\technical_selection_service_test.php')
    if ($LASTEXITCODE -ne 0) { throw 'Technical-selection API tests failed.' }
    Get-ChildItem $api -Recurse -Filter '*.php' -File |
        Where-Object { $_.FullName -notmatch '\\vendor\\' } |
        ForEach-Object {
            & $php -l $_.FullName | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "PHP lint failed: $($_.FullName)" }
        }
}

Write-Output 'Technical-selection release matrix passed.'
