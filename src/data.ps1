# Funções para aquisição de dados

function Get-InstalledVersions {
    param()
    
    if (-not (Test-Path $baseDir)) {
        return @{
            status = "warning"
            message = "O diretório $baseDir não existe. Nenhuma ferramenta instalada."
            components = @()
        }
    }
    
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
        @{ name = 'certbot'; dir = $certbotDir },
        @{ name = 'openssl'; dir = $openSSLDir },
        @{ name = 'php-cs-fixer'; dir = $phpcsfixerDir }
    )
    
    $result = @()
    foreach ($comp in $components) {
        $item = @{
            name = $comp.name
            installed = $false
            versions = @()
        }
        
        if (Test-Path $comp.dir) {
            if ($comp.name -eq 'git') {
                $versions = Get-ChildItem $comp.dir -Directory | Where-Object { $_.Name -like $comp.pattern } | ForEach-Object { $_.Name }
            } else {
                $versions = Get-ChildItem $comp.dir -Directory -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }
            }
            
            if ($versions -and $versions.Count -gt 0) {
                $item.installed = $true
                $item.versions = $versions
            }
        }
        
        $result += $item
    }
    
    return @{
        status = "success"
        message = ""
        components = $result
    }
}

function Get-NginxVersions {
    param()
    
    try {
        $page = Invoke-WebRequest -Uri "https://nginx.org/en/download.html"
        $matches = [regex]::Matches($page.Content, "nginx-([\d\.]+)\.zip")
        $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
        
        $installed = @()
        if (Test-Path $nginxDir) { 
            $installed = Get-ChildItem $nginxDir -Directory | ForEach-Object { $_.Name -replace '^nginx-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Nginx disponíveis para Windows:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Nginx: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-PHPVersions {
    param()
    
    try {
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
        $versions = $allMatches | Sort-Object -Descending | Get-Unique
        
        $installed = @()
        if (Test-Path $phpDir) { 
            $installed = Get-ChildItem $phpDir -Directory | ForEach-Object { $_.Name -replace '^php-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de PHP disponíveis para Windows x64:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do PHP: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-NodeVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://nodejs.org/dist/index.json"
        $versions = $json | Select-Object -ExpandProperty version
        
        $installed = @()
        if (Test-Path $nodeDir) { 
            $installed = Get-ChildItem $nodeDir -Directory | ForEach-Object { $_.Name -replace '^node-v', '' -replace '-win-x64$', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Node.js disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Node.js: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-PythonVersions {
    param()
    
    try {
        # Define all Python indexes
        $pythonIndexUrls = @(
            "https://www.python.org/ftp/python/index-windows-recent.json",
            "https://www.python.org/ftp/python/index-windows-legacy.json",
            "https://www.python.org/ftp/python/index-windows.json"
        )
        
        # Extract all versions that have amd64 zip files
        $versions = @()
        
        # Process each index file
        foreach ($indexUrl in $pythonIndexUrls) {
            try {
                $pythonVersions = Invoke-RestMethod -Uri $indexUrl -UseBasicParsing
                
                if ($pythonVersions -and $pythonVersions.versions) {
                    foreach ($version in $pythonVersions.versions) {
                        # Check for regular amd64 package (not embeddable or test)
                        if ($version.url -match "python-(\d+\.\d+\.\d+)-amd64\.zip$" -and $version.url -notmatch "embeddable|test") {
                            # Store matches in local variable to avoid scope issues
                            $localMatches = $Matches
                            # Extract version number directly from the regex match
                            $versionNumber = $localMatches[1]
                            # Avoid duplicates
                            if ($versions -notcontains $versionNumber) {
                                $versions += $versionNumber
                            }
                        }
                    }
                }
            }
            catch {
                Write-Verbose "Erro ao carregar ${indexUrl}: $($_.ToString())"
                # Continue with next index if this one fails
                continue
            }
        }
        
        $versions = $versions | Sort-Object {[version]$_} -Descending
        
        $installed = @()
        if (Test-Path $pythonDir) { 
            $installed = Get-ChildItem $pythonDir -Directory | ForEach-Object { $_.Name -replace '^python-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Python disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Python: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-MongoDBVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://www.mongodb.com/try/download/community/json"
        $versions = $json.versions | Where-Object { $_.platform -eq "windows" } | Select-Object -ExpandProperty version
        
        $installed = @()
        if (Test-Path $mongoDir) { 
            $installed = Get-ChildItem $mongoDir -Directory | ForEach-Object { $_.Name -replace '^mongodb-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de MongoDB disponíveis para Windows:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do MongoDB: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-RedisVersions {
    param()
    
    try {
        $page = Invoke-WebRequest -Uri "https://github.com/microsoftarchive/redis/releases"
        $matches = [regex]::Matches($page.Content, "/microsoftarchive/redis/releases/tag/([\d\.]+)")
        $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
        
        $installed = @()
        if (Test-Path $redisDir) { 
            $installed = Get-ChildItem $redisDir -Directory | ForEach-Object { $_.Name -replace '^redis-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Redis para Windows (Microsoft Archive):"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Redis: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-PgSQLVersions {
    param()
    
    try {
        $page = Invoke-WebRequest -Uri "https://www.enterprisedb.com/downloads/postgres-postgresql-downloads"
        $matches = [regex]::Matches($page.Content, "PostgreSQL ([\d\.]+)")
        $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
        
        $installed = @()
        if (Test-Path $pgsqlDir) { 
            $installed = Get-ChildItem $pgsqlDir -Directory | ForEach-Object { $_.Name -replace '^pgsql-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de PostgreSQL disponíveis para Windows:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do PostgreSQL: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-MailHogVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/mailhog/MailHog/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $mailhogDir) { 
            $installed = Get-ChildItem $mailhogDir -Directory | ForEach-Object { $_.Name -replace '^mailhog-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de MailHog disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do MailHog: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-ElasticsearchVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/elastic/elasticsearch/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $elasticDir) { 
            $installed = Get-ChildItem $elasticDir -Directory | ForEach-Object { $_.Name -replace '^elasticsearch-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Elasticsearch disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Elasticsearch: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-MemcachedVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/memcached/memcached/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $memcachedDir) { 
            $installed = Get-ChildItem $memcachedDir -Directory | ForEach-Object { $_.Name -replace '^memcached-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Memcached disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Memcached: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-DockerVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/docker/cli/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $dockerDir) { 
            $installed = Get-ChildItem $dockerDir -Directory | ForEach-Object { $_.Name -replace '^docker-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Docker CLI disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Docker: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-YarnVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/yarnpkg/yarn/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $yarnDir) { 
            $installed = Get-ChildItem $yarnDir -Directory | ForEach-Object { $_.Name -replace '^yarn-v', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Yarn disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Yarn: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-PnpmVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/pnpm/pnpm/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $pnpmDir) { 
            $installed = Get-ChildItem $pnpmDir -Directory | ForEach-Object { $_.Name -replace '^pnpm-v', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de pnpm disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do pnpm: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-WPCLIVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/wp-cli/wp-cli/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $wpcliDir) { 
            $installed = Get-ChildItem $wpcliDir -Directory | ForEach-Object { $_.Name -replace '^wp-cli-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de WP-CLI disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do WP-CLI: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-AdminerVersions {
    param()
    
    try {
        $page = Invoke-WebRequest -Uri "https://www.adminer.org/en/"
        $matches = [regex]::Matches($page.Content, "Adminer ([\d\.]+)")
        $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
        
        $installed = @()
        if (Test-Path $adminerDir) { 
            $installed = Get-ChildItem $adminerDir -Directory | ForEach-Object { $_.Name -replace '^adminer-', '' -replace '\.php$', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Adminer disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Adminer: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-PoetryVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/python-poetry/poetry/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $poetryDir) { 
            $installed = Get-ChildItem $poetryDir -Directory | ForEach-Object { $_.Name -replace '^poetry-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Poetry disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Poetry: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-RubyVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/oneclick/rubyinstaller2/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $rubyDir) { 
            $installed = Get-ChildItem $rubyDir -Directory | ForEach-Object { $_.Name -replace '^ruby-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de RubyInstaller2 disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Ruby: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-GoVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://go.dev/dl/?mode=json"
        $versions = $json | Select-Object -ExpandProperty version
        
        $installed = @()
        if (Test-Path $goDir) { 
            $installed = Get-ChildItem $goDir -Directory | ForEach-Object { $_.Name -replace '^go', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Go disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Go: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-CertbotVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/certbot/certbot/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $certbotDir) { 
            $installed = Get-ChildItem $certbotDir -Directory | ForEach-Object { $_.Name -replace '^certbot-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Certbot disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Certbot: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-OpenSSLVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://raw.githubusercontent.com/slproweb/opensslhashes/master/win32_openssl_hashes.json"
        $entries = $json.files.PSObject.Properties | Where-Object {
            $_.Name -like 'Win64OpenSSL*' -or $_.Name -like 'WinUniversalOpenSSL*'
        }
        $normal = @()
        $light = @()
        foreach ($entry in $entries) {
            $ver = $entry.Value.basever
            if ($entry.Value.light -eq $true) {
                $light += $ver
            } else {
                $normal += $ver
            }
        }
        $normal = $normal | Sort-Object -Descending | Select-Object -Unique
        $light = $light | Sort-Object -Descending | Select-Object -Unique
        $basevers = @()
        for ($i = 0; $i -lt $normal.Count; $i++) {
            $basevers += $normal[$i]
            if ($light -contains $normal[$i]) {
                $light = $light | Where-Object { $_ -ne $normal[$i] }
                $basevers += "light-$($normal[$i])"
            }
        }
        foreach ($l in $light) {
            if (-not ($normal -contains $l)) {
                $basevers += "light-$l"
            }
        }
        
        $installed = @()
        if (Test-Path $opensslDir) {
            $installed = Get-ChildItem $opensslDir -Directory | ForEach-Object {
                $n = $_.Name -replace '^openssl-', ''
                if ($n -like 'light-*') { $n } else { $n -replace '^light-', '' }
            }
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de OpenSSL (SLProWeb) disponíveis:"
            versions = $basevers
            installed = $installed
            orderDescending = $false
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do OpenSSL: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-PHPCsFixerVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/PHP-CS-Fixer/PHP-CS-Fixer/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $phpcsfixerDir) { 
            $installed = Get-ChildItem $phpcsfixerDir -Directory | ForEach-Object { $_.Name -replace '^php-cs-fixer-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de PHP CS Fixer disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do PHP CS Fixer: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-ComposerVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/composer/composer/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $composerDir) { 
            $installed = Get-ChildItem $composerDir -Directory | ForEach-Object { $_.Name -replace '^composer-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Composer disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Composer: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-MySQLVersions {
    param()
    
    try {
        # Use MySQL Docker tags as a reliable source of MySQL versions
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/mysql/mysql-server/tags"
        $versions = $json | ForEach-Object { 
            if ($_.name -match '^mysql-(.+)$') { 
                $matches[1] 
            } 
        } | Sort-Object -Descending | Select-Object -First 30
        
        $installed = @()
        if (Test-Path $mysqlDir) { 
            $installed = Get-ChildItem $mysqlDir -Directory | ForEach-Object { $_.Name -replace '^mysql-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de MySQL disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do MySQL: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-PhpMyAdminVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/phpmyadmin/phpmyadmin/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $pmaDir) { 
            $installed = Get-ChildItem $pmaDir -Directory | ForEach-Object { $_.Name -replace '^phpmyadmin-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de phpMyAdmin disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do phpMyAdmin: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-GitVersions {
    param()
    
    try {
        $json = Invoke-RestMethod -Uri "https://api.github.com/repos/git-for-windows/git/releases"
        $versions = $json | Select-Object -ExpandProperty tag_name
        
        $installed = @()
        if (Test-Path $baseDir) { 
            $installed = Get-ChildItem $baseDir -Directory -Filter "git-*" | ForEach-Object { $_.Name -replace '^git-', '' } 
        }
        
        return @{
            status = "success"
            message = ""
            header = "Versões de Git para Windows disponíveis:"
            versions = $versions
            installed = $installed
        }
    }
    catch {
        return @{
            status = "error"
            message = "Erro ao obter versões do Git: $_"
            versions = @()
            installed = @()
        }
    }
}

function Get-ComponentStatus {
    param([string]$Component)
    
    $dir = switch ($Component) {
        "php" { $phpDir }
        "nginx" { $nginxDir }
        "mysql" { $mysqlDir }
        "nodejs" { $nodeDir }
        "python" { $pythonDir }
        "composer" { $composerDir }
        "git" { $baseDir }
        "phpmyadmin" { $pmaDir }
        "mongodb" { $mongoDir }
        "redis" { $redisDir }
        "pgsql" { $pgsqlDir }
        "mailhog" { $mailhogDir }
        "elasticsearch" { $elasticDir }
        "memcached" { $memcachedDir }
        "docker" { $dockerDir }
        "yarn" { $yarnDir }
        "pnpm" { $pnpmDir }
        "wpcli" { $wpcliDir }
        "adminer" { $adminerDir }
        "poetry" { $poetryDir }
        "ruby" { $rubyDir }
        "go" { $goDir }
        "certbot" { $certbotDir }        "openssl" { $openSSLDir }
        "php-cs-fixer" { $phpcsfixerDir }
        default { return @{ installed = $false; versions = @(); message = "Componente desconhecido" } }
    }
    
    $versions = @()
    if (Test-Path $dir) {
        if ($Component -eq "git") {
            $versions = Get-ChildItem $dir -Directory | Where-Object { $_.Name -like 'git-*' } | ForEach-Object { $_.Name }
        } else {
            $versions = Get-ChildItem $dir -Directory | ForEach-Object { $_.Name }
        }
    }
    
    if ($versions.Count -eq 0) {
        return @{ 
            installed = $false
            versions = @()
            message = "$Component não está instalado."
        }
    }
    
    return @{
        installed = $true
        versions = $versions
        message = "$Component instalado(s)"
    }
}

function Get-AllComponentsStatus {
    $components = @(
        "php", "nginx", "mysql", "nodejs", "python", "composer", "git", "phpmyadmin",
        "mongodb", "redis", "pgsql", "mailhog", "elasticsearch", "memcached",
        "docker", "yarn", "pnpm", "wpcli", "adminer", "poetry", "ruby", "go",
        "certbot", "openssl", "phpcsfixer"
    )
    
    $results = @{}
    foreach ($comp in $components) {
        $results[$comp] = Get-ComponentStatus -Component $comp
    }
    
    return $results
}
