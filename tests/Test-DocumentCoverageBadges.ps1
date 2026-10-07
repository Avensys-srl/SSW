param([Parameter(Mandatory=$true)][string]$BuildDirectory)
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) { throw 'Run with SysWOW64 Windows PowerShell (x86).' }
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'SSW.exe'))
$library = [Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'SSWLib.dll'))
$form = $assembly.GetType('SSW.CLDocumentCoverageAuditForm', $true)
$rowType = $library.GetType('SSW.CLProductDocumentCoverageRow', $true)
$flags = [Reflection.BindingFlags]'NonPublic,Static'
$htmlMethod = $form.GetMethod('CommercialBadgesHtml', $flags)
$missingMethod = $form.GetMethod('HasMissingTrackedDocument', $flags)
foreach ($status in @('Online', 'Missing', 'Unverified')) {
    $row = [Activator]::CreateInstance($rowType)
    $row.CommercialSheetOnlineStatus['EN'] = $status
    foreach ($language in @('BG','CS','HU','IS','NO','RO','SL')) {
        $row.CommercialSheetOnlineStatus[$language] = 'Missing'
    }
    $html = $htmlMethod.Invoke($null, @($row))
    foreach ($language in @('BG','CS','HU','IS','NO','RO','SL')) {
        $color = if ($status -eq 'Online') { 'ok' } else { 'bad' }
        if ($html -notmatch ('class="badge ' + $color + '" title="EN: ' + $status + '">' + $language + '</span>')) {
            throw "Incorrect fallback badge $language for EN=$status"
        }
    }
    $missing = $missingMethod.Invoke($null, @($row))
    if ($missing -ne ($status -eq 'Missing')) { throw "Incorrect missing filter for EN=$status" }
}
$directory = Join-Path ([IO.Path]::GetTempPath()) ([Guid]::NewGuid().ToString('N') + '\EN')
[IO.Directory]::CreateDirectory($directory) | Out-Null
$path = Join-Path $directory 'coverage-test_EN_AV.pdf'
try {
    [IO.File]::WriteAllText($path, 'Test fixture; not a production document.')
    $row = [Activator]::CreateInstance($rowType)
    $row.CommercialSheetPaths.Add($path)
    $row.CommercialSheetOnlineStatus['EN'] = 'Missing'
    $html = $htmlMethod.Invoke($null, @($row))
    if ($html -notmatch 'Fallback EN available' -or $missingMethod.Invoke($null, @($row))) {
        throw 'Offline EN must provide fallback even when online EN is missing.'
    }
} finally { Remove-Item -LiteralPath $path -ErrorAction SilentlyContinue }
$stepMethod = $form.GetMethod('StepAvailability', $flags)
foreach ($status in @('Online', 'Missing', 'Unverified')) {
    $row = [Activator]::CreateInstance($rowType)
    $row.StepModelOnlineStatus = $status
    $expected = switch ($status) { 'Online' { 'Online' } 'Missing' { 'Mancante / Missing' } default { 'Online non verificato / Unverified' } }
    if ($stepMethod.Invoke($null, @($row)) -ne $expected) { throw "Incorrect STEP state $status" }
    if ($missingMethod.Invoke($null, @($row)) -ne ($status -eq 'Missing')) { throw "Incorrect STEP filter $status" }
}
'Document badges and STEP passed: available/missing/unverified states, EN fallback and confirmed-missing filter.'
