param([string]$BinaryDirectory = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV'))
$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -BinaryDirectory $BinaryDirectory
    exit $LASTEXITCODE
}
Add-Type -AssemblyName System.Drawing
$binary = (Resolve-Path -LiteralPath $BinaryDirectory).Path
[void][Reflection.Assembly]::LoadFrom((Join-Path $binary 'System.Data.SqlServerCe.dll'))
$assembly = [Reflection.Assembly]::LoadFrom((Join-Path $binary 'SSWLib.dll'))
$flags = [Reflection.BindingFlags]'NonPublic,Static'
$legacy = $assembly.GetType('SSW.CLLegacySdfInstallationLayoutRepository').GetMethod('LegacyPortRoles', $flags)
$reference = Get-Content (Join-Path $PSScriptRoot 'fixtures\installation-layout-reference.json') -Raw | ConvertFrom-Json
foreach ($family in @('OSC','SSC')) {
    foreach ($entry in $reference.$family.PSObject.Properties) {
        $actual = $legacy.Invoke($null, @($entry.Name, $family, ''))
        if (($actual -join '|') -ne ($entry.Value -join '|')) { throw "Legacy mismatch $family/$($entry.Name)" }
    }
}
$connection = New-Object System.Data.SqlServerCe.SqlCeConnection(('Data Source="{0}"; Password="{1}"' -f (Join-Path $binary 'data\DataCentral.sdf'), '@D3C1L4T2%'))
$connection.Open()
try {
    $q = $connection.CreateCommand()
    $q.CommandText = 'SELECT c.Code,c.ReferenceView,p.PositionNumber,p.AirRole,c.Id FROM CLFlowConfigurations c INNER JOIN CLFlowConfigurationPorts p ON p.IdFlowConfiguration=c.Id ORDER BY c.Id,p.PositionNumber'
    $table = New-Object Data.DataTable
    $table.Load($q.ExecuteReader())
    foreach ($group in ($table.Rows | Group-Object Id)) {
        $rows = @($group.Group)
        $family = if ($rows[0].ReferenceView.StartsWith('SSC_')) { 'SSC' } else { 'OSC' }
        $expected = $reference.$family.($rows[0].Code)
        if ($null -ne $expected -and (($rows | ForEach-Object AirRole) -join '|') -ne ($expected -join '|')) { throw "SDF mismatch $family/$($rows[0].Code)" }
    }
} finally { $connection.Dispose() }
$renderer = $assembly.GetType('SSW.CLInstallationLayoutReportRenderer')
$placements = $renderer.GetMethod('BuildPortPlacements', $flags)
foreach ($view in @('OSC_NORTH_SOUTH','OSC_EAST_WEST','OSC_CEILING','OSC_FLOOR','SSC_FRONT','SSC_FLAT','SSC_UPRIGHT')) {
    $snapshot = New-Object SSW.CLInstallationLayoutSnapshot
    $referenceRoles = if ($view -eq 'OSC_EAST_WEST') { $reference.OSC.B1 } else { $reference.OSC.B6 }
    for ($i=1; $i -le 4; $i++) {
        $port = New-Object SSW.CLFlowPortDefinition
        $port.Position=$i; $port.FlowCode=$referenceRoles[$i-1]
        $snapshot.FlowPorts.Add($port)
    }
    $sameSide=$view.StartsWith('SSC_')
    $upright=$view -eq 'SSC_UPRIGHT'
    $flat=$view -eq 'SSC_FLAT'
    $ew=$view -eq 'OSC_EAST_WEST'
    $mode=if ($sameSide) { if ($flat -or $upright) {'floor'} else {'ceiling'} } elseif ($view -eq 'OSC_CEILING') {'ceiling'} elseif ($view -eq 'OSC_FLOOR') {'floor'} else {'wall'}
    $rect=New-Object Drawing.RectangleF(200,150,500,180)
    $ports=$placements.Invoke($null,@($snapshot.PSObject.BaseObject,$rect.PSObject.BaseObject,$sameSide,$upright,$flat,$ew,$mode))
    $expectedEdges=if ($sameSide) { if ($flat) {@('Bottom')*4} else {@('Top')*4} } elseif ($ew) {@('Left','Left','Right','Right')} else {@('Bottom','Bottom','Top','Top')}
    for ($i=0; $i -lt 4; $i++) {
        if ([string]$ports[$i].Edge -ne $expectedEdges[$i] -or $ports[$i].Number -ne ($i+1)) { throw "Report position mismatch $view/$i" }
    }
    $config=New-Object SSW.CLInstallationConfiguration
    $config.Code='TEST'; $config.InstallationMode=$mode
    $config.ReferenceView=if ($view -in @('OSC_CEILING','OSC_FLOOR')) {'OSC_NORTH_SOUTH'} else {$view}
    $config.AccessSide=if ($mode -eq 'wall' -or $upright) {'front'} elseif ($mode -eq 'floor') {'upper'} else {'lower'}
    $snapshot.Configurations.Add($config)
    $bitmap=New-Object Drawing.Bitmap(1600,560)
    $bitmap.SetResolution(192,192)
    $graphics=[Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear([Drawing.Color]::White)
        $graphics.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $renderer.GetMethod('DrawLayout',$flags).Invoke($null,@($graphics.PSObject.BaseObject,$snapshot.PSObject.BaseObject,'','TEST',$mode))
        $bitmap.Save((Join-Path $env:TEMP ('ssw-report-layout-'+$view+'.png')),[Drawing.Imaging.ImageFormat]::Png)
        if ($view -eq 'OSC_EAST_WEST') {
            $unitTopLeft = $bitmap.GetPixel(370,125)
            $oldWideEdge = $bitmap.GetPixel(250,125)
            if ($unitTopLeft.R -gt 40 -or $oldWideEdge.R -lt 240) { throw 'Report side-view unit proportion mismatch' }
        }
    } finally {$graphics.Dispose();$bitmap.Dispose()}
}
Write-Host '22 reference sequences, exported catalog and seven report geometries passed.'
