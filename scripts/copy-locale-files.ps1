# Copy locale files to installer and uninstaller output directories
# This ensures that the locale files are available for direct loading from disk
# even if the embedded resources don't work properly

$PSScriptRoot = $PWD.Path
$SharedLocaleDir = Join-Path $PSScriptRoot "src\Shared\locale"
$InstallerBuildDir = Join-Path $PSScriptRoot "src\INSTALLER\bin\Release\net9.0-windows\win-x64\publish"
$UninstallerBuildDir = Join-Path $PSScriptRoot "src\UNINSTALLER\bin\Release\net9.0-windows\win-x64\publish"
$InstallerInstallDir = Join-Path $PSScriptRoot "install"

Write-Host "=== Copying locale files to build output folders ==="
Write-Host "Source locale directory: $SharedLocaleDir"

# Create locale directories if they don't exist
$InstallerLocaleDir = Join-Path $InstallerBuildDir "locale"
$UninstallerLocaleDir = Join-Path $UninstallerBuildDir "locale"

if (-not (Test-Path $InstallerLocaleDir)) {
    New-Item -ItemType Directory -Path $InstallerLocaleDir -Force | Out-Null
    Write-Host "Created installer locale directory: $InstallerLocaleDir"
}

if (-not (Test-Path $UninstallerLocaleDir)) {
    New-Item -ItemType Directory -Path $UninstallerLocaleDir -Force | Out-Null
    Write-Host "Created uninstaller locale directory: $UninstallerLocaleDir"
}

# Copy all JSON files from Shared/locale to installer and uninstaller directories
$LocaleFiles = Get-ChildItem -Path $SharedLocaleDir -Filter "*.json"
foreach ($file in $LocaleFiles) {
    Write-Host "Copying locale file: $($file.Name)"
    
    # Copy to installer publish directory
    Copy-Item -Path $file.FullName -Destination (Join-Path $InstallerLocaleDir $file.Name) -Force
    
    # Copy to uninstaller publish directory
    Copy-Item -Path $file.FullName -Destination (Join-Path $UninstallerLocaleDir $file.Name) -Force
}

Write-Host "Locale files copied successfully to build output directories"

# Also copy to installer install directory (where the final installer exe is)
$InstallerLocaleDir = Join-Path $InstallerInstallDir "locale"
if (-not (Test-Path $InstallerLocaleDir)) {
    New-Item -ItemType Directory -Path $InstallerLocaleDir -Force | Out-Null
    Write-Host "Created installer package locale directory: $InstallerLocaleDir"
}

foreach ($file in $LocaleFiles) {
    Copy-Item -Path $file.FullName -Destination (Join-Path $InstallerLocaleDir $file.Name) -Force
}

Write-Host "Locale files copied successfully to installer package directory"
Write-Host "=== Copy operation complete ==="
