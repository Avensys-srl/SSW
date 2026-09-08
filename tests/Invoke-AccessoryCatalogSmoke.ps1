param(
    [string]$DatabasePath = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV\data\DataCentral.sdf'),
    [string]$BinaryDirectory = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV')
)

$ErrorActionPreference = 'Stop'

if ([Environment]::Is64BitProcess) {
    $x86PowerShell = Join-Path $env:WINDIR 'SysWOW64\WindowsPowerShell\v1.0\powershell.exe'
    & $x86PowerShell -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath `
        -DatabasePath $DatabasePath -BinaryDirectory $BinaryDirectory
    exit $LASTEXITCODE
}

$sourceDatabase = (Resolve-Path -LiteralPath $DatabasePath).Path
$database = Join-Path $env:TEMP ('ssw-accessory-catalog-smoke-' + [Guid]::NewGuid().ToString('N') + '.sdf')
Copy-Item -LiteralPath $sourceDatabase -Destination $database
trap {
    Remove-Item -LiteralPath $database -Force -ErrorAction SilentlyContinue
    throw $_
}
$binaryDirectory = (Resolve-Path -LiteralPath $BinaryDirectory).Path
$sqlCeAssembly = Join-Path $binaryDirectory 'System.Data.SqlServerCe.dll'
$sswAssembly = Join-Path $binaryDirectory 'SSWLib.dll'

[void][Reflection.Assembly]::LoadFrom($sqlCeAssembly)
[void][Reflection.Assembly]::LoadFrom($sswAssembly)

$connectionString = 'Data Source="{0}"; Password="{1}"' -f $database, '@D3C1L4T2%'
$connection = New-Object System.Data.SqlServerCe.SqlCeConnection($connectionString)
$connection.Open()
try {
    $command = $connection.CreateCommand()
    $command.CommandText = @'
SELECT TOP (1) IdHeatRecoveryModel, COUNT(*) AS RelationCount
FROM CLHeatRecoveryModelSelectionItems
GROUP BY IdHeatRecoveryModel
'@
    $reader = $command.ExecuteReader()
    if (-not $reader.Read()) { throw 'No effective accessory relations found in the SDF.' }
    $modelId = [int]$reader['IdHeatRecoveryModel']
    $expectedCount = [int]$reader['RelationCount']
    $reader.Close()

    foreach ($languageCode in @('no', 'is', 'cs')) {
        $command.CommandText = "SELECT COUNT(*) FROM CLSelectionCategoryTranslations WHERE LanguageCode = '$languageCode'"
        $categoryTranslationCount = [int]$command.ExecuteScalar()
        $command.CommandText = "SELECT COUNT(*) FROM CLSelectionItemTranslations WHERE LanguageCode = '$languageCode'"
        $itemTranslationCount = [int]$command.ExecuteScalar()
        if ($categoryTranslationCount -ne 12 -or $itemTranslationCount -ne 62) {
            throw "Incomplete $languageCode catalog translations: categories=$categoryTranslationCount items=$itemTranslationCount."
        }
    }
}
finally {
    $connection.Dispose()
}

$items = [SSW.CLSelectionCatalogRepository]::GetEffectiveItems($database, $modelId, 'it')
if ($items.Count -ne $expectedCount) {
    throw "Repository returned $($items.Count) items; expected $expectedCount for model $modelId."
}
if (($items | Where-Object { $_.DefaultQuantity -lt 1 -or $_.MaxQuantity -lt $_.DefaultQuantity }).Count -gt 0) {
    throw 'Invalid quantity bounds returned by the repository.'
}
if (($items | Where-Object { $_.IsStandard -and -not $_.DefaultSelected }).Count -gt 0) {
    throw 'A standard item is not selected by default.'
}

$functionCount = ($items | Where-Object { $_.FunctionNames.Count -gt 0 }).Count
$dependencyCount = ($items | ForEach-Object { $_.Dependencies.Count } | Measure-Object -Sum).Sum
$ktsItems = @($items | Where-Object { $_.ExclusiveGroupCode -eq 'KTS' })
if ($dependencyCount -lt 1) { throw 'No dependency rules were returned for the model.' }
if ($ktsItems.Count -ne 4) { throw "Expected four mutually exclusive KTS items; found $($ktsItems.Count)." }
$basicKts = @($ktsItems | Where-Object { $_.Code -eq 'KTS BASIC' })
if ($basicKts.Count -ne 1 -or $basicKts[0].ControllerLevel -ne 0) {
    throw 'KTS Basic controller level was not exported correctly.'
}
if (-not $basicKts[0].DefaultSelected) {
    throw 'KTS Basic is not selected by default.'
}
if (($ktsItems | Where-Object { $_.Code -ne 'KTS BASIC' -and $_.DefaultSelected }).Count -gt 0) {
    throw 'A KTS controller other than Basic is selected by default.'
}
# Change only the disposable SDF copy: an explicit model exception must win.
$connection = New-Object System.Data.SqlServerCe.SqlCeConnection($connectionString)
$connection.Open()
try {
    $command = $connection.CreateCommand()
    $extra = $ktsItems | Where-Object Code -eq 'KTS EXTRA'
    $command.CommandText = "UPDATE CLHeatRecoveryModelSelectionItems SET DefaultSelected = 0 WHERE IdHeatRecoveryModel = $modelId AND IdSelectionItem = $($basicKts[0].Id)"
    [void]$command.ExecuteNonQuery()
    $command.CommandText = "UPDATE CLHeatRecoveryModelSelectionItems SET DefaultSelected = 1 WHERE IdHeatRecoveryModel = $modelId AND IdSelectionItem = $($extra.Id)"
    [void]$command.ExecuteNonQuery()
} finally { $connection.Dispose() }
$overridden = [SSW.CLSelectionCatalogRepository]::GetEffectiveItems($database, $modelId, 'it')
if (($overridden | Where-Object { $_.Code -eq 'KTS BASIC' -and $_.DefaultSelected }).Count -gt 0 -or
    ($overridden | Where-Object { $_.Code -eq 'KTS EXTRA' -and $_.DefaultSelected }).Count -ne 1) {
    throw 'An explicit catalog default was overwritten by runtime policy.'
}

$normalize = [SSW.CLNextUiApplicationService].GetMethod('NormalizeAccessorySelection', [Reflection.BindingFlags]'Static,NonPublic')
function New-TestItem([int]$id, [string]$code, [string]$availability = 'Optional', [string]$group = '') {
    $item = New-Object SSW.CLSelectionCatalogItem
    $item.Id = $id; $item.Code = $code; $item.Availability = $availability
    $item.CustomerSelectable = $true; $item.ExclusiveGroupCode = $group
    return $item
}
function Test-Normalize($catalog, [int[]]$ids) {
    $list = New-Object 'System.Collections.Generic.List[SSW.CLSelectionCatalogItem]'
    foreach ($item in $catalog) { $list.Add($item) }
    $selected = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($id in $ids) { [void]$selected.Add($id) }
    [void]$normalize.Invoke($null, [object[]]@($list.PSObject.BaseObject, $selected.PSObject.BaseObject))
    return ,$selected
}
$unavailable = New-TestItem 1 'UNAVAILABLE' 'Unavailable'
if ((Test-Normalize @($unavailable) @(1)).Count -ne 0) { throw 'Unavailable optional accessory survived normalization.' }
$standard = New-TestItem 2 'STANDARD' 'Standard' 'KTS'
$optional = New-TestItem 3 'ALTERNATIVE' 'Optional' 'KTS'
$selection = Test-Normalize @($standard,$optional) @(3)
if ($selection.Count -ne 1 -or -not $selection.Contains(2)) { throw 'A controller default displaced the standard controller.' }
$needsController = New-TestItem 4 'REQUIRES_CONTROLLER'
$needsController.MinimumControllerLevel = 1
if ((Test-Normalize @($needsController) @(4)).Count -ne 0) { throw 'Missing minimum controller was accepted.' }
$source = New-TestItem 5 'SOURCE'
$rule = New-Object SSW.CLSelectionDependencyRule
$rule.TargetItemId = 1; $rule.TargetCode = 'UNAVAILABLE'; $rule.DependencyType = 'Requires'
$source.Dependencies.Add($rule)
if ((Test-Normalize @($source,$unavailable) @(5)).Count -ne 0) { throw 'Unavailable dependency was included.' }
$cycleA = New-TestItem 6 'CYCLE_A' 'Optional' 'EXCLUSIVE'
$cycleB = New-TestItem 7 'CYCLE_B' 'Optional' 'EXCLUSIVE'
foreach ($pair in @(@($cycleA,7), @($cycleB,6))) {
    $rule = New-Object SSW.CLSelectionDependencyRule
    $rule.TargetItemId = $pair[1]; $rule.DependencyType = 'Requires'
    $pair[0].Dependencies.Add($rule)
}
$cycleRejected = $false
try { $cycleRejected = (Test-Normalize @($cycleA,$cycleB) @(6)).Count -eq 0 } catch { $cycleRejected = $true }
if (-not $cycleRejected) { throw 'Contradictory dependency cycle was accepted.' }
$flags = [Reflection.BindingFlags]'Static,NonPublic'
$layoutRepository = [SSW.CLNormalizedSdfInstallationLayoutRepository]
$validatePorts = $layoutRepository.GetMethod('ValidatePortRoles', $flags)
$roles = New-Object 'System.Collections.Generic.SortedDictionary[int,string]'
$roles.Add(1,'Fresh'); $roles.Add(2,'Supply'); $roles.Add(3,'Return'); $roles.Add(4,'Exhaust')
[void]$validatePorts.Invoke($null,[object[]]@($roles.PSObject.BaseObject,'E2'))
$roles[4] = 'Fresh'
$rejected = $false
try { [void]$validatePorts.Invoke($null,[object[]]@($roles.PSObject.BaseObject,'E2')) } catch { $rejected = $true }
if (-not $rejected) { throw 'Duplicate airflow roles were accepted.' }
$hasSchema = $layoutRepository.GetMethod('HasNormalizedSchema', $flags)
$connection = New-Object System.Data.SqlServerCe.SqlCeConnection($connectionString)
$connection.Open()
try {
    if (-not $hasSchema.Invoke($null,[object[]]@($connection.PSObject.BaseObject))) { throw 'Schema 4 not recognized.' }
    $command = $connection.CreateCommand()
    $command.CommandText = 'DROP TABLE CLFlowConfigurationPorts'
    [void]$command.ExecuteNonQuery()
    $rejected = $false
    try { [void]$hasSchema.Invoke($null,[object[]]@($connection.PSObject.BaseObject)) } catch { $rejected = $true }
    if (-not $rejected) { throw 'Partial normalized schema fell back to legacy.' }
    foreach ($table in @('CLHeatRecoveryModelFlowConfigurations','CLFlowConfigurations')) {
        $command.CommandText = "DROP TABLE $table"
        [void]$command.ExecuteNonQuery()
    }
    $rejected = $false
    try { [void]$hasSchema.Invoke($null,[object[]]@($connection.PSObject.BaseObject)) } catch { $rejected = $true }
    if (-not $rejected) { throw 'Schema 4 with missing tables fell back to legacy.' }
    $command.CommandText = 'UPDATE CLDatabaseMetadata SET SchemaVersion = 3'
    [void]$command.ExecuteNonQuery()
    if ($hasSchema.Invoke($null,[object[]]@($connection.PSObject.BaseObject))) { throw 'Old schema did not retain legacy compatibility.' }
} finally { $connection.Dispose() }
Write-Host "Accessory catalog smoke passed: model=$modelId items=$($items.Count) linkedAccessories=$functionCount dependencies=$dependencyCount kts=$($ktsItems.Count) languages=no,is,cs; overrides and normalization checked"
Write-Host 'Layout integrity: unique airflow roles, partial/missing schema rejection and genuine legacy compatibility passed.'
Remove-Item -LiteralPath $database -Force -ErrorAction SilentlyContinue
