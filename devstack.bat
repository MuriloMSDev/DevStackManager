@echo off
echo Iniciando DevStack GUI...

REM Tenta usar o PowerShell Core (pwsh.exe) se estiver disponível
where pwsh >nul 2>nul
if %ERRORLEVEL% equ 0 (
    REM Usa PowerShell Core (pwsh.exe)
    start "" /min pwsh.exe -WindowStyle Hidden -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup.ps1" gui
) else (
    REM Usa Windows PowerShell (powershell.exe)
    start "" /min powershell.exe -WindowStyle Hidden -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup.ps1" gui
)

REM Espera um momento e fecha este console
timeout /t 1 /nobreak >nul
exit
