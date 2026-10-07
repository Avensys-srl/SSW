param([Parameter(Mandatory=$true)][string]$PackageDirectory,
      [Parameter(Mandatory=$true)][string]$TestRoot)
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$TestRoot = [IO.Path]::GetFullPath($TestRoot)
if (Test-Path $TestRoot) { throw 'Use a new empty test directory.' }
New-Item -ItemType Directory -Path $TestRoot | Out-Null
$env:SSW_NEXT_SMOKE_REPORT_DIRECTORY = Join-Path $TestRoot 'reports'
function Run([string]$exe, [string[]]$arguments) {
    $process = Start-Process -FilePath $exe -ArgumentList $arguments -WorkingDirectory ([IO.Path]::GetDirectoryName($exe)) -WindowStyle Hidden -Wait -PassThru
    if ($process.ExitCode -ne 0) { throw "Process failed ($($process.ExitCode)): $exe" }
}
function Install([string]$version, [string]$directory) {
    Run (Join-Path $PackageDirectory ('SSW_Setup_' + $version.Replace('.','_') + '.exe')) @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART','/SP-',('/DIR="' + $directory + '"'),('/LOG="' + (Join-Path $TestRoot ($version + '-setup.log')) + '"'))
    $actual = [Diagnostics.FileVersionInfo]::GetVersionInfo((Join-Path $directory 'SSW.exe')).FileVersion
    if ([Version]$actual -ne [Version]$version) { throw "Wrong installed version: $actual" }
}
# Separate AppId and no shortcuts are required in these test-only packages.
$fresh = Join-Path $TestRoot 'fresh'
Install '2.0.0.24' $fresh
Run (Join-Path $fresh 'SSW.exe') @('--next-ui-smoke')
Run (Join-Path $fresh 'unins000.exe') @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART')
$upgrade = Join-Path $TestRoot 'upgrade'
Install '2.0.0.23' $upgrade
$selection = Join-Path $upgrade 'preserved-selection.sswsel'
Copy-Item (Join-Path $repo 'docs\examples\selection-v1.sswsel') $selection
$before = (Get-FileHash $selection -Algorithm SHA256).Hash
Install '2.0.0.24' $upgrade
if ((Get-FileHash $selection -Algorithm SHA256).Hash -ne $before) { throw 'Installer changed a saved selection.' }
Run (Join-Path $upgrade 'SSW.exe') @('--next-ui-smoke')
Run (Join-Path $upgrade 'SSW.exe') @('--next-ui-screenshot',('"' + (Join-Path $TestRoot 'installed-startup.png') + '"'),'project')
[pscustomobject]@{freshVersion='2.0.0.24';fromVersion='2.0.0.23';toVersion='2.0.0.24';selectionSha256=$before;serviceSmoke='passed';scope='isolated AppId; not actual customer registration or license activation'} |
    ConvertTo-Json | Set-Content (Join-Path $TestRoot 'result.json')
'Fresh installation and 2.x upgrade passed.'
