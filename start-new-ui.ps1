$ErrorActionPreference = 'Stop'
$executable = Join-Path $PSScriptRoot 'SSW\bin\x86\NewUI\SSW.exe'
if (-not (Test-Path -LiteralPath $executable)) {
    throw 'Run build-local.ps1 first.'
}
if (-not (Test-Path -LiteralPath (Join-Path (Split-Path $executable) 'data\DataCentral.sdf'))) {
    throw 'Place a compatible AV DataCentral.sdf in SSW\bin\x86\NewUI\data first.'
}
Start-Process -FilePath $executable -WorkingDirectory (Split-Path $executable)
