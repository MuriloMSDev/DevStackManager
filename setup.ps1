param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet("install", "site", "uninstall", "path", "list")]
    [string]$Command,

    [Parameter(Position=1)]
    [string[]]$Args
)

$baseDir = "$env:USERPROFILE\devstack"
$nginxDir = "$baseDir\nginx"
$phpDir = "$baseDir\php"
$mysqlDir = "$baseDir\mysql"
$nodeDir = "$baseDir\nodejs"
$pythonDir = "$baseDir\python"
$composerDir = "$baseDir\composer"
$nginxSitesDir = "configs\nginx\sites"

function Get-LatestNginxVersion {
    $page = Invoke-WebRequest -Uri "https://nginx.org/en/download.html"
    if ($page.Content -match "nginx-([\d\.]+)\.zip") {
        return $matches[1]
    } else {
        throw "Não foi possível obter a última versão do Nginx."
    }
}

function Get-LatestPHPVersion {
    $url = "https://windows.php.net/downloads/releases/"
    $page = Invoke-WebRequest -Uri $url
    $matches = [regex]::Matches($page.Content, "php-([\d\.]+)-Win32-[^-]+-x64\.zip")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value }
    if ($versions.Count -gt 0) {
        return ($versions | Sort-Object {[version]$_} -Descending | Select-Object -First 1)
    }
    throw "Não foi possível obter a última versão do PHP."
}

function Get-LatestNodeVersion {
    $json = Invoke-RestMethod -Uri "https://nodejs.org/dist/index.json"
    return $json | Where-Object { $_.lts } | Select-Object -First 1 -ExpandProperty version -replace "v"
}

function Get-LatestPythonVersion {
    $page = Invoke-WebRequest -Uri "https://www.python.org/downloads/windows/"
    if ($page.Content -match "Latest Python 3 Release - Python ([\d\.]+)") {
        return $matches[1]
    } else {
        throw "Não foi possível obter a última versão do Python."
    }
}

function Get-LatestComposerVersion {
    $json = Invoke-RestMethod -Uri "https://getcomposer.org/versions"
    return $json.stable[0].version
}

function Get-LatestPhpMyAdminVersion {
    $json = Invoke-RestMethod -Uri "https://www.phpmyadmin.net/home_page/version.json"
    return $json.version
}

function Install-Nginx {
    $latestNginxVersion = Get-LatestNginxVersion
    $nginxExtractedDir = "$nginxDir\nginx-$latestNginxVersion"
    $rollback = $false
    try {
        if (Test-Path $nginxExtractedDir) {
            Write-Host "Nginx $latestNginxVersion já está instalado."
        } else {
            if (-not (Test-Path $nginxDir)) {
                New-Item -ItemType Directory -Force -Path $nginxDir | Out-Null
            }
            Write-Host "Baixando Nginx $latestNginxVersion..."
            $nginxZip = "$nginxDir\nginx-$latestNginxVersion.zip"
            Invoke-WebRequest -Uri "https://nginx.org/download/nginx-$latestNginxVersion.zip" -OutFile $nginxZip
            Expand-Archive $nginxZip -DestinationPath $nginxDir -Force
            Remove-Item $nginxZip
            Write-Host "Nginx $latestNginxVersion instalado."
            $rollback = $true
        }
        $nginxExe = Join-Path $nginxExtractedDir "nginx.exe"
        $nginxVersionExe = Join-Path $nginxExtractedDir "nginx-$latestNginxVersion.exe"
        if (Test-Path $nginxExe) {
            Rename-Item -Path $nginxExe -NewName "nginx-$latestNginxVersion.exe" -Force
            Write-Host "Renomeado nginx.exe para nginx-$latestNginxVersion.exe"
        }
    } catch {
        Write-Host "Erro ao instalar Nginx $($latestNginxVersion): $($_)"
        if ($rollback) { Rollback-Install $nginxDir }
        throw
    }
}

function Rollback-Install {
    param(
        [string]$DirToRemove
    )
    if ($DirToRemove -and (Test-Path $DirToRemove)) {
        Remove-Item -Recurse -Force $DirToRemove
        Remove-FromPath @($DirToRemove)
        Write-Host "Rollback: pasta $DirToRemove removida."
    }
}

function Install-PHP {
    param($version)
    if (-not $version) {
        $version = Get-LatestPHPVersion
    }
    $phpSubDir = "$phpDir\php-$version"
    $rollback = $false
    try {
        if (Test-Path $phpSubDir) {
            Write-Host "PHP $version já está instalado."
        } else {
            if (-not (Test-Path $phpDir)) {
                New-Item -ItemType Directory -Force -Path $phpDir | Out-Null
            }
            Write-Host "Baixando PHP $version..."
            $phpZip = "$phpDir\php-$version.zip"
            $urls = @(
                "https://windows.php.net/downloads/releases/",
                "https://windows.php.net/downloads/releases/archives/"
            )
            $phpUrl = $null
            foreach ($baseUrl in $urls) {
                $page = Invoke-WebRequest -Uri $baseUrl
                $match = [regex]::Match($page.Content, "php-$version-Win32-[^-]+-x64\.zip")
                if ($match.Success) {
                    $phpUrl = $baseUrl + $match.Value
                    break
                }
            }
            if (-not $phpUrl) {
                throw "Não foi encontrado o arquivo .zip para PHP $version nas releases oficiais."
            }
            Invoke-WebRequest -Uri $phpUrl -OutFile $phpZip
            Expand-Archive $phpZip -DestinationPath $phpSubDir -Force
            Remove-Item $phpZip
            Write-Host "PHP $version instalado."
            $rollback = $true
        }
        $phpExe = Join-Path $phpSubDir "php.exe"
        $phpVersionExe = Join-Path $phpSubDir "php-$version.exe"
        if (Test-Path $phpExe) {
            Rename-Item -Path $phpExe -NewName "php-$version.exe" -Force
            Write-Host "Renomeado php.exe para php-$version.exe"
        }
        $localIniSrc = "configs\php\local.ini"
        $localIniDst = Join-Path $phpSubDir "local.ini"
        if (Test-Path $localIniSrc) {
            Copy-Item $localIniSrc $localIniDst -Force
            Write-Host "Arquivo local.ini copiado para $phpSubDir"
        } else {
            Write-Host "Arquivo configs\php\local.ini não encontrado. Pulei a cópia do local.ini."
        }
    } catch {
        Write-Host "Erro ao instalar PHP $($version): $($_)"
        if ($rollback) { Rollback-Install $phpSubDir }
        throw
    }
}

function Install-MySQL {
    param($version = "8.0.36")
    $mysqlExtractedDir = "$mysqlDir\mysql-$version-winx64"
    $rollback = $false
    try {
        if (Test-Path $mysqlExtractedDir) {
            Write-Host "MySQL $version já está instalado."
        } else {
            Write-Host "Baixando MySQL $version..."
            $mysqlZip = "$mysqlDir\mysql-$version-winx64.zip"
            Invoke-WebRequest -Uri "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-$version-winx64.zip" -OutFile $mysqlZip
            Expand-Archive $mysqlZip -DestinationPath $mysqlDir -Force
            Remove-Item $mysqlZip
            Write-Host "MySQL $version instalado."
            $rollback = $true
        }
        $mysqlBinDir = Join-Path $mysqlExtractedDir "bin"
        $mysqldExe = Join-Path $mysqlBinDir "mysqld.exe"
        $mysqldVersionExe = Join-Path $mysqlBinDir "mysqld-$version.exe"
        if (Test-Path $mysqldExe) {
            Rename-Item -Path $mysqldExe -NewName "mysqld-$version.exe" -Force
            Write-Host "Renomeado mysqld.exe para mysqld-$version.exe"
        }
    } catch {
        Write-Host "Erro ao instalar MySQL $($version): $($_)"
        if ($rollback) { Rollback-Install $mysqlExtractedDir }
        throw
    }
}

function Install-NodeJS {
    param($version)
    if (-not $version) {
        $version = Get-LatestNodeVersion
    }
    $nodeSubDir = "$nodeDir\node-v$version-win-x64"
    $rollback = $false
    try {
        if (Test-Path $nodeSubDir) {
            Write-Host "Node.js $version já está instalado."
        } else {
            Write-Host "Baixando Node.js $version..."
            $nodeZip = "$nodeDir\node-v$version-win-x64.zip"
            $nodeUrl = "https://nodejs.org/dist/v$version/node-v$version-win-x64.zip"
            Invoke-WebRequest -Uri $nodeUrl -OutFile $nodeZip
            Expand-Archive $nodeZip -DestinationPath $nodeDir -Force
            Remove-Item $nodeZip
            Write-Host "Node.js $version instalado."
            $rollback = $true
        }
        $nodeExe = Join-Path $nodeSubDir "node.exe"
        $nodeVersionExe = Join-Path $nodeSubDir "node-$version.exe"
        if (Test-Path $nodeExe) {
            Rename-Item -Path $nodeExe -NewName "node-$version.exe" -Force
            Write-Host "Renomeado node.exe para node-$version.exe"
        }
    } catch {
        Write-Host "Erro ao instalar Node.js $($version): $($_)"
        if ($rollback) { Rollback-Install $nodeSubDir }
        throw
    }
}

function Install-Python {
    param($version)
    if (-not $version) {
        $version = Get-LatestPythonVersion
    }
    $pySubDir = "$pythonDir\python-$version"
    $rollback = $false
    try {
        if (Test-Path $pySubDir) {
            Write-Host "Python $version já está instalado."
        } else {
            Write-Host "Baixando Python $version..."
            $pyExe = "$pythonDir\python-$version-amd64.exe"
            $pyUrl = "https://www.python.org/ftp/python/$version/python-$version-amd64.exe"
            Invoke-WebRequest -Uri $pyUrl -OutFile $pyExe
            New-Item -ItemType Directory -Force -Path $pySubDir | Out-Null
            Write-Host "Instalando Python $version em modo silencioso..."
            Start-Process -FilePath $pyExe -ArgumentList "/quiet InstallAllUsers=0 TargetDir=$pySubDir" -Wait
            Remove-Item $pyExe
            Write-Host "Python $version instalado."
            $rollback = $true
        }
        $pythonExe = Join-Path $pySubDir "python.exe"
        $pythonVersionExe = Join-Path $pySubDir "python-$version.exe"
        if (Test-Path $pythonExe) {
            Rename-Item -Path $pythonExe -NewName "python-$version.exe" -Force
            Write-Host "Renomeado python.exe para python-$version.exe"
        }
    } catch {
        Write-Host "Erro ao instalar Python $($version): $($_)"
        if ($rollback) { Rollback-Install $pySubDir }
        throw
    }
}

function Install-Composer {
    param($version)
    if (-not $version) {
        $version = Get-LatestComposerVersion
    }
    $composerPhar = "$composerDir\composer-$version.phar"
    $rollback = $false
    try {
        if (Test-Path $composerPhar) {
            Write-Host "Composer $version já está instalado."
        } else {
            Write-Host "Baixando Composer $version..."
            New-Item -ItemType Directory -Force -Path $composerDir | Out-Null
            $composerUrl = "https://getcomposer.org/download/$version/composer.phar"
            Invoke-WebRequest -Uri $composerUrl -OutFile $composerPhar
            Write-Host "Composer $version instalado."
            $rollback = $true
        }
    } catch {
        Write-Host "Erro ao instalar Composer $($version): $($_)"
        if ($rollback) { Rollback-Install $composerDir }
        throw
    }
}

function Install-PhpMyAdmin {
    param($version)
    if (-not $version) {
        $version = Get-LatestPhpMyAdminVersion
    }
    $pmaDir = "$baseDir\phpmyadmin-$version"
    $rollback = $false
    try {
        if (Test-Path $pmaDir) {
            Write-Host "phpMyAdmin $version já está instalado."
        } else {
            Write-Host "Baixando phpMyAdmin $version..."
            $pmaZip = "$baseDir\phpmyadmin-$version-all-languages.zip"
            $pmaUrl = "https://files.phpmyadmin.net/phpMyAdmin/$version/phpMyAdmin-$version-all-languages.zip"
            Invoke-WebRequest -Uri $pmaUrl -OutFile $pmaZip
            Expand-Archive $pmaZip -DestinationPath $baseDir -Force
            Rename-Item -Path "$baseDir\phpMyAdmin-$version-all-languages" -NewName "phpmyadmin-$version"
            Remove-Item $pmaZip
            Write-Host "phpMyAdmin $version instalado em $pmaDir."
            $rollback = $true
        }
    } catch {
        Write-Host "Erro ao instalar phpMyAdmin $($version): $($_)"
        if ($rollback) { Rollback-Install $pmaDir }
        throw
    }
}

function Create-NginxSiteConfig {
    param(
        [string]$Domain,
        [string]$Root = "/var/www/$Domain",
        [string]$PhpUpstream = "php-upstream"
    )

    $confPath = "$nginxSitesDir\$Domain.conf"
    $serverName = "$Domain.localhost"

    $template = @"
server {

    listen 80;
    listen [::]:80;

    # For https
    #listen 443 ssl;
    #listen [::]:443 ssl ipv6only=on;
    #ssl_certificate /etc/nginx/ssl/default.crt;
    #ssl_certificate_key /etc/nginx/ssl/default.key;

    server_name $serverName;
    root $Root;
    index index.php index.html index.htm;

    location / {
         try_files \$uri \$uri/ /index.php\$is_args\$args;
    }

    location ~ \.php$ {
        try_files \$uri /index.php =404;
        fastcgi_pass $PhpUpstream;
        fastcgi_index index.php;
        fastcgi_buffers 16 16k;
        fastcgi_buffer_size 32k;
        fastcgi_param SCRIPT_FILENAME \$document_root\$fastcgi_script_name;
        #fixes timeouts
        fastcgi_read_timeout 600;
        include fastcgi_params;
    }

    location ~ /\.ht {
        deny all;
    }

    location /.well-known/acme-challenge/ {
        root /var/www/letsencrypt/;
        log_not_found off;
    }

    location /api {
        rewrite ^/api/(\w+).*$ /api.php?type=\$1 last;
    }

    error_log /var/log/nginx/${Domain}_error.log;
    access_log /var/log/nginx/${Domain}_access.log;
}
"@

    if (!(Test-Path $nginxSitesDir)) {
        New-Item -ItemType Directory -Force -Path $nginxSitesDir | Out-Null
    }
    Set-Content -Path $confPath -Value $template
    Write-Host "Arquivo $confPath criado/configurado com sucesso!"

    $hostsPath = "$env:SystemRoot\System32\drivers\etc\hosts"
    $entry = "127.0.0.1`t$serverName"
    $hostsContent = Get-Content $hostsPath -ErrorAction SilentlyContinue
    if ($hostsContent -notcontains $entry) {
        Add-Content -Path $hostsPath -Value $entry
        Write-Host "Adicionado $serverName ao arquivo hosts."
    } else {
        Write-Host "$serverName já está presente no arquivo hosts."
    }
}

function Add-BinDirsToPath {
    $pathsToAdd = @()

    # PHP
    if (Test-Path $phpDir) {
        Get-ChildItem $phpDir -Directory | ForEach-Object {
            $phpExe = Get-ChildItem $_.FullName -Filter "php-*.exe" -File -ErrorAction SilentlyContinue
            if ($phpExe) {
                $pathsToAdd += $_.FullName
            }
        }
    }

    # Node.js
    if (Test-Path $nodeDir) {
        Get-ChildItem $nodeDir -Directory | ForEach-Object {
            $nodeExe = Get-ChildItem $_.FullName -Filter "node-*.exe" -File -ErrorAction SilentlyContinue
            if ($nodeExe) {
                $pathsToAdd += $_.FullName
            }
        }
    }

    # Python
    if (Test-Path $pythonDir) {
        Get-ChildItem $pythonDir -Directory | ForEach-Object {
            $pythonExe = Get-ChildItem $_.FullName -Filter "python-*.exe" -File -ErrorAction SilentlyContinue
            if ($pythonExe) {
                $pathsToAdd += $_.FullName
            }
        }
    }

    # Nginx
    if (Test-Path $nginxDir) {
        Get-ChildItem $nginxDir -Directory | ForEach-Object {
            $nginxExe = Get-ChildItem $_.FullName -Filter "nginx-*.exe" -File -ErrorAction SilentlyContinue
            if ($nginxExe) {
                $pathsToAdd += $_.FullName
            }
        }
    }

    # Composer (adiciona o diretório do composer.phar)
    if (Test-Path $composerDir) {
        $pathsToAdd += $composerDir
    }

    # MySQL (bin)
    if (Test-Path $mysqlDir) {
        Get-ChildItem $mysqlDir -Directory | ForEach-Object {
            $mysqlBin = Join-Path $_.FullName "bin"
            $mysqldExe = Get-ChildItem $mysqlBin -Filter "mysqld-*.exe" -File -ErrorAction SilentlyContinue
            if ($mysqldExe) {
                $pathsToAdd += $mysqlBin
            }
        }
    }

    $currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $currentPathList = $currentPath -split ';'
    $newPaths = $pathsToAdd | Where-Object { $_ -and ($currentPathList -notcontains $_) }

    if ($newPaths.Count -gt 0) {
        $newPathValue = ($currentPathList + $newPaths) -join ';'
        [Environment]::SetEnvironmentVariable("Path", $newPathValue, "User")
        $env:PATH = $newPathValue
        Write-Host "Os seguintes diretórios foram adicionados ao PATH do usuário:"
        $newPaths | ForEach-Object { Write-Host "  $_" }
        Write-Host "O PATH do terminal atual também foi atualizado."
    } else {
        Write-Host "Nenhum novo diretório foi adicionado ao PATH."
    }
}

function Remove-FromPath {
    param([string[]]$dirsToRemove)
    $currentPath = [Environment]::GetEnvironmentVariable("Path", "User")
    $currentPathList = $currentPath -split ';'
    $newPathList = $currentPathList | Where-Object { $dirsToRemove -notcontains $_ }
    $newPathValue = $newPathList -join ';'
    [Environment]::SetEnvironmentVariable("Path", $newPathValue, "User")
    $env:PATH = $newPathValue
}

function Uninstall-PHP {
    param($version)
    if ($version) {
        $phpSubDir = "$phpDir\php-$version"
        if (Test-Path $phpSubDir) {
            Remove-Item -Recurse -Force $phpSubDir
            Remove-FromPath @($phpSubDir)
            Write-Host "PHP $version removido."
        } else {
            Write-Host "PHP $version não está instalado."
        }
    } else {
        Get-ChildItem $phpDir -Directory | ForEach-Object {
            Remove-Item -Recurse -Force $_.FullName
            Remove-FromPath @($_.FullName)
            Write-Host "PHP removido: $($_.Name)"
        }
    }
}

function Uninstall-MySQL {
    param($version = "8.0.36")
    $mysqlExtractedDir = "$mysqlDir\mysql-$version-winx64"
    $mysqlBin = "$mysqlExtractedDir\bin"
    if (Test-Path $mysqlExtractedDir) {
        Remove-Item -Recurse -Force $mysqlExtractedDir
        Remove-FromPath @($mysqlBin)
        Write-Host "MySQL $version removido."
    } else {
        Write-Host "MySQL $version não está instalado."
    }
}

function Uninstall-NodeJS {
    param($version)
    if ($version) {
        $nodeSubDir = "$nodeDir\node-v$version-win-x64"
        if (Test-Path $nodeSubDir) {
            Remove-Item -Recurse -Force $nodeSubDir
            Remove-FromPath @($nodeSubDir)
            Write-Host "Node.js $version removido."
        } else {
            Write-Host "Node.js $version não está instalado."
        }
    } else {
        Get-ChildItem $nodeDir -Directory | ForEach-Object {
            Remove-Item -Recurse -Force $_.FullName
            Remove-FromPath @($_.FullName)
            Write-Host "Node.js removido: $($_.Name)"
        }
    }
}

function Uninstall-Python {
    param($version)
    if ($version) {
        $pySubDir = "$pythonDir\python-$version"
        if (Test-Path $pySubDir) {
            Remove-Item -Recurse -Force $pySubDir
            Remove-FromPath @($pySubDir)
            Write-Host "Python $version removido."
        } else {
            Write-Host "Python $version não está instalado."
        }
    } else {
        Get-ChildItem $pythonDir -Directory | ForEach-Object {
            Remove-Item -Recurse -Force $_.FullName
            Remove-FromPath @($_.FullName)
            Write-Host "Python removido: $($_.Name)"
        }
    }
}

function Uninstall-Composer {
    param($version)
    if ($version) {
        $composerPhar = "$composerDir\composer-$version.phar"
        if (Test-Path $composerPhar) {
            Remove-Item -Force $composerPhar
            Write-Host "Composer $version removido."
        } else {
            Write-Host "Composer $version não está instalado."
        }
        if ((Get-ChildItem $composerDir).Count -eq 0) {
            Remove-FromPath @($composerDir)
        }
    } else {
        if (Test-Path $composerDir) {
            Remove-Item -Recurse -Force $composerDir
            Remove-FromPath @($composerDir)
            Write-Host "Composer removido."
        } else {
            Write-Host "Composer não está instalado."
        }
    }
}

function Uninstall-Nginx {
    if (Test-Path $nginxDir) {
        Get-ChildItem $nginxDir -Directory | ForEach-Object {
            Remove-Item -Recurse -Force $_.FullName
            Remove-FromPath @($_.FullName)
            Write-Host "Nginx removido: $($_.Name)"
        }
    } else {
        Write-Host "Nginx não está instalado."
    }
}

function Uninstall-PhpMyAdmin {
    param($version)
    if ($version) {
        $pmaDir = "$baseDir\phpmyadmin-$version"
        if (Test-Path $pmaDir) {
            Remove-Item -Recurse -Force $pmaDir
            Write-Host "phpMyAdmin $version removido."
        } else {
            Write-Host "phpMyAdmin $version não está instalado."
        }
    } else {
        Get-ChildItem $baseDir -Directory | Where-Object { $_.Name -like "phpmyadmin-*" } | ForEach-Object {
            Remove-Item -Recurse -Force $_.FullName
            Write-Host "phpMyAdmin removido: $($_.Name)"
        }
    }
}

function List-PHPVersions {
    $urls = @(
        "https://windows.php.net/downloads/releases/",
        "https://windows.php.net/downloads/releases/archives/"
    )
    $allMatches = @()
    foreach ($url in $urls) {
        $page = Invoke-WebRequest -Uri $url
        $matches = [regex]::Matches($page.Content, "php-([\d\.]+)-Win32-[^-]+-x64\.zip")
        $allMatches += $matches | ForEach-Object { $_.Groups[1].Value }
    }
    Write-Host $allMatches
    $versions = $allMatches | Sort-Object -Descending | Get-Unique
    Write-Host "Versões de PHP disponíveis para Windows x64:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-NodeVersions {
    $json = Invoke-RestMethod -Uri "https://nodejs.org/dist/index.json"
    $versions = $json | Select-Object -ExpandProperty version
    Write-Host "Versões de Node.js disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-PythonVersions {
    $page = Invoke-WebRequest -Uri "https://www.python.org/downloads/windows/"
    $matches = [regex]::Matches($page.Content, "Python ([\d\.]+) ")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value }
    $versions = $versions | Sort-Object -Descending | Get-Unique
    Write-Host "Versões de Python disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

if ($Command -eq "list") {
    if ($Args.Count -eq 0) {
        Write-Host "Uso: setup.ps1 list <php|nodejs|python>"
        exit 1
    }
    switch ($Args[0].ToLower()) {
        "php"     { List-PHPVersions }
        "nodejs"  { List-NodeVersions }
        "node"    { List-NodeVersions }
        "python"  { List-PythonVersions }
        default   { Write-Host "Ferramenta desconhecida: $($Args[0])" }
    }
    exit 0
}

if ($Command -eq "site") {
    if ($Args.Count -lt 1) {
        Write-Host "Uso: setup.ps1 site <dominio> [-root <diretorio>] [-php <php-upstream>]"
        exit 1
    }
    $Domain = $Args[0]
    $Root = "/var/www/$Domain"
    $PhpUpstream = "php-upstream"

    for ($i = 1; $i -lt $Args.Count; $i++) {
        switch ($Args[$i]) {
            "-root" {
                $i++; if ($i -lt $Args.Count) { $Root = $Args[$i] }
            }
            "-php" {
                $i++; if ($i -lt $Args.Count) { $PhpUpstream = $Args[$i] }
            }
        }
    }
    Create-NginxSiteConfig -Domain $Domain -Root $Root -PhpUpstream $PhpUpstream
    exit 0
}

if ($Command -eq "install") {
    foreach ($component in $Args) {
        if ($component -like "php-*") {
            $phpVersion = $component -replace "php-"
            Install-PHP $phpVersion
        } elseif ($component -eq "php") {
            Install-PHP
        } elseif ($component -eq "nginx") {
            Install-Nginx
        } elseif ($component -like "mysql-*") {
            $mysqlVersion = $component -replace "mysql-"
            Install-MySQL $mysqlVersion
        } elseif ($component -eq "mysql") {
            Install-MySQL
        } elseif ($component -like "nodejs-*") {
            $nodeVersion = $component -replace "nodejs-"
            Install-NodeJS $nodeVersion
        } elseif ($component -eq "nodejs" -or $component -eq "node") {
            Install-NodeJS
        } elseif ($component -like "python-*") {
            $pyVersion = $component -replace "python-"
            Install-Python $pyVersion
        } elseif ($component -eq "python") {
            Install-Python
        } elseif ($component -like "composer-*") {
            $composerVersion = $component -replace "composer-"
            Install-Composer $composerVersion
        } elseif ($component -eq "composer") {
            Install-Composer
        } elseif ($component -like "phpmyadmin-*") {
            $pmaVersion = $component -replace "phpmyadmin-"
            Install-PhpMyAdmin $pmaVersion
        } elseif ($component -eq "phpmyadmin") {
            Install-PhpMyAdmin
        } else {
            Write-Host "Componente desconhecido: $component"
        }
    }
    Add-BinDirsToPath
} elseif ($Command -eq "path") {
    Add-BinDirsToPath
} elseif ($Command -eq "uninstall") {
    foreach ($component in $Args) {
        if ($component -like "php-*") {
            $phpVersion = $component -replace "php-"
            Uninstall-PHP $phpVersion
        } elseif ($component -eq "php") {
            Uninstall-PHP
        } elseif ($component -eq "nginx") {
            Uninstall-Nginx
        } elseif ($component -like "mysql-*") {
            $mysqlVersion = $component -replace "mysql-"
            Uninstall-MySQL $mysqlVersion
        } elseif ($component -eq "mysql") {
            Uninstall-MySQL
        } elseif ($component -like "nodejs-*") {
            $nodeVersion = $component -replace "nodejs-"
            Uninstall-NodeJS $nodeVersion
        } elseif ($component -eq "nodejs" -or $component -eq "node") {
            Uninstall-NodeJS
        } elseif ($component -like "python-*") {
            $pyVersion = $component -replace "python-"
            Uninstall-Python $pyVersion
        } elseif ($component -eq "python") {
            Uninstall-Python
        } elseif ($component -like "composer-*") {
            $composerVersion = $component -replace "composer-"
            Uninstall-Composer $composerVersion
        } elseif ($component -eq "composer") {
            Uninstall-Composer
        } elseif ($component -like "phpmyadmin-*") {
            $pmaVersion = $component -replace "phpmyadmin-"
            Uninstall-PhpMyAdmin $pmaVersion
        } elseif ($component -eq "phpmyadmin") {
            Uninstall-PhpMyAdmin
        } else {
            Write-Host "Componente desconhecido: $component"
        }
    }
    Write-Host "Uninstall finalizado."
    exit 0
} else {
    Write-Host "Comando desconhecido: $Command"
}