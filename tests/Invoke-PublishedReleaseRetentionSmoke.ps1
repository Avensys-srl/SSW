$ErrorActionPreference = 'Stop'

$repo = Split-Path $PSScriptRoot -Parent
$publisher = Join-Path $repo 'installer\Publish-ReleaseArtifacts.ps1'
$root = Join-Path ([IO.Path]::GetTempPath()) ('ssw-publish-' + [Guid]::NewGuid().ToString('N'))
$source = Join-Path $root 'source'
$public = Join-Path $root 'public'

try {
    New-Item -ItemType Directory -Path $source, $public -Force | Out-Null
    Set-Content -LiteralPath (Join-Path $public 'SSW_Setup_1_3_0_50.exe') -Value 'old installer'
    Set-Content -LiteralPath (Join-Path $public 'SSW_Setup_1_3_0_50.manifest.json') -Value '{}'
    Set-Content -LiteralPath (Join-Path $public 'keep-me.txt') -Value 'unrelated'

    $installer = Join-Path $source 'SSW_Setup_1_3_0_51.exe'
    $manifest = Join-Path $source 'SSW_Setup_1_3_0_51.manifest.json'
    Set-Content -LiteralPath $installer -Value 'current installer'
    $installerFile = Get-Item -LiteralPath $installer
    $hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash
    @{
        manifest_version = 1
        release = @{
            version = '1.3.0.51'
            installer = @{
                filename = $installerFile.Name
                size_bytes = $installerFile.Length
                sha256 = $hash
            }
        }
    } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath $manifest

    & $publisher -InstallerPath $installer -ManifestPath $manifest -PublishDirectory $public | Out-Null

    $actual = @(Get-ChildItem -LiteralPath $public -File | Select-Object -ExpandProperty Name | Sort-Object)
    $expected = @('keep-me.txt', 'SSW_Setup_1_3_0_51.exe', 'SSW_Setup_1_3_0_51.manifest.json') | Sort-Object
    if (Compare-Object $expected $actual) {
        throw "Unexpected public release contents: $($actual -join ', ')"
    }
    if ((Get-FileHash -LiteralPath (Join-Path $public $installerFile.Name) -Algorithm SHA256).Hash -ne $hash) {
        throw 'Published installer hash changed.'
    }

    Write-Output 'Published release retention passed.'
} finally {
    Remove-Item -LiteralPath $root -Recurse -Force -ErrorAction SilentlyContinue
}
