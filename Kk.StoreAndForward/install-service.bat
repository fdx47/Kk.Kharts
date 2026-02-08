@echo off
setlocal enabledelayedexpansion

rem ============================================================================
rem  Installateur KK.UG6x.StoreAndForward
rem  Ce script doit se trouver dans le MEME dossier que KK.UG6x.StoreAndForward.exe
rem ============================================================================

set "DOTNET_VERSION=10.0.0"
set "SERVICE_NAME=KK.UG6x.StoreAndForward"
set "SERVICE_DISPLAY_NAME=KK Milesight Store & Forward"
set "SERVICE_DESCRIPTION=Kropkontrol Milesight Store & Forward"
set "SERVICE_PORT=5017"

set "SCRIPT_DIR=%~dp0"
set "APP_DIR=%SCRIPT_DIR%KK.UG6x.StoreAndForward"
set "EXE_PATH=%APP_DIR%\KK.UG6x.StoreAndForward.exe"

if exist "%EXE_PATH%" goto CHECK_ADMIN
echo [ERREUR] Impossible de trouver "%EXE_PATH%".
echo          Copiez ce script dans le meme dossier que l'executable.
exit /b 1

:CHECK_ADMIN
net session >nul 2>&1
if %errorlevel%==0 goto CHECK_DOTNET
echo [ERREUR] Ce script doit etre lance dans une console elevee (Admin).
exit /b 1

:CHECK_DOTNET

set "DOTNET_INSTALL=%TEMP%\dotnet-install.ps1"
if not exist "%DOTNET_INSTALL%" (
    echo [INFO] Telechargement de dotnet-install.ps1 ...
    powershell -NoProfile -ExecutionPolicy Bypass -Command "Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile '%DOTNET_INSTALL%'"
    if %errorlevel% neq 0 (
        echo [ERREUR] Telechargement impossible.
        exit /b 1
    )
)

echo [INFO] Verification du runtime .NET %DOTNET_VERSION% ...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%DOTNET_INSTALL%' -Runtime dotnet -Version %DOTNET_VERSION% -InstallDir 'C:\Program Files\dotnet'"
if %errorlevel% neq 0 (
    echo [ERREUR] Installation du runtime .NET echouee.
    exit /b 1
)

echo [INFO] Verification du runtime ASP.NET Core %DOTNET_VERSION% ...
powershell -NoProfile -ExecutionPolicy Bypass -Command "& '%DOTNET_INSTALL%' -Runtime aspnetcore -Version %DOTNET_VERSION% -InstallDir 'C:\Program Files\dotnet'"
if %errorlevel% neq 0 (
    echo [ERREUR] Installation du runtime ASP.NET Core echouee.
    exit /b 1
)

echo [INFO] Arret/Suppression de l'ancien service (si present)...
sc stop "%SERVICE_NAME%" >nul 2>&1
sc delete "%SERVICE_NAME%" >nul 2>&1

set "BIN_PATH=\"%EXE_PATH%\" --urls http://0.0.0.0:%SERVICE_PORT%"

echo [INFO] Creation du service "%SERVICE_NAME%" ...
sc create "%SERVICE_NAME%" binPath= "%BIN_PATH%" start= auto DisplayName= "%SERVICE_DISPLAY_NAME%"
if %errorlevel% neq 0 (
    echo [ERREUR] Impossible de creer le service.
    exit /b %errorlevel%
)

sc description "%SERVICE_NAME%" "%SERVICE_DESCRIPTION%"

echo [INFO] Demarrage du service...
sc start "%SERVICE_NAME%"
if %errorlevel% neq 0 (
    echo [AVERTISSEMENT] Service cree mais non demarre automatiquement. Consultez l'Observateur d'evenements.
) else (
    echo [INFO] Service demarre avec succes.
)

echo.
echo [TERMINÉ] Installation finalisee.
echo     Dossier : %APP_DIR%
echo     Service : %SERVICE_NAME%
echo     URL     : http://localhost:%SERVICE_PORT%
echo.
echo Utilisez "sc stop %SERVICE_NAME%" et "sc start %SERVICE_NAME%" pour le controler.

echo.
endlocal