Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Linq
Imports Climalombarda.DataCentral.LTModel

Public NotInheritable Class CLInstallationLayoutReportRenderer

    Private Const ImageWidth As Integer = 1600
    Private Const ImageHeight As Integer = 560

    Private Sub New()
    End Sub

    Public Shared Function Render(model As CLDCHeatRecoveryModel,
        requestedConfigurationCode As String,
        requestedInstallationMode As String) As Bitmap

        Using content = Create(model, requestedConfigurationCode, requestedInstallationMode)
            Return DirectCast(content.Image.Clone(), Bitmap)
        End Using
    End Function

    Public Shared Function Create(model As CLDCHeatRecoveryModel,
        requestedConfigurationCode As String,
        requestedInstallationMode As String) As CLInstallationLayoutReportContent

        If model Is Nothing Then
            Return New CLInstallationLayoutReportContent(New Bitmap(2, 2),
                T("Report_InstallationLayout_Title", "Installation configuration"),
                T("Report_InstallationLayout_Configuration", "Configuration"),
                String.Empty,
                T("Report_InstallationLayout_Installation", "Installation"),
                String.Empty)
        End If

        Dim snapshot = CLInstallationLayoutRepository.Create().
            GetForModel(model, requestedConfigurationCode)
        Dim configuration = snapshot.Configurations.FirstOrDefault(Function(item) _
            String.Equals(item.Code, snapshot.ConfigurationCode,
                StringComparison.OrdinalIgnoreCase))
        If configuration Is Nothing Then
            configuration = snapshot.Configurations.FirstOrDefault()
        End If
        Dim configurationCode = If(configuration Is Nothing,
            If(snapshot.ConfigurationCode, String.Empty), configuration.Code)
        If String.IsNullOrWhiteSpace(configurationCode) Then configurationCode = "-"
        If configuration Is Nothing Then
            ' Historical reports remain readable, but must not invent a drawing
            ' for a model with no active installation configuration.
            Return New CLInstallationLayoutReportContent(New Bitmap(2, 2),
                T("Report_InstallationLayout_Title", "Installation configuration"),
                T("Report_InstallationLayout_Configuration", "Configuration"),
                T("MainForm_Accessories_Unavailable", "Not available for this unit."),
                T("Report_InstallationLayout_Installation", "Installation"), String.Empty)
        End If
        Dim installationMode = NormalizeInstallationMode(configuration.InstallationMode)
        If installationMode.Length = 0 Then installationMode = NormalizeInstallationMode(requestedInstallationMode)
        If installationMode.Length = 0 Then installationMode = "ceiling"

        Dim bitmap As New Bitmap(ImageWidth, ImageHeight, PixelFormat.Format32bppArgb)
        bitmap.SetResolution(192.0F, 192.0F)
        Using canvas As Graphics = Graphics.FromImage(bitmap)
            canvas.SmoothingMode = SmoothingMode.AntiAlias
            canvas.TextRenderingHint = Drawing.Text.TextRenderingHint.ClearTypeGridFit
            canvas.Clear(Color.White)
            DrawLayout(canvas, snapshot, model.Code, configurationCode, installationMode)
        End Using
        Return New CLInstallationLayoutReportContent(bitmap,
            T("Report_InstallationLayout_Title", "Installation configuration"),
            T("Report_InstallationLayout_Configuration", "Configuration"),
            configurationCode,
            T("Report_InstallationLayout_Installation", "Installation"),
            InstallationCaption(installationMode))
    End Function

    Private Shared Sub DrawLayout(graphics As Graphics,
        snapshot As CLInstallationLayoutSnapshot, modelCode As String,
        configurationCode As String, installationMode As String)

        Dim configuration = snapshot.Configurations.Single(Function(item) EqualsCode(item.Code, configurationCode))
        Dim sameSide = configuration.ReferenceView.StartsWith("SSC_", StringComparison.OrdinalIgnoreCase)
        Dim uprightSameSide = sameSide AndAlso installationMode = "floor" AndAlso
            configuration.ReferenceView = "SSC_UPRIGHT"
        Dim flatFloorSameSide = sameSide AndAlso installationMode = "floor" AndAlso
            configuration.ReferenceView = "SSC_FLAT"
        Dim eastWestWall = Not sameSide AndAlso installationMode = "wall" AndAlso
            configuration.ReferenceView = "OSC_EAST_WEST"

        Dim unitRectangle As RectangleF
        If installationMode = "wall" OrElse uprightSameSide Then
            unitRectangle = New RectangleF(430, 140, 740, 250)
        Else
            ' Keep the report diagram aligned with the proportions used by the
            ' Next UI: a wide, shallow casing with ample space for flow labels.
            unitRectangle = New RectangleF(220, 205, 1160, 150)
        End If

        Using unitBrush As New SolidBrush(Color.FromArgb(232, 237, 243)),
              unitPen As New Pen(Color.FromArgb(110, 130, 151), 3.0F),
              ductPen As New Pen(Color.FromArgb(39, 53, 68), 3.0F),
              accessFont As New Font("Arial", 10.0F, FontStyle.Regular)

            graphics.FillRectangle(unitBrush, unitRectangle)
            graphics.DrawRectangle(unitPen, unitRectangle.X, unitRectangle.Y,
                unitRectangle.Width, unitRectangle.Height)
            DrawExchanger(graphics, unitRectangle)

            Dim ports = BuildPortPlacements(snapshot, unitRectangle, sameSide,
                uprightSameSide, flatFloorSameSide, eastWestWall, installationMode)
            For Each placement In ports
                DrawPort(graphics, placement, ductPen)
            Next

            DrawAccessPanel(graphics, unitRectangle, accessFont,
                configuration.AccessSide)
        End Using
    End Sub

    Private Shared Function BuildPortPlacements(snapshot As CLInstallationLayoutSnapshot,
        unitRectangle As RectangleF, sameSide As Boolean, uprightSameSide As Boolean,
        flatFloorSameSide As Boolean, eastWestWall As Boolean,
        installationMode As String) As List(Of PortPlacement)

        Dim result As New List(Of PortPlacement)()
        Dim ordered = snapshot.FlowPorts.
            Where(Function(item) item.Position.HasValue).
            OrderBy(Function(item) item.Position.Value).ToList()
        If ordered.Count <> 4 Then ordered = snapshot.FlowPorts.Take(4).ToList()

        If sameSide Then
            Dim edge As PortEdge
            Dim faceOn As Boolean
            Dim centerY As Single
            If uprightSameSide Then
                ' A1/B1: the unit stands on its back, with ducts facing the ceiling.
                edge = PortEdge.Top
                faceOn = False
                centerY = unitRectangle.Top
            ElseIf flatFloorSameSide Then
                ' A3/B3: the unit lies on the floor and the four ports face the viewer.
                edge = PortEdge.Bottom
                faceOn = True
                centerY = unitRectangle.Bottom - 55.0F
            Else
                ' Ceiling/fallback SSC layouts show the four ports on the front face.
                edge = PortEdge.Top
                faceOn = True
                centerY = unitRectangle.Top + 55.0F
            End If
            Dim sameSidePositions = {0.13F, 0.38F, 0.63F, 0.88F}
            For index = 0 To ordered.Count - 1
                Dim x = unitRectangle.Left + unitRectangle.Width * sameSidePositions(index)
                result.Add(New PortPlacement(ordered(index).FlowCode, edge,
                    New PointF(x, centerY), faceOn))
            Next
            Return result
        End If

        If installationMode = "wall" AndAlso eastWestWall Then
            Dim points = {
                New Tuple(Of PortEdge, PointF)(PortEdge.Left,
                    New PointF(unitRectangle.Left, unitRectangle.Top + unitRectangle.Height * 0.32F)),
                New Tuple(Of PortEdge, PointF)(PortEdge.Right,
                    New PointF(unitRectangle.Right, unitRectangle.Top + unitRectangle.Height * 0.32F)),
                New Tuple(Of PortEdge, PointF)(PortEdge.Left,
                    New PointF(unitRectangle.Left, unitRectangle.Top + unitRectangle.Height * 0.72F)),
                New Tuple(Of PortEdge, PointF)(PortEdge.Right,
                    New PointF(unitRectangle.Right, unitRectangle.Top + unitRectangle.Height * 0.72F))}
            For index = 0 To Math.Min(ordered.Count, points.Length) - 1
                result.Add(New PortPlacement(ordered(index).FlowCode,
                    points(index).Item1, points(index).Item2, False))
            Next
            Return result
        End If

        Dim topBottom = {
            New PointF(unitRectangle.Left + unitRectangle.Width * 0.24F, unitRectangle.Top),
            New PointF(unitRectangle.Left + unitRectangle.Width * 0.76F, unitRectangle.Top),
            New PointF(unitRectangle.Left + unitRectangle.Width * 0.24F, unitRectangle.Bottom),
            New PointF(unitRectangle.Left + unitRectangle.Width * 0.76F, unitRectangle.Bottom)}
        For index = 0 To Math.Min(ordered.Count, topBottom.Length) - 1
            Dim edge = If(index < 2, PortEdge.Top, PortEdge.Bottom)
            Dim faceOn = installationMode <> "wall"
            result.Add(New PortPlacement(ordered(index).FlowCode, edge,
                topBottom(index), faceOn))
        Next
        Return result
    End Function

    Private Shared Sub DrawPort(graphics As Graphics, placement As PortPlacement,
        ductPen As Pen)

        Const radius As Single = 30.0F
        Const ductLength As Single = 54.0F
        If placement.FaceOn Then
            graphics.DrawEllipse(ductPen, placement.Center.X - radius,
                placement.Center.Y - radius, radius * 2, radius * 2)
        Else
            Dim rectangle As RectangleF
            If placement.Edge = PortEdge.Left OrElse placement.Edge = PortEdge.Right Then
                rectangle = New RectangleF(
                    If(placement.Edge = PortEdge.Left,
                        placement.Center.X - ductLength, placement.Center.X),
                    placement.Center.Y - radius, ductLength, radius * 2)
            Else
                rectangle = New RectangleF(placement.Center.X - radius,
                    If(placement.Edge = PortEdge.Top,
                        placement.Center.Y - ductLength, placement.Center.Y),
                    radius * 2, ductLength)
            End If
            graphics.DrawRectangle(ductPen, rectangle.X, rectangle.Y,
                rectangle.Width, rectangle.Height)
        End If
        DrawFlowLabel(graphics, placement)
    End Sub

    Private Shared Sub DrawFlowLabel(graphics As Graphics, placement As PortPlacement)
        Dim incoming = String.Equals(placement.FlowCode, "Fresh",
            StringComparison.OrdinalIgnoreCase) OrElse
            String.Equals(placement.FlowCode, "Return", StringComparison.OrdinalIgnoreCase)
        Dim color = FlowColor(placement.FlowCode)
        Dim label = FlowCaption(placement.FlowCode)
        Using font As New Font("Arial", 10.0F, FontStyle.Bold),
              brush As New SolidBrush(color),
              pen As New Pen(color, 3.0F)
            pen.CustomEndCap = New AdjustableArrowCap(5, 6, True)
            Dim p1 As PointF
            Dim p2 As PointF
            Dim textRectangle As RectangleF
            Select Case placement.Edge
                Case PortEdge.Top
                    Dim topOuterY = placement.Center.Y - If(placement.FaceOn, 30.0F, 54.0F)
                    Dim topRowY = topOuterY - 55.0F
                    Dim topArrowX = placement.Center.X - 92.0F
                    If incoming Then
                        p1 = New PointF(topArrowX, topRowY - 13.0F)
                        p2 = New PointF(topArrowX, topRowY + 13.0F)
                    Else
                        p1 = New PointF(topArrowX, topRowY + 13.0F)
                        p2 = New PointF(topArrowX, topRowY - 13.0F)
                    End If
                    textRectangle = New RectangleF(placement.Center.X - 64,
                        topRowY - 16.0F, 190, 32)
                Case PortEdge.Bottom
                    Dim bottomOuterY = placement.Center.Y + If(placement.FaceOn, 30.0F, 54.0F)
                    Dim bottomRowY = bottomOuterY + 55.0F
                    Dim bottomArrowX = placement.Center.X - 92.0F
                    If incoming Then
                        p1 = New PointF(bottomArrowX, bottomRowY + 13.0F)
                        p2 = New PointF(bottomArrowX, bottomRowY - 13.0F)
                    Else
                        p1 = New PointF(bottomArrowX, bottomRowY - 13.0F)
                        p2 = New PointF(bottomArrowX, bottomRowY + 13.0F)
                    End If
                    textRectangle = New RectangleF(placement.Center.X - 64,
                        bottomRowY - 16.0F, 190, 32)
                Case PortEdge.Left
                    p1 = New PointF(placement.Center.X - 100, placement.Center.Y)
                    p2 = New PointF(p1.X + If(incoming, 26, -26), p1.Y)
                    textRectangle = New RectangleF(placement.Center.X - 210,
                        placement.Center.Y - 16, 100, 32)
                Case Else
                    p1 = New PointF(placement.Center.X + 100, placement.Center.Y)
                    p2 = New PointF(p1.X - If(incoming, 26, -26), p1.Y)
                    textRectangle = New RectangleF(placement.Center.X + 110,
                        placement.Center.Y - 16, 150, 32)
            End Select
            graphics.DrawLine(pen, p1, p2)
            graphics.DrawString(label, font, brush, textRectangle)
        End Using
    End Sub

    Private Shared Sub DrawExchanger(graphics As Graphics, unitRectangle As RectangleF)
        Dim centerX = unitRectangle.Left + unitRectangle.Width / 2.0F
        Dim centerY = unitRectangle.Top + unitRectangle.Height / 2.0F
        Dim points = {
            New PointF(centerX, centerY - 34),
            New PointF(centerX + 34, centerY),
            New PointF(centerX, centerY + 34),
            New PointF(centerX - 34, centerY)}
        Using pen As New Pen(Color.FromArgb(110, 130, 151), 3.0F)
            graphics.DrawPolygon(pen, points)
        End Using
    End Sub

    Private Shared Sub DrawAccessPanel(graphics As Graphics,
        unitRectangle As RectangleF, font As Font, position As String)

        Dim caption = T("Report_InstallationLayout_AccessPanel", "Access panel") & ": " &
            AccessCaption(position)
        Dim size = graphics.MeasureString(caption, font)
        Dim width = Math.Max(280.0F, size.Width + 30.0F)
        Dim x = unitRectangle.Left + (unitRectangle.Width - width) / 2.0F
        Dim y As Single
        Select Case position
            Case "upper"
                y = unitRectangle.Top - 17.0F
            Case "lower"
                y = unitRectangle.Bottom - 17.0F
            Case Else
                y = unitRectangle.Top + unitRectangle.Height * 0.64F
        End Select
        Using brush As New SolidBrush(Color.FromArgb(91, 111, 132))
            graphics.FillRectangle(brush, x, y, width, 34.0F)
        End Using
        DrawCenteredText(graphics, caption, font, Brushes.White,
            New RectangleF(x, y + 4, width, 26))
    End Sub

    Private Shared Function AccessPosition(installationMode As String,
        uprightSameSide As Boolean) As String
        If installationMode = "ceiling" Then Return "lower"
        If installationMode = "floor" AndAlso Not uprightSameSide Then Return "upper"
        Return "front"
    End Function

    Private Shared Function NormalizeInstallationMode(value As String) As String
        Dim normalized = If(value, String.Empty).Trim().ToLowerInvariant()
        Select Case normalized
            Case "ceiling", "floor", "wall"
                Return normalized
            Case Else
                Return String.Empty
        End Select
    End Function

    Private Shared Function InstallationCaption(mode As String) As String
        Select Case mode
            Case "floor" : Return T("Report_InstallationLayout_Floor", "Floor")
            Case "wall" : Return T("Report_InstallationLayout_Wall", "Wall")
            Case Else : Return T("Report_InstallationLayout_Ceiling", "Ceiling")
        End Select
    End Function

    Private Shared Function AccessCaption(position As String) As String
        Select Case position
            Case "upper" : Return T("Report_InstallationLayout_UpperAccess", "Upper access")
            Case "lower" : Return T("Report_InstallationLayout_LowerAccess", "Lower access")
            Case Else : Return T("Report_InstallationLayout_FrontAccess", "Front access")
        End Select
    End Function

    Private Shared Function FlowCaption(flowCode As String) As String
        Select Case If(flowCode, String.Empty).Trim().ToLowerInvariant()
            Case "fresh" : Return T("Report_InstallationLayout_Fresh", "Fresh air")
            Case "return" : Return T("Report_InstallationLayout_Return", "Return air")
            Case "supply" : Return T("Report_InstallationLayout_Supply", "Supply air")
            Case Else : Return T("Report_InstallationLayout_Exhaust", "Exhaust air")
        End Select
    End Function

    Private Shared Function FlowColor(flowCode As String) As Color
        Select Case If(flowCode, String.Empty).Trim().ToLowerInvariant()
            Case "fresh" : Return Color.FromArgb(33, 132, 215)
            Case "return" : Return Color.FromArgb(219, 145, 0)
            Case "supply" : Return Color.FromArgb(224, 67, 54)
            Case Else : Return Color.FromArgb(104, 123, 143)
        End Select
    End Function

    Private Shared Function IsSameSide(connectionCode As String,
        modelCode As String) As Boolean
        Return If(connectionCode, String.Empty).IndexOf("SSC",
            StringComparison.OrdinalIgnoreCase) >= 0 OrElse
            If(modelCode, String.Empty).IndexOf("SSC",
                StringComparison.OrdinalIgnoreCase) >= 0
    End Function

    Private Shared Function EqualsCode(left As String, right As String) As Boolean
        Return String.Equals(left, right, StringComparison.OrdinalIgnoreCase)
    End Function

    Private Shared Sub DrawCenteredText(graphics As Graphics, text As String,
        font As Font, brush As Brush, rectangle As RectangleF)
        Using format As New StringFormat With {
            .Alignment = StringAlignment.Center,
            .LineAlignment = StringAlignment.Center,
            .Trimming = StringTrimming.EllipsisCharacter
        }
            graphics.DrawString(If(text, String.Empty), font, brush, rectangle, format)
        End Using
    End Sub

    Private Shared Function T(key As String, fallback As String) As String
        Try
            Dim value = CLEnvironment.Current.Localization.GetString(key)
            If Not String.IsNullOrWhiteSpace(value) AndAlso value <> key AndAlso
                value.Trim() <> "?" Then Return value
        Catch
        End Try
        Return fallback
    End Function

    Private Enum PortEdge
        Top
        Right
        Bottom
        Left
    End Enum

    Private NotInheritable Class PortPlacement
        Public Sub New(flowCode As String, edge As PortEdge,
            center As PointF, faceOn As Boolean)
            Me.FlowCode = flowCode
            Me.Edge = edge
            Me.Center = center
            Me.FaceOn = faceOn
        End Sub

        Public ReadOnly Property FlowCode As String
        Public ReadOnly Property Edge As PortEdge
        Public ReadOnly Property Center As PointF
        Public ReadOnly Property FaceOn As Boolean
    End Class
End Class

Public NotInheritable Class CLInstallationLayoutReportContent
    Implements IDisposable

    Public Sub New(image As Bitmap, title As String,
        configurationCaption As String, configurationValue As String,
        installationCaption As String, installationValue As String)
        Me.Image = image
        Me.Title = title
        Me.ConfigurationCaption = configurationCaption
        Me.ConfigurationValue = configurationValue
        Me.InstallationCaption = installationCaption
        Me.InstallationValue = installationValue
    End Sub

    Public ReadOnly Property Image As Bitmap
    Public ReadOnly Property Title As String
    Public ReadOnly Property ConfigurationCaption As String
    Public ReadOnly Property ConfigurationValue As String
    Public ReadOnly Property InstallationCaption As String
    Public ReadOnly Property InstallationValue As String

    Public Sub Dispose() Implements IDisposable.Dispose
        If Image IsNot Nothing Then Image.Dispose()
    End Sub
End Class
