$baseDir = "C:\devstack"
$nginxDir = "$baseDir\nginx"
$phpDir = "$baseDir\php"
$mysqlDir = "$baseDir\mysql"
$nodeDir = "$baseDir\nodejs"
$pythonDir = "$baseDir\python"
$composerDir = "$baseDir\composer"
$pmaDir = "$baseDir\phpmyadmin"
$mongoDir = "$baseDir\mongodb"
$redisDir = "$baseDir\redis"
$pgsqlDir = "$baseDir\pgsql"
$mailhogDir = "$baseDir\mailhog"
$elasticDir = "$baseDir\elasticsearch"
$memcachedDir = "$baseDir\memcached"
$dockerDir = "$baseDir\docker"
$yarnDir = "$baseDir\yarn"
$pnpmDir = "$baseDir\pnpm"
$wpcliDir = "$baseDir\wpcli"
$adminerDir = "$baseDir\adminer"
$poetryDir = "$baseDir\poetry"
$rubyDir = "$baseDir\ruby"
$goDir = "$baseDir\go"
$certbotDir = "$baseDir\certbot"
$nginxSitesDir = "conf\sites-enabled"
$openSSLDir = "$baseDir\openssl"
$phpcsfixerDir = "$baseDir\phpcsfixer"
$tmpDir = "$baseDir\tmp"

Set-Variable -Name baseDir -Value $baseDir -Scope Global
Set-Variable -Name nginxDir -Value $nginxDir -Scope Global
Set-Variable -Name phpDir -Value $phpDir -Scope Global
Set-Variable -Name mysqlDir -Value $mysqlDir -Scope Global
Set-Variable -Name nodeDir -Value $nodeDir -Scope Global
Set-Variable -Name pythonDir -Value $pythonDir -Scope Global
Set-Variable -Name composerDir -Value $composerDir -Scope Global
Set-Variable -Name pmaDir -Value $pmaDir -Scope Global
Set-Variable -Name mongoDir -Value $mongoDir -Scope Global
Set-Variable -Name redisDir -Value $redisDir -Scope Global
Set-Variable -Name pgsqlDir -Value $pgsqlDir -Scope Global
Set-Variable -Name mailhogDir -Value $mailhogDir -Scope Global
Set-Variable -Name elasticDir -Value $elasticDir -Scope Global
Set-Variable -Name memcachedDir -Value $memcachedDir -Scope Global
Set-Variable -Name dockerDir -Value $dockerDir -Scope Global
Set-Variable -Name yarnDir -Value $yarnDir -Scope Global
Set-Variable -Name pnpmDir -Value $pnpmDir -Scope Global
Set-Variable -Name wpcliDir -Value $wpcliDir -Scope Global
Set-Variable -Name adminerDir -Value $adminerDir -Scope Global
Set-Variable -Name poetryDir -Value $poetryDir -Scope Global
Set-Variable -Name rubyDir -Value $rubyDir -Scope Global
Set-Variable -Name goDir -Value $goDir -Scope Global
Set-Variable -Name certbotDir -Value $certbotDir -Scope Global
Set-Variable -Name nginxSitesDir -Value $nginxSitesDir -Scope Global
Set-Variable -Name openSSLDir -Value $openSSLDir -Scope Global
Set-Variable -Name phpcsfixerDir -Value $phpcsfixerDir -Scope Global
Set-Variable -Name tmpDir -Value $tmpDir -Scope Global

. "$PSScriptRoot\install.ps1"
. "$PSScriptRoot\uninstall.ps1"
. "$PSScriptRoot\path.ps1"
. "$PSScriptRoot\data.ps1"
. "$PSScriptRoot\list.ps1"
. "$PSScriptRoot\process.ps1"
. "$PSScriptRoot\gui.ps1"