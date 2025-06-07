function List-InstalledVersions {
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
        @{ name = 'certbot'; dir = $certbotDir },
        @{ name = 'openssl'; dir = $opensslDir }
    )
    # Tabela de Ferramentas Instaladas
    $col1 = 15; $col2 = 40
    $header = ('_' * ($col1 + $col2 + 3))
    Write-Host $header
    Write-Host ("|{0}|{1}|" -f (Center-Text 'Ferramenta' $col1), (Center-Text 'Versões Instaladas' $col2))
    Write-Host ("|" + ('-' * $col1) + "+" + ('-' * $col2) + "|")
    foreach ($comp in $components) {
        if (-not (Test-Path $comp.dir)) {
            $ferramenta = Center-Text $comp.name $col1
            $status = Center-Text 'NÃO INSTALADO' $col2
            Write-Host -NoNewline "|"
            Write-Host -NoNewline $ferramenta -ForegroundColor Red
            Write-Host -NoNewline "|"
            Write-Host -NoNewline $status -ForegroundColor Red
            Write-Host "|"
            continue
        }
        if ($comp.name -eq 'git') {
            $versions = Get-ChildItem $comp.dir -Directory | Where-Object { $_.Name -like $comp.pattern } | ForEach-Object { $_.Name }
        } else {
            $versions = Get-ChildItem $comp.dir -Directory -ErrorAction SilentlyContinue | ForEach-Object { $_.Name }
        }
        if ($versions -and $versions.Count -gt 0) {
            $ferramenta = Center-Text $comp.name $col1
            $status = Center-Text ($versions -join ', ') $col2
            Write-Host -NoNewline "|"
            Write-Host -NoNewline $ferramenta -ForegroundColor Green
            Write-Host -NoNewline "|"
            Write-Host -NoNewline $status -ForegroundColor Green
            Write-Host "|"
        } else {
            $ferramenta = Center-Text $comp.name $col1
            $status = Center-Text 'NÃO INSTALADO' $col2
            Write-Host -NoNewline "|"
            Write-Host -NoNewline $ferramenta -ForegroundColor Red
            Write-Host -NoNewline "|"
            Write-Host -NoNewline $status -ForegroundColor Red
            Write-Host "|"
        }
    }
    Write-Host ("¯" * ($col1 + $col2 + 3))
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

function List-MongoDBVersions {
    $json = Invoke-RestMethod -Uri "https://www.mongodb.com/try/download/community/json"
    $versions = $json.versions | Where-Object { $_.platform -eq "windows" } | Select-Object -ExpandProperty version
    Write-Host "Versões de MongoDB disponíveis para Windows:" 
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-RedisVersions {
    $page = Invoke-WebRequest -Uri "https://github.com/microsoftarchive/redis/releases"
    $matches = [regex]::Matches($page.Content, "/microsoftarchive/redis/releases/tag/([\d\.]+)")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
    Write-Host "Versões de Redis para Windows (Microsoft Archive):"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-PgSQLVersions {
    $page = Invoke-WebRequest -Uri "https://www.enterprisedb.com/downloads/postgres-postgresql-downloads"
    $matches = [regex]::Matches($page.Content, "PostgreSQL ([\d\.]+)")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
    Write-Host "Versões de PostgreSQL disponíveis para Windows:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-MailHogVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/mailhog/MailHog/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de MailHog disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-ElasticsearchVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/elastic/elasticsearch/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de Elasticsearch disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-MemcachedVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/memcached/memcached/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de Memcached disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-DockerVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/docker/cli/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de Docker CLI disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-YarnVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/yarnpkg/yarn/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de Yarn disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-PnpmVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/pnpm/pnpm/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de pnpm disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-WPCLIVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/wp-cli/wp-cli/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de WP-CLI disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-AdminerVersions {
    $page = Invoke-WebRequest -Uri "https://www.adminer.org/en/"
    $matches = [regex]::Matches($page.Content, "Adminer ([\d\.]+)")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
    Write-Host "Versões de Adminer disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-PoetryVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/python-poetry/poetry/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de Poetry disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-RubyVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/oneclick/rubyinstaller2/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de RubyInstaller2 disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-GoVersions {
    $json = Invoke-RestMethod -Uri "https://go.dev/dl/?mode=json"
    $versions = $json | Select-Object -ExpandProperty version
    Write-Host "Versões de Go disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-CertbotVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/certbot/certbot/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de Certbot disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-OpenSSLVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/slproweb/openssl/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    Write-Host "Versões de OpenSSL (SLProWeb) disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}