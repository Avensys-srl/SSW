param([string]$BuildDirectory = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV'), [switch]$Online)
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    $arguments = @('-NoProfile','-ExecutionPolicy','Bypass','-File',$PSCommandPath,'-BuildDirectory',$BuildDirectory)
    if ($Online) { $arguments += '-Online' }
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" @arguments
    exit $LASTEXITCODE
}
$repo = Split-Path $PSScriptRoot -Parent
$previousCache = $env:SSW_APPLICATION_DOCUMENT_CACHE_DIRECTORY
$root = Join-Path $env:TEMP ('ssw-application-catalog-' + [Guid]::NewGuid().ToString('N'))
$env:SSW_APPLICATION_DOCUMENT_CACHE_DIRECTORY = $root
New-Item -ItemType Directory -Path $root | Out-Null
Copy-Item (Join-Path $repo 'SSW\Resources\application-documents.json') (Join-Path $root 'application-documents.json')
$resolver = [ResolveEventHandler] {
    param($sender,$event)
    $path = Join-Path $BuildDirectory (([Reflection.AssemblyName]$event.Name).Name + '.dll')
    if (Test-Path $path) { return [Reflection.Assembly]::LoadFrom($path) }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
try {
    $assembly = [Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'SSWLib.dll'))
    $brochure = $assembly.GetType('SSW.CLCommercialBrochureService')
    $application = $assembly.GetType('SSW.CLApplicationDocumentService')
    $json = [IO.File]::ReadAllText((Join-Path $root 'application-documents.json'))
    $catalog = $brochure.GetMethod('ParseCatalog').Invoke($null,@($json))
    $candidateMethod = $application.GetMethod('CandidateFiles')
    function Candidates($series,$configuration,$size,$language,$fallback=$true) {
        $candidateMethod.Invoke($null,@($catalog,[string]$series,[string]$configuration,$size,[string]$language,[bool]$fallback))
    }
    function Assert($condition,$message) { if (!$condition) { throw $message } }
    foreach ($language in @('IT','FR','DE','NL','DA','SV','PL','BG','CS','HU','IS','NO','RO','SL')) {
        $candidates = @(Candidates '7' 'ST' 127 $language)
        Assert ($candidates.Count -eq 2 -and $candidates[1].Language -eq 'EN') 'Requested language / EN fallback'
    }
    Assert (@(Candidates '7' 'ST' 127 'EN').Count -eq 1) 'No duplicate EN'
    foreach ($configuration in @('FS','HCI','VS','CFI','CDR','CFD')) { Assert (@(Candidates '7' $configuration 127 'EN').Count -eq 0) 'Wrong variant matched' }
    foreach ($size in @(47,77,126,128,$null)) { Assert (@(Candidates '7' 'ST' $size 'EN').Count -eq 0) 'Wrong size matched' }
    Assert (@(Candidates '1' 'ST' 127 'EN').Count -eq 0) 'Wrong family matched'
    $badCatalog = $brochure.GetMethod('ParseCatalog').Invoke($null,@($json.Replace('ApplicationDocuments/S7/{lang}/ST_127_{lang}_AV.pdf','ApplicationDocuments/S7/{lang}/../../bad.pdf')))
    Assert (@($candidateMethod.Invoke($null,@($badCatalog,'7','ST',127,'EN',$true))).Count -eq 0) 'Traversal accepted'
    if ($Online) {
        $resolve = $application.GetMethod('Resolve')
        $onlineResult = @($resolve.Invoke($null,@('7','ST',127,'IT',$true,$true)))
        Assert ($onlineResult.Count -eq 1 -and $onlineResult[0].Language -eq 'EN') 'Online EN fallback'
        $expected = Join-Path $repo 'SSW\Resources\ApplicationDocuments\S7\EN\ST_127_EN_AV.pdf'
        Assert ((Get-FileHash $expected).Hash -eq (Get-FileHash $onlineResult[0].Path).Hash) 'Downloaded PDF differs from source'
        $offlineResult = @($resolve.Invoke($null,@('7','ST',127,'IT',$false,$true)))
        Assert ($offlineResult.Count -eq 1 -and $offlineResult[0].Path -eq $onlineResult[0].Path) 'Offline cache reuse'
        Assert (@($resolve.Invoke($null,@('7','ST',127,'IT',$false,$false))).Count -eq 0) 'Strict language lookup leaked EN'
    }
    'PASS: application catalog model/size/language/path rules; online PDF and offline cache when requested.'
} finally {
    $env:SSW_APPLICATION_DOCUMENT_CACHE_DIRECTORY = $previousCache
    [AppDomain]::CurrentDomain.remove_AssemblyResolve($resolver)
}
