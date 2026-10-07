param([Parameter(Mandatory=$true)][string]$BuildDirectory, [switch]$Online)
$ErrorActionPreference='Stop'
if ([Environment]::Is64BitProcess) { throw 'Use x86 Windows PowerShell.' }
$repo=(Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$root=Join-Path $env:TEMP ('ssw-brochure-test-'+[Guid]::NewGuid().ToString('N'))
$env:SSW_BROCHURE_CACHE_DIRECTORY=$root
New-Item -ItemType Directory -Path $root | Out-Null
Copy-Item (Join-Path $repo 'SSW\Resources\brochures.json') (Join-Path $root 'brochures.json')
$resolver=[ResolveEventHandler] {
    param($sender,$event)
    $path=Join-Path $BuildDirectory (([Reflection.AssemblyName]$event.Name).Name+'.dll')
    if(Test-Path $path){return [Reflection.Assembly]::LoadFrom($path)}
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
$assembly=[Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'SSWLib.dll'))
$service=$assembly.GetType('SSW.CLCommercialBrochureService')
$parse=$service.GetMethod('ParseCatalog')
$candidates=$service.GetMethod('CandidateFiles')
$json=[IO.File]::ReadAllText((Join-Path $repo 'SSW\Resources\brochures.json'))
$catalog=$parse.Invoke($null,@($json))
function Candidates($series,$configuration,$size,$language,$fallback=$true) {
    $candidates.Invoke($null,@($catalog,[string]$series,[string]$configuration,$size,[string]$language,[bool]$fallback))
}
function Assert($condition,$message) {if(-not $condition){throw $message}}
foreach($series in @('3','6','32')) {
    $result=@(Candidates $series 'OSC' 100 'FR')
    Assert ($result.Count -eq 2 -and $result[0].Id -eq 'GRAND_TERTIAIRE') "Wrong general series $series"
}
foreach($config in @('OSC','SSC')) {
    Assert ((@(Candidates '8' $config 88 'FR'))[0].Id -eq 'PETIT_TERTIAIRE') 'Inclusive upper size boundary'
    Assert ((@(Candidates '8' $config 89 'FR'))[0].Id -eq 'GRAND_TERTIAIRE') 'Inclusive lower size boundary'
}
Assert ((@(Candidates '7' 'HCI' 127 'fr-FR'))[0].Id -eq 'SCHOOL_HCI') 'Regional language normalization'
Assert (@(Candidates '7' 'FS' 127 'FR').Count -eq 0) 'FS must not receive HCI brochure'
Assert (@(Candidates '0' 'LT' 20 'FR').Count -eq 0) 'Unsupported series'
Assert (@(Candidates '8' 'OSC' $null 'FR').Count -eq 0) 'Missing size must not match size rules'
$it=@(Candidates '7' 'HCI' 127 'IT')
Assert ($it.Count -eq 2 -and $it[0].Language -eq 'IT' -and $it[1].Language -eq 'EN') 'Requested language then EN, never FR'
foreach($language in @('BG','CS','HU','IS','NO','RO','SL')) {
    $result=@(Candidates '7' 'HCI' 127 $language)
    Assert ($result[1].Language -eq 'EN') 'English fallback language'
}
Assert (@(Candidates '7' 'HCI' 127 'EN').Count -eq 1) 'No duplicate English candidate'
$bad=$json.Replace('COMMERCIAL/{lang}/BROCHURE_SCHOOL_HCI_{lang}_NEUTRE.pdf','COMMERCIAL/{lang}/../../bad.pdf')
$badCatalog=$parse.Invoke($null,@($bad))
Assert (@($candidates.Invoke($null,@($badCatalog,'7','HCI',127,'FR',$true))).Count -eq 0) 'Reject traversal'
try { $parse.Invoke($null,@($json.Replace('"schema_version": 1','"schema_version": 99'))); throw 'Unexpected schema accepted' } catch { if($_.Exception.Message -eq 'Unexpected schema accepted'){throw} }
if($Online) {
    $resolve=$service.GetMethod('Resolve')
    foreach($case in @(@('3','OSC',100,'GRAND_TERTIAIRE'),@('8','OSC',88,'PETIT_TERTIAIRE'),@('7','HCI',127,'SCHOOL_HCI'))) {
        $found=@($resolve.Invoke($null,@($case[0],$case[1],$case[2],'FR',$true)))
        Assert ($found.Count -eq 1 -and $found[0].Id -eq $case[3] -and $found[0].Language -eq 'FR' -and (Test-Path $found[0].Path)) 'Online FR PDF download'
        $offline=@($resolve.Invoke($null,@($case[0],$case[1],$case[2],'FR',$false)))
        Assert ($offline.Count -eq 1 -and $offline[0].Path -eq $found[0].Path) 'Offline cache reuse'
        $other=@($resolve.Invoke($null,@($case[0],$case[1],$case[2],'IT',$true)))
        Assert ($other.Count -eq 0) 'FR-only PDF must not leak into IT or EN'
    }
    $english=(@(Candidates '7' 'HCI' 127 'EN'))[0]
    New-Item -ItemType Directory -Path (Split-Path $english.Path) -Force | Out-Null
    Copy-Item $found[0].Path $english.Path
    $fallback=@($resolve.Invoke($null,@('7','HCI',127,'IT',$false)))
    Assert ($fallback.Count -eq 1 -and $fallback[0].Language -eq 'EN') 'Offline EN fallback when an English PDF exists'
}
Remove-Item Env:\SSW_BROCHURE_CACHE_DIRECTORY
'Brochure catalog associations, boundaries, languages and path safety passed.'
