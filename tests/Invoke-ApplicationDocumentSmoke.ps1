param(
    [string]$BinaryDirectory = (Join-Path $PSScriptRoot '..\SSW\bin\x86\AV')
)

$ErrorActionPreference = 'Stop'
if ([Environment]::Is64BitProcess) {
    & "$env:WINDIR\SysWOW64\WindowsPowerShell\v1.0\powershell.exe" `
        -NoProfile -ExecutionPolicy Bypass -File $PSCommandPath -BinaryDirectory $BinaryDirectory
    exit $LASTEXITCODE
}
$binary = (Resolve-Path -LiteralPath $BinaryDirectory).Path
$previousCache = $env:SSW_APPLICATION_DOCUMENT_CACHE_DIRECTORY
$env:SSW_APPLICATION_DOCUMENT_CACHE_DIRECTORY = Join-Path $env:TEMP ('ssw-application-cache-' + [Guid]::NewGuid().ToString('N'))
$relativePdf = 'css\ApplicationDocuments\S7\EN\ST_127_EN_AV.pdf'
$packagedPdf = Join-Path $binary $relativePdf
$sourcePdf = Join-Path $PSScriptRoot '..\SSW\Resources\ApplicationDocuments\S7\EN\ST_127_EN_AV.pdf'
if ((Get-FileHash -LiteralPath $sourcePdf).Hash -ne (Get-FileHash -LiteralPath $packagedPdf).Hash) {
    throw 'Packaged application PDF differs from the original asset.'
}

# A separate domain gives the resolver the real application base directory,
# without starting SSW, accessing customer credentials or downloading anything.
$probeAssembly = Join-Path $env:TEMP ('ssw-documents-' + [Guid]::NewGuid().ToString('N') + '.dll')
$source = @'
using System;
using System.IO;
using System.Reflection;
public sealed class ApplicationDocumentProbe : MarshalByRefObject
{
    public int Run()
    {
        string root = AppDomain.CurrentDomain.BaseDirectory;
        var assembly = Assembly.LoadFrom(Path.Combine(root, "SSWLib.dll"));
        var method = assembly.GetType("SSW.CLProductDocumentService", true)
            .GetMethod("ResolveApplicationDocument");
        if (method == null) throw new Exception("Application document resolver missing.");
        var modelType = method.GetParameters()[0].ParameterType;
        var model = Activator.CreateInstance(modelType);
        var seriesProperty = modelType.GetProperty("CLSerie");
        var series = Activator.CreateInstance(seriesProperty.PropertyType);
        series.GetType().GetProperty("Code").SetValue(series, "7", null);
        seriesProperty.SetValue(model, series, null);
        modelType.GetProperty("Name").SetValue(model, "SG 127 ST", null);
        var sizeProperty = modelType.GetProperty("Size");
        sizeProperty.SetValue(model, Convert.ChangeType(127, Nullable.GetUnderlyingType(sizeProperty.PropertyType) ?? sizeProperty.PropertyType), null);
        var connectionProperty = modelType.GetProperty("CLEnumItem_AeraulicConnection");
        var connection = Activator.CreateInstance(connectionProperty.PropertyType);
        connection.GetType().GetProperty("TextCode").SetValue(connection, "ST", null);
        connectionProperty.SetValue(model, connection, null);
        string expected = Path.Combine(root, "css", "ApplicationDocuments", "S7", "EN", "ST_127_EN_AV.pdf");
        int count = 0;
        foreach (string language in new[] { "EN", "en-GB", "it", "de", "fr", "nl", "bg", "cs", "da", "hu", "is", "no", "pl", "ro", "sl", "sv", "" })
        {
            string actual = (string)method.Invoke(null, new object[] { model, language, "AV", true, false });
            if (!String.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Wrong ST 127 application document for language " + language);
            count++;
        }
        foreach (string name in new[] { "SG 127 FS", "SG 127 VS", "SG 127 HCI", "SG 77 ST", "SG 47 ST" })
        {
            modelType.GetProperty("Name").SetValue(model, name, null);
            connection.GetType().GetProperty("TextCode").SetValue(connection, name.Split(' ')[2], null);
            sizeProperty.SetValue(model, Convert.ChangeType(Int32.Parse(name.Split(' ')[1]), Nullable.GetUnderlyingType(sizeProperty.PropertyType) ?? sizeProperty.PropertyType), null);
            if (!String.IsNullOrEmpty((string)method.Invoke(null, new object[] { model, "EN", "AV", true, false })))
                throw new Exception("ST 127 application document leaked to " + name);
            count++;
        }
        modelType.GetProperty("Name").SetValue(model, "SG 127 ST", null);
        sizeProperty.SetValue(model, Convert.ChangeType(127, Nullable.GetUnderlyingType(sizeProperty.PropertyType) ?? sizeProperty.PropertyType), null);
        connection.GetType().GetProperty("TextCode").SetValue(connection, "ST", null);
        if (!String.IsNullOrEmpty((string)method.Invoke(null, new object[] { model, "EN", "OTHER", true, false })))
            throw new Exception("Avensys PDF leaked to another brand.");
        count++;
        series.GetType().GetProperty("Code").SetValue(series, "1", null);
        if (!String.IsNullOrEmpty((string)method.Invoke(null, new object[] { model, "EN", "AV", true, false })))
            throw new Exception("Application document leaked to another series.");
        count++;
        if (!String.IsNullOrEmpty((string)method.Invoke(null, new object[] { null, "EN", "AV", true, false })))
            throw new Exception("Missing model should not have an application document.");
        return count + 1;
    }
}
'@
$domain = $null
try {
    Add-Type -TypeDefinition $source -OutputAssembly $probeAssembly -OutputType Library
    [void][Reflection.Assembly]::Load([IO.File]::ReadAllBytes($probeAssembly))
    $setup = New-Object AppDomainSetup
    $setup.ApplicationBase = $binary
    $setup.ConfigurationFile = Join-Path $binary 'SSW.exe.config'
    $domain = [AppDomain]::CreateDomain('ApplicationDocumentSmoke', $null, $setup)
    $probe = $domain.CreateInstanceFromAndUnwrap($probeAssembly, 'ApplicationDocumentProbe')
    $count = $probe.Run()
    Write-Host "Application document smoke passed: $count cases; packaged PDF hash matches."
} finally {
    $env:SSW_APPLICATION_DOCUMENT_CACHE_DIRECTORY = $previousCache
    if ($domain) { [AppDomain]::Unload($domain) }
    if (Test-Path -LiteralPath $probeAssembly) { Remove-Item -LiteralPath $probeAssembly -Force }
}
