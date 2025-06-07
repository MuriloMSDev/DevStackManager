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
        @{ name = 'certbot'; dir = $certbotDir }
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
            $status = Center-Text '(não instalado)' $col2
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
            $status = Center-Text '(não instalado)' $col2
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