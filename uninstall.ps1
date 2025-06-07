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

function Uninstall-MongoDB { param($version) Write-Host '[stub] Desinstalação do MongoDB ainda não implementada.' }
function Uninstall-Redis { param($version) Write-Host '[stub] Desinstalação do Redis ainda não implementada.' }
function Uninstall-PgSQL { param($version) Write-Host '[stub] Desinstalação do PostgreSQL ainda não implementada.' }
function Uninstall-MailHog { param($version) Write-Host '[stub] Desinstalação do MailHog ainda não implementada.' }
function Uninstall-Elasticsearch { param($version) Write-Host '[stub] Desinstalação do Elasticsearch ainda não implementada.' }
function Uninstall-Memcached { param($version) Write-Host '[stub] Desinstalação do Memcached ainda não implementada.' }
function Uninstall-Docker { param($version) Write-Host '[stub] Desinstalação do Docker ainda não implementada.' }
function Uninstall-Yarn { param($version) Write-Host '[stub] Desinstalação do Yarn ainda não implementada.' }
function Uninstall-Pnpm { param($version) Write-Host '[stub] Desinstalação do pnpm ainda não implementada.' }
function Uninstall-WPCLI { param($version) Write-Host '[stub] Desinstalação do WP-CLI ainda não implementada.' }
function Uninstall-Adminer { param($version) Write-Host '[stub] Desinstalação do Adminer ainda não implementada.' }
function Uninstall-Poetry { param($version) Write-Host '[stub] Desinstalação do Poetry ainda não implementada.' }
function Uninstall-Ruby { param($version) Write-Host '[stub] Desinstalação do Ruby ainda não implementada.' }
function Uninstall-Go { param($version) Write-Host '[stub] Desinstalação do Go ainda não implementada.' }
function Uninstall-Certbot { param($version) Write-Host '[stub] Desinstalação do Certbot ainda não implementada.' }