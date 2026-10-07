param(
    [string]$DevUrl
)

$ErrorActionPreference = 'Stop'
$executable = Join-Path $PSScriptRoot 'SSW\bin\x86\NewUI\SSW.exe'
if (-not (Test-Path -LiteralPath $executable)) {
    throw 'Run build-local.ps1 first.'
}
if (-not (Test-Path -LiteralPath (Join-Path (Split-Path $executable) 'data\DataCentral.sdf'))) {
    throw 'Place a compatible AV DataCentral.sdf in SSW\bin\x86\NewUI\data first.'
}
$previousDevUrl = $env:SSW_NEXT_UI_DEV_URL
try {
    $env:SSW_NEXT_UI_DEV_URL = $DevUrl
    Start-Process -FilePath $executable -WorkingDirectory (Split-Path $executable)
}
finally {
    $env:SSW_NEXT_UI_DEV_URL = $previousDevUrl
}
