Imports System.Data.SqlServerCe
Imports System.Globalization
Imports Climalombarda.DataCentral.LTModel

Public Enum CLElectricHeaterMode
    EHD
    PEHD
End Enum

Public Class CLElectricHeaterDefinition
    Public Property Id As Integer
    Public Property Code As String
    Public Property ManagementCode As String
    Public Property Name As String
    Public Property Mode As CLElectricHeaterMode
    Public Property Installation As CLCoilInstallationType
    Public Property VoltageV As Double
    Public Property PhaseCount As Integer
    Public Property FrequencyHz As Double
    Public Property PowerW As Double
    Public Property CurrentA As Double
    Public Property NumberOfStages As Integer
    Public Property Quantity As Integer = 1
    Public Property IsDefault As Boolean

    Public ReadOnly Property TotalPowerW As Double
        Get
            Return PowerW * Math.Max(1, Quantity)
        End Get
    End Property

    Public ReadOnly Property TotalCurrentA As Double
        Get
            Return CurrentA * Math.Max(1, Quantity)
        End Get
    End Property

    Public Function Clone() As CLElectricHeaterDefinition
        Return DirectCast(MemberwiseClone(), CLElectricHeaterDefinition)
    End Function

    Public Overrides Function ToString() As String
        Return If(String.IsNullOrWhiteSpace(Name), Code, Name)
    End Function
End Class

Public Class CLElectricHeaterCalculationResult
    Public Property Mode As CLElectricHeaterMode
    Public Property PowerW As Double
    Public Property CurrentA As Double
    Public Property AirInletTemperatureC As Double
    Public Property AirOutletTemperatureC As Double
    Public Property AirPressureDropPa As Double
    Public Property NominalAirPressureDropPa As Double
    Public Property ExhaustOutletTemperatureC As Double?
End Class

Public NotInheritable Class CLElectricHeaterCalculator
    Private Const AirDensity As Double = 1.2D
    Private Const AirSpecificHeat As Double = 1005D

    Private Sub New()
    End Sub

    Public Shared Function GetAvailableHeaters(model As CLDCHeatRecoveryModel) As List(Of CLElectricHeaterDefinition)
        Dim heaters As New List(Of CLElectricHeaterDefinition)()
        If model Is Nothing OrElse CLEnvironment.Current Is Nothing OrElse String.IsNullOrWhiteSpace(CLEnvironment.Current.DCLiteDatabasePath) Then Return heaters
        If CLEnvironment.Current.DatabaseCompatibility IsNot Nothing AndAlso Not CLEnvironment.Current.DatabaseCompatibility.HasFeature("ElectricHeaters") Then Return heaters

        Try
            Using connection As New SqlCeConnection(String.Format("Data Source=""{0}""; Password=""{1}""", CLEnvironment.Current.DCLiteDatabasePath, "@D3C1L4T2%"))
                connection.Open()
                Using command As SqlCeCommand = connection.CreateCommand()
                    command.CommandText =
                        "SELECT h.Id, h.Code, h.ManagementCode, h.Name, h.Voltage_V, h.PhaseCount, h.Frequency_Hz, " &
                        "h.Power_W, h.Current_A, h.NumberOfStages, r.Quantity, r.InstallationType, r.HeaterMode, r.IsDefault " &
                        "FROM CLHeatRecoveryModelElectricHeaters r " &
                        "INNER JOIN CLElectricHeaters h ON h.Id = r.IdElectricHeater " &
                        "WHERE r.IdHeatRecoveryModel = @ModelId AND r.Active = 1 AND h.Active = 1 " &
                        "ORDER BY CASE WHEN r.InstallationType = 'Internal' THEN 0 ELSE 1 END, r.IsDefault DESC, r.SortOrder, h.Name"
                    command.Parameters.Add(New SqlCeParameter("@ModelId", model.Id))
                    Using reader As SqlCeDataReader = command.ExecuteReader()
                        While reader.Read()
                            Dim mode As CLElectricHeaterMode
                            If Not [Enum].TryParse(Convert.ToString(reader("HeaterMode"), CultureInfo.InvariantCulture), True, mode) Then Continue While
                            Dim installation As CLCoilInstallationType
                            If Not [Enum].TryParse(Convert.ToString(reader("InstallationType"), CultureInfo.InvariantCulture), True, installation) Then installation = CLCoilInstallationType.Internal
                            heaters.Add(New CLElectricHeaterDefinition With {
                                .Id = Convert.ToInt32(reader("Id"), CultureInfo.InvariantCulture),
                                .Code = Convert.ToString(reader("Code"), CultureInfo.InvariantCulture),
                                .ManagementCode = Convert.ToString(reader("ManagementCode"), CultureInfo.InvariantCulture),
                                .Name = Convert.ToString(reader("Name"), CultureInfo.InvariantCulture),
                                .Mode = mode,
                                .Installation = installation,
                                .VoltageV = Convert.ToDouble(reader("Voltage_V"), CultureInfo.InvariantCulture),
                                .PhaseCount = Convert.ToInt32(reader("PhaseCount"), CultureInfo.InvariantCulture),
                                .FrequencyHz = Convert.ToDouble(reader("Frequency_Hz"), CultureInfo.InvariantCulture),
                                .PowerW = Convert.ToDouble(reader("Power_W"), CultureInfo.InvariantCulture),
                                .CurrentA = Convert.ToDouble(reader("Current_A"), CultureInfo.InvariantCulture),
                                .NumberOfStages = Convert.ToInt32(reader("NumberOfStages"), CultureInfo.InvariantCulture),
                                .Quantity = Math.Max(1, Convert.ToInt32(reader("Quantity"), CultureInfo.InvariantCulture)),
                                .IsDefault = Convert.ToBoolean(reader("IsDefault"), CultureInfo.InvariantCulture)
                            })
                        End While
                    End Using
                End Using
            End Using
        Catch
            Return New List(Of CLElectricHeaterDefinition)()
        End Try
        Return heaters
    End Function

    Public Shared Function TemperatureRise(powerW As Double, airflowM3h As Double) As Double
        If powerW <= 0 OrElse airflowM3h <= 0 Then Return 0
        Return powerW / (AirDensity * (airflowM3h / 3600D) * AirSpecificHeat)
    End Function

    Public Shared Function NominalPressureDrop(modelNominalAirflowM3h As Double) As Double
        Return If(modelNominalAirflowM3h < 1000D, 10D, 15D)
    End Function

    Public Shared Function PressureDropAtAirflow(nominalDropPa As Double, airflowM3h As Double, nominalAirflowM3h As Double) As Double
        If nominalDropPa <= 0 OrElse airflowM3h <= 0 OrElse nominalAirflowM3h <= 0 Then Return 0
        Return nominalDropPa * Math.Pow(airflowM3h / nominalAirflowM3h, 2D)
    End Function
End Class
