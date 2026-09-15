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
            DrawSchematic(canvas, snapshot, model.Code, configurationCode, installationMode)
        End Using
        Return New CLInstallationLayoutReportContent(bitmap,
            T("Report_InstallationLayout_Title", "Installation configuration"),
            T("Report_InstallationLayout_Configuration", "Configuration"),
            configurationCode,
            T("Report_InstallationLayout_Installation", "Installation"),
            InstallationCaption(installationMode))
    End Function

    Private Shared Sub DrawSchematic(g As Graphics, snapshot As CLInstallationLayoutSnapshot,
        modelCode As String, code As String, mode As String)
        Dim c = snapshot.Configurations.Single(Function(item) EqualsCode(item.Code, code))
        Dim ssc = c.ReferenceView.StartsWith("SSC_", StringComparison.OrdinalIgnoreCase)
        Dim ns = Not ssc AndAlso mode = "wall" AndAlso c.ReferenceView = "OSC_NORTH_SOUTH"
        Dim upright = ssc AndAlso mode = "floor" AndAlso c.ReferenceView = "SSC_UPRIGHT" AndAlso {"A1", "B1"}.Contains(code)
        Using outline As New Pen(ColorTranslator.FromHtml("#91A0AE"), 3), red As New Pen(ColorTranslator.FromHtml("#D62828"), 5),
            title As New Font("Arial", 15, FontStyle.Bold), label As New Font("Arial", 12), number As New Font("Arial", 15, FontStyle.Bold)
            DrawCenteredText(g, InstallationViewCaption(mode, Not ns), title, Brushes.Black, New RectangleF(If(upright, 10, 20), 10, 480, 40))
            DrawCenteredText(g, modelCode, title, Brushes.Black, New RectangleF(550, 10, 530, 40))
            Dim state = g.Save()
            g.TranslateTransform(40, 70)
            g.ScaleTransform(1.4F, 1.4F)
            If ns Then
                g.TranslateTransform(300, 210)
                g.RotateTransform(180)
            End If
            If mode = "wall" Then
                g.DrawRectangle(outline, 100.0F, 20.0F, 53.333F, 170.0F)
                g.DrawLine(red, 82, 35, 82, 175)
                g.DrawLine(red, 82, 70, 100, 70)
                g.DrawLine(red, 82, 140, 100, 140)
                DrawRedArrow(g, red, New PointF(270, 105), New PointF(163.333F, 105))
            ElseIf upright Then
                g.DrawRectangle(outline, 123.3335F, 25.0F, 53.333F, 170.0F)
                g.DrawLine(red, 95, 205, 205, 205)
                DrawRedArrow(g, red, New PointF(25, 110), New PointF(113.3335F, 110))
                DrawCenteredText(g, AccessCaption(c.AccessSide), label, Brushes.Black, New RectangleF(0, 65, 138.3335F, 30))
            Else
                g.DrawRectangle(outline, 65.0F, 65.0F, 170.0F, 53.333F)
                Dim supportY = If(mode = "floor", 165, 45)
                g.DrawLine(red, 45, supportY, 255, supportY)
                If mode = "ceiling" Then
                    g.DrawLine(red, 85, 45, 85, 65)
                    g.DrawLine(red, 215, 45, 215, 65)
                End If
                If c.AccessSide = "upper" Then
                    DrawRedArrow(g, red, New PointF(150, 15), New PointF(150, 55))
                Else
                    DrawRedArrow(g, red, New PointF(150, 200), New PointF(150, 128.333F))
                End If
            End If
            g.Restore(state)
            If mode = "floor" Then DrawCenteredText(g, SchematicLabel(4), label, Brushes.Black, New RectangleF(If(upright, 10, 20), 450, 480, 40))
            Dim r = If(ns, New RectangleF(700, 85, 196, 336), New RectangleF(580, 125, 420, 245))
            g.DrawRectangle(outline, r.X, r.Y, r.Width, r.Height)
            For Each p In snapshot.FlowPorts.Where(Function(item) item.Position.HasValue).OrderBy(Function(item) item.Position.Value)
                Dim n = p.Position.Value
                Dim x As Single
                Dim y As Single
                If ssc Then
                    x = r.Left + r.Width * (25 + (n - 1) * 63) / 240.0F
                    y = r.Top
                ElseIf ns Then
                    x = r.Left + r.Width * If(n Mod 2 = 1, 0.25F, 0.75F)
                    y = If(n <= 2, r.Bottom, r.Top)
                Else
                    x = If(n <= 2, r.Left, r.Right)
                    y = r.Top + r.Height * If(n Mod 2 = 1, 0.25F, 0.75F)
                End If
                DrawCircle(g, p.FlowCode, x, y, n, number)
            Next
            Using captionFont As New Font("Arial", 11), format As New StringFormat With {
                .Alignment = StringAlignment.Center, .LineAlignment = StringAlignment.Center,
                .Trimming = StringTrimming.None
            }
                g.DrawString(SchematicLabel(0), captionFont, Brushes.Black,
                    New RectangleF(r.Left + 20, r.Top + 45, r.Width - 40, r.Height - 90), format)
            End Using
            Dim roles = {"Fresh", "Supply", "Return", "Exhaust"}
            For i = 0 To 3
                DrawCircle(g, roles(i), 1150, 65 + i * 65, Nothing, number)
                g.DrawString(FlowCaption(roles(i)), label, Brushes.Black, 1200, 50 + i * 65)
            Next
            DrawRedArrow(g, red, New PointF(1150, 335), New PointF(1150, 380))
            g.DrawString(AccessCaption(c.AccessSide), label, Brushes.Black, New RectangleF(1200, 340, 380, 60))
        End Using
    End Sub

    Private Shared Sub DrawRedArrow(g As Graphics, pen As Pen, start As PointF, finish As PointF)
        Dim dx = finish.X - start.X
        Dim dy = finish.Y - start.Y
        Dim length = CSng(Math.Sqrt(dx * dx + dy * dy))
        dx /= length
        dy /= length
        g.DrawLine(pen, start, finish)
        g.DrawLine(pen, finish, New PointF(finish.X - dx * 12 - dy * 10, finish.Y - dy * 12 + dx * 10))
        g.DrawLine(pen, finish, New PointF(finish.X - dx * 12 + dy * 10, finish.Y - dy * 12 - dx * 10))
    End Sub

    Private Shared Sub DrawCircle(g As Graphics, role As String, x As Single, y As Single, n As Integer?, font As Font)
        Dim incoming = EqualsCode(role, "Fresh") OrElse EqualsCode(role, "Return")
        Using brush As New SolidBrush(If(incoming, Color.White, FlowColor(role))), pen As New Pen(FlowColor(role), 4)
            g.FillEllipse(brush, x - 25, y - 25, 50, 50)
            g.DrawEllipse(pen, x - 25, y - 25, 50, 50)
            If n.HasValue Then DrawCenteredText(g, n.Value.ToString(), font, If(incoming, Brushes.Black, Brushes.White), New RectangleF(x - 25, y - 25, 50, 50))
        End Using
    End Sub

    Private Shared Function SchematicLabel(index As Integer) As String
        Dim fallback = {"Airflow view from the access-panel side", "Air drawn towards the unit", "Air discharged from the unit", "Panel access / viewing direction", "Optional SHK shelf kit"}
        Return T("Report_Schematic_" & index.ToString(), fallback(index))
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
        If eastWestWall Then
            ' Side-connected units use the same compact proportion as the UI.
            unitRectangle = New RectangleF(370, 125, 380, 300)
        ElseIf installationMode = "wall" OrElse uprightSameSide Then
            unitRectangle = New RectangleF(250, 125, 620, 300)
        Else
            unitRectangle = New RectangleF(140, 195, 820, 170)
        End If

        Using unitBrush As New SolidBrush(Color.White),
              unitPen As New Pen(Color.FromArgb(17, 17, 17), 3.0F),
              ductPen As New Pen(Color.FromArgb(39, 53, 68), 3.0F),
              accessFont As New Font("Arial", 10.0F, FontStyle.Regular),
              installationFont As New Font("Arial", 12.0F, FontStyle.Bold)

            Dim viewCaption = (If(modelCode, String.Empty) & " " &
                InstallationViewCaption(installationMode, eastWestWall)).Trim()
            If sameSide Then
                DrawCenteredText(graphics,
                    viewCaption, installationFont, Brushes.Black,
                    New RectangleF(140.0F, 12.0F, 820.0F, 30.0F))
            Else
                graphics.DrawString(viewCaption,
                    installationFont, Brushes.Black, New RectangleF(20.0F, 12.0F, 320.0F, 30.0F))
            End If

            graphics.FillRectangle(unitBrush, unitRectangle)
            graphics.DrawRectangle(unitPen, unitRectangle.X, unitRectangle.Y,
                unitRectangle.Width, unitRectangle.Height)
            Using seamPen As New Pen(Color.FromArgb(210, 220, 218), 1.5F)
                graphics.DrawRectangle(seamPen, unitRectangle.X + 13,
                    unitRectangle.Y + 13, unitRectangle.Width - 26, unitRectangle.Height - 26)
            End Using

            Dim ports = BuildPortPlacements(snapshot, unitRectangle, sameSide,
                uprightSameSide, flatFloorSameSide, eastWestWall, installationMode)
            For Each placement In ports
                Using flowPen As New Pen(FlowColor(placement.FlowCode), 5.0F)
                    DrawPort(graphics, placement, flowPen)
                End Using
            Next

            DrawAccessPanel(graphics, unitRectangle, accessFont,
                configuration.AccessSide)
            DrawFlowLegend(graphics, configuration.AccessSide)
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
                    New PointF(x, centerY), faceOn, ordered(index).Position.Value))
            Next
            Return result
        End If

        If installationMode = "wall" AndAlso eastWestWall Then
            Dim points = {
                New Tuple(Of PortEdge, PointF)(PortEdge.Left,
                    New PointF(unitRectangle.Left, unitRectangle.Top + unitRectangle.Height * 0.32F)),
                New Tuple(Of PortEdge, PointF)(PortEdge.Left,
                    New PointF(unitRectangle.Left, unitRectangle.Top + unitRectangle.Height * 0.72F)),
                New Tuple(Of PortEdge, PointF)(PortEdge.Right,
                    New PointF(unitRectangle.Right, unitRectangle.Top + unitRectangle.Height * 0.32F)),
                New Tuple(Of PortEdge, PointF)(PortEdge.Right,
                    New PointF(unitRectangle.Right, unitRectangle.Top + unitRectangle.Height * 0.72F))}
            For index = 0 To Math.Min(ordered.Count, points.Length) - 1
                result.Add(New PortPlacement(ordered(index).FlowCode,
                    points(index).Item1, points(index).Item2, False, ordered(index).Position.Value))
            Next
            Return result
        End If

        Dim topBottom = {
            New PointF(unitRectangle.Left + unitRectangle.Width * 0.24F, unitRectangle.Bottom),
            New PointF(unitRectangle.Left + unitRectangle.Width * 0.76F, unitRectangle.Bottom),
            New PointF(unitRectangle.Left + unitRectangle.Width * 0.24F, unitRectangle.Top),
            New PointF(unitRectangle.Left + unitRectangle.Width * 0.76F, unitRectangle.Top)}
        For index = 0 To Math.Min(ordered.Count, topBottom.Length) - 1
            Dim edge = If(index < 2, PortEdge.Bottom, PortEdge.Top)
            Dim faceOn = installationMode <> "wall"
            result.Add(New PortPlacement(ordered(index).FlowCode, edge,
                topBottom(index), faceOn, ordered(index).Position.Value, faceOn AndAlso index >= 2))
        Next
        Return result
    End Function

    Private Shared Sub DrawPort(graphics As Graphics, placement As PortPlacement,
        ductPen As Pen)

        ' Symbols may touch the duct, but the duct is always painted on top.
        DrawFlowSymbol(graphics, placement)
        Const radius As Single = 30.0F
        Const ductLength As Single = 54.0F
        Dim numberCenter = placement.Center
        If placement.FaceOn Then
            If placement.Rear Then
                graphics.DrawArc(ductPen, placement.Center.X - radius,
                    placement.Center.Y - radius, radius * 2, radius * 2, 180, 180)
                numberCenter.Y -= 14
            Else
                graphics.FillEllipse(Brushes.White, placement.Center.X - radius,
                    placement.Center.Y - radius, radius * 2, radius * 2)
                graphics.DrawEllipse(ductPen, placement.Center.X - radius,
                    placement.Center.Y - radius, radius * 2, radius * 2)
            End If
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
            numberCenter = New PointF(rectangle.X + rectangle.Width / 2, rectangle.Y + rectangle.Height / 2)
        End If
        Using numberFont As New Font("Arial", 11.0F, FontStyle.Bold), numberBrush As New SolidBrush(Color.Black)
            DrawCenteredText(graphics, placement.Number.ToString(), numberFont, numberBrush,
                New RectangleF(numberCenter.X - 15, numberCenter.Y - 15, 30, 30))
        End Using
    End Sub

    Private Shared Sub DrawFlowSymbol(graphics As Graphics, placement As PortPlacement)
        Const width As Single = 39.0F
        Const height As Single = 43.5F
        Dim outerOffset = If(placement.FaceOn, 30.0F, 54.0F)
        Dim rectangle As RectangleF
        Select Case placement.Edge
            Case PortEdge.Top
                Dim bottomOffset = If(placement.FaceOn, height, outerOffset + 8.0F)
                rectangle = New RectangleF(placement.Center.X - width / 2,
                    placement.Center.Y - bottomOffset - height, width, height)
            Case PortEdge.Bottom
                Dim topOffset = If(placement.FaceOn, height, outerOffset + 8.0F)
                rectangle = New RectangleF(placement.Center.X - width / 2,
                    placement.Center.Y + topOffset, width, height)
            Case PortEdge.Left
                rectangle = New RectangleF(placement.Center.X - 54.0F - width - 12.0F,
                    placement.Center.Y - height / 2, width, height)
            Case Else
                rectangle = New RectangleF(placement.Center.X + 54.0F + 12.0F,
                    placement.Center.Y - height / 2, width, height)
        End Select
        Using symbol = LoadFlowIcon(placement.FlowCode)
            graphics.DrawImage(symbol, rectangle)
        End Using
    End Sub

    Private Shared Sub DrawFlowLegend(graphics As Graphics, accessPosition As String)
        Const x As Single = 1115.0F
        Const y As Single = 40.0F
        Const width As Single = 425.0F
        Const height As Single = 480.0F
        Using border As New Pen(Color.FromArgb(210, 220, 218), 2.0F),
              labelFont As New Font("Arial", 10.0F, FontStyle.Bold),
              observerLabelFont As New Font("Arial", 8.0F, FontStyle.Bold),
              arrowFont As New Font("Arial", 18.0F, FontStyle.Bold)
            graphics.DrawRectangle(border, x, y, width, height)
            Dim roles = {"Fresh", "Supply", "Return", "Exhaust"}
            For index = 0 To roles.Length - 1
                Dim itemY = y + 25.0F + index * 82.0F
                Using symbol = LoadFlowIcon(roles(index))
                    graphics.DrawImage(symbol, New RectangleF(x + 31.5F, itemY + 7.25F, 39.0F, 43.5F))
                End Using
                graphics.DrawString(FlowCaption(roles(index)), labelFont, Brushes.Black,
                    New RectangleF(x + 97.0F, itemY + 16.0F, width - 120.0F, 34.0F))
                Using colorPen As New Pen(FlowColor(roles(index)), 5.0F)
                    graphics.DrawLine(colorPen, x + 97.0F, itemY + 49.0F,
                        x + 157.0F, itemY + 49.0F)
                End Using
            Next
            Dim accessY = y + 25.0F + roles.Length * 82.0F
            If accessPosition = "front" Then
                DrawObserverCross(graphics, New PointF(x + 51.0F, accessY + 29.0F), 11.0F, 2.0F)
            Else
                DrawCenteredText(graphics, AccessArrow(accessPosition), arrowFont, Brushes.Black,
                    New RectangleF(x + 25.0F, accessY, 52.0F, 58.0F))
            End If
            Dim accessLabel = If(accessPosition = "front", LocalizedObserverAccessLabel(), LocalizedAccessLabel())
            Dim accessLabelFont = If(accessPosition = "front", observerLabelFont, labelFont)
            graphics.DrawString(accessLabel, accessLabelFont, Brushes.Black,
                New RectangleF(x + 97.0F, accessY + 16.0F, width - 105.0F, 34.0F))
        End Using
    End Sub

    Private Shared Function LoadFlowIcon(flowCode As String) As Bitmap
        Dim suffix = "." & flowCode.ToLowerInvariant() & ".png"
        Dim assembly = GetType(CLInstallationLayoutReportRenderer).Assembly
        Dim resourceName = assembly.GetManifestResourceNames().FirstOrDefault(
            Function(name) name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        If String.IsNullOrEmpty(resourceName) Then
            Throw New InvalidOperationException("Missing airflow icon resource: " & suffix)
        End If
        Using stream = assembly.GetManifestResourceStream(resourceName),
              source As New Bitmap(stream)
            Return DirectCast(source.Clone(), Bitmap)
        End Using
    End Function

    Private Shared Sub DrawAccessPanel(graphics As Graphics,
        unitRectangle As RectangleF, font As Font, position As String)

        Dim label = If(position = "front", LocalizedObserverAccessLabel(), LocalizedAccessLabel())
        Dim centerX = unitRectangle.Left + unitRectangle.Width / 2.0F
        Using arrowFont As New Font("Arial", 30.0F, FontStyle.Bold)
        Select Case position
            Case "upper"
                DrawCenteredText(graphics, label, font, Brushes.Black,
                    New RectangleF(centerX - 70.0F, unitRectangle.Top - 88.0F, 140.0F, 22.0F))
                DrawCenteredText(graphics, AccessArrow(position), arrowFont, Brushes.Black,
                    New RectangleF(centerX - 30.0F, unitRectangle.Top - 52.0F, 60.0F, 44.0F))
            Case "lower"
                DrawCenteredText(graphics, AccessArrow(position), arrowFont, Brushes.Black,
                    New RectangleF(centerX - 30.0F, unitRectangle.Bottom + 7.0F, 60.0F, 44.0F))
                DrawCenteredText(graphics, label, font, Brushes.Black,
                    New RectangleF(centerX - 70.0F, unitRectangle.Bottom + 63.0F, 140.0F, 22.0F))
            Case Else
                DrawObserverCross(graphics,
                    New PointF(centerX, unitRectangle.Top + unitRectangle.Height * 0.56F), 18.0F, 2.5F)
                DrawCenteredText(graphics, label, font, Brushes.Black,
                    New RectangleF(centerX - 160.0F, unitRectangle.Top + unitRectangle.Height * 0.67F, 320.0F, 22.0F))
        End Select
        End Using
    End Sub

    Private Shared Function LocalizedAccessLabel() As String
        Select Case Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            Case "it" : Return "Accesso"
            Case "bg" : Return "Достъп"
            Case "cs" : Return "Přístup"
            Case "da" : Return "Adgang"
            Case "de" : Return "Zugang"
            Case "fr" : Return "Accès"
            Case "hu" : Return "Hozzáférés"
            Case "is" : Return "Aðgangur"
            Case "nl" : Return "Toegang"
            Case "no" : Return "Tilgang"
            Case "pl" : Return "Dostęp"
            Case "ro" : Return "Acces"
            Case "sl" : Return "Dostop"
            Case "sv" : Return "Åtkomst"
            Case Else : Return "Access"
        End Select
    End Function

    Private Shared Function LocalizedObserverAccessLabel() As String
        Select Case Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            Case "it" : Return "Accesso lato osservatore"
            Case "bg" : Return "Достъп от страната на наблюдателя"
            Case "cs" : Return "Přístup ze strany pozorovatele"
            Case "da" : Return "Adgang fra observatørsiden"
            Case "de" : Return "Zugang auf Betrachterseite"
            Case "fr" : Return "Accès côté observateur"
            Case "hu" : Return "Hozzáférés a megfigyelő oldaláról"
            Case "is" : Return "Aðgangur frá áhorfendahlið"
            Case "nl" : Return "Toegang aan waarnemerszijde"
            Case "no" : Return "Tilgang fra observatørsiden"
            Case "pl" : Return "Dostęp od strony obserwatora"
            Case "ro" : Return "Acces dinspre observator"
            Case "sl" : Return "Dostop s strani opazovalca"
            Case "sv" : Return "Åtkomst från betraktarsidan"
            Case Else : Return "Observer-side access"
        End Select
    End Function

    Private Shared Function AccessArrow(position As String) As String
        Select Case position
            Case "upper" : Return ChrW(&H2193)
            Case "lower" : Return ChrW(&H2191)
            Case Else : Return String.Empty
        End Select
    End Function

    Private Shared Sub DrawObserverCross(graphics As Graphics, center As PointF,
        length As Single, thickness As Single)

        Dim offset = CSng(length / (2.0F * Math.Sqrt(2.0R)))
        Using pen As New Pen(Color.Black, thickness)
            graphics.DrawLine(pen, center.X - offset, center.Y - offset,
                center.X + offset, center.Y + offset)
            graphics.DrawLine(pen, center.X - offset, center.Y + offset,
                center.X + offset, center.Y - offset)
        End Using
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

    Private Shared Function InstallationViewCaption(mode As String, eastWestWall As Boolean) As String
        If mode = "ceiling" Then Return T("Report_InstallationLayout_Ceiling", "Ceiling")
        If mode = "floor" Then Return T("Report_InstallationLayout_Floor", "Floor")

        Select Case Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            Case "it" : Return If(eastWestWall, "Murale Est-Ovest", "Murale Nord-Sud")
            Case "de" : Return If(eastWestWall, "Wand Ost-West", "Wand Nord-Süd")
            Case "fr" : Return If(eastWestWall, "Murale Est-Ouest", "Murale Nord-Sud")
            Case "nl" : Return If(eastWestWall, "Wand oost-west", "Wand noord-zuid")
            Case "pl" : Return If(eastWestWall, "Ściana wschód-zachód", "Ściana północ-południe")
            Case "ro" : Return If(eastWestWall, "Perete est-vest", "Perete nord-sud")
            Case Else : Return If(eastWestWall, "East-West wall", "North-South wall")
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
            Case "fresh" : Return ColorTranslator.FromHtml("#43A047")
            Case "return" : Return ColorTranslator.FromHtml("#F2B800")
            Case "supply" : Return ColorTranslator.FromHtml("#008FD3")
            Case Else : Return ColorTranslator.FromHtml("#8B5A2B")
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
            center As PointF, faceOn As Boolean, number As Integer, Optional rear As Boolean = False)
            Me.FlowCode = flowCode
            Me.Edge = edge
            Me.Center = center
            Me.FaceOn = faceOn
            Me.Number = number
            Me.Rear = rear
        End Sub

        Public ReadOnly Property FlowCode As String
        Public ReadOnly Property Edge As PortEdge
        Public ReadOnly Property Center As PointF
        Public ReadOnly Property FaceOn As Boolean
        Public ReadOnly Property Number As Integer
        Public ReadOnly Property Rear As Boolean
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
