# Script para compilar DevStack CLI e GUI
# Execute este script a partir da pasta scripts
param(
    [switch]$WithInstaller = $false
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
dotnet publish "src\CLI\DevStackCLI.csproj" -c Release -r win-x64 --self-contained true
if ($LASTEXITCODE -ne 0) {
    Write-Host "Erro ao compilar DevStack CLI!" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Compilar DevStack GUI (WinExe)
Write-Host "Compilando DevStack GUI (DevStackGUI.exe)..." -ForegroundColor Cyan
dotnet publish "src\GUI\DevStackGUI.csproj" -c Release -r win-x64 --self-contained true
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
Write-Host "  CLI: src\CLI\bin\Release\net9.0-windows\win-x64\publish\" -ForegroundColor Gray
Write-Host "  GUI: src\GUI\bin\Release\net9.0-windows\win-x64\publish\" -ForegroundColor Gray
Write-Host ""

# Mostrar informações dos arquivos gerados
$cliPath = "src\CLI\bin\Release\net9.0-windows\win-x64\publish\DevStack.exe"
$guiPath = "src\GUI\bin\Release\net9.0-windows\win-x64\publish\DevStackGUI.exe"

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

# Deploy to release folder
Write-Host ""
Write-Host "=== Iniciando Deploy para Pasta Release ===" -ForegroundColor Magenta

# Caminhos
$cliSourceDir = "src\CLI\bin\Release\net9.0-windows\win-x64\publish"
$guiSourceDir = "src\GUI\bin\Release\net9.0-windows\win-x64\publish"
$releaseDir = "release"

# Criar pasta release se não existir
if (!(Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
    Write-Host "Pasta release criada." -ForegroundColor Green
}

# Limpar pasta release
Write-Host "Limpando pasta release" -ForegroundColor Yellow
Get-ChildItem $releaseDir | Remove-Item -Recurse -Force

Write-Host ""

# Copiar DevStack.exe (CLI) e dependências
Write-Host "Copiando DevStack.exe (CLI) e dependências..." -ForegroundColor Cyan
Copy-Item "$cliSourceDir\DevStack.exe" "$releaseDir\DevStack.exe" -Force

# Copiar DevStackGUI.exe (GUI) e dependências
Write-Host "Copiando DevStackGUI.exe (GUI)..." -ForegroundColor Cyan
Copy-Item "$guiSourceDir\DevStackGUI.exe" "$releaseDir\DevStackGUI.exe" -Force
Copy-Item "$guiSourceDir\PresentationNative_cor3.dll" "$releaseDir\PresentationNative_cor3.dll" -Force
Copy-Item "$guiSourceDir\wpfgfx_cor3.dll" "$releaseDir\wpfgfx_cor3.dll" -Force

# Copiar ícone
Write-Host "Copiando ícone..." -ForegroundColor Cyan
Copy-Item "src\Shared\DevStack.ico" "$releaseDir\DevStack.ico" -Force

# Verificar se a pasta configs já existe (foi movida anteriormente)
if (!(Test-Path "$releaseDir\configs")) {
    Write-Host "Movendo pasta configs..." -ForegroundColor Cyan
    if (Test-Path "configs") {
        Copy-Item "configs" "$releaseDir\configs" -Recurse -Force
    } else {
        Write-Host "Aviso: Pasta configs não encontrada" -ForegroundColor Yellow
    }
} else {
    Write-Host "Pasta configs já existe no release." -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Deploy Concluído! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Arquivos na pasta release:" -ForegroundColor Yellow
Get-ChildItem "$releaseDir" | ForEach-Object {
    if ($_.PSIsContainer) {
        Write-Host "  📁 $($_.Name)\" -ForegroundColor Blue
    } else {
        Write-Host "  📄 $($_.Name)" -ForegroundColor White
    }
}

Write-Host ""
Write-Host "Executáveis prontos na pasta release:" -ForegroundColor Yellow
Write-Host "  DevStack.exe     - Interface de linha de comando" -ForegroundColor White
Write-Host "  DevStackGUI.exe  - Interface gráfica" -ForegroundColor White

# Build installer if requested
if ($WithInstaller) {
    Write-Host ""
    Write-Host "=== Building Installer ===" -ForegroundColor Magenta
    $installerScript = Join-Path $PSScriptRoot "build-installer.ps1"
    if (Test-Path $installerScript) {
        & $installerScript
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Installer created successfully!" -ForegroundColor Green
        } else {
            Write-Host "Failed to create installer!" -ForegroundColor Red
        }
    } else {
        Write-Host "Installer script not found: $installerScript" -ForegroundColor Red
    }
}
