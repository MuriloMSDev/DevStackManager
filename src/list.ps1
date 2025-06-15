function List-InstalledVersions {
    $data = Get-InstalledVersions
    
    if ($data.status -eq "warning") {
        Write-WarningMsg $data.message
        return
    }
    
    Write-Host "Ferramentas instaladas:"
    
    # Tabela de Ferramentas Instaladas
    $col1 = 15; $col2 = 40
    $header = ('_' * ($col1 + $col2 + 3))
    Write-Host $header -ForegroundColor Gray
    Write-Host ("|{0}|{1}|" -f (Center-Text 'Ferramenta' $col1), (Center-Text 'Versões Instaladas' $col2)) -ForegroundColor Gray
    Write-Host ("|" + ('-' * $col1) + "+" + ('-' * $col2) + "|") -ForegroundColor Gray
    
    foreach ($comp in $data.components) {
        $ferramenta = Center-Text $comp.name $col1
        
        if ($comp.installed -and $comp.versions.Count -gt 0) {
            $status = Center-Text ($comp.versions -join ', ') $col2
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $ferramenta -ForegroundColor Green
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $status -ForegroundColor Green
            Write-Host "|" -ForegroundColor Gray
        } else {
            $status = Center-Text 'NÃO INSTALADO' $col2
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $ferramenta -ForegroundColor Red
            Write-Host -NoNewline "|" -ForegroundColor Gray
            Write-Host -NoNewline $status -ForegroundColor Red
            Write-Host "|" -ForegroundColor Gray
        }
    }
    
    Write-Host ("¯" * ($col1 + $col2 + 3)) -ForegroundColor Gray
}

function Print-HorizontalTable {
    param(
        [hashtable]$Data
    )
    
    if ($Data.status -eq "error") {
        Write-ErrorMsg $Data.message
        return
    }
    
    $Items = $Data.versions
    $Header = $Data.header
    $Installed = $Data.installed
    $OrderDescending = if ($null -eq $Data.orderDescending) { $true } else { $Data.orderDescending }
    $Cols = 6
    
    if (-not $Items -or $Items.Count -eq 0) {
        Write-Host "Nenhuma versão encontrada."
        return
    }
    
    # Ordenar da maior para menor (descendente)
    if ($OrderDescending) {
        $Items = $Items | Sort-Object -Descending
    }
    
    $total = $Items.Count
    $rows = [Math]::Ceiling($total / $Cols)
    $width = 16
    $tableWidth = ($width * $Cols) + $Cols + 1
    Write-Host $Header
    Write-Host ("-" * $tableWidth)
    
    # Preencher linhas de cima para baixo (coluna 1: maior, coluna 2: próxima maior, etc)
    for ($r = 0; $r -lt $rows; $r++) {
        $row = "|"
        for ($c = 0; $c -lt $Cols; $c++) {
            $idx = $c * $rows + $r
            if ($idx -lt $total) {
                $val = $Items[$idx] -replace '[^\d\.]', ''
                if ($Installed -contains $val) {
                    $cell = Center-Text $val $width
                    $row += "`e[32m$cell`e[0m|"  # ANSI verde
                } else {
                    $row += (Center-Text $val $width) + "|"
                }
            } else {
                $row += (Center-Text "" $width) + "|"
            }
        }
        
        # Corrigir cor: Write-Host não interpreta ANSI, então usar Write-Host -ForegroundColor Green para cada célula instalada
        $parts = $row -split '\|'
        Write-Host -NoNewline "|"
        for ($i = 1; $i -le $Cols; $i++) {
            $cellVal = $parts[$i]
            $cellText = $cellVal.Trim()
            if ($cellText -and $Installed -contains $cellText) {
                Write-Host -NoNewline $cellVal -ForegroundColor Green
            } else {
                Write-Host -NoNewline $cellVal -ForegroundColor Gray
            }
            Write-Host -NoNewline "|"
        }
        Write-Host ""
    }
    
    Write-Host ("¯" * $tableWidth)
}

function List-NginxVersions {
    $data = Get-NginxVersions
    Print-HorizontalTable -Data $data
}

function List-PHPVersions {
    $data = Get-PHPVersions
    Print-HorizontalTable -Data $data
}

function List-NodeVersions {
    $data = Get-NodeVersions
    Print-HorizontalTable -Data $data
}

function List-PythonVersions {
    $data = Get-PythonVersions
    Print-HorizontalTable -Data $data
}

function List-MongoDBVersions {
    $data = Get-MongoDBVersions
    Print-HorizontalTable -Data $data
}

function List-RedisVersions {
    $data = Get-RedisVersions
    Print-HorizontalTable -Data $data
}

function List-PgSQLVersions {
    $data = Get-PgSQLVersions
    Print-HorizontalTable -Data $data
}

function List-MailHogVersions {
    $data = Get-MailHogVersions
    Print-HorizontalTable -Data $data
}

function List-ElasticsearchVersions {
    $data = Get-ElasticsearchVersions
    Print-HorizontalTable -Data $data
}

function List-MemcachedVersions {
    $data = Get-MemcachedVersions
    Print-HorizontalTable -Data $data
}

function List-DockerVersions {
    $data = Get-DockerVersions
    Print-HorizontalTable -Data $data
}

function List-YarnVersions {
    $data = Get-YarnVersions
    Print-HorizontalTable -Data $data
}

function List-PnpmVersions {
    $data = Get-PnpmVersions
    Print-HorizontalTable -Data $data
}

function List-WPCLIVersions {
    $data = Get-WPCLIVersions
    Print-HorizontalTable -Data $data
}

function List-AdminerVersions {
    $data = Get-AdminerVersions
    Print-HorizontalTable -Data $data
}

function List-PoetryVersions {
    $data = Get-PoetryVersions
    Print-HorizontalTable -Data $data
}

function List-RubyVersions {
    $data = Get-RubyVersions
    Print-HorizontalTable -Data $data
}

function List-GoVersions {
    $data = Get-GoVersions
    Print-HorizontalTable -Data $data
}

function List-CertbotVersions {
    $data = Get-CertbotVersions
    Print-HorizontalTable -Data $data
}

function List-OpenSSLVersions {
    $data = Get-OpenSSLVersions
    Print-HorizontalTable -Data $data
}

function List-PHPCsFixerVersions {
    $data = Get-PHPCsFixerVersions
    Print-HorizontalTable -Data $data
}

function List-ComposerVersions {
    $data = Get-ComposerVersions
    Print-HorizontalTable -Data $data
}

function List-MySQLVersions {
    $data = Get-MySQLVersions
    Print-HorizontalTable -Data $data
}

function List-PhpMyAdminVersions {
    $data = Get-PhpMyAdminVersions
    Print-HorizontalTable -Data $data
}

function List-GitVersions {
    $data = Get-GitVersions
    Print-HorizontalTable -Data $data
}