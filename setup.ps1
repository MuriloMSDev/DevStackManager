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

function Center-Text($text, $width) {
    $pad = [Math]::Max(0, $width - $text.Length)
    $padLeft = [Math]::Floor($pad / 2)
    $padRight = $pad - $padLeft
    return (' ' * $padLeft) + $text + (' ' * $padRight)
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
        Write-Log "Comando executado: list $($Args -join ' ')"
        if ($Args.Count -eq 0) {
            Write-Host "Uso: setup.ps1 list <php|nodejs|python|--installed|-i>"
            exit 1
        }
        $firstArg = $Args[0].Trim()
        if ($firstArg -eq '--installed') {
            List-InstalledVersions
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
        Write-Log "Comando executado: site $($Args -join ' ')"
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
        Write-Log "Comando executado: install $($Args -join ' ')"
        Install-Commands @Args
    }
    "path" {
        Write-Log "Comando executado: path $($Args -join ' ')"
        Add-BinDirsToPath
    }
    "uninstall" {
        Write-Log "Comando executado: uninstall $($Args -join ' ')"
        Uninstall-Commands @Args
    }
    "start" {
        Write-Log "Comando executado: start $($Args -join ' ')"
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 start <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            if (Test-Path $nginxDir) {
                ForEach-Version "nginx" { param($v) & $PSCommandPath -Command start -Args @("nginx", $v) }
            } else {
                Write-WarningMsg "Diretório do nginx não encontrado. Ignorando."
            }
            if (Test-Path $phpDir) {
                ForEach-Version "php" { param($v) & $PSCommandPath -Command start -Args @("php", $v) }
            } else {
                Write-WarningMsg "Diretório do PHP não encontrado. Ignorando."
            }
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
        Write-Log "Comando executado: stop $($Args -join ' ')"
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 stop <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            if (Test-Path $nginxDir) {
                ForEach-Version "nginx" { param($v) & $PSCommandPath -Command stop -Args @("nginx", $v) }
            } else {
                Write-WarningMsg "Diretório do nginx não encontrado. Ignorando."
            }
            if (Test-Path $phpDir) {
                ForEach-Version "php" { param($v) & $PSCommandPath -Command stop -Args @("php", $v) }
            } else {
                Write-WarningMsg "Diretório do PHP não encontrado. Ignorando."
            }
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
        Write-Log "Comando executado: restart $($Args -join ' ')"
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 restart <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            if (Test-Path $nginxDir) {
                ForEach-Version "nginx" { param($v) & $PSCommandPath -Command restart -Args @("nginx", $v) }
            } else {
                Write-WarningMsg "Diretório do nginx não encontrado. Ignorando."
            }
            if (Test-Path $phpDir) {
                ForEach-Version "php" { param($v) & $PSCommandPath -Command restart -Args @("php", $v) }
            } else {
                Write-WarningMsg "Diretório do PHP não encontrado. Ignorando."
            }
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
        Write-Log "Comando executado: status $($Args -join ' ')"
        Status-All
    }
    "test" {
        Write-Log "Comando executado: test $($Args -join ' ')"
        Test-All
    }
    "deps" {
        Write-Log "Comando executado: deps $($Args -join ' ')"
        Deps-Check
    }
    "update" {
        Write-Log "Comando executado: update $($Args -join ' ')"
        foreach ($component in $Args) {
            Update-Component $component
        }
    }
    "alias" {
        Write-Log "Comando executado: alias $($Args -join ' ')"
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 alias <componente> <versão>"
            exit 1
        }
        Alias-Component $Args[0] $Args[1]
    }
    "self-update" {
        Write-Log "Comando executado: self-update $($Args -join ' ')"
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
        Write-Log "Comando executado: clean $($Args -join ' ')"
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
        Write-Log "Comando executado: backup $($Args -join ' ')"
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
        Write-Log "Comando executado: logs $($Args -join ' ')"
        # Exibe as últimas 50 linhas do log principal
        $logFile = Join-Path $baseDir "devstack.log"
        if (Test-Path $logFile) {
            Write-Host "Últimas 50 linhas de $($logFile):"
            Get-Content $logFile -Tail 50
        } else {
            Write-WarningMsg "Arquivo de log não encontrado."
        }
    }
    "enable" {
        Write-Log "Comando executado: enable $($Args -join ' ')"
        # Ativa um serviço (Windows Service) relacionado ao DevStack
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 enable <serviço>"; exit 1 }
        $svc = $Args[0]
        try {
            Start-Service -Name $svc
            Write-Info "Serviço $($svc) ativado."
        } catch {
            Write-ErrorMsg "Erro ao ativar serviço $($svc): $_"
        }
    }
    "disable" {
        Write-Log "Comando executado: disable $($Args -join ' ')"
        # Desativa um serviço (Windows Service) relacionado ao DevStack
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 disable <serviço>"; exit 1 }
        $svc = $Args[0]
        try {
            Stop-Service -Name $svc
            Write-Info "Serviço $($svc) desativado."
        } catch {
            Write-ErrorMsg "Erro ao desativar serviço $($svc): $_"
        }
    }
    "config" {
        Write-Log "Comando executado: config $($Args -join ' ')"
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
        Write-Log "Comando executado: reset $($Args -join ' ')"
        # Remove e reinstala uma ferramenta
        if ($Args.Count -lt 1) { Write-Host "Uso: setup.ps1 reset <componente>"; exit 1 }
        $comp = $Args[0]
        Write-Info "Resetando $comp..."
        & $PSCommandPath uninstall $comp
        & $PSCommandPath install $comp
        Write-Info "$comp resetado."
    }
    "proxy" {
        Write-Log "Comando executado: proxy $($Args -join ' ')"
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
        Write-Log "Comando executado: ssl $($Args -join ' ')"
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
        Write-Info "Certificado gerado: $($crt), $($key)"
    }
    "db" {
        Write-Log "Comando executado: db $($Args -join ' ')"
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
            default { Write-Host "Banco de dados não suportado: $($db)" }
        }
    }
    "service" {
        Write-Log "Comando executado: service $($Args -join ' ')"
        # Lista serviços DevStack (Windows Services)
        $services = Get-Service | Where-Object { $_.DisplayName -like '*devstack*' -or $_.ServiceType -eq 'Win32OwnProcess' }
        if ($services) {
            $services | Format-Table Name, Status, DisplayName
        } else {
            Write-Host "Nenhum serviço DevStack encontrado."
        }
    }
    "doctor" {
        Write-Log "Comando executado: doctor $($Args -join ' ')"
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
        # Tabela Status
        $table = @()
        foreach ($c in $checks) {
            $status = if (Test-Path $c.path) { 'OK' } else { 'NÃO INSTALADO' }
            $table += [PSCustomObject]@{ Ferramenta = $c.name; Status = $status }
        }
        $col1 = 15; $col2 = 20
        $header = ('_' * ($col1 + $col2 + 3))
        Write-Host $header
        Write-Host ("|{0}|{1}|" -f (Center-Text 'Ferramenta' $col1), (Center-Text 'Status' $col2))
        Write-Host ("|" + ('-' * $col1) + "+" + ('-' * $col2) + "|")
        foreach ($row in $table) {
            $color = if ($row.Status -eq 'OK') { 'Green' } else { 'Red' }
            $ferramenta = Center-Text $row.Ferramenta $col1
            $status = Center-Text $row.Status $col2
            Write-Host -NoNewline "|"
            Write-Host -NoNewline $ferramenta -ForegroundColor $color
            Write-Host -NoNewline "|"
            Write-Host -NoNewline $status -ForegroundColor $color
            Write-Host "|"
        }
        Write-Host ("¯" * ($col1 + $col2 + 3))
        # Tabela PATH
        $pathList = $env:Path -split ';'
        $maxPathLen = ($pathList | Measure-Object -Property Length -Maximum).Maximum
        $headerPath = ('_' * ($maxPathLen + 4))
        Write-Host $headerPath
        Write-Host ("| {0,-$maxPathLen} |" -f 'PATH')
        Write-Host ("|" + ('-' * ($maxPathLen + 2)) + "|")
        foreach ($p in $pathList) {
            if (![string]::IsNullOrWhiteSpace($p)) {
                Write-Host ("| {0,-$maxPathLen} |" -f $p) -ForegroundColor DarkGray
            }
        }
        Write-Host ("¯" * ($maxPathLen + 4))
        # Tabela Usuário
        $user = $env:USERNAME
        $userLen = $user.Length
        $colUser = [Math]::Max(8, $userLen)
        $headerUser = ('_' * ($colUser + 4))
        Write-Host $headerUser
        Write-Host ("| {0,-$colUser} |" -f 'Usuário')
        Write-Host ("|" + ('-' * ($colUser + 2)) + "|")
        Write-Host -NoNewline "| "
        Write-Host -NoNewline ("{0,-$colUser}" -f $user) -ForegroundColor Cyan
        Write-Host " |"
        Write-Host ("¯" * ($colUser + 4))
        # Tabela Sistema
        $os = $env:OS
        $osLen = $os.Length
        $colOS = [Math]::Max(8, $osLen)
        $headerOS = ('_' * ($colOS + 4))
        Write-Host $headerOS
        Write-Host ("| {0,-$colOS} |" -f 'Sistema')
        Write-Host ("|" + ('-' * ($colOS + 2)) + "|")
        Write-Host -NoNewline "| "
        Write-Host -NoNewline ("{0,-$colOS}" -f $os) -ForegroundColor Cyan
        Write-Host " |"
        Write-Host ("¯" * ($colOS + 4))
    }
    "global" {
        Write-Log "Comando executado: global $($Args -join ' ')"
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