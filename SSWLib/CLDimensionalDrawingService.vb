Imports Climalombarda.DataCentral.LTModel
Imports System.Data.SqlServerCe
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography

Public NotInheritable Class CLDimensionalDrawingResult
    Public Property Available As Boolean
    Public Property Code As String
    Public Property Revision As String
    Public Property FileName As String
    Public Property MimeType As String
    Public Property Sha256 As String
    Public Property PageWidthPoints As Double
    Public Property PageHeightPoints As Double
    Public Property PageRotation As Integer
    Public Property ContentBase64 As String
    Public Property Orientation As String
    Public Property Dimensions As New List(Of CLDimensionalValue)()
End Class

Public NotInheritable Class CLDimensionalDrawingService
    Private Const DatabasePassword As String = "@D3C1L4T2%"

    Private Sub New()
    End Sub

    Public Shared Function Resolve(modelCode As String,
        configurationCode As String,
        Optional includeContent As Boolean = True) As CLDimensionalDrawingResult

        Dim result As New CLDimensionalDrawingResult()
        If CLEnvironment.Current Is Nothing OrElse String.IsNullOrWhiteSpace(modelCode) Then
            Return result
        End If

        Dim model = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            FirstOrDefault(Function(item) item.Code = modelCode)
        If model Is Nothing Then Return result

        Dim layout = CLInstallationLayoutRepository.Create().GetForModel(
            model, configurationCode)
        Dim configuration = layout.Configurations.FirstOrDefault(Function(item) _
            String.Equals(item.Code, layout.ConfigurationCode,
                StringComparison.OrdinalIgnoreCase))
        Dim orientation = If(configuration IsNot Nothing AndAlso
            configuration.Orientation = CLInstallationLayoutOrientation.Vertical,
            "V", "H")
        result.Orientation = orientation
        result.Dimensions = If(orientation = "V",
            layout.VerticalDimensions, layout.HorizontalDimensions).
            Select(Function(item) New CLDimensionalValue With {
                .Code = item.Code,
                .ValueMillimeters = item.ValueMillimeters
            }).ToList()

        Dim databasePath = CLEnvironment.Current.DCLiteDatabasePath
        If String.IsNullOrWhiteSpace(databasePath) OrElse Not File.Exists(databasePath) Then
            Return result
        End If
        If CLEnvironment.Current.DatabaseCompatibility IsNot Nothing AndAlso
            Not CLEnvironment.Current.DatabaseCompatibility.HasFeature("DimensionalDrawings") Then
            Return result
        End If

        Using connection As New SqlCeConnection(String.Format(
            "Data Source=""{0}""; Password=""{1}""", databasePath, DatabasePassword))
            connection.Open()
            If Not TableExists(connection, "CLDimensionalDrawings") OrElse
                Not TableExists(connection, "CLHeatRecoveryModelDimensionalDrawings") Then
                Return result
            End If

            Using command = connection.CreateCommand()
                command.CommandText =
                    "SELECT drawing.Code,drawing.Revision,drawing.FileName,drawing.MimeType," &
                    "drawing.PageWidthPoints,drawing.PageHeightPoints,drawing.PageRotation," &
                    "drawing.ContentHash,drawing.PdfData,relation.OrientationScope " &
                    "FROM CLHeatRecoveryModelDimensionalDrawings relation " &
                    "INNER JOIN CLDimensionalDrawings drawing ON drawing.Id=relation.IdDimensionalDrawing " &
                    "WHERE relation.IdHeatRecoveryModel=@ModelId AND relation.Active=1 " &
                    "AND drawing.Active=1 AND (relation.OrientationScope=@Orientation OR relation.OrientationScope='B') " &
                    "ORDER BY CASE WHEN relation.OrientationScope=@Orientation THEN 0 ELSE 1 END, relation.SortOrder, drawing.Code"
                command.Parameters.AddWithValue("@ModelId", model.Id)
                command.Parameters.AddWithValue("@Orientation", orientation)
                Using reader = command.ExecuteReader()
                    Dim rows As New List(Of DrawingRow)()
                    While reader.Read()
                        rows.Add(New DrawingRow With {
                            .Code = reader.GetString(0),
                            .Revision = reader.GetString(1),
                            .FileName = reader.GetString(2),
                            .MimeType = reader.GetString(3),
                            .PageWidthPoints = Convert.ToDouble(reader.GetValue(4)),
                            .PageHeightPoints = Convert.ToDouble(reader.GetValue(5)),
                            .PageRotation = Convert.ToInt32(reader.GetValue(6)),
                            .Sha256 = reader.GetString(7),
                            .Content = DirectCast(reader.GetValue(8), Byte()),
                            .Scope = reader.GetString(9)
                        })
                    End While
                    If rows.Count = 0 Then Return result
                    Dim preferredScope = If(rows.Any(Function(item) item.Scope = orientation), orientation, "B")
                    Dim preferred = rows.Where(Function(item) item.Scope = preferredScope).ToList()
                    If preferred.Count <> 1 Then
                        Throw New InvalidDataException(
                            "Multiple dimensional drawings are active for model " & modelCode &
                            " and orientation " & preferredScope & ".")
                    End If
                    ApplyDrawing(result, preferred(0), includeContent)
                End Using
            End Using
        End Using
        Return result
    End Function

    Private Shared Sub ApplyDrawing(result As CLDimensionalDrawingResult,
        row As DrawingRow,
        includeContent As Boolean)

        If Not String.Equals(row.MimeType, "application/pdf",
            StringComparison.OrdinalIgnoreCase) OrElse row.Content Is Nothing OrElse
            row.Content.Length < 5 OrElse row.Content(0) <> AscW("%"c) OrElse
            row.Content(1) <> AscW("P"c) OrElse row.Content(2) <> AscW("D"c) OrElse
            row.Content(3) <> AscW("F"c) OrElse row.Content(4) <> AscW("-"c) Then
            Throw New InvalidDataException("Invalid dimensional drawing PDF.")
        End If
        Dim actualHash = ComputeSha256(row.Content)
        If Not String.Equals(actualHash, row.Sha256,
            StringComparison.OrdinalIgnoreCase) Then
            Throw New InvalidDataException("Dimensional drawing hash mismatch.")
        End If

        result.Available = True
        result.Code = row.Code
        result.Revision = row.Revision
        result.FileName = row.FileName
        result.MimeType = row.MimeType
        result.Sha256 = actualHash
        result.PageWidthPoints = row.PageWidthPoints
        result.PageHeightPoints = row.PageHeightPoints
        result.PageRotation = row.PageRotation
        If includeContent Then
            result.ContentBase64 = Convert.ToBase64String(row.Content)
        End If
    End Sub

    Private Shared Function TableExists(connection As SqlCeConnection,
        tableName As String) As Boolean

        Using command = connection.CreateCommand()
            command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME=@TableName"
            command.Parameters.AddWithValue("@TableName", tableName)
            Return Convert.ToInt32(command.ExecuteScalar()) > 0
        End Using
    End Function

    Private Shared Function ComputeSha256(content As Byte()) As String
        Using sha = SHA256.Create()
            Return BitConverter.ToString(sha.ComputeHash(content)).Replace("-", String.Empty)
        End Using
    End Function

    Private NotInheritable Class DrawingRow
        Public Property Code As String
        Public Property Revision As String
        Public Property FileName As String
        Public Property MimeType As String
        Public Property PageWidthPoints As Double
        Public Property PageHeightPoints As Double
        Public Property PageRotation As Integer
        Public Property Sha256 As String
        Public Property Content As Byte()
        Public Property Scope As String
    End Class
End Class
