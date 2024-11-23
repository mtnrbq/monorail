# $sourceKey = Get-Content "C:\Users\<USER>\.local\share\containers\podman\machine\machine"
# $sourceKey | Out-File -FilePath ".\.devcontainer\podman_key" -Encoding utf8
$podmanConn = podman system connection list --format json | ConvertFrom-Json
$defaultConn = $podmanConn | Where-Object { $_.Default -eq $true }

if ($defaultConn) {

    $destfile = ".\.devcontainer\podman_key"

    # Copy the identity file to .devcontainer/podman_key
    Copy-Item -Path $defaultConn.Identity -Destination $destfile -Force

    # Set appropriate permissions on the key
    icacls $destfile /inheritance:r
    icacls $destfile /grant:r "${env:USERNAME}:F"

    Write-Host 'Private key copied to $destfile'
} else {
    Write-Error "No default Podman connection found"
}