function Start-Component {
    param(
        [string]$Component,
        [string]$Version
    )
    switch ($Component) {
        "nginx" {
            $nginxExe = Join-Path $nginxDir "nginx-$Version\nginx-$Version.exe"
            $nginxWorkDir = Join-Path $nginxDir "nginx-$Version"
            if (Test-Path $nginxExe) {
                $proc = Get-Process | Where-Object { $_.Path -eq $nginxExe }
                if ($proc) {
                    Write-Host "Nginx $Version já está em execução."
                } else {
                    Start-Process -FilePath $nginxExe -WorkingDirectory $nginxWorkDir -NoNewWindow
                    Write-Host "Nginx $Version iniciado."
                }
            } else {
                Write-Host "Nginx $Version não encontrado."
            }
        }
        "php" {
            $phpExe = Join-Path $phpDir "php-$Version\php-cgi-$Version.exe"
            $phpWorkDir = Join-Path $phpDir "php-$Version"
            if (Test-Path $phpExe) {
                $proc = Get-Process | Where-Object { $_.Path -eq $phpExe }
                if ($proc) {
                    Write-Host "php-cgi $Version já está em execução."
                } else {
                    Start-Process -FilePath $phpExe -ArgumentList "-b 127.${Version}:9000" -WorkingDirectory $phpWorkDir -NoNewWindow
                    Write-Host "php-cgi $Version iniciado."
                }
            } else {
                Write-Host "php-cgi $Version não encontrado."
            }
        }
        default {
            Write-Host "Componente desconhecido: $Component"
        }
    }
}

function Stop-Component {
    param(
        [string]$Component,
        [string]$Version
    )
    switch ($Component) {
        "nginx" {
            $nginxExe = Join-Path $nginxDir "nginx-$Version\nginx-$Version.exe"
            if (Test-Path $nginxExe) {
                $proc = Get-Process | Where-Object { $_.Path -eq $nginxExe }
                if ($proc) {
                    Stop-Process -Id $proc.Id -Force
                    Write-Host "Nginx $Version parado."
                } else {
                    Write-Host "Nginx $Version não está em execução."
                }
            } else {
                Write-Host "Nginx $Version não encontrado."
            }
        }
        "php" {
            $phpExe = Join-Path $phpDir "php-$Version\php-cgi-$Version.exe"
            $procs = Get-Process | Where-Object { $_.Path -eq $phpExe }
            if ($procs) {
                $procs | ForEach-Object { Stop-Process -Id $_.Id -Force }
                Write-Host "php-cgi $Version parado."
            } else {
                Write-Host "php-cgi $Version não está em execução."
            }
        }
        default {
            Write-Host "Componente desconhecido: $Component"
        }
    }
}

function ForEach-Version ($Component, [ScriptBlock]$Action) {
    $dir = switch ($Component) {
        "nginx" { $nginxDir }
        "php"   { $phpDir }
        default { return }
    }
    $prefix = "$Component-"
    Get-ChildItem -Path $dir -Directory -Filter "$prefix*" | ForEach-Object {
        $version = $_.Name -replace "^$prefix"
        & $Action $version
    }
}