function Uninstall-Commands {
    foreach ($component in $args) {
        switch -Regex ($component) {
            "^php-(.+)$"           { Uninstall-PHP ($component -replace "^php-") }
            "^php$"                { Uninstall-PHP }
            "^nginx$"              { Uninstall-Nginx }
            "^mysql-(.+)$"         { Uninstall-MySQL ($component -replace "^mysql-") }
            "^mysql$"              { Uninstall-MySQL }
            "^nodejs-(.+)$"        { Uninstall-NodeJS ($component -replace "^nodejs-") }
            "^(nodejs|node)$"      { Uninstall-NodeJS }
            "^python-(.+)$"        { Uninstall-Python ($component -replace "^python-") }
            "^python$"             { Uninstall-Python }
            "^composer-(.+)$"      { Uninstall-Composer ($component -replace "^composer-") }
            "^composer$"           { Uninstall-Composer }
            "^phpmyadmin-(.+)$"    { Uninstall-PhpMyAdmin ($component -replace "^phpmyadmin-") }
            "^phpmyadmin$"         { Uninstall-PhpMyAdmin }
            "^git-(.+)$"           { Uninstall-Git ($component -replace "^git-") }
            "^git$"                { Uninstall-Git }
            "^mongodb-(.+)$"       { Uninstall-MongoDB ($component -replace "^mongodb-") }
            "^mongodb$"            { Uninstall-MongoDB }
            "^redis-(.+)$"         { Uninstall-Redis ($component -replace "^redis-") }
            "^redis$"              { Uninstall-Redis }
            "^pgsql-(.+)$"         { Uninstall-PgSQL ($component -replace "^pgsql-") }
            "^pgsql$"              { Uninstall-PgSQL }
            "^mailhog-(.+)$"       { Uninstall-MailHog ($component -replace "^mailhog-") }
            "^mailhog$"            { Uninstall-MailHog }
            "^elasticsearch-(.+)$" { Uninstall-Elasticsearch ($component -replace "^elasticsearch-") }
            "^elasticsearch$"      { Uninstall-Elasticsearch }
            "^memcached-(.+)$"     { Uninstall-Memcached ($component -replace "^memcached-") }
            "^memcached$"          { Uninstall-Memcached }
            "^docker-(.+)$"        { Uninstall-Docker ($component -replace "^docker-") }
            "^docker$"             { Uninstall-Docker }
            "^yarn-(.+)$"          { Uninstall-Yarn ($component -replace "^yarn-") }
            "^yarn$"               { Uninstall-Yarn }
            "^pnpm-(.+)$"          { Uninstall-Pnpm ($component -replace "^pnpm-") }
            "^pnpm$"               { Uninstall-Pnpm }
            "^wpcli-(.+)$"         { Uninstall-WPCLI ($component -replace "^wpcli-") }
            "^wpcli$"              { Uninstall-WPCLI }
            "^adminer-(.+)$"       { Uninstall-Adminer ($component -replace "^adminer-") }
            "^adminer$"            { Uninstall-Adminer }
            "^poetry-(.+)$"        { Uninstall-Poetry ($component -replace "^poetry-") }
            "^poetry$"             { Uninstall-Poetry }
            "^ruby-(.+)$"          { Uninstall-Ruby ($component -replace "^ruby-") }
            "^ruby$"               { Uninstall-Ruby }
            "^go-(.+)$"            { Uninstall-Go ($component -replace "^go-") }
            "^go$"                 { Uninstall-Go }
            "^certbot-(.+)$"       { Uninstall-Certbot ($component -replace "^certbot-") }
            "^certbot$"            { Uninstall-Certbot }
            "^openssl-(.+)$"       { Uninstall-OpenSSL ($component -replace "^openssl-") }
            "^openssl$"            { Uninstall-OpenSSL }
            "^php-cs-fixer-(.+)$"  { Uninstall-PHPCsFixer ($component -replace "^php-cs-fixer-") }
            "^php-cs-fixer$"       { Uninstall-PHPCsFixer }
            default                { Write-Host "Componente desconhecido: $component" }
        }
    }
    Write-Host "Uninstall finalizado."
}

function Uninstall-GenericTool {
    param(
        [string]$ToolDir,
        [string]$SubDir
    )
    $targetDir = Join-Path $ToolDir $SubDir
    if (Test-Path $targetDir) {
        Remove-Item -Recurse -Force $targetDir
        Remove-FromPath @($targetDir)
        Write-Host "$SubDir removido."
    } else {
        Write-Host "$SubDir não está instalado."
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
        $phpSubDir = "php-$version"
        Uninstall-GenericTool -ToolDir $phpDir -SubDir $phpSubDir
    } else {
        Get-ChildItem $phpDir -Directory | ForEach-Object {
            Uninstall-GenericTool -ToolDir $phpDir -SubDir $_.Name
        }
    }
}

function Uninstall-MySQL {
    param($version = "8.0.36")
    if ($version) {
        $mysqlSubDir = "mysql-$version-winx64"
        Uninstall-GenericTool -ToolDir $mysqlDir -SubDir $mysqlSubDir
    } else {
        Get-ChildItem $mysqlDir -Directory | ForEach-Object {
            Uninstall-GenericTool -ToolDir $mysqlDir -SubDir $_.Name
        }
    }
}

function Uninstall-NodeJS {
    param($version)
    if ($version) {
        $nodeSubDir = "node-v$version-win-x64"
        Uninstall-GenericTool -ToolDir $nodeDir -SubDir $nodeSubDir
    } else {
        Get-ChildItem $nodeDir -Directory | ForEach-Object {
            Uninstall-GenericTool -ToolDir $nodeDir -SubDir $_.Name
        }
    }
}

function Uninstall-Python {
    param($version)
    if ($version) {
        $pySubDir = "python-$version"
        Uninstall-GenericTool -ToolDir $pythonDir -SubDir $pySubDir
    } else {
        Get-ChildItem $pythonDir -Directory | ForEach-Object {
            Uninstall-GenericTool -ToolDir $pythonDir -SubDir $_.Name
        }
    }
}

function Uninstall-Composer {
    param($version)
    if ($version) {
        $composerSubDir = "composer-$version"
        Uninstall-GenericTool -ToolDir $composerDir -SubDir $composerSubDir
    } else {
        Get-ChildItem $composerDir -Directory | ForEach-Object {
            Uninstall-GenericTool -ToolDir $composerDir -SubDir $_.Name
        }
    }
}

function Uninstall-Nginx {
    if (Test-Path $nginxDir) {
        Get-ChildItem $nginxDir -Directory | ForEach-Object {
            Uninstall-GenericTool -ToolDir $nginxDir -SubDir $_.Name
        }
    } else {
        Write-Host "Nginx não está instalado."
    }
}

function Uninstall-PhpMyAdmin {
    param($version)
    if ($version) {
        $pmaSubDir = "phpmyadmin-$version"
        Uninstall-GenericTool -ToolDir $pmaDir -SubDir $pmaSubDir
    } else {
        Get-ChildItem $pmaDir -Directory | ForEach-Object {
            Uninstall-GenericTool -ToolDir $pmaDir -SubDir $_.Name
        }
    }
}

function Uninstall-Git {
    param($version)
    if ($version) {
        $gitSubDir = "git-$version"
        Uninstall-GenericTool -ToolDir $baseDir -SubDir $gitSubDir
    } else {
        Get-ChildItem $baseDir -Directory | Where-Object { $_.Name -like 'git-*' } | ForEach-Object {
            Uninstall-GenericTool -ToolDir $baseDir -SubDir $_.Name
        }
    }
}

function Uninstall-MongoDB {
    param($version)
    if ($version) {
        $mongoSubDir = "mongodb-$version"
        Uninstall-GenericTool -ToolDir $mongoDir -SubDir $mongoSubDir
    } else {
        if (Test-Path $mongoDir) {
            Get-ChildItem $mongoDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $mongoDir -SubDir $_.Name
            }
        } else {
            Write-Host "MongoDB não está instalado."
        }
    }
}

function Uninstall-Redis {
    param($version)
    if ($version) {
        $redisSubDir = "redis-$version"
        Uninstall-GenericTool -ToolDir $redisDir -SubDir $redisSubDir
    } else {
        if (Test-Path $redisDir) {
            Get-ChildItem $redisDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $redisDir -SubDir $_.Name
            }
        } else {
            Write-Host "Redis não está instalado."
        }
    }
}

function Uninstall-PgSQL {
    param($version)
    if ($version) {
        $pgsqlSubDir = "pgsql-$version"
        Uninstall-GenericTool -ToolDir $pgsqlDir -SubDir $pgsqlSubDir
    } else {
        if (Test-Path $pgsqlDir) {
            Get-ChildItem $pgsqlDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $pgsqlDir -SubDir $_.Name
            }
        } else {
            Write-Host "PostgreSQL não está instalado."
        }
    }
}

function Uninstall-MailHog {
    param($version)
    if ($version) {
        $mailhogSubDir = "mailhog-$version"
        Uninstall-GenericTool -ToolDir $mailhogDir -SubDir $mailhogSubDir
    } else {
        if (Test-Path $mailhogDir) {
            Get-ChildItem $mailhogDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $mailhogDir -SubDir $_.Name
            }
        } else {
            Write-Host "MailHog não está instalado."
        }
    }
}

function Uninstall-Elasticsearch {
    param($version)
    if ($version) {
        $elasticSubDir = "elasticsearch-$version"
        Uninstall-GenericTool -ToolDir $elasticDir -SubDir $elasticSubDir
    } else {
        if (Test-Path $elasticDir) {
            Get-ChildItem $elasticDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $elasticDir -SubDir $_.Name
            }
        } else {
            Write-Host "Elasticsearch não está instalado."
        }
    }
}

function Uninstall-Memcached {
    param($version)
    if ($version) {
        $memcachedSubDir = "memcached-$version"
        Uninstall-GenericTool -ToolDir $memcachedDir -SubDir $memcachedSubDir
    } else {
        if (Test-Path $memcachedDir) {
            Get-ChildItem $memcachedDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $memcachedDir -SubDir $_.Name
            }
        } else {
            Write-Host "Memcached não está instalado."
        }
    }
}

function Uninstall-Docker {
    param($version)
    if ($version) {
        $dockerSubDir = "docker-$version"
        Uninstall-GenericTool -ToolDir $dockerDir -SubDir $dockerSubDir
    } else {
        if (Test-Path $dockerDir) {
            Get-ChildItem $dockerDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $dockerDir -SubDir $_.Name
            }
        } else {
            Write-Host "Docker não está instalado."
        }
    }
}

function Uninstall-Yarn {
    param($version)
    if ($version) {
        $yarnSubDir = "yarn-$version"
        Uninstall-GenericTool -ToolDir $yarnDir -SubDir $yarnSubDir
    } else {
        if (Test-Path $yarnDir) {
            Get-ChildItem $yarnDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $yarnDir -SubDir $_.Name
            }
        } else {
            Write-Host "Yarn não está instalado."
        }
    }
}

function Uninstall-Pnpm {
    param($version)
    if ($version) {
        $pnpmSubDir = "pnpm-$version"
        Uninstall-GenericTool -ToolDir $pnpmDir -SubDir $pnpmSubDir
    } else {
        if (Test-Path $pnpmDir) {
            Get-ChildItem $pnpmDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $pnpmDir -SubDir $_.Name
            }
        } else {
            Write-Host "pnpm não está instalado."
        }
    }
}

function Uninstall-WPCLI {
    param($version)
    if ($version) {
        $wpcliSubDir = "wpcli-$version"
        Uninstall-GenericTool -ToolDir $wpcliDir -SubDir $wpcliSubDir
    } else {
        if (Test-Path $wpcliDir) {
            Get-ChildItem $wpcliDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $wpcliDir -SubDir $_.Name
            }
        } else {
            Write-Host "WP-CLI não está instalado."
        }
    }
}

function Uninstall-Adminer {
    param($version)
    if ($version) {
        $adminerSubDir = "adminer-$version"
        Uninstall-GenericTool -ToolDir $adminerDir -SubDir $adminerSubDir
    } else {
        if (Test-Path $adminerDir) {
            Get-ChildItem $adminerDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $adminerDir -SubDir $_.Name
            }
        } else {
            Write-Host "Adminer não está instalado."
        }
    }
}

function Uninstall-Poetry {
    param($version)
    if ($version) {
        $poetrySubDir = "poetry-$version"
        Uninstall-GenericTool -ToolDir $poetryDir -SubDir $poetrySubDir
    } else {
        if (Test-Path $poetryDir) {
            Get-ChildItem $poetryDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $poetryDir -SubDir $_.Name
            }
        } else {
            Write-Host "Poetry não está instalado."
        }
    }
}

function Uninstall-Ruby {
    param($version)
    if ($version) {
        $rubySubDir = "ruby-$version"
        Uninstall-GenericTool -ToolDir $rubyDir -SubDir $rubySubDir
    } else {
        if (Test-Path $rubyDir) {
            Get-ChildItem $rubyDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $rubyDir -SubDir $_.Name
            }
        } else {
            Write-Host "Ruby não está instalado."
        }
    }
}

function Uninstall-Go {
    param($version)
    if ($version) {
        $goSubDir = "go-$version"
        Uninstall-GenericTool -ToolDir $goDir -SubDir $goSubDir
    } else {
        if (Test-Path $goDir) {
            Get-ChildItem $goDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $goDir -SubDir $_.Name
            }
        } else {
            Write-Host "Go não está instalado."
        }
    }
}

function Uninstall-Certbot {
    param($version)
    if ($version) {
        $certbotSubDir = "certbot-$version"
        Uninstall-GenericTool -ToolDir $certbotDir -SubDir $certbotSubDir
    } else {
        if (Test-Path $certbotDir) {
            Get-ChildItem $certbotDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $certbotDir -SubDir $_.Name
            }
        } else {
            Write-Host "Certbot não está instalado."
        }
    }
}

function Uninstall-OpenSSL {
    param($version)
    if ($version) {
        $opensslSubDir = "openssl-$version"
        Uninstall-GenericTool -ToolDir $openSSLDir -SubDir $opensslSubDir
    } else {
        if (Test-Path $openSSLDir) {
            Get-ChildItem $openSSLDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $openSSLDir -SubDir $_.Name
            }
        } else {
            Write-Host "OpenSSL não está instalado."
        }
    }
}

function Uninstall-PHPCsFixer {
    param($version)
    if ($version) {
        $phpcsfixerSubDir = "php-cs-fixer-$version"
        Uninstall-GenericTool -ToolDir $phpcsfixerDir -SubDir $phpcsfixerSubDir
    } else {
        if (Test-Path $phpcsfixerDir) {
            Get-ChildItem $phpcsfixerDir -Directory | ForEach-Object {
                Uninstall-GenericTool -ToolDir $phpcsfixerDir -SubDir $_.Name
            }
        } else {
            Write-Host "PHP CS Fixer não está instalado."
        }
    }
}