param([Parameter(Mandatory=$true)][string]$BuildDirectory)
$ErrorActionPreference='Stop'
if ([Environment]::Is64BitProcess) { throw 'Run using x86 Windows PowerShell.' }
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$fixture = Join-Path $repo 'docs\examples\selection-v1.sswsel'
$root = Join-Path $env:TEMP ('ssw-offer-archive-test-' + [Guid]::NewGuid().ToString('N'))
$env:SSW_OFFER_ARCHIVE_PATH=$root
New-Item -ItemType Directory $root | Out-Null
$resolver = [ResolveEventHandler] {
    param($sender,$event)
    $path=Join-Path $BuildDirectory (([Reflection.AssemblyName]$event.Name).Name + '.dll')
    if (Test-Path $path) { return [Reflection.Assembly]::LoadFrom($path) }
    return $null
}
[AppDomain]::CurrentDomain.add_AssemblyResolve($resolver)
[Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'SSWLib.dll')) | Out-Null
$assembly=[Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'SSW.exe'))
$archive=$assembly.GetType('SSW.CLOfferArchive')
$flags=[Reflection.BindingFlags]'NonPublic,Static'
$read=$archive.GetMethod('Read',$flags)
$hostType=$assembly.GetType('SSW.CLNextHostForm')
$save=$hostType.GetMethod('SaveOfferReminderSelection',$flags)
$document=[SSW.CLSelectionProjectSerializer]::Load($fixture)
$document.Identity.PublicReference='1111-2222-3333-4444'
$document.Identity.OfferStatus='Definitive'
$document.Identity.Revision=1
$document.Identity.OfferRevision=1
$document.Identity.OfferDefinitiveAtUtc=[DateTime]::UtcNow
$first=$save.Invoke($null,@($document))
$before=(Get-FileHash $first).Hash
if ($read.Invoke($null,@()).Count -ne 1) { throw 'Definitive offer without reminder missing.' }
$document.Identity.Revision=2
$document.Identity.OfferRevision=2
$second=$save.Invoke($null,@($document))
if ($first -eq $second -or (Get-FileHash $first).Hash -ne $before) { throw 'Revision overwrote previous offer.' }
Copy-Item $second (Join-Path $root 'old-duplicate.sswsel')
if ($read.Invoke($null,@()).Count -ne 2) { throw 'Archive revision grouping failed.' }
$document.Identity.OfferStatus='Provisional'
[SSW.CLSelectionProjectSerializer]::Save((Join-Path $root 'draft.sswsel'),$document)
if ($read.Invoke($null,@()).Count -ne 2) { throw 'Provisional incorrectly listed as definitive.' }
Remove-Item Env:\SSW_OFFER_ARCHIVE_PATH
'Offer archive passed: no-reminder visibility, revisions preserved, reprints deduplicated, provisional excluded.'
