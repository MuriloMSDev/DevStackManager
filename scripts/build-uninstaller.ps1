# DevStack Uninstaller Build Script

$ErrorActionPreference = "Stop"

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$srcDir = Join-Path $rootDir "src"
$releaseDir = Join-Path $rootDir "release"

Write-Host "=== DevStack Uninstaller Build Script ===" -ForegroundColor Green

# Get version from VERSION file
$versionFile = Join-Path $rootDir "VERSION"
if (!(Test-Path $versionFile)) {
    throw "VERSION file not found at: $versionFile"
}

$version = (Get-Content $versionFile).Trim()
Write-Host "Detected version: $version" -ForegroundColor Green

# Build uninstaller project using publish to create single file
Write-Host "Building uninstaller project as single file..." -ForegroundColor Yellow
$uninstallerSrcPath = Join-Path $srcDir "UNINSTALLER"
Push-Location $uninstallerSrcPath

try {
    dotnet publish -c Release -p:PublishSingleFile=true -p:SelfContained=true -r win-x64
    if ($LASTEXITCODE -ne 0) {
        throw "Uninstaller build failed"
    }
    
    # Find the generated uninstaller exe in publish folder
    $uninstallerBinPath = Join-Path $uninstallerSrcPath "bin\Release\net9.0-windows\win-x64\publish"
    $uninstallerExeName = "DevStack-Uninstaller.exe"
    
    # Look for any exe file in the publish directory
    $exeFiles = Get-ChildItem $uninstallerBinPath -Filter "*.exe" -ErrorAction SilentlyContinue
    if ($exeFiles.Count -eq 0) {
        throw "No uninstaller executable found in: $uninstallerBinPath"
    }
    
    $sourceUninstallerPath = $exeFiles[0].FullName
    
    # Copy uninstaller exe to release directory with correct name
    $targetUninstallerPath = Join-Path $releaseDir $uninstallerExeName
    Copy-Item $sourceUninstallerPath $targetUninstallerPath -Force
    
    Write-Host "Uninstaller built successfully: $targetUninstallerPath" -ForegroundColor Green
    
} finally {
    Pop-Location
}

Write-Host "=== Uninstaller Build Complete ===" -ForegroundColor Green
Write-Host "Uninstaller copied to release directory: $releaseDir" -ForegroundColor Yellow
