Imports System.Globalization
Imports System.Net
Imports System.Text

Public NotInheritable Class CLMultiSelectionEmailComposer
    Private Sub New()
    End Sub

    Public Shared Function BuildSubject(project As CLMultiSelectionProjectDocument) As String
        Dim text As CLMultiSelectionEmailText = CLMultiSelectionEmailText.ForLanguage(project.LanguageCode)
        Return String.Format(CultureInfo.CurrentCulture, text.Subject, project.Reference)
    End Function

    Public Shared Function BuildHtml(project As CLMultiSelectionProjectDocument) As String
        If project Is Nothing Then Throw New ArgumentNullException(NameOf(project))
        Dim text As CLMultiSelectionEmailText = CLMultiSelectionEmailText.ForLanguage(project.LanguageCode)
        Dim culture As CultureInfo = ProjectCulture(project.LanguageCode)
        Dim builder As New StringBuilder()
        builder.Append("<html><body style='font-family:Segoe UI,Arial,sans-serif;font-size:10.5pt;color:#172033'>")
        builder.Append("<p>").Append(H(text.Greeting)).Append("</p>")
        builder.Append("<p>").Append(H(text.Thanks)).Append("</p>")
        builder.Append("<p><strong>").Append(H(text.Project)).Append(":</strong> ").Append(H(project.Reference)).Append("</p>")
        builder.Append("<p>").Append(H(text.Intro)).Append("</p>")
        builder.Append("<table cellspacing='0' cellpadding='6' style='border-collapse:collapse;border:1px solid #8d96a3'>")
        builder.Append("<tr style='background:#e7eaee'>")
        For Each heading As String In New String() {text.Reference, text.Unit, text.Airflow, text.Pressure, text.Pdf}
            builder.Append("<th style='border:1px solid #8d96a3;text-align:left'>").Append(H(heading)).Append("</th>")
        Next
        builder.Append("</tr>")
        For Each item As CLMultiSelectionProjectItem In project.Items
            builder.Append("<tr>")
            Cell(builder, If(String.IsNullOrWhiteSpace(item.CustomerReference), "-", item.CustomerReference))
            Cell(builder, item.UnitName)
            Cell(builder, FormatNumber(item.AirflowM3h, culture) & " m³/h")
            Cell(builder, FormatNumber(item.PressurePa, culture) & " Pa")
            Cell(builder, item.PdfFileName)
            builder.Append("</tr>")
        Next
        builder.Append("</table>")
        builder.Append("<p>").Append(H(text.Closing)).Append("</p>")
        builder.Append("<p>").Append(H(text.Regards)).Append("</p>")
        builder.Append("</body></html>")
        Return builder.ToString()
    End Function

    Private Shared Sub Cell(builder As StringBuilder, value As String)
        builder.Append("<td style='border:1px solid #c7ccd3'>").Append(H(value)).Append("</td>")
    End Sub

    Private Shared Function H(value As String) As String
        Return WebUtility.HtmlEncode(If(value, String.Empty))
    End Function

    Private Shared Function FormatNumber(value As Double?, culture As CultureInfo) As String
        Return If(value.HasValue, value.Value.ToString("0.##", culture), "-")
    End Function

    Private Shared Function ProjectCulture(languageCode As String) As CultureInfo
        Dim cultureName As String
        Select Case If(languageCode, "en").Trim().ToLowerInvariant()
            Case "bg" : cultureName = "bg-BG"
            Case "da" : cultureName = "da-DK"
            Case "de" : cultureName = "de-DE"
            Case "fr" : cultureName = "fr-FR"
            Case "hu" : cultureName = "hu-HU"
            Case "it" : cultureName = "it-IT"
            Case "nl" : cultureName = "nl-NL"
            Case "no" : cultureName = "nb-NO"
            Case "pl" : cultureName = "pl-PL"
            Case "ro" : cultureName = "ro-RO"
            Case "sl" : cultureName = "sl-SI"
            Case "sv" : cultureName = "sv-SE"
            Case "is" : cultureName = "is-IS"
            Case Else : cultureName = "en-GB"
        End Select
        Return CultureInfo.GetCultureInfo(cultureName)
    End Function
End Class

Friend NotInheritable Class CLMultiSelectionEmailText
    Public Property Subject As String
    Public Property Greeting As String
    Public Property Thanks As String
    Public Property Project As String
    Public Property Intro As String
    Public Property Reference As String
    Public Property Unit As String
    Public Property Airflow As String
    Public Property Pressure As String
    Public Property Pdf As String
    Public Property Closing As String
    Public Property Regards As String

    Public Shared Function ForLanguage(languageCode As String) As CLMultiSelectionEmailText
        Select Case If(languageCode, "en").Trim().ToLowerInvariant()
            Case "it"
                Return T("Progetto - {0}", "Buongiorno,", "Grazie per la vostra richiesta.", "Progetto di riferimento", "In allegato trasmettiamo i PDF relativi alle selezioni riportate nella tabella seguente.", "Riferimento", "Unità selezionata", "Portata", "Pressione", "File PDF", "Rimaniamo a disposizione per eventuali chiarimenti o approfondimenti tecnici.", "Cordiali saluti")
            Case "fr"
                Return T("Projet - {0}", "Bonjour,", "Nous vous remercions pour votre demande.", "Projet de référence", "Veuillez trouver en pièces jointes les PDF relatifs aux sélections indiquées dans le tableau suivant.", "Référence", "Unité sélectionnée", "Débit", "Pression", "Fichier PDF", "Nous restons à votre disposition pour tout renseignement ou complément technique.", "Cordialement")
            Case "de"
                Return T("Projekt - {0}", "Guten Tag,", "vielen Dank für Ihre Anfrage.", "Projektreferenz", "Anbei erhalten Sie die PDF-Dateien zu den in der folgenden Tabelle aufgeführten Auswahlen.", "Referenz", "Ausgewähltes Gerät", "Luftmenge", "Druck", "PDF-Datei", "Für Rückfragen oder weitere technische Informationen stehen wir Ihnen gerne zur Verfügung.", "Mit freundlichen Grüßen")
            Case "nl"
                Return T("Project - {0}", "Goedendag,", "Hartelijk dank voor uw aanvraag.", "Projectreferentie", "In de bijlage vindt u de PDF-bestanden van de selecties in onderstaande tabel.", "Referentie", "Geselecteerde unit", "Luchtdebiet", "Druk", "PDF-bestand", "Wij blijven beschikbaar voor vragen of aanvullende technische informatie.", "Met vriendelijke groet")
            Case "pl"
                Return T("Projekt - {0}", "Dzień dobry,", "Dziękujemy za zapytanie.", "Numer projektu", "W załączeniu przesyłamy pliki PDF dotyczące doborów wymienionych w poniższej tabeli.", "Referencja", "Wybrana jednostka", "Przepływ powietrza", "Ciśnienie", "Plik PDF", "Pozostajemy do dyspozycji w razie pytań lub potrzeby dodatkowych informacji technicznych.", "Z poważaniem")
            Case "sl"
                Return T("Projekt - {0}", "Pozdravljeni,", "Hvala za vaše povpraševanje.", "Referenca projekta", "V prilogi pošiljamo datoteke PDF za izbire, navedene v spodnji tabeli.", "Referenca", "Izbrana enota", "Pretok zraka", "Tlak", "Datoteka PDF", "Za vsa vprašanja ali dodatne tehnične informacije smo vam na voljo.", "Lep pozdrav")
            Case "bg"
                Return T("Проект - {0}", "Здравейте,", "Благодарим Ви за запитването.", "Референция на проекта", "Приложено изпращаме PDF файловете за селекциите, посочени в следващата таблица.", "Референция", "Избран модул", "Въздушен дебит", "Налягане", "PDF файл", "Оставаме на разположение за въпроси или допълнителна техническа информация.", "С уважение")
            Case "ro"
                Return T("Proiect - {0}", "Bună ziua,", "Vă mulțumim pentru solicitare.", "Referință proiect", "Vă transmitem atașat fișierele PDF aferente selecțiilor din tabelul următor.", "Referință", "Unitate selectată", "Debit de aer", "Presiune", "Fișier PDF", "Rămânem la dispoziția dumneavoastră pentru clarificări sau informații tehnice suplimentare.", "Cu stimă")
            Case "hu"
                Return T("Projekt - {0}", "Tisztelt Partnerünk!", "Köszönjük megkeresését.", "Projekt hivatkozása", "Mellékelten küldjük az alábbi táblázatban szereplő kiválasztások PDF-fájljait.", "Hivatkozás", "Kiválasztott egység", "Légszállítás", "Nyomás", "PDF-fájl", "Kérdés vagy további műszaki információ esetén rendelkezésére állunk.", "Üdvözlettel")
            Case "da"
                Return T("Projekt - {0}", "Goddag,", "Tak for jeres forespørgsel.", "Projektreference", "Vedlagt fremsendes PDF-filerne for de valg, der er angivet i tabellen nedenfor.", "Reference", "Valgt aggregat", "Luftmængde", "Tryk", "PDF-fil", "Vi står til rådighed for spørgsmål eller yderligere tekniske oplysninger.", "Med venlig hilsen")
            Case "sv"
                Return T("Projekt - {0}", "Hej,", "Tack för er förfrågan.", "Projektreferens", "Bifogat skickar vi PDF-filerna för valen i tabellen nedan.", "Referens", "Valt aggregat", "Luftflöde", "Tryck", "PDF-fil", "Vi står till förfogande för frågor eller ytterligare teknisk information.", "Med vänlig hälsning")
            Case "no"
                Return T("Prosjekt - {0}", "God dag,", "Takk for forespørselen.", "Prosjektreferanse", "Vedlagt følger PDF-filene for valgene i tabellen nedenfor.", "Referanse", "Valgt aggregat", "Luftmengde", "Trykk", "PDF-fil", "Ta gjerne kontakt dersom dere trenger avklaringer eller ytterligere teknisk informasjon.", "Med vennlig hilsen")
            Case "is"
                Return T("Verkefni - {0}", "Góðan dag,", "Takk fyrir fyrirspurnina.", "Tilvísun verkefnis", "Meðfylgjandi eru PDF-skjölin fyrir valkostina í töflunni hér að neðan.", "Tilvísun", "Valin eining", "Loftflæði", "Þrýstingur", "PDF-skjal", "Vinsamlegast hafið samband ef þörf er á frekari skýringum eða tæknilegum upplýsingum.", "Með bestu kveðju")
            Case Else
                Return T("Project - {0}", "Good morning,", "Thank you for your enquiry.", "Project reference", "Please find attached the PDF files relating to the selections listed in the following table.", "Reference", "Selected unit", "Airflow", "Pressure", "PDF file", "Please contact us if you require any clarification or further technical information.", "Kind regards")
        End Select
    End Function

    Private Shared Function T(subject As String, greeting As String, thanks As String,
        project As String, intro As String, reference As String, unit As String,
        airflow As String, pressure As String, pdf As String, closing As String,
        regards As String) As CLMultiSelectionEmailText

        Return New CLMultiSelectionEmailText With {
            .Subject = subject, .Greeting = greeting, .Thanks = thanks, .Project = project,
            .Intro = intro, .Reference = reference, .Unit = unit, .Airflow = airflow,
            .Pressure = pressure, .Pdf = pdf, .Closing = closing, .Regards = regards
        }
    End Function
End Class
