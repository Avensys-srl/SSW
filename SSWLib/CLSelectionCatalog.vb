Imports System.Data.SqlServerCe
Imports System.Globalization

Public NotInheritable Class CLSelectionCatalogItem
    Public Property Id As Integer
    Public Property Code As String
    Public Property ItemType As String
    Public Property CategoryCode As String
    Public Property CategoryName As String
    Public Property Name As String
    Public Property Description As String
    Public Property Availability As String
    Public Property InstallationType As String
    Public Property DefaultSelected As Boolean
    Public Property DefaultQuantity As Integer
    Public Property MaxQuantity As Integer
    Public Property CustomerSelectable As Boolean
    Public Property MinimumControllerLevel As Integer
    Public Property SortOrder As Integer
    Public Property FunctionNames As New List(Of String)()

    Public ReadOnly Property IsStandard As Boolean
        Get
            Return String.Equals(Availability, "Standard", StringComparison.OrdinalIgnoreCase)
        End Get
    End Property

    Public ReadOnly Property FunctionsText As String
        Get
            Return String.Join(System.Environment.NewLine, FunctionNames)
        End Get
    End Property
End Class

Public NotInheritable Class CLSelectionCatalogRepository
    Private Const DatabasePassword As String = "@D3C1L4T2%"

    Private Sub New()
    End Sub

    Public Shared Function GetEffectiveItems(databasePath As String,
        modelId As Integer,
        languageCode As String) As List(Of CLSelectionCatalogItem)

        Dim result As New List(Of CLSelectionCatalogItem)()
        If String.IsNullOrWhiteSpace(databasePath) OrElse modelId <= 0 Then Return result

        Dim normalizedLanguage = If(String.IsNullOrWhiteSpace(languageCode), "en", languageCode.Trim().ToLowerInvariant())
        Using connection As New SqlCeConnection(String.Format(
            CultureInfo.InvariantCulture,
            "Data Source=""{0}""; Password=""{1}""",
            databasePath,
            DatabasePassword))

            connection.Open()
            Using command = connection.CreateCommand()
                command.CommandText =
                    "SELECT i.Id, i.Code, i.ItemType, c.Code AS CategoryCode, " &
                    "ct.Name AS TranslatedCategoryName, c.EnglishName AS CategoryEnglishName, " &
                    "it.Name AS TranslatedItemName, i.EnglishName AS ItemEnglishName, " &
                    "it.Description AS TranslatedItemDescription, i.EnglishDescription AS ItemEnglishDescription, " &
                    "r.Availability, r.InstallationType, r.DefaultSelected, r.DefaultQuantity, " &
                    "r.MaxQuantity, i.CustomerSelectable, r.MinimumControllerLevel, r.SortOrder " &
                    "FROM CLHeatRecoveryModelSelectionItems r " &
                    "INNER JOIN CLSelectionItems i ON i.Id = r.IdSelectionItem " &
                    "INNER JOIN CLSelectionCategories c ON c.Id = i.IdCategory " &
                    "LEFT OUTER JOIN CLSelectionItemTranslations it ON it.IdSelectionItem = i.Id AND it.LanguageCode = @LanguageCode " &
                    "LEFT OUTER JOIN CLSelectionCategoryTranslations ct ON ct.IdCategory = c.Id AND ct.LanguageCode = @LanguageCode " &
                    "WHERE r.IdHeatRecoveryModel = @ModelId " &
                    "ORDER BY c.SortOrder, r.SortOrder, i.SortOrder, i.Code"
                command.Parameters.Add(New SqlCeParameter("@LanguageCode", normalizedLanguage))
                command.Parameters.Add(New SqlCeParameter("@ModelId", modelId))

                Using reader = command.ExecuteReader()
                    While reader.Read()
                        result.Add(New CLSelectionCatalogItem With {
                            .Id = ReadInt(reader, "Id"),
                            .Code = ReadString(reader, "Code"),
                            .ItemType = ReadString(reader, "ItemType"),
                            .CategoryCode = ReadString(reader, "CategoryCode"),
                            .CategoryName = FirstNonEmpty(
                                ReadString(reader, "TranslatedCategoryName"),
                                ReadString(reader, "CategoryEnglishName")),
                            .Name = FirstNonEmpty(
                                ReadString(reader, "TranslatedItemName"),
                                ReadString(reader, "ItemEnglishName"),
                                ReadString(reader, "Code")),
                            .Description = FirstNonEmpty(
                                ReadString(reader, "TranslatedItemDescription"),
                                ReadString(reader, "ItemEnglishDescription")),
                            .Availability = ReadString(reader, "Availability"),
                            .InstallationType = ReadString(reader, "InstallationType"),
                            .DefaultSelected = ReadBoolean(reader, "DefaultSelected"),
                            .DefaultQuantity = Math.Max(1, ReadInt(reader, "DefaultQuantity")),
                            .MaxQuantity = Math.Max(1, ReadInt(reader, "MaxQuantity")),
                            .CustomerSelectable = ReadBoolean(reader, "CustomerSelectable"),
                            .MinimumControllerLevel = ReadInt(reader, "MinimumControllerLevel"),
                            .SortOrder = ReadInt(reader, "SortOrder")
                        })
                    End While
                End Using
            End Using

            Dim byId = result.ToDictionary(Function(item) item.Id)
            Using command = connection.CreateCommand()
                command.CommandText =
                    "SELECT link.IdAccessoryItem, translation.Name AS TranslatedFunctionName, " &
                    "functionItem.EnglishName AS FunctionEnglishName, functionItem.Code AS FunctionCode " &
                    "FROM CLAccessoryControlFunctions link " &
                    "INNER JOIN CLSelectionItems functionItem ON functionItem.Id = link.IdControlFunctionItem " &
                    "INNER JOIN CLHeatRecoveryModelSelectionItems relation ON relation.IdSelectionItem = functionItem.Id " &
                    "LEFT OUTER JOIN CLSelectionItemTranslations translation " &
                    "ON translation.IdSelectionItem = functionItem.Id AND translation.LanguageCode = @LanguageCode " &
                    "WHERE relation.IdHeatRecoveryModel = @ModelId " &
                    "ORDER BY link.SortOrder, functionItem.SortOrder"
                command.Parameters.Add(New SqlCeParameter("@LanguageCode", normalizedLanguage))
                command.Parameters.Add(New SqlCeParameter("@ModelId", modelId))
                Using reader = command.ExecuteReader()
                    While reader.Read()
                        Dim accessoryId = ReadInt(reader, "IdAccessoryItem")
                        Dim item As CLSelectionCatalogItem = Nothing
                        If byId.TryGetValue(accessoryId, item) Then
                            Dim functionName = FirstNonEmpty(
                                ReadString(reader, "TranslatedFunctionName"),
                                ReadString(reader, "FunctionEnglishName"),
                                ReadString(reader, "FunctionCode"))
                            If Not String.IsNullOrWhiteSpace(functionName) AndAlso Not item.FunctionNames.Contains(functionName) Then
                                item.FunctionNames.Add(functionName)
                            End If
                        End If
                    End While
                End Using
            End Using
        End Using

        Return result
    End Function

    Private Shared Function ReadString(reader As SqlCeDataReader, name As String) As String
        Return If(reader(name) Is DBNull.Value, String.Empty, Convert.ToString(reader(name), CultureInfo.InvariantCulture))
    End Function

    Private Shared Function ReadInt(reader As SqlCeDataReader, name As String) As Integer
        Return If(reader(name) Is DBNull.Value, 0, Convert.ToInt32(reader(name), CultureInfo.InvariantCulture))
    End Function

    Private Shared Function ReadBoolean(reader As SqlCeDataReader, name As String) As Boolean
        Return reader(name) IsNot DBNull.Value AndAlso Convert.ToBoolean(reader(name), CultureInfo.InvariantCulture)
    End Function

    Private Shared Function FirstNonEmpty(ParamArray values() As String) As String
        For Each value In values
            If Not String.IsNullOrWhiteSpace(value) Then Return value
        Next
        Return String.Empty
    End Function
End Class
