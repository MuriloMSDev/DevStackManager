function List-PHPVersions {
    $urls = @(
        "https://windows.php.net/downloads/releases/",
        "https://windows.php.net/downloads/releases/archives/"
    )
    $allMatches = @()
    foreach ($url in $urls) {
        $page = Invoke-WebRequest -Uri $url
        $matches = [regex]::Matches($page.Content, "php-([\d\.]+)-Win32-[^-]+-x64\.zip")
        $allMatches += $matches | ForEach-Object { $_.Groups[1].Value }
    }
    Write-Host $allMatches
    $versions = $allMatches | Sort-Object -Descending | Get-Unique
    Write-Host "Versões de PHP disponíveis para Windows x64:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-NodeVersions {
    $json = Invoke-RestMethod -Uri "https://nodejs.org/dist/index.json"
    $versions = $json | Select-Object -ExpandProperty version
    Write-Host "Versões de Node.js disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

function List-PythonVersions {
    $page = Invoke-WebRequest -Uri "https://www.python.org/downloads/windows/"
    $matches = [regex]::Matches($page.Content, "Python ([\d\.]+) ")
    $versions = $matches | ForEach-Object { $_.Groups[1].Value }
    $versions = $versions | Sort-Object -Descending | Get-Unique
    Write-Host "Versões de Python disponíveis:"
    $versions | ForEach-Object { Write-Host "  $_" }
}

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