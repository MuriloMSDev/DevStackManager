# Script para preparar o release do DevStack
# Execute este script após o build para copiar todos os arquivos necessários para a pasta release

Write-Host "=== DevStack Deploy Script ===" -ForegroundColor Green
Write-Host ""

# Verificar se os executáveis foram gerados
$cliExe = "..\src\CLI\bin\Release\net9.0-windows\DevStack.exe"
$guiExe = "..\src\GUI\bin\Release\net9.0-windows\DevStackGUI.exe"

if (!(Test-Path $cliExe) -or !(Test-Path $guiExe)) {
    Write-Host "Erro: Execute o build.ps1 primeiro para gerar os executáveis" -ForegroundColor Red
    Write-Host "Esperado:" -ForegroundColor Yellow
    Write-Host "  $cliExe" -ForegroundColor Gray
    Write-Host "  $guiExe" -ForegroundColor Gray
    exit 1
}

# Caminhos
$cliSourceDir = "..\src\CLI\bin\Release\net9.0-windows"
$guiSourceDir = "..\src\GUI\bin\Release\net9.0-windows"
$releaseDir = "..\release"

# Criar pasta release se não existir
if (!(Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
    Write-Host "Pasta release criada." -ForegroundColor Green
}

# Limpar pasta release (exceto configs)
Write-Host "Limpando pasta release (preservando configs)..." -ForegroundColor Yellow
Get-ChildItem $releaseDir | Where-Object { $_.Name -ne "configs" } | Remove-Item -Recurse -Force

Write-Host ""

# Copiar DevStack.exe (CLI) e dependências
Write-Host "Copiando DevStack.exe (CLI) e dependências..." -ForegroundColor Cyan
Copy-Item "$cliSourceDir\DevStack.exe" "$releaseDir\DevStack.exe" -Force
Copy-Item "$cliSourceDir\DevStack.dll" "$releaseDir\DevStack.dll" -Force
Copy-Item "$cliSourceDir\DevStack.deps.json" "$releaseDir\DevStack.deps.json" -Force
Copy-Item "$cliSourceDir\DevStack.runtimeconfig.json" "$releaseDir\DevStack.runtimeconfig.json" -Force

# Copiar DevStackGUI.exe (GUI) e dependências
Write-Host "Copiando DevStackGUI.exe (GUI) e dependências..." -ForegroundColor Cyan
Copy-Item "$guiSourceDir\DevStackGUI.exe" "$releaseDir\DevStackGUI.exe" -Force
Copy-Item "$guiSourceDir\DevStackGUI.dll" "$releaseDir\DevStackGUI.dll" -Force
Copy-Item "$guiSourceDir\DevStackGUI.deps.json" "$releaseDir\DevStackGUI.deps.json" -Force
Copy-Item "$guiSourceDir\DevStackGUI.runtimeconfig.json" "$releaseDir\DevStackGUI.runtimeconfig.json" -Force

# Copiar ícone
Write-Host "Copiando ícone..." -ForegroundColor Cyan
Copy-Item "..\src\Shared\DevStack.ico" "$releaseDir\DevStack.ico" -Force

# Copiar dependências DLL únicas (evitar duplicatas)
Write-Host "Copiando dependências DLL..." -ForegroundColor Cyan
$copiedDlls = @()

# Coletar DLLs do CLI
Get-ChildItem "$cliSourceDir" -Filter "*.dll" | Where-Object { $_.Name -notmatch "^DevStack" } | ForEach-Object {
    if ($_.Name -notin $copiedDlls) {
        Copy-Item $_.FullName "$releaseDir" -Force
        $copiedDlls += $_.Name
    }
}

# Coletar DLLs do GUI (apenas as que ainda não foram copiadas)
Get-ChildItem "$guiSourceDir" -Filter "*.dll" | Where-Object { $_.Name -notmatch "^DevStack" } | ForEach-Object {
    if ($_.Name -notin $copiedDlls) {
        Copy-Item $_.FullName "$releaseDir" -Force
        $copiedDlls += $_.Name
    }
}

# Copiar pasta runtimes se existir
$cliRuntimes = "$cliSourceDir\runtimes"
$guiRuntimes = "$guiSourceDir\runtimes"
if (Test-Path $cliRuntimes) {
    Write-Host "Copiando runtimes do CLI..." -ForegroundColor Cyan
    Copy-Item $cliRuntimes "$releaseDir\runtimes" -Recurse -Force
} elseif (Test-Path $guiRuntimes) {
    Write-Host "Copiando runtimes do GUI..." -ForegroundColor Cyan
    Copy-Item $guiRuntimes "$releaseDir\runtimes" -Recurse -Force
}

# Verificar se a pasta configs já existe (foi movida anteriormente)
if (!(Test-Path "$releaseDir\configs")) {
    Write-Host "Movendo pasta configs..." -ForegroundColor Cyan
    if (Test-Path "..\configs") {
        Move-Item "..\configs" "$releaseDir\configs" -Force
    } else {
        Write-Host "Aviso: Pasta configs não encontrada em ..\configs" -ForegroundColor Yellow
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
Write-Host "Executáveis prontos:" -ForegroundColor Yellow
Write-Host "  DevStack.exe     - Interface de linha de comando" -ForegroundColor White
Write-Host "  DevStackGUI.exe  - Interface gráfica" -ForegroundColor White
Write-Host ""
Write-Host "Para usar, execute os comandos a partir da pasta release:" -ForegroundColor Cyan
Write-Host "  .\DevStack.exe [comando] [argumentos]" -ForegroundColor Gray
Write-Host "  .\DevStackGUI.exe" -ForegroundColor Gray
