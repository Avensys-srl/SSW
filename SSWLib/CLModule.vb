Imports System
Imports Microsoft.VisualBasic
Imports System.Windows.Forms.DataVisualization.Charting
Imports Climalombarda.Common
Imports Climalombarda.DataCentral.LTModel

Public Module CLModule

    Public Const ChartSeries_OriginalCurve_Name As String = "OriginalCurve"
    Public Const ChartSeries_RegulationLevelCurve_Name As String = "RegulationLevelCurve"
    Public Const ChartSeries_WorkingPoint_Name As String = "WorkingPoint"
    Public Const ChartSeries_WorkingArea_Name As String = "WorkingArea"

    Public ChartSeries_OriginalCurve_Color As Color = Color.FromArgb(&HFF418CF0)
    Public ChartSeries_RegulationLevelCurve_Color As Color = Color.FromArgb(&HFFFCB441)
    Public ChartSeries_WorkingPoint_Color As Color = Color.FromArgb(&HFFE0400A)
    Public ChartSeries_WorkingArea_Color As Color = Color.FromArgb(104, 179, 215)

    Private ReadOnly Property Environment As CLEnvironment
        Get
            Return CLEnvironment.Current
        End Get
    End Property

    Structure termo
        Dim Fresh_inlet_temp As Double
        Dim Fresh_inlet_rh As Double
        Dim Return_inlet_temp As Double
        Dim Return_inlet_rh As Double
        Dim Supply_outlet_temp As Double
        Dim Supply_outlet_rh As Double
        Dim Exhaust_outlet_temp As Double
        Dim Exhaust_outlet_rh As Double
        Dim efficiency As Double
        Dim heat_recovery As Double
        Dim water_produced As Double
        Dim sensible_heat As Double
        Dim latent_heat As Double
    End Structure

    Structure psychro
        Dim pwsat As Double
        Dim pw As Double
        Dim w As Double
        Dim wsat As Double
        Dim u As Double
        Dim v As Double
        Dim h As Double
        Dim tdew As Double
        Dim twb As Double
        Dim rh As Double
    End Structure

    Structure FanERP2018
        Dim FanP As Double
        Dim FanW As Double
        Dim FanEta As Double
        Dim DeltaP As Double
        Dim E As Double
        Dim L2018 As Double
        Dim SFPint As Double
        Dim Delta As Double
    End Structure

    Public Enum CLMeasureUnit
        IP
        SI
    End Enum

    Dim cpa, rhoa, pr, T0, cpm, rhom, hc As Double
    Dim ztin, ztout, ztstd, vtin, vtout, vtstd, vmed As Double
    Dim ratiov, aftin, aftout, afstd, afmed, ratioaf, ratiopl As Double
    Dim mass_dry_air, psatin, ppartin, tdew_in, mass_moisture, volume_moisture, mda, mm, massin, cpwetin, mcp_wet, mcp_dry, ratio_mcp As Double
    Dim mass_dry_air_out, psatout, ppartout, tdew_out, massout, cpwetout, mcp_wet_out, mda_out, mm_out, mf, vf, ef As Double
    Dim thot, tcold, tdew, Mdamphot, mldryhot, mass_moisture_in, volume_moisture_in, mass_moisture_out, volume_moisture_out, average_moisture, ratio_moist As Double
    Dim rsens, n, eff34, T4, Q, T2, cond, psat2, rh2, mm2, Qsens, Qlat, dQ, T2new, ts, eff, Mdampcold, mldrycold, psat4, rh4, mm4, pd, hr, mcond As Double

    Function termo_calc(ByVal tin As Double,
      ByVal hrin As Double,
      ByVal tout As Double,
      ByVal hrout As Double,
      ByVal af As Double,
      ByVal model As String,
      ByVal pl As Double,
      onlyeff As Boolean) As termo

        Dim calcolato As termo

        'costanti
        cpa = 1006.45
        rhoa = 1.293
        pr = 101325
        T0 = 273.15
        cpm = 1859.74
        rhom = 0.8037
        hc = 2454111

        'Calcolo viscosità
        ztin = (T0 + tin - 1400) / 200
        ztout = (T0 + tout - 1400) / 200
        ztstd = (T0 + 20 - 1400) / 200
        vtin = (51.537354 + 4.858258 * ztin - 0.106514 * ztin ^ 2 - 0.002914 * ztin ^ 3 - 0.00163 * ztin ^ 4 + 0.000448 * ztin ^ 5) * 0.000001
        vtout = (51.537354 + 4.858258 * ztout - 0.106514 * ztout ^ 2 - 0.002914 * ztout ^ 3 - 0.00163 * ztout ^ 4 + 0.000448 * ztout ^ 5) * 0.000001
        vtstd = (51.537354 + 4.858258 * ztstd - 0.106514 * ztstd ^ 2 - 0.002914 * ztstd ^ 3 - 0.00163 * ztstd ^ 4 + 0.000448 * ztstd ^ 5) * 0.000001

        vmed = (vtin + vtout) / 2
        ratiov = vmed / vtstd

        'Calcolo Portate
        aftin = af
        aftout = af * (tout + T0) / (tin + T0)
        afstd = af * (20 + T0) / (tin + T0)

        afmed = (aftin + aftout) / 2

        ratioaf = afmed / afstd

        'Calcolo lunghezza
        ratiopl = (0.4 - 0.008) / (pl - 0.008)

        'Calcolo valori IN
        mass_dry_air = rhoa * af * (T0 / (tin + T0)) / 3600
        psatin = 100 * Math.Exp(17.433 - 19513.7 * Math.Exp(-1.27095 * Math.Log(tin + T0)))
        ppartin = hrin * psatin
        tdew_in = Math.Exp((1 / 1.27095) * Math.Log(19513.7 / (17.433 - Math.Log(ppartin / 100)))) - T0
        mass_moisture_in = mass_dry_air * (rhom / rhoa) * (ppartin) / (pr - (ppartin))

        volume_moisture_in = 3600 * mass_moisture_in / (rhom * (tin + T0) / T0)
        mda = mass_dry_air * af / (volume_moisture_in + af)
        mm = mass_moisture_in * af / (volume_moisture_in + af)
        massin = mm + mda

        cpwetin = cpa * mda / massin + cpm * mm / massin
        mcp_wet = cpwetin * massin
        mcp_dry = cpa * mass_dry_air

        ratio_mcp = mcp_wet / mcp_dry


        'Calcolo valori OUT
        mass_dry_air_out = mda

        psatout = 100 * Math.Exp(17.433 - 19513.7 * Math.Exp(-1.27095 * Math.Log(tout + T0)))
        ppartout = hrout * psatout
        tdew_out = Math.Exp((1 / 1.27095) * Math.Log(19513.7 / (17.433 - Math.Log(ppartout / 100)))) - T0
        mass_moisture_out = mass_dry_air_out * (rhom / rhoa) * (ppartout) / (pr - (ppartout))

        volume_moisture_out = 3600 * mass_moisture_out / (rhom * (tout + T0) / T0)

        massout = mass_moisture_out + mass_dry_air_out

        cpwetout = cpa * mass_dry_air_out / massout + cpm * mass_moisture_out / massout
        mcp_wet_out = cpwetout * massout

        mda_out = mass_dry_air_out * mcp_wet / mcp_wet_out
        mm_out = mass_moisture_out * mcp_wet / mcp_wet_out

        average_moisture = volume_moisture_in + volume_moisture_out
        ratio_moist = (af + average_moisture) / af

        mf = ratio_mcp * ratiopl * massin
        vf = af * ratiov * ratioaf * ratiopl * ratio_moist

        'Caratteristiche RS160
        If UCase(model) = "MODEL 1" Then
            ef = 0.97 - (6.10139 * mf ^ 3 - 4.01818 * mf ^ 2 + 1.77231 * mf) ' 0.97 - ....corretivo per evitare 100%

        End If
        'Caratteristiche RS300
        If UCase(model) = "MODEL 2" Then
            ef = 0.97 - (5.77875 * mf ^ 3 - 4.36345 * mf ^ 2 + 2.0758 * mf) ' 0.97 - ....corretivo per evitare 100%

        End If
        'Caratteristiche RS220
        If UCase(model) = "MODEL 3" Then
            mf = mf * 82 / 392
            vf = vf * 82 / 392
            ef = (0.0017748 * vf * vf - 0.455242 * vf + 100) / 100 - 0.03 ' -0.03  ....corretivo per evitare 100%

        End If

        'Caratteristiche NL180
        If UCase(model) = "MODEL NL180" Then
            ef = 1 - (-0.442211055 * mf ^ 2 + 0.954371859 * mf + 0.048143729)

        End If


        'Caratteristiche Rotativi
        If UCase(model).Contains("MODEL R") Then


            If UCase(model).Contains("500") Then
                eff34 = (0.000007 * af ^ 2 - 0.0235 * af + 89.6) / 100
            End If

            If UCase(model).Contains("600") Then
                eff34 = (0.000004 * af ^ 2 - 0.017 * af + 90.15) / 100
            End If

            If UCase(model).Contains("700") Then
                eff34 = (0.0000025 * af ^ 2 - 0.0132 * af + 90.56) / 100
            End If

            If UCase(model).Contains("1000") Then
                eff34 = (0.0000004 * af ^ 2 - 0.0053 * af + 89.351) / 100
            End If

            If UCase(model).Contains("1200") Then
                eff34 = (0.0000002 * af ^ 2 - 0.0039 * af + 92.253) / 100
            End If

            If UCase(model).Contains("1300") Then
                eff34 = (-0.00248514 * af + 91.892) / 100
            End If

            If UCase(model).Contains("1316") Then
                eff34 = (-0.00140114 * af + 91.621) / 100
            End If

            If UCase(model).Contains("1700") Then
                eff34 = (-0.00140114 * af + 91.621) / 100
            End If


        ElseIf UCase(model) = "MODEL EN366" Then  'Caso entalpico
            eff34 = (-0.046 * af + 84.15) / 100

        Else

            'Identifico quali sono il ramo caldo e ramo freddo, per procedere con il calcolo
            'che consisterà nel convergere sul ramo che da caldo si raffredda (indipendentemente dal
            'fatto che si chiami Exhaust o Supply
            If tin > tout Then
                thot = tin
                tcold = tout
                tdew = tdew_in
                Mdamphot = mm
                mldryhot = mda
                Mdampcold = mm_out
                mldrycold = mda_out
            Else
                thot = tout
                tcold = tin
                tdew = tdew_out
                Mdamphot = mm_out
                mldryhot = mda_out
                Mdampcold = mm
                mldrycold = mda
            End If

            rsens = 1 'valore iniziale del rapporto tra calore sensibile e totale (1 = no consensazione, no calore latente)
            Dim stepsCounter As Int32 = 0
            Dim qPrevious As Double = 0
            Do

                eff34 = ef + (1.588 * (1 - rsens) - 0.5793 * (1 - rsens) * (1 - rsens)) * (1 - ef)

                T4 = tcold + eff34 * (thot - tcold)
                Q = (T4 - tcold) * mcp_wet

                If stepsCounter = 0 Then
                    T2 = thot - rsens * (Q / mcp_wet)
                Else
                    T2 = T2new - (Q - qPrevious) / (7 * mcp_wet)
                End If

                psat2 = 100 * Math.Exp(17.433 - 19513.7 * Math.Exp(-1.27095 * Math.Log(T2 + T0)))

                If T2 < tdew Then  'Controllo se sono sotto la temperatura di condensa e quindi decido il valore dell'umidità relativa
                    rh2 = 1
                Else
                    rh2 = ((Mdamphot * rhoa * pr) / (rhom * mldryhot)) / (psat2 * (1 + (Mdamphot * rhoa) / (rhom * mldryhot)))
                End If

                mm2 = mldryhot * (rhom / rhoa) * (psat2 * rh2) / (pr - (psat2 * rh2))
                Qsens = (thot - T2) * mcp_wet
                Qlat = hc * (Mdamphot - mm2)
                dQ = Q - Qsens - Qlat
                T2new = tdew - ((Q - (thot - tdew) * mcp_wet) / (Qsens + Qlat - (thot - tdew) * mcp_wet)) * (tdew - T2)
                rsens = Qsens / (Qsens + Qlat)

                stepsCounter += 1
                qPrevious = Q

            Loop While (Math.Abs(dQ) > 0.001) 'Fintanto che la condizione è VERA = livello di precisione della convergenza

            'Compleatamento dello stato termodinamico Punto 4
            psat4 = 100 * Math.Exp(17.433 - 19513.7 * Math.Exp(-1.27095 * Math.Log(T4 + T0)))
            rh4 = ((Mdampcold * rhoa * pr) / (rhom * mldrycold)) / (psat4 * (1 + (Mdampcold * rhoa) / (rhom * mldrycold)))
            mm4 = mldrycold * (rhom / rhoa) * (psat4 * rh4) / (pr - (psat4 * rh4))

            'Devo riassegnare le temperature ai rami corretti
            If tin > tout Then
                calcolato.Supply_outlet_temp = T4
                calcolato.Supply_outlet_rh = rh4
                calcolato.Exhaust_outlet_temp = T2
                calcolato.Exhaust_outlet_rh = rh2
                calcolato.water_produced = 3600 * (mm - mm2)
            Else
                calcolato.Supply_outlet_temp = T2
                calcolato.Supply_outlet_rh = rh2
                calcolato.Exhaust_outlet_temp = T4
                calcolato.Exhaust_outlet_rh = rh4
                calcolato.water_produced = 3600 * (mm_out - mm2)
            End If



        End If


        'Se sono MODEL R devo cambiare il calcolo (dopo che l'efficienza è stata calcolata perchè dipende tutto da qui)
        If (UCase(model).Contains("MODEL R") Or UCase(model).Contains("MODEL EN366")) And onlyeff = False Then

            Dim psychro_r As psychro
            Dim psychro_f As psychro
            Dim psychro_s As psychro
            Dim psychro_e As psychro
            Dim eff_hr As Double = 1.035 * eff34
            Dim tsup, texh As Double
            Dim wsup, wexh As Double
            Dim tf, tr, hrf, hrr As Double
            'portata massica
            Dim rho As Double = 1.2
            Dim m As Double = rho * af / 3600

            'riposizione lo temperature
            If tin > tout Then
                tr = tin
                tf = tout
                hrr = hrin
                hrf = hrout
            Else
                tr = tout
                tf = tin
                hrr = hrout
                hrf = hrin
            End If


            'punti termodinamici noti
            psychro_r = PsychroCalc(tr, hrr)
            psychro_f = PsychroCalc(tf, hrf)

            'temperatura uscita 1
            tsup = tf + eff34 * (tr - tf)
            wsup = psychro_f.w + eff_hr * (psychro_r.w - psychro_f.w)
            psychro_s = PsychroCalcW(tsup, wsup)

            'temperatura uscita 1
            wexh = psychro_r.w - (wsup - psychro_f.w)
            texh = tr - (tsup - tf)
            psychro_e = PsychroCalcW(texh, wexh)

            Q = m * (psychro_s.h - psychro_f.h) * 1000
            Qsens = m * 1005 * (tsup - tf)
            Qlat = Q - Qsens
            calcolato.water_produced = (wsup - psychro_f.w) * m * 3600


            If tin > tout Then
                calcolato.Supply_outlet_temp = tsup
                calcolato.Supply_outlet_rh = psychro_s.rh
                calcolato.Exhaust_outlet_temp = texh
                calcolato.Exhaust_outlet_rh = psychro_e.rh
            Else
                calcolato.Supply_outlet_temp = texh
                calcolato.Supply_outlet_rh = psychro_e.rh
                calcolato.Exhaust_outlet_temp = tsup
                calcolato.Exhaust_outlet_rh = psychro_s.rh
            End If


        End If

        calcolato.Fresh_inlet_temp = tin
        calcolato.Fresh_inlet_rh = hrin
        calcolato.Return_inlet_temp = tout
        calcolato.Return_inlet_rh = hrout
        calcolato.efficiency = eff34
        calcolato.heat_recovery = Q
        calcolato.sensible_heat = Qsens
        calcolato.latent_heat = Qlat

        Return calcolato

    End Function

    Function GetMaxValue(values As Double()) As Double

        Dim maxValue As Double = 0

        For Each value As Double In values
            If maxValue < value Then
                maxValue = value
            End If
        Next

        Return maxValue
    End Function

    Function ERP2018_calculation(ByVal af As Double, ByVal eff As Double, ByVal esp As Double, ByVal wunit As Double, ByVal dcHeatRecoveryModel As CLDCHeatRecoveryModel) As FanERP2018
        Dim MotorType As String
        Dim MotorNumbers As Int16
        Dim calc As FanERP2018

        MotorType = dcHeatRecoveryModel.MotorType
        MotorNumbers = dcHeatRecoveryModel.MotorsNumbers / 2 'per singolo ramo


        'Iterpolate motori AF vs P con aF i
        '133  y = -4E-06x3 + 0,0011x2 - 0,764x + 477,46
        '160  y = -0.0003x2 - 0.3514x + 439.54
        '220  y = -0.0001x2 + 0.1289x + 133.3
        '315  y = -6E-05x2 - 0.0233x + 760.87


        If MotorType.ToString.CompareTo("133") = 0 Then
            calc.FanP = -0.000004 * (af * 3600 / MotorNumbers) ^ 3 + 0.0011 * (af * 3600 / MotorNumbers) ^ 2 - 0.764 * (af * 3600 / MotorNumbers) + 477.46
        ElseIf MotorType.ToString.CompareTo("160") = 0 Then
            calc.FanP = -0.0003 * (af * 3600 / MotorNumbers) ^ 2 - 0.3514 * (af * 3600 / MotorNumbers) + 439.54
        ElseIf MotorType.ToString.CompareTo("220") = 0 Then
            calc.FanP = -0.0001 * (af * 3600 / MotorNumbers) ^ 2 + 0.1289 * (af * 3600 / MotorNumbers) + 133.3
        ElseIf (dcHeatRecoveryModel.Code.CompareTo("CLRC 073 OSC") = 0 Or dcHeatRecoveryModel.Code.CompareTo("CLRC 073 SSC") = 0) Then
            calc.FanP = (-0.00006 * (af * 3600 / MotorNumbers) ^ 2 - 0.0233 * (af * 3600 / MotorNumbers) + 760.87) * 0.63
        ElseIf (dcHeatRecoveryModel.Code.CompareTo("CLRC 093 OSC") = 0 Or dcHeatRecoveryModel.Code.CompareTo("CLRC 093 SSC") = 0) Then
            calc.FanP = (-0.00006 * (af * 3600 / MotorNumbers) ^ 2 - 0.0233 * (af * 3600 / MotorNumbers) + 760.87) * 0.58
        ElseIf (dcHeatRecoveryModel.Code.CompareTo("CLRC 223 SSC") = 0 Or dcHeatRecoveryModel.Code.CompareTo("CLRC 223 OSC") = 0) Then
            calc.FanP = (-0.00006 * (af * 3600 / MotorNumbers) ^ 2 - 0.0233 * (af * 3600 / MotorNumbers) + 760.87) * 0.83
        ElseIf MotorType.ToString.CompareTo("315") = 0 Then
            calc.FanP = (-0.00006 * (af * 3600 / MotorNumbers) ^ 2 - 0.0233 * (af * 3600 / MotorNumbers) + 760.87) * 0.88
        ElseIf MotorType.ToString.CompareTo("250") = 0 Then
            calc.FanP = 0
        Else
            calc.FanP = 0
        End If

        calc.FanW = wunit - 3.5 'potenza complessiva di un ramo (solo motori)
        calc.FanEta = af * calc.FanP / calc.FanW 'efficienza del ramo

        calc.DeltaP = calc.FanP - esp  'caduta di pressione
        calc.E = (eff / 100 - 0.73) * 3000  ' parametro E della norma
        calc.L2018 = 1100 + calc.E - 300 * af / 2 ' Calcolo limite ERP2018
        calc.SFPint = 2 * calc.DeltaP / calc.FanEta * (wunit) / calc.FanW ' calcolo SFP interno
        calc.Delta = calc.L2018 - calc.SFPint  ' Verifico la differenza, se positiva è un punto di lavoro valido

        Return calc

    End Function


    Function curva(ByVal measureUnit As CLMeasureUnit,
    ByVal af As Double,
    ByVal dcHeatRecoveryModel As CLDCHeatRecoveryModel,
    ByVal tin As Double,
    ByVal hrin As Double,
    ByVal tout As Double,
    ByVal hrout As Double,
    ByVal reg As Integer,
    ByVal chart1 As Chart,
    ByVal chart2 As Chart,
    ByVal chart3 As Chart,
    ByVal press As Double,
    ByVal showSFPArea As Boolean,
    ByVal showERPArea As Boolean,
    ByVal sfpLimit As Double,
    ByVal showPassiveHausArea As Boolean,
    ByVal passiveHausLimit As Double) As Double()

        Dim j As Integer = 0
        Dim x(5), y(5), z(5), workpoint(3) As Double
        Dim x_new() As Double = New Double() {}
        Dim y_new() As Double = New Double() {}
        Dim z_new() As Double = New Double() {}
        Dim e() As Double = New Double() {}
        Dim af_ref As Double

        Dim tempin, humrin, tempout, humrout, recup As Double
        Dim tipo As String
        Dim currentSeries As Series

        If measureUnit = CLMeasureUnit.IP Then
            af_ref = af * 3.6
        Else
            af_ref = af
        End If


        tempin = tin
        humrin = hrin
        tempout = tout
        humrout = hrout
        recup = 1
        tipo = "MODEL 1"

        'Matching dati scelti  - database
        x = dcHeatRecoveryModel.AirflowsItems
        y = dcHeatRecoveryModel.PressuresItems
        z = dcHeatRecoveryModel.PowersItems
        recup = dcHeatRecoveryModel.LenRec
        tipo = dcHeatRecoveryModel.ModRec

        For i As Integer = 0 To z.Length - 1
            z(i) = z(i) - 3.5
        Next

        'Calcolo livello di interpolazione per discretizzare meglio il calcolo
        If Math.Floor(x.Max) >= 5000 Then
            ReDim x_new(Math.Floor(x.Max / 50) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 50 * i
            Next i
        ElseIf Math.Floor(x.Max) >= 2000 And Math.Floor(x.Max) < 5000 Then
            ReDim x_new(Math.Floor(x.Max / 25) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 25 * i
            Next i
        ElseIf Math.Floor(x.Max) >= 1000 And Math.Floor(x.Max) < 2000 Then
            ReDim x_new(Math.Floor(x.Max / 15) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 15 * i
            Next i
        ElseIf Math.Floor(x.Max) >= 500 And Math.Floor(x.Max) < 1000 Then
            ReDim x_new(Math.Floor(x.Max / 10) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 10 * i
            Next i
        Else
            ReDim x_new(Math.Floor(x.Max / 1) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 1 * i
            Next i
        End If

        alglib.spline1dconvcubic(x, y, x_new, y_new)
        alglib.spline1dconvcubic(x, z, x_new, z_new)



        Dim x_new_ori As Double()
        Dim y_new_ori As Double()
        Dim z_new_ori As Double()

        x_new_ori = x_new.Clone()
        y_new_ori = y_new.Clone()
        z_new_ori = z_new.Clone()

        'mainForm.regbar.Visible = True
        'mainForm.HScrollBar1.Visible = True

        'Calcolo curve con regolazione

        ' And mainForm.regbar.Visible = True
        If (reg <> 100) Then

            'Dim dpreg, powerfact As Double
            'Dim index As Integer

            'index = -1

            ''Calcolo i coefficienti di regolazione
            'If dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_CLRC_13_OSC _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_CLRC_23) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_CLRC_33) _
            '    OrElse dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_QUANTUM_25 _
            '    OrElse dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_QUANTUM_35 _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_77_TSC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_77_HOC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_127_TSC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_127_HOC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_127_FS) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_47_CDR) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_47_CFD) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_47_TSC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_47_HOC) Then

            '    dpreg = 0.9 * (-0.0006 * reg ^ 3 + 0.0736 * reg ^ 2 - 4.3473 * reg + 287.27)
            '    If dpreg < 0 Then
            '        dpreg = Math.Abs(dpreg / 2)
            '    End If
            '    powerfact = 0.0429 * Math.Exp(0.0316 * reg)
            'ElseIf dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_CLRC_13_SSC Then

            '    dpreg = 0.7 * (-0.0006 * reg ^ 3 + 0.0736 * reg ^ 2 - 4.3473 * reg + 287.27)
            '    If dpreg < 0 Then
            '        dpreg = Math.Abs(dpreg / 2)
            '    End If
            '    powerfact = 0.0429 * Math.Exp(0.0316 * reg)
            'ElseIf dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_CLRC_43) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_CLRC_53) _
            '    OrElse dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_QUANTUM_45 _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_77_CDR) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_77_CFD) _
            'Then

            '    dpreg = 0.9 * (-0.0012 * reg ^ 3 + 0.1213 * reg ^ 2 - 5.1555 * reg + 498.53)
            '    powerfact = 0.0429 * Math.Exp(0.0316 * reg)
            'Else
            '    dpreg = 0.9 * (-0.0013 * reg ^ 3 + 0.1673 * reg ^ 2 - 9.8802 * reg + 835.88) '652.88 originale
            '    powerfact = 0.0429 * Math.Exp(0.0316 * reg)
            'End If

            'For i As Integer = 0 To (x_new.Length - 1)
            '    y_new(i) = y_new(i) - dpreg
            '    If (y_new(i) <= 0 And index = -1) Then
            '        index = i - 1
            '    End If
            '    z_new(i) = z_new(i) * powerfact
            'Next i

            ''Riadatto le dimensioni dei vettori da disegnare per rendere il grafico più leggibile
            '' e copio i valori ricalcolati.
            'If index > -1 Then
            '    ReDim Preserve x_new(index)
            '    ReDim Preserve y_new(index)
            '    ReDim Preserve z_new(index)
            'End If

            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = x_new(i) * (reg / 100)
                y_new(i) = y_new(i) * (reg / 100) ^ 2
                z_new(i) = z_new(i) * (reg / 100) ^ 3
            Next i

        End If

        'Ricalcolo curva di efficienza
        ReDim e(x_new_ori.Length - 1)
        For i As Integer = 0 To x_new_ori.Length - 1
            Dim temp_termo As termo
            temp_termo = termo_calc(tempin, humrin, tempout, humrout, x_new_ori(i), tipo, recup, 0)
            e(i) = 100 * temp_termo.efficiency
        Next i

        'Calcolo il punto di lavoro richiesto, se non fattibile mi metto a AF_max

        Dim idmax, idmin As Integer
        Dim xcalc As Double

        idmax = 0
        idmin = 0
        xcalc = af_ref

        For i As Integer = 0 To (x_new.Length - 1)
            If (x_new(i) >= af_ref And idmax = 0) Then
                idmax = i
            ElseIf (idmax = 0 And i = x_new.Length - 1) Then
                idmax = i
                xcalc = x_new(idmax)
            End If
        Next i

        workpoint(1) = xcalc

        If y_new(idmax) < 0 Then
            workpoint(2) = 0
        ElseIf idmax < (x_new.Length - 1) Then
            Dim k As Double
            k = (y_new(idmax + 1) - y_new(idmax)) / (x_new(idmax + 1) - x_new(idmax))
            workpoint(2) = k * (workpoint(1) - x_new(idmax)) + y_new(idmax)
        Else
            workpoint(2) = y_new(idmax)
        End If


        workpoint(3) = z_new(idmax)




        'Ridisegno i Grafici
        Try
            Dim xArea1 As New List(Of Double)
            Dim yArea1 As New List(Of Double)

            If measureUnit = CLMeasureUnit.IP Then
                For counter As Integer = 0 To (x_new_ori.Length - 1)
                    x_new_ori(counter) = x_new_ori(counter) / 3.6
                Next
                For i As Integer = 0 To (x_new.Length - 1)
                    x_new(i) = x_new(i) / 3.6
                Next i
            End If

            If showSFPArea Then
                Dim sfp As Double

                For counter As Integer = 1 To (x_new.Length - 1)
                    sfp = ((2 * z_new(counter)) * 3.6) / x_new(counter)
                    If sfp <= sfpLimit Then
                        xArea1.Add(x_new(counter))
                        yArea1.Add(y_new(counter))
                    End If
                Next
            ElseIf showPassiveHausArea Then
                Dim passiveHaus As Double

                For counter As Integer = 1 To (x_new.Length - 1)
                    passiveHaus = (z_new(counter) * 2) / x_new(counter)
                    If passiveHaus <= passiveHausLimit Then
                        xArea1.Add(x_new(counter))
                        yArea1.Add(y_new(counter))
                    End If
                Next
            ElseIf showERPArea Then
                Dim erpfan As FanERP2018
                For counter As Integer = 1 To (x_new.Length - 1)
                    erpfan = ERP2018_calculation(x_new(counter) / 3600, e(counter), y_new(counter), z_new(counter), dcHeatRecoveryModel)
                    If erpfan.Delta > 0 Then
                        xArea1.Add(x_new(counter))
                        yArea1.Add(y_new(counter))
                    End If
                Next

            End If

            chart1.Series.Clear()

            ' Serie - curva originale
            currentSeries = chart1.Series.Add(ChartSeries_OriginalCurve_Name)
            currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Spline
            currentSeries.Points.DataBindXY(x_new_ori, y_new_ori)
            currentSeries.MarkerSize = 10
            currentSeries.BorderWidth = 3
            currentSeries.Color = ChartSeries_OriginalCurve_Color

            ' Serie - area di lavoro
            If showSFPArea Or showPassiveHausArea Or showERPArea Then
                currentSeries = chart1.Series.Add(ChartSeries_WorkingArea_Name)
                currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.SplineArea
                'currentSeries.Color = Color.FromArgb(104, 179, 215)
                currentSeries.Points.DataBindXY(xArea1.ToArray(), yArea1.ToArray())
                currentSeries.Color = ChartSeries_WorkingArea_Color
            End If

            ' Serie - curva con regulation level
            currentSeries = chart1.Series.Add(ChartSeries_RegulationLevelCurve_Name)
            currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Spline
            currentSeries.Points.DataBindXY(x_new, y_new)
            currentSeries.BorderWidth = 3
            currentSeries.Color = ChartSeries_RegulationLevelCurve_Color

            ' Serie - punto di lavoro
            currentSeries = chart1.Series.Add(ChartSeries_WorkingPoint_Name)
            currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Point
            currentSeries.Points.DataBindXY(New Double() {workpoint(1)}, New Double() {workpoint(2)})
            currentSeries.MarkerSize = 10
            currentSeries.Color = ChartSeries_WorkingPoint_Color

            'chart1.ApplyPaletteColors()
            chart1.ChartAreas(0).AxisX.Minimum = 0
            chart1.ChartAreas(0).AxisY.Minimum = 0

            If measureUnit = CLMeasureUnit.SI Then
                chart1.ChartAreas(0).AxisY.Title = Environment.Localization.GetString(CLMessageResources.MainForm_Pressure.ToString()) & " [Pa]"
                chart1.ChartAreas(0).AxisX.Title = Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()) & " [m3/h]"
            Else
                chart1.ChartAreas(0).AxisY.Title = Environment.Localization.GetString(CLMessageResources.MainForm_Pressure.ToString()) & " [Pa]"
                chart1.ChartAreas(0).AxisX.Title = Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()) & " [l/s]"
            End If

        Catch argEx As ArgumentException
            MsgBox(argEx.Message)
        End Try

        Try

            chart2.Series.Clear()
            ' Serie - Curva originale
            currentSeries = chart2.Series.Add(ChartSeries_OriginalCurve_Name)
            currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Spline
            currentSeries.Points.DataBindXY(x_new_ori, z_new_ori)
            currentSeries.BorderWidth = 3
            currentSeries.Color = ChartSeries_OriginalCurve_Color

            ' Serie - Curva con regulation level
            'currentSeries = chart2.Series.Add(ChartSeries_RegulationLevelCurve_Name)
            'currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Spline
            'currentSeries.Points.DataBindXY(x_new, z_new)
            'currentSeries.BorderWidth = 3
            'currentSeries.Color = ChartSeries_RegulationLevelCurve_Color

            ' Serie - Punto di lavoro
            currentSeries = chart2.Series.Add(ChartSeries_WorkingPoint_Name)
            currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Point
            currentSeries.Points.DataBindXY(New Double() {workpoint(1)}, New Double() {workpoint(3)})
            currentSeries.MarkerSize = 10
            currentSeries.Color = ChartSeries_WorkingPoint_Color

            chart2.ChartAreas(0).AxisX.Minimum = 0
            chart2.ChartAreas(0).AxisY.Minimum = 0

            chart2.Series.Remove(chart2.Series(ChartSeries_OriginalCurve_Name))

            chart2.ChartAreas(0).AxisX.Maximum = Math.Ceiling(x_new_ori.Max() / 4) * 4
            chart2.ChartAreas(0).AxisY.Maximum = Math.Ceiling(z_new_ori.Max() / 4) * 4

            chart2.ChartAreas(0).AxisX.Interval = Math.Ceiling(x_new_ori.Max() / 4)
            chart2.ChartAreas(0).AxisY.Interval = Math.Ceiling(z_new_ori.Max() / 4)

            chart2.ChartAreas(0).AxisY.Title = Environment.Localization.GetString(CLMessageResources.MainForm_PowerSupply.ToString()) & " [W]"
            If measureUnit = CLMeasureUnit.SI Then
                chart2.ChartAreas(0).AxisX.Title = Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()) & " [m3/h]"
            Else
                chart2.ChartAreas(0).AxisX.Title = Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()) & " [l/s]"
            End If

        Catch argEx As ArgumentException
            MsgBox(argEx.Message)
        End Try

        Try
            chart3.Series.Clear()

            ' Serie - Curva originale
            currentSeries = chart3.Series.Add(ChartSeries_OriginalCurve_Name)
            currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Spline
            currentSeries.Points.DataBindXY(x_new_ori, e)
            currentSeries.BorderWidth = 3
            currentSeries.Color = ChartSeries_OriginalCurve_Color

            ' Serie - Punto di lavoro
            currentSeries = chart3.Series.Add(ChartSeries_WorkingPoint_Name)
            currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Point
            currentSeries.Points.DataBindXY(New Double() {workpoint(1)}, New Double() {e(idmax)})
            currentSeries.MarkerSize = 10
            currentSeries.Color = ChartSeries_WorkingPoint_Color

            chart3.ChartAreas(0).AxisX.Minimum = 0
            chart3.ChartAreas(0).AxisY.Minimum = 0

            If measureUnit = CLMeasureUnit.SI Then
                chart3.ChartAreas(0).AxisY.Title = Environment.Localization.GetString(CLMessageResources.MainForm_Efficiency.ToString()) & " [%]"
                chart3.ChartAreas(0).AxisX.Title = Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()) & " [m3/h]"
            Else
                chart3.ChartAreas(0).AxisY.Title = Environment.Localization.GetString(CLMessageResources.MainForm_Efficiency.ToString()) & " [%]"
                chart3.ChartAreas(0).AxisX.Title = Environment.Localization.GetString(CLMessageResources.MainForm_AirFlow.ToString()) & " [l/s]"
            End If

        Catch argEx As ArgumentException
            MsgBox(argEx.Message)
        End Try

        If measureUnit = CLMeasureUnit.IP Then
            workpoint(1) = workpoint(1) / 3.6
        End If


        Return workpoint

    End Function

    Function maxflow(ByVal measureUnit As CLMeasureUnit,
        ByVal dcHeatRecoveryModel As CLDCHeatRecoveryModel,
        ByVal airflow As String) As String

        Dim af_ctrl, max As Double

        max = dcHeatRecoveryModel.AirflowsItems.Max()

        If measureUnit = CLMeasureUnit.IP Then
            af_ctrl = CDbl(airflow.Trim() * 3.6)
        Else
            af_ctrl = CDbl(airflow.Trim())
        End If

        If af_ctrl > max Then
            maxflow = CStr(max)
        Else
            maxflow = CStr(af_ctrl)
        End If

        If measureUnit = CLMeasureUnit.IP Then
            maxflow = CDbl(maxflow / 3.6)
        End If

    End Function

    Function punto(ByVal dcHeatRecoveryModel As CLDCHeatRecoveryModel,
        ByVal tin As Double,
        ByVal hrin As Double,
        ByVal tout As Double,
        ByVal hrout As Double,
        ByVal reg As Integer,
        ByVal af_ref As Double) As Double()

        Dim j As Integer = 0
        Dim x(5), y(5), z(5), workpoint(3) As Double
        Dim x_new() As Double = New Double() {}
        Dim y_new() As Double = New Double() {}
        Dim z_new() As Double = New Double() {}
        Dim e() As Double = New Double() {}

        Dim tempin, humrin, tempout, humrout, recup As Double
        Dim tipo As String

        tempin = tin
        humrin = hrin
        tempout = tout
        humrout = hrout
        recup = 1
        tipo = "MODEL 1"

        'Matching dati scelti  - database
        x = dcHeatRecoveryModel.AirflowsItems
        y = dcHeatRecoveryModel.PressuresItems
        z = dcHeatRecoveryModel.PowersItems
        recup = dcHeatRecoveryModel.LenRec
        tipo = dcHeatRecoveryModel.ModRec

        For i As Integer = 0 To z.Length - 1
            z(i) = z(i) - 3.5
        Next

        'Calcolo livello di interpolazione per discretizzare meglio il calcolo
        If Math.Floor(x.Max) >= 5000 Then
            ReDim x_new(Math.Floor(x.Max / 50) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 50 * i
            Next i
        ElseIf Math.Floor(x.Max) >= 2000 And Math.Floor(x.Max) < 5000 Then
            ReDim x_new(Math.Floor(x.Max / 25) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 25 * i
            Next i
        ElseIf Math.Floor(x.Max) >= 1000 And Math.Floor(x.Max) < 2000 Then
            ReDim x_new(Math.Floor(x.Max / 15) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 15 * i
            Next i
        ElseIf Math.Floor(x.Max) >= 500 And Math.Floor(x.Max) < 1000 Then
            ReDim x_new(Math.Floor(x.Max / 10) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 10 * i
            Next i
        Else
            ReDim x_new(Math.Floor(x.Max / 1) + 1)
            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = 1 * i
            Next i
        End If

        alglib.spline1dconvcubic(x, y, x_new, y_new)
        alglib.spline1dconvcubic(x, z, x_new, z_new)


        'Calcolo curve con regolazione

        If (reg <> 100) Then

            'Dim dpreg, powerfact As Double
            'Dim index As Integer

            'index = -1

            ''Calcolo i coefficienti di regolazione
            'If dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_CLRC_13_SSC _
            '    OrElse dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_CLRC_13_OSC _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_CLRC_23) _
            '    OrElse dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_QUANTUM_25 _
            '    OrElse dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_QUANTUM_35 _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_77_TSC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_77_HOC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_127_TSC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_127_HOC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_127_FS) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_47_CDR) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_47_CFD) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_47_TSC) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_47_HOC) _
            '    Then
            '    dpreg = 0.9 * (-0.0006 * reg ^ 3 + 0.0736 * reg ^ 2 - 4.3473 * reg + 287.27)
            '    powerfact = 0.0429 * Math.Exp(0.0316 * reg)
            'ElseIf dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_CLRC_43) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_CLRC_53) _
            '    OrElse dcHeatRecoveryModel.Code = CLEnvironment.ModelCode_QUANTUM_45 _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_77_CDR) _
            '    OrElse dcHeatRecoveryModel.Code.StartsWith(CLEnvironment.ModelCode_SG_77_CFD) _
            '    Then
            '    dpreg = 0.9 * (-0.0012 * reg ^ 3 + 0.1213 * reg ^ 2 - 5.1555 * reg + 498.53)
            '    powerfact = 0.0429 * Math.Exp(0.0316 * reg)
            'Else
            '    'dpreg = 0.9 * (-0.0013 * reg ^ 3 + 0.1673 * reg ^ 2 - 9.8802 * reg + 652.88)
            '    powerfact = 0.0429 * Math.Exp(0.0316 * reg)

            '    dpreg = reg * reg

            'End If



            'For i As Integer = 0 To (x_new.Length - 1)
            '    y_new(i) = y_new(i) * dpreg
            '    If (y_new(i) <= 0 And index = -1) Then
            '        index = i - 1
            '    End If
            '    z_new(i) = z_new(i) * powerfact
            'Next i

            'Riadatto le dimensioni dei vettori da disegnare per rendere il grafico più leggibile
            ' e copio i valori ricalcolati.
            'If index > -1 Then
            '    ReDim Preserve x_new(index)
            '    ReDim Preserve y_new(index)
            '    ReDim Preserve z_new(index)
            'End If

            For i As Integer = 0 To (x_new.Length - 1)
                x_new(i) = x_new(i) * (reg / 100)
                y_new(i) = y_new(i) * (reg / 100) ^ 2
                z_new(i) = z_new(i) * (reg / 100) ^ 3
            Next i

        End If

        'Ricalcolo curva di efficienza
        ReDim e(x_new.Length - 1)
        For i As Integer = 0 To x_new.Length - 1
            Dim temp_termo As termo
            temp_termo = termo_calc(tempin, humrin, tempout, humrout, x_new(i), tipo, recup, 1)
            e(i) = 100 * temp_termo.efficiency
        Next i

        'Calcolo il punto di lavoro richiesto, se non fattibile mi metto a AF_max

        Dim idmax, idmin As Integer
        Dim xcalc As Double

        idmax = 0
        idmin = 0
        xcalc = af_ref

        For i As Integer = 0 To (x_new.Length - 1)
            If (x_new(i) >= af_ref And idmax = 0) Then
                idmax = i
            ElseIf (idmax = 0 And i = x_new.Length - 1) Then
                idmax = i
                xcalc = x_new(idmax)
            End If
        Next i

        workpoint(0) = e(idmax)
        workpoint(1) = xcalc

        If y_new(idmax) < 0 Then
            workpoint(2) = 0
        ElseIf idmax < (x_new.Length - 1) Then
            Dim k As Double
            k = (y_new(idmax + 1) - y_new(idmax)) / (x_new(idmax + 1) - x_new(idmax))
            workpoint(2) = k * (workpoint(1) - x_new(idmax)) + y_new(idmax)
        Else
            workpoint(2) = y_new(idmax)
        End If

        workpoint(3) = z_new(idmax)

        Return workpoint

    End Function

    Function sap_calc(ByVal dcHeatRecoveryModel As CLDCHeatRecoveryModel,
        ByVal af_ref As Double) As Double()

        Dim value(3) As Double
        Dim calc As Double()
        Dim reg As Integer
        Dim pascal As Double
        Dim found As Boolean = False
        Dim idreg As Integer = 20
        Dim k As Double = 0.75

        Select Case Math.Floor(af_ref)
            Case 54
                pascal = 67 * k
            Case 75
                pascal = 85 * k
            Case 97
                pascal = 108 * k
            Case 118
                pascal = 131 * k
            Case 140
                pascal = 154 * k
            Case 162
                pascal = 177 * k
            Case 183
                pascal = 200 * k
            Case Else
                pascal = 0
        End Select


        For reg = 20 To 100
            calc = punto(dcHeatRecoveryModel, 5, 80 / 100, 25, 28 / 100, reg, af_ref)
            If (calc(2) >= pascal And found = False) Then
                idreg = reg
                found = True
            End If
        Next

        If found = True Then
            calc = punto(dcHeatRecoveryModel, 5, 80 / 100, 25, 28 / 100, idreg, af_ref)
        Else
            calc = {0, 1, 0, 3.5}
            idreg = 0
        End If


        value(0) = (2 * calc(3) - 7) * 3.6 / calc(1)  ' SFP
        value(1) = calc(0)                  ' EFF
        value(2) = idreg                    ' REG

        Return value

    End Function

#Region "====[ Sound Functions ]===="

    Function sound_power_correction(ByVal reg As Double, qref As Double, pref As Double, q1 As Double, p1 As Double, sound_value As Double) As Double

        Dim k1, k2, k3, corr_reg, b1, b2, corr_flow, new_sound As Double

        k1 = -0.0000508304
        k2 = 0.010028623
        k3 = 0.505101875
        b1 = 0.12782
        b2 = -7

        If reg = 100 Then
            corr_reg = 1
        Else
            corr_reg = k1 * reg ^ 2 + k2 * reg + k3

        End If

        corr_flow = b1 * Math.Log10(p1 / pref) + b2 * Math.Log10(q1 / qref)

        new_sound = corr_reg * sound_value - corr_flow

        Return new_sound

    End Function

    'Public Function Sound_Calculate(dcModel As CLDCHeatRecoveryModel,
    '      ByVal sound100Values() As Double,
    '      ByVal reg As Double,
    '      ByVal airflow As Double,
    '      ByVal pressure As Double) As Double()

    '    Dim soundValues As New List(Of Double)
    '    Dim soundValue As Double

    '    For itemsCounter = 0 To dcModel.SoundData_InletItems.Length - 1
    '        soundValue = Math.Round(sound_power_correction(reg, dcModel.AirflowsItems.Max(), dcModel.PressuresItems.Max(), IIf(airflow = 0, 1, airflow), IIf(pressure = 0, 1, pressure), sound100Values(itemsCounter)), 1)
    '        soundValues.Add(soundValue)
    '    Next

    '    ' Aggiunge il totale
    '    soundValues.Add(dcModel.SoundData_Inlet_Total.Value)

    '    Return soundValues.ToArray()
    'End Function

#End Region

#Region "==== [ CO2 Level ] ===="

    Function calc_volume_litri(ByVal w As Double, ByVal l As Double, ByVal h As Double) As Double

        Dim volume As Double = 0

        volume = w * l * h * 1000

        Return volume

    End Function

    Function calc_superf_m2(ByVal w As Double, ByVal l As Double) As Double

        Dim super As Double = 0

        super = w * l

        Return super

    End Function

    Function co2prodpersona(ByVal met As Double) As Double

        Dim co2 As Double = 0

        co2 = 0.00276 * 1.645 * met / (0.23 * 0.83 + 0.77) * 3600

        Return co2

    End Function

    Function airflowdemand(ByVal extco2 As Double, ByVal maxco2 As Double, ByVal persone As Double, co2pers As Double) As Double

        Dim airflow As Double = 0
        Dim qv As Double = 0

        qv = persone * co2pers / 3600  'l/s

        airflow = 1000000 * qv / (maxco2 - extco2)  'l/s

        Return airflow

    End Function

    Sub co2time(ByVal volume As Double,
                ByVal extco2 As Double,
                ByVal maxco2 As Double,
                ByVal af_demand As Double,
                ByVal personebreak As Double,
                ByVal personepresenti As Double,
                ByVal tempobreak As Double,
                ByVal tempopresenza As Double,
                ByVal chart1 As Chart)

        Dim ppmpersona As Double = 0
        Dim yco2 As Double() = New Double() {}
        Dim xtime As Double() = New Double() {}
        Dim i As Integer = 1
        Dim j As Integer = 0
        Dim periodo As Integer = 300  'minuti presi in esame
        Dim tempo_corrente As Boolean = 0
        Dim co2maxinter As Double = 0
        Dim currentSeries As Series

        ReDim xtime(periodo)
        ReDim yco2(periodo)

        For k As Integer = 0 To periodo
            xtime(k) = k
        Next

        'dimensionamento sul massimo delle persone presenti
        ppmpersona = (maxco2 - extco2) / Math.Max(personebreak, personepresenti)

        'all'istante iniziale il livello di co2 è uguale
        yco2(0) = extco2


        'calcolo della CO2 secondo gli intervalli temporali
        ' La struttura dell'intervallo di tempo è del tipo
        ' t = iA+jB con i >= j e t <= periodo

        For k As Integer = 1 To periodo

            'verifico in che intervallo mi trovo

            If xtime(k) <= i * tempopresenza + j * tempobreak AndAlso i > j Then
                tempo_corrente = 0
            Else
                If j < i Then
                    j = j + 1
                End If
                If xtime(k) <= i * tempopresenza + j * tempobreak AndAlso i = j Then
                    tempo_corrente = 1
                Else
                    tempo_corrente = 0
                    i = i + 1
                End If
            End If

            'calcolo quindi la co2 massima per ogni intervallo

            If tempo_corrente = 0 Then
                co2maxinter = extco2 + personepresenti * ppmpersona
            Else
                co2maxinter = extco2 + personebreak * ppmpersona
            End If

            yco2(k) = (co2maxinter - yco2(k - 1)) * (1 - Math.Exp(-1 * af_demand / volume * 60 * (xtime(k) - xtime(k - 1)))) + yco2(k - 1)

        Next

        ' trasformo il valore di xtime in ore per il grafico

        For k As Integer = 0 To periodo
            xtime(k) = xtime(k) / 60
        Next

        chart1.Series.Clear()

        currentSeries = chart1.Series.Add("CO2 Level")
        currentSeries.ChartType = DataVisualization.Charting.SeriesChartType.Spline
        currentSeries.Points.DataBindXY(xtime, yco2)
        currentSeries.BorderWidth = 3
        currentSeries.Color = ChartSeries_OriginalCurve_Color
        chart1.ChartAreas(0).AxisX.Minimum = 0
        chart1.ChartAreas(0).AxisY.Minimum = 0
        chart1.ChartAreas(0).AxisY.Maximum = 2000
        chart1.ChartAreas(0).AxisX.Maximum = periodo / 60
        chart1.ChartAreas(0).AxisX.MinorGrid.Enabled = True
        chart1.ChartAreas(0).AxisY.MinorGrid.Enabled = True
        chart1.ChartAreas(0).AxisX.MinorGrid.Enabled = True

    End Sub

#End Region


#Region "====[Psycometric Calculator]===="

    Function PsychroCalc(t As Double, rh As Double) As psychro

        Dim calcolato As psychro
        Dim p As Double = 101325 'J/kgK

        Dim tk As Double = t + 273.15

        Dim c1 As Double = -5674.5359
        Dim c2 As Double = 6.3925247
        Dim c3 As Double = -0.009677843
        Dim c4 As Double = 0.00000062215701
        Dim c5 As Double = 0.0000000020747825
        Dim c6 As Double = -0.0000000000009484024
        Dim c7 As Double = 4.1635019
        Dim c8 As Double = -5800.2206
        Dim c9 As Double = 1.3914993
        Dim c10 As Double = -0.048640239
        Dim c11 As Double = 0.000041764768
        Dim c12 As Double = -0.000000014452093
        Dim c13 As Double = 6.5459673
        Dim c14 As Double = 6.54
        Dim c15 As Double = 14.526
        Dim c16 As Double = 0.7389
        Dim c17 As Double = 0.09486
        Dim c18 As Double = 0.4569

        calcolato.rh = rh

        'calcolo la pressione vapore alla saturazione in Pascal

        If t < 0 Then

            calcolato.pwsat = Math.Exp(c1 / tk + c2 + c3 * tk + c4 * tk ^ 2 + c5 * tk ^ 3 + c6 * tk ^ 4 + c7 * Math.Log(tk))

        Else
            calcolato.pwsat = Math.Exp(c8 / tk + c9 + c10 * tk + c11 * tk ^ 2 + c12 * tk ^ 3 + c13 * Math.Log(tk))

        End If

        'calcolo la pressione vapore in Pascal

        calcolato.pw = rh * calcolato.pwsat

        'calcolo l'umidita' assoluta  kg/kg
        calcolato.w = 0.62198 * calcolato.pw / (101325 - calcolato.pw)

        'calcolo l'umidita' assoluta di saturazione in kg/kg
        calcolato.wsat = 0.62198 * calcolato.pwsat / (101325 - calcolato.pwsat)

        'calcolo  grado di saturazione

        calcolato.u = calcolato.w / calcolato.wsat

        'calcolo il volume specifico in m3/h

        calcolato.v = 0.2871 * tk * (1 + 1.6078 * calcolato.w) / p

        'calcolo l'entalpia

        calcolato.h = 1.006 * t + calcolato.w * (2501 + 1.805 * t)

        'calcolo la temperatura di rugiada
        Dim alpha As Double = Math.Log(calcolato.pw / 1000)

        If t > 0 Then

            calcolato.tdew = c14 + c15 * alpha + c16 * alpha ^ 2 + c17 * alpha ^ 3 + c18 * (calcolato.pw / 1000) ^ 0.1984

        Else
            calcolato.tdew = 6.09 + 12.608 * alpha + 0.4959 * alpha ^ 2

        End If

        ' calcolo della temperatura di bulbo umido

        Dim passo As Double = 0.001
        Dim err As Double
        Dim pwswet As Double
        Dim wswet As Double
        Dim tkwb As Double

        calcolato.twb = t


        Do
            calcolato.twb = calcolato.twb - passo
            tkwb = calcolato.twb + 273.15

            If calcolato.twb < 0 Then

                pwswet = Math.Exp(c1 / tkwb + c2 + c3 * tkwb + c4 * tkwb ^ 2 + c5 * tkwb ^ 3 + c6 * tkwb ^ 4 + c7 * Math.Log(tkwb))

            Else
                pwswet = Math.Exp(c8 / tkwb + c9 + c10 * tkwb + c11 * tkwb ^ 2 + c12 * tkwb ^ 3 + c13 * Math.Log(tkwb))

            End If


            wswet = 0.62198 * pwswet / (101325 - pwswet)

            err = calcolato.w - (((2501 - 2.381 * calcolato.twb) * wswet + 1.006 * (calcolato.twb - t)) / (2501 + 1.805 * t - 4.186 * calcolato.twb))


        Loop While err < 0.00003

        Return calcolato
    End Function

    Function PsychroCalcW(t As Double, w As Double) As psychro

        Dim calcolato As psychro
        Dim p As Double = 101325 'J/kgK

        Dim tk As Double = t + 273.15

        Dim c1 As Double = -5674.5359
        Dim c2 As Double = 6.3925247
        Dim c3 As Double = -0.009677843
        Dim c4 As Double = 0.00000062215701
        Dim c5 As Double = 0.0000000020747825
        Dim c6 As Double = -0.0000000000009484024
        Dim c7 As Double = 4.1635019
        Dim c8 As Double = -5800.2206
        Dim c9 As Double = 1.3914993
        Dim c10 As Double = -0.048640239
        Dim c11 As Double = 0.000041764768
        Dim c12 As Double = -0.000000014452093
        Dim c13 As Double = 6.5459673
        Dim c14 As Double = 6.54
        Dim c15 As Double = 14.526
        Dim c16 As Double = 0.7389
        Dim c17 As Double = 0.09486
        Dim c18 As Double = 0.4569

        calcolato.w = w

        'calcolo la pressione vapore alla saturazione in Pascal

        If t < 0 Then

            calcolato.pwsat = Math.Exp(c1 / tk + c2 + c3 * tk + c4 * tk ^ 2 + c5 * tk ^ 3 + c6 * tk ^ 4 + c7 * Math.Log(tk))

        Else
            calcolato.pwsat = Math.Exp(c8 / tk + c9 + c10 * tk + c11 * tk ^ 2 + c12 * tk ^ 3 + c13 * Math.Log(tk))

        End If

        'calcolo la pressione vapore in Pascal
		calcolato.pw = calcolato.w * 101325 / (0.62198 + calcolato.w)
		
		'calcolo l'umidita' relativa
        calcolato.rh = calcolato.pw / calcolato.pwsat

        'calcolo l'umidita' assoluta  kg/kg
        calcolato.w = 0.62198 * calcolato.pw / (101325 - calcolato.pw)

        'calcolo l'umidita' assoluta di saturazione in kg/kg
        calcolato.wsat = 0.62198 * calcolato.pwsat / (101325 - calcolato.pwsat)

        'calcolo  grado di saturazione

        calcolato.u = calcolato.w / calcolato.wsat

        'calcolo il volume specifico in m3/h

        calcolato.v = 0.2871 * tk * (1 + 1.6078 * calcolato.w) / p

        'calcolo l'entalpia

        calcolato.h = 1.006 * t + calcolato.w * (2501 + 1.805 * t)

        'calcolo la temperatura di rugiada
        Dim alpha As Double = Math.Log(calcolato.pw / 1000)

        If t > 0 Then

            calcolato.tdew = c14 + c15 * alpha + c16 * alpha ^ 2 + c17 * alpha ^ 3 + c18 * (calcolato.pw / 1000) ^ 0.1984

        Else
            calcolato.tdew = 6.09 + 12.608 * alpha + 0.4959 * alpha ^ 2

        End If

        ' calcolo della temperatura di bulbo umido

        Dim passo As Double = 0.001
        Dim err As Double
        Dim pwswet As Double
        Dim wswet As Double
        Dim tkwb As Double

        calcolato.twb = t


        Do
            calcolato.twb = calcolato.twb - passo
            tkwb = calcolato.twb + 273.15

            If calcolato.twb < 0 Then

                pwswet = Math.Exp(c1 / tkwb + c2 + c3 * tkwb + c4 * tkwb ^ 2 + c5 * tkwb ^ 3 + c6 * tkwb ^ 4 + c7 * Math.Log(tkwb))

            Else
                pwswet = Math.Exp(c8 / tkwb + c9 + c10 * tkwb + c11 * tkwb ^ 2 + c12 * tkwb ^ 3 + c13 * Math.Log(tkwb))

            End If


            wswet = 0.62198 * pwswet / (101325 - pwswet)

            err = calcolato.w - (((2501 - 2.381 * calcolato.twb) * wswet + 1.006 * (calcolato.twb - t)) / (2501 + 1.805 * t - 4.186 * calcolato.twb))


        Loop While err < 0.00003

        Return calcolato
    End Function
#End Region

End Module

