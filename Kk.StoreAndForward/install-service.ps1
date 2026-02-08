# Fixed version for service installation
#Requires -RunAsAdministrator

# Configuration
$DOTNET_VERSION = "10.0.0"
$SERVICE_NAME = "KK.UG6x.StoreAndForward"
$SERVICE_DISPLAY_NAME = "Kropkontrol Milesight Store and Forward"
$SERVICE_DESCRIPTION = "Store and Forward bridge with embedded Kestrel UI"
$SERVICE_PORT = "5017"
$EXE_PATH = Join-Path $PSScriptRoot "KK.UG6x.StoreAndForward.exe"

# Validate executable exists
if (-not (Test-Path $EXE_PATH)) {
    Write-Host "[ERREUR] Fichier executable introuvable: $EXE_PATH" -ForegroundColor Red
    Write-Host "         Copiez ce script dans le meme dossier que KK.UG6x.StoreAndForward.exe"
    exit 1
}

# Install .NET runtime
Write-Host "[INFO] Verification du runtime .NET $DOTNET_VERSION ..."
$dotnetInstall = "$env:TEMP\dotnet-install.ps1"
if (-not (Test-Path $dotnetInstall)) {
    Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile $dotnetInstall
}
& $dotnetInstall -Runtime dotnet -Version $DOTNET_VERSION -InstallDir "C:\Program Files\dotnet"

# Install ASP.NET Core runtime
Write-Host "[INFO] Verification du runtime ASP.NET Core $DOTNET_VERSION ..."
& $dotnetInstall -Runtime aspnetcore -Version $DOTNET_VERSION -InstallDir "C:\Program Files\dotnet"

# Stop/remove existing service
Write-Host "[INFO] Arret/Suppression de l'ancien service (si present)..."
if (Get-Service $SERVICE_NAME -ErrorAction SilentlyContinue) {
    Stop-Service $SERVICE_NAME -Force
    sc.exe delete $SERVICE_NAME
}

# Test executable first
Write-Host "[INFO] Test de l'executable (attendre 5 secondes) ..."
Start-Process -FilePath $EXE_PATH -ArgumentList "--urls http://0.0.0.0:$SERVICE_PORT" -NoNewWindow
Start-Sleep -Seconds 5
$process = Get-Process | Where-Object { $_.Path -eq $EXE_PATH } | Select-Object -First 1
if (-not $process) {
    Write-Host "[ERREUR] L'executable n'a pas demarre correctement" -ForegroundColor Red
    Write-Host "         Verifiez les dependances et les droits d'execution"
    exit 1
}
Stop-Process -Id $process.Id -Force
Write-Host "[SUCCES] Executable teste avec succes" -ForegroundColor Green

# Create new service
$binPath = "`"$EXE_PATH`" --urls http://0.0.0.0:$SERVICE_PORT"
Write-Host "[INFO] Creation du service $SERVICE_NAME ..."
$serviceParams = @{
    Name = $SERVICE_NAME
    BinaryPathName = $binPath
    DisplayName = $SERVICE_DISPLAY_NAME
    Description = $SERVICE_DESCRIPTION
    StartupType = 'Automatic'
}
New-Service @serviceParams | Out-Null

# Configure service to auto-start
Set-Service -Name $SERVICE_NAME -StartupType Automatic

# Start service
Write-Host "[INFO] Demarrage du service..."
try {
    Start-Service $SERVICE_NAME -ErrorAction Stop
    Write-Host "[SUCCES] Service demarre avec succes." -ForegroundColor Green
}
catch {
    Write-Host "[ERREUR] Impossible de demarrer le service: $_" -ForegroundColor Red
    Write-Host "         Consultez l'Observateur d'evenements pour plus de details."
}

# Final output
Write-Host "`n[TERMINÉ] Installation finalisee."
Write-Host "     Dossier : $PSScriptRoot"
Write-Host "     Service : $SERVICE_NAME"
Write-Host "     URL     : http://localhost:$SERVICE_PORT"
Write-Host "`nUtilisez ces commandes pour controler le service:"
Write-Host "    Stop-Service $SERVICE_NAME"
Write-Host "    Start-Service $SERVICE_NAME"
