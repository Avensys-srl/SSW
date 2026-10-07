param(
    [ValidateRange(1024, 65535)]
    [int]$Port = 5173
)

$ErrorActionPreference = 'Stop'
$nodeDirectory = Join-Path $env:ProgramFiles 'nodejs'
if (Test-Path -LiteralPath (Join-Path $nodeDirectory 'npm.cmd')) {
    $env:PATH = $nodeDirectory + ';' + $env:PATH
}
if (-not (Get-Command npm.cmd -ErrorAction SilentlyContinue)) {
    throw 'Install Node.js LTS with npm first.'
}
Push-Location (Join-Path $PSScriptRoot 'frontend\ssw-next')
try {
    if (-not (Test-Path -LiteralPath 'node_modules\.package-lock.json')) {
        & npm.cmd ci
        if ($LASTEXITCODE -ne 0) { throw 'Frontend package restore failed.' }
    }
    & npm.cmd run dev -- --port $Port --strictPort
    if ($LASTEXITCODE -ne 0) { throw 'Frontend development server failed.' }
}
finally {
    Pop-Location
}
