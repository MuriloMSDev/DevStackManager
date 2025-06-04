param(
    [Parameter(Mandatory=$true, Position=0)]
    [ValidateSet("install", "site", "uninstall", "path", "list", "start", "stop", "restart")]
    [string]$Command,

    [Parameter(Position=1, ValueFromRemainingArguments=$true)]
    [string[]]$Args
)

$baseDir = "C:\devstack"
$nginxDir = "$baseDir\nginx"
$phpDir = "$baseDir\php"
$mysqlDir = "$baseDir\mysql"
$nodeDir = "$baseDir\nodejs"
$pythonDir = "$baseDir\python"
$composerDir = "$baseDir\composer"
$nginxSitesDir = "conf\sites-enabled"

. "$PSScriptRoot\install.ps1"
. "$PSScriptRoot\uninstall.ps1"
. "$PSScriptRoot\path.ps1"
. "$PSScriptRoot\list.ps1"
. "$PSScriptRoot\process.ps1"

switch ($Command) {
    "list" {
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
    }
    "site" {
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 site <dominio> [-root <diretorio>] [-php <php-upstream>] [-nginx <nginx-version>]"
            exit 1
        }
        $Domain = $Args[0]

        for ($i = 1; $i -lt $Args.Count; $i++) {
            switch ($Args[$i]) {
                "-root" {
                    $i++; if ($i -lt $Args.Count) { $Root = $Args[$i] }
                }
                "-php" {
                    $i++; if ($i -lt $Args.Count) { $PhpUpstream = $Args[$i] }
                }
                "-nginx" {
                    $i++; if ($i -lt $Args.Count) { $NginxVersion = $Args[$i] }
                }
                "-index" {
                    $i++; if ($i -lt $Args.Count) { $IndexLocation = $Args[$i] }
                }
            }
        }
        Create-NginxSiteConfig -Domain $Domain -Root $Root -PhpUpstream $PhpUpstream -NginxVersion $NginxVersion -IndexLocation $IndexLocation
    }
    "install" {
        foreach ($component in $Args) {
            switch -Regex ($component) {
                "^php-(.+)$"         { Install-PHP ($component -replace "^php-") }
                "^php$"              { Install-PHP }
                "^nginx$"            { Install-Nginx }
                "^mysql-(.+)$"       { Install-MySQL ($component -replace "^mysql-") }
                "^mysql$"            { Install-MySQL }
                "^nodejs-(.+)$"      { Install-NodeJS ($component -replace "^nodejs-") }
                "^(nodejs|node)$"    { Install-NodeJS }
                "^python-(.+)$"      { Install-Python ($component -replace "^python-") }
                "^python$"           { Install-Python }
                "^composer-(.+)$"    { Install-Composer ($component -replace "^composer-") }
                "^composer$"         { Install-Composer }
                "^phpmyadmin-(.+)$"  { Install-PhpMyAdmin ($component -replace "^phpmyadmin-") }
                "^phpmyadmin$"       { Install-PhpMyAdmin }
                default              { Write-Host "Componente desconhecido: $component" }
            }
        }
        Add-BinDirsToPath
    }
    "path" {
        Add-BinDirsToPath
    }
    "uninstall" {
        foreach ($component in $Args) {
            switch -Regex ($component) {
                "^php-(.+)$"         { Uninstall-PHP ($component -replace "^php-") }
                "^php$"              { Uninstall-PHP }
                "^nginx$"            { Uninstall-Nginx }
                "^mysql-(.+)$"       { Uninstall-MySQL ($component -replace "^mysql-") }
                "^mysql$"            { Uninstall-MySQL }
                "^nodejs-(.+)$"      { Uninstall-NodeJS ($component -replace "^nodejs-") }
                "^(nodejs|node)$"    { Uninstall-NodeJS }
                "^python-(.+)$"      { Uninstall-Python ($component -replace "^python-") }
                "^python$"           { Uninstall-Python }
                "^composer-(.+)$"    { Uninstall-Composer ($component -replace "^composer-") }
                "^composer$"         { Uninstall-Composer }
                "^phpmyadmin-(.+)$"  { Uninstall-PhpMyAdmin ($component -replace "^phpmyadmin-") }
                "^phpmyadmin$"       { Uninstall-PhpMyAdmin }
                default              { Write-Host "Componente desconhecido: $component" }
            }
        }
        Write-Host "Uninstall finalizado."
    }
    "start" {
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 start <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            ForEach-Version "nginx" { param($v) & $PSCommandPath start nginx $v }
            ForEach-Version "php"   { param($v) & $PSCommandPath start php $v }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 start <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Start-Component $target $version
    }
    "stop" {
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 stop <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            ForEach-Version "nginx" { param($v) & $PSCommandPath stop nginx $v }
            ForEach-Version "php"   { param($v) & $PSCommandPath stop php $v }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 stop <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Stop-Component $target $version
    }
    "restart" {
        if ($Args.Count -lt 1) {
            Write-Host "Uso: setup.ps1 restart <nginx|php|--all> [<x.x.x>]"
            exit 1
        }
        $target = $Args[0].ToLower()
        if ($target -eq "--all") {
            ForEach-Version "nginx" { param($v) & $PSCommandPath restart nginx $v }
            ForEach-Version "php"   { param($v) & $PSCommandPath restart php $v }
            return
        }
        if ($Args.Count -lt 2) {
            Write-Host "Uso: setup.ps1 restart <nginx|php> <x.x.x>"
            exit 1
        }
        $version = $Args[1]
        Stop-Component $target $version
        Start-Sleep -Seconds 1
        Start-Component $target $version
    }
    default {
        Write-Host "Comando desconhecido: $Command"
    }
}