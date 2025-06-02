param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet("install", "site", "uninstall", "path", "list")]
    [string]$Command,

    [Parameter(Position=1)]
    [string[]]$Args
)

$baseDir = "$env:USERPROFILE\devstack"
$nginxDir = "$baseDir\nginx"
$phpDir = "$baseDir\php"
$mysqlDir = "$baseDir\mysql"
$nodeDir = "$baseDir\nodejs"
$pythonDir = "$baseDir\python"
$composerDir = "$baseDir\composer"
$nginxSitesDir = "configs\nginx\sites"

. "$PSScriptRoot\install.ps1"
. "$PSScriptRoot\uninstall.ps1"
. "$PSScriptRoot\path.ps1"
. "$PSScriptRoot\list.ps1"

if ($Command -eq "list") {
    if ($Args.Count -eq 0) {
        Write-Host "Uso: setup.ps1 list <php|nodejs|python>"
        exit 1
    }
    switch ($Args[0].ToLower()) {
        "php"     { List-PHPVersions }
        "nodejs"  { List-NodeVersions }
        "node"    { List-NodeVersions }
        "python"  { List-PythonVersions }
        default   { Write-Host "Ferramenta desconhecida: $($Args[0])" }
    }
    exit 0
}

if ($Command -eq "site") {
    if ($Args.Count -lt 1) {
        Write-Host "Uso: setup.ps1 site <dominio> [-root <diretorio>] [-php <php-upstream>]"
        exit 1
    }
    $Domain = $Args[0]
    $Root = "/var/www/$Domain"
    $PhpUpstream = "php-upstream"

    for ($i = 1; $i -lt $Args.Count; $i++) {
        switch ($Args[$i]) {
            "-root" {
                $i++; if ($i -lt $Args.Count) { $Root = $Args[$i] }
            }
            "-php" {
                $i++; if ($i -lt $Args.Count) { $PhpUpstream = $Args[$i] }
            }
        }
    }
    Create-NginxSiteConfig -Domain $Domain -Root $Root -PhpUpstream $PhpUpstream
    exit 0
}

if ($Command -eq "install") {
    foreach ($component in $Args) {
        if ($component -like "php-*") {
            $phpVersion = $component -replace "php-"
            Install-PHP $phpVersion
        } elseif ($component -eq "php") {
            Install-PHP
        } elseif ($component -eq "nginx") {
            Install-Nginx
        } elseif ($component -like "mysql-*") {
            $mysqlVersion = $component -replace "mysql-"
            Install-MySQL $mysqlVersion
        } elseif ($component -eq "mysql") {
            Install-MySQL
        } elseif ($component -like "nodejs-*") {
            $nodeVersion = $component -replace "nodejs-"
            Install-NodeJS $nodeVersion
        } elseif ($component -eq "nodejs" -or $component -eq "node") {
            Install-NodeJS
        } elseif ($component -like "python-*") {
            $pyVersion = $component -replace "python-"
            Install-Python $pyVersion
        } elseif ($component -eq "python") {
            Install-Python
        } elseif ($component -like "composer-*") {
            $composerVersion = $component -replace "composer-"
            Install-Composer $composerVersion
        } elseif ($component -eq "composer") {
            Install-Composer
        } elseif ($component -like "phpmyadmin-*") {
            $pmaVersion = $component -replace "phpmyadmin-"
            Install-PhpMyAdmin $pmaVersion
        } elseif ($component -eq "phpmyadmin") {
            Install-PhpMyAdmin
        } else {
            Write-Host "Componente desconhecido: $component"
        }
    }
    Add-BinDirsToPath
} elseif ($Command -eq "path") {
    Add-BinDirsToPath
} elseif ($Command -eq "uninstall") {
    foreach ($component in $Args) {
        if ($component -like "php-*") {
            $phpVersion = $component -replace "php-"
            Uninstall-PHP $phpVersion
        } elseif ($component -eq "php") {
            Uninstall-PHP
        } elseif ($component -eq "nginx") {
            Uninstall-Nginx
        } elseif ($component -like "mysql-*") {
            $mysqlVersion = $component -replace "mysql-"
            Uninstall-MySQL $mysqlVersion
        } elseif ($component -eq "mysql") {
            Uninstall-MySQL
        } elseif ($component -like "nodejs-*") {
            $nodeVersion = $component -replace "nodejs-"
            Uninstall-NodeJS $nodeVersion
        } elseif ($component -eq "nodejs" -or $component -eq "node") {
            Uninstall-NodeJS
        } elseif ($component -like "python-*") {
            $pyVersion = $component -replace "python-"
            Uninstall-Python $pyVersion
        } elseif ($component -eq "python") {
            Uninstall-Python
        } elseif ($component -like "composer-*") {
            $composerVersion = $component -replace "composer-"
            Uninstall-Composer $composerVersion
        } elseif ($component -eq "composer") {
            Uninstall-Composer
        } elseif ($component -like "phpmyadmin-*") {
            $pmaVersion = $component -replace "phpmyadmin-"
            Uninstall-PhpMyAdmin $pmaVersion
        } elseif ($component -eq "phpmyadmin") {
            Uninstall-PhpMyAdmin
        } else {
            Write-Host "Componente desconhecido: $component"
        }
    }
    Write-Host "Uninstall finalizado."
    exit 0
} else {
    Write-Host "Comando desconhecido: $Command"
}