# Script para compilar DevStack CLI e GUI
# Execute este script a partir da pasta scripts
param(
    [switch]$WithInstaller = $false,
    [switch]$Clean = $false
)

Write-Host "=== DevStack Build Script ===" -ForegroundColor Green
Write-Host ""

# Verificar se estamos na pasta correta
if (!(Test-Path "src\CLI\DevStackCLI.csproj") -or !(Test-Path "src\GUI\DevStackGUI.csproj")) {
    Write-Host "Erro: Não foi possível encontrar os arquivos .csproj nas pastas src/CLI e src/GUI" -ForegroundColor Red
    Write-Host "Certifique-se de que a estrutura de pastas está correta e execute o script a partir da raiz do projeto." -ForegroundColor Yellow
    exit 1
}

# Parar execução do GUI se estiver rodando
Write-Host "Verificando se DevStackGUI está em execução..." -ForegroundColor Yellow
$guiProcesses = Get-Process -Name "DevStackGUI" -ErrorAction SilentlyContinue
if ($guiProcesses) {
    Write-Host "Encontrado(s) $($guiProcesses.Count) processo(s) DevStackGUI em execução. Finalizando..." -ForegroundColor Yellow
    $guiProcesses | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "DevStackGUI finalizado." -ForegroundColor Green
} else {
    Write-Host "DevStackGUI não está em execução." -ForegroundColor Gray
}

# Parar execução do CLI se estiver rodando
Write-Host "Verificando se DevStack CLI está em execução..." -ForegroundColor Yellow
$cliProcesses = Get-Process -Name "DevStack" -ErrorAction SilentlyContinue
if ($cliProcesses) {
    Write-Host "Encontrado(s) $($cliProcesses.Count) processo(s) DevStack CLI em execução. Finalizando..." -ForegroundColor Yellow
    $cliProcesses | Stop-Process -Force
    Start-Sleep -Seconds 2
    Write-Host "DevStack CLI finalizado." -ForegroundColor Green
} else {
    Write-Host "DevStack CLI não está em execução." -ForegroundColor Gray
}

# Limpar builds anteriores
Write-Host "Limpando builds anteriores..." -ForegroundColor Yellow
if (Test-Path "bin") {
    Remove-Item -Recurse -Force "bin"
}
if (Test-Path "obj") {
    Remove-Item -Recurse -Force "obj"
}

Write-Host ""

# Compilar DevStack CLI (Exe)
Write-Host "Compilando DevStack CLI (DevStack.exe)..." -ForegroundColor Cyan
dotnet build "src\CLI\DevStackCLI.csproj" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao compilar DevStack CLI!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Compilar DevStack GUI (WinExe)
Write-Host "Compilando DevStack GUI (DevStackGUI.exe)..." -ForegroundColor Cyan
dotnet build "src\GUI\DevStackGUI.csproj" -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao compilar DevStack GUI!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Build Concluído com Sucesso! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Arquivos gerados:" -ForegroundColor Yellow
Write-Host "  • DevStack.exe     (CLI - Console Application)" -ForegroundColor White
Write-Host "  • DevStackGUI.exe  (GUI - Windows Application)" -ForegroundColor White
Write-Host ""
Write-Host "Localizados em:" -ForegroundColor Gray
Write-Host "  CLI: src\CLI\bin\Release\net9.0-windows\" -ForegroundColor Gray
Write-Host "  GUI: src\GUI\bin\Release\net9.0-windows\" -ForegroundColor Gray
Write-Host ""

# Mostrar informações dos arquivos gerados
$cliPath = "src\CLI\bin\Release\net9.0-windows\DevStack.exe"
$guiPath = "src\GUI\bin\Release\net9.0-windows\DevStackGUI.exe"

if (Test-Path $cliPath) {
    $cliInfo = Get-Item $cliPath
    Write-Host "DevStack.exe:    $($cliInfo.Length) bytes - $($cliInfo.LastWriteTime)" -ForegroundColor Green
}

if (Test-Path $guiPath) {
    $guiInfo = Get-Item $guiPath
    Write-Host "DevStackGUI.exe: $($guiInfo.Length) bytes - $($guiInfo.LastWriteTime)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Uso:" -ForegroundColor Yellow
Write-Host "  DevStack.exe [comando] [argumentos]    # Interface de linha de comando" -ForegroundColor White
Write-Host "  DevStackGUI.exe                        # Interface gráfica" -ForegroundColor White

# Build installer if requested
if ($WithInstaller) {
    Write-Host ""
    Write-Host "=== Building Installer ===" -ForegroundColor Magenta
    
    $installerScript = Join-Path $PSScriptRoot "build-installer.ps1"
    if (Test-Path $installerScript) {
        if ($Clean) {
            & $installerScript -Clean
        } else {
            & $installerScript
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Installer created successfully!" -ForegroundColor Green
        } else {
            Write-Host "Failed to create installer!" -ForegroundColor Red
        }
    } else {
        Write-Host "Installer script not found: $installerScript" -ForegroundColor Red
    }
}
