# DevStack Installer Build Script

$ErrorActionPreference = "Stop"

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$srcDir = Join-Path $rootDir "src"
$releaseDir = Join-Path $rootDir "release"
$installerDir = Join-Path $rootDir "installer"

Write-Host "=== DevStack Installer Build Script ===" -ForegroundColor Green
Write-Host "Root Directory: $rootDir" -ForegroundColor Yellow
Write-Host "Release Directory: $releaseDir" -ForegroundColor Yellow
Write-Host "Installer Directory: $installerDir" -ForegroundColor Yellow

# Clean directories
Write-Host "Cleaning build directories..." -ForegroundColor Yellow
if (Test-Path $installerDir) {
    Remove-Item $installerDir -Recurse -Force
}

# Create installer directory
if (!(Test-Path $installerDir)) {
    New-Item -Path $installerDir -ItemType Directory -Force | Out-Null
}

# Get version from VERSION file
$versionFile = Join-Path $rootDir "VERSION"
if (!(Test-Path $versionFile)) {
    throw "VERSION file not found at: $versionFile"
}

$version = (Get-Content $versionFile).Trim()
Write-Host "Detected version: $version" -ForegroundColor Green

# Update installer project version - skip this to avoid corruption
Write-Host "Using installer project version: $version" -ForegroundColor Green

# Check if release directory exists and has files
if (!(Test-Path $releaseDir)) {
    throw "Release directory not found: $releaseDir. Please build the projects first."
}

$releaseFiles = Get-ChildItem $releaseDir -File
if ($releaseFiles.Count -eq 0) {
    throw "No files found in release directory. Please build the projects first."
}

# Build uninstaller first
Write-Host "Building uninstaller..." -ForegroundColor Yellow
$buildUninstallerScript = Join-Path $scriptDir "build-uninstaller.ps1"
if (Test-Path $buildUninstallerScript) {
    & $buildUninstallerScript
    if ($LASTEXITCODE -ne 0) {
        throw "Uninstaller build failed"
    }
    Write-Host "Uninstaller included in release" -ForegroundColor Green
} else {
    Write-Host "Warning: Uninstaller build script not found, skipping uninstaller..." -ForegroundColor Orange
}

# Create zip file from release directory
$zipPath = Join-Path $installerDir "DevStack.zip"
$tempZipForBuild = Join-Path $srcDir "INSTALLER\DevStack.zip"

Write-Host "Creating zip file from release directory: $releaseDir" -ForegroundColor Yellow
Write-Host "Target zip file: $zipPath" -ForegroundColor Yellow

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

# Ensure we're zipping the contents of the release directory, not the directory itself
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($releaseDir, $zipPath)
Write-Host "Zip file created successfully" -ForegroundColor Green

# Copy zip to installer project directory for embedding
Copy-Item $zipPath $tempZipForBuild -Force
Write-Host "Zip file copied to installer project for embedding" -ForegroundColor Yellow

# Build installer project using publish to create single file
Write-Host "Building installer project as single file..." -ForegroundColor Yellow
$installerSrcPath = Join-Path $srcDir "INSTALLER"
Push-Location $installerSrcPath

try {
    dotnet publish -c Release -p:PublishSingleFile=true -p:SelfContained=true -r win-x64
    if ($LASTEXITCODE -ne 0) {
        throw "Installer build failed"
    }
    
    # Find the generated installer exe in publish folder
    $installerBinPath = Join-Path $installerSrcPath "bin\Release\net9.0-windows\win-x64\publish"
    $installerExeName = "DevStack-$version-Installer.exe"
    
    # Look for any exe file in the publish directory
    $exeFiles = Get-ChildItem $installerBinPath -Filter "*.exe" -ErrorAction SilentlyContinue
    if ($exeFiles.Count -eq 0) {
        throw "No installer executable found in: $installerBinPath"
    }
    
    $sourceInstallerPath = $exeFiles[0].FullName
    
    # Copy installer exe to installer directory with correct name
    $targetInstallerPath = Join-Path $installerDir $installerExeName
    Copy-Item $sourceInstallerPath $targetInstallerPath -Force
    Write-Host "Installer built successfully: $targetInstallerPath" -ForegroundColor Green

    # Compacta o installer .exe em um .zip com o mesmo nome
    $zipInstallerPath = $targetInstallerPath.Replace('.exe', '.zip')
    if (Test-Path $zipInstallerPath) {
        Remove-Item $zipInstallerPath -Force
    }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::Open($zipInstallerPath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $targetInstallerPath, [System.IO.Path]::GetFileName($targetInstallerPath)) | Out-Null
    } finally {
        $zip.Dispose()
    }
    Write-Host "Installer compactado em: $zipInstallerPath" -ForegroundColor Green
    # Remove o .exe após zipar para evitar upload acidental
    Remove-Item $targetInstallerPath -Force
    Write-Host "Installer .exe removido após compactação." -ForegroundColor Yellow
} finally {
    Pop-Location
    
    # Clean up - remove the temporary zip file from installer project directory
    if (Test-Path $tempZipForBuild) {
        Remove-Item $tempZipForBuild -Force
        Write-Host "Cleaned up temporary zip file from installer project" -ForegroundColor Yellow
    }
    
    # Also remove the zip from installer directory since it's now embedded
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
        Write-Host "Removed external zip file (now embedded in installer)" -ForegroundColor Yellow
    }
}

# Zip file is now embedded in installer executable - no external files needed
Write-Host "Installation files are embedded in the installer executable" -ForegroundColor Green

Write-Host "=== Build Complete ===" -ForegroundColor Green
Write-Host "Installer location: $installerDir" -ForegroundColor Yellow
Write-Host "Installer executable: DevStack-$version-Installer.exe" -ForegroundColor Yellow

# List contents of installer directory
Write-Host "`nInstaller directory contents:" -ForegroundColor Cyan
Get-ChildItem $installerDir | ForEach-Object {
    Write-Host "  $($_.Name)" -ForegroundColor White
}
