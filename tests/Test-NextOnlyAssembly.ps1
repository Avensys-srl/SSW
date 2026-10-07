param([Parameter(Mandatory=$true)][string]$BuildDirectory, [switch]$Diagnostic)
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) { throw 'Use x86 Windows PowerShell.' }
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $BuildDirectory 'SSWLib.dll'))
$present = $null -ne $assembly.GetType('SSW.CLMainForm')
if ($present -ne [bool]$Diagnostic) { throw 'Incorrect legacy UI assembly boundary.' }
$baseline = $null -ne $assembly.GetType('SSW.CLTechnicalBaselineCommand')
if ($baseline -ne [bool]$Diagnostic) { throw 'Incorrect diagnostic baseline boundary.' }
foreach ($name in @('CLNextUiApplicationService','CLReportViewerForm','CLSelectionProjectSerializer','CLProductDocumentService')) {
    if ($null -eq $assembly.GetType('SSW.' + $name)) { throw "Shared runtime component lost: $name" }
}
$metadata = [Reflection.CustomAttributeData]::GetCustomAttributes($assembly) |
    Where-Object { $_.AttributeType.FullName -eq 'System.Reflection.AssemblyMetadataAttribute' -and $_.ConstructorArguments[0].Value -eq 'SSWLegacyReferenceUi' }
$expected = if ($Diagnostic) { 'true' } else { 'false' }
if ($metadata.ConstructorArguments[1].Value -ne $expected) { throw 'Invalid diagnostic packaging marker.' }
"Assembly boundary passed: legacyReferenceUi=$present sharedRuntime=retained"
