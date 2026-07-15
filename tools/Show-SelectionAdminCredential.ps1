param([switch]$CopyPassword)

$path = Join-Path $env:USERPROFILE '.avensys\ssw-selection-admin.xml'
if (-not (Test-Path -LiteralPath $path)) { throw 'The local Avensys selection-admin credential was not found.' }
$credential = Import-Clixml -LiteralPath $path
$password = $credential.GetNetworkCredential().Password
if ($CopyPassword) {
    Set-Clipboard -Value $password
    Write-Output "Password copied to the clipboard for user $($credential.UserName)."
} else {
    Write-Output "User: $($credential.UserName)"
    Write-Output "Use -CopyPassword to copy the DPAPI-protected password to the clipboard."
}
