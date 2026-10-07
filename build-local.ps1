param(
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (-not (Test-Path -LiteralPath $vswhere)) {
    throw 'Install Visual Studio Build Tools with the .NET desktop build tools workload.'
}
$msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (-not $msbuild) {
    throw 'MSBuild was not found.'
}
$solution = Join-Path $PSScriptRoot 'SSW.sln'
& $msbuild $solution /t:Restore /p:RestorePackagesConfig=true /m /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'NuGet restore failed.' }
& $msbuild $solution /t:Rebuild "/p:Configuration=$Configuration" /p:Platform=x86 /m /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'SSW build failed.' }
