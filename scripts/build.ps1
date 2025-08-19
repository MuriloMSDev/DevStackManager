# DevStackManager Unified Build Script
# This script consolidates build, installer, uninstaller and locale operations for performance and efficiency.
# Usage: .\build.ps1 [-WithInstaller]

param(
    [switch]$WithInstaller = $false
)

$ErrorActionPreference = "Stop"

# Directories
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$srcDir = Join-Path $rootDir "src"
$releaseDir = Join-Path $rootDir "release"
$installDir = Join-Path $rootDir "install"
$versionFile = Join-Path $rootDir "VERSION"

function Build-Projects {
    Write-Host "=== DevStack Build ===" -ForegroundColor Green
    # Stop running processes
    foreach ($proc in @("DevStackGUI", "DevStack")) {
        $running = Get-Process -Name $proc -ErrorAction SilentlyContinue
        if ($running) { $running | Stop-Process -Force }
    }
    # Clean previous builds
    foreach ($dir in @("bin", "obj")) {
        if (Test-Path $dir) { Remove-Item -Recurse -Force $dir }
    }
    # Build CLI & GUI
    dotnet publish "$srcDir\CLI\DevStackCLI.csproj" -c Release -r win-x64 --self-contained true
    if ($LASTEXITCODE -ne 0) { throw "DevStack CLI build failed" }
    dotnet publish "$srcDir\GUI\DevStackGUI.csproj" -c Release -r win-x64 --self-contained true
    if ($LASTEXITCODE -ne 0) { throw "DevStack GUI build failed" }
    # Deploy to release
    if (!(Test-Path $releaseDir)) { New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null }
    Get-ChildItem $releaseDir | Remove-Item -Recurse -Force
    Copy-Item "$srcDir\CLI\bin\Release\net9.0-windows\win-x64\publish\DevStack.exe" "$releaseDir\DevStack.exe" -Force
    Copy-Item "$srcDir\GUI\bin\Release\net9.0-windows\win-x64\publish\DevStackGUI.exe" "$releaseDir\DevStackGUI.exe" -Force
    Copy-Item "$srcDir\Shared\DevStack.ico" "$releaseDir\DevStack.ico" -Force
    if (Test-Path "configs") { Copy-Item "configs" "$releaseDir\configs" -Recurse -Force }
    Write-Host "Build and deploy complete." -ForegroundColor Green
}

function Build-Uninstaller {
    Write-Host "Building Uninstaller..." -ForegroundColor Yellow
    $uninstallerSrcPath = Join-Path $srcDir "UNINSTALLER"
    Push-Location $uninstallerSrcPath
    dotnet publish -c Release -p:PublishSingleFile=true -p:SelfContained=true -r win-x64
    if ($LASTEXITCODE -ne 0) { throw "Uninstaller build failed" }
    $uninstallerBinPath = "$uninstallerSrcPath\bin\Release\net9.0-windows\win-x64\publish"
    $uninstallerExeName = "DevStack-Uninstaller.exe"
    $exeFiles = Get-ChildItem $uninstallerBinPath -Filter "*.exe" -ErrorAction SilentlyContinue
    if ($exeFiles.Count -eq 0) { throw "No uninstaller executable found" }
    Copy-Item $exeFiles[0].FullName "$releaseDir\$uninstallerExeName" -Force
    Pop-Location
    Write-Host "Uninstaller built and copied." -ForegroundColor Green
}

function Build-Installer {
    Write-Host "Building Installer..." -ForegroundColor Yellow
    # Clean installer dir
    if (Test-Path $installDir) { Remove-Item $installDir -Recurse -Force }
    New-Item -Path $installDir -ItemType Directory -Force | Out-Null
    if (!(Test-Path $versionFile)) { throw "VERSION file not found" }
    $version = (Get-Content $versionFile).Trim()
    if (!(Test-Path $releaseDir)) { throw "Release directory not found. Build projects first." }
    $releaseFiles = Get-ChildItem $releaseDir -File
    if ($releaseFiles.Count -eq 0) { throw "No files in release directory. Build projects first." }
    Build-Uninstaller
    $zipPath = Join-Path $installDir "DevStack.zip"
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory($releaseDir, $zipPath)
    $installerSrcPath = Join-Path $srcDir "INSTALLER"
    Push-Location $installerSrcPath
    dotnet publish -c Release -p:PublishSingleFile=true -p:SelfContained=true -r win-x64
    if ($LASTEXITCODE -ne 0) { throw "Installer build failed" }
    $installerBinPath = "$installerSrcPath\bin\Release\net9.0-windows\win-x64\publish"
    $installerExeName = "DevStack-$version-Installer.exe"
    $exeFiles = Get-ChildItem $installerBinPath -Filter "*.exe" -ErrorAction SilentlyContinue
    if ($exeFiles.Count -eq 0) { throw "No installer executable found" }
    $targetInstallerPath = Join-Path $installDir $installerExeName
    Copy-Item $exeFiles[0].FullName $targetInstallerPath -Force
    $zipInstallerPath = $targetInstallerPath.Replace('.exe', '.zip')
    if (Test-Path $zipInstallerPath) { Remove-Item $zipInstallerPath -Force }
    $zip = [System.IO.Compression.ZipFile]::Open($zipInstallerPath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $targetInstallerPath, [System.IO.Path]::GetFileName($targetInstallerPath)) | Out-Null
    } finally {
        $zip.Dispose()
    }
    Pop-Location
    if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
    Write-Host "Installer built and compacted." -ForegroundColor Green
}

function Copy-LocaleFiles {
    Write-Host "Copying locale files..." -ForegroundColor Magenta
    $SharedLocaleDir = Join-Path $srcDir "Shared\locale"
    $InstallerBuildDir = Join-Path $srcDir "INSTALLER\bin\Release\net9.0-windows\win-x64\publish"
    $UninstallerBuildDir = Join-Path $srcDir "UNINSTALLER\bin\Release\net9.0-windows\win-x64\publish"
    $InstallerInstallDir = $installDir
    $InstallerLocaleDir = Join-Path $InstallerBuildDir "locale"
    $UninstallerLocaleDir = Join-Path $UninstallerBuildDir "locale"
    foreach ($dir in @($InstallerLocaleDir, $UninstallerLocaleDir)) {
        if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    }
    $LocaleFiles = Get-ChildItem -Path $SharedLocaleDir -Filter "*.json"
    foreach ($file in $LocaleFiles) {
        Copy-Item -Path $file.FullName -Destination (Join-Path $InstallerLocaleDir $file.Name) -Force | Out-Null
        Copy-Item -Path $file.FullName -Destination (Join-Path $UninstallerLocaleDir $file.Name) -Force | Out-Null
    }
    $InstallerLocaleDir = Join-Path $InstallerInstallDir "locale"
    if (-not (Test-Path $InstallerLocaleDir)) { New-Item -ItemType Directory -Path $InstallerLocaleDir -Force | Out-Null }
    foreach ($file in $LocaleFiles) {
        Copy-Item -Path $file.FullName -Destination (Join-Path $InstallerLocaleDir $file.Name) -Force | Out-Null
    }
    Write-Host "Locale files copied." -ForegroundColor Green
}

# Main execution
${startTime} = Get-Date
Build-Projects
Copy-LocaleFiles
if ($WithInstaller) { Build-Installer }
${endTime} = Get-Date
$elapsed = ($endTime - $startTime).TotalSeconds
Write-Host ("All operations complete. ({0:N1}s)" -f $elapsed) -ForegroundColor Green
