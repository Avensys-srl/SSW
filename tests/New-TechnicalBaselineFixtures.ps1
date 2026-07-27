param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'fixtures\technical-baselines\inputs')
)

$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null

function New-Scenario {
    param(
        [string]$Code,
        [bool]$Enabled,
        [double]$Airflow,
        [double]$Pressure,
        [double]$OutdoorTemperature,
        [double]$OutdoorRh,
        [double]$ReturnTemperature,
        [double]$ReturnRh
    )
    [ordered]@{
        enabled = $Enabled
        scenarioCode = $Code
        standardCode = $(if ($Code -eq 'Summer') { 'Summer' } else { '' })
        supplyAirflowM3h = $Airflow
        extractAirflowM3h = $Airflow
        maximumPressurePa = $Pressure
        outdoorTemperatureC = $OutdoorTemperature
        outdoorRelativeHumidityPercent = $OutdoorRh
        returnTemperatureC = $ReturnTemperature
        returnRelativeHumidityPercent = $ReturnRh
        regulationPercent = 100
    }
}

function New-Document {
    param(
        [Guid]$Id,
        [string]$Unit,
        [string]$Series,
        [double]$Airflow = 100,
        [double]$Pressure = 426
    )
    [ordered]@{
        format = 'SSWSelection'
        selectionFormatVersion = 2
        projectId = $Id
        createdAtUtc = '2000-01-01T00:00:00Z'
        modifiedAtUtc = '2000-01-01T00:00:00Z'
        versions = [ordered]@{
            selectionFormatVersion = 2
            reportTemplateVersion = 3
            apiContractVersion = 1
        }
        features = @()
        identity = [ordered]@{ localDraftReference = 'BASELINE' }
        selection = [ordered]@{
            customerCode = 'TEST'
            customerReference = ''
            unit = [ordered]@{ code = $Unit; managementCode = $Series; name = $Unit }
            winter = New-Scenario 'Winter' $true $Airflow $Pressure -10 80 20 60
            summer = New-Scenario 'Summer' $true $Airflow $Pressure 32 80 26 50
            waterCoil = [ordered]@{
                enabled = $false
                selectionCase = 'Standard'
                installationType = 'External'
                calculationMode = 'HCD'
                fluid = [ordered]@{ code = 'Water'; glycolPercent = 10 }
                geometry = [ordered]@{
                    geometryCode = '2510'; lengthMm = 350; heightMm = 250; tubes = 10
                    numberOfRows = 3; finSpacingMm = 2.1; numberOfCircuits = 4
                }
                coolingWaterInletTemperatureC = 7
                coolingWaterOutletTemperatureC = 12
                heatingWaterInletTemperatureC = 80
                heatingWaterOutletTemperatureC = 70
            }
            electricHeater = [ordered]@{
                enabled = $false
                ehd = [ordered]@{ enabled = $false; mode = 'EHD'; selectionCase = 'Standard'; installationType = 'Internal' }
                pehd = [ordered]@{ enabled = $false; mode = 'PEHD'; selectionCase = 'Standard'; installationType = 'Internal' }
            }
            accessories = @()
            report = [ordered]@{
                languageCode = 'it'
                includePerformanceCharts = $true
                includeSoundPower = $false
                includeCo2 = $false
            }
        }
        revisionTracking = [ordered]@{}
    }
}

function Save-Fixture {
    param([string]$Name, [object]$Document)
    $path = Join-Path $OutputDirectory ($Name + '.sswsel')
    $json = $Document | ConvertTo-Json -Depth 30
    [IO.File]::WriteAllText($path, $json, [Text.UTF8Encoding]::new($false))
}

$standard = New-Document ([Guid]'10000000-0000-0000-0000-000000000001') 'CLRC 038 OSC' 'ECOP'
$standard.selection.accessories = @(
    [ordered]@{
        code = 'CAFS'; itemType = 'Hardware'; quantity = 1; availability = 'Optional'
        installationType = 'Internal'; localizedDisplayName = 'Constant airflow control - supply'
        localizedDescription = 'Constant airflow control - supply'; localizedFunctionNames = @()
    },
    [ordered]@{
        code = 'KTS EXTRA'; itemType = 'Hardware'; quantity = 1; availability = 'Optional'
        installationType = 'External'; localizedDisplayName = 'KTS Extra'
        localizedDescription = 'KTS Extra touch screen controller'; localizedFunctionNames = @()
    }
)
$standard.features = @('SummerCalculation', 'AccessoriesAndControlFunctions')
Save-Fixture 'standard-accessories' $standard

$coil = New-Document ([Guid]'10000000-0000-0000-0000-000000000002') 'CLRC 038 OSC' 'ECOP'
$coil.selection.waterCoil.enabled = $true
$coil.selection.waterCoil.coil = [ordered]@{ id = 1028; code = 'CWD 033 - 043 - 053'; name = 'CWD 033 - 043 - 053' }
$coil.features = @('SummerCalculation', 'WaterCoils')
Save-Fixture 'water-coil-hcd' $coil

$heater = New-Document ([Guid]'10000000-0000-0000-0000-000000000003') 'CLRC 038 OSC' 'ECOP'
$heater.selection.electricHeater.enabled = $true
$heater.selection.electricHeater.ehd = [ordered]@{
    enabled = $true; mode = 'EHD'; selectionCase = 'Standard'; installationType = 'Internal'
    heater = [ordered]@{ id = 6; code = 'EH-0.75-230'; name = 'EH-0.75-230' }
}
$heater.selection.electricHeater.pehd = [ordered]@{
    enabled = $true; mode = 'PEHD'; selectionCase = 'Standard'; installationType = 'Internal'
    heater = [ordered]@{ id = 20; code = 'EH-0.9-230'; name = 'EH-0.9-230' }
}
$heater.features = @('SummerCalculation', 'ElectricHeaters')
Save-Fixture 'electric-heaters' $heater

$enthalpic = New-Document ([Guid]'10000000-0000-0000-0000-000000000004') 'PRIME 020DL EN' 'PRIME'
$enthalpic.features = @('SummerCalculation')
Save-Fixture 'enthalpic-enrfc27' $enthalpic

Write-Output "Technical baseline fixtures written to $OutputDirectory"
