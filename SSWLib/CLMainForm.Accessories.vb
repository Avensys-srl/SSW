Imports System.Globalization

Partial Public Class CLMainForm
    Private tbpData_Accessories As TabPage
    Private dgvAccessories As DataGridView
    Private txtAccessoriesSearch As TextBox
    Private cmbAccessoriesCategory As ComboBox
    Private lblAccessoriesSearch As Label
    Private lblAccessoriesCategory As Label
    Private lblAccessoriesSummary As Label
    Private m_AccessoriesChanging As Boolean
    Private m_AccessoriesModelId As Integer = -1
    Private m_AccessoryItems As New List(Of CLSelectionCatalogItem)()
    Private ReadOnly m_AccessorySelected As New HashSet(Of Integer)()
    Private ReadOnly m_AccessoryQuantities As New Dictionary(Of Integer, Integer)()

    Private Sub Accessories_InitializeTab()
        If tbpData_Accessories IsNot Nothing Then Return

        tbpData_Accessories = New TabPage(Accessories_Text("MainForm_Accessories_Tab", "Accessories and functions")) With {
            .UseVisualStyleBackColor = True
        }

        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 64, .Padding = New Padding(10, 8, 10, 6)}
        lblAccessoriesSearch = New Label With {.AutoSize = True, .Location = New Point(10, 12)}
        lblAccessoriesCategory = New Label With {.AutoSize = True, .Location = New Point(330, 12)}
        txtAccessoriesSearch = New TextBox With {.Location = New Point(78, 9), .Width = 230}
        cmbAccessoriesCategory = New ComboBox With {
            .Location = New Point(400, 9),
            .Width = 230,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        lblAccessoriesSummary = New Label With {
            .Location = New Point(10, 38),
            .AutoEllipsis = True,
            .Size = New Size(1040, 20),
            .Font = New Font(tbpData_Accessories.Font, FontStyle.Bold)
        }
        header.Controls.AddRange(New Control() {
            lblAccessoriesSearch, txtAccessoriesSearch, lblAccessoriesCategory,
            cmbAccessoriesCategory, lblAccessoriesSummary
        })

        dgvAccessories = New DataGridView With {
            .Dock = DockStyle.Fill,
            .AllowUserToAddRows = False,
            .AllowUserToDeleteRows = False,
            .AllowUserToResizeRows = False,
            .AutoGenerateColumns = False,
            .BackgroundColor = SystemColors.Window,
            .BorderStyle = BorderStyle.FixedSingle,
            .RowHeadersVisible = False,
            .SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            .MultiSelect = False,
            .AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells
        }
        dgvAccessories.DefaultCellStyle.WrapMode = DataGridViewTriState.True
        Accessories_CreateColumns()

        tbpData_Accessories.Controls.Add(dgvAccessories)
        tbpData_Accessories.Controls.Add(header)
        AddHandler txtAccessoriesSearch.TextChanged, AddressOf Accessories_FilterChanged
        AddHandler cmbAccessoriesCategory.SelectedIndexChanged, AddressOf Accessories_FilterChanged
        AddHandler dgvAccessories.CurrentCellDirtyStateChanged, AddressOf Accessories_CurrentCellDirtyStateChanged
        AddHandler dgvAccessories.CellValueChanged, AddressOf Accessories_CellValueChanged
        AddHandler dgvAccessories.CellValidating, AddressOf Accessories_CellValidating

        tbcData.TabPages.Insert(Math.Min(3, tbcData.TabPages.Count), tbpData_Accessories)
        Accessories_UpdateLocalizedTexts()
    End Sub

    Private Sub Accessories_CreateColumns()
        dgvAccessories.Columns.Add(New DataGridViewCheckBoxColumn With {
            .Name = "Selected", .Width = 58, .ThreeState = False
        })
        dgvAccessories.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Code", .Width = 92, .ReadOnly = True
        })
        dgvAccessories.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Category", .Width = 140, .ReadOnly = True
        })
        dgvAccessories.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Description", .AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            .MinimumWidth = 220, .ReadOnly = True
        })
        dgvAccessories.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Functions", .Width = 260, .ReadOnly = True
        })
        dgvAccessories.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Status", .Width = 90, .ReadOnly = True
        })
        dgvAccessories.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Installation", .Width = 90, .ReadOnly = True
        })
        dgvAccessories.Columns.Add(New DataGridViewTextBoxColumn With {
            .Name = "Quantity", .Width = 68
        })
    End Sub

    Private Sub Accessories_FillAvailable()
        If tbpData_Accessories Is Nothing Then Return
        Dim model = SelectedHeatRecoveryModel
        Dim modelId = If(model Is Nothing, -1, model.Id)
        Dim modelChanged = modelId <> m_AccessoriesModelId
        m_AccessoriesModelId = modelId

        m_AccessoryItems.Clear()
        If model IsNot Nothing AndAlso Environment.DatabaseCompatibility IsNot Nothing AndAlso
            Environment.DatabaseCompatibility.HasFeature("AccessoriesAndControlFunctions") Then
            m_AccessoryItems = CLSelectionCatalogRepository.GetEffectiveItems(
                Environment.DCLiteDatabasePath,
                model.Id,
                Environment.PrimaryLanguageCode)
        End If

        If modelChanged Then
            m_AccessorySelected.Clear()
            m_AccessoryQuantities.Clear()
            For Each item In m_AccessoryItems
                If item.IsStandard OrElse item.DefaultSelected Then m_AccessorySelected.Add(item.Id)
                m_AccessoryQuantities(item.Id) = item.DefaultQuantity
            Next
        End If

        Accessories_FillCategories()
        Accessories_ApplyFilter()
        tbpData_Accessories.Enabled = m_AccessoryItems.Count > 0
    End Sub

    Private Sub Accessories_FillCategories()
        Dim previous = TryCast(cmbAccessoriesCategory.SelectedItem, CLAccessoryCategoryChoice)
        m_AccessoriesChanging = True
        Try
            cmbAccessoriesCategory.Items.Clear()
            cmbAccessoriesCategory.Items.Add(New CLAccessoryCategoryChoice(String.Empty, Accessories_Text("MainForm_Accessories_AllCategories", "All categories")))
            For Each category In m_AccessoryItems.
                GroupBy(Function(item) item.CategoryCode).
                Select(Function(group) New CLAccessoryCategoryChoice(group.Key, group.First().CategoryName)).
                OrderBy(Function(item) item.Name)

                cmbAccessoriesCategory.Items.Add(category)
            Next
            Dim selectedIndex = 0
            If previous IsNot Nothing Then
                For index = 0 To cmbAccessoriesCategory.Items.Count - 1
                    If String.Equals(DirectCast(cmbAccessoriesCategory.Items(index), CLAccessoryCategoryChoice).Code,
                        previous.Code, StringComparison.OrdinalIgnoreCase) Then
                        selectedIndex = index
                        Exit For
                    End If
                Next
            End If
            If cmbAccessoriesCategory.Items.Count > 0 Then cmbAccessoriesCategory.SelectedIndex = selectedIndex
        Finally
            m_AccessoriesChanging = False
        End Try
    End Sub

    Private Sub Accessories_ApplyFilter()
        If dgvAccessories Is Nothing Then Return
        Dim search = If(txtAccessoriesSearch.Text, String.Empty).Trim()
        Dim category = TryCast(cmbAccessoriesCategory.SelectedItem, CLAccessoryCategoryChoice)
        Dim categoryCode = If(category Is Nothing, String.Empty, category.Code)
        Dim filtered = m_AccessoryItems.Where(
            Function(item) Accessories_ItemMatchesFilter(item, categoryCode, search)).ToList()

        m_AccessoriesChanging = True
        Try
            dgvAccessories.Rows.Clear()
            Dim previousCategory = String.Empty
            For Each item In filtered
                Dim selected = m_AccessorySelected.Contains(item.Id)
                Dim quantity = If(m_AccessoryQuantities.ContainsKey(item.Id), m_AccessoryQuantities(item.Id), item.DefaultQuantity)
                Dim rowIndex = dgvAccessories.Rows.Add(
                    selected,
                    item.Code,
                    If(String.Equals(previousCategory, item.CategoryCode, StringComparison.OrdinalIgnoreCase), String.Empty, item.CategoryName),
                    item.Name,
                    item.FunctionsText,
                    Accessories_StatusText(item),
                    Accessories_InstallationText(item.InstallationType),
                    quantity)
                Dim row = dgvAccessories.Rows(rowIndex)
                row.Tag = item
                row.Cells("Selected").ReadOnly = item.IsStandard OrElse Not item.CustomerSelectable
                row.Cells("Quantity").ReadOnly = Not selected OrElse item.MaxQuantity <= 1
                row.Cells("Quantity").ToolTipText = String.Format(CultureInfo.CurrentCulture, "1 - {0}", item.MaxQuantity)
                If Not String.Equals(previousCategory, item.CategoryCode, StringComparison.OrdinalIgnoreCase) Then
                    row.DefaultCellStyle.BackColor = Color.FromArgb(242, 246, 251)
                    row.Cells("Category").Style.Font = New Font(dgvAccessories.Font, FontStyle.Bold)
                    previousCategory = item.CategoryCode
                End If
            Next
        Finally
            m_AccessoriesChanging = False
        End Try
        Accessories_UpdateSummary()
    End Sub

    Private Function Accessories_ItemMatchesFilter(item As CLSelectionCatalogItem,
        categoryCode As String,
        search As String) As Boolean

        Return (String.IsNullOrEmpty(categoryCode) OrElse
                String.Equals(item.CategoryCode, categoryCode, StringComparison.OrdinalIgnoreCase)) AndAlso
            (String.IsNullOrEmpty(search) OrElse
                item.Code.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 OrElse
                item.Name.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 OrElse
                item.CategoryName.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0 OrElse
                item.FunctionsText.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) >= 0)
    End Function

    Private Sub Accessories_FilterChanged(sender As Object, e As EventArgs)
        If Not m_AccessoriesChanging Then Accessories_ApplyFilter()
    End Sub

    Private Sub Accessories_CurrentCellDirtyStateChanged(sender As Object, e As EventArgs)
        If dgvAccessories.IsCurrentCellDirty Then dgvAccessories.CommitEdit(DataGridViewDataErrorContexts.Commit)
    End Sub

    Private Sub Accessories_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs)
        If m_AccessoriesChanging OrElse e.RowIndex < 0 OrElse e.ColumnIndex < 0 Then Return
        Dim row = dgvAccessories.Rows(e.RowIndex)
        Dim item = TryCast(row.Tag, CLSelectionCatalogItem)
        If item Is Nothing Then Return

        If dgvAccessories.Columns(e.ColumnIndex).Name = "Selected" Then
            Dim selected = Convert.ToBoolean(row.Cells("Selected").Value, CultureInfo.InvariantCulture)
            If selected Then
                m_AccessorySelected.Add(item.Id)
            Else
                m_AccessorySelected.Remove(item.Id)
            End If
            row.Cells("Quantity").ReadOnly = Not selected OrElse item.MaxQuantity <= 1
            Accessories_UpdateSummary()
        ElseIf dgvAccessories.Columns(e.ColumnIndex).Name = "Quantity" Then
            Accessories_StoreQuantity(row, item)
        End If
    End Sub

    Private Sub Accessories_CellValidating(sender As Object, e As DataGridViewCellValidatingEventArgs)
        If e.RowIndex < 0 OrElse dgvAccessories.Columns(e.ColumnIndex).Name <> "Quantity" Then Return
        Dim item = TryCast(dgvAccessories.Rows(e.RowIndex).Tag, CLSelectionCatalogItem)
        If item Is Nothing Then Return
        Dim value As Integer
        If Not Integer.TryParse(Convert.ToString(e.FormattedValue, CultureInfo.CurrentCulture), value) OrElse
            value < 1 OrElse value > item.MaxQuantity Then
            e.Cancel = True
            dgvAccessories.Rows(e.RowIndex).ErrorText = String.Format(CultureInfo.CurrentCulture, "1 - {0}", item.MaxQuantity)
        Else
            dgvAccessories.Rows(e.RowIndex).ErrorText = String.Empty
        End If
    End Sub

    Private Sub Accessories_StoreQuantity(row As DataGridViewRow, item As CLSelectionCatalogItem)
        Dim value As Integer
        If Integer.TryParse(Convert.ToString(row.Cells("Quantity").Value, CultureInfo.CurrentCulture), value) Then
            m_AccessoryQuantities(item.Id) = Math.Max(1, Math.Min(item.MaxQuantity, value))
        End If
    End Sub

    Private Sub Accessories_UpdateSummary()
        If lblAccessoriesSummary Is Nothing Then Return
        Dim codes = m_AccessoryItems.
            Where(Function(item) m_AccessorySelected.Contains(item.Id)).
            Select(Function(item) item.Code).
            OrderBy(Function(code) code).
            ToArray()
        lblAccessoriesSummary.Text = Accessories_Text("MainForm_Accessories_Selected", "Selected") & ": " &
            If(codes.Length = 0, "-", String.Join(" | ", codes))
    End Sub

    Private Sub Accessories_UpdateLocalizedTexts()
        If tbpData_Accessories Is Nothing Then Return
        tbpData_Accessories.Text = Accessories_Text("MainForm_Accessories_Tab", "Accessories and functions")
        lblAccessoriesSearch.Text = Accessories_Text("MainForm_Accessories_Search", "Search")
        lblAccessoriesCategory.Text = Accessories_Text("MainForm_Accessories_Category", "Category")
        dgvAccessories.Columns("Selected").HeaderText = Accessories_Text("MainForm_Accessories_Selected", "Selected")
        dgvAccessories.Columns("Code").HeaderText = Accessories_Text("MainForm_Accessories_Code", "Code")
        dgvAccessories.Columns("Category").HeaderText = Accessories_Text("MainForm_Accessories_Category", "Category")
        dgvAccessories.Columns("Description").HeaderText = Accessories_Text("MainForm_Accessories_Description", "Description")
        dgvAccessories.Columns("Functions").HeaderText = Accessories_Text("MainForm_Accessories_Functions", "Functions")
        dgvAccessories.Columns("Status").HeaderText = Accessories_Text("MainForm_Accessories_Status", "Status")
        dgvAccessories.Columns("Installation").HeaderText = Accessories_Text("MainForm_CoilPerformance_Installation", "Installation")
        dgvAccessories.Columns("Quantity").HeaderText = Accessories_Text("MainForm_ElectricHeater_Quantity", "Quantity")
        Accessories_FillAvailable()
    End Sub

    Private Function Accessories_StatusText(item As CLSelectionCatalogItem) As String
        Return If(item.IsStandard,
            Accessories_Text("MainForm_Accessories_Standard", "Standard"),
            Accessories_Text("MainForm_Accessories_Optional", "Optional"))
    End Function

    Private Function Accessories_InstallationText(value As String) As String
        If String.Equals(value, "Internal", StringComparison.OrdinalIgnoreCase) Then
            Return CoilPerformance_Text("MainForm_CoilPerformance_Internal", "Internal")
        End If
        Return CoilPerformance_Text("MainForm_CoilPerformance_External", "External")
    End Function

    Private Function Accessories_Text(key As String, fallback As String) As String
        Return CoilPerformance_Text(key, fallback)
    End Function

    Private NotInheritable Class CLAccessoryCategoryChoice
        Public Sub New(code As String, name As String)
            Me.Code = code
            Me.Name = name
        End Sub
        Public Property Code As String
        Public Property Name As String
        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class
End Class
