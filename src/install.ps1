function Install-Commands {
    foreach ($component in $args) {
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
            "^openssl-(.+)$"     { Install-OpenSSL ($component -replace "^openssl-") }
            "^openssl$"          { Install-OpenSSL }
            default              { Write-Host "Componente desconhecido: $component" }
        }
    }
    Add-BinDirsToPath
}

function Install-GenericTool {
    param(
        [string]$ToolDir,
        [string]$Version,
        [string]$ZipUrl,
        [string]$SubDir,
        [string]$ExeName,
        [string]$Prefix
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

function Write-Info($msg) { Write-Host $msg -ForegroundColor Cyan }
function Write-WarningMsg($msg) { Write-Host $msg -ForegroundColor Yellow }
function Write-ErrorMsg($msg) { Write-Host $msg -ForegroundColor Red }
function Write-Log($msg) {
    $logFile = Join-Path $baseDir "devstack.log"
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Add-Content -Path $logFile -Value "[$timestamp] $msg"
}

function Download-And-ExtractZip {
    param(
        [string]$Url,
        [string]$ZipPath,
        [string]$ExtractTo
    )
    Write-Info "Baixando $Url ..."
    Write-Progress -Activity "Baixando" -Status $Url -PercentComplete 0
    Invoke-WebRequest -Uri $Url -OutFile $ZipPath -ErrorAction Stop
    Write-Progress -Activity "Baixando" -Status "Extraindo..." -PercentComplete 50
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::OpenRead($ZipPath)
    $zipBaseName = [IO.Path]::GetFileNameWithoutExtension($ZipPath)
    $hasSubDir = $false
    foreach ($entry in $zip.Entries) {
        if ($entry.FullName -match "^$zipBaseName/") {
            $hasSubDir = $true
            break
        }
    }
    $zip.Dispose()
    if ($hasSubDir) {
        $dest = [IO.Path]::GetDirectoryName($ZipPath)
        Expand-Archive $ZipPath -DestinationPath $dest -Force -ErrorAction Stop
    } else {
        Expand-Archive $ZipPath -DestinationPath $ExtractTo -Force -ErrorAction Stop
    }
    Remove-Item $ZipPath
    Write-Progress -Activity "Baixando" -Completed
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

function Get-LatestMongoDBVersion {
    $json = Invoke-RestMethod -Uri "https://www.mongodb.com/try/download/community/json/windows-server" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    $ver = $json.versions | Where-Object { $_.arch -eq 'x86_64' -and $_.edition -eq 'community' } | Sort-Object {[version]$_.version} -Descending | Select-Object -First 1
    return $ver.version
}
function Get-LatestRedisVersion {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/tporadowski/redis/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.tag_name.TrimStart('v')
}
function Get-LatestPgSQLVersion {
    $page = Invoke-WebRequest -Uri "https://www.enterprisedb.com/download-postgresql-binaries" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    if ($page.Content -match "postgresql-([\d\.-]+)-windows-x64-binaries.zip") {
        return $matches[1]
    } else {
        throw "Não foi possível obter a última versão do PostgreSQL."
    }
}
function Get-LatestMailHogVersion {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/mailhog/MailHog/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.tag_name.TrimStart('v')
}
function Get-LatestElasticsearchVersion {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/elastic/elasticsearch/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.tag_name.TrimStart('v')
}
function Get-LatestMemcachedVersion {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/nono303/memcached/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.tag_name
}
function Get-LatestDockerVersion {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/docker/desktop/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.tag_name.TrimStart('v')
}
function Get-LatestYarnVersion {
    $json = Invoke-RestMethod -Uri "https://registry.npmjs.org/yarn/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.version
}
function Get-LatestPnpmVersion {
    $json = Invoke-RestMethod -Uri "https://registry.npmjs.org/pnpm/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.version
}
function Get-LatestWPCLIVersion {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/wp-cli/wp-cli/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.tag_name.TrimStart('v')
}
function Get-LatestAdminerVersion {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/vrana/adminer/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.tag_name.TrimStart('v')
}
function Get-LatestPoetryVersion {
    $json = Invoke-RestMethod -Uri "https://pypi.org/pypi/poetry/json" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.info.version
}
function Get-LatestRubyVersion {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/oneclick/rubyinstaller2/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.tag_name.Split('-')[1]
}
function Get-LatestGoVersion {
    $json = Invoke-RestMethod -Uri "https://go.dev/dl/?mode=json" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return ($json | Where-Object { $_.stable } | Select-Object -First 1).version.TrimStart('go')
}
function Get-LatestCertbotVersion {
    $json = Invoke-RestMethod -Uri "https://pypi.org/pypi/certbot/json" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    return $json.info.version
}
function Get-LatestOpenSSLVersion {
    $json = Invoke-RestMethod -Uri "https://raw.githubusercontent.com/slproweb/opensslhashes/master/win32_openssl_hashes.json" -Headers @{ 'User-Agent' = 'DevStackSetup' }
    $entries = $json.files.PSObject.Properties | Where-Object {
        $_.Name -like 'Win64OpenSSL*' -or $_.Name -like 'WinUniversalOpenSSL*'
    }
    $normal = @()
    foreach ($entry in $entries) {
        if ($entry.Value.light -ne $true) {
            $normal += $entry.Value.basever
        }
    }
    if ($normal.Count -eq 0) {
        throw "Não foi possível obter versões normais do OpenSSL no JSON."
    }
    $latest = $normal | Sort-Object -Descending | Select-Object -First 1
    return $latest
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
    Rename-MainExe -Dir (Join-Path $phpDir $subDir) -ExeName "php-cgi.exe" -Version $version -Prefix "php-cgi"
    $phpIniSrc = "configs\php\php.ini"
    $phpIniDst = Join-Path (Join-Path $phpDir $subDir) "php.ini"
    if (Test-Path $phpIniSrc) {
        Copy-Item $phpIniSrc $phpIniDst -Force
        Write-Host "Arquivo php.ini copiado para $($phpDir)\$subDir"
        # Gera linhas extension= para todas as DLLs da pasta ext
        $extDir = Join-Path (Join-Path $phpDir $subDir) "ext"
        if (Test-Path $extDir) {
            # Lista de extensões essenciais para CakePHP e Laravel (MySQL)
            $acceptExt = @(
                "mbstring", "intl", "pdo", "pdo_mysql", "pdo_pgsql", "openssl", "json", "fileinfo", "curl", "gd", "gd2", "zip", "xml", "xmlrpc"
            )

            $extensions = Get-ChildItem $extDir -Filter *.dll | ForEach-Object {
                $name = $_.BaseName -replace '^php_',''
                if ($acceptExt -contains $name) {
                    "extension=$name"
                }
            }
            Add-Content -Path $phpIniDst -Value "`n; Extensões essenciais para CakePHP e Laravel:`n$($extensions -join "`n")"
            Write-Host "Bloco de extensões essenciais adicionado ao php.ini"
        }
    } else {
        Write-Host "Arquivo configs\php\php.ini não encontrado. Pulei a cópia do php.ini."
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
    $composerSubDir = "composer-$version"
    $composerPhar = "composer-$version.phar"
    $composerPharPath = Join-Path $composerDir $composerSubDir
    if (Test-Path $composerPharPath) {
        Write-Info "Composer $version já está instalado."
        return
    }
    Write-Info "Baixando Composer $version..."
    New-Item -ItemType Directory -Force -Path $composerPharPath | Out-Null
    $composerUrl = "https://getcomposer.org/download/$version/composer.phar"
    Invoke-WebRequest -Uri $composerUrl -OutFile (Join-Path $composerPharPath $composerPhar) -ErrorAction Stop
    Write-Info "Composer $version instalado."
    Write-Log "Composer $version instalado em $composerPharPath"
}

function Install-PhpMyAdmin {
    param($version)
    if (-not $version) {
        $version = Get-LatestPhpMyAdminVersion
    }
    if (Test-Path $pmaDir) {
        Write-Info "phpMyAdmin $version já está instalado."
        return
    }
    Write-Info "Baixando phpMyAdmin $version..."
    $pmaZip = Join-Path $baseDir "phpmyadmin-$version-all-languages.zip"
    $pmaUrl = "https://files.phpmyadmin.net/phpMyAdmin/$version/phpMyAdmin-$version-all-languages.zip"
    Invoke-WebRequest -Uri $pmaUrl -OutFile $pmaZip -ErrorAction Stop
    Expand-Archive $pmaZip -DestinationPath $baseDir -Force -ErrorAction Stop
    Rename-Item -Path (Join-Path $baseDir "phpMyAdmin-$version-all-languages") -NewName "phpmyadmin-$version" -Force
    Remove-Item $pmaZip
    Write-Info "phpMyAdmin $version instalado em $pmaDir."
    Write-Log "phpMyAdmin $version instalado em $pmaDir"
}

function Install-Git {
    param($version)
    if (-not $version) {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/git-for-windows/git/releases/latest" -Headers @{ 'User-Agent' = 'DevStackSetup' }
        $version = $json.tag_name.TrimStart('v')
    }
    $gitSubDir = "git-$version"
    $gitDirFull = Join-Path $baseDir $gitSubDir
    if (Test-Path $gitDirFull) {
        Write-Info "Git $version já está instalado."
        return
    }
    Write-Info "Baixando Git $version..."
    $gitUrl = "https://github.com/git-for-windows/git/releases/download/v$version/PortableGit-$version-64-bit.7z.exe"
    $git7zExe = Join-Path $baseDir "PortableGit-$version-64-bit.7z.exe"
    Invoke-WebRequest -Uri $gitUrl -OutFile $git7zExe -ErrorAction Stop
    New-Item -ItemType Directory -Force -Path $gitDirFull | Out-Null
    Write-Info "Extraindo Git $version..."
    Start-Process -FilePath $git7zExe -ArgumentList "-y -o$gitDirFull" -Wait
    Remove-Item $git7zExe
    Write-Info "Git $version instalado em $gitDirFull."
    Write-Log "Git $version instalado em $gitDirFull"
}

function Create-NginxSiteConfig {
    param(
        [string]$Domain,
        [string]$Root,
        [string]$PhpUpstream,
        [string]$NginxVersion,
        [string]$IndexLocation
    )

    if (-not $NginxVersion) {
        $NginxVersion = Get-LatestNginxVersion
    }
    $nginxVersionDir = Join-Path $nginxDir "nginx-$NginxVersion"
    $nginxSitesDirFull = Join-Path $nginxVersionDir $nginxSitesDir

    if (!(Test-Path $nginxVersionDir)) {
        throw "A versão do Nginx ($NginxVersion) não está instalada em $nginxVersionDir."
    }

    if (!(Test-Path $nginxSitesDirFull)) {
        New-Item -ItemType Directory -Force -Path $nginxSitesDirFull | Out-Null
    }

    if (-not $IndexLocation) {
        $IndexLocation = "webroot"
    }

    if (-not $PhpUpstream) {
        $PhpUpstream = "127.0.0.1:9000"
    }

    if (-not $Root -and (Test-Path "C:\Workspace\$Domain")) {
        $Root = "C:\Workspace\$Domain"
    }

    $confPath = Join-Path $nginxSitesDirFull "$Domain.conf"
    $serverName = "$Domain.localhost"
    $rootPath = Join-Path $Root $IndexLocation

    $template = @"
server {

    listen 80;
    listen [::]:80;

    server_name $serverName;
    root $rootPath;
    index index.php index.html index.htm;

    location / {
         try_files `$uri `$uri/ /index.php`$is_args`$args;
    }

    location ~ \.php$ {
        try_files `$uri /index.php =404;
        fastcgi_pass $PhpUpstream;
        fastcgi_index index.php;
        fastcgi_buffers 16 16k;
        fastcgi_buffer_size 32k;
        fastcgi_param SCRIPT_FILENAME `$document_root`$fastcgi_script_name;
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
        rewrite ^/api/(\w+).*$ /api.php?type=`$1 last;
    }

    error_log logs\${Domain}_error.log;
    access_log logs\${Domain}_access.log;
}
"@

    Set-Content -Path $confPath -Value $template
    Write-Host "Arquivo $confPath criado/configurado com sucesso!"

    $hostsPath = "$($env:SystemRoot)\System32\drivers\etc\hosts"
    $entry = "127.0.0.1`t$serverName"
    $hostsContent = Get-Content $hostsPath -ErrorAction SilentlyContinue
    if ($hostsContent -notcontains $entry) {
        Add-Content -Path $hostsPath -Value $entry
        Write-Host "Adicionado $serverName ao arquivo hosts."
    } else {
        Write-Host "$serverName já está presente no arquivo hosts."
    }
}

function Install-MongoDB {
    param($version)
    if (-not $version) { $version = Get-LatestMongoDBVersion }
    $subDir = "mongodb-$version"
    $zipUrl = "https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-$version.zip"
    Install-GenericTool -ToolDir $mongoDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "bin\mongod.exe" -Prefix "mongod"
    Write-Info "MongoDB $version instalado."
}
function Install-Redis {
    param($version)
    if (-not $version) { $version = Get-LatestRedisVersion }
    $subDir = "redis-$version"
    $zipUrl = "https://github.com/tporadowski/redis/releases/download/v$version/redis-$version.zip"
    Install-GenericTool -ToolDir $redisDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "redis-server.exe" -Prefix "redis"
    Write-Info "Redis $version instalado."
}
function Install-PgSQL {
    param($version)
    if (-not $version) { $version = Get-LatestPgSQLVersion }
    $subDir = "pgsql-$version"
    $zipUrl = "https://get.enterprisedb.com/postgresql/postgresql-$version-windows-x64-binaries.zip"
    Install-GenericTool -ToolDir $pgsqlDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "bin\psql.exe" -Prefix "psql"
    Write-Info "PostgreSQL $version instalado."
}
function Install-MailHog {
    param($version)
    if (-not $version) { $version = Get-LatestMailHogVersion }
    $subDir = "mailhog-$version"
    $zipUrl = "https://github.com/mailhog/MailHog/releases/download/v$version/MailHog_windows_amd64.zip"
    Install-GenericTool -ToolDir $mailhogDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "MailHog.exe" -Prefix "mailhog"
    Write-Info "MailHog $version instalado."
}
function Install-Elasticsearch {
    param($version)
    if (-not $version) { $version = Get-LatestElasticsearchVersion }
    $subDir = "elasticsearch-$version"
    $zipUrl = "https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-$version-windows-x86_64.zip"
    Install-GenericTool -ToolDir $elasticDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "bin\elasticsearch.bat" -Prefix "elasticsearch"
    Write-Info "Elasticsearch $version instalado."
}
function Install-Memcached {
    param($version)
    if (-not $version) { $version = Get-LatestMemcachedVersion }
    $subDir = "memcached-$version"
    $zipUrl = "https://github.com/nono303/memcached/releases/download/$version/memcached-$version-win64.zip"
    Install-GenericTool -ToolDir $memcachedDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "memcached.exe" -Prefix "memcached"
    Write-Info "Memcached $version instalado."
}
function Install-Docker {
    param($version)
    if (-not $version) { $version = Get-LatestDockerVersion }
    $url = "https://desktop.docker.com/win/main/amd64/Docker%20Desktop%20Installer.exe"
    $installer = Join-Path $dockerDir "DockerDesktopInstaller.exe"
    if (-not (Test-Path $dockerDir)) { New-Item -ItemType Directory -Force -Path $dockerDir | Out-Null }
    Write-Info "Baixando Docker Desktop..."
    Invoke-WebRequest -Uri $url -OutFile $installer -ErrorAction Stop
    Write-Info "Executando instalador do Docker Desktop..."
    Start-Process -FilePath $installer -ArgumentList "/install /quiet" -Wait
    Remove-Item $installer
    Write-Info "Docker Desktop instalado."
}
function Install-Yarn {
    param($version)
    if (-not $version) { $version = Get-LatestYarnVersion }
    Write-Info "Instalando Yarn via npm..."
    & npm install -g yarn
    Write-Info "Yarn instalado globalmente."
}
function Install-Pnpm {
    param($version)
    if (-not $version) { $version = Get-LatestPnpmVersion }
    Write-Info "Instalando pnpm via npm..."
    & npm install -g pnpm
    Write-Info "pnpm instalado globalmente."
}
function Install-WPCLI {
    param($version)
    if (-not $version) { $version = Get-LatestWPCLIVersion }
    $subDir = "wpcli-$version"
    $url = "https://github.com/wp-cli/wp-cli/releases/download/v$version/wp-cli-$version.phar"
    if (-not (Test-Path $wpcliDir)) { New-Item -ItemType Directory -Force -Path $wpcliDir | Out-Null }
    $pharPath = Join-Path $wpcliDir "wp-cli-$version.phar"
    Invoke-WebRequest -Uri $url -OutFile $pharPath -ErrorAction Stop
    $batPath = Join-Path $wpcliDir "wp.bat"
    Set-Content -Path $batPath -Value "@echo off`nphp %~dp0wp-cli-$version.phar %*"
    Write-Info "WP-CLI $version instalado em $wpcliDir. Use 'wp' no terminal."
}
function Install-Adminer {
    param($version)
    if (-not $version) { $version = Get-LatestAdminerVersion }
    $subDir = "adminer-$version"
    if (-not (Test-Path $adminerDir)) { New-Item -ItemType Directory -Force -Path $adminerDir | Out-Null }
    $url = "https://github.com/vrana/adminer/releases/download/v$version/adminer-$version.php"
    $phpPath = Join-Path $adminerDir "adminer.php"
    Invoke-WebRequest -Uri $url -OutFile $phpPath -ErrorAction Stop
    Write-Info "Adminer $version instalado em $adminerDir. Abra o arquivo PHP no navegador."
}
function Install-Poetry {
    param($version)
    if (-not $version) { $version = Get-LatestPoetryVersion }
    Write-Info "Instalando Poetry via pip..."
    & pip install --upgrade poetry
    Write-Info "Poetry instalado globalmente."
}
function Install-Ruby {
    param($version)
    if (-not $version) { $version = Get-LatestRubyVersion }
    $subDir = "ruby-$version"
    $zipUrl = "https://github.com/oneclick/rubyinstaller2/releases/download/RubyInstaller-$version/rubyinstaller-$version-x64.7z"
    Install-GenericTool -ToolDir $rubyDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "bin\ruby.exe" -Prefix "ruby"
    Write-Info "Ruby $version instalado."
}
function Install-Go {
    param($version)
    if (-not $version) { $version = Get-LatestGoVersion }
    $subDir = "go-$version"
    $zipUrl = "https://go.dev/dl/go$version.windows-amd64.zip"
    Install-GenericTool -ToolDir $goDir -Version $version -ZipUrl $zipUrl -SubDir $subDir -ExeName "bin\go.exe" -Prefix "go"
    Write-Info "Go $version instalado."
}
function Install-Certbot {
    param($version)
    if (-not $version) { $version = Get-LatestCertbotVersion }
    Write-Info "Instalando Certbot via pip..."
    & pip install --upgrade certbot
    Write-Info "Certbot instalado globalmente."
}
function Install-OpenSSL {
    param(
        [string]$version,
        [string]$arch = "x64"  # default to 64-bit
    )
    if (-not $version) { $version = Get-LatestOpenSSLVersion }
    $archPrefix = if ($arch -eq "x86") { "Win32OpenSSL" } else { "Win64OpenSSL" }
    $subDir = "openssl-$version"
    $versionUnderscore = $version -replace '\.', '_'
    $installerName = "$archPrefix-$versionUnderscore.exe"
    $installerUrl = "https://slproweb.com/download/$installerName"
    Write-Host $installerUrl
    $installDir = "C:\devstack\openssl\$subDir"
    $installerPath = Join-Path $tmpDir $installerName
    if (-not (Test-Path $tmpDir)) { New-Item -ItemType Directory -Force -Path $tmpDir | Out-Null }
    Write-Info "Baixando instalador do OpenSSL $version ($arch)..."
    Invoke-WebRequest -Uri $installerUrl -OutFile $installerPath -ErrorAction Stop
    Write-Info "Executando instalador do OpenSSL $version ($arch)..."
    if (-not (Test-Path $installDir)) { New-Item -ItemType Directory -Force -Path $installDir | Out-Null }
    Start-Process -FilePath $installerPath -ArgumentList "/silent /DIR=`"$installDir`"" -Wait
    Remove-Item $installerPath
    Write-Info "OpenSSL $version ($arch) instalado via instalador em $installDir"
}