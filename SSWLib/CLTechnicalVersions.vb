Imports System.Reflection

Public NotInheritable Class CLTechnicalVersions

    Private Sub New()
    End Sub

    Public Const CurrentDatabaseSchemaVersion As Integer = 5
    Public Const CurrentSelectionFormatVersion As Integer = 2
    Public Const CurrentReportTemplateVersion As Integer = 4
    Public Const CurrentApiContractVersion As Integer = 1

    Public Shared ReadOnly Property SoftwareVersion As Version
        Get
            Return Assembly.GetExecutingAssembly().GetName().Version
        End Get
    End Property

    Public Shared ReadOnly Property CalculationEngineVersion As Version
        Get
            Return Assembly.GetAssembly(GetType(CLModule)).GetName().Version
        End Get
    End Property

End Class
