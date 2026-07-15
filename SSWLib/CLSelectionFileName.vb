Imports System.Collections.Generic
Imports System.IO
Imports System.Text

Public NotInheritable Class CLSelectionFileName
    Private Const DefaultMaximumReferenceLength As Integer = 30

    Private Sub New()
    End Sub

    Public Shared Function SanitizeReference(value As String,
        Optional maximumLength As Integer = DefaultMaximumReferenceLength) As String

        If String.IsNullOrWhiteSpace(value) OrElse maximumLength <= 0 Then Return String.Empty

        Dim invalidCharacters As New HashSet(Of Char)(Path.GetInvalidFileNameChars())
        Dim result As New StringBuilder()
        Dim previousWasSeparator As Boolean = False

        For Each character As Char In value.Trim()
            Dim isSeparator As Boolean = Char.IsWhiteSpace(character) OrElse invalidCharacters.Contains(character)
            If isSeparator Then
                If result.Length > 0 AndAlso Not previousWasSeparator Then result.Append("_"c)
                previousWasSeparator = True
            Else
                result.Append(character)
                previousWasSeparator = False
            End If

            If result.Length >= maximumLength Then Exit For
        Next

        Return result.ToString().Trim("_"c, "."c)
    End Function

    Public Shared Function BuildSuggestedName(baseName As String, customerReference As String) As String
        Dim safeBaseName As String = If(String.IsNullOrWhiteSpace(baseName), "selection", baseName.Trim())
        Dim prefix As String = SanitizeReference(customerReference)
        Return If(String.IsNullOrEmpty(prefix), safeBaseName, prefix & "_" & safeBaseName)
    End Function
End Class
