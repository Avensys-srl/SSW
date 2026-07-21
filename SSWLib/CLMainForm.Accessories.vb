Imports System.Data
Imports System.Globalization

Partial Public Class CLMainForm
    Private tbpData_Accessories As TabPage
    Private dgvAccessories As DataGridView
    Private txtAccessoriesSearch As TextBox
    Private cmbAccessoriesCategory As ComboBox
    Private lblAccessoriesSearch As Label
    Private lblAccessoriesCategory As Label
    Private grbAccessoriesSummary As GroupBox
    Private lblAccessoriesSummary As Label
    Private m_AccessoriesChanging As Boolean
    Private m_AccessoriesModelId As Integer = -1
    Private m_AccessoryItems As New List(Of CLSelectionCatalogItem)()
    Private ReadOnly m_AccessorySelected As New HashSet(Of Integer)()
    Private ReadOnly m_AccessoryQuantities As New Dictionary(Of Integer, Integer)()
    Private m_AccessoryUnresolvedSelections As New List(Of CLAccessorySelection)()

    Private Sub Accessories_InitializeTab()
        If tbpData_Accessories IsNot Nothing Then Return

        tbpData_Accessories = New TabPage(Accessories_Text("MainForm_Accessories_Tab", "Accessories and functions")) With {
            .UseVisualStyleBackColor = True
        }

        Dim header As New Panel With {.Dock = DockStyle.Top, .Height = 40, .Padding = New Padding(10, 8, 10, 6)}
        lblAccessoriesSearch = New Label With {.AutoSize = True, .Location = New Point(10, 12)}
        lblAccessoriesCategory = New Label With {.AutoSize = True, .Location = New Point(330, 12)}
        txtAccessoriesSearch = New TextBox With {.Location = New Point(78, 9), .Width = 230}
        cmbAccessoriesCategory = New ComboBox With {
            .Location = New Point(400, 9),
            .Width = 230,
            .DropDownStyle = ComboBoxStyle.DropDownList
        }
        grbAccessoriesSummary = New GroupBox With {
            .Location = New Point(grbPerformance_TemperatureConditions.Left,
                grbPerformance_TemperatureConditions.Bottom + 6),
            .Size = New Size(grbPerformance_TemperatureConditions.Width, 35),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        }
        lblAccessoriesSummary = New Label With {
            .Location = New Point(6, 14),
            .AutoEllipsis = True,
            .Size = New Size(grbAccessoriesSummary.ClientSize.Width - 12, 17),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right,
            .Font = New Font(grbPerformance_TemperatureConditions.Font, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleLeft,
            .UseMnemonic = False
        }
        header.Controls.AddRange(New Control() {
            lblAccessoriesSearch, txtAccessoriesSearch, lblAccessoriesCategory,
            cmbAccessoriesCategory
        })
        grbAccessoriesSummary.Controls.Add(lblAccessoriesSummary)
        pnlPerformance_Data.Controls.Add(grbAccessoriesSummary)
        grbAccessoriesSummary.BringToFront()

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
            m_AccessoryUnresolvedSelections.Clear()
            For Each item In m_AccessoryItems
                If item.IsStandard OrElse item.DefaultSelected Then m_AccessorySelected.Add(item.Id)
                m_AccessoryQuantities(item.Id) = item.DefaultQuantity
            Next
            Accessories_NormalizeSelection()
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
                Dim disabledReason = Accessories_DisabledReason(item)
                Dim requiredReason = Accessories_RequiredReason(item)
                row.Cells("Selected").ReadOnly = item.IsStandard OrElse Not item.CustomerSelectable OrElse
                    Not String.IsNullOrEmpty(disabledReason) OrElse Not String.IsNullOrEmpty(requiredReason)
                row.Cells("Quantity").ReadOnly = Not selected OrElse item.MaxQuantity <= 1
                row.Cells("Quantity").ToolTipText = String.Format(CultureInfo.CurrentCulture, "1 - {0}", item.MaxQuantity)
                row.Cells("Selected").ToolTipText = FirstAccessoryMessage(disabledReason, requiredReason)
                If Not String.IsNullOrEmpty(disabledReason) Then
                    row.DefaultCellStyle.ForeColor = SystemColors.GrayText
                End If
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
                Accessories_RemoveDependentsForExclusiveGroupChange(
                    m_AccessoryItems, m_AccessorySelected, item)
                m_AccessorySelected.Add(item.Id)
                Accessories_SelectDependencies(item)
                Accessories_ApplyExclusiveGroup(item)
            Else
                If String.IsNullOrEmpty(Accessories_RequiredReason(item)) Then
                    m_AccessorySelected.Remove(item.Id)
                    Accessories_RemoveEnabledDependents(item.Id)
                Else
                    m_AccessorySelected.Add(item.Id)
                End If
            End If
            Accessories_NormalizeSelection()
            Accessories_ApplyFilter()
            Project_MarkDirty()
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
            Dim normalized = Math.Max(1, Math.Min(item.MaxQuantity, value))
            Dim previous = If(m_AccessoryQuantities.ContainsKey(item.Id), m_AccessoryQuantities(item.Id), item.DefaultQuantity)
            m_AccessoryQuantities(item.Id) = normalized
            If previous <> normalized Then Project_MarkDirty()
        End If
    End Sub

    Private Function Accessories_CaptureSelection() As List(Of CLAccessorySelection)
        Dim result As New List(Of CLAccessorySelection)()
        For Each item In m_AccessoryItems.
            Where(Function(candidate) m_AccessorySelected.Contains(candidate.Id)).
            OrderBy(Function(candidate) candidate.Code, StringComparer.OrdinalIgnoreCase)

            result.Add(New CLAccessorySelection With {
                .Code = item.Code,
                .ItemType = item.ItemType,
                .Quantity = If(m_AccessoryQuantities.ContainsKey(item.Id), m_AccessoryQuantities(item.Id), item.DefaultQuantity),
                .Availability = item.Availability,
                .InstallationType = item.InstallationType,
                .LocalizedDisplayName = item.Name,
                .LocalizedDescription = item.Description,
                .LocalizedFunctionNames = item.FunctionNames.ToList()
            })
        Next
        For Each saved In m_AccessoryUnresolvedSelections.
            Where(Function(candidate) Not result.Any(Function(current) String.Equals(current.Code, candidate.Code, StringComparison.OrdinalIgnoreCase))).
            OrderBy(Function(candidate) candidate.Code, StringComparer.OrdinalIgnoreCase)

            result.Add(Accessories_CloneSelection(saved))
        Next
        Return result
    End Function

    Private Sub Accessories_ApplySelection(savedSelections As List(Of CLAccessorySelection))
        m_AccessoriesChanging = True
        Try
            m_AccessorySelected.Clear()
            m_AccessoryQuantities.Clear()
            m_AccessoryUnresolvedSelections.Clear()

            For Each item In m_AccessoryItems
                m_AccessoryQuantities(item.Id) = item.DefaultQuantity
                If item.IsStandard Then m_AccessorySelected.Add(item.Id)
            Next

            For Each saved In If(savedSelections, New List(Of CLAccessorySelection)())
                If saved Is Nothing OrElse String.IsNullOrWhiteSpace(saved.Code) Then Continue For
                Dim item = m_AccessoryItems.FirstOrDefault(
                    Function(candidate) String.Equals(candidate.Code, saved.Code, StringComparison.OrdinalIgnoreCase))
                If item Is Nothing Then
                    m_AccessoryUnresolvedSelections.Add(Accessories_CloneSelection(saved))
                Else
                    m_AccessorySelected.Add(item.Id)
                    m_AccessoryQuantities(item.Id) = Math.Max(1, Math.Min(item.MaxQuantity, saved.Quantity))
                End If
            Next
            Accessories_NormalizeSelection()
        Finally
            m_AccessoriesChanging = False
        End Try
        Accessories_ApplyFilter()
    End Sub

    Private Shared Function Accessories_CloneSelection(value As CLAccessorySelection) As CLAccessorySelection
        Return New CLAccessorySelection With {
            .Code = value.Code,
            .ItemType = value.ItemType,
            .Quantity = value.Quantity,
            .Availability = value.Availability,
            .InstallationType = value.InstallationType,
            .LocalizedDisplayName = value.LocalizedDisplayName,
            .LocalizedDescription = value.LocalizedDescription,
            .LocalizedFunctionNames = If(value.LocalizedFunctionNames,
                New List(Of String)()).ToList()
        }
    End Function

    Private Function Report_CreateAccessoryReportTable() As DataTable
        Dim table As New DataTable("AccessoryReport")
        For Each columnName In New String() {
            "Title", "CodeCaption", "DescriptionCaption", "FunctionsCaption", "StatusCaption",
            "Code", "Description", "Functions", "Status"
        }
            table.Columns.Add(columnName, GetType(String))
        Next

        For Each selected In Accessories_CaptureSelection()
            Dim item = m_AccessoryItems.FirstOrDefault(
                Function(candidate) String.Equals(candidate.Code, selected.Code,
                    StringComparison.OrdinalIgnoreCase))
            Dim displayName = If(item Is Nothing, selected.LocalizedDisplayName, item.Name)
            Dim functionNames = If(item Is Nothing,
                If(selected.LocalizedFunctionNames, New List(Of String)()),
                item.FunctionNames)
            Dim code = selected.Code
            If selected.Quantity > 1 Then code &= " x" & selected.Quantity.ToString(CultureInfo.CurrentCulture)
            Report_AddAccessoryRow(table, code,
                If(String.IsNullOrWhiteSpace(displayName), selected.Code, displayName),
                functionNames, String.Equals(selected.Availability, "Standard",
                    StringComparison.OrdinalIgnoreCase), selected.InstallationType)
        Next

        Dim waterCoil = Project_CaptureWaterCoil()
        If waterCoil.Enabled Then
            Dim mode = If(String.IsNullOrWhiteSpace(waterCoil.CalculationMode), "HCD", waterCoil.CalculationMode)
            Dim description = If(String.IsNullOrWhiteSpace(waterCoil.Coil.Name), waterCoil.Coil.Code, waterCoil.Coil.Name)
            Report_AddAccessoryRow(table, mode, If(String.IsNullOrWhiteSpace(description), mode, description),
                Nothing, False, waterCoil.InstallationType)
        End If

        If m_ElectricModeControls.ContainsKey(CLElectricHeaterMode.PEHD) AndAlso
            m_ElectricModeControls.ContainsKey(CLElectricHeaterMode.EHD) Then
            Dim electricHeater = ElectricHeater_CaptureSelection()
            For Each heaterMode In New CLElectricHeaterModeSelection() {electricHeater.PEHD, electricHeater.EHD}
                If heaterMode Is Nothing OrElse Not heaterMode.Enabled Then Continue For
                Dim mode = If(String.IsNullOrWhiteSpace(heaterMode.Mode), "-", heaterMode.Mode)
                Dim description = If(String.IsNullOrWhiteSpace(heaterMode.Heater.Name), heaterMode.Heater.Code, heaterMode.Heater.Name)
                Report_AddAccessoryRow(table, mode, If(String.IsNullOrWhiteSpace(description), mode, description),
                    Nothing, False, heaterMode.InstallationType)
            Next
        End If
        Return table
    End Function

    Private Sub Report_AddAccessoryRow(table As DataTable,
        code As String,
        description As String,
        functionNames As IEnumerable(Of String),
        isStandard As Boolean,
        installationType As String)

        Dim installation = Accessories_InstallationText(installationType)
        Dim statusSymbol = If(isStandard, ChrW(&H25CF),
            If(String.Equals(installationType, "Internal", StringComparison.OrdinalIgnoreCase),
                ChrW(&H2666), ChrW(&H25A0)))
        Dim statusText = If(isStandard,
            Accessories_Text("MainForm_Accessories_Standard", "Standard"), installation)
        Dim functions = If(functionNames, Enumerable.Empty(Of String)()).
            Where(Function(value) Not String.IsNullOrWhiteSpace(value)).ToList()

        Dim row = table.NewRow()
        row("Title") = Accessories_Text("MainForm_Accessories_Tab", "Accessories and functions")
        row("CodeCaption") = Accessories_Text("MainForm_Accessories_Code", "Code")
        row("DescriptionCaption") = Accessories_Text("MainForm_Accessories_Description", "Description")
        row("FunctionsCaption") = Accessories_Text("MainForm_Accessories_Functions", "Functions")
        row("StatusCaption") = Accessories_Text("MainForm_Accessories_Status", "Status")
        row("Code") = code
        row("Description") = description
        row("Functions") = If(functions.Count = 0, "-", String.Join(System.Environment.NewLine, functions))
        row("Status") = String.Format(CultureInfo.CurrentCulture, "{0} {1}", statusSymbol, statusText)
        table.Rows.Add(row)
    End Sub

    Private Sub Accessories_NormalizeSelection()
        For Each item In m_AccessoryItems.Where(Function(candidate) candidate.IsStandard)
            m_AccessorySelected.Add(item.Id)
        Next

        Dim changed As Boolean
        Do
            changed = False
            For Each item In m_AccessoryItems.Where(Function(candidate) m_AccessorySelected.Contains(candidate.Id))
                For Each dependency In item.Dependencies
                    If Accessories_IsAutoDependency(dependency.DependencyType) AndAlso
                        Not m_AccessorySelected.Contains(dependency.TargetItemId) Then
                        m_AccessorySelected.Add(dependency.TargetItemId)
                        Dim target = m_AccessoryItems.FirstOrDefault(Function(candidate) candidate.Id = dependency.TargetItemId)
                        If target IsNot Nothing Then Accessories_ApplyExclusiveGroup(target)
                        changed = True
                    ElseIf String.Equals(dependency.DependencyType, "Enables", StringComparison.OrdinalIgnoreCase) AndAlso
                        Not m_AccessorySelected.Contains(dependency.TargetItemId) Then
                        m_AccessorySelected.Remove(item.Id)
                        changed = True
                    End If
                Next
            Next
        Loop While changed
    End Sub

    Private Sub Accessories_SelectDependencies(item As CLSelectionCatalogItem)
        For Each dependency In item.Dependencies
            If Accessories_IsAutoDependency(dependency.DependencyType) Then
                m_AccessorySelected.Add(dependency.TargetItemId)
                Dim target = m_AccessoryItems.FirstOrDefault(Function(candidate) candidate.Id = dependency.TargetItemId)
                If target IsNot Nothing Then Accessories_ApplyExclusiveGroup(target)
            End If
        Next
    End Sub

    Private Sub Accessories_ApplyExclusiveGroup(selectedItem As CLSelectionCatalogItem)
        If String.IsNullOrWhiteSpace(selectedItem.ExclusiveGroupCode) Then Return
        For Each item In m_AccessoryItems
            If item.Id <> selectedItem.Id AndAlso
                String.Equals(item.ExclusiveGroupCode, selectedItem.ExclusiveGroupCode, StringComparison.OrdinalIgnoreCase) Then
                m_AccessorySelected.Remove(item.Id)
            End If
        Next
    End Sub

    Private Shared Sub Accessories_RemoveDependentsForExclusiveGroupChange(
        items As IEnumerable(Of CLSelectionCatalogItem),
        selectedIds As HashSet(Of Integer),
        selectedItem As CLSelectionCatalogItem)

        If String.IsNullOrWhiteSpace(selectedItem.ExclusiveGroupCode) Then Return

        Dim itemList = items.ToList()
        Dim removedTargets As New HashSet(Of Integer)(itemList.
            Where(Function(item) item.Id <> selectedItem.Id AndAlso
                selectedIds.Contains(item.Id) AndAlso
                String.Equals(item.ExclusiveGroupCode, selectedItem.ExclusiveGroupCode,
                    StringComparison.OrdinalIgnoreCase)).
            Select(Function(item) item.Id))
        If removedTargets.Count = 0 Then Return

        Dim changed As Boolean
        Do
            changed = False
            For Each dependent In itemList.
                Where(Function(item) selectedIds.Contains(item.Id) AndAlso
                    Not item.IsStandard).
                ToArray()

                If dependent.Dependencies.Any(
                    Function(dependency) Accessories_IsAutoDependency(dependency.DependencyType) AndAlso
                        removedTargets.Contains(dependency.TargetItemId)) Then
                    selectedIds.Remove(dependent.Id)
                    removedTargets.Add(dependent.Id)
                    changed = True
                End If
            Next
        Loop While changed
    End Sub

    Private Sub Accessories_RemoveEnabledDependents(targetItemId As Integer)
        For Each item In m_AccessoryItems
            If Accessories_HasDependency(item, "Enables", targetItemId) Then
                m_AccessorySelected.Remove(item.Id)
            End If
        Next
    End Sub

    Private Function Accessories_DisabledReason(item As CLSelectionCatalogItem) As String
        If Not String.Equals(item.ExclusiveGroupCode, "KTS", StringComparison.OrdinalIgnoreCase) Then
            Dim selectedController = m_AccessoryItems.FirstOrDefault(
                Function(candidate) Accessories_IsSelectedKts(candidate))
            If selectedController IsNot Nothing AndAlso
                selectedController.ControllerLevel < item.MinimumControllerLevel Then
                Return Accessories_Text("MainForm_Accessories_RequiresExtraController",
                    "Requires KTS Extra or higher.")
            End If
        End If

        For Each dependency In item.Dependencies
            If String.Equals(dependency.DependencyType, "Enables", StringComparison.OrdinalIgnoreCase) AndAlso
                Not m_AccessorySelected.Contains(dependency.TargetItemId) Then
                Return String.Format(CultureInfo.CurrentCulture,
                    Accessories_Text("MainForm_Accessories_EnableFirst", "Select {0} first."),
                    dependency.TargetCode)
            End If
            If String.Equals(dependency.DependencyType, "Conflicts", StringComparison.OrdinalIgnoreCase) AndAlso
                m_AccessorySelected.Contains(dependency.TargetItemId) Then
                Return String.Format(CultureInfo.CurrentCulture,
                    Accessories_Text("MainForm_Accessories_ConflictsWith", "Not compatible with {0}."),
                    dependency.TargetCode)
            End If
        Next

        Dim reverseConflict = m_AccessoryItems.FirstOrDefault(
            Function(source) m_AccessorySelected.Contains(source.Id) AndAlso
                Accessories_HasDependency(source, "Conflicts", item.Id))
        If reverseConflict IsNot Nothing Then
            Return String.Format(CultureInfo.CurrentCulture,
                Accessories_Text("MainForm_Accessories_ConflictsWith", "Not compatible with {0}."),
                reverseConflict.Code)
        End If

        If String.Equals(item.ExclusiveGroupCode, "KTS", StringComparison.OrdinalIgnoreCase) Then
            Dim requiringItems = m_AccessoryItems.Where(
                Function(candidate) Accessories_RequiresHigherController(candidate, item.ControllerLevel)).
                Select(Function(candidate) candidate.Code).ToArray()
            If requiringItems.Length > 0 Then
                Return String.Format(CultureInfo.CurrentCulture,
                    Accessories_Text("MainForm_Accessories_ControllerLevel", "Requires a higher controller level because of: {0}."),
                    String.Join(", ", requiringItems))
            End If
        End If

        Return String.Empty
    End Function

    Private Function Accessories_RequiredReason(item As CLSelectionCatalogItem) As String
        Dim requiringItems = m_AccessoryItems.Where(
            Function(source) m_AccessorySelected.Contains(source.Id) AndAlso
                (Accessories_HasDependency(source, "Requires", item.Id) OrElse
                 Accessories_HasDependency(source, "Includes", item.Id))).
            Select(Function(source) source.Code).ToArray()
        If requiringItems.Length = 0 Then Return String.Empty
        Return String.Format(CultureInfo.CurrentCulture,
            Accessories_Text("MainForm_Accessories_RequiredBy", "Required by: {0}."),
            String.Join(", ", requiringItems))
    End Function

    Private Shared Function Accessories_HasDependency(item As CLSelectionCatalogItem,
        dependencyType As String,
        targetItemId As Integer) As Boolean

        Return item.Dependencies.Any(Function(dependency) String.Equals(
            dependency.DependencyType, dependencyType, StringComparison.OrdinalIgnoreCase) AndAlso
            dependency.TargetItemId = targetItemId)
    End Function

    Private Shared Function Accessories_IsAutoDependency(dependencyType As String) As Boolean
        Return String.Equals(dependencyType, "Requires", StringComparison.OrdinalIgnoreCase) OrElse
            String.Equals(dependencyType, "Includes", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Function Accessories_RequiresHigherController(item As CLSelectionCatalogItem,
        controllerLevel As Integer) As Boolean

        Return m_AccessorySelected.Contains(item.Id) AndAlso
            Not String.Equals(item.ExclusiveGroupCode, "KTS", StringComparison.OrdinalIgnoreCase) AndAlso
            item.MinimumControllerLevel > controllerLevel
    End Function

    Private Function Accessories_IsSelectedKts(item As CLSelectionCatalogItem) As Boolean
        Return m_AccessorySelected.Contains(item.Id) AndAlso
            String.Equals(item.ExclusiveGroupCode, "KTS", StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Function FirstAccessoryMessage(ParamArray messages() As String) As String
        For Each message In messages
            If Not String.IsNullOrWhiteSpace(message) Then Return message
        Next
        Return String.Empty
    End Function

    Private Sub Accessories_UpdateSummary()
        If lblAccessoriesSummary Is Nothing Then Return
        Dim selectedItems = m_AccessoryItems.
            Where(Function(item) m_AccessorySelected.Contains(item.Id)).
            OrderBy(Function(item) item.Code).
            ToArray()
        Dim codes = selectedItems.
            Select(Function(item) item.Code).
            ToArray()
        Dim descriptions = selectedItems.
            Select(Function(item) item.Name).
            Where(Function(description) Not String.IsNullOrWhiteSpace(description)).
            ToArray()
        Dim separator = " " & ChrW(&HB7) & " "
        lblAccessoriesSummary.Text = If(codes.Length = 0, "-", String.Join(separator, codes))
        ToolTip1.SetToolTip(lblAccessoriesSummary,
            If(descriptions.Length = 0, "-", String.Join(System.Environment.NewLine, descriptions)))
    End Sub

    Private Sub Accessories_UpdateLocalizedTexts()
        If tbpData_Accessories Is Nothing Then Return
        tbpData_Accessories.Text = Accessories_Text("MainForm_Accessories_Tab", "Accessories and functions")
        grbAccessoriesSummary.Text = Accessories_Text("MainForm_Accessories_Summary", "Selected accessories")
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
            String.Empty)
    End Function

    Private Function Accessories_InstallationText(value As String) As String
        If String.Equals(value, "Internal", StringComparison.OrdinalIgnoreCase) Then
            Return CoilPerformance_Text("MainForm_CoilPerformance_Internal", "Internal")
        End If
        If String.Equals(value, "RequestedInternal", StringComparison.OrdinalIgnoreCase) Then
            Return CoilPerformance_Text("MainForm_CoilPerformance_RequestInternal", "Request internal")
        End If
        Return CoilPerformance_Text("MainForm_CoilPerformance_ExternalInstallation", "External")
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
