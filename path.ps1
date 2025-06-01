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
        Get-ChildItem $composerDir -Directory | ForEach-Object {
            $composerPhar = Get-ChildItem $_.FullName -Filter "composer-*.phar" -File -ErrorAction SilentlyContinue
            if ($composerPhar) {
                $pathsToAdd += $_.FullName
            }
        }
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