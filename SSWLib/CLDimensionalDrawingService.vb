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
    Public Property AdditionalDimensions As New List(Of CLDimensionalValue)()
    Public Property VisibleDimensions As New List(Of CLDimensionalValue)()
    Public Property UnitWeightKilograms As Double?
    Public Property Packaging As CLDimensionalPackaging
End Class

Public NotInheritable Class CLDimensionalPackaging
    Public Property Orientation As String
    Public Property PalletLengthMillimeters As Integer?
    Public Property PalletWidthMillimeters As Integer?
    Public Property PalletHeightMillimeters As Integer?
    Public Property MaxUnits As Integer?
    Public Property PalletWeightKilograms As Double?
    Public Property TotalWeightKilograms As Double?
End Class

Public NotInheritable Class CLDimensionalDrawingService
    Private Const DatabasePassword As String = "@D3C1L4T2%"

    Private Sub New()
    End Sub

    Public Shared Function Resolve(modelCode As String,
        configurationCode As String,
        Optional includeContent As Boolean = True) As CLDimensionalDrawingResult

        Dim result = ResolveCore(modelCode, configurationCode, includeContent)
        result.VisibleDimensions = BuildVisibleDimensions(result)
        Return result
    End Function

    Private Shared Function ResolveCore(modelCode As String,
        configurationCode As String,
        includeContent As Boolean) As CLDimensionalDrawingResult

        Dim result As New CLDimensionalDrawingResult()
        If CLEnvironment.Current Is Nothing OrElse String.IsNullOrWhiteSpace(modelCode) Then
            Return result
        End If

        Dim model = CLEnvironment.Current.DCContext.CLDCHeatRecoveryModels.
            FirstOrDefault(Function(item) item.Code = modelCode)
        If model Is Nothing Then Return result
        result.UnitWeightKilograms = model.Weight

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

            LoadAdditionalDimensions(connection, model.Id, orientation, result)

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

    Private Shared Sub LoadAdditionalDimensions(connection As SqlCeConnection,
        modelId As Integer,
        orientation As String,
        result As CLDimensionalDrawingResult)

        If CLEnvironment.Current.DatabaseCompatibility Is Nothing OrElse
            Not CLEnvironment.Current.DatabaseCompatibility.HasFeature("AdditionalModelDimensions") OrElse
            Not TableExists(connection, "CLHeatRecoveryModelDimensions") OrElse
            Not TableExists(connection, "CLHeatRecoveryModelPackaging") Then
            Return
        End If

        Using command = connection.CreateCommand()
            command.CommandText =
                "SELECT OrientationScope,Width,Length,Height,Length1,Length2,Diameter,Diameter1 " &
                "FROM CLHeatRecoveryModelDimensions WHERE IdHeatRecoveryModel=@ModelId " &
                "AND (OrientationScope='B' OR OrientationScope=@Orientation) " &
                "ORDER BY CASE WHEN OrientationScope='B' THEN 0 ELSE 1 END"
            command.Parameters.AddWithValue("@ModelId", modelId)
            command.Parameters.AddWithValue("@Orientation", orientation)
            Using reader = command.ExecuteReader()
                While reader.Read()
                    Dim suffix = If(reader.GetString(0).Trim() = "B", "", If(orientation = "H", "o", "v"))
                    AddDimension(result.AdditionalDimensions, "W" & suffix, reader, 1)
                    AddDimension(result.AdditionalDimensions, "L" & suffix, reader, 2)
                    AddDimension(result.AdditionalDimensions, "H" & suffix, reader, 3)
                    AddDimension(result.AdditionalDimensions, "L1" & suffix, reader, 4)
                    AddDimension(result.AdditionalDimensions, "L2" & suffix, reader, 5)
                    AddDimension(result.AdditionalDimensions, "D" & suffix, reader, 6)
                    AddDimension(result.AdditionalDimensions, "D1" & suffix, reader, 7)
                End While
            End Using
        End Using

        Using command = connection.CreateCommand()
            command.CommandText =
                "SELECT PalletLength,PalletWidth,PalletHeight,MaxUnits,PalletWeight,TotalWeight " &
                "FROM CLHeatRecoveryModelPackaging WHERE IdHeatRecoveryModel=@ModelId AND Orientation=@Orientation"
            command.Parameters.AddWithValue("@ModelId", modelId)
            command.Parameters.AddWithValue("@Orientation", orientation)
            Using reader = command.ExecuteReader()
                If reader.Read() Then
                    result.Packaging = New CLDimensionalPackaging With {
                        .Orientation = orientation,
                        .PalletLengthMillimeters = NullableInteger(reader, 0),
                        .PalletWidthMillimeters = NullableInteger(reader, 1),
                        .PalletHeightMillimeters = NullableInteger(reader, 2),
                        .MaxUnits = NullableInteger(reader, 3),
                        .PalletWeightKilograms = NullableDouble(reader, 4),
                        .TotalWeightKilograms = NullableDouble(reader, 5)
                    }
                End If
            End Using
        End Using
    End Sub

    Private Shared Function BuildVisibleDimensions(result As CLDimensionalDrawingResult) As List(Of CLDimensionalValue)
        Dim visibleAdditional = result.AdditionalDimensions.
            Where(Function(item) item.ValueMillimeters.HasValue AndAlso item.ValueMillimeters.Value <> 0).
            ToList()
        Dim additionalCodes = New HashSet(Of String)(
            visibleAdditional.Select(Function(item) item.Code),
            StringComparer.OrdinalIgnoreCase)
        Dim legacyLabels As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase) From {
            {"A", "W"}, {"B", "L"}, {"C", "H"}, {"D", "D"}
        }
        Dim visible As New List(Of CLDimensionalValue)()
        For Each item In result.Dimensions
            If Not item.ValueMillimeters.HasValue OrElse item.ValueMillimeters.Value = 0 Then Continue For
            Dim label = If(legacyLabels.ContainsKey(item.Code), legacyLabels(item.Code), item.Code)
            If additionalCodes.Contains(label) Then Continue For
            visible.Add(New CLDimensionalValue With {
                .Code = label,
                .ValueMillimeters = item.ValueMillimeters
            })
        Next
        visible.AddRange(visibleAdditional)
        Return visible
    End Function

    Private Shared Sub AddDimension(values As List(Of CLDimensionalValue),
        code As String,
        reader As SqlCeDataReader,
        ordinal As Integer)
        If reader.IsDBNull(ordinal) Then Return
        values.Add(New CLDimensionalValue With {
            .Code = code,
            .ValueMillimeters = Convert.ToDouble(reader.GetValue(ordinal))
        })
    End Sub

    Private Shared Function NullableInteger(reader As SqlCeDataReader, ordinal As Integer) As Integer?
        If reader.IsDBNull(ordinal) Then Return Nothing
        Return Convert.ToInt32(reader.GetValue(ordinal))
    End Function

    Private Shared Function NullableDouble(reader As SqlCeDataReader, ordinal As Integer) As Double?
        If reader.IsDBNull(ordinal) Then Return Nothing
        Return Convert.ToDouble(reader.GetValue(ordinal))
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
