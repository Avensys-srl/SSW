param(
    [string]$Configuration = 'AV',
    [switch]$Update,
    [switch]$NumericOnly
)

$ErrorActionPreference = 'Stop'
if ($Update -and $NumericOnly) { throw 'Numeric-only validation cannot update reference fixtures.' }
$repo = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$fixtureRoot = Join-Path $PSScriptRoot 'fixtures\technical-baselines'
$inputRoot = Join-Path $fixtureRoot 'inputs'
$expectedRoot = Join-Path $fixtureRoot 'expected'
$executable = Join-Path $repo "SSW\bin\x86\$Configuration\SSW.exe"
$tolerances = Get-Content -LiteralPath (Join-Path $fixtureRoot 'technical-baseline-tolerances.json') -Raw | ConvertFrom-Json

function Read-BaselineJson([string]$Path) {
    # PowerShell 7.5 otherwise converts ISO strings to DateTime objects; walking
    # DateTime.Date recursively never terminates. Windows PowerShell kept strings.
    $options = @{}
    if ((Get-Command ConvertFrom-Json).Parameters.ContainsKey('DateKind')) {
        $options.DateKind = 'String'
    }
    return (Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json @options)
}

if (-not (Test-Path -LiteralPath $executable)) {
    throw "AV/x86 executable not found: $executable"
}

$actualRoot = Join-Path $env:TEMP ('ssw-technical-baselines-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $actualRoot -Force | Out-Null

function Test-NumericValue([object]$Value) {
    return $Value -is [byte] -or $Value -is [sbyte] -or
        $Value -is [int16] -or $Value -is [uint16] -or
        $Value -is [int32] -or $Value -is [uint32] -or
        $Value -is [int64] -or $Value -is [uint64] -or
        $Value -is [single] -or $Value -is [double] -or $Value -is [decimal]
}

function Get-Tolerance([string]$Path) {
    $absolute = [double]$tolerances.default.absolute
    $relative = [double]$tolerances.default.relative
    foreach ($rule in $tolerances.rules) {
        if ($Path -match [string]$rule.pattern) {
            $absolute = [double]$rule.absolute
            $relative = [double]$rule.relative
            break
        }
    }
    return @($absolute, $relative)
}

function Compare-BaselineNode {
    param(
        [object]$Expected,
        [object]$Actual,
        [string]$Path,
        [System.Collections.Generic.List[string]]$Differences
    )

    if ($Differences.Count -ge 50) { return }
    if ($null -eq $Expected -or $null -eq $Actual) {
        if ($null -ne $Expected -or $null -ne $Actual) {
            $Differences.Add("$Path expected='$Expected' actual='$Actual'")
        }
        return
    }

    if ((Test-NumericValue $Expected) -and (Test-NumericValue $Actual)) {
        $expectedNumber = [double]$Expected
        $actualNumber = [double]$Actual
        $limits = Get-Tolerance $Path
        $allowed = [Math]::Max($limits[0], $limits[1] * [Math]::Max([Math]::Abs($expectedNumber), [Math]::Abs($actualNumber)))
        if ([Math]::Abs($expectedNumber - $actualNumber) -gt $allowed) {
            $Differences.Add("$Path expected=$expectedNumber actual=$actualNumber tolerance=$allowed")
        }
        return
    }

    if ($Expected -is [string] -or $Expected -is [bool]) {
        if ($NumericOnly) { return }
        if (-not [object]::Equals($Expected, $Actual)) {
            $Differences.Add("$Path expected='$Expected' actual='$Actual'")
        }
        return
    }

    if ($Expected -is [System.Collections.IEnumerable] -and -not ($Expected -is [pscustomobject])) {
        $expectedItems = @($Expected)
        $actualItems = @($Actual)
        if ($NumericOnly -and @($expectedItems | Where-Object { $_ -isnot [string] -and $_ -isnot [bool] }).Count -eq 0) { return }
        if ($expectedItems.Count -ne $actualItems.Count) {
            $Differences.Add("$Path count expected=$($expectedItems.Count) actual=$($actualItems.Count)")
            return
        }
        for ($index = 0; $index -lt $expectedItems.Count; $index++) {
            Compare-BaselineNode $expectedItems[$index] $actualItems[$index] "$Path[$index]" $Differences
        }
        return
    }

    $expectedProperties = @($Expected.PSObject.Properties.Name | Sort-Object)
    $actualProperties = @($Actual.PSObject.Properties.Name | Sort-Object)
    if (-not $NumericOnly -and ($expectedProperties -join '|') -ne ($actualProperties -join '|')) {
        $Differences.Add("$Path property set differs")
        return
    }
    foreach ($propertyName in $expectedProperties) {
        Compare-BaselineNode $Expected.$propertyName $Actual.$propertyName "$Path.$propertyName" $Differences
    }
}

try {
    $fixtures = @(Get-ChildItem -LiteralPath $inputRoot -Filter '*.sswsel' | Sort-Object Name)
    if ($fixtures.Count -lt 4) { throw "Expected at least four technical baseline fixtures; found $($fixtures.Count)." }

    foreach ($fixture in $fixtures) {
        $actualPath = Join-Path $actualRoot ($fixture.BaseName + '.json')
        $process = Start-Process -FilePath $executable `
            -ArgumentList @('--technical-baseline', $fixture.FullName, $actualPath) `
            -WindowStyle Hidden -Wait -PassThru
        if ($process.ExitCode -ne 0 -or -not (Test-Path -LiteralPath $actualPath)) {
            throw "Baseline capture failed for $($fixture.Name), exit code $($process.ExitCode)."
        }

        $expectedPath = Join-Path $expectedRoot ($fixture.BaseName + '.json')
        if ($Update) {
            New-Item -ItemType Directory -Path $expectedRoot -Force | Out-Null
            Copy-Item -LiteralPath $actualPath -Destination $expectedPath -Force
            Write-Output "Approved baseline: $($fixture.BaseName)"
            continue
        }
        if (-not (Test-Path -LiteralPath $expectedPath)) {
            throw "Expected baseline missing for $($fixture.Name). Run with -Update after reviewing the capture."
        }

        $expected = Read-BaselineJson $expectedPath
        $actual = Read-BaselineJson $actualPath
        $differences = New-Object 'System.Collections.Generic.List[string]'
        Compare-BaselineNode $expected $actual '$' $differences
        if ($differences.Count -gt 0) {
            throw "Technical baseline regression in $($fixture.Name):`n$($differences -join [Environment]::NewLine)"
        }
        Write-Output "Technical baseline passed: $($fixture.BaseName)"
    }
}
finally {
    $resolvedActualRoot = [IO.Path]::GetFullPath($actualRoot)
    if (-not $resolvedActualRoot.StartsWith([IO.Path]::GetFullPath($env:TEMP) + [IO.Path]::DirectorySeparatorChar)) {
        throw 'Refusing to remove a baseline directory outside TEMP.'
    }
    Remove-Item -LiteralPath $actualRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Write-Output $(if ($NumericOnly) { 'All numeric technical baselines passed; text, catalog metadata and additive fields are outside this mode.' } else { 'All technical calculation baselines passed.' })
