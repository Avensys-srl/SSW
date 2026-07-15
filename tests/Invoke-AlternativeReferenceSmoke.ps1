$ErrorActionPreference = 'Stop'

$releaseDirectory = Join-Path (Split-Path $PSScriptRoot -Parent) 'SSW\bin\x86\AV'
Set-Location $releaseDirectory
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $releaseDirectory 'SSWLib.dll'))
$type = $assembly.GetType('SSW.CLMainForm', $true)
$method = $type.GetMethod(
    'Project_NextAlternativeReference',
    [Reflection.BindingFlags]'NonPublic,Static')

$cases = @(
    @('Progetto cliente', 'Alt. 01: Progetto cliente'),
    @('Alt. 01: Progetto cliente', 'Alt. 02: Progetto cliente'),
    @(' alt. 09 : Progetto cliente ', 'Alt. 10: Progetto cliente'),
    @('', 'Alt. 01')
)

foreach ($case in $cases) {
    $actual = $method.Invoke($null, @($case[0]))
    if ($actual -ne $case[1]) {
        throw "Expected '$($case[1])', got '$actual'."
    }
}

Write-Output 'Alternative reference progression passed.'
