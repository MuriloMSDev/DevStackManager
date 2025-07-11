
# Script para listar todas as versões python-x.y.z-amd64.zip e python-x.y.z-embed-amd64.zip do site oficial e salvar em python.json
$baseUrl = "https://www.python.org/ftp/python/"
$outputPath = "python.json"

$html = Invoke-WebRequest -Uri $baseUrl -UseBasicParsing
$pattern = 'MinGit-(\d+)\.(\d+)\.(\d+)(?:\.([0-9]+))?(?:\.windows)?-64-bit\.zip'
$matches = [regex]::Matches($html.Content, $pattern)
$versions = @()
foreach ($m in $matches) {
    $x = $m.Groups[1].Value
    $y = $m.Groups[2].Value
    $z = $m.Groups[3].Value
    $d = $m.Groups[4].Success ? $m.Groups[4].Value : "0"
    $ver = "$x.$y.$z.$d"
    if ($versions -notcontains $ver) {
        $versions += $ver
    }
}
# Ordenação correta por versão (maior para menor)
$versions = $versions | Sort-Object -Descending -Property {
    $parts = $_ -split '\.'
    return [int]$parts[0]*1000000000 + [int]$parts[1]*1000000 + [int]$parts[2]*1000 + [int]$parts[3]
}
Write-Host "Versões encontradas:" $versions.Count
$jsonArray = @()
foreach ($ver in $versions) {
    $folderUrl = "$baseUrl$ver/"
    $files = @(
        "python-$ver-amd64.zip",
        "python-$ver-embed-amd64.zip"
    )
    $found = $false
    foreach ($file in $files) {
        $fileUrl = $folderUrl + $file
        try {
            $request = [System.Net.WebRequest]::Create($fileUrl)
            $request.Method = "HEAD"
            $response = $request.GetResponse()
            $fileCheck = @{
                StatusCode = [int]$response.StatusCode
            }
            $response.Close()
        } catch {
            Write-Host "Não encontrado: $fileUrl"
            continue
        }
        if ($fileCheck.StatusCode -eq 200) {
            $obj = [PSCustomObject]@{
                version = $ver
                url = $fileUrl
            }
            $jsonArray += $obj
            Write-Host "Encontrado: $fileUrl"
            $found = $true
            break
        }
    }
}
$jsonArray | ConvertTo-Json -Depth 10 | Set-Content $outputPath
Write-Host "Arquivo $outputPath gerado com sucesso."