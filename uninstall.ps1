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
        $composerPhar = "composer-$version.phar"
        Uninstall-GenericTool -ToolDir $composerDir -SubDir $composerPhar
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
        Uninstall-GenericTool -ToolDir $baseDir -SubDir $pmaSubDir
    } else {
        Get-ChildItem $pmaDir -Directory | ForEach-Object {
            Uninstall-GenericTool -ToolDir $pmaDir -SubDir $_.Name
        }
    }
}