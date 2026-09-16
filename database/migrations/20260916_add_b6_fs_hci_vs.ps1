param(
    [Parameter(Mandatory = $true)]
    [string]$DatabasePath
)

$ErrorActionPreference = "Stop"

if ([IntPtr]::Size -ne 4) {
    $powerShell32 = Join-Path $env:WINDIR "SysWOW64\WindowsPowerShell\v1.0\powershell.exe"
    & $powerShell32 -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -DatabasePath $DatabasePath
    exit $LASTEXITCODE
}

$repositoryRoot = Split-Path (Split-Path $PSScriptRoot -Parent) -Parent
$providerPath = Join-Path $repositoryRoot "packages\Microsoft.SqlServer.Compact.4.0.8876.1\lib\net40\System.Data.SqlServerCe.dll"
$nativePath = Join-Path $repositoryRoot "packages\Microsoft.SqlServer.Compact.4.0.8876.1\NativeBinaries\x86"
$databaseFullPath = [IO.Path]::GetFullPath($DatabasePath)

if (-not (Test-Path -LiteralPath $databaseFullPath -PathType Leaf)) {
    throw "Database not found: $databaseFullPath"
}

$env:PATH = "$nativePath;$env:PATH"
Add-Type -Path $providerPath

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$backupPath = "$databaseFullPath.before-b6-fs-hci-vs-$timestamp.bak"
Copy-Item -LiteralPath $databaseFullPath -Destination $backupPath

$connection = New-Object System.Data.SqlServerCe.SqlCeConnection(
    "Data Source=$databaseFullPath; Password=@D3C1L4T2%")
$connection.Open()
$transaction = $connection.BeginTransaction()

function New-Command([string]$Sql) {
    $command = $connection.CreateCommand()
    $command.Transaction = $transaction
    $command.CommandText = $Sql
    return $command
}

function Add-Parameter($Command, [string]$Name, $Value) {
    $parameter = $Command.Parameters.Add($Name, [Data.SqlDbType]::NVarChar)
    $parameter.Value = $Value
}

function Get-Scalar([string]$Sql) {
    $command = New-Command $Sql
    return $command.ExecuteScalar()
}

function Get-NextId([string]$TableName) {
    if ($TableName -notin @(
        "CLFlowConfigurations",
        "CLFlowConfigurationPorts",
        "CLHeatRecoveryModelFlowConfigurations")) {
        throw "Unsupported table for ID allocation: $TableName"
    }
    $value = Get-Scalar "SELECT MAX(Id) FROM $TableName"
    if ($null -eq $value -or $value -is [DBNull]) { return 1 }
    return ([int]$value) + 1
}

function Get-ConfigurationId([int]$ConnectionId, [string]$Code) {
    $command = New-Command "SELECT Id FROM CLFlowConfigurations WHERE IdAeraulicConnection=@connection AND Code=@code"
    $null = $command.Parameters.AddWithValue("@connection", $ConnectionId)
    Add-Parameter $command "@code" $Code
    $value = $command.ExecuteScalar()
    if ($null -eq $value -or $value -is [DBNull]) { return $null }
    return [int]$value
}

try {
    $a4Count = [int](Get-Scalar @"
SELECT COUNT(*)
FROM CLHeatRecoveryModels m
INNER JOIN CLHeatRecoveryModelFlowConfigurations r ON r.IdHeatRecoveryModel=m.Id
INNER JOIN CLFlowConfigurations f ON f.Id=r.IdFlowConfiguration
WHERE m.Code='CLRC 18A OSC' AND f.Code='A4' AND f.Orientation='H'
  AND f.Active=1 AND r.Active=1
"@)
    $a4PortCount = [int](Get-Scalar @"
SELECT COUNT(*)
FROM CLHeatRecoveryModels m
INNER JOIN CLHeatRecoveryModelFlowConfigurations r ON r.IdHeatRecoveryModel=m.Id
INNER JOIN CLFlowConfigurations f ON f.Id=r.IdFlowConfiguration
INNER JOIN CLFlowConfigurationPorts p ON p.IdFlowConfiguration=f.Id
WHERE m.Code='CLRC 18A OSC' AND f.Code='A4' AND f.Orientation='H'
  AND f.Active=1 AND r.Active=1
"@)
    if ($a4Count -ne 1 -or $a4PortCount -ne 4) {
        throw "CLRC 18A OSC must have one active A4-H relation with four flow ports."
    }

    $sourceConfigurationId = [int](Get-Scalar @"
SELECT TOP (1) f.Id
FROM CLHeatRecoveryModels m
INNER JOIN CLEnumItems e ON e.Id=m.IdAeraulicConnection
INNER JOIN CLHeatRecoveryModelFlowConfigurations r ON r.IdHeatRecoveryModel=m.Id
INNER JOIN CLFlowConfigurations f ON f.Id=r.IdFlowConfiguration
WHERE m.Code='SG 127 FS' AND e.TextCode='FS' AND f.Code='B6'
ORDER BY r.Active DESC, f.Active DESC
"@)
    if ($sourceConfigurationId -le 0) {
        throw "The source B6 relation for SG 127 FS was not found."
    }

    $sourcePorts = @{}
    $sourceCommand = New-Command "SELECT PositionNumber, AirRole FROM CLFlowConfigurationPorts WHERE IdFlowConfiguration=@id ORDER BY PositionNumber"
    $null = $sourceCommand.Parameters.AddWithValue("@id", $sourceConfigurationId)
    $reader = $sourceCommand.ExecuteReader()
    while ($reader.Read()) {
        $sourcePorts[[int]$reader.GetByte(0)] = $reader.GetString(1)
    }
    $reader.Close()
    if ($sourcePorts.Count -ne 4 -or (@($sourcePorts.Keys | Sort-Object) -join ",") -ne "1,2,3,4") {
        throw "The source B6 configuration does not contain four numbered flow ports."
    }

    foreach ($connectionCode in @("FS", "HCI", "VS")) {
        $connectionCommand = New-Command "SELECT Id FROM CLEnumItems WHERE TextCode=@code"
        Add-Parameter $connectionCommand "@code" $connectionCode
        $connectionIdValue = $connectionCommand.ExecuteScalar()
        if ($null -eq $connectionIdValue -or $connectionIdValue -is [DBNull]) {
            throw "Aeraulic connection not found: $connectionCode"
        }
        $connectionId = [int]$connectionIdValue

        $configurationId = Get-ConfigurationId $connectionId "B6"
        if ($null -eq $configurationId) {
            $newConfigurationId = Get-NextId "CLFlowConfigurations"
            $insertConfiguration = New-Command @"
INSERT INTO CLFlowConfigurations
    (Id, Code, IdAeraulicConnection, Orientation, InstallationMode, AccessSide,
     ReferenceView, Active, SortOrder)
VALUES
    (@id, 'B6', @connection, 'H', 'ceiling', 'lower',
     'OSC_NORTH_SOUTH', 1, 6606)
"@
            $null = $insertConfiguration.Parameters.AddWithValue("@id", $newConfigurationId)
            $null = $insertConfiguration.Parameters.AddWithValue("@connection", $connectionId)
            $null = $insertConfiguration.ExecuteNonQuery()
            $configurationId = $newConfigurationId
        }

        foreach ($position in 1..4) {
            $portCommand = New-Command "SELECT Id FROM CLFlowConfigurationPorts WHERE IdFlowConfiguration=@configuration AND PositionNumber=@position"
            $null = $portCommand.Parameters.AddWithValue("@configuration", $configurationId)
            $null = $portCommand.Parameters.AddWithValue("@position", $position)
            $portId = $portCommand.ExecuteScalar()
            if ($null -eq $portId -or $portId -is [DBNull]) {
                $newPortId = Get-NextId "CLFlowConfigurationPorts"
                $insertPort = New-Command "INSERT INTO CLFlowConfigurationPorts (Id, IdFlowConfiguration, PositionNumber, AirRole) VALUES (@id, @configuration, @position, @role)"
                $null = $insertPort.Parameters.AddWithValue("@id", $newPortId)
                $null = $insertPort.Parameters.AddWithValue("@configuration", $configurationId)
                $null = $insertPort.Parameters.AddWithValue("@position", $position)
                Add-Parameter $insertPort "@role" $sourcePorts[$position]
                $null = $insertPort.ExecuteNonQuery()
            } else {
                $updatePort = New-Command "UPDATE CLFlowConfigurationPorts SET AirRole=@role WHERE Id=@id"
                Add-Parameter $updatePort "@role" $sourcePorts[$position]
                $null = $updatePort.Parameters.AddWithValue("@id", [int]$portId)
                $null = $updatePort.ExecuteNonQuery()
            }
        }

        $deleteWrongB6 = New-Command @"
DELETE FROM CLHeatRecoveryModelFlowConfigurations
WHERE IdHeatRecoveryModel IN (
    SELECT m.Id FROM CLHeatRecoveryModels m
    INNER JOIN CLEnumItems e ON e.Id=m.IdAeraulicConnection
    WHERE e.TextCode=@code)
AND IdFlowConfiguration IN (
    SELECT Id FROM CLFlowConfigurations WHERE Code='B6' AND Id<>@configuration)
"@
        Add-Parameter $deleteWrongB6 "@code" $connectionCode
        $null = $deleteWrongB6.Parameters.AddWithValue("@configuration", $configurationId)
        $null = $deleteWrongB6.ExecuteNonQuery()

        $modelsCommand = New-Command @"
SELECT m.Id
FROM CLHeatRecoveryModels m
INNER JOIN CLEnumItems e ON e.Id=m.IdAeraulicConnection
WHERE e.TextCode=@code
"@
        Add-Parameter $modelsCommand "@code" $connectionCode
        $modelIds = New-Object System.Collections.Generic.List[int]
        $modelReader = $modelsCommand.ExecuteReader()
        while ($modelReader.Read()) { $modelIds.Add($modelReader.GetInt32(0)) }
        $modelReader.Close()
        if ($modelIds.Count -eq 0) { throw "No models found for $connectionCode" }

        foreach ($modelId in $modelIds) {
            $relationCommand = New-Command "SELECT Id FROM CLHeatRecoveryModelFlowConfigurations WHERE IdHeatRecoveryModel=@model AND IdFlowConfiguration=@configuration"
            $null = $relationCommand.Parameters.AddWithValue("@model", $modelId)
            $null = $relationCommand.Parameters.AddWithValue("@configuration", $configurationId)
            $relationId = $relationCommand.ExecuteScalar()
            if ($null -eq $relationId -or $relationId -is [DBNull]) {
                $newRelationId = Get-NextId "CLHeatRecoveryModelFlowConfigurations"
                $insertRelation = New-Command "INSERT INTO CLHeatRecoveryModelFlowConfigurations (Id, IdHeatRecoveryModel, IdFlowConfiguration, IsDefault, Active, SortOrder) VALUES (@id, @model, @configuration, 1, 1, 1)"
                $null = $insertRelation.Parameters.AddWithValue("@id", $newRelationId)
                $null = $insertRelation.Parameters.AddWithValue("@model", $modelId)
                $null = $insertRelation.Parameters.AddWithValue("@configuration", $configurationId)
                $null = $insertRelation.ExecuteNonQuery()
            } else {
                $updateRelation = New-Command "UPDATE CLHeatRecoveryModelFlowConfigurations SET IsDefault=1, Active=1, SortOrder=1 WHERE Id=@id"
                $null = $updateRelation.Parameters.AddWithValue("@id", [int]$relationId)
                $null = $updateRelation.ExecuteNonQuery()
            }
        }

        $updateLegacy = New-Command @"
UPDATE CLHeatRecoveryModels SET HorVariants='B6'
WHERE IdAeraulicConnection=@connection
"@
        $null = $updateLegacy.Parameters.AddWithValue("@connection", $connectionId)
        $null = $updateLegacy.ExecuteNonQuery()
    }

    $oscConnectionId = [int](Get-Scalar "SELECT Id FROM CLEnumItems WHERE TextCode='OSC'")
    $a4ConfigurationId = Get-ConfigurationId $oscConnectionId "A4"
    if ($null -eq $a4ConfigurationId) {
        throw "The OSC A4-H configuration was not found."
    }
    $a4DefinitionCount = [int](Get-Scalar @"
SELECT COUNT(*) FROM CLFlowConfigurations
WHERE Id=$a4ConfigurationId AND Orientation='H' AND Active=1
"@)
    $a4DefinitionPorts = [int](Get-Scalar @"
SELECT COUNT(*) FROM CLFlowConfigurationPorts
WHERE IdFlowConfiguration=$a4ConfigurationId
"@)
    if ($a4DefinitionCount -ne 1 -or $a4DefinitionPorts -ne 4) {
        throw "The OSC A4-H definition is not active or does not contain four ports."
    }

    $removeOtherSerie8Layouts = New-Command @"
DELETE FROM CLHeatRecoveryModelFlowConfigurations
WHERE IdHeatRecoveryModel IN (
    SELECT m.Id
    FROM CLHeatRecoveryModels m
    INNER JOIN CLSeries s ON s.Id=m.IdSerie
    WHERE s.Code='8' AND m.IdAeraulicConnection=@osc)
AND IdFlowConfiguration<>@configuration
"@
    $null = $removeOtherSerie8Layouts.Parameters.AddWithValue("@osc", $oscConnectionId)
    $null = $removeOtherSerie8Layouts.Parameters.AddWithValue("@configuration", $a4ConfigurationId)
    $null = $removeOtherSerie8Layouts.ExecuteNonQuery()

    $serie8ModelsCommand = New-Command @"
SELECT m.Id
FROM CLHeatRecoveryModels m
INNER JOIN CLSeries s ON s.Id=m.IdSerie
WHERE s.Code='8' AND m.IdAeraulicConnection=@osc
"@
    $null = $serie8ModelsCommand.Parameters.AddWithValue("@osc", $oscConnectionId)
    $serie8ModelIds = New-Object System.Collections.Generic.List[int]
    $serie8Reader = $serie8ModelsCommand.ExecuteReader()
    while ($serie8Reader.Read()) { $serie8ModelIds.Add($serie8Reader.GetInt32(0)) }
    $serie8Reader.Close()
    if ($serie8ModelIds.Count -eq 0) { throw "No series 8 OSC models were found." }

    foreach ($modelId in $serie8ModelIds) {
        $serie8RelationCommand = New-Command "SELECT Id FROM CLHeatRecoveryModelFlowConfigurations WHERE IdHeatRecoveryModel=@model AND IdFlowConfiguration=@configuration"
        $null = $serie8RelationCommand.Parameters.AddWithValue("@model", $modelId)
        $null = $serie8RelationCommand.Parameters.AddWithValue("@configuration", $a4ConfigurationId)
        $serie8RelationId = $serie8RelationCommand.ExecuteScalar()
        if ($null -eq $serie8RelationId -or $serie8RelationId -is [DBNull]) {
            $newRelationId = Get-NextId "CLHeatRecoveryModelFlowConfigurations"
            $insertSerie8Relation = New-Command "INSERT INTO CLHeatRecoveryModelFlowConfigurations (Id, IdHeatRecoveryModel, IdFlowConfiguration, IsDefault, Active, SortOrder) VALUES (@id, @model, @configuration, 1, 1, 1)"
            $null = $insertSerie8Relation.Parameters.AddWithValue("@id", $newRelationId)
            $null = $insertSerie8Relation.Parameters.AddWithValue("@model", $modelId)
            $null = $insertSerie8Relation.Parameters.AddWithValue("@configuration", $a4ConfigurationId)
            $null = $insertSerie8Relation.ExecuteNonQuery()
        } else {
            $updateSerie8Relation = New-Command "UPDATE CLHeatRecoveryModelFlowConfigurations SET IsDefault=1, Active=1, SortOrder=1 WHERE Id=@id"
            $null = $updateSerie8Relation.Parameters.AddWithValue("@id", [int]$serie8RelationId)
            $null = $updateSerie8Relation.ExecuteNonQuery()
        }
    }

    $updateSerie8Legacy = New-Command @"
UPDATE CLHeatRecoveryModels SET HorVariants='A4', VerVariants=''
WHERE IdSerie IN (SELECT Id FROM CLSeries WHERE Code='8')
  AND IdAeraulicConnection=@osc
"@
    $null = $updateSerie8Legacy.Parameters.AddWithValue("@osc", $oscConnectionId)
    $null = $updateSerie8Legacy.ExecuteNonQuery()

    $transaction.Commit()
    Write-Output "Migration completed."
    Write-Output "Backup: $backupPath"
} catch {
    $transaction.Rollback()
    Remove-Item -LiteralPath $backupPath -ErrorAction SilentlyContinue
    throw
} finally {
    $connection.Close()
}
