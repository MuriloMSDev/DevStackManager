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
                try {
                    $proc = Get-Process | Where-Object { $_.Path -eq $nginxExe }
                } catch { $proc = $null }
                if ($proc) {
                    Write-WarningMsg "Nginx $Version já está em execução."
                } else {
                    Start-Process -FilePath $nginxExe -WorkingDirectory $nginxWorkDir -NoNewWindow
                    Write-Info "Nginx $Version iniciado."
                    Write-Log "Nginx $Version iniciado."
                }
            } else {
                Write-ErrorMsg "Nginx $Version não encontrado."
            }
        }
        "php" {
            $phpExe = Join-Path $phpDir "php-$Version\php-cgi-$Version.exe"
            $phpWorkDir = Join-Path $phpDir "php-$Version"
            if (Test-Path $phpExe) {
                try {
                    $proc = Get-Process | Where-Object { $_.Path -eq $phpExe }
                } catch { $proc = $null }
                if ($proc) {
                    Write-WarningMsg "php-cgi $Version já está em execução."
                } else {
                    Start-Process -FilePath $phpExe -ArgumentList "-b 127.${Version}:9000" -WorkingDirectory $phpWorkDir -NoNewWindow
                    Write-Info "php-cgi $Version iniciado."
                    Write-Log "php-cgi $Version iniciado."
                }
            } else {
                Write-ErrorMsg "php-cgi $Version não encontrado."
            }
        }
        default {
            Write-ErrorMsg "Componente desconhecido: $Component"
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
                try {
                    $proc = Get-Process | Where-Object { $_.Path -eq $nginxExe }
                } catch { $proc = $null }
                if ($proc) {
                    Stop-Process -Id $proc.Id -Force
                    Write-Info "Nginx $Version parado."
                    Write-Log "Nginx $Version parado."
                } else {
                    Write-WarningMsg "Nginx $Version não está em execução."
                }
            } else {
                Write-ErrorMsg "Nginx $Version não encontrado."
            }
        }
        "php" {
            $phpExe = Join-Path $phpDir "php-$Version\php-cgi-$Version.exe"
            try {
                $procs = Get-Process | Where-Object { $_.Path -eq $phpExe }
            } catch { $procs = @() }
            if ($procs) {
                $procs | ForEach-Object { Stop-Process -Id $_.Id -Force }
                Write-Info "php-cgi $Version parado."
                Write-Log "php-cgi $Version parado."
            } else {
                Write-WarningMsg "php-cgi $Version não está em execução."
            }
        }
        default {
            Write-ErrorMsg "Componente desconhecido: $Component"
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