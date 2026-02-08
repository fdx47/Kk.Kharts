param(
    [string]$ConnectionString = "Server=db19330.public.databaseasp.net; Database=db19330; User Id=db19330; Password=4Na+Y=8s6e#W; Encrypt=False; MultipleActiveResultSets=True;",
    [string]$BaseFolder = "c:\\DevLocal\\Kk.Kharts\\KK.Kharts-IoT\\Kk.Kharts.Api\\vpn\\ovpn_fleet_pack_2026-01-24_163929\\ovpn"
)

Add-Type -AssemblyName System.Data

if (-not (Test-Path -Path $BaseFolder)) {
    Write-Error "Base folder '$BaseFolder' not found."
    exit 1
}

$connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
$connection.Open()

$files = Get-ChildItem -Path $BaseFolder -Filter *.ovpn -Recurse
$updated = 0
$skipped = 0

foreach ($file in $files) {
    $commonName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    $content = Get-Content -Path $file.FullName -Raw

    $command = $connection.CreateCommand()
    $command.CommandText = @"
UPDATE [kropkharts].[vpn_profiles]
SET ovpn_file_name = @FileName,
    ovpn_content = @Content
WHERE common_name = @CommonName;
"@

    $command.Parameters.Add("@FileName", [System.Data.SqlDbType]::NVarChar, 200) | Out-Null
    $command.Parameters["@FileName"].Value = $file.Name

    $command.Parameters.Add("@Content", [System.Data.SqlDbType]::NVarChar, -1) | Out-Null
    $command.Parameters["@Content"].Value = $content

    $command.Parameters.Add("@CommonName", [System.Data.SqlDbType]::NVarChar, 100) | Out-Null
    $command.Parameters["@CommonName"].Value = $commonName

    $rows = $command.ExecuteNonQuery()
    if ($rows -gt 0) {
        $updated++
    }
    else {
        $skipped++
        Write-Warning "No profile found for '$commonName' - skipping."
    }
}

Write-Host "OVPN import completed. Updated: $updated, Skipped: $skipped"

$connection.Close()
