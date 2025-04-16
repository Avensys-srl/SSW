Imports ClimaLombarda.Common
Imports System.Reflection

Public MustInherit Class CLSSWInfo

    Public Shared ReadOnly Property SSWInfoClassType As Type
        Get
            Return Nothing
        End Get
    End Property

    Public Overridable ReadOnly Property ReleaseVersion As Version
        Get
            Return Assembly.GetAssembly(GetType(CLMainForm)).GetName().Version
        End Get
    End Property

    Public Overridable ReadOnly Property ReleaseDate As DateTime
        Get
            Return New CLAssemblyInfoEx(Reflection.Assembly.GetExecutingAssembly()).BuildDateTime
        End Get
    End Property

    Public MustOverride ReadOnly Property DefaultLanguage As String
    Public MustOverride ReadOnly Property SelectionSoftwareTitle As String
    Public MustOverride ReadOnly Property CustomerInfo As String
    Public MustOverride ReadOnly Property EmbeddedIcon As System.Drawing.Icon
    Public MustOverride ReadOnly Property Code As String

    Public MustOverride Sub PrepareEnvironment(environment As CLEnvironment)

    Protected Function GetResourceIcon(resourceName As String) As Icon
        Return New Icon(Assembly.GetEntryAssembly().GetManifestResourceStream(String.Format("SSW.Resources.{0}", resourceName)))
    End Function

End Class
