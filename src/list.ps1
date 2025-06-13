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
        @{ name = 'openssl'; dir = $opensslDir },
        @{ name = 'php-cs-fixer'; dir = $phpcsfixerDir }
    )
    # Tabela de Ferramentas Instaladas
    $col1 = 15; $col2 = 40
    $header = ('_' * ($col1 + $col2 + 3))
    Write-Host $header -ForegroundColor Gray
    Write-Host ("|{0}|{1}|" -f (Center-Text 'Ferramenta' $col1), (Center-Text 'Versões Instaladas' $col2)) -ForegroundColor Gray
    Write-Host ("|" + ('-' * $col1) + "+" + ('-' * $col2) + "|") -ForegroundColor Gray
    foreach ($comp in $components) {
        if (-not (Test-Path $comp.dir)) {
            $ferramenta = Center-Text $comp.name $col1
            $status = Center-Text 'NÃO INSTALADO' $col2
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $ferramenta -ForegroundColor Red
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $status -ForegroundColor Red
            Write-Host "|" -ForegroundColor Gray
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
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $ferramenta -ForegroundColor Green
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $status -ForegroundColor Green
            Write-Host "|" -ForegroundColor Gray
        } else {
            $ferramenta = Center-Text $comp.name $col1
            $status = Center-Text 'NÃO INSTALADO' $col2
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $ferramenta -ForegroundColor Red
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $status -ForegroundColor Red
            Write-Host "|" -ForegroundColor Gray
        }
    }
    Write-Host ("¯" * ($col1 + $col2 + 3)) -ForegroundColor Gray
}

function Print-HorizontalTable {
    param(
        [string[]]$Items,
        [int]$Cols = 6,
        [string]$Header = "Versões disponíveis:",
        [string[]]$Installed = @(),
        [boolean]$OrderDescending = $true
    )
    if (-not $Items -or $Items.Count -eq 0) {
        Write-Host "Nenhuma versão encontrada."
        return
    }
    # Ordenar da maior para menor (descendente)
    if ($OrderDescending) {
        $Items = $Items | Sort-Object -Descending
    }
    $total = $Items.Count
    $rows = [Math]::Ceiling($total / $Cols)
    $width = 16
    $tableWidth = ($width * $Cols) + $Cols + 1
    Write-Host $Header
    Write-Host ("-" * $tableWidth)
    # Preencher linhas de cima para baixo (coluna 1: maior, coluna 2: próxima maior, etc)
    for ($r = 0; $r -lt $rows; $r++) {
        $row = "|"
        for ($c = 0; $c -lt $Cols; $c++) {
            $idx = $c * $rows + $r
            if ($idx -lt $total) {
                $val = $Items[$idx] -replace '[^\d\.]', ''
                if ($Installed -contains $val) {
                    $cell = Center-Text $val $width
                    $row += "`e[32m$cell`e[0m|"  # ANSI verde
                } else {
                    $row += (Center-Text $val $width) + "|"
                }
            } else {
                $row += (Center-Text "" $width) + "|"
            }
        }
        # Corrigir cor: Write-Host não interpreta ANSI, então usar Write-Host -ForegroundColor Green para cada célula instalada
        $parts = $row -split '\|'
        Write-Host -NoNewline "|"
        for ($i = 1; $i -le $Cols; $i++) {
            $cellVal = $parts[$i]
            $cellText = $cellVal.Trim()
            if ($cellText -and $Installed -contains $cellText) {
                Write-Host -NoNewline $cellVal -ForegroundColor Green
            } else {
                Write-Host -NoNewline $cellVal -ForegroundColor Gray
            }
            Write-Host -NoNewline "|"
        }
        Write-Host ""
    }
    Write-Host ("¯" * $tableWidth)
}

function List-NginxVersions {
    $page = Invoke-WebRequest -Uri "https://nginx.org/en/download.html"
    $matches = [regex]::Matches($page.Content, "nginx-([\d\.]+)\.zip")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
    $installed = @()
    if (Test-Path $nginxDir) { $installed = Get-ChildItem $nginxDir -Directory | ForEach-Object { $_.Name -replace '^nginx-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Nginx disponíveis para Windows:" -Installed $installed
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
    $installed = @()
    if (Test-Path $phpDir) { $installed = Get-ChildItem $phpDir -Directory | ForEach-Object { $_.Name -replace '^php-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de PHP disponíveis para Windows x64:" -Installed $installed
}

function List-NodeVersions {
    $json = Invoke-RestMethod -Uri "https://nodejs.org/dist/index.json"
    $versions = $json | Select-Object -ExpandProperty version
    $installed = @()
    if (Test-Path $nodeDir) { $installed = Get-ChildItem $nodeDir -Directory | ForEach-Object { $_.Name -replace '^node-v', '' -replace '-win-x64$', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Node.js disponíveis:" -Installed $installed
}

function List-PythonVersions {
    $page = Invoke-WebRequest -Uri "https://www.python.org/downloads/windows/"
    $matches = [regex]::Matches($page.Content, "Python ([\d\.]+) ")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value }
    $versions = $versions | Sort-Object -Descending | Get-Unique
    $installed = @()
    if (Test-Path $pythonDir) { $installed = Get-ChildItem $pythonDir -Directory | ForEach-Object { $_.Name -replace '^python-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Python disponíveis:" -Installed $installed
}

function List-MongoDBVersions {
    $json = Invoke-RestMethod -Uri "https://www.mongodb.com/try/download/community/json"
    $versions = $json.versions | Where-Object { $_.platform -eq "windows" } | Select-Object -ExpandProperty version
    $installed = @()
    if (Test-Path $mongoDir) { $installed = Get-ChildItem $mongoDir -Directory | ForEach-Object { $_.Name -replace '^mongodb-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de MongoDB disponíveis para Windows:" -Installed $installed
}

function List-RedisVersions {
    $page = Invoke-WebRequest -Uri "https://github.com/microsoftarchive/redis/releases"
    $matches = [regex]::Matches($page.Content, "/microsoftarchive/redis/releases/tag/([\d\.]+)")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
    $installed = @()
    if (Test-Path $redisDir) { $installed = Get-ChildItem $redisDir -Directory | ForEach-Object { $_.Name -replace '^redis-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Redis para Windows (Microsoft Archive):" -Installed $installed
}

function List-PgSQLVersions {
    $page = Invoke-WebRequest -Uri "https://www.enterprisedb.com/downloads/postgres-postgresql-downloads"
    $matches = [regex]::Matches($page.Content, "PostgreSQL ([\d\.]+)")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
    $installed = @()
    if (Test-Path $pgsqlDir) { $installed = Get-ChildItem $pgsqlDir -Directory | ForEach-Object { $_.Name -replace '^pgsql-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de PostgreSQL disponíveis para Windows:" -Installed $installed
}

function List-MailHogVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/mailhog/MailHog/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $mailhogDir) { $installed = Get-ChildItem $mailhogDir -Directory | ForEach-Object { $_.Name -replace '^mailhog-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de MailHog disponíveis:" -Installed $installed
}

function List-ElasticsearchVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/elastic/elasticsearch/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $elasticDir) { $installed = Get-ChildItem $elasticDir -Directory | ForEach-Object { $_.Name -replace '^elasticsearch-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Elasticsearch disponíveis:" -Installed $installed
}

function List-MemcachedVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/memcached/memcached/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $memcachedDir) { $installed = Get-ChildItem $memcachedDir -Directory | ForEach-Object { $_.Name -replace '^memcached-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Memcached disponíveis:" -Installed $installed
}

function List-DockerVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/docker/cli/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $dockerDir) { $installed = Get-ChildItem $dockerDir -Directory | ForEach-Object { $_.Name -replace '^docker-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Docker CLI disponíveis:" -Installed $installed
}

function List-YarnVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/yarnpkg/yarn/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $yarnDir) { $installed = Get-ChildItem $yarnDir -Directory | ForEach-Object { $_.Name -replace '^yarn-v', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Yarn disponíveis:" -Installed $installed
}

function List-PnpmVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/pnpm/pnpm/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $pnpmDir) { $installed = Get-ChildItem $pnpmDir -Directory | ForEach-Object { $_.Name -replace '^pnpm-v', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de pnpm disponíveis:" -Installed $installed
}

function List-WPCLIVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/wp-cli/wp-cli/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $wpcliDir) { $installed = Get-ChildItem $wpcliDir -Directory | ForEach-Object { $_.Name -replace '^wp-cli-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de WP-CLI disponíveis:" -Installed $installed
}

function List-AdminerVersions {
    $page = Invoke-WebRequest -Uri "https://www.adminer.org/en/"
    $matches = [regex]::Matches($page.Content, "Adminer ([\d\.]+)")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Descending | Get-Unique
    $installed = @()
    if (Test-Path $adminerDir) { $installed = Get-ChildItem $adminerDir -Directory | ForEach-Object { $_.Name -replace '^adminer-', '' -replace '\.php$', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Adminer disponíveis:" -Installed $installed
}

function List-PoetryVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/python-poetry/poetry/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $poetryDir) { $installed = Get-ChildItem $poetryDir -Directory | ForEach-Object { $_.Name -replace '^poetry-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Poetry disponíveis:" -Installed $installed
}

function List-RubyVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/oneclick/rubyinstaller2/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $rubyDir) { $installed = Get-ChildItem $rubyDir -Directory | ForEach-Object { $_.Name -replace '^ruby-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de RubyInstaller2 disponíveis:" -Installed $installed
}

function List-GoVersions {
    $json = Invoke-RestMethod -Uri "https://go.dev/dl/?mode=json"
    $versions = $json | Select-Object -ExpandProperty version
    $installed = @()
    if (Test-Path $goDir) { $installed = Get-ChildItem $goDir -Directory | ForEach-Object { $_.Name -replace '^go', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Go disponíveis:" -Installed $installed
}

function List-CertbotVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/certbot/certbot/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $certbotDir) { $installed = Get-ChildItem $certbotDir -Directory | ForEach-Object { $_.Name -replace '^certbot-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de Certbot disponíveis:" -Installed $installed
}

function List-OpenSSLVersions {
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
    Print-HorizontalTable -Items $basevers -Header "Versões de OpenSSL (SLProWeb) disponíveis:" -Installed $installed -OrderDescending $false
}

function List-PHPCsFixerVersions {
    $json = Invoke-RestMethod -Uri "https://api.github.com/repos/PHP-CS-Fixer/PHP-CS-Fixer/releases"
    $versions = $json | Select-Object -ExpandProperty tag_name
    $installed = @()
    if (Test-Path $phpcsfixerDir) { $installed = Get-ChildItem $phpcsfixerDir -Directory | ForEach-Object { $_.Name -replace '^php-cs-fixer-', '' } }
    Print-HorizontalTable -Items $versions -Header "Versões de PHP CS Fixer disponíveis:" -Installed $installed
}