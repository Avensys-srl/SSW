Imports Climalombarda.DataCentral.LTModel
Imports System.Globalization
Imports System.Linq

Public NotInheritable Class CLNextUiModelSummary
    Public Property Id As Integer
    Public Property Code As String
    Public Property Name As String
    Public Property SeriesCode As String
    Public Property NominalAirflowM3h As Double
    Public Property StaticPressurePa As Double
    Public Property AeraulicConnectionCode As String
End Class

Public NotInheritable Class CLNextUiAccessorySummary
    Public Property Code As String
    Public Property Name As String
    Public Property Category As String
    Public Property Installation As String
    Public Property Included As Boolean
    Public Property Locked As Boolean
End Class

Public NotInheritable Class CLNextUiCalculationInput
    Public Property ModelCode As String
    Public Property SupplyAirflowM3h As Double
    Public Property ExtractAirflowM3h As Double
    Public Property PressurePa As Double
    Public Property RegulationPercent As Double = 100
    Public Property WinterOutdoorTemperatureC As Double = -10
    Public Property WinterOutdoorRhPercent As Double = 80
    Public Property WinterReturnTemperatureC As Double = 20
    Public Property WinterReturnRhPercent As Double = 60
    Public Property SummerOutdoorTemperatureC As Double = 32
    Public Property SummerOutdoorRhPercent As Double = 80
    Public Property SummerReturnTemperatureC As Double = 26
    Public Property SummerReturnRhPercent As Double = 50
End Class

Public NotInheritable Class CLNextUiCalculationResult
    Public Property Model As CLNextUiModelSummary
    Public Property Winter As CLBalancedScenarioCalculation
    Public Property Summer As CLBalancedScenarioCalculation
    Public Property Layout As CLInstallationLayoutSnapshot
    Public Property Accessories As New List(Of CLNextUiAccessorySummary)()
End Class

Public NotInheritable Class CLNextUiApplicationService
    Private Sub New()
    End Sub

    Public Shared Function GetModels() As List(Of CLNextUiModelSummary)
        Return CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            OrderBy(Function(model) model.CLSerie.Code).
            ThenBy(Function(model) model.Code).
            ToList().
            Select(Function(model) MapModel(model)).
            ToList()
    End Function

    Public Shared Function Calculate(input As CLNextUiCalculationInput) As CLNextUiCalculationResult
        If input Is Nothing Then Throw New ArgumentNullException("input")
        Dim model = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            FirstOrDefault(Function(item) item.Code = input.ModelCode)
        If model Is Nothing Then Throw New InvalidOperationException("Selected model was not found.")

        Dim airflow = If(input.SupplyAirflowM3h > 0, input.SupplyAirflowM3h, CDbl(model.NominalAirflow.GetValueOrDefault()))
        Dim pressure = If(input.PressurePa >= 0, input.PressurePa, CDbl(model.StaticPressure.GetValueOrDefault()))
        Dim winter = CalculateSeason(
            model, "Winter", airflow, pressure, input.RegulationPercent,
            input.WinterOutdoorTemperatureC, input.WinterOutdoorRhPercent,
            input.WinterReturnTemperatureC, input.WinterReturnRhPercent)
        Dim summer = CalculateSeason(
            model, "Summer", airflow, pressure, input.RegulationPercent,
            input.SummerOutdoorTemperatureC, input.SummerOutdoorRhPercent,
            input.SummerReturnTemperatureC, input.SummerReturnRhPercent)

        Return New CLNextUiCalculationResult With {
            .Model = MapModel(model),
            .Winter = winter,
            .Summer = summer,
            .Layout = New CLLegacySdfInstallationLayoutRepository().GetForModel(model),
            .Accessories = GetAccessories(model)
        }
    End Function

    Private Shared Function CalculateSeason(
        model As CLDCHeatRecoveryModel,
        scenarioCode As String,
        airflow As Double,
        pressure As Double,
        regulationPercent As Double,
        outdoorTemperature As Double,
        outdoorRh As Double,
        returnTemperature As Double,
        returnRh As Double) As CLBalancedScenarioCalculation

        Dim scenario As New CLSeasonCalculationInput With {
            .Enabled = True,
            .ScenarioCode = scenarioCode,
            .Airflows = CLAirflowPair.Balanced(airflow),
            .MaximumPressurePa = CLBranchValuePair.Balanced(pressure),
            .OutdoorTemperatureC = outdoorTemperature,
            .OutdoorRelativeHumidityPercent = outdoorRh,
            .ReturnTemperatureC = returnTemperature,
            .ReturnRelativeHumidityPercent = returnRh,
            .RegulationPercent = regulationPercent
        }
        Return CLSelectionApplicationService.CalculateBalancedScenario(
            New CLBalancedScenarioCalculationRequest With {
                .Scenario = scenario,
                .Model = model,
                .MeasureUnit = CLMeasureUnit.SI
            })
    End Function

    Private Shared Function GetAccessories(model As CLDCHeatRecoveryModel) As List(Of CLNextUiAccessorySummary)
        Dim languageCode = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
        Return CLSelectionCatalogRepository.GetEffectiveItems(
                CLEnvironment.Current.DCLiteDatabasePath,
                model.Id,
                languageCode).
            Where(Function(item) Not String.Equals(item.ItemType, "ControlFunction", StringComparison.OrdinalIgnoreCase)).
            Select(Function(item) New CLNextUiAccessorySummary With {
                .Code = item.Code,
                .Name = item.Name,
                .Category = item.CategoryName,
                .Installation = item.InstallationType,
                .Included = item.DefaultSelected OrElse item.IsStandard,
                .Locked = item.IsStandard OrElse Not item.CustomerSelectable
            }).
            ToList()
    End Function

    Private Shared Function MapModel(model As CLDCHeatRecoveryModel) As CLNextUiModelSummary
        Return New CLNextUiModelSummary With {
            .Id = model.Id,
            .Code = model.Code,
            .Name = CLEnvironment.Current.GetCustomerHeatRecoveryModelName(model),
            .SeriesCode = If(model.CLSerie Is Nothing, String.Empty, model.CLSerie.Code),
            .NominalAirflowM3h = model.NominalAirflow.GetValueOrDefault(),
            .StaticPressurePa = model.StaticPressure.GetValueOrDefault(),
            .AeraulicConnectionCode = If(model.CLEnumItem_AeraulicConnection Is Nothing,
                String.Empty,
                model.CLEnumItem_AeraulicConnection.TextCode)
        }
    End Function
End Class
