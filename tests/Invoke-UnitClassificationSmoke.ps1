param([string]$Configuration = 'AV')
$ErrorActionPreference = 'Stop'
$directory = Join-Path (Split-Path $PSScriptRoot -Parent) "SSW\bin\x86\$Configuration"
$previous = $env:SSW_CLASSIFICATION_SMOKE
try {
    $env:SSW_CLASSIFICATION_SMOKE = '1'
    $process = Start-Process -FilePath (Join-Path $directory 'SSW.exe') -ArgumentList '--next-ui-smoke' -WorkingDirectory $directory -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0) {
        $diagnostic = Get-Content (Join-Path $directory 'next-ui-smoke-error.log') -Raw -ErrorAction SilentlyContinue
        throw "Classification smoke failed ($($process.ExitCode)): $diagnostic"
    }
    Get-Content (Join-Path $directory 'classification-smoke.txt')
} finally {
    $env:SSW_CLASSIFICATION_SMOKE = $previous
}
