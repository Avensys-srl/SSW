param(
    [string]$Configuration = 'AV'
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$releaseDirectory = Join-Path $repo "SSW\bin\x86\$Configuration"
$executable = Join-Path $releaseDirectory 'SSW.exe'
$frontendIndex = Join-Path $releaseDirectory 'frontend\ssw-next\index.html'
$webViewLoader = Join-Path $releaseDirectory 'runtimes\win-x86\native\WebView2Loader.dll'

foreach ($path in @($executable, $frontendIndex, $webViewLoader)) {
    if (-not (Test-Path -LiteralPath $path)) {
        throw "SSW Next UI runtime file not found: $path"
    }
}

$process = Start-Process -FilePath $executable `
    -ArgumentList '--next-ui-smoke' `
    -WorkingDirectory $releaseDirectory `
    -Wait `
    -PassThru
if ($process.ExitCode -ne 0) {
    throw "SSW Next UI application-service smoke failed with exit code $($process.ExitCode)."
}

Write-Host 'SSW Next UI application-service smoke passed.'
