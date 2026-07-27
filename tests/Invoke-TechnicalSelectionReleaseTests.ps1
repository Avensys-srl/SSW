param(
    [string]$Configuration = 'AV',
    [string]$Platform = 'x86',
    [switch]$SkipBuild,
    [switch]$SkipApiTests
)

$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$msbuild = 'C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe'
if (-not (Test-Path $msbuild)) { throw 'Visual Studio 2019 MSBuild not found.' }

if (-not $SkipBuild) {
    & $msbuild (Join-Path $repo 'SSW.sln') "/p:Configuration=$Configuration" "/p:Platform=$Platform" /m /v:minimal
    if ($LASTEXITCODE -ne 0) { throw 'SSW build failed.' }
}

$versionProps = Get-Content -LiteralPath (Join-Path $repo 'SSWVersion.props') -Raw
$versionMatch = [regex]::Match($versionProps, '<SSWVersion>\s*([^<]+?)\s*</SSWVersion>')
if (-not $versionMatch.Success) { throw 'SSWVersion.props does not define SSWVersion.' }
$expectedVersion = [Version]$versionMatch.Groups[1].Value
$releaseDirectory = Join-Path $repo "SSW\bin\x86\$Configuration"
foreach ($name in @('SSW.exe', 'SSWLib.dll')) {
    $path = Join-Path $releaseDirectory $name
    if (-not (Test-Path -LiteralPath $path)) { throw "Release binary not found: $path" }
    $actualVersion = [Version]([Diagnostics.FileVersionInfo]::GetVersionInfo($path).FileVersion)
    if ($actualVersion -ne $expectedVersion) {
        throw "$name version $actualVersion does not match SSWVersion $expectedVersion."
    }
}

$smokeProject = Join-Path $repo 'tests\SelectionIdentitySmoke\SelectionIdentitySmoke.csproj'
& $msbuild $smokeProject /p:Configuration=Release /p:Platform=x86 /v:minimal
if ($LASTEXITCODE -ne 0) { throw 'SelectionIdentitySmoke build failed.' }
$smoke = Join-Path $repo 'SSWLib\bin\x86\Release\SelectionIdentitySmoke.exe'
& $smoke
if ($LASTEXITCODE -ne 0) { throw 'SelectionIdentitySmoke failed.' }

$x86PowerShell = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
& $x86PowerShell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Invoke-AlternativeReferenceSmoke.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Alternative reference smoke test failed.' }

& $x86PowerShell -NoProfile -ExecutionPolicy Bypass -File (Join-Path $PSScriptRoot 'Invoke-AccessoryCatalogSmoke.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Accessory catalog smoke test failed.' }

& (Join-Path $PSScriptRoot 'Invoke-PublishedReleaseRetentionSmoke.ps1')
if ($LASTEXITCODE -ne 0) { throw 'Published release retention smoke test failed.' }

& (Join-Path $PSScriptRoot 'Invoke-TechnicalBaselines.ps1') -Configuration $Configuration
if ($LASTEXITCODE -ne 0) { throw 'Technical calculation baselines failed.' }

$resourceFiles = Get-ChildItem (Join-Path $repo 'SSWLib') -Filter 'Resources.*.resx'
if ($resourceFiles.Count -ne 14) { throw "Expected 14 localized RESX files, found $($resourceFiles.Count)." }
$requiredKeys = @('Update_Title','Update_CheckFailed','Update_PackageIntegrityFailed',
    'MainForm_SelectionRegistration_Title','Water',
    'MainForm_CoilPerformance_ResultAirOut',
    'MainForm_CoilPerformance_InvalidCoolingTemperatures',
    'MainForm_CoilPerformance_InvalidHeatingTemperatures',
    'MainForm_CoilPerformance_LowWaterDeltaT',
    'MainForm_CoilPerformance_CriticalWaterDeltaT',
    'MainForm_CoilPerformance_WaterPressureDropWarning',
    'MainForm_Project_CreateAlternative',
    'MainForm_CoilPerformance_DimensionsNote',
    'MainForm_CoilPerformance_CustomDisclaimerTitle',
    'MainForm_CoilPerformance_CustomDisclaimer',
    'MainForm_CoilPerformance_CustomDisclaimerAccepted',
    'MainForm_CoilPerformance_ElectricPostHeaterConflict',
    'MainForm_ElectricHeater_Tab',
    'MainForm_ElectricHeater_Enable',
    'MainForm_ElectricHeater_AirOut',
    'MainForm_ElectricHeater_WaterConflict',
    'MainForm_ElectricHeater_CustomDisclaimer',
    'MainForm_ElectricHeater_FrostWarning',
    'MainForm_ElectricHeater_Code',
    'MainForm_ElectricHeater_ManagementCode',
    'MainForm_ElectricHeater_Frequency',
    'MainForm_ElectricHeater_Stages',
    'MainForm_ElectricHeater_Quantity',
    'MainForm_ElectricHeater_NominalPressureDrop',
    'MainForm_ElectricHeater_FrostStatus',
    'MainForm_ElectricHeater_FrostTargetReached',
    'MainForm_ElectricHeater_FrostTargetNotReached',
    'MainForm_ElectricHeater_CustomDisclaimerAccepted',
    'FollowUp_ScheduleTitle','FollowUp_ScheduleEnabled','FollowUp_ScheduleDays',
    'FollowUp_ScheduleExplanation','FollowUp_Cancel','FollowUp_PrepareEmail',
    'FollowUp_CancelReminder','FollowUp_CenterTitle','FollowUp_CloseSuccessful',
    'FollowUp_CloseUnsuccessful','FollowUp_CloseWindow','FollowUp_ColumnDue',
    'FollowUp_ColumnReference','FollowUp_ColumnReschedules','FollowUp_ColumnStatus',
    'FollowUp_ColumnType','FollowUp_Confirm','FollowUp_FilterClosed',
    'FollowUp_FilterDue','FollowUp_FilterUpcoming','FollowUp_Open','FollowUp_Refresh',
    'FollowUp_Reschedule','FollowUp_StatusCancelled','FollowUp_StatusPending',
    'FollowUp_StatusSucceeded','FollowUp_StatusUnsuccessful','FollowUp_TargetProject',
    'FollowUp_TargetSelection','FollowUp_BellNone','FollowUp_BellDue',
    'FollowUp_SyncOffline','FollowUp_SyncCurrent','FollowUp_MissingFile',
    'FollowUp_OpenError','FollowUp_NoItems','Help_MenuTip_FollowUps',
    'Help_Topic_FollowUps_Title','Help_Topic_FollowUps_Body')
foreach ($file in $resourceFiles) {
    [xml]$document = Get-Content -LiteralPath $file.FullName -Raw
    $names = @($document.root.data | ForEach-Object { $_.name })
    foreach ($key in $requiredKeys) {
        if ($names -notcontains $key) { throw "$($file.Name) is missing resource $key." }
    }
}

foreach ($report in @('CLMainReport.rdlc','CLMainReportWithCO2.rdlc','CLMainReport_Coil.rdlc','CLMainReportWithCO2_Coil.rdlc')) {
    [xml]$reportDocument = Get-Content -LiteralPath (Join-Path $repo "SSWLib\$report") -Raw
    $reportDataSetNames = @($reportDocument.Report.DataSets.DataSet | ForEach-Object { $_.Name })
    foreach ($requiredDataSet in @('ElectricHeaterReport','ElectricHeaterEHDReport','ElectricHeaterPEHDReport')) {
        if ($reportDataSetNames -notcontains $requiredDataSet) {
            throw "$report is missing dataset $requiredDataSet."
        }
    }
}

foreach ($report in @('CLMainReport_Coil.rdlc','CLMainReportWithCO2_Coil.rdlc')) {
    $reportPath = Join-Path $repo "SSWLib\$report"
    [xml]$reportDocument = Get-Content -LiteralPath $reportPath -Raw
    $namespace = New-Object System.Xml.XmlNamespaceManager($reportDocument.NameTable)
    $namespace.AddNamespace('r', $reportDocument.DocumentElement.NamespaceURI)

    $electricTable = $reportDocument.SelectSingleNode('//r:Tablix[@Name="TablixElectricHeaterReport"]', $namespace)
    if ($null -eq $electricTable -or $electricTable.DataSetName -ne 'ElectricHeaterReport') {
        throw "$report does not contain the electric-heater result table."
    }

    $winterTemperature = $reportDocument.SelectSingleNode('//r:Tablix[@Name="Tablix8"]', $namespace)
    if ($null -eq $winterTemperature -or $winterTemperature.InnerXml -match 'ElectricHeaterPEHDReport') {
        throw "$report repeats the PEHD air condition in the winter heat-exchanger block."
    }
    if ($electricTable.InnerXml -notmatch 'AirOutCaption' -or $electricTable.InnerXml -notmatch 'AirOut') {
        throw "$report electric-heater table does not expose the combined outlet temperature and humidity."
    }
}

if (-not $SkipApiTests) {
    $php = 'C:\xampp\php\php.exe'
    $api = 'A:\webavensys\api'
    if (-not (Test-Path $php) -or -not (Test-Path $api)) { throw 'PHP runtime or deployed API not found.' }
    & $php (Join-Path $api 'tests\technical_selection_service_test.php')
    if ($LASTEXITCODE -ne 0) { throw 'Technical-selection API tests failed.' }
    Get-ChildItem $api -Recurse -Filter '*.php' -File |
        Where-Object { $_.FullName -notmatch '\\vendor\\' } |
        ForEach-Object {
            & $php -l $_.FullName | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "PHP lint failed: $($_.FullName)" }
        }
}

Write-Output 'Technical-selection release matrix passed.'
