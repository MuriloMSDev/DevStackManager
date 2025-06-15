@echo off
IF "%~1"=="" (
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup.ps1"
) ELSE (
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0setup.ps1" %*
)
