param(
    [string]$Configuration = "AV",
    [string]$Platform = "x86",
    [string]$BuildOutputDir = "",
    [string]$InnoSetupCompiler = "",
    [string]$SignTool = "",
    [string]$CertificatePath = "",
    [string]$CertificateThumbprint = $env:SSW_SIGN_CERT_THUMBPRINT,
    [string]$CertificatePassword = $env:SSW_SIGN_CERT_PASSWORD,
    [string]$TimestampUrl = "http://timestamp.digicert.com",
    [string]$PublishCopyDir = "F:\DOCUMENTS\tools\Selection Software",
    [string]$BootstrapKey = $env:SSW_SELECTION_BOOTSTRAP_KEY_AV,
    [string]$BootstrapEnvironmentName = "SSW_SELECTION_BOOTSTRAP_KEY_AV",
    [switch]$SkipBuild,
    [switch]$SkipSigning,
    [switch]$SkipBootstrap
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$solutionPath = Join-Path $repoRoot "SSW.sln"
$innoScriptPath = Join-Path $PSScriptRoot "SSW.iss"
$installerOutputDir = Join-Path $PSScriptRoot "output"
$assemblyInfoPath = Join-Path $repoRoot "SSWLib\My Project\AssemblyInfo.vb"
$appProjectPath = Join-Path $repoRoot "SSW\SSW.csproj"

function Find-FirstExistingPath {
    param([string[]]$Paths)

    foreach ($path in $Paths) {
        if ($path -and (Test-Path $path)) {
            return (Resolve-Path $path).Path
        }
    }

    return ""
}

function Find-MSBuild {
    $command = Get-Command msbuild.exe -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    return Find-FirstExistingPath @(
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Professional\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe",
        "${env:ProgramFiles(x86)}\Microsoft Visual Studio\2019\BuildTools\MSBuild\Current\Bin\MSBuild.exe"
    )
}

function Find-InnoSetupCompiler {
    if ($InnoSetupCompiler) {
        return (Resolve-Path $InnoSetupCompiler).Path
    }

    return Find-FirstExistingPath @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe",
        "${env:LOCALAPPDATA}\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 5\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 5\ISCC.exe",
        "${env:LOCALAPPDATA}\Programs\Inno Setup 5\ISCC.exe"
    )
}

function Find-SignTool {
    if ($SignTool) {
        return (Resolve-Path $SignTool).Path
    }

    $command = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $kitRoots = @(
        "${env:ProgramFiles(x86)}\Windows Kits\10\bin",
        "${env:ProgramFiles}\Windows Kits\10\bin"
    )

    foreach ($kitRoot in $kitRoots) {
        if (Test-Path $kitRoot) {
            $candidate = Get-ChildItem $kitRoot -Recurse -Filter signtool.exe -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -match "\\x64\\signtool\.exe$|\\x86\\signtool\.exe$" } |
                Sort-Object FullName -Descending |
                Select-Object -First 1
            if ($candidate) {
                return $candidate.FullName
            }
        }
    }

    return ""
}

function Get-AppVersion {
    $content = Get-Content $assemblyInfoPath -Raw
    $matches = [regex]::Matches($content, '<Assembly:\s*AssemblyVersion\("([^"]+)"\)>')
    if ($matches.Count -eq 0) {
        throw "Unable to read AssemblyVersion from $assemblyInfoPath"
    }

    return $matches[$matches.Count - 1].Groups[1].Value
}

function Get-DefaultCertificateThumbprint {
    $content = Get-Content $appProjectPath -Raw
    $match = [regex]::Match($content, '<ManifestCertificateThumbprint>([^<]+)</ManifestCertificateThumbprint>')
    if ($match.Success) {
        return $match.Groups[1].Value.Trim()
    }

    return ""
}

function Invoke-SignFile {
    param(
        [string]$FilePath,
        [string]$SignToolPath,
        [string]$CertPath,
        [string]$CertThumbprint,
        [string]$CertPassword
    )

    if (-not (Test-Path $FilePath)) {
        throw "File to sign not found: $FilePath"
    }

    $args = @("sign", "/fd", "SHA256")
    if ($CertThumbprint) {
        $args += @("/sha1", $CertThumbprint)
    } else {
        $args += @("/f", $CertPath)
        if ($CertPassword) {
            $args += @("/p", $CertPassword)
        }
    }
    if ($TimestampUrl) {
        $args += @("/tr", $TimestampUrl, "/td", "SHA256")
    }
    $args += $FilePath

    & $SignToolPath @args
    if ($LASTEXITCODE -ne 0 -and $TimestampUrl) {
        Write-Warning "Timestamp signing failed for $FilePath. Retrying without timestamp."
        $args = @("sign", "/fd", "SHA256")
        if ($CertThumbprint) {
            $args += @("/sha1", $CertThumbprint)
        } else {
            $args += @("/f", $CertPath)
            if ($CertPassword) {
                $args += @("/p", $CertPassword)
            }
        }
        $args += $FilePath
        & $SignToolPath @args
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Signing failed for $FilePath"
    }
}

$appVersion = Get-AppVersion
$msbuildPath = Find-MSBuild
$isccPath = Find-InnoSetupCompiler

if ([string]::IsNullOrWhiteSpace($BootstrapKey)) {
    $BootstrapKey = [Environment]::GetEnvironmentVariable(
        $BootstrapEnvironmentName,
        [EnvironmentVariableTarget]::User)
}
if ([string]::IsNullOrWhiteSpace($BootstrapKey)) {
    $BootstrapKey = [Environment]::GetEnvironmentVariable(
        $BootstrapEnvironmentName,
        [EnvironmentVariableTarget]::Machine)
}

if (-not $SkipBootstrap) {
    if ([string]::IsNullOrWhiteSpace($BootstrapKey)) {
        throw "The technical-selection bootstrap key is missing. Set SSW_SELECTION_BOOTSTRAP_KEY_AV or pass -BootstrapKey."
    }
    if ($BootstrapKey -notmatch '^[A-Za-z0-9_-]{32,}$') {
        throw "The technical-selection bootstrap key has an invalid format."
    }
    if ($BootstrapEnvironmentName -notmatch '^SSW_SELECTION_BOOTSTRAP_KEY_[A-Z0-9]+$') {
        throw "The bootstrap environment variable name has an invalid format."
    }
}

if (-not $msbuildPath) {
    throw "MSBuild not found. Install Visual Studio Build Tools or pass -MSBuild path by adding it to PATH."
}

if (-not $isccPath) {
    throw "Inno Setup compiler not found. Install Inno Setup 6 or pass -InnoSetupCompiler."
}

if (-not $BuildOutputDir) {
    $BuildOutputDir = Join-Path $repoRoot "SSW\bin\x86\AV"
}

if (-not $SkipBuild) {
    Write-Host "Building $solutionPath ($Configuration|$Platform)..."
    & $msbuildPath $solutionPath "/p:Configuration=$Configuration" "/p:Platform=$Platform" "/v:minimal"
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed."
    }
}

$BuildOutputDir = (Resolve-Path $BuildOutputDir).Path
if (-not (Test-Path (Join-Path $BuildOutputDir "SSW.exe"))) {
    throw "Build output does not contain SSW.exe: $BuildOutputDir"
}

if (-not $CertificatePath) {
    if ($env:SSW_SIGN_CERT_PATH) {
        $CertificatePath = $env:SSW_SIGN_CERT_PATH
    } elseif (-not $CertificateThumbprint) {
        $CertificatePath = Join-Path $repoRoot "SSW\ssw.pfx"
    }
}

if (-not $CertificateThumbprint) {
    $CertificateThumbprint = Get-DefaultCertificateThumbprint
}

$signToolPath = Find-SignTool
$canSign = -not $SkipSigning -and $signToolPath -and (
    $CertificateThumbprint -or ($CertificatePath -and (Test-Path $CertificatePath))
)

if ($canSign) {
    Write-Host "Signing application binaries..."
    Invoke-SignFile -FilePath (Join-Path $BuildOutputDir "SSW.exe") -SignToolPath $signToolPath -CertPath $CertificatePath -CertThumbprint $CertificateThumbprint -CertPassword $CertificatePassword
    Invoke-SignFile -FilePath (Join-Path $BuildOutputDir "SSWLib.dll") -SignToolPath $signToolPath -CertPath $CertificatePath -CertThumbprint $CertificateThumbprint -CertPassword $CertificatePassword
} elseif (-not $SkipSigning) {
    Write-Warning "Signing skipped: SignTool or certificate not found. Use -SignTool, -CertificateThumbprint, -CertificatePath or matching environment variables."
}

New-Item -ItemType Directory -Force -Path $installerOutputDir | Out-Null

$iconFile = Join-Path $repoRoot "SSW\Resources\AV_icon.ico"
$isccArgs = @(
    "/DAppVersion=$appVersion",
    "/DSourceDir=$BuildOutputDir",
    "/DOutputDir=$installerOutputDir"
)
if (Test-Path $iconFile) {
    $isccArgs += "/DIconFile=$iconFile"
}
if (-not $SkipBootstrap) {
    $isccArgs += "/DBootstrapEnvironmentName=$BootstrapEnvironmentName"
    $isccArgs += "/DBootstrapKey=$BootstrapKey"
}
$isccArgs += $innoScriptPath

Write-Host "Building installer..."
& $isccPath @isccArgs
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup build failed."
}

$installerFileVersion = $appVersion.Replace(".", "_")
$installerPath = Join-Path $installerOutputDir "SSW_Setup_$installerFileVersion.exe"
if (-not (Test-Path $installerPath)) {
    throw "Installer was not created: $installerPath"
}

if ($canSign) {
    Write-Host "Signing installer..."
    Invoke-SignFile -FilePath $installerPath -SignToolPath $signToolPath -CertPath $CertificatePath -CertThumbprint $CertificateThumbprint -CertPassword $CertificatePassword
}

if ($PublishCopyDir) {
    New-Item -ItemType Directory -Force -Path $PublishCopyDir | Out-Null
    Copy-Item -LiteralPath $installerPath -Destination $PublishCopyDir -Force
    Write-Host "Installer copied to: $PublishCopyDir"
}

Write-Host "Installer ready: $installerPath"
