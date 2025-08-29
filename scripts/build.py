#!/usr/bin/env python3
"""
DevStackManager Unified Build Script
This script consolidates build, installer, uninstaller and locale operations for performance and efficiency.
Usage: python build.py [--installer]
"""

import os
import shutil
import subprocess
import argparse
import sys
from pathlib import Path
from datetime import datetime
import zipfile

def run_command(cmd, cwd=None, check=True):
    """Run a shell command and return the result."""
    try:
        result = subprocess.run(cmd, shell=True, cwd=cwd, capture_output=True, text=True, check=check)
        return result
    except subprocess.CalledProcessError as e:
        print(f"Command failed: {' '.join(cmd) if isinstance(cmd, list) else cmd}")
        print(f"Error: {e.stderr}")
        raise

def build_projects():
    """Build CLI and GUI projects."""
    print("=== DevStack Build ===")

    # Stop running processes
    for proc in ["DevStackGUI", "DevStack"]:
        try:
            run_command(f"taskkill /f /im {proc}.exe", check=False)
        except:
            pass  # Process might not be running

    # Clean previous builds
    for dir_name in ["bin", "obj"]:
        dir_path = src_dir / dir_name
        if dir_path.exists():
            shutil.rmtree(dir_path)

    # Build CLI & GUI
    cli_project = src_dir / "CLI" / "DevStackCLI.csproj"
    gui_project = src_dir / "GUI" / "DevStackGUI.csproj"

    print("Building CLI...")
    run_command(f"dotnet publish \"{cli_project}\" -c Release -r win-x64 --self-contained true")

    print("Building GUI...")
    run_command(f"dotnet publish \"{gui_project}\" -c Release -r win-x64 --self-contained true")

    # Deploy to release
    release_dir.mkdir(exist_ok=True)

    # Clear release directory
    for item in release_dir.iterdir():
        if item.is_file():
            item.unlink()
        else:
            shutil.rmtree(item)

    # Copy files
    cli_exe = src_dir / "CLI" / "bin" / "Release" / "net9.0-windows" / "win-x64" / "publish" / "DevStack.exe"
    gui_exe = src_dir / "GUI" / "bin" / "Release" / "net9.0-windows" / "win-x64" / "publish" / "DevStackGUI.exe"
    ico_file = src_dir / "Shared" / "DevStack.ico"

    shutil.copy2(cli_exe, release_dir / "DevStack.exe")
    shutil.copy2(gui_exe, release_dir / "DevStackGUI.exe")
    shutil.copy2(ico_file, release_dir / "DevStack.ico")

    # Copy configs if exists
    configs_dir = root_dir / "configs"
    if configs_dir.exists():
        shutil.copytree(configs_dir, release_dir / "configs", dirs_exist_ok=True)

    print("Build and deploy complete.")

def build_uninstaller():
    """Build the uninstaller."""
    print("Building Uninstaller...")

    uninstaller_src_path = src_dir / "UNINSTALLER"
    os.chdir(uninstaller_src_path)

    run_command("dotnet publish -c Release -p:PublishSingleFile=true -p:SelfContained=true -r win-x64")

    uninstaller_bin_path = uninstaller_src_path / "bin" / "Release" / "net9.0-windows" / "win-x64" / "publish"
    exe_files = list(uninstaller_bin_path.glob("*.exe"))

    if not exe_files:
        raise FileNotFoundError("No uninstaller executable found")

    uninstaller_exe = exe_files[0]
    target_path = release_dir / "DevStack-Uninstaller.exe"
    shutil.copy2(uninstaller_exe, target_path)

    os.chdir(root_dir)
    print("Uninstaller built and copied.")

def build_installer():
    """Build the installer."""
    print("Building Installer...")

    # Clean installer dir
    if install_dir.exists():
        shutil.rmtree(install_dir)
    install_dir.mkdir()

    # Check version file
    if not version_file.exists():
        raise FileNotFoundError("VERSION file not found")

    with open(version_file, 'r') as f:
        version = f.read().strip()

    # Check release directory
    if not release_dir.exists():
        raise FileNotFoundError("Release directory not found. Build projects first.")

    release_files = list(release_dir.iterdir())
    if not release_files:
        raise FileNotFoundError("No files in release directory. Build projects first.")

    # Build uninstaller
    build_uninstaller()

    # Create zip with LZMA compression for better compression
    zip_path = install_dir / "DevStack.zip"
    try:
        # Try LZMA first (better compression)
        with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_LZMA) as zipf:
            for file_path in release_dir.rglob('*'):
                if file_path.is_file():
                    arcname = file_path.relative_to(release_dir)
                    zipf.write(file_path, arcname)
        print("Created DevStack.zip with LZMA compression")
    except Exception as e:
        print(f"LZMA failed, falling back to DEFLATE: {e}")
        # Fallback to DEFLATE with max compression
        with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as zipf:
            for file_path in release_dir.rglob('*'):
                if file_path.is_file():
                    arcname = file_path.relative_to(release_dir)
                    zipf.write(file_path, arcname)

    # Build installer
    installer_src_path = src_dir / "INSTALLER"
    os.chdir(installer_src_path)

    run_command("dotnet publish -c Release -p:PublishSingleFile=true -p:SelfContained=true -r win-x64")

    installer_bin_path = installer_src_path / "bin" / "Release" / "net9.0-windows" / "win-x64" / "publish"
    exe_files = list(installer_bin_path.glob("*.exe"))

    if not exe_files:
        raise FileNotFoundError("No installer executable found")

    installer_exe = exe_files[0]
    installer_name = f"DevStack-{version}-Installer.exe"
    target_installer_path = install_dir / installer_name
    shutil.copy2(installer_exe, target_installer_path)

    # Create installer zip with LZMA compression
    zip_installer_path = target_installer_path.with_suffix('.zip')
    try:
        # Try LZMA first (better compression)
        with zipfile.ZipFile(zip_installer_path, 'w', zipfile.ZIP_LZMA) as zipf:
            zipf.write(target_installer_path, target_installer_path.name)
        print("Created installer zip with LZMA compression")
    except Exception as e:
        print(f"LZMA failed, falling back to DEFLATE: {e}")
        # Fallback to DEFLATE with max compression
        with zipfile.ZipFile(zip_installer_path, 'w', zipfile.ZIP_DEFLATED, compresslevel=9) as zipf:
            zipf.write(target_installer_path, target_installer_path.name)

    os.chdir(root_dir)

    # Remove temp zip
    if zip_path.exists():
        zip_path.unlink()

    print("Installer built and compacted.")

def copy_locale_files():
    """Copy locale files to various directories."""
    print("Copying locale files...")

    shared_locale_dir = src_dir / "Shared" / "locale"
    installer_build_dir = src_dir / "INSTALLER" / "bin" / "Release" / "net9.0-windows" / "win-x64" / "publish"
    uninstaller_build_dir = src_dir / "UNINSTALLER" / "bin" / "Release" / "net9.0-windows" / "win-x64" / "publish"

    installer_locale_dir = installer_build_dir / "locale"
    uninstaller_locale_dir = uninstaller_build_dir / "locale"
    installer_install_locale_dir = install_dir / "locale"

    # Create directories
    for dir_path in [installer_locale_dir, uninstaller_locale_dir, installer_install_locale_dir]:
        dir_path.mkdir(parents=True, exist_ok=True)

    # Copy locale files
    locale_files = list(shared_locale_dir.glob("*.json"))
    for locale_file in locale_files:
        shutil.copy2(locale_file, installer_locale_dir / locale_file.name)
        shutil.copy2(locale_file, uninstaller_locale_dir / locale_file.name)
        shutil.copy2(locale_file, installer_install_locale_dir / locale_file.name)

    print("Locale files copied.")

def main():
    """Main execution function."""
    parser = argparse.ArgumentParser(description="DevStackManager Unified Build Script")
    parser.add_argument('--installer', action='store_true', help='Build installer as well')
    args = parser.parse_args()

    start_time = datetime.now()

    try:
        build_projects()
        copy_locale_files()
        if args.installer:
            build_installer()

        end_time = datetime.now()
        elapsed = (end_time - start_time).total_seconds()
        print(f"All operations complete. ({elapsed:.1f}s)")

    except Exception as e:
        print(f"Error: {e}")
        sys.exit(1)

if __name__ == "__main__":
    # Define paths
    script_dir = Path(__file__).parent
    root_dir = script_dir.parent
    src_dir = root_dir / "src"
    release_dir = root_dir / "release"
    install_dir = root_dir / "install"
    version_file = root_dir / "VERSION"

    main()
