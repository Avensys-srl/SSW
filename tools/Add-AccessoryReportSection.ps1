param(
    [string]$RepositoryRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'

$reportFiles = @(
    'CLMainReport.rdlc',
    'CLMainReport_Coil.rdlc',
    'CLMainReportWithCO2.rdlc',
    'CLMainReportWithCO2_Coil.rdlc'
)

function New-TextboxXml {
    param(
        [string]$Name,
        [string]$Value,
        [bool]$Bold = $false,
        [string]$Alignment = 'Left',
        [string]$Background = '',
        [string]$BorderColor = 'LightGray'
    )

    $fontWeight = if ($Bold) { '<FontWeight>Bold</FontWeight>' } else { '' }
    $backgroundXml = if ($Background) { "<BackgroundColor>$Background</BackgroundColor>" } else { '' }
    return @"
<Textbox Name="$Name"><CanGrow>true</CanGrow><KeepTogether>true</KeepTogether><Paragraphs><Paragraph><TextRuns><TextRun><Value>$Value</Value><Style><FontFamily>Arial</FontFamily><FontSize>8pt</FontSize>$fontWeight</Style></TextRun></TextRuns><Style><TextAlign>$Alignment</TextAlign></Style></Paragraph></Paragraphs><Style><Border><Color>$BorderColor</Color><Style>Solid</Style><Width>0.5pt</Width></Border>$backgroundXml<PaddingLeft>2pt</PaddingLeft><PaddingRight>2pt</PaddingRight><PaddingTop>2pt</PaddingTop><PaddingBottom>2pt</PaddingBottom></Style></Textbox>
"@
}

function New-CellXml {
    param([string]$Textbox, [int]$ColSpan = 1)
    $span = if ($ColSpan -gt 1) { "<ColSpan>$ColSpan</ColSpan>" } else { '' }
    return "<TablixCell><CellContents>$Textbox$span</CellContents></TablixCell>"
}

$titleCell = (New-CellXml (New-TextboxXml 'AccessoryReport_Title' '=First(Fields!Title.Value, "AccessoryReport")' $true 'Center' '#D0D0D0' 'Black') 4) +
    '<TablixCell /><TablixCell /><TablixCell />'
$headerCells = @(
    $(New-CellXml -Textbox (New-TextboxXml 'AccessoryReport_CodeHeader' '=First(Fields!CodeCaption.Value, "AccessoryReport")' $true 'Left' '#E6E6E6' 'Black'))
    $(New-CellXml -Textbox (New-TextboxXml 'AccessoryReport_DescriptionHeader' '=First(Fields!DescriptionCaption.Value, "AccessoryReport")' $true 'Left' '#E6E6E6' 'Black'))
    $(New-CellXml -Textbox (New-TextboxXml 'AccessoryReport_FunctionsHeader' '=First(Fields!FunctionsCaption.Value, "AccessoryReport")' $true 'Left' '#E6E6E6' 'Black'))
    $(New-CellXml -Textbox (New-TextboxXml 'AccessoryReport_StatusHeader' '=First(Fields!StatusCaption.Value, "AccessoryReport")' $true 'Left' '#E6E6E6' 'Black'))
) -join ''
$detailCells = @(
    $(New-CellXml -Textbox (New-TextboxXml 'AccessoryReport_Code' '=Fields!Code.Value'))
    $(New-CellXml -Textbox (New-TextboxXml 'AccessoryReport_Description' '=Fields!Description.Value'))
    $(New-CellXml -Textbox (New-TextboxXml 'AccessoryReport_Functions' '=Fields!Functions.Value'))
    $(New-CellXml -Textbox (New-TextboxXml 'AccessoryReport_Status' '=Fields!Status.Value'))
) -join ''

$datasetFields = @(
    'Title', 'CodeCaption', 'DescriptionCaption', 'FunctionsCaption', 'StatusCaption',
    'Code', 'Description', 'Functions', 'Status'
) | ForEach-Object {
    "<Field Name=`"$_`"><DataField>$_</DataField><rd:TypeName>System.String</rd:TypeName></Field>"
}
$datasetXml = @"
    <DataSet Name="AccessoryReport">
      <Query><DataSourceName>CLMainReportDataSet</DataSourceName><CommandText>/* Local Query */</CommandText></Query>
      <Fields>$($datasetFields -join '')</Fields>
    </DataSet>
"@

foreach ($reportFile in $reportFiles) {
    $path = Join-Path (Join-Path $RepositoryRoot 'SSWLib') $reportFile
    $content = [IO.File]::ReadAllText($path)
    if ($content.Contains('Name="AccessorySelectionBlock"')) {
        Write-Host "$reportFile already contains AccessorySelectionBlock"
        continue
    }

    [xml]$document = $content
    $namespace = New-Object Xml.XmlNamespaceManager($document.NameTable)
    $namespace.AddNamespace('r', $document.DocumentElement.NamespaceURI)
    $bodyHeightNode = $document.SelectSingleNode('//r:Body/r:Height', $namespace)
    if ($null -eq $bodyHeightNode) { throw "Body height not found in $reportFile" }
    $oldHeightText = $bodyHeightNode.InnerText
    $oldHeight = [double]::Parse($oldHeightText.Replace('cm', ''), [Globalization.CultureInfo]::InvariantCulture)
    $top = $oldHeight + 0.1
    $newHeight = $oldHeight + 2.2
    $topText = $top.ToString('0.#####', [Globalization.CultureInfo]::InvariantCulture) + 'cm'
    $newHeightText = $newHeight.ToString('0.#####', [Globalization.CultureInfo]::InvariantCulture) + 'cm'

    $blockXml = @"
      <Rectangle Name="AccessorySelectionBlock">
        <ReportItems>
          <Tablix Name="TablixAccessoryReport">
            <TablixBody>
              <TablixColumns><TablixColumn><Width>3cm</Width></TablixColumn><TablixColumn><Width>6.5cm</Width></TablixColumn><TablixColumn><Width>6cm</Width></TablixColumn><TablixColumn><Width>4.46238cm</Width></TablixColumn></TablixColumns>
              <TablixRows>
                <TablixRow><Height>0.6cm</Height><TablixCells>$titleCell</TablixCells></TablixRow>
                <TablixRow><Height>0.55cm</Height><TablixCells>$headerCells</TablixCells></TablixRow>
                <TablixRow><Height>0.65cm</Height><TablixCells>$detailCells</TablixCells></TablixRow>
              </TablixRows>
            </TablixBody>
            <TablixColumnHierarchy><TablixMembers><TablixMember /><TablixMember /><TablixMember /><TablixMember /></TablixMembers></TablixColumnHierarchy>
            <TablixRowHierarchy><TablixMembers><TablixMember><KeepWithGroup>After</KeepWithGroup></TablixMember><TablixMember><KeepWithGroup>After</KeepWithGroup></TablixMember><TablixMember><Group Name="AccessoryReport_Details" /></TablixMember></TablixMembers></TablixRowHierarchy>
            <RepeatColumnHeaders>true</RepeatColumnHeaders><DataSetName>AccessoryReport</DataSetName><Top>0cm</Top><Left>0cm</Left><Height>1.8cm</Height><Width>19.96238cm</Width><Style />
          </Tablix>
        </ReportItems>
        <KeepTogether>false</KeepTogether><Top>$topText</Top><Height>2cm</Height><Width>19.96238cm</Width>
        <Visibility><Hidden>=CountRows("AccessoryReport") = 0</Hidden></Visibility>
        <Style><Border><Color>Black</Color><Style>Solid</Style><Width>1pt</Width></Border></Style>
      </Rectangle>
"@

    $bodyHeightToken = "<Height>$oldHeightText</Height>"
    $heightIndex = $content.IndexOf($bodyHeightToken, [StringComparison]::Ordinal)
    if ($heightIndex -lt 0) { throw "Body height token not found in $reportFile" }
    $reportItemsEnd = $content.LastIndexOf('</ReportItems>', $heightIndex, [StringComparison]::Ordinal)
    if ($reportItemsEnd -lt 0) { throw "Body ReportItems end not found in $reportFile" }
    $content = $content.Insert($reportItemsEnd, $blockXml)
    $content = $content.Replace($bodyHeightToken, "<Height>$newHeightText</Height>")

    $dataSetsEnd = $content.LastIndexOf('</DataSets>', [StringComparison]::Ordinal)
    if ($dataSetsEnd -lt 0) { throw "DataSets end not found in $reportFile" }
    $content = $content.Insert($dataSetsEnd, $datasetXml)
    $utf8WithoutBom = New-Object Text.UTF8Encoding($false)
    [IO.File]::WriteAllText($path, $content, $utf8WithoutBom)
    Write-Host "Updated $reportFile"
}
