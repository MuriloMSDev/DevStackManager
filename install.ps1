function Install-GenericTool {
    param(
        [string]$ToolDir,
        [string]$Version,
        [string]$ZipUrl,
        [string]$SubDir,         # Ex: "php-$Version" ou "node-v$Version-win-x64"
        [string]$ExeName,        # Ex: php.exe
        [string]$Prefix          # Ex: php
    )
    $targetDir = Join-Path $ToolDir $SubDir
    $rollback = $false
    try {
        if (Test-Path $targetDir) {
            Write-Host "$Prefix $Version já está instalado."
        } else {
            if (-not (Test-Path $ToolDir)) {
                New-Item -ItemType Directory -Force -Path $ToolDir | Out-Null
            }
            Write-Host "Baixando $Prefix $Version..."
            $zipPath = Join-Path $ToolDir "$Prefix-$Version.zip"
            Download-And-ExtractZip -Url $ZipUrl -ZipPath $zipPath -ExtractTo $targetDir
            Write-Host "$Prefix $Version instalado."
            $rollback = $true
        }
        Rename-MainExe -Dir $targetDir -ExeName $ExeName -Version $Version -Prefix $Prefix
    } catch {
        Write-Host "Erro ao instalar $Prefix $($Version): $($_)"
        if ($rollback) { Rollback-Install $targetDir }
        throw
    }
}

function Download-And-ExtractZip {
    param(
        [string]$Url,
        [string]$ZipPath,
        [string]$ExtractTo
    )
    Invoke-WebRequest -Uri $Url -OutFile $ZipPath
    Expand-Archive $ZipPath -DestinationPath $ExtractTo -Force
    Remove-Item $ZipPath
}

function Rename-MainExe {
    param(
        [string]$Dir,
        [string]$ExeName,      # Ex: php.exe
        [string]$Version,      # Ex: 8.3.21
        [string]$Prefix        # Ex: php
    )
    $exePath = Join-Path $Dir $ExeName
    $exeVersionPath = Join-Path $Dir "$Prefix-$Version.exe"
    if (Test-Path $exePath) {
        Rename-Item -Path $exePath -NewName "$Prefix-$Version.exe" -Force
        Write-Host "Renomeado $ExeName para $Prefix-$Version.exe"
    }
}

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
    $version = Get-LatestNginxVersion
    $subDir = "nginx-$version"
    $zipUrl = "https://nginx.org/download/nginx-$version.zip"
    Write-Host $nginxDir
    Install-GenericTool -ToolDir $nginxDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "nginx.exe" -Prefix "nginx"
}

function Install-PHP {
    param($version)
    if (-not $version) {
        $version = Get-LatestPHPVersion
    }
    $urls = @(
        "https://windows.php.net/downloads/releases/",
        "https://windows.php.net/downloads/releases/archives/"
    )
    $phpZipName = "php-$version-Win32-vs16-x64.zip"
    $phpUrl = $null
    foreach ($baseUrl in $urls) {
        $page = Invoke-WebRequest -Uri $baseUrl
        $match = [regex]::Match($page.Content, "php-$version-Win32-[^-]+-x64\.zip")
        if ($match.Success) {
            $phpZipName = $match.Value
            $phpUrl = $baseUrl + $phpZipName
            break
        }
    }
    if (-not $phpUrl) {
        throw "Não foi encontrado o arquivo .zip para PHP $version nas releases oficiais."
    }
    $subDir = "php-$version"
    Install-GenericTool -ToolDir $phpDir -Version $version -ZipUrl $phpUrl -SubDir $subDir -ExeName "php.exe" -Prefix "php"
    $localIniSrc = "configs\php\local.ini"
    $localIniDst = Join-Path (Join-Path $phpDir $subDir) "local.ini"
    if (Test-Path $localIniSrc) {
        Copy-Item $localIniSrc $localIniDst -Force
        Write-Host "Arquivo local.ini copiado para $($phpDir)\$subDir"
    } else {
        Write-Host "Arquivo configs\php\local.ini não encontrado. Pulei a cópia do local.ini."
    }
}

function Install-MySQL {
    param($version = "8.0.36")
    $subDir = "mysql-$version-winx64"
    $zipUrl = "https://dev.mysql.com/get/Downloads/MySQL-8.0/mysql-$version-winx64.zip"
    Install-GenericTool -ToolDir $mysqlDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "bin\mysqld.exe" -Prefix "mysqld"
}

function Install-NodeJS {
    param($version)
    if (-not $version) {
        $version = Get-LatestNodeVersion
    }
    $subDir = "node-v$version-win-x64"
    $zipUrl = "https://nodejs.org/dist/v$version/node-v$version-win-x64.zip"
    Install-GenericTool -ToolDir $nodeDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "node.exe" -Prefix "node"
}

function Install-Python {
    param($version)
    if (-not $version) {
        $version = Get-LatestPythonVersion
    }
    $pySubDir = "python-$version"
    $pyDirFull = Join-Path $pythonDir $pySubDir
    if (Test-Path $pyDirFull) {
        Write-Host "Python $version já está instalado."
        return
    }
    Write-Host "Baixando Python $version..."
    $pyExe = "$pythonDir\python-$version-amd64.exe"
    $pyUrl = "https://www.python.org/ftp/python/$version/python-$version-amd64.exe"
    Invoke-WebRequest -Uri $pyUrl -OutFile $pyExe
    New-Item -ItemType Directory -Force -Path $pyDirFull | Out-Null
    Write-Host "Instalando Python $version em modo silencioso..."
    Start-Process -FilePath $pyExe -ArgumentList "/quiet InstallAllUsers=0 TargetDir=$pyDirFull" -Wait
    Remove-Item $pyExe
    Write-Host "Python $version instalado."
    $pythonExe = Join-Path $pyDirFull "python.exe"
    if (Test-Path $pythonExe) {
        Rename-Item -Path $pythonExe -NewName "python-$version.exe" -Force
        Write-Host "Renomeado python.exe para python-$version.exe"
    }
}

function Install-Composer {
    param($version)
    if (-not $version) {
        $version = Get-LatestComposerVersion
    }
    $composerPhar = "composer-$version.phar"
    $composerPharPath = Join-Path $composerDir "composer-$version"
    if (Test-Path $composerPharPath) {
        Write-Host "Composer $version já está instalado."
        return
    }
    Write-Host "Baixando Composer $version..."
    New-Item -ItemType Directory -Force -Path $composerPharPath | Out-Null
    $composerUrl = "https://getcomposer.org/download/$version/composer.phar"
    Invoke-WebRequest -Uri $composerUrl -OutFile (Join-Path $composerPharPath $composerPhar)
    Write-Host "Composer $version instalado."
}

function Install-PhpMyAdmin {
    param($version)
    if (-not $version) {
        $version = Get-LatestPhpMyAdminVersion
    }
    $pmaDir = "$baseDir\phpmyadmin-$version"
    if (Test-Path $pmaDir) {
        Write-Host "phpMyAdmin $version já está instalado."
        return
    }
    Write-Host "Baixando phpMyAdmin $version..."
    $pmaZip = "$baseDir\phpmyadmin-$version-all-languages.zip"
    $pmaUrl = "https://files.phpmyadmin.net/phpMyAdmin/$version/phpMyAdmin-$version-all-languages.zip"
    Invoke-WebRequest -Uri $pmaUrl -OutFile $pmaZip
    Expand-Archive $pmaZip -DestinationPath $baseDir -Force
    Rename-Item -Path "$baseDir\phpMyAdmin-$version-all-languages" -NewName "phpmyadmin-$version"
    Remove-Item $pmaZip
    Write-Host "phpMyAdmin $version instalado em $pmaDir."
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