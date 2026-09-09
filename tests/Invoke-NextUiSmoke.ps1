param(
    [string]$Configuration = 'AV'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
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

$nextRuntimeFiles = @(
    (Join-Path $repo 'SSW\CLNextHostForm.cs'),
    (Join-Path $repo 'SSW\CLProgram.cs'),
    (Join-Path $repo 'frontend\ssw-next\src\main.ts')
)
$forbiddenRuntimeReferences = Select-String `
    -LiteralPath $nextRuntimeFiles `
    -Pattern 'CLMainForm|OpenLegacy|legacy\.open'
if ($forbiddenRuntimeReferences) {
    $details = $forbiddenRuntimeReferences |
        ForEach-Object { "$($_.Path):$($_.LineNumber): $($_.Line.Trim())" }
    throw "SSW Next runtime contains a legacy UI dependency:`n$($details -join "`n")"
}

Write-Host 'SSW Next runtime legacy-dependency gate passed.'

$process = Start-Process -FilePath $executable `
    -ArgumentList '--next-ui-smoke' `
    -WorkingDirectory $releaseDirectory `
    -WindowStyle Hidden `
    -Wait `
    -PassThru
if ($process.ExitCode -ne 0) {
    throw "SSW Next UI application-service smoke failed with exit code $($process.ExitCode)."
}

Write-Host 'SSW Next UI application-service smoke passed.'

foreach ($step in @('layout', 'layout-transitions', 'layout-review', 'layout-review-accepted', 'dimensional-drawing', 'co2', 'sound', 'documents')) {
    $screenshot = Join-Path $env:TEMP (
        "ssw-next-$step-" + [Guid]::NewGuid().ToString('N') + '.png')
    $screenshotError = $screenshot + '.error.log'
    try {
        $screenshotProcess = Start-Process -FilePath $executable `
            -ArgumentList @(
                '--next-ui-screenshot',
                ('"' + $screenshot + '"'),
                $step
            ) `
            -WorkingDirectory $releaseDirectory `
            -WindowStyle Hidden `
            -Wait `
            -PassThru
        if ($screenshotProcess.ExitCode -ne 0) {
            $detail = if (Test-Path -LiteralPath $screenshotError) {
                Get-Content -LiteralPath $screenshotError -Raw
            } else {
                'No screenshot diagnostic was produced.'
            }
            throw "SSW Next UI screenshot '$step' failed with exit code $($screenshotProcess.ExitCode).`n$detail"
        }
        if (-not (Test-Path -LiteralPath $screenshot) -or
            (Get-Item -LiteralPath $screenshot).Length -lt 10000) {
            throw "SSW Next UI screenshot '$step' was not created or is unexpectedly small."
        }

        $image = [System.Drawing.Image]::FromFile($screenshot)
        try {
            if ($image.Width -lt 1000 -or $image.Height -lt 700) {
                throw "SSW Next UI screenshot '$step' has invalid dimensions: $($image.Width)x$($image.Height)."
            }
            Write-Host "SSW Next UI screenshot '$step' passed: $($image.Width)x$($image.Height)."
        } finally {
            $image.Dispose()
        }
    } finally {
        Remove-Item -LiteralPath $screenshot -Force -ErrorAction SilentlyContinue
        Remove-Item -LiteralPath $screenshotError -Force -ErrorAction SilentlyContinue
    }
}
