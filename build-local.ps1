param(
    [ValidateSet('AV')]
    [string]$Configuration = 'AV'
)

$ErrorActionPreference = 'Stop'
$nodeDirectory = Join-Path $env:ProgramFiles 'nodejs'
if (Test-Path -LiteralPath (Join-Path $nodeDirectory 'npm.cmd')) {
    $env:PATH = $nodeDirectory + ';' + $env:PATH
}
if (-not (Get-Command npm.cmd -ErrorAction SilentlyContinue)) {
    throw 'Install Node.js LTS (including npm) to build the new UI assets.'
}
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
& $msbuild (Join-Path $PSScriptRoot 'SSW\SSW.csproj') /t:Restore /p:RestorePackagesConfig=true "/p:SolutionDir=$PSScriptRoot\" /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'SSW application package restore failed.' }
$outputDirectory = Join-Path $PSScriptRoot 'SSW\bin\x86\NewUI\'
& $msbuild $solution /t:Rebuild "/p:Configuration=$Configuration" /p:Platform=x86 "/p:OutputPath=$outputDirectory" /p:IncludeCSS=false /m /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'SSW build failed.' }
