#Requires -Version 5.1

<#
.SYNOPSIS
    Script para verificar e atualizar as versões disponíveis dos componentes DevStack.

.DESCRIPTION
    Este script permite:
    - Verificar URLs existentes nos arquivos JSON
    - Remover URLs quebradas/inválidas
    - Buscar novas versões disponíveis
    - Atualizar os arquivos JSON com novas versões
    - Criar backups antes das alterações
    - Reordenar versões em ordem crescente

.PARAMETER Component
    Componente específico para atualizar. Se não especificado, atualiza todos.

.PARAMETER CheckOnly
    Apenas verifica URLs existentes sem atualizar.

.PARAMETER UpdateAll
    Atualiza todos os componentes automaticamente.

.PARAMETER ClearCache
    Limpa o cache de versões falhadas. Use com -Component para limpar cache específico, ou sozinho para limpar todo o cache.

.PARAMETER ClearBackups
    Limpa backups antigos. Use com -Component para limpar backups específicos, ou sozinho para limpar todos os backups antigos (mais de 30 dias).

.PARAMETER ShowBackups
    Mostra informações dos backups. Use com -Component para mostrar backups específicos, ou sozinho para mostrar todos.

.EXAMPLE
    .\Update-VersionsManager.ps1 -CheckOnly
    Verifica apenas os URLs existentes de todos os componentes

.EXAMPLE
    .\Update-VersionsManager.ps1 -Component "php" -CheckOnly
    Verifica apenas os URLs existentes do PHP

.EXAMPLE
    .\Update-VersionsManager.ps1 -Component "php"
    Atualiza apenas o PHP

.EXAMPLE
    .\Update-VersionsManager.ps1 -UpdateAll
    Atualiza todos os componentes

.EXAMPLE
    .\Update-VersionsManager.ps1 -ClearCache
    Limpa todo o cache de versões falhadas

.EXAMPLE
    .\Update-VersionsManager.ps1 -Component "php" -ClearCache
    Limpa apenas o cache do PHP

.EXAMPLE
    .\Update-VersionsManager.ps1 -ClearBackups
    Limpa backups antigos de todos os componentes

.EXAMPLE
    .\Update-VersionsManager.ps1 -Component "php" -ClearBackups
    Limpa backups antigos apenas do PHP

.EXAMPLE
    .\Update-VersionsManager.ps1 -ShowBackups
    Mostra informações de todos os backups
#>

param(
    [string]$Component = "",
    [switch]$CheckOnly = $false,
    [switch]$UpdateAll = $false,
    [switch]$ClearCache = $false,
    [switch]$ClearBackups = $false,
    [switch]$ShowBackups = $false
)

# Configurações globais
$AvailableVersionsPath = Join-Path $PSScriptRoot "..\src\Shared\AvailableVersions"
$BackupPath = Join-Path $AvailableVersionsPath "backup"
$CachePath = Join-Path $BackupPath "cache"
$MaxWorkers = 50
$TimeoutSeconds = 30

# Headers para evitar detecção como bot
$Headers = @{
    'User-Agent' = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36'
    'Accept' = 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8'
    'Accept-Language' = 'en-US,en;q=0.5'
    'Accept-Encoding' = 'gzip, deflate'
    'Connection' = 'keep-alive'
    'Upgrade-Insecure-Requests' = '1'
}

# Classe para resultado de verificação de URL
class UrlCheckResult {
    [string]$Url
    [bool]$IsValid
    [string]$ErrorMessage
    [int]$StatusCode
}

# Classe para resultado de busca de novas versões
class NewVersionResult {
    [string]$Component
    [System.Collections.Generic.List[PSObject]]$NewVersions
    [string]$ErrorMessage
}

# Função para criar backup
function New-Backup {
    param([string]$FilePath)
    
    if (-not (Test-Path $BackupPath)) {
        New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null
    }
    
    $fileName = Split-Path $FilePath -Leaf
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $backupFile = Join-Path $BackupPath "$($fileName)_$timestamp.bak"
    
    Copy-Item $FilePath $backupFile -Force
    Write-Host "Backup criado: $backupFile" -ForegroundColor Green
    
    # Limpa backups antigos automaticamente (mantém apenas os 10 mais recentes)
    Clear-OldBackups $fileName
}

# Função para limpar backups antigos automaticamente
function Clear-OldBackups {
    param(
        [string]$FileName,
        [int]$KeepCount = 10
    )
    
    if (-not (Test-Path $BackupPath)) {
        return
    }
    
    $baseFileName = $FileName -replace '\.json$', ''
    $backupFiles = Get-ChildItem $BackupPath -Filter "$baseFileName*.bak" | Sort-Object LastWriteTime -Descending
    
    if ($backupFiles.Count -gt $KeepCount) {
        $filesToRemove = $backupFiles | Select-Object -Skip $KeepCount
        foreach ($file in $filesToRemove) {
            Remove-Item $file.FullName -Force
            Write-Host "  Backup antigo removido: $($file.Name)" -ForegroundColor DarkGray
        }
        Write-Host "  $($filesToRemove.Count) backups antigos removidos (mantidos $KeepCount mais recentes)" -ForegroundColor Gray
    }
}

# Função para obter cache de versões falhadas
function Get-FailedVersionsCache {
    param([string]$ComponentName)
    
    if (-not (Test-Path $CachePath)) {
        New-Item -ItemType Directory -Path $CachePath -Force | Out-Null
    }
    
    $cacheFile = Join-Path $CachePath "$ComponentName-failed.json"
    
    if (Test-Path $cacheFile) {
        try {
            $cacheContent = Get-Content $cacheFile -Raw | ConvertFrom-Json
            
            # Remove entradas antigas (mais de 7 dias)
            $cutoffDate = (Get-Date).AddDays(-7)
            $validCache = $cacheContent | Where-Object { 
                (Get-Date $_.FailedDate) -gt $cutoffDate 
            }
            
            # Se houve remoção de entradas antigas, atualiza o cache
            if ($validCache.Count -ne $cacheContent.Count) {
                if ($validCache.Count -eq 0) {
                    Remove-Item $cacheFile -Force
                    Write-Host "  Cache de versões falhadas removido (expirado)" -ForegroundColor Gray
                } else {
                    $validCache | ConvertTo-Json -Depth 10 | Set-Content $cacheFile -Encoding UTF8
                    Write-Host "  Cache de versões falhadas limpo ($($cacheContent.Count - $validCache.Count) entradas expiradas removidas)" -ForegroundColor Gray
                }
            }
            
            return $validCache
        }
        catch {
            Write-Warning "Erro ao ler cache de versões falhadas: $($_.Exception.Message)"
            return @()
        }
    }
    
    return @()
}

# Função para salvar versões falhadas no cache
function Save-FailedVersionsCache {
    param(
        [string]$ComponentName,
        [array]$FailedVersions
    )
    
    if ($FailedVersions.Count -eq 0) {
        return
    }
    
    if (-not (Test-Path $CachePath)) {
        New-Item -ItemType Directory -Path $CachePath -Force | Out-Null
    }
    
    $cacheFile = Join-Path $CachePath "$ComponentName-failed.json"
    
    # Carrega cache existente
    $existingCache = Get-FailedVersionsCache $ComponentName
    
    # Adiciona novas versões falhadas
    $currentDate = Get-Date -Format "yyyy-MM-ddTHH:mm:ss"
    $newFailedEntries = $FailedVersions | ForEach-Object {
        @{
            Version = $_.version
            Url = $_.url
            FailedDate = $currentDate
            ErrorMessage = $_.ErrorMessage
        }
    }
    
    # Combina com cache existente (evita duplicatas)
    $allCachedVersions = @($existingCache)
    foreach ($newEntry in $newFailedEntries) {
        $existing = $allCachedVersions | Where-Object { $_.Version -eq $newEntry.Version -and $_.Url -eq $newEntry.Url }
        if (-not $existing) {
            $allCachedVersions += $newEntry
        }
    }
    
    # Salva cache atualizado
    $allCachedVersions | ConvertTo-Json -Depth 10 | Set-Content $cacheFile -Encoding UTF8
    
    Write-Host "  Cache de versões falhadas atualizado: $($newFailedEntries.Count) novas entradas" -ForegroundColor Gray
}

# Função para obter informações dos backups
function Get-BackupInfo {
    param([string]$ComponentName = "")
    
    if (-not (Test-Path $BackupPath)) {
        return @()
    }
    
    $filter = if ($ComponentName) { "$ComponentName*.bak" } else { "*.bak" }
    $backupFiles = Get-ChildItem $BackupPath -Filter $filter | Where-Object { $_.Name -notlike "*failed.json" }
    
    $backupInfo = @()
    foreach ($file in $backupFiles) {
        # Extrai informações do nome do arquivo: componente_yyyyMMdd_HHmmss.bak
        if ($file.Name -match '^(.+?)_(\d{8})_(\d{6})\.bak$') {
            $component = $matches[1]
            $dateString = $matches[2]
            $timeString = $matches[3]
            
            try {
                $backupDate = [DateTime]::ParseExact("$dateString$timeString", "yyyyMMddHHmmss", $null)
                $daysOld = ((Get-Date) - $backupDate).Days
                
                $backupInfo += @{
                    Component = $component
                    FileName = $file.Name
                    FullPath = $file.FullName
                    BackupDate = $backupDate
                    DaysOld = $daysOld
                    SizeKB = [Math]::Round($file.Length / 1KB, 2)
                }
            }
            catch {
                # Se não conseguir parsear a data, adiciona com data desconhecida
                $backupInfo += @{
                    Component = $component
                    FileName = $file.Name
                    FullPath = $file.FullName
                    BackupDate = $file.LastWriteTime
                    DaysOld = ((Get-Date) - $file.LastWriteTime).Days
                    SizeKB = [Math]::Round($file.Length / 1KB, 2)
                }
            }
        }
    }
    
    return $backupInfo | Sort-Object Component, BackupDate -Descending
}

# Função para mostrar backups
function Show-BackupInfo {
    param([string]$ComponentName = "")
    
    $title = if ($ComponentName) { "=== Backups de $ComponentName ===" } else { "=== Todos os Backups ===" }
    Write-Host "`n$title" -ForegroundColor Cyan
    
    $backups = Get-BackupInfo $ComponentName
    
    if ($backups.Count -eq 0) {
        Write-Host "Nenhum backup encontrado" -ForegroundColor Gray
        return
    }
    
    $currentComponent = ""
    $totalSize = 0
    
    foreach ($backup in $backups) {
        if ($backup.Component -ne $currentComponent) {
            if ($currentComponent) { Write-Host "" }
            Write-Host "--- $($backup.Component) ---" -ForegroundColor Yellow
            $currentComponent = $backup.Component
        }
        
        $ageStatus = if ($backup.DaysOld -gt 30) { " (ANTIGO)" } else { "" }
        $dateFormatted = $backup.BackupDate.ToString("dd/MM/yyyy HH:mm:ss")
        
        Write-Host "  • $($backup.FileName) - $dateFormatted ($($backup.DaysOld) dias) - $($backup.SizeKB) KB$ageStatus" -ForegroundColor Gray
        $totalSize += $backup.SizeKB
    }
    
    Write-Host "`nTotal: $($backups.Count) backups - $([Math]::Round($totalSize, 2)) KB" -ForegroundColor Green
}

# Função para limpar backups antigos
function Clear-OldBackupsManual {
    param(
        [string]$ComponentName = "",
        [int]$DaysOld = 30
    )
    
    $backups = Get-BackupInfo $ComponentName
    $oldBackups = $backups | Where-Object { $_.DaysOld -gt $DaysOld }
    
    if ($oldBackups.Count -eq 0) {
        $scopeText = if ($ComponentName) { "de $ComponentName " } else { "" }
        Write-Host "Nenhum backup ${scopeText}encontrado com mais de $DaysOld dias" -ForegroundColor Gray
        return
    }
    
    Write-Host "`nBackups que serão removidos:" -ForegroundColor Yellow
    foreach ($backup in $oldBackups) {
        Write-Host "  • $($backup.Component): $($backup.FileName) ($($backup.DaysOld) dias)" -ForegroundColor Gray
    }
    
    $totalSize = ($oldBackups | Measure-Object -Property SizeKB -Sum).Sum
    Write-Host "`nTotal a ser removido: $($oldBackups.Count) arquivos - $([Math]::Round($totalSize, 2)) KB" -ForegroundColor Yellow
    
    $confirm = Read-Host "`nConfirma a remoção? (s/N)"
    if ($confirm -eq "s" -or $confirm -eq "S") {
        foreach ($backup in $oldBackups) {
            Remove-Item $backup.FullPath -Force
        }
        Write-Host "$($oldBackups.Count) backups removidos com sucesso" -ForegroundColor Green
    }
}

# Função para limpar todos os backups
function Clear-AllBackups {
    param([string]$ComponentName = "")
    
    $backups = Get-BackupInfo $ComponentName
    
    if ($backups.Count -eq 0) {
        $scopeText = if ($ComponentName) { "de $ComponentName " } else { "" }
        Write-Host "Nenhum backup ${scopeText}encontrado" -ForegroundColor Gray
        return
    }
    
    $totalSize = ($backups | Measure-Object -Property SizeKB -Sum).Sum
    $scopeText = if ($ComponentName) { "de $ComponentName " } else { "" }
    
    Write-Host "`nSerão removidos $($backups.Count) backups ${scopeText}($([Math]::Round($totalSize, 2)) KB)" -ForegroundColor Yellow
    
    $confirm = Read-Host "`nTem certeza que deseja remover TODOS os backups ${scopeText}? (s/N)"
    if ($confirm -eq "s" -or $confirm -eq "S") {
        foreach ($backup in $backups) {
            Remove-Item $backup.FullPath -Force
        }
        Write-Host "$($backups.Count) backups removidos com sucesso" -ForegroundColor Green
    }
}

# Função para restaurar backup
function Restore-Backup {
    param([string]$ComponentName = "")
    
    $backups = Get-BackupInfo $ComponentName
    
    if ($backups.Count -eq 0) {
        $scopeText = if ($ComponentName) { "de $ComponentName " } else { "" }
        Write-Host "Nenhum backup ${scopeText}encontrado" -ForegroundColor Gray
        return
    }
    
    Write-Host "`nBackups disponíveis:" -ForegroundColor Yellow
    for ($i = 0; $i -lt $backups.Count; $i++) {
        $backup = $backups[$i]
        $dateFormatted = $backup.BackupDate.ToString("dd/MM/yyyy HH:mm:ss")
        Write-Host "$($i + 1). $($backup.Component) - $dateFormatted ($($backup.DaysOld) dias)" -ForegroundColor Gray
    }
    
    $choice = Read-Host "`nEscolha o backup para restaurar (1-$($backups.Count))"
    $index = [int]$choice - 1
    
    if ($index -ge 0 -and $index -lt $backups.Count) {
        $selectedBackup = $backups[$index]
        $originalFile = Join-Path $AvailableVersionsPath "$($selectedBackup.Component).json"
        
        Write-Host "`nRestaurar:" -ForegroundColor Yellow
        Write-Host "  De: $($selectedBackup.FileName)" -ForegroundColor Gray
        Write-Host "  Para: $($selectedBackup.Component).json" -ForegroundColor Gray
        
        if (Test-Path $originalFile) {
            Write-Host "  ⚠ O arquivo atual será sobrescrito" -ForegroundColor Red
        }
        
        $confirm = Read-Host "`nConfirma a restauração? (s/N)"
        if ($confirm -eq "s" -or $confirm -eq "S") {
            # Cria backup do arquivo atual antes de restaurar
            if (Test-Path $originalFile) {
                $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
                $preRestoreBackup = Join-Path $BackupPath "$($selectedBackup.Component)_pre_restore_$timestamp.bak"
                Copy-Item $originalFile $preRestoreBackup -Force
                Write-Host "Backup pré-restauração criado: $preRestoreBackup" -ForegroundColor Green
            }
            
            # Restaura o backup
            Copy-Item $selectedBackup.FullPath $originalFile -Force
            Write-Host "Backup restaurado com sucesso!" -ForegroundColor Green
        }
    }
    else {
        Write-Warning "Escolha inválida"
    }
}

# Função para normalizar versão
function Get-NormalizedVersion {
    param(
        [string]$VersionString,
        [string]$ComponentName = ""
    )
    
    # Remove prefixos comuns como v, V, node-v, go, php-, mysql-, nginx-, mongodb-windows-x86_64-, etc.
    $version = $VersionString -replace '^[vV]', '' # Remove v ou V no início
    $version = $version -replace '^node-v?', '' # Remove node-v ou node- no início  
    $version = $version -replace '^go', '' # Remove go no início
    
    # PREFIXOS MAIS ESPECÍFICOS PRIMEIRO (para evitar conflitos)
    $version = $version -replace '^mongodb-windows-x86_64-', '' # Remove mongodb-windows-x86_64- no início
    $version = $version -replace '^php-cs-fixer-?v?', '' # Remove php-cs-fixer-, php-cs-fixer-v ou php-cs-fixerv no início
    $version = $version -replace '^phpMyAdmin-', '' # Remove phpMyAdmin- no início
    $version = $version -replace '^Win64OpenSSL-', '' # Remove Win64OpenSSL- no início
    $version = $version -replace '^elasticsearch-', '' # Remove elasticsearch- no início
    $version = $version -replace '^postgresql-', '' # Remove postgresql- no início
    $version = $version -replace '^dbeaver-ce-', '' # Remove dbeaver-ce- no início
    $version = $version -replace '^wp-cli-', '' # Remove wp-cli- no início
    $version = $version -replace '^MinGit-', '' # Remove MinGit- no início
    
    # PREFIXOS GENÉRICOS POR ÚLTIMO (para não interferir nos específicos)
    $version = $version -replace '^adminer-', '' # Remove adminer- no início
    $version = $version -replace '^composer-', '' # Remove composer- no início
    $version = $version -replace '^python-', '' # Remove python- no início
    $version = $version -replace '^mysql-', '' # Remove mysql- no início
    $version = $version -replace '^nginx-', '' # Remove nginx- no início
    $version = $version -replace '^php-', '' # Remove php- no início (GENÉRICO - por último)
    
    # Remove sufixos comuns como -winx64, -win-x64, -windows-x86_64, -all-languages, etc.
    $version = $version -replace '-winx64.*$', '' # Remove -winx64 e tudo após
    $version = $version -replace '-win-x64.*$', '' # Remove -win-x64 e tudo após
    $version = $version -replace '-win32\.win32\.x86_64.*$', '' # Remove -win32.win32.x86_64 e tudo após
    $version = $version -replace '-windows-x86_64.*$', '' # Remove -windows-x86_64 e tudo após
    $version = $version -replace '-all-languages.*$', '' # Remove -all-languages e tudo após
    $version = $version -replace '-amd64.*$', '' # Remove -amd64 e tudo após
    $version = $version -replace '-embed-amd64.*$', '' # Remove -embed-amd64 e tudo após
    $version = $version -replace '-64-bit.*$', '' # Remove -64-bit e tudo após
    $version = $version -replace '\.zip$', '' # Remove .zip no final
    $version = $version -replace '\.exe$', '' # Remove .exe no final
    $version = $version -replace '\.phar$', '' # Remove .phar no final
    $version = $version -replace '\.php$', '' # Remove .php no final
    $version = $version -replace '-Win32-vs1[67]-x64.*$', '' # Remove -Win32-vs16-x64 ou -Win32-vs17-x64 e tudo após
    $version = $version -replace '\.windows\.(\d+)', '' # Remove apenas 'windows.X' (mantém o resto da versão)
    $version = $version -replace '_(\d+)_(\d+)', '.$1.$2' # Converte underscores em pontos para OpenSSL (3_5_1 -> 3.5.1)
    
    # Remove sufixos de pre-release e build como alpha, beta, rc, etc.
    $version = $version -replace '-?(alpha|beta|rc|dev|snapshot)\d*.*$', '' # Remove alpha, beta, rc, dev, snapshot
    
    # Extrai a versão principal - suporta de 2 a 4 partes (X.Y, X.Y.Z, X.Y.Z.W)
    # Exemplos que deve capturar:
    # - 2.8.10.3 (git) - captura 2.8.10.3
    # - 3.11.9rc1 (python) - captura 3.11.9 (rc1 já foi removido acima)
    # - 8.4.5 (mysql) - captura 8.4.5
    # - 1.24.5 (go) - captura 1.24.5
    # - 2.8.10 (composer) - captura 2.8.10
    # - 5.3.0 (adminer) - captura 5.3.0
    # - 25.1.4 (dbeaver) - captura 25.1.4
    if ($version -match '^(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)') {
        $extractedVersion = $matches[1]
        
        # Componentes que normalmente usam 4 dígitos (x.y.z.w)
        $fourDigitComponents = @("phpmyadmin")
        
        # Se o componente normalmente usa 4 dígitos e a versão tem apenas 3 dígitos, adiciona .0
        if ($ComponentName.ToLower() -in $fourDigitComponents -and $extractedVersion -match '^\d+\.\d+\.\d+$') {
            $extractedVersion = "$extractedVersion.0"
        }
        
        return $extractedVersion
    }
    
    return $null
}

# Função para verificar URL
function Test-UrlValid {
    param([string]$Url)
    
    $result = [UrlCheckResult]::new()
    $result.Url = $Url
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method Head -Headers $Headers -TimeoutSec $TimeoutSeconds -ErrorAction Stop
        $result.IsValid = $true
        $result.StatusCode = $response.StatusCode
    }
    catch {
        $result.IsValid = $false
        $result.ErrorMessage = $_.Exception.Message
        if ($_.Exception.Response) {
            $result.StatusCode = $_.Exception.Response.StatusCode.value__
        }
    }
    
    return $result
}

# Função para verificar se uma URL específica existe antes de adicioná-la
function Test-SingleUrl {
    param([string]$Url)
    
    try {
        Write-Host "  Verificando: $Url" -ForegroundColor Gray
        $response = Invoke-WebRequest -Uri $Url -Method Head -Headers $Headers -TimeoutSec $TimeoutSeconds -ErrorAction Stop
        Write-Host "  ✓ URL válida (Status: $($response.StatusCode))" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "  ✗ URL inválida: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Função para verificar URLs de novas versões em paralelo
function Test-NewVersionUrls {
    param(
        [array]$NewVersions,
        [string]$ComponentName = ""
    )
    
    if ($NewVersions.Count -eq 0) {
        return @()
    }
    
    # Carrega cache de versões falhadas
    $failedCache = @()
    if ($ComponentName) {
        $failedCache = Get-FailedVersionsCache $ComponentName
        if ($failedCache.Count -gt 0) {
            Write-Host "  Cache carregado: $($failedCache.Count) versões falhadas conhecidas" -ForegroundColor Gray
        }
    }
    
    # Filtra versões que já falharam anteriormente
    $versionsToCheck = @()
    $skippedVersions = @()
    
    foreach ($version in $NewVersions) {
        $cachedFailure = $failedCache | Where-Object { 
            $_.Version -eq $version.version -and $_.Url -eq $version.url 
        }
        
        if ($cachedFailure) {
            $skippedVersions += $version
            Write-Host "  ⚠ Pulando $($version.version) (falhou em $($cachedFailure.FailedDate)): $($version.url)" -ForegroundColor Yellow
        } else {
            $versionsToCheck += $version
        }
    }
    
    if ($skippedVersions.Count -gt 0) {
        Write-Host "  $($skippedVersions.Count) versões puladas (cache de falhas)" -ForegroundColor Yellow
    }
    
    if ($versionsToCheck.Count -eq 0) {
        Write-Host "  Nenhuma nova versão para verificar (todas no cache de falhas)" -ForegroundColor Gray
        return @()
    }
    
    $urls = $versionsToCheck | ForEach-Object { $_.url }
    Write-Host "  Verificando $($urls.Count) URLs de novas versões com $MaxWorkers workers..." -ForegroundColor Yellow
    
    $results = @()
    $jobs = @()
    
    for ($i = 0; $i -lt $urls.Count; $i += $MaxWorkers) {
        $batch = $urls[$i..([Math]::Min($i + $MaxWorkers - 1, $urls.Count - 1))]
        
        foreach ($url in $batch) {
            $job = Start-Job -ScriptBlock {
                param($url, $headers, $timeout)
                
                $result = @{
                    Url = $url
                    IsValid = $false
                    ErrorMessage = ""
                    StatusCode = 0
                }
                
                try {
                    $response = Invoke-WebRequest -Uri $url -Method Head -Headers $headers -TimeoutSec $timeout -ErrorAction Stop
                    $result.IsValid = $true
                    $result.StatusCode = $response.StatusCode
                }
                catch {
                    $result.ErrorMessage = $_.Exception.Message
                    if ($_.Exception.Response) {
                        $result.StatusCode = $_.Exception.Response.StatusCode.value__
                    }
                }
                
                return $result
            } -ArgumentList $url, $Headers, $TimeoutSeconds
            
            $jobs += $job
        }
        
        # Aguarda o batch completar
        $jobs | Wait-Job | Out-Null
        
        # Coleta resultados com progresso gradativo
        $initialCount = $results.Count
        foreach ($job in $jobs) {
            $result = Receive-Job $job
            $results += $result
            Remove-Job $job
            
            # Atualiza progresso gradativamente a cada job processado
            $currentPercent = ($results.Count / $urls.Count) * 100
            Write-Progress -Activity "Verificando URLs de novas versões" -Status "Processado: $($results.Count)/$($urls.Count)" -PercentComplete $currentPercent

            # Pequena pausa para visualizar o progresso gradativo (10ms por job)
            Start-Sleep -Milliseconds 10
        }
        
        $jobs = @()
    }
    
    Write-Progress -Activity "Verificando URLs de novas versões" -Completed
    
    # Separa versões válidas e inválidas
    $validVersions = @()
    $failedVersions = @()
    
    foreach ($version in $versionsToCheck) {
        $urlResult = $results | Where-Object { $_.Url -eq $version.url }
        if ($urlResult.IsValid) {
            Write-Host "  ✓ $($version.version): $($version.url)" -ForegroundColor Green
            $validVersions += $version
        }
        else {
            Write-Host "  ✗ $($version.version): $($version.url) - $($urlResult.ErrorMessage)" -ForegroundColor Red
            # Adiciona informação do erro para o cache
            $version | Add-Member -NotePropertyName "ErrorMessage" -NotePropertyValue $urlResult.ErrorMessage
            $failedVersions += $version
        }
    }
    
    # Salva versões falhadas no cache
    if ($ComponentName -and $failedVersions.Count -gt 0) {
        Save-FailedVersionsCache $ComponentName $failedVersions
    }
    
    return $validVersions
}

# Função para verificar URLs em paralelo
function Test-UrlsParallel {
    param([array]$Urls)
    
    $results = @()
    $jobs = @()
    
    Write-Host "Verificando $($Urls.Count) URLs com $MaxWorkers workers..." -ForegroundColor Yellow
    
    for ($i = 0; $i -lt $Urls.Count; $i += $MaxWorkers) {
        $batch = $Urls[$i..([Math]::Min($i + $MaxWorkers - 1, $Urls.Count - 1))]
        
        foreach ($url in $batch) {
            $job = Start-Job -ScriptBlock {
                param($url, $headers, $timeout)
                
                $result = @{
                    Url = $url
                    IsValid = $false
                    ErrorMessage = ""
                    StatusCode = 0
                }
                
                try {
                    $response = Invoke-WebRequest -Uri $url -Method Head -Headers $headers -TimeoutSec $timeout -ErrorAction Stop
                    $result.IsValid = $true
                    $result.StatusCode = $response.StatusCode
                }
                catch {
                    $result.ErrorMessage = $_.Exception.Message
                    if ($_.Exception.Response) {
                        $result.StatusCode = $_.Exception.Response.StatusCode.value__
                    }
                }
                
                return $result
            } -ArgumentList $url, $Headers, $TimeoutSeconds
            
            $jobs += $job
        }
        
        # Aguarda o batch completar
        $jobs | Wait-Job | Out-Null
        
        # Coleta resultados com progresso gradativo
        foreach ($job in $jobs) {
            $result = Receive-Job $job
            $results += $result
            Remove-Job $job
            
            # Atualiza progresso gradativamente a cada job processado
            $currentPercent = ($results.Count / $Urls.Count) * 100
            Write-Progress -Activity "Verificando URLs" -Status "Processado: $($results.Count)/$($Urls.Count)" -PercentComplete $currentPercent
            
            # Pequena pausa para visualizar o progresso gradativo (10ms por job)
            Start-Sleep -Milliseconds 10
        }
        
        $jobs = @()
    }
    
    Write-Progress -Activity "Verificando URLs" -Completed
    return $results
}

# Função para buscar novas versões do Git
function Get-GitNewVersions {
    param([array]$ExistingVersions)
    
    try {        
        $apiUrl = "https://api.github.com/repos/git-for-windows/git/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "git"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            # Procura o asset MinGit
            $asset = $release.assets | Where-Object { $_.name -like "*MinGit*64-bit.zip" } | Select-Object -First 1
            if ($asset) {
                $newVersions += @{
                    version = $version
                    url = $asset.browser_download_url
                }
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "git"
    }
    catch {
        Write-Warning "Erro ao buscar versões do Git: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do Node.js
function Get-NodeNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://nodejs.org/dist/index.json"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            $version = Get-NormalizedVersion $release.version "node"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://nodejs.org/dist/$($release.version)/node-$($release.version)-win-x64.zip"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "node"
    }
    catch {
        Write-Warning "Erro ao buscar versões do Node.js: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do PHP
function Get-PhpNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $baseUrl = "https://windows.php.net/downloads/releases/"
        $archiveUrl = "https://windows.php.net/downloads/releases/archives/"
        
        $newVersions = @()
        
        # Busca versões atuais
        foreach ($url in @($baseUrl, $archiveUrl)) {
            try {
                $response = Invoke-WebRequest -Uri $url -Headers $Headers -TimeoutSec $TimeoutSeconds
                
                $links = $response.Links | Where-Object { $_.href -like "*php-*-Win32-vs*-x64.zip" }
                
                foreach ($link in $links) {
                    if ($link.href -match 'php-(\d+\.\d+\.\d+)-Win32') {
                        $versionString = $matches[1]
                        $version = Get-NormalizedVersion $versionString "php"
                        
                        if (-not $version) { continue }
                        if ($version -in $ExistingVersions.version) { continue }
                        
                        $downloadUrl = if ($link.href.StartsWith('http')) { $link.href } else { $url + $link.href }
                        
                        $newVersions += @{
                            version = $version
                            url = $downloadUrl
                        }
                    }
                }
            }
            catch {
                Write-Warning "Erro ao buscar de $url : $($_.Exception.Message)"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "php"
    }
    catch {
        Write-Warning "Erro ao buscar versões do PHP: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do Python
function Get-PythonNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/python/cpython/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "python"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://www.python.org/ftp/python/$version/python-$version-amd64.zip"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "python"
    }
    catch {
        Write-Warning "Erro ao buscar versões do Python: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do MySQL
function Get-MySqlNewVersions {
    param([array]$ExistingVersions)
    
    try {
        # Busca releases do GitHub oficial do MySQL
        $apiUrl = "https://api.github.com/repos/mysql/mysql-server/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            # Extrai versão do tag (ex: mysql-8.4.5 -> 8.4.5)
            if ($release.tag_name -match '^mysql-(\d+\.\d+\.\d+)$') {
                $version = $matches[1]
            } else {
                # Tenta normalizar outras variações
                $version = Get-NormalizedVersion $release.tag_name "mysql"
                if (-not $version) { continue }
            }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            # Constrói URL baseada no padrão do MySQL
            # Extrai versão principal (ex: 8.4.5 -> 8.4)
            if ($version -match '^(\d+)\.(\d+)\.(\d+)') {
                $majorMinor = "$($matches[1]).$($matches[2])"
                $downloadUrl = "https://dev.mysql.com/get/Downloads/MySQL-$majorMinor/mysql-$version-winx64.zip"
                
                $newVersions += @{
                    version = $version
                    url = $downloadUrl
                }
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "mysql"
    }
    catch {
        Write-Warning "Erro ao buscar versões do MySQL: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do Go
function Get-GoNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/golang/go/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "go"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://go.dev/dl/go$version.windows-amd64.zip"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "go"
    }
    catch {
        Write-Warning "Erro ao buscar versões do Go: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do MongoDB
function Get-MongodbNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/mongodb/mongo/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "mongodb"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-$version.zip"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "mongodb"
    }
    catch {
        Write-Warning "Erro ao buscar versões do MongoDB: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do Nginx
function Get-NginxNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $baseUrl = "https://nginx.org/download/"
        $response = Invoke-WebRequest -Uri $baseUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        $links = $response.Links | Where-Object { $_.href -like "*nginx-*.zip" }
        
        foreach ($link in $links) {
            if ($link.href -match 'nginx-(\d+\.\d+\.\d+)\.zip') {
                $versionString = $matches[1]
                $version = Get-NormalizedVersion $versionString "nginx"
                
                if (-not $version) { continue }
                if ($version -in $ExistingVersions.version) { continue }
                
                $newVersions += @{
                    version = $version
                    url = "https://nginx.org/download/$($link.href)"
                }
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "nginx"
    }
    catch {
        Write-Warning "Erro ao buscar versões do Nginx: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do Elasticsearch
function Get-ElasticsearchNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/elastic/elasticsearch/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "elasticsearch"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-$version-windows-x86_64.zip"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "elasticsearch"
    }
    catch {
        Write-Warning "Erro ao buscar versões do Elasticsearch: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do Composer
function Get-ComposerNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/composer/composer/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "composer"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://getcomposer.org/download/$version/composer.phar"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "composer"
    }
    catch {
        Write-Warning "Erro ao buscar versões do Composer: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do Adminer
function Get-AdminerNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/vrana/adminer/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "adminer"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://github.com/vrana/adminer/releases/download/v$version/adminer-$version.php"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "adminer"
    }
    catch {
        Write-Warning "Erro ao buscar versões do Adminer: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do DBeaver
function Get-DbeaverNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/dbeaver/dbeaver/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "dbeaver"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://dbeaver.io/files/$version/dbeaver-ce-$version-win32.win32.x86_64.zip"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "dbeaver"
    }
    catch {
        Write-Warning "Erro ao buscar versões do DBeaver: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do OpenSSL
function Get-OpensslNewVersions {
    param([array]$ExistingVersions)
    
    try {        
        $apiUrl = "https://api.github.com/repos/openssl/openssl/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "openssl"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            # Converte versão para formato do Shining Light (ex: 3.1.0 -> 3_1_0)
            $versionFormatted = $version -replace '\.', '_'
            $newVersions += @{
                version = $version
                url = "https://slproweb.com/download/Win64OpenSSL-$versionFormatted.exe"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "openssl"
    }
    catch {
        Write-Warning "Erro ao buscar versões do OpenSSL: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do PostgreSQL
function Get-PgsqlNewVersions {
    param([array]$ExistingVersions)
    
    try {        
        $newVersions = @()
        
        # Busca informações da página de download oficial da EnterpriseDB
        $downloadPageUrl = "https://www.enterprisedb.com/download-postgresql-binaries"
        $response = Invoke-WebRequest -Uri $downloadPageUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        # Extrai informações das versões disponíveis
        # Padrão: Version X.Y [Windows x86-64](https://sbp.enterprisedb.com/getfile.jsp?fileid=XXXXXX)
        $content = $response.Content
        
        # Busca por padrões de versão e URLs
        $versionPattern = 'Version\s+(\d+\.\d+(?:\.\d+)?)\s+.*?Windows\s+x86-64.*?fileid=(\d+)'
        $matches = [regex]::Matches($content, $versionPattern, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
        
        foreach ($match in $matches) {
            $versionString = $match.Groups[1].Value
            $fileId = $match.Groups[2].Value
            
            # Normaliza a versão (adiciona .0 se necessário para manter formato X.Y.Z)
            if ($versionString -match '^\d+\.\d+$') {
                $version = "$versionString.0"
            } else {
                $version = $versionString
            }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $downloadUrl = "https://sbp.enterprisedb.com/getfile.jsp?fileid=$fileId"
            
            $newVersions += @{
                version = $version
                url = $downloadUrl
            }
            
            Write-Host "  Encontrada versão: $version (FileID: $fileId)" -ForegroundColor Green
        }
        
        if ($newVersions.Count -gt 0) {
            Write-Host "  Total de novas versões encontradas: $($newVersions.Count)" -ForegroundColor Yellow
        } else {
            Write-Host "  Nenhuma nova versão encontrada" -ForegroundColor Gray
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "pgsql"
    }
    catch {
        Write-Warning "Erro ao buscar versões do PostgreSQL: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do PHP CS Fixer
function Get-PhpcsFixerNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/PHP-CS-Fixer/PHP-CS-Fixer/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "phpcsfixer"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://github.com/PHP-CS-Fixer/PHP-CS-Fixer/releases/download/v$version/php-cs-fixer.phar"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "phpcsfixer"
    }
    catch {
        Write-Warning "Erro ao buscar versões do PHP CS Fixer: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do phpMyAdmin
function Get-PhpmyadminNewVersions {
    param([array]$ExistingVersions)
    
    try {        
        $apiUrl = "https://api.github.com/repos/phpmyadmin/phpmyadmin/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "phpmyadmin"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://files.phpmyadmin.net/phpMyAdmin/$version/phpMyAdmin-$version-all-languages.zip"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "phpmyadmin"
    }
    catch {
        Write-Warning "Erro ao buscar versões do phpMyAdmin: $($_.Exception.Message)"
        return @()
    }
}

# Função para buscar novas versões do WP-CLI
function Get-WpcliNewVersions {
    param([array]$ExistingVersions)
    
    try {
        $apiUrl = "https://api.github.com/repos/wp-cli/wp-cli/releases"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers $Headers -TimeoutSec $TimeoutSeconds
        
        $newVersions = @()
        
        foreach ($release in $response) {
            if ($release.prerelease -or $release.draft) { continue }
            
            $version = Get-NormalizedVersion $release.tag_name "wpcli"
            if (-not $version) { continue }
            
            if ($version -in $ExistingVersions.version) { continue }
            
            $newVersions += @{
                version = $version
                url = "https://github.com/wp-cli/wp-cli/releases/download/v$version/wp-cli-$version.phar"
            }
        }
        
        # Verifica URLs em paralelo e retorna apenas as válidas
        return Test-NewVersionUrls $newVersions "wpcli"
    }
    catch {
        Write-Warning "Erro ao buscar versões do WP-CLI: $($_.Exception.Message)"
        return @()
    }
}

# Função genérica para buscar novas versões
function Get-NewVersionsForComponent {
    param(
        [string]$ComponentName,
        [array]$ExistingVersions
    )
    
    switch ($ComponentName.ToLower()) {
        "git" { return Get-GitNewVersions $ExistingVersions }
        "node" { return Get-NodeNewVersions $ExistingVersions }
        "php" { return Get-PhpNewVersions $ExistingVersions }
        "python" { return Get-PythonNewVersions $ExistingVersions }
        "mysql" { return Get-MySqlNewVersions $ExistingVersions }
        "go" { return Get-GoNewVersions $ExistingVersions }
        "mongodb" { return Get-MongodbNewVersions $ExistingVersions }
        "nginx" { return Get-NginxNewVersions $ExistingVersions }
        "elasticsearch" { return Get-ElasticsearchNewVersions $ExistingVersions }
        "composer" { return Get-ComposerNewVersions $ExistingVersions }
        "adminer" { return Get-AdminerNewVersions $ExistingVersions }
        "dbeaver" { return Get-DbeaverNewVersions $ExistingVersions }
        "openssl" { return Get-OpensslNewVersions $ExistingVersions }
        "pgsql" { return Get-PgsqlNewVersions $ExistingVersions }
        "phpcsfixer" { return Get-PhpcsFixerNewVersions $ExistingVersions }
        "phpmyadmin" { return Get-PhpmyadminNewVersions $ExistingVersions }
        "wpcli" { return Get-WpcliNewVersions $ExistingVersions }
        default { 
            Write-Warning "Busca de novas versões não implementada para: $ComponentName"
            return @()
        }
    }
}

# Função para ordenar versões
function Sort-Versions {
    param([array]$Versions)
    
    return $Versions | Sort-Object { 
        $parts = $_.version.Split('.')
        $major = [int]($parts[0] -replace '\D', '')
        $minor = if ($parts.Length -gt 1) { [int]($parts[1] -replace '\D', '') } else { 0 }
        $patch = if ($parts.Length -gt 2) { [int]($parts[2] -replace '\D', '') } else { 0 }
        $build = if ($parts.Length -gt 3) { [int]($parts[3] -replace '\D', '') } else { 0 }
        
        # Cria um número para ordenação
        ($major * 1000000) + ($minor * 10000) + ($patch * 100) + $build
    }
}

# Função para processar um componente
function Process-Component {
    param(
        [string]$ComponentName,
        [string]$FilePath,
        [bool]$CheckOnly = $false
    )
    
    Write-Host "`n=== Processando $ComponentName ===" -ForegroundColor Cyan
    
    if (-not (Test-Path $FilePath)) {
        Write-Warning "Arquivo não encontrado: $FilePath"
        return
    }
    
    try {
        $jsonContent = Get-Content $FilePath -Raw | ConvertFrom-Json
        Write-Host "Carregadas $($jsonContent.Count) versões existentes" -ForegroundColor Green
        
        # Verifica URLs existentes
        $urls = $jsonContent | ForEach-Object { $_.url }
        $results = Test-UrlsParallel $urls
        
        $validUrls = ($results | Where-Object { $_.IsValid }).Count
        $invalidUrls = ($results | Where-Object { -not $_.IsValid }).Count
        
        Write-Host "URLs válidas: $validUrls" -ForegroundColor Green
        Write-Host "URLs inválidas: $invalidUrls" -ForegroundColor Red
        
        if ($invalidUrls -gt 0) {
            Write-Host "`nURLs inválidas encontradas:" -ForegroundColor Yellow
            $results | Where-Object { -not $_.IsValid } | ForEach-Object {
                Write-Host "  - $($_.Url) (Status: $($_.StatusCode))" -ForegroundColor Red
            }
        }
        
        # Busca novas versões (tanto para CheckOnly quanto para atualização)
        Write-Host "`nBuscando novas versões..." -ForegroundColor Yellow
        $newVersions = Get-NewVersionsForComponent $ComponentName $jsonContent
        
        if ($newVersions.Count -gt 0) {
            Write-Host "Encontradas $($newVersions.Count) novas versões:" -ForegroundColor Green
            
            foreach ($newVersion in $newVersions) {
                Write-Host "  + $($newVersion.version): $($newVersion.url)" -ForegroundColor Cyan
            }
        }
        else {
            Write-Host "Nenhuma nova versão encontrada" -ForegroundColor Yellow
        }
        
        if ($CheckOnly) {
            Write-Host "`n[MODO VERIFICAÇÃO] - Nenhuma alteração foi salva" -ForegroundColor Magenta
            return
        }
        
        # Remove URLs inválidas
        if ($invalidUrls -gt 0) {
            $invalidUrlsList = ($results | Where-Object { -not $_.IsValid }).Url
            $validEntries = $jsonContent | Where-Object { $_.url -notin $invalidUrlsList }
            
            if ($validEntries.Count -lt $jsonContent.Count) {
                Write-Host "Removendo $($jsonContent.Count - $validEntries.Count) entradas com URLs inválidas..." -ForegroundColor Yellow
                $jsonContent = $validEntries
            }
        }
        
        
        if ($newVersions.Count -gt 0) {
            # Adiciona novas versões
            $allVersions = @($jsonContent) + $newVersions
            
            # Ordena em ordem crescente
            $sortedVersions = Sort-Versions $allVersions
            
            # Cria backup
            New-Backup $FilePath
            
            # Salva arquivo atualizado
            $sortedVersions | ConvertTo-Json -Depth 10 | Set-Content $FilePath -Encoding UTF8
            
            Write-Host "Arquivo atualizado com $($allVersions.Count) versões (ordem crescente)" -ForegroundColor Green
        }
        else {
            if ($invalidUrls -gt 0) {
                # Mesmo sem novas versões, salva se removeu URLs inválidas
                $sortedVersions = Sort-Versions $jsonContent
                New-Backup $FilePath
                $sortedVersions | ConvertTo-Json -Depth 10 | Set-Content $FilePath -Encoding UTF8
                Write-Host "Arquivo atualizado (removidas URLs inválidas, ordem crescente)" -ForegroundColor Green
            }
        }
    }
    catch {
        Write-Error "Erro ao processar $ComponentName : $($_.Exception.Message)"
    }
}

# Função para mostrar menu de gerenciamento de cache
function Show-CacheManagementMenu {
    while ($true) {
        Write-Host "`n=== Gerenciamento de Cache e Backups ===" -ForegroundColor Cyan
        Write-Host "--- Cache de Versões Falhadas ---"
        Write-Host "1. Visualizar cache de versões falhadas"
        Write-Host "2. Limpar cache de um componente específico"
        Write-Host "3. Limpar todo o cache"
        Write-Host "4. Limpar cache expirado (mais de 7 dias)"
        Write-Host ""
        Write-Host "--- Gerenciamento de Backups ---"
        Write-Host "5. Visualizar backups"
        Write-Host "6. Limpar backups antigos (mais de 30 dias)"
        Write-Host "7. Limpar todos os backups"
        Write-Host "8. Restaurar backup"
        Write-Host "9. Gerenciar backups por componente"
        Write-Host ""
        Write-Host "10. Voltar ao menu principal"
        
        $choice = Read-Host "`nEscolha uma opção (1-10)"
        
        switch ($choice) {
            "1" {
                Show-FailedVersionsCache
            }
            "2" {
                Clear-ComponentCache
            }
            "3" {
                Clear-AllCache
            }
            "4" {
                Clear-ExpiredCache
            }
            "5" {
                Show-BackupInfo
            }
            "6" {
                Clear-OldBackupsManual
            }
            "7" {
                Clear-AllBackups
            }
            "8" {
                Restore-Backup
            }
            "9" {
                Show-ComponentBackupMenu
            }
            "10" {
                return
            }
            default {
                Write-Warning "Opção inválida"
            }
        }
    }
}

# Função para mostrar menu de backups por componente
function Show-ComponentBackupMenu {
    # Obtém lista de componentes com backups
    $backups = Get-BackupInfo
    $components = $backups | Group-Object Component | Sort-Object Name
    
    if ($components.Count -eq 0) {
        Write-Host "Nenhum componente com backups encontrado" -ForegroundColor Gray
        return
    }
    
    while ($true) {
        Write-Host "`n=== Gerenciamento de Backups por Componente ===" -ForegroundColor Cyan
        Write-Host "Componentes disponíveis:"
        
        for ($i = 0; $i -lt $components.Count; $i++) {
            $component = $components[$i]
            $backupCount = $component.Count
            $oldestBackup = ($component.Group | Sort-Object DaysOld -Descending | Select-Object -First 1).DaysOld
            $totalSize = [Math]::Round(($component.Group | Measure-Object -Property SizeKB -Sum).Sum, 2)
            
            Write-Host "$($i + 1). $($component.Name) ($backupCount backups, mais antigo: $oldestBackup dias, $totalSize KB)"
        }
        
        Write-Host "$($components.Count + 1). Voltar"
        
        $choice = Read-Host "`nEscolha o componente (1-$($components.Count + 1))"
        $index = [int]$choice - 1
        
        if ($index -eq $components.Count) {
            return
        }
        elseif ($index -ge 0 -and $index -lt $components.Count) {
            $selectedComponent = $components[$index].Name
            Show-SingleComponentBackupMenu $selectedComponent
        }
        else {
            Write-Warning "Escolha inválida"
        }
    }
}

# Função para mostrar menu de um componente específico
function Show-SingleComponentBackupMenu {
    param([string]$ComponentName)
    
    while ($true) {
        Write-Host "`n=== Backups de $ComponentName ===" -ForegroundColor Cyan
        Write-Host "1. Visualizar backups"
        Write-Host "2. Limpar backups antigos (mais de 30 dias)"
        Write-Host "3. Limpar todos os backups"
        Write-Host "4. Restaurar backup"
        Write-Host "5. Voltar"
        
        $choice = Read-Host "`nEscolha uma opção (1-5)"
        
        switch ($choice) {
            "1" {
                Show-BackupInfo $ComponentName
            }
            "2" {
                Clear-OldBackupsManual $ComponentName
            }
            "3" {
                Clear-AllBackups $ComponentName
            }
            "4" {
                Restore-Backup $ComponentName
            }
            "5" {
                return
            }
            default {
                Write-Warning "Opção inválida"
            }
        }
    }
}

# Função para visualizar cache de versões falhadas
function Show-FailedVersionsCache {
    Write-Host "`n=== Cache de Versões Falhadas ===" -ForegroundColor Cyan
    
    if (-not (Test-Path $CachePath)) {
        Write-Host "Nenhum cache encontrado" -ForegroundColor Gray
        return
    }
    
    $cacheFiles = Get-ChildItem $CachePath -Filter "*-failed.json"
    
    if ($cacheFiles.Count -eq 0) {
        Write-Host "Nenhum cache de versões falhadas encontrado" -ForegroundColor Gray
        return
    }
    
    foreach ($cacheFile in $cacheFiles) {
        $componentName = $cacheFile.BaseName -replace '-failed$', ''
        Write-Host "`n--- $componentName ---" -ForegroundColor Yellow
        
        try {
            $cacheContent = Get-Content $cacheFile.FullName -Raw | ConvertFrom-Json
            
            if ($cacheContent.Count -eq 0) {
                Write-Host "  Cache vazio" -ForegroundColor Gray
                continue
            }
            
            $cacheContent | ForEach-Object {
                $daysSince = ((Get-Date) - (Get-Date $_.FailedDate)).Days
                $status = if ($daysSince -gt 7) { " (EXPIRADO)" } else { "" }
                Write-Host "  • $($_.Version) - Falhou há $daysSince dias$status" -ForegroundColor Gray
                Write-Host "    URL: $($_.Url)" -ForegroundColor DarkGray
                Write-Host "    Erro: $($_.ErrorMessage)" -ForegroundColor DarkGray
            }
        }
        catch {
            Write-Warning "Erro ao ler cache de $componentName : $($_.Exception.Message)"
        }
    }
}

# Função para limpar cache de um componente específico
function Clear-ComponentCache {
    if (-not (Test-Path $CachePath)) {
        Write-Host "Nenhum cache encontrado" -ForegroundColor Gray
        return
    }
    
    $cacheFiles = Get-ChildItem $CachePath -Filter "*-failed.json"
    
    if ($cacheFiles.Count -eq 0) {
        Write-Host "Nenhum cache de versões falhadas encontrado" -ForegroundColor Gray
        return
    }
    
    Write-Host "`nComponentes com cache disponível:" -ForegroundColor Yellow
    for ($i = 0; $i -lt $cacheFiles.Count; $i++) {
        $componentName = $cacheFiles[$i].BaseName -replace '-failed$', ''
        Write-Host "$($i + 1). $componentName"
    }
    
    $choice = Read-Host "`nEscolha o componente (1-$($cacheFiles.Count))"
    $index = [int]$choice - 1
    
    if ($index -ge 0 -and $index -lt $cacheFiles.Count) {
        $selectedFile = $cacheFiles[$index]
        $componentName = $selectedFile.BaseName -replace '-failed$', ''
        
        $confirm = Read-Host "`nTem certeza que deseja limpar o cache de '$componentName'? (s/N)"
        if ($confirm -eq "s" -or $confirm -eq "S") {
            Remove-Item $selectedFile.FullName -Force
            Write-Host "Cache de '$componentName' removido com sucesso" -ForegroundColor Green
        }
    }
    else {
        Write-Warning "Escolha inválida"
    }
}

# Função para limpar todo o cache
function Clear-AllCache {
    if (-not (Test-Path $CachePath)) {
        Write-Host "Nenhum cache encontrado" -ForegroundColor Gray
        return
    }
    
    $confirm = Read-Host "`nTem certeza que deseja limpar TODO o cache de versões falhadas? (s/N)"
    if ($confirm -eq "s" -or $confirm -eq "S") {
        $cacheFiles = Get-ChildItem $CachePath -Filter "*-failed.json"
        
        if ($cacheFiles.Count -gt 0) {
            $cacheFiles | Remove-Item -Force
            Write-Host "Todo o cache foi removido ($($cacheFiles.Count) arquivos)" -ForegroundColor Green
        }
        else {
            Write-Host "Nenhum cache encontrado para remover" -ForegroundColor Gray
        }
    }
}

# Função para limpar cache expirado
function Clear-ExpiredCache {
    if (-not (Test-Path $CachePath)) {
        Write-Host "Nenhum cache encontrado" -ForegroundColor Gray
        return
    }
    
    $cacheFiles = Get-ChildItem $CachePath -Filter "*-failed.json"
    
    if ($cacheFiles.Count -eq 0) {
        Write-Host "Nenhum cache encontrado" -ForegroundColor Gray
        return
    }
    
    $cutoffDate = (Get-Date).AddDays(-7)
    $removedCount = 0
    $totalFiles = 0
    
    foreach ($cacheFile in $cacheFiles) {
        $componentName = $cacheFile.BaseName -replace '-failed$', ''
        
        try {
            $cacheContent = Get-Content $cacheFile.FullName -Raw | ConvertFrom-Json
            $originalCount = $cacheContent.Count
            
            # Remove entradas expiradas
            $validCache = $cacheContent | Where-Object { 
                (Get-Date $_.FailedDate) -gt $cutoffDate 
            }
            
            if ($validCache.Count -ne $originalCount) {
                if ($validCache.Count -eq 0) {
                    Remove-Item $cacheFile.FullName -Force
                    Write-Host "Cache de $componentName removido completamente (todas as entradas expiraram)" -ForegroundColor Yellow
                    $totalFiles++
                } else {
                    $validCache | ConvertTo-Json -Depth 10 | Set-Content $cacheFile.FullName -Encoding UTF8
                    Write-Host "Cache de ${componentName}: $($originalCount - $validCache.Count) entradas expiradas removidas" -ForegroundColor Yellow
                }
                
                $removedCount += ($originalCount - $validCache.Count)
            }
        }
        catch {
            Write-Warning "Erro ao processar cache de ${componentName}: $($_.Exception.Message)"
        }
    }
    
    if ($removedCount -eq 0 -and $totalFiles -eq 0) {
        Write-Host "Nenhuma entrada expirada encontrada" -ForegroundColor Green
    }
    else {
        Write-Host "`nLimpeza concluída:" -ForegroundColor Green
        if ($removedCount -gt 0) {
            Write-Host "  • $removedCount entradas expiradas removidas" -ForegroundColor Green
        }
        if ($totalFiles -gt 0) {
            Write-Host "  • $totalFiles arquivos de cache removidos completamente" -ForegroundColor Green
        }
    }
}

# Função principal
function Main {
    Write-Host "=== DevStack Version Manager ===" -ForegroundColor Cyan
    Write-Host "Data: $(Get-Date)" -ForegroundColor Gray
    
    # Verifica se a pasta existe
    if (-not (Test-Path $AvailableVersionsPath)) {
        Write-Error "Pasta não encontrada: $AvailableVersionsPath"
        return
    }
    
    # Obtém lista de componentes
    $jsonFiles = Get-ChildItem $AvailableVersionsPath -Filter "*.json" | Where-Object { $_.Name -ne "backup" }
    
    if (-not $jsonFiles) {
        Write-Warning "Nenhum arquivo JSON encontrado em: $AvailableVersionsPath"
        return
    }
    
    Write-Host "Componentes disponíveis:" -ForegroundColor Gray
    foreach ($file in $jsonFiles) {
        $componentName = $file.BaseName
        Write-Host "  - $componentName" -ForegroundColor Gray
    }
    
    # Processa componentes
    if ($ClearCache) {
        # Limpeza de cache
        if ($Component) {
            # Cache de componente específico
            $cacheFile = Join-Path $CachePath "$Component-failed.json"
            if (Test-Path $cacheFile) {
                Remove-Item $cacheFile -Force
                Write-Host "Cache de '$Component' removido com sucesso" -ForegroundColor Green
            }
            else {
                Write-Warning "Cache de '$Component' não encontrado"
            }
        }
        else {
            # Todo o cache
            if (Test-Path $CachePath) {
                $cacheFiles = Get-ChildItem $CachePath -Filter "*-failed.json"
                if ($cacheFiles.Count -gt 0) {
                    $cacheFiles | Remove-Item -Force
                    Write-Host "Todo o cache foi removido ($($cacheFiles.Count) arquivos)" -ForegroundColor Green
                }
                else {
                    Write-Host "Nenhum cache encontrado para remover" -ForegroundColor Gray
                }
            }
            else {
                Write-Host "Pasta de cache não existe" -ForegroundColor Gray
            }
        }
        return
    }
    elseif ($ClearBackups) {
        # Limpeza de backups
        Clear-OldBackupsManual $Component 30
        return
    }
    elseif ($ShowBackups) {
        # Mostrar backups
        Show-BackupInfo $Component
        return
    }
    elseif ($Component) {
        # Componente específico
        $file = $jsonFiles | Where-Object { $_.BaseName -eq $Component }
        if ($file) {
            Process-Component $Component $file.FullName $CheckOnly
        }
        else {
            Write-Warning "Componente '$Component' não encontrado"
        }
    }
    elseif ($UpdateAll) {
        # Todos os componentes automaticamente
        foreach ($file in $jsonFiles) {
            Process-Component $file.BaseName $file.FullName $CheckOnly
        }
    }
    elseif ($CheckOnly) {
        # Apenas verificação de todos os componentes
        foreach ($file in $jsonFiles) {
            Process-Component $file.BaseName $file.FullName $true
        }
    }
    else {
        # Menu interativo
        while ($true) {
            Write-Host "`n=== Menu Principal ===" -ForegroundColor Cyan
            Write-Host "1. Verificar todos os componentes (apenas verificação)"
            Write-Host "2. Verificar um componente específico (apenas verificação)"
            Write-Host "3. Atualizar um componente específico"
            Write-Host "4. Atualizar todos os componentes"
            Write-Host "5. Gerenciar cache e backups"
            Write-Host "6. Sair"
            
            $choice = Read-Host "`nEscolha uma opção (1-6)"
            
            switch ($choice) {
                "1" {
                    foreach ($file in $jsonFiles) {
                        Process-Component $file.BaseName $file.FullName $true
                    }
                }
                "2" {
                    Write-Host "`nComponentes disponíveis:"
                    for ($i = 0; $i -lt $jsonFiles.Count; $i++) {
                        Write-Host "$($i + 1). $($jsonFiles[$i].BaseName)"
                    }
                    
                    $componentChoice = Read-Host "`nEscolha o componente (1-$($jsonFiles.Count))"
                    $index = [int]$componentChoice - 1
                    
                    if ($index -ge 0 -and $index -lt $jsonFiles.Count) {
                        $selectedFile = $jsonFiles[$index]
                        Process-Component $selectedFile.BaseName $selectedFile.FullName $true
                    }
                    else {
                        Write-Warning "Escolha inválida"
                    }
                }
                "3" {
                    Write-Host "`nComponentes disponíveis:"
                    for ($i = 0; $i -lt $jsonFiles.Count; $i++) {
                        Write-Host "$($i + 1). $($jsonFiles[$i].BaseName)"
                    }
                    
                    $componentChoice = Read-Host "`nEscolha o componente (1-$($jsonFiles.Count))"
                    $index = [int]$componentChoice - 1
                    
                    if ($index -ge 0 -and $index -lt $jsonFiles.Count) {
                        $selectedFile = $jsonFiles[$index]
                        Process-Component $selectedFile.BaseName $selectedFile.FullName $false
                    }
                    else {
                        Write-Warning "Escolha inválida"
                    }
                }
                "4" {
                    $confirm = Read-Host "`nTem certeza que deseja atualizar TODOS os componentes? (s/N)"
                    if ($confirm -eq "s" -or $confirm -eq "S") {
                        foreach ($file in $jsonFiles) {
                            Process-Component $file.BaseName $file.FullName $false
                        }
                    }
                }
                "5" {
                    Show-CacheManagementMenu
                }
                "6" {
                    Write-Host "Saindo..." -ForegroundColor Green
                    return
                }
                default {
                    Write-Warning "Opção inválida"
                }
            }
        }
    }
    
    Write-Host "`n=== Processamento concluído ===" -ForegroundColor Green
}

# Executa o script
Main
