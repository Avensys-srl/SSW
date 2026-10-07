Imports System.Data
Imports System.Globalization
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text
Imports System.Text.Json
Imports System.Windows.Forms
Imports System.Windows.Forms.DataVisualization.Charting
Imports Microsoft.Reporting.WinForms

Public NotInheritable Class CLTechnicalBaselineDocument
    Public Property SchemaVersion As Integer = 1
    Public Property Fixture As String
    Public Property UnitCode As String
    Public Property Calculation As CLCalculatedSelectionSnapshot
    Public Property Charts As New List(Of CLTechnicalBaselineChart)()
    Public Property ReportTemplate As String
    Public Property ReportDataSets As New List(Of CLTechnicalBaselineDataSet)()
    Public Property Runtime As New Dictionary(Of String, String)(StringComparer.Ordinal)
End Class

Public NotInheritable Class CLTechnicalBaselineChart
    Public Property Name As String
    Public Property Areas As New List(Of CLTechnicalBaselineChartArea)()
    Public Property Series As New List(Of CLTechnicalBaselineSeries)()
End Class

Public NotInheritable Class CLTechnicalBaselineChartArea
    Public Property Name As String
    Public Property XMinimum As Double?
    Public Property XMaximum As Double?
    Public Property XInterval As Double?
    Public Property YMinimum As Double?
    Public Property YMaximum As Double?
    Public Property YInterval As Double?
End Class

Public NotInheritable Class CLTechnicalBaselineSeries
    Public Property Name As String
    Public Property ChartArea As String
    Public Property ChartType As String
    Public Property Points As New List(Of CLTechnicalBaselinePoint)()
End Class

Public NotInheritable Class CLTechnicalBaselinePoint
    Public Property X As Double?
    Public Property Y As Double?
    Public Property IsEmpty As Boolean
End Class

Public NotInheritable Class CLTechnicalBaselineDataSet
    Public Property Name As String
    Public Property Columns As New List(Of String)()
    Public Property Rows As New List(Of Dictionary(Of String, Object))()
End Class

Partial Public Class CLMainForm
    Private m_TechnicalBaselineReportSink As Action(Of List(Of ReportDataSource), String)

    Public Function TechnicalBaselineCapture(fixturePath As String) As CLTechnicalBaselineDocument
        Dim document As CLSelectionProjectDocument = CLSelectionProjectSerializer.Load(fixturePath)
        Dim language = Environment.FindLanguage(CLEnvironment.LanguageCode_IT)
        If language IsNot Nothing Then Environment.SetLanguage(language)

        document.Identity = New CLSelectionIdentity With {.LocalDraftReference = "BASELINE"}
        document.RevisionTracking = New CLSelectionRevisionTracking()
        m_ProjectDocument = document
        Project_ApplyDocument(document)
        Project_CaptureForm(document)
        NormalizeSnapshot(document.Snapshot)

        Dim result As New CLTechnicalBaselineDocument With {
            .Fixture = Path.GetFileName(fixturePath),
            .UnitCode = If(document.Selection.Unit Is Nothing, Nothing, document.Selection.Unit.Code),
            .Calculation = document.Snapshot
        }
        result.Charts.Add(CaptureChart("pressure", crtPerformance_Chart1))
        result.Charts.Add(CaptureChart("power", crtPerformance_Chart2))
        result.Charts.Add(CaptureChart("efficiency", crtPerformance_Chart3))

        m_TechnicalBaselineReportSink =
            Sub(sources As List(Of ReportDataSource), reportTemplate As String)
                result.ReportTemplate = reportTemplate
                For Each source As ReportDataSource In sources
                    result.ReportDataSets.Add(CaptureDataSet(source))
                Next
            End Sub
        Try
            Report_Generate()
        Finally
            m_TechnicalBaselineReportSink = Nothing
        End Try

        Dim databaseInfo As CLDatabaseCompatibilityInfo = Environment.DatabaseCompatibility
        result.Runtime("databaseSchema") = databaseInfo.SchemaVersion.ToString(CultureInfo.InvariantCulture)
        result.Runtime("databaseData") = If(databaseInfo.DataVersion, String.Empty)
        result.Runtime("databaseContent") = If(databaseInfo.ContentHash, String.Empty)
        result.Runtime("coilLibrary") = HashFile(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "COILcalc.dll"))
        Return result
    End Function

    Private Shared Sub NormalizeSnapshot(snapshot As CLCalculatedSelectionSnapshot)
        If snapshot Is Nothing Then Return
        snapshot.CalculatedAtUtc = New DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        snapshot.Versions = New CLSelectionVersionSet()
    End Sub

    Private Shared Function CaptureChart(name As String, chart As Chart) As CLTechnicalBaselineChart
        Dim result As New CLTechnicalBaselineChart With {.Name = name}
        For Each area As ChartArea In chart.ChartAreas
            result.Areas.Add(New CLTechnicalBaselineChartArea With {
                .Name = area.Name,
                .XMinimum = Finite(area.AxisX.Minimum),
                .XMaximum = Finite(area.AxisX.Maximum),
                .XInterval = Finite(area.AxisX.Interval),
                .YMinimum = Finite(area.AxisY.Minimum),
                .YMaximum = Finite(area.AxisY.Maximum),
                .YInterval = Finite(area.AxisY.Interval)
            })
        Next
        For Each series As Series In chart.Series
            Dim captured As New CLTechnicalBaselineSeries With {
                .Name = series.Name,
                .ChartArea = series.ChartArea,
                .ChartType = series.ChartType.ToString()
            }
            For Each point As DataPoint In series.Points
                captured.Points.Add(New CLTechnicalBaselinePoint With {
                    .X = Finite(point.XValue),
                    .Y = If(point.YValues Is Nothing OrElse point.YValues.Length = 0, Nothing, Finite(point.YValues(0))),
                    .IsEmpty = point.IsEmpty
                })
            Next
            result.Series.Add(captured)
        Next
        Return result
    End Function

    Private Shared Function CaptureDataSet(source As ReportDataSource) As CLTechnicalBaselineDataSet
        Dim result As New CLTechnicalBaselineDataSet With {.Name = source.Name}
        Dim table As DataTable = TryCast(source.Value, DataTable)
        If table Is Nothing Then Return result
        For Each column As DataColumn In table.Columns
            result.Columns.Add(column.ColumnName)
        Next
        For Each row As DataRow In table.Rows
            Dim captured As New Dictionary(Of String, Object)(StringComparer.Ordinal)
            For Each column As DataColumn In table.Columns
                captured(column.ColumnName) = NormalizeReportValue(column.ColumnName, row(column))
            Next
            result.Rows.Add(captured)
        Next
        Return result
    End Function

    Private Shared Function NormalizeReportValue(columnName As String, value As Object) As Object
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return Nothing
        If TypeOf value Is Byte() Then Return String.Format(CultureInfo.InvariantCulture, "binary:{0}", DirectCast(value, Byte()).Length)
        If TypeOf value Is DateTime Then Return "2000-01-01"
        Select Case columnName
            Case "SoftwareRelease_Value", "TechnicalSelectionReference_Value",
                 "TechnicalSelectionRevision_Value", "TechnicalSelectionStatus_Value"
                Return "<volatile>"
        End Select
        If TypeOf value Is Single OrElse TypeOf value Is Double OrElse TypeOf value Is Decimal Then
            Return Convert.ToDouble(value, CultureInfo.InvariantCulture)
        End If
        Return value
    End Function

    Private Shared Function Finite(value As Double) As Double?
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return Nothing
        Return value
    End Function

    Private Shared Function HashFile(path As String) As String
        If Not File.Exists(path) Then Return "missing"
        Using stream As FileStream = File.OpenRead(path), algorithm As SHA256 = SHA256.Create()
            Return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", String.Empty)
        End Using
    End Function
End Class

Public NotInheritable Class CLTechnicalBaselineCommand
    Private Sub New()
    End Sub

    Public Shared Function Run(args As String()) As Integer
        If args Is Nothing OrElse args.Length <> 2 Then
            Console.Error.WriteLine("Usage: SSW.exe --technical-baseline <fixture.sswsel> <output.json>")
            Return 2
        End If

        Dim fixturePath As String = Path.GetFullPath(args(0))
        Dim outputPath As String = Path.GetFullPath(args(1))
        Dim tempDirectory As String = Path.Combine(Path.GetTempPath(), "ssw-baseline-" & Guid.NewGuid().ToString("N"))
        Directory.CreateDirectory(tempDirectory)
        System.Environment.SetEnvironmentVariable("SSW_TECHNICAL_BASELINE_MODE", "1")
        System.Environment.SetEnvironmentVariable("SSW_SELECTION_STATE_PATH", Path.Combine(tempDirectory, "state.json"))
        System.Environment.SetEnvironmentVariable("SSW_SELECTION_RECENT_PATH", Path.Combine(tempDirectory, "recent.json"))
        System.Environment.SetEnvironmentVariable("SSW_SELECTION_CREDENTIAL_PATH", Path.Combine(tempDirectory, "credentials.json"))
        System.Environment.SetEnvironmentVariable("SSW_FOLLOW_UP_STATE_PATH", Path.Combine(tempDirectory, "follow-ups.json"))

        Try
            Using form As New CLMainForm()
                form.Show()
                Application.DoEvents()
                form.Hide()
                Dim baseline As CLTechnicalBaselineDocument = form.TechnicalBaselineCapture(fixturePath)
                Dim options As New JsonSerializerOptions With {
                    .PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    .WriteIndented = True,
                    .IgnoreNullValues = True
                }
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath))
                File.WriteAllText(outputPath, JsonSerializer.Serialize(baseline, options), New UTF8Encoding(False))
            End Using
            Return 0
        Catch exception As Exception
            Console.Error.WriteLine(exception.ToString())
            Return 1
        Finally
            Try
                Directory.Delete(tempDirectory, True)
            Catch
            End Try
        End Try
    End Function
End Class
