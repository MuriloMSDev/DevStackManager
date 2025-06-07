param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet("install", "site", "uninstall", "path", "list", "start", "stop", "restart", "status", "update", "deps", "test", "alias", "global", "self-update", "clean", "backup", "logs", "enable", "disable", "config", "reset", "proxy", "ssl", "db", "service", "doctor")]
    [string]$Command,

    [Parameter(Position=1)]
    [string[]]$Args
)

$baseDir = "C:\devstack"
$nginxDir = "$baseDir\nginx"
$phpDir = "$baseDir\php"
$mysqlDir = "$baseDir\mysql"
$nodeDir = "$baseDir\nodejs"
$pythonDir = "$baseDir\python"
$composerDir = "$baseDir\composer"
$pmaDir = "$baseDir\phpmyadmin"
$mongoDir = "$baseDir\mongodb"
$redisDir = "$baseDir\redis"
$pgsqlDir = "$baseDir\pgsql"
$mailhogDir = "$baseDir\mailhog"
$elasticDir = "$baseDir\elasticsearch"
$memcachedDir = "$baseDir\memcached"
$dockerDir = "$baseDir\docker"
$yarnDir = "$baseDir\yarn"
$pnpmDir = "$baseDir\pnpm"
$wpcliDir = "$baseDir\wpcli"
$adminerDir = "$baseDir\adminer"
$poetryDir = "$baseDir\poetry"
$rubyDir = "$baseDir\ruby"
$goDir = "$baseDir\go"
$certbotDir = "$baseDir\certbot"
$nginxSitesDir = "conf\sites-enabled"

. "$PSScriptRoot\src\install.ps1"
. "$PSScriptRoot\src\uninstall.ps1"
. "$PSScriptRoot\src\path.ps1"
. "$PSScriptRoot\src\list.ps1"
. "$PSScriptRoot\src\process.ps1"

Set-Variable -Name baseDir -Value $baseDir -Scope Global
Set-Variable -Name nginxDir -Value $nginxDir -Scope Global
Set-Variable -Name phpDir -Value $phpDir -Scope Global
Set-Variable -Name mysqlDir -Value $mysqlDir -Scope Global
Set-Variable -Name nodeDir -Value $nodeDir -Scope Global
Set-Variable -Name pythonDir -Value $pythonDir -Scope Global
Set-Variable -Name composerDir -Value $composerDir -Scope Global
Set-Variable -Name pmaDir -Value $pmaDir -Scope Global
Set-Variable -Name mongoDir -Value $mongoDir -Scope Global
Set-Variable -Name redisDir -Value $redisDir -Scope Global
Set-Variable -Name pgsqlDir -Value $pgsqlDir -Scope Global
Set-Variable -Name mailhogDir -Value $mailhogDir -Scope Global
Set-Variable -Name elasticDir -Value $elasticDir -Scope Global
Set-Variable -Name memcachedDir -Value $memcachedDir -Scope Global
Set-Variable -Name dockerDir -Value $dockerDir -Scope Global
Set-Variable -Name yarnDir -Value $yarnDir -Scope Global
Set-Variable -Name pnpmDir -Value $pnpmDir -Scope Global
Set-Variable -Name wpcliDir -Value $wpcliDir -Scope Global
Set-Variable -Name adminerDir -Value $adminerDir -Scope Global
Set-Variable -Name poetryDir -Value $poetryDir -Scope Global
Set-Variable -Name rubyDir -Value $rubyDir -Scope Global
Set-Variable -Name goDir -Value $goDir -Scope Global
Set-Variable -Name certbotDir -Value $certbotDir -Scope Global
Set-Variable -Name nginxSitesDir -Value $nginxSitesDir -Scope Global

function Write-Info($msg) { Write-Host $msg -ForegroundColor Cyan }
function Write-WarningMsg($msg) { Write-Host $msg -ForegroundColor Yellow }
function Write-ErrorMsg($msg) { Write-Host $msg -ForegroundColor Red }
function Write-Log($msg) {
    $logFile = Join-Path $baseDir "devstack.log"
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $logFile -Value "[$timestamp] $msg"
}

function Status-Component {
    param([string]$Component)
    $dir = switch ($Component) {
        "php" { $phpDir }
        "nginx" { $nginxDir }
        "mysql" { $mysqlDir }
        "nodejs" { $nodeDir }
        "python" { $pythonDir }
        "composer" { $composerDir }
        "git" { $baseDir }
        default { return }
    }
    if ($Component -eq "git") {
        $versions = Get-ChildItem $dir -Directory | Where-Object { $_.Name -like 'git-*' } | ForEach-Object { $_.Name }
    } else {
        $versions = Get-ChildItem $dir -Directory | ForEach-Object { $_.Name }
    }
    if ($versions.Count -eq 0) {
        Write-WarningMsg "$Component não está instalado."
        return
    }
    Write-Info "$Component instalado(s):"
    $versions | ForEach-Object {
        Write-Host "  $_"
    }
}

function Status-All {
    Write-Host "Status do DevStack:"
    Status-Component "php"
    Status-Component "nginx"
    Status-Component "mysql"
    Status-Component "nodejs"
    Status-Component "python"
    Status-Component "composer"
}

function Test-All {
    Write-Host "Testando ferramentas instaladas:"
    $tools = @(
        @{ name = "php"; exe = "php-*.exe"; dir = $phpDir; args = "-v" },
        @{ name = "nginx"; exe = "nginx-*.exe"; dir = $nginxDir; args = "-v" },
        @{ name = "mysql"; exe = "mysqld-*.exe"; dir = $mysqlDir; args = "--version" },
        @{ name = "nodejs"; exe = "node-*.exe"; dir = $nodeDir; args = "-v" },
        @{ name = "python"; exe = "python-*.exe"; dir = $pythonDir; args = "--version" },
        @{ name = "git"; exe = "git.exe"; dir = $baseDir; args = "--version" }
    )
    foreach ($tool in $tools) {
        if ($tool.name -eq "git") {
            $found = Get-ChildItem $tool.dir -Recurse -Filter $tool.exe -ErrorAction SilentlyContinue | Where-Object { $_.FullName -like "*\\cmd\\git.exe" } | Select-Object -First 1
        } else {
            $found = Get-ChildItem $tool.dir -Recurse -Filter $tool.exe -ErrorAction SilentlyContinue | Select-Object -First 1
        }
        if ($found) {
            try {
                $output = & $found.FullName $tool.args
                Write-Info "$($tool.name): $output"
            } catch {
                Write-ErrorMsg "$($tool.name): erro ao executar $($found.FullName)"
            }
        } else {
            Write-WarningMsg "$($tool.name): não encontrado."
        }
    }
}

function Deps-Check {
    Write-Host "Verificando dependências do sistema..."
    $missing = @()
    if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        $missing += "Permissão de administrador"
    }
    if (-not (Get-Command Expand-Archive -ErrorAction SilentlyContinue)) {
        $missing += "Expand-Archive (PowerShell 5+)"
    }
    if ($missing.Count -eq 0) {
        Write-Info "Todas as dependências estão presentes."
    } else {
        Write-ErrorMsg "Dependências ausentes: $($missing -join ", ")"
    }
}

function Update-Component {
    param([string]$Component)
    switch ($Component) {
        "php" { Install-PHP }
        "nginx" { Install-Nginx }
        "mysql" { Install-MySQL }
        "nodejs" { Install-NodeJS }
        "python" { Install-Python }
        "composer" { Install-Composer }
        "phpmyadmin" { Install-PhpMyAdmin }
        "git" { Install-Git }
        default { Write-ErrorMsg "Componente desconhecido: $Component" }
    }
}

function Alias-Component {
    param([string]$Component, [string]$Version)
    $aliasDir = Join-Path $baseDir "aliases"
    if (-not (Test-Path $aliasDir)) { New-Item -ItemType Directory -Path $aliasDir | Out-Null }
    $exe = switch ($Component) {
        "php" { Join-Path $phpDir "php-$Version\php-$Version.exe" }
        "nginx" { Join-Path $nginxDir "nginx-$Version\nginx-$Version.exe" }
        "nodejs" { Join-Path $nodeDir "node-v$Version-win-x64\node-$Version.exe" }
        "python" { Join-Path $pythonDir "python-$Version\python-$Version.exe" }
        "git" { Join-Path $baseDir "git-$Version\cmd\git.exe" }
        default { $null }
    }
    if ($exe -and (Test-Path $exe)) {
        $bat = Join-Path $aliasDir "$Component$Version.bat"
        Set-Content -Path $bat -Value "@echo off`r`n\"$exe\" %*"
        Write-Info "Alias criado: $bat"
    } else {
        Write-ErrorMsg "Executável não encontrado para $Component $Version"
    }
}

switch ($Command) {
    "list" {
        if ($Args.Count -eq 0) {
            Write-Host "Uso: setup.ps1 list <php|nodejs|python|--installed|-i>"
            exit 1
        }
        $firstArg = $Args[0].Trim()
        if ($firstArg -eq '--installed') {
            if (-not (Test-Path $baseDir)) {
                Write-WarningMsg "O diretório $baseDir não existe. Nenhuma ferramenta instalada."
                return
            }
            Write-Host "Ferramentas instaladas:"
            $components = @(
                @{ name = 'php'; dir = $phpDir },
                @{ name = 'nginx'; dir = $nginxDir },
                @{ name = 'mysql'; dir = $mysqlDir },
                @{ name = 'nodejs'; dir = $nodeDir },
                @{ name = 'python'; dir = $pythonDir },
                @{ name = 'composer'; dir = $composerDir },
                @{ name = 'phpmyadmin'; dir = $pmaDir },
                @{ name = 'git'; dir = $baseDir; pattern = 'git-*' },
                @{ name = 'mongodb'; dir = $mongoDir },
                @{ name = 'redis'; dir = $redisDir },
                @{ name = 'pgsql'; dir = $pgsqlDir },
                @{ name = 'mailhog'; dir = $mailhogDir },
                @{ name = 'elasticsearch'; dir = $elasticDir },
                @{ name = 'memcached'; dir = $memcachedDir },
                @{ name = 'docker'; dir = $dockerDir },
                @{ name = 'yarn'; dir = $yarnDir },
                @{ name = 'pnpm'; dir = $pnpmDir },
                @{ name = 'wpcli'; dir = $wpcliDir },
                @{ name = 'adminer'; dir = $adminerDir },
                @{ name = 'poetry'; dir = $poetryDir },
                @{ name = 'ruby'; dir = $rubyDir },
                @{ name = 'go'; dir = $goDir },
                @{ name = 'certbot'; dir = $certbotDir }
            )
            foreach ($comp in $components) {
                if (-not (Test-Path $comp.dir)) {
                    Write-Host ("{0,-12}: (não instalado)" -f $comp.name)
                    continue
                }
                if ($comp.name -eq 'git') {
                    $versions = Get-ChildItem $comp.dir -Directory | Where-Object { $_.Name -like $comp.pattern } | ForEach-Object { $_.Name }
                } else {
                    $versions = Get-ChildItem $comp.dir -Directory -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }
                }
                if ($versions -and $versions.Count -gt 0) {
                    Write-Host ("{0,-12}: {1}" -f $comp.name, ($versions -join ", "))
                } else {
                    Write-Host ("{0,-12}: (não instalado)" -f $comp.name)
                }
            }
            return
        }
        switch ($firstArg.ToLower()) {
            "php"     { List-PHPVersions }
            "nodejs"  { List-NodeVersions }
            "node"    { List-NodeVersions }
            "python"  { List-PythonVersions }
            default   { Write-Host "Ferramenta desconhecida: $($firstArg)" }
        }
    }
    "site" {
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 site <dominio> [-root <diretorio>] [-php <php-upstream>] [-nginx <nginx-version>]"
            exit 1
        }
        $Domain = $Args[0]

        for ($i = 1; $i -lt $Args.Count; $i++) {
            switch ($Args[$i]) {
                "-root" {
                    $i++; if ($i -lt $Args.Count) { $Root = $Args[$i] }
                }
                "-php" {
                    $i++; if ($i -lt $Args.Count) { $PhpUpstream = $Args[$i] }
                }
                "-nginx" {
                    $i++; if ($i -lt $Args.Count) { $NginxVersion = $Args[$i] }
                }
                "-index" {
                    $i++; if ($i -lt $Args.Count) { $IndexLocation = $Args[$i] }
                }
            }
        }
        Create-NginxSiteConfig -Domain $Domain -Root $Root -PhpUpstream $PhpUpstream -NginxVersion $NginxVersion -IndexLocation $IndexLocation
    }
    "install" {
        foreach ($component in $Args) {
            switch -Regex ($component) {
                "^php-(.+)$"         { Install-PHP ($component -replace "^php-") }
                "^php$"              { Install-PHP }
                "^nginx$"            { Install-Nginx }
                "^mysql-(.+)$"       { Install-MySQL ($component -replace "^mysql-") }
                "^mysql$"            { Install-MySQL }
                "^nodejs-(.+)$"      { Install-NodeJS ($component -replace "^nodejs-") }
                "^(nodejs|node)$"    { Install-NodeJS }
                "^python-(.+)$"      { Install-Python ($component -replace "^python-") }
                "^python$"           { Install-Python }
                "^composer-(.+)$"    { Install-Composer ($component -replace "^composer-") }
                "^composer$"         { Install-Composer }
                "^phpmyadmin-(.+)$"  { Install-PhpMyAdmin ($component -replace "^phpmyadmin-") }
                "^phpmyadmin$"       { Install-PhpMyAdmin }
                "^git-(.+)$"         { Install-Git ($component -replace "^git-") }
                "^git$"              { Install-Git }
                "^mongodb-(.+)$"     { Install-MongoDB ($component -replace "^mongodb-") }
                "^mongodb$"          { Install-MongoDB }
                "^redis-(.+)$"       { Install-Redis ($component -replace "^redis-") }
                "^redis$"            { Install-Redis }
                "^pgsql-(.+)$"       { Install-PgSQL ($component -replace "^pgsql-") }
                "^pgsql$"            { Install-PgSQL }
                "^mailhog-(.+)$"     { Install-MailHog ($component -replace "^mailhog-") }
                "^mailhog$"          { Install-MailHog }
                "^elasticsearch-(.+)$" { Install-Elasticsearch ($component -replace "^elasticsearch-") }
                "^elasticsearch$"    { Install-Elasticsearch }
                "^memcached-(.+)$"   { Install-Memcached ($component -replace "^memcached-") }
                "^memcached$"        { Install-Memcached }
                "^docker-(.+)$"      { Install-Docker ($component -replace "^docker-") }
                "^docker$"           { Install-Docker }
                "^yarn-(.+)$"        { Install-Yarn ($component -replace "^yarn-") }
                "^yarn$"             { Install-Yarn }
                "^pnpm-(.+)$"        { Install-Pnpm ($component -replace "^pnpm-") }
                "^pnpm$"             { Install-Pnpm }
                "^wpcli-(.+)$"       { Install-WPCLI ($component -replace "^wpcli-") }
                "^wpcli$"            { Install-WPCLI }
                "^adminer-(.+)$"     { Install-Adminer ($component -replace "^adminer-") }
                "^adminer$"          { Install-Adminer }
                "^poetry-(.+)$"      { Install-Poetry ($component -replace "^poetry-") }
                "^poetry$"           { Install-Poetry }
                "^ruby-(.+)$"        { Install-Ruby ($component -replace "^ruby-") }
                "^ruby$"             { Install-Ruby }
                "^go-(.+)$"          { Install-Go ($component -replace "^go-") }
                "^go$"               { Install-Go }
                "^certbot-(.+)$"     { Install-Certbot ($component -replace "^certbot-") }
                "^certbot$"          { Install-Certbot }
                default              { Write-Host "Componente desconhecido: $component" }
            }
        }
        Add-BinDirsToPath
    }
    "path" {
        Add-BinDirsToPath
    }
    "uninstall" {
        foreach ($component in $Args) {
            switch -Regex ($component) {
                "^php-(.+)$"         { Uninstall-PHP ($component -replace "^php-") }
                "^php$"              { Uninstall-PHP }
                "^nginx$"            { Uninstall-Nginx }
                "^mysql-(.+)$"       { Uninstall-MySQL ($component -replace "^mysql-") }
                "^mysql$"            { Uninstall-MySQL }
                "^nodejs-(.+)$"      { Uninstall-NodeJS ($component -replace "^nodejs-") }
                "^(nodejs|node)$"    { Uninstall-NodeJS }
                "^python-(.+)$"      { Uninstall-Python ($component -replace "^python-") }
                "^python$"           { Uninstall-Python }
                "^composer-(.+)$"    { Uninstall-Composer ($component -replace "^composer-") }
                "^composer$"         { Uninstall-Composer }
                "^phpmyadmin-(.+)$"  { Uninstall-PhpMyAdmin ($component -replace "^phpmyadmin-") }
                "^phpmyadmin$"       { Uninstall-PhpMyAdmin }
                "^git-(.+)$"         { Uninstall-Git ($component -replace "^git-") }
                "^git$"              { Uninstall-Git }
                "^mongodb-(.+)$"     { Uninstall-MongoDB ($component -replace "^mongodb-") }
                "^mongodb$"          { Uninstall-MongoDB }
                "^redis-(.+)$"       { Uninstall-Redis ($component -replace "^redis-") }
                "^redis$"            { Uninstall-Redis }
                "^pgsql-(.+)$"       { Uninstall-PgSQL ($component -replace "^pgsql-") }
                "^pgsql$"            { Uninstall-PgSQL }
                "^mailhog-(.+)$"     { Uninstall-MailHog ($component -replace "^mailhog-") }
                "^mailhog$"          { Uninstall-MailHog }
                "^elasticsearch-(.+)$" { Uninstall-Elasticsearch ($component -replace "^elasticsearch-") }
                "^elasticsearch$"    { Uninstall-Elasticsearch }
                "^memcached-(.+)$"   { Uninstall-Memcached ($component -replace "^memcached-") }
                "^memcached$"        { Uninstall-Memcached }
                "^docker-(.+)$"      { Uninstall-Docker ($component -replace "^docker-") }
                "^docker$"           { Uninstall-Docker }
                "^yarn-(.+)$"        { Uninstall-Yarn ($component -replace "^yarn-") }
                "^yarn$"             { Uninstall-Yarn }
                "^pnpm-(.+)$"        { Uninstall-Pnpm ($component -replace "^pnpm-") }
                "^pnpm$"             { Uninstall-Pnpm }
                "^wpcli-(.+)$"       { Uninstall-WPCLI ($component -replace "^wpcli-") }
                "^wpcli$"            { Uninstall-WPCLI }
                "^adminer-(.+)$"     { Uninstall-Adminer ($component -replace "^adminer-") }
                "^adminer$"          { Uninstall-Adminer }
                "^poetry-(.+)$"      { Uninstall-Poetry ($component -replace "^poetry-") }
                "^poetry$"           { Uninstall-Poetry }
                "^ruby-(.+)$"        { Uninstall-Ruby ($component -replace "^ruby-") }
                "^ruby$"             { Uninstall-Ruby }
                "^go-(.+)$"          { Uninstall-Go ($component -replace "^go-") }
                "^go$"               { Uninstall-Go }
                "^certbot-(.+)$"     { Uninstall-Certbot ($component -replace "^certbot-") }
                "^certbot$"          { Uninstall-Certbot }
                default              { Write-Host "Componente desconhecido: $component" }
            }
        }
        Write-Host "Uninstall finalizado."
    }
    "start" {
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 start <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            ForEach-Version "nginx" { param($v) & $PSCommandPath start nginx $v }
            ForEach-Version "php"   { param($v) & $PSCommandPath start php $v }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 start <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Start-Component $target $version
    }
    "stop" {
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 stop <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            ForEach-Version "nginx" { param($v) & $PSCommandPath stop nginx $v }
            ForEach-Version "php"   { param($v) & $PSCommandPath stop php $v }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 stop <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Stop-Component $target $version
    }
    "restart" {
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 restart <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            ForEach-Version "nginx" { param($v) & $PSCommandPath restart nginx $v }
            ForEach-Version "php"   { param($v) & $PSCommandPath restart php $v }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 restart <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Stop-Component $target $version
        Start-Sleep -Seconds 1
        Start-Component $target $version
    }
    "status" {
        Status-All
    }
    "test" {
        Test-All
    }
    "deps" {
        Deps-Check
    }
    "update" {
        foreach ($component in $Args) {
            Update-Component $component
        }
    }
    "alias" {
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 alias <componente> <versão>"
            exit 1
        }
        Alias-Component $Args[0] $Args[1]
    }
    "self-update" {
        # Atualiza o DevStackSetup via git pull ou cópia do repositório
        $repoDir = $PSScriptRoot
        if (Test-Path (Join-Path $repoDir ".git")) {
            Write-Info "Atualizando via git pull..."
            try {
                Push-Location $repoDir
                git pull
                Pop-Location
                Write-Info "DevStackSetup atualizado com sucesso."
            } catch {
                Write-ErrorMsg "Erro ao atualizar via git: $_"
            }
        } else {
            Write-WarningMsg "Não é um repositório git. Atualize manualmente copiando os arquivos do repositório."
        }
    }
    "clean" {
        # Limpa arquivos temporários e logs
        $logDir = Join-Path $baseDir "logs"
        $tmpDir = Join-Path $baseDir "tmp"
        $logFile = Join-Path $baseDir "devstack.log"
        $count = 0
        if (Test-Path $logFile) { Remove-Item $logFile -Force; $count++ }
        if (Test-Path $logDir) { Remove-Item $logDir -Recurse -Force; $count++ }
        if (Test-Path $tmpDir) { Remove-Item $tmpDir -Recurse -Force; $count++ }
        Write-Info "Limpeza concluída. ($count itens removidos)"
    }
    "backup" {
        # Backup dos diretórios de configuração e logs
        $backupDir = Join-Path $baseDir ("backup-" + (Get-Date -Format "yyyyMMdd-HHmmss"))
        $toBackup = @("configs", "devstack.log")
        foreach ($item in $toBackup) {
            $src = Join-Path $baseDir $item
            if (Test-Path $src) {
                Copy-Item $src -Destination $backupDir -Recurse -Force
            }
        }
        Write-Info "Backup criado em $backupDir"
    }
    "logs" {
        # Exibe as últimas 50 linhas do log principal
        $logFile = Join-Path $baseDir "devstack.log"
        if (Test-Path $logFile) {
            Write-Host "Últimas 50 linhas de $logFile:"
            Get-Content $logFile -Tail 50
        } else {
            Write-WarningMsg "Arquivo de log não encontrado."
        }
    }
    "enable" {
        # Ativa um serviço (Windows Service) relacionado ao DevStack
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 enable <serviço>"; exit 1 }
        $svc = $Args[0]
        try {
            Start-Service -Name $svc
            Write-Info "Serviço $svc ativado."
        } catch {
            Write-ErrorMsg "Erro ao ativar serviço $svc: $_"
        }
    }
    "disable" {
        # Desativa um serviço (Windows Service) relacionado ao DevStack
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 disable <serviço>"; exit 1 }
        $svc = $Args[0]
        try {
            Stop-Service -Name $svc
            Write-Info "Serviço $svc desativado."
        } catch {
            Write-ErrorMsg "Erro ao desativar serviço $svc: $_"
        }
    }
    "config" {
        # Abre o diretório de configuração para edição
        $configDir = Join-Path $baseDir "configs"
        if (Test-Path $configDir) {
            Invoke-Item $configDir
            Write-Info "Diretório de configuração aberto."
        } else {
            Write-WarningMsg "Diretório de configuração não encontrado."
        }
    }
    "reset" {
        # Remove e reinstala uma ferramenta
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 reset <componente>"; exit 1 }
        $comp = $Args[0]
        Write-Info "Resetando $comp..."
        & $PSCommandPath uninstall $comp
        & $PSCommandPath install $comp
        Write-Info "$comp resetado."
    }
    "proxy" {
        # Gerencia variáveis de ambiente de proxy
        if ($Args.Count -eq 0) {
            Write-Host "Proxy atual: $env:HTTP_PROXY"
            return
        }
        switch ($Args[0]) {
            "set" {
                if ($Args.Count -lt 2) { Write-Host "Uso: setup.ps1 proxy set <url>"; exit 1 }
                $env:HTTP_PROXY = $Args[1]
                $env:HTTPS_PROXY = $Args[1]
                Write-Info "Proxy definido para $($Args[1])"
            }
            "unset" {
                Remove-Item Env:HTTP_PROXY -ErrorAction SilentlyContinue
                Remove-Item Env:HTTPS_PROXY -ErrorAction SilentlyContinue
                Write-Info "Proxy removido."
            }
            default {
                Write-Host "Uso: setup.ps1 proxy [set <url>|unset|show]"
            }
        }
    }
    "ssl" {
        # Gera certificado SSL autoassinado para um domínio
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 ssl <dominio>"; exit 1 }
        $domain = $Args[0]
        $sslDir = Join-Path $baseDir "configs\nginx\ssl"
        if (-not (Test-Path $sslDir)) { New-Item -ItemType Directory -Path $sslDir | Out-Null }
        $crt = Join-Path $sslDir "$domain.crt"
        $key = Join-Path $sslDir "$domain.key"
        $openssl = "openssl"
        if (-not (Get-Command $openssl -ErrorAction SilentlyContinue)) {
            Write-ErrorMsg "OpenSSL não encontrado no PATH. Instale para usar este comando."
            exit 1
        }
        & $openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout $key -out $crt -subj "/CN=$domain"
        Write-Info "Certificado gerado: $crt, $key"
    }
    "db" {
        # Gerenciamento básico de bancos de dados (MySQL, PostgreSQL, MongoDB)
        if ($Args.Count -lt 2) { Write-Host "Uso: setup.ps1 db <mysql|pgsql|mongo> <comando> [args...]"; exit 1 }
        $db = $Args[0].ToLower()
        $cmd = $Args[1].ToLower()
        switch ($db) {
            "mysql" {
                $mysqlExe = Get-ChildItem $mysqlDir -Recurse -Filter "mysql.exe" | Select-Object -First 1
                if (-not $mysqlExe) { Write-ErrorMsg "mysql.exe não encontrado."; return }
                switch ($cmd) {
                    "list" { & $mysqlExe.FullName -e "SHOW DATABASES;" }
                    "create" { & $mysqlExe.FullName -e "CREATE DATABASE $($Args[2]);" }
                    "drop" { & $mysqlExe.FullName -e "DROP DATABASE $($Args[2]);" }
                    default { Write-Host "Comando db mysql desconhecido." }
                }
            }
            "pgsql" {
                $psqlExe = Get-ChildItem $pgsqlDir -Recurse -Filter "psql.exe" | Select-Object -First 1
                if (-not $psqlExe) { Write-ErrorMsg "psql.exe não encontrado."; return }
                switch ($cmd) {
                    "list" { & $psqlExe.FullName -c "\l" }
                    "create" { & $psqlExe.FullName -c "CREATE DATABASE $($Args[2]);" }
                    "drop" { & $psqlExe.FullName -c "DROP DATABASE $($Args[2]);" }
                    default { Write-Host "Comando db pgsql desconhecido." }
                }
            }
            "mongo" {
                $mongoExe = Get-ChildItem $mongoDir -Recurse -Filter "mongo.exe" | Select-Object -First 1
                if (-not $mongoExe) { Write-ErrorMsg "mongo.exe não encontrado."; return }
                switch ($cmd) {
                    "list" { & $mongoExe.FullName --eval "db.adminCommand('listDatabases')" }
                    "create" { & $mongoExe.FullName --eval "db.getSiblingDB('$($Args[2])')" }
                    "drop" { & $mongoExe.FullName --eval "db.getSiblingDB('$($Args[2])').dropDatabase()" }
                    default { Write-Host "Comando db mongo desconhecido." }
                }
            }
            default { Write-Host "Banco de dados não suportado: $db" }
        }
    }
    "service" {
        # Lista serviços DevStack (Windows Services)
        $services = Get-Service | Where-Object { $_.DisplayName -like '*devstack*' -or $_.ServiceType -eq 'Win32OwnProcess' }
        if ($services) {
            $services | Format-Table Name, Status, DisplayName
        } else {
            Write-Host "Nenhum serviço DevStack encontrado."
        }
    }
    "doctor" {
        # Diagnóstico do ambiente DevStack
        Write-Host "Diagnóstico do ambiente DevStack:"
        $checks = @(
            @{ name = "PHP"; path = $phpDir },
            @{ name = "Nginx"; path = $nginxDir },
            @{ name = "MySQL"; path = $mysqlDir },
            @{ name = "Node.js"; path = $nodeDir },
            @{ name = "Python"; path = $pythonDir },
            @{ name = "Composer"; path = $composerDir },
            @{ name = "Git"; path = $baseDir },
            @{ name = "MongoDB"; path = $mongoDir },
            @{ name = "Redis"; path = $redisDir },
            @{ name = "PgSQL"; path = $pgsqlDir },
            @{ name = "MailHog"; path = $mailhogDir },
            @{ name = "Elasticsearch"; path = $elasticDir },
            @{ name = "Memcached"; path = $memcachedDir },
            @{ name = "Docker"; path = $dockerDir },
            @{ name = "Yarn"; path = $yarnDir },
            @{ name = "pnpm"; path = $pnpmDir },
            @{ name = "WP-CLI"; path = $wpcliDir },
            @{ name = "Adminer"; path = $adminerDir },
            @{ name = "Poetry"; path = $poetryDir },
            @{ name = "Ruby"; path = $rubyDir },
            @{ name = "Go"; path = $goDir },
            @{ name = "Certbot"; path = $certbotDir }
        )
        foreach ($c in $checks) {
            if (Test-Path $c.path) {
                Write-Host ("{0,-12}: OK" -f $c.name) -ForegroundColor Green
            } else {
                Write-Host ("{0,-12}: NÃO INSTALADO" -f $c.name) -ForegroundColor Red
            }
        }
        Write-Host "PATH: $env:Path"
        Write-Host "Usuário: $env:USERNAME"
        Write-Host "Sistema: $env:OS"
    }
    "global" {
        $devstackDir = $PSScriptRoot
        $currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
        if ($currentPath -notlike "*$devstackDir*") {
            [Environment]::SetEnvironmentVariable("Path", "$currentPath;$devstackDir", "User")
            Write-Host "Diretório $devstackDir adicionado ao PATH do usuário." -ForegroundColor Green
        } else {
            Write-Host "Diretório $devstackDir já está no PATH do usuário." -ForegroundColor Yellow
        }
        $profilePath = $PROFILE
        $aliasLine = "Set-Alias devstack '$devstackDir\setup.ps1'"
        if (-not (Test-Path $profilePath)) { New-Item -ItemType File -Path $profilePath -Force | Out-Null }
        $profileContent = Get-Content $profilePath -Raw
        if ($profileContent -notmatch "devstack.*setup.ps1") {
            Add-Content $profilePath $aliasLine
            Write-Host "Alias 'devstack' adicionado ao seu perfil do PowerShell." -ForegroundColor Green
        } else {
            Write-Host "Alias 'devstack' já existe no seu perfil do PowerShell." -ForegroundColor Yellow
        }
        Write-Host "Agora você pode rodar 'devstack' ou 'setup.ps1' de qualquer lugar no terminal." -ForegroundColor Cyan
    }
    default {
        Write-Host "Comando desconhecido: $Command"
    }
}