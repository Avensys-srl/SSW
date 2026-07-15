Imports System.Collections.Generic

Public Enum CLCoilHydraulicIssueCode
    InvalidCoolingTemperatures
    InvalidHeatingTemperatures
    LowWaterDeltaT
    CriticalWaterDeltaT
End Enum

Public NotInheritable Class CLCoilHydraulicIssue
    Public Property Code As CLCoilHydraulicIssueCode
    Public Property IsBlocking As Boolean
End Class

Public NotInheritable Class CLCoilHydraulicRules
    Public Const MinimumDeltaT As Double = 1.0
    Public Const CriticalDeltaT As Double = 3.0
    Public Const WarningDeltaT As Double = 5.0
    Public Const MaximumRecommendedWaterPressureDrop As Double = 40.0

    Private Sub New()
    End Sub

    Public Shared Function Evaluate(mode As CLCoilPerformanceMode,
        coolingIn As Double,
        coolingOut As Double,
        heatingIn As Double,
        heatingOut As Double) As List(Of CLCoilHydraulicIssue)

        Dim issues As New List(Of CLCoilHydraulicIssue)()
        If mode = CLCoilPerformanceMode.CWD OrElse mode = CLCoilPerformanceMode.HCD Then
            AddDeltaIssue(issues, coolingOut - coolingIn, CLCoilHydraulicIssueCode.InvalidCoolingTemperatures)
        End If
        If mode = CLCoilPerformanceMode.HWD OrElse mode = CLCoilPerformanceMode.HCD Then
            AddDeltaIssue(issues, heatingIn - heatingOut, CLCoilHydraulicIssueCode.InvalidHeatingTemperatures)
        End If
        Return issues
    End Function

    Private Shared Sub AddDeltaIssue(issues As List(Of CLCoilHydraulicIssue),
        deltaT As Double,
        invalidCode As CLCoilHydraulicIssueCode)

        If deltaT < MinimumDeltaT Then
            issues.Add(New CLCoilHydraulicIssue With {.Code = invalidCode, .IsBlocking = True})
        ElseIf deltaT < CriticalDeltaT Then
            issues.Add(New CLCoilHydraulicIssue With {.Code = CLCoilHydraulicIssueCode.CriticalWaterDeltaT})
        ElseIf deltaT < WarningDeltaT Then
            issues.Add(New CLCoilHydraulicIssue With {.Code = CLCoilHydraulicIssueCode.LowWaterDeltaT})
        End If
    End Sub
End Class
