$baseUrl = "https://github.com/git-for-windows/git/releases?page="
$results = @()
$page = 1

while ($true) {
    $url = "$baseUrl$page"
    Write-Host "Baixando $url"
    $html = Invoke-WebRequest -Uri $url -UseBasicParsing
    $content = $html.Content

    $pattern = 'MinGit-([\d\.]+)-(64-bit)\.zip'
    $matches = [regex]::Matches($content, $pattern)

    foreach ($match in $matches) {
        $rawVersion = $match.Groups[1].Value
        $arch = $match.Groups[2].Value
        $filename = $match.Value

        $versionParts = $rawVersion -split '\.'
        $version = if ($versionParts.Count -eq 3) {
            "$($versionParts[0]).$($versionParts[1]).$($versionParts[2]).0"
        } else {
            $rawVersion
        }

        $downloadUrl = "https://github.com/git-for-windows/git/releases/download/v$rawVersion.windows.1/$filename"
        if (-not ($results.version -contains $version)) {
            $results += @{
                version = $version
                url = $downloadUrl
            }
        }
    }

    if ($content -notmatch 'href="\/git-for-windows\/git\/releases\?page=' + ($page + 1) + '"') {
        break
    }
    $page++
}

$results | ConvertTo-Json | Set-Content -Path "$PSScriptRoot\git.json"
Write-Host "Arquivo git.json gerado!"