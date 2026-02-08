@echo off
cls
REM =======================================================
REM        Sincronização de C:\DevLocal com OneDrive
REM  Ignorando bin, obj, .vs, .git, node_modules e arquivos temporários
REM =======================================================

SET "ORIGEM=C:\DevLocal\Kk.Kharts"
SET "DESTINO=C:\OneDrive\OneDrive - 3CTEC\Dev"
SET "PASTAS_EXCLUIDAS=bin obj .vs .git node_modules"
SET "ARQS_EXCLUIDOS=*.user *.suo *.log *.tmp *.swp"
SET "LOG=%DESTINO%\SyncLog.txt"

echo ==================================================
echo        Iniciando sincronizacao de pastas
echo  Origem: %ORIGEM%
echo  Destino: %DESTINO%
echo ==================================================
echo.

REM Comando robocopy
robocopy "%ORIGEM%" "%DESTINO%" /E /Z /COPYALL /R:3 /W:5 /XD %PASTAS_EXCLUIDAS% /XF %ARQS_EXCLUIDOS% /V /TEE /LOG+:"%LOG%"

REM Verificação de erros
IF %ERRORLEVEL% GEQ 8 (
    echo.
    echo ==================================================
    echo ERRO: Alguns arquivos nao foram copiados corretamente!
    echo Veja o log em %LOG%
    echo ==================================================
) ELSE (
    echo.
    echo ==================================================
    echo Sincronizacao concluida com sucesso!
    echo Todos os arquivos importantes foram copiados.
    echo Log: %LOG%
    echo ==================================================
)

echo.
pause
