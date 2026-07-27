Public NotInheritable Class CLSelectionContractMapper

    Private Sub New()
    End Sub

    Public Shared Function FromProject(document As CLSelectionProjectDocument,
        Optional forceLegacyBalancedAirflows As Boolean = True) As CLSelectionCalculationInput

        If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
        Return FromSelection(document.Selection, forceLegacyBalancedAirflows)
    End Function

    Public Shared Function FromSelection(selection As CLTechnicalSelection,
        Optional forceLegacyBalancedAirflows As Boolean = True) As CLSelectionCalculationInput

        If selection Is Nothing Then Throw New ArgumentNullException(NameOf(selection))

        Dim result As New CLSelectionCalculationInput With {
            .CustomerCode = selection.CustomerCode,
            .CustomerReference = selection.CustomerReference,
            .Unit = MapEntity(selection.Unit),
            .Winter = MapScenario(selection.Winter, forceLegacyBalancedAirflows),
            .Summer = MapScenario(selection.Summer, forceLegacyBalancedAirflows),
            .WaterCoil = MapWaterCoil(selection.WaterCoil),
            .ElectricHeater = MapElectricHeater(selection.ElectricHeater)
        }

        If selection.Accessories IsNot Nothing Then
            For Each accessory As CLAccessorySelection In selection.Accessories
                result.Accessories.Add(New CLAccessoryCalculationOption With {
                    .Code = accessory.Code,
                    .ItemType = accessory.ItemType,
                    .Quantity = accessory.Quantity,
                    .Availability = accessory.Availability,
                    .InstallationType = accessory.InstallationType
                })
            Next
        End If

        Return result
    End Function

    Public Shared Function FromSnapshot(snapshot As CLCalculatedSelectionSnapshot) As CLSelectionCalculationResult
        If snapshot Is Nothing Then Return Nothing

        Dim result As New CLSelectionCalculationResult With {
            .StatusCode = snapshot.Status,
            .Winter = MapScenarioResult(snapshot.Winter),
            .Summer = MapScenarioResult(snapshot.Summer)
        }

        If snapshot.WaterCoils IsNot Nothing Then
            For Each coil As CLWaterCoilCalculationSnapshot In snapshot.WaterCoils
                result.WaterCoils.Add(New CLWaterCoilResult With {
                    .ScenarioCode = coil.ScenarioCode,
                    .Mode = coil.Mode,
                    .StatusCode = coil.Status,
                    .CapacityW = coil.CapacityW,
                    .SensibleCapacityW = coil.SensibleCapacityW,
                    .AirOutletTemperatureC = coil.AirOutletTemperatureC,
                    .AirOutletRelativeHumidityPercent = coil.AirOutletRelativeHumidityPercent,
                    .CondensateLitersPerHour = coil.CondensateLitersPerHour,
                    .AirPressureDropPa = coil.AirPressureDropPa,
                    .FluidPressureDropKPa = coil.FluidPressureDropKPa,
                    .FluidFlowLitersPerHour = coil.FluidFlowLitersPerHour,
                    .FluidVelocityMetersPerSecond = coil.FluidVelocityMetersPerSecond,
                    .FaceVelocityMetersPerSecond = coil.FaceVelocityMetersPerSecond
                })
            Next
        End If

        If snapshot.ElectricHeaters IsNot Nothing Then
            For Each heater As CLElectricHeaterCalculationSnapshot In snapshot.ElectricHeaters
                result.ElectricHeaters.Add(New CLElectricHeaterResult With {
                    .ScenarioCode = heater.ScenarioCode,
                    .Mode = heater.Mode,
                    .HeaterCode = heater.HeaterCode,
                    .PowerW = heater.PowerW,
                    .CurrentA = heater.CurrentA,
                    .AirInletTemperatureC = heater.AirInletTemperatureC,
                    .AirOutletTemperatureC = heater.AirOutletTemperatureC,
                    .AirOutletRelativeHumidityPercent = heater.AirOutletRelativeHumidityPercent,
                    .AirPressureDropPa = heater.AirPressureDropPa,
                    .ExhaustOutletTemperatureC = heater.ExhaustOutletTemperatureC
                })
            Next
        End If

        Return result
    End Function

    Private Shared Function MapScenario(source As CLOperatingScenarioInput,
        forceLegacyBalancedAirflows As Boolean) As CLSeasonCalculationInput

        If source Is Nothing Then Return New CLSeasonCalculationInput()

        Dim extractAirflow As Double? = source.ExtractAirflowM3h
        If forceLegacyBalancedAirflows OrElse Not extractAirflow.HasValue Then
            extractAirflow = source.SupplyAirflowM3h
        End If

        Return New CLSeasonCalculationInput With {
            .Enabled = source.Enabled,
            .ScenarioCode = source.ScenarioCode,
            .StandardCode = source.StandardCode,
            .Airflows = New CLAirflowPair With {
                .SupplyM3h = source.SupplyAirflowM3h,
                .ExtractM3h = extractAirflow
            },
            .MaximumPressurePa = CLBranchValuePair.Balanced(source.MaximumPressurePa),
            .OutdoorTemperatureC = source.OutdoorTemperatureC,
            .OutdoorRelativeHumidityPercent = source.OutdoorRelativeHumidityPercent,
            .ReturnTemperatureC = source.ReturnTemperatureC,
            .ReturnRelativeHumidityPercent = source.ReturnRelativeHumidityPercent,
            .RegulationPercent = source.RegulationPercent
        }
    End Function

    Private Shared Function MapScenarioResult(source As CLScenarioCalculationSnapshot) As CLSeasonCalculationResult
        If source Is Nothing Then Return Nothing

        Return New CLSeasonCalculationResult With {
            .ScenarioCode = source.ScenarioCode,
            .SupplyBranch = New CLBranchCalculationResult With {
                .BranchCode = "Supply",
                .AirflowM3h = source.AirflowM3h,
                .AvailablePressurePa = source.AvailablePressurePa,
                .AbsorbedPowerW = source.AbsorbedPowerW
            },
            .ExtractBranch = New CLBranchCalculationResult With {
                .BranchCode = "Extract",
                .AirflowM3h = source.AirflowM3h,
                .AvailablePressurePa = source.AvailablePressurePa,
                .AbsorbedPowerW = source.AbsorbedPowerW
            },
            .Thermodynamics = New CLThermodynamicCalculationResult With {
                .HeatTransferredW = source.HeatTransferredW,
                .SensibleHeatW = source.SensibleHeatW,
                .LatentHeatW = source.LatentHeatW,
                .EfficiencyPercent = source.EfficiencyPercent,
                .CondensateLitersPerHour = source.CondensateLitersPerHour,
                .SupplyOutletTemperatureC = source.SupplyOutletTemperatureC,
                .SupplyOutletRelativeHumidityPercent = source.SupplyOutletRelativeHumidityPercent,
                .ExhaustOutletTemperatureC = source.ExhaustOutletTemperatureC,
                .ExhaustOutletRelativeHumidityPercent = source.ExhaustOutletRelativeHumidityPercent
            }
        }
    End Function

    Private Shared Function MapWaterCoil(source As CLWaterCoilSelection) As CLWaterCoilCalculationOptions
        If source Is Nothing Then Return New CLWaterCoilCalculationOptions()

        Dim result As New CLWaterCoilCalculationOptions With {
            .Enabled = source.Enabled,
            .CustomDesignDisclaimerAccepted = source.CustomDesignDisclaimerAccepted,
            .SelectionCase = source.SelectionCase,
            .InstallationType = source.InstallationType,
            .CalculationMode = source.CalculationMode,
            .Coil = MapEntity(source.Coil),
            .CoolingWaterInletTemperatureC = source.CoolingWaterInletTemperatureC,
            .CoolingWaterOutletTemperatureC = source.CoolingWaterOutletTemperatureC,
            .HeatingWaterInletTemperatureC = source.HeatingWaterInletTemperatureC,
            .HeatingWaterOutletTemperatureC = source.HeatingWaterOutletTemperatureC
        }

        If source.Fluid IsNot Nothing Then
            result.FluidCode = source.Fluid.Code
            result.GlycolPercent = source.Fluid.GlycolPercent
        End If
        If source.Geometry IsNot Nothing Then
            result.GeometryCode = source.Geometry.GeometryCode
            result.LengthMm = source.Geometry.LengthMm
            result.HeightMm = source.Geometry.HeightMm
            result.Tubes = source.Geometry.Tubes
            result.NumberOfRows = source.Geometry.NumberOfRows
            result.FinSpacingMm = source.Geometry.FinSpacingMm
            result.NumberOfCircuits = source.Geometry.NumberOfCircuits
            result.HeaderTypeCode = source.Geometry.HeaderTypeCode
        End If

        Return result
    End Function

    Private Shared Function MapElectricHeater(source As CLElectricHeaterSelection) As CLElectricHeaterCalculationOptions
        If source Is Nothing Then Return New CLElectricHeaterCalculationOptions()

        Return New CLElectricHeaterCalculationOptions With {
            .Enabled = source.Enabled,
            .Stages = source.Stages,
            .NominalPowerW = source.NominalPowerW,
            .EHD = MapElectricHeaterMode(source.EHD),
            .PEHD = MapElectricHeaterMode(source.PEHD)
        }
    End Function

    Private Shared Function MapElectricHeaterMode(source As CLElectricHeaterModeSelection) As CLElectricHeaterModeCalculationOption
        If source Is Nothing Then Return New CLElectricHeaterModeCalculationOption()

        Return New CLElectricHeaterModeCalculationOption With {
            .Enabled = source.Enabled,
            .Mode = source.Mode,
            .SelectionCase = source.SelectionCase,
            .InstallationType = source.InstallationType,
            .CustomDesignDisclaimerAccepted = source.CustomDesignDisclaimerAccepted,
            .Heater = MapEntity(source.Heater)
        }
    End Function

    Private Shared Function MapEntity(source As CLSelectionEntityReference) As CLCalculationEntityReference
        If source Is Nothing Then Return New CLCalculationEntityReference()

        Return New CLCalculationEntityReference With {
            .Id = source.Id,
            .Code = source.Code,
            .ManagementCode = source.ManagementCode,
            .Name = source.Name
        }
    End Function

End Class
