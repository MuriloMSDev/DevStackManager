# DevStack GUI
# Este arquivo implementa uma interface gráfica para o DevStackSetup
# utilizando Windows Forms e as funções existentes nos outros arquivos

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# Variáveis globais para controle da UI
$script:statusLabel = $null
$script:installedDataGrid = $null
$script:componentComboBox = $null
$script:versionComboBox = $null
$script:uninstallComponentComboBox = $null
$script:uninstallVersionComboBox = $null
$script:uninstallVersionMap = @{} # Mapeamento de versões exibidas para nomes completos
$script:serviceDataGrid = $null
$script:utilsOutputBox = $null # Saída do console na aba Utilitários
$script:utilsExecuteButton = $null # Botão de execução na aba Utilitários

# Função para atualizar a mensagem na barra de status
function Update-StatusMessage {
    param([string]$message)
    if ($script:statusLabel -ne $null) {
        $script:statusLabel.Text = $message
        [System.Windows.Forms.Application]::DoEvents()
    }
}

# Função principal para iniciar a GUI
function Start-DevStackGUI {
    # Criar o formulário principal
    $mainForm = New-Object System.Windows.Forms.Form
    $mainForm.Text = "DevStack Manager"
    $mainForm.Size = New-Object System.Drawing.Size(800, 600)
    $mainForm.StartPosition = "CenterScreen"
    
    # Tentar encontrar um ícone adequado para o formulário
    try {
        # Localizações possíveis para o executável do PowerShell
        $possiblePaths = @(
            "$PSHOME\powershell.exe",                             # PowerShell Core
            "$env:windir\System32\WindowsPowerShell\v1.0\powershell.exe", # Windows PowerShell
            "$env:windir\System32\cmd.exe"                        # CMD como fallback
        )
        
        $iconPath = $null
        foreach ($path in $possiblePaths) {
            if (Test-Path $path) {
                $iconPath = $path
                break
            }
        }
        
        if ($iconPath) {
            $mainForm.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon($iconPath)
        }
    }
    catch {
        # Silenciosamente ignora erros relacionados ao ícone, pois não é essencial
        Write-Verbose "Não foi possível definir o ícone do formulário: $_"
    }
    
    $mainForm.FormBorderStyle = [System.Windows.Forms.FormBorderStyle]::FixedSingle
    $mainForm.MaximizeBox = $false

    # Criar TabControl para organizar as diferentes funcionalidades
    $tabControl = New-Object System.Windows.Forms.TabControl
    $tabControl.Location = New-Object System.Drawing.Point(10, 10)
    $tabControl.Size = New-Object System.Drawing.Size(770, 540)
    $mainForm.Controls.Add($tabControl)
      # Adicionar evento para atualizar conteúdo quando uma tab for selecionada
    $tabControl.Add_SelectedIndexChanged({
        $selectedTab = $tabControl.SelectedTab
        
        switch ($selectedTab.Text) {
            "Ferramentas Instaladas" {
                Update-StatusMessage "Atualizando lista de ferramentas instaladas..."
                Update-InstalledToolsList
            }
            "Desinstalar" {
                Update-StatusMessage "Atualizando componentes disponíveis para desinstalação..."
                Update-UninstallComponentList
            }
 "Serviços" {
                Update-StatusMessage "Atualizando status dos serviços..."
                Update-ServicesList
            }
            "Utilitários" {
                # Garantir que a referência ao OutputBox ainda é válida
                if ($script:utilsOutputBox -eq $null) {
                    # Tentar encontrar o RichTextBox na tab
                    $tabPage = $selectedTab
                    $outputBoxes = $tabPage.Controls | Where-Object { $_ -is [System.Windows.Forms.RichTextBox] }
                    if ($outputBoxes -and $outputBoxes.Count -gt 0) {
                        $script:utilsOutputBox = $outputBoxes[0]
                        Update-StatusMessage "Reinicializando console de utilitários..."
                    }
                }
            }
            Default {
                # Para outras abas, não precisamos fazer nada especial
                Update-StatusMessage "Tab $($selectedTab.Text) selecionada"
            }
        }
    })

    # Tab 1: Ferramentas Instaladas
    $tabInstaladas = New-Object System.Windows.Forms.TabPage
    $tabInstaladas.Text = "Ferramentas Instaladas"
    $tabControl.Controls.Add($tabInstaladas)

    # Tab 2: Instalação
    $tabInstall = New-Object System.Windows.Forms.TabPage
    $tabInstall.Text = "Instalar"
    $tabControl.Controls.Add($tabInstall)

    # Tab 3: Desinstalação
    $tabUninstall = New-Object System.Windows.Forms.TabPage
    $tabUninstall.Text = "Desinstalar"
    $tabControl.Controls.Add($tabUninstall)

    # Tab 4: Serviços
    $tabServices = New-Object System.Windows.Forms.TabPage
    $tabServices.Text = "Serviços"
    $tabControl.Controls.Add($tabServices)

    # Tab 5: Configurações
    $tabConfig = New-Object System.Windows.Forms.TabPage
    $tabConfig.Text = "Configurações"
    $tabControl.Controls.Add($tabConfig)

    # Tab 6: Sites
    $tabSites = New-Object System.Windows.Forms.TabPage
    $tabSites.Text = "Sites"
    $tabControl.Controls.Add($tabSites)

    # Tab 7: Utilitários
    $tabUtils = New-Object System.Windows.Forms.TabPage
    $tabUtils.Text = "Utilitários"
    $tabControl.Controls.Add($tabUtils)

    # Configurar a Tab de Ferramentas Instaladas
    Setup-InstalledToolsTab $tabInstaladas

    # Configurar a Tab de Instalação
    Setup-InstallTab $tabInstall

    # Configurar a Tab de Desinstalação
    Setup-UninstallTab $tabUninstall

    # Configurar a Tab de Serviços
    Setup-ServicesTab $tabServices

    # Configurar a Tab de Configurações
    Setup-ConfigTab $tabConfig

    # Configurar a Tab de Sites
    Setup-SitesTab $tabSites    # Configurar a Tab de Utilitários
    Setup-UtilsTab $tabUtils
    
    # Mostrar o formulário
    $mainForm.Add_Shown({
        $mainForm.Activate()
        # Atualizar a lista de ferramentas instaladas ao iniciar
        Update-InstalledToolsList
    })
    
    # Configurar barra de status
    $statusStrip = New-Object System.Windows.Forms.StatusStrip
    $statusLabel = New-Object System.Windows.Forms.ToolStripStatusLabel
    $statusLabel.Text = "Pronto"
    $statusStrip.Items.Add($statusLabel)
    $mainForm.Controls.Add($statusStrip)
    
    # Tornar a barra de status acessível globalmente
    $script:statusLabel = $statusLabel

    [void]$mainForm.ShowDialog()
}

# Esta seção foi movida para o início do arquivo

# Configuração da Tab de Ferramentas Instaladas
function Setup-InstalledToolsTab {
    param($tabPage)

    # Criar DataGridView para listar ferramentas instaladas
    $dataGrid = New-Object System.Windows.Forms.DataGridView
    $dataGrid.Location = New-Object System.Drawing.Point(10, 10)
    $dataGrid.Size = New-Object System.Drawing.Size(740, 450)
    $dataGrid.AutoSizeColumnsMode = [System.Windows.Forms.DataGridViewAutoSizeColumnsMode]::Fill
    $dataGrid.AllowUserToAddRows = $false
    $dataGrid.AllowUserToDeleteRows = $false
    $dataGrid.ReadOnly = $true
    $dataGrid.RowHeadersVisible = $false
    $dataGrid.SelectionMode = [System.Windows.Forms.DataGridViewSelectionMode]::FullRowSelect
    
    # Adicionar colunas
    $dataGrid.ColumnCount = 2
    $dataGrid.Columns[0].Name = "Ferramenta"
    $dataGrid.Columns[1].Name = "Versões Instaladas"
    
    $tabPage.Controls.Add($dataGrid)
    
    # Botão de atualizar
    $refreshButton = New-Object System.Windows.Forms.Button
    $refreshButton.Location = New-Object System.Drawing.Point(10, 470)
    $refreshButton.Size = New-Object System.Drawing.Size(100, 30)
    $refreshButton.Text = "Atualizar"
    $refreshButton.Add_Click({
        Update-StatusMessage "Atualizando lista de ferramentas instaladas..."
        Update-InstalledToolsList
        Update-StatusMessage "Lista atualizada."
    })
    $tabPage.Controls.Add($refreshButton)
    
    # Armazenar o DataGridView na variável global
    $script:installedDataGrid = $dataGrid
}

# Função para atualizar a lista de ferramentas instaladas
function Update-InstalledToolsList {
    $script:installedDataGrid.Rows.Clear()
    
    # Obter dados usando a função existente
    $data = Get-InstalledVersions
    
    if ($data.status -eq "warning") {
        [System.Windows.Forms.MessageBox]::Show($data.message, "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
        return
    }
    
    foreach ($comp in $data.components) {
        $ferramenta = $comp.name
        
        if ($comp.installed -and $comp.versions.Count -gt 0) {
            $versoes = $comp.versions -join ', '
        } else {
            $versoes = "NÃO INSTALADO"
        }
        
        [void]$script:installedDataGrid.Rows.Add($ferramenta, $versoes)
        
        # Colorir a linha de acordo com o status (verde se instalado, vermelho se não)
        $rowIndex = $script:installedDataGrid.Rows.Count - 1
        if ($comp.installed) {
            $script:installedDataGrid.Rows[$rowIndex].DefaultCellStyle.ForeColor = [System.Drawing.Color]::Green
        } else {
            $script:installedDataGrid.Rows[$rowIndex].DefaultCellStyle.ForeColor = [System.Drawing.Color]::Red
        }
    }
}

# Configuração da Tab de Instalação
function Setup-InstallTab {
    param($tabPage)
    
    # Criar controles para seleção de componente e versão
    $componentLabel = New-Object System.Windows.Forms.Label
    $componentLabel.Location = New-Object System.Drawing.Point(10, 20)
    $componentLabel.Size = New-Object System.Drawing.Size(100, 23)
    $componentLabel.Text = "Componente:"
    $tabPage.Controls.Add($componentLabel)
    
    $componentCombo = New-Object System.Windows.Forms.ComboBox
    $componentCombo.Location = New-Object System.Drawing.Point(120, 20)
    $componentCombo.Size = New-Object System.Drawing.Size(200, 23)
    $componentCombo.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    
    # Adicionar itens ao ComboBox de componentes
    $components = @(
        "php", "nginx", "mysql", "nodejs", "python", "composer", "git", "phpmyadmin",
        "mongodb", "redis", "pgsql", "mailhog", "elasticsearch", "memcached",
        "docker", "yarn", "pnpm", "wpcli", "adminer", "poetry", "ruby", "go",
        "certbot", "openssl", "php-cs-fixer"
    )
    
    foreach ($comp in $components) {
        [void]$componentCombo.Items.Add($comp)
    }
      $componentCombo.Add_SelectedIndexChanged({
        Update-StatusMessage "Carregando versões disponíveis para $($script:componentComboBox.SelectedItem)..."
        Update-VersionList
    })
    
    $tabPage.Controls.Add($componentCombo)
    
    $versionLabel = New-Object System.Windows.Forms.Label
    $versionLabel.Location = New-Object System.Drawing.Point(10, 60)
    $versionLabel.Size = New-Object System.Drawing.Size(100, 23)
    $versionLabel.Text = "Versão:"
    $tabPage.Controls.Add($versionLabel)
    
    $versionCombo = New-Object System.Windows.Forms.ComboBox
    $versionCombo.Location = New-Object System.Drawing.Point(120, 60)
    $versionCombo.Size = New-Object System.Drawing.Size(200, 23)
    $versionCombo.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    $tabPage.Controls.Add($versionCombo)
    
    # Armazenar referência em variável global antes de usá-la
    $script:uninstallVersionComboBox = $versionCombo
    
    # Botão de instalação
    $installButton = New-Object System.Windows.Forms.Button
    $installButton.Location = New-Object System.Drawing.Point(120, 100)
    $installButton.Size = New-Object System.Drawing.Size(100, 30)
    $installButton.Text = "Instalar"
    $installButton.Add_Click({
        $component = $script:componentComboBox.SelectedItem
        $version = $script:versionComboBox.SelectedItem
        
        if (-not $component) {
            [System.Windows.Forms.MessageBox]::Show("Selecione um componente.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
        
        if (-not $version) {
            [System.Windows.Forms.MessageBox]::Show("Selecione uma versão.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
          $result = [System.Windows.Forms.MessageBox]::Show("Deseja instalar $component versão ${version}?", "Confirmação", [System.Windows.Forms.MessageBoxButtons]::YesNo, [System.Windows.Forms.MessageBoxIcon]::Question)
        
        if ($result -eq [System.Windows.Forms.DialogResult]::Yes) {
            Update-StatusMessage "Instalando $component versão ${version}..."
              # Usar a função existente no install.ps1
            try {
                # Formato correto para instalação: componente-versão
                & "$PSScriptRoot\..\setup.ps1" install $component $version
                Update-StatusMessage "$component versão $version instalado com sucesso."
                [System.Windows.Forms.MessageBox]::Show("$component versão $version foi instalado com sucesso.", "Sucesso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
                
                # Atualizar todas as abas com os novos dados
                Update-AllTabs
            }
            catch {
                Update-StatusMessage "Erro na instalação: $_"
                    [System.Windows.Forms.MessageBox]::Show("Erro ao instalar $component versão ${version}: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
            }
        }
    })
    
    $tabPage.Controls.Add($installButton)
    
    # Tela de console para mostrar o progresso
    $outputBox = New-Object System.Windows.Forms.RichTextBox
    $outputBox.Location = New-Object System.Drawing.Point(10, 150)
    $outputBox.Size = New-Object System.Drawing.Size(740, 350)
    $outputBox.ReadOnly = $true
    $outputBox.BackColor = [System.Drawing.Color]::Black
    $outputBox.ForeColor = [System.Drawing.Color]::White
    $outputBox.Font = New-Object System.Drawing.Font("Consolas", 10)
    $tabPage.Controls.Add($outputBox)
    
    # Armazenar referências em variáveis globais
    $script:componentComboBox = $componentCombo
    $script:versionComboBox = $versionCombo
    $script:installOutputBox = $outputBox
}

# Função para atualizar a lista de versões disponíveis
function Update-VersionList {
    $component = $script:componentComboBox.SelectedItem
    
    if (-not $component) {
        return
    }
    
    $script:versionComboBox.Items.Clear()
    
    # Obter versões disponíveis usando as funções existentes
    switch ($component) {
        "php" { $data = Get-PHPVersions }
        "nginx" { $data = Get-NginxVersions }
        "mysql" { $data = Get-MySQLVersions }
        "nodejs" { $data = Get-NodeVersions }
        "python" { $data = Get-PythonVersions }
        "composer" { $data = Get-ComposerVersions }
        "git" { $data = Get-GitVersions }
        "phpmyadmin" { $data = Get-PhpMyAdminVersions }
        "mongodb" { $data = Get-MongoDBVersions }
        "redis" { $data = Get-RedisVersions }
        "pgsql" { $data = Get-PgSQLVersions }
        "mailhog" { $data = Get-MailHogVersions }
        "elasticsearch" { $data = Get-ElasticsearchVersions }
        "memcached" { $data = Get-MemcachedVersions }
        "docker" { $data = Get-DockerVersions }
        "yarn" { $data = Get-YarnVersions }
        "pnpm" { $data = Get-PnpmVersions }
        "wpcli" { $data = Get-WPCLIVersions }
        "adminer" { $data = Get-AdminerVersions }
        "poetry" { $data = Get-PoetryVersions }
        "ruby" { $data = Get-RubyVersions }
        "go" { $data = Get-GoVersions }
        "certbot" { $data = Get-CertbotVersions }
        "openssl" { $data = Get-OpenSSLVersions }
        "php-cs-fixer" { $data = Get-PHPCsFixerVersions }
    }
    
    if ($data.status -eq "error") {
        [System.Windows.Forms.MessageBox]::Show($data.message, "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        return
    }
    
    foreach ($version in $data.versions) {
        [void]$script:versionComboBox.Items.Add($version)
    }
    
    if ($script:versionComboBox.Items.Count -gt 0) {
        $script:versionComboBox.SelectedIndex = 0
    }
    
    Update-StatusMessage "Versões carregadas para $component"
}

# Configuração da Tab de Desinstalação
function Setup-UninstallTab {
    param($tabPage)
    
    # Criar controles para seleção de componente e versão
    $componentLabel = New-Object System.Windows.Forms.Label
    $componentLabel.Location = New-Object System.Drawing.Point(10, 20)
    $componentLabel.Size = New-Object System.Drawing.Size(100, 23)
    $componentLabel.Text = "Componente:"
    $tabPage.Controls.Add($componentLabel)
    $componentCombo = New-Object System.Windows.Forms.ComboBox
    $componentCombo.Location = New-Object System.Drawing.Point(120, 20)
    $componentCombo.Size = New-Object System.Drawing.Size(200, 23)
    $componentCombo.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    
    # Armazenar referência em variável global antes de usá-la
    $script:uninstallComponentComboBox = $componentCombo
    
    $componentCombo.Add_SelectedIndexChanged({
        Update-StatusMessage "Carregando versões instaladas de $($script:uninstallComponentComboBox.SelectedItem)..."
        Update-UninstallVersionList
    })
    
    $tabPage.Controls.Add($componentCombo)
    
    $versionLabel = New-Object System.Windows.Forms.Label
    $versionLabel.Location = New-Object System.Drawing.Point(10, 60)
    $versionLabel.Size = New-Object System.Drawing.Size(100, 23)
    $versionLabel.Text = "Versão:"
    $tabPage.Controls.Add($versionLabel)
    
    $versionCombo = New-Object System.Windows.Forms.ComboBox
    $versionCombo.Location = New-Object System.Drawing.Point(120, 60)
    $versionCombo.Size = New-Object System.Drawing.Size(200, 23)
    $versionCombo.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    $tabPage.Controls.Add($versionCombo)
    
    # Armazenar referência em variável global antes de usá-la
    $script:uninstallVersionComboBox = $versionCombo
    
    # Botão de desinstalação
    $uninstallButton = New-Object System.Windows.Forms.Button
    $uninstallButton.Location = New-Object System.Drawing.Point(120, 100)
    $uninstallButton.Size = New-Object System.Drawing.Size(100, 30)
    $uninstallButton.Text = "Desinstalar"
    $uninstallButton.Add_Click({
        $component = $script:uninstallComponentComboBox.SelectedItem
        $selectedIndex = $script:uninstallVersionComboBox.SelectedIndex
        
        if (-not $component) {
            [System.Windows.Forms.MessageBox]::Show("Selecione um componente.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
        
        if ($selectedIndex -lt 0) {
            [System.Windows.Forms.MessageBox]::Show("Selecione uma versão.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
          # Obtemos a versão formatada para exibição
        $versionDisplay = $script:uninstallVersionComboBox.SelectedItem
                
        $result = [System.Windows.Forms.MessageBox]::Show("Tem certeza que deseja desinstalar $component versão ${versionDisplay}?", "Confirmação", [System.Windows.Forms.MessageBoxButtons]::YesNo, [System.Windows.Forms.MessageBoxIcon]::Warning)
        
        if ($result -eq [System.Windows.Forms.DialogResult]::Yes) {
            Update-StatusMessage "Desinstalando $component versão ${versionDisplay}..."
              # Usar a função existente no uninstall.ps1
            try {                # Formato correto para desinstalação: componente-versão
                & "$PSScriptRoot\..\setup.ps1" uninstall "$component-$versionDisplay"
                Update-StatusMessage "$component versão ${versionDisplay} desinstalado com sucesso."
                [System.Windows.Forms.MessageBox]::Show("$component versão $versionDisplay foi desinstalado com sucesso.", "Sucesso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
                
                # Atualizar todas as abas com os novos dados
                Update-AllTabs
            }
            catch {
                Update-StatusMessage "Erro na desinstalação: $_"
                [System.Windows.Forms.MessageBox]::Show("Erro ao desinstalar $component versão ${version}: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
            }
        }
    })
    $tabPage.Controls.Add($uninstallButton)
    
    # Populamos o ComboBox somente com componentes instalados após toda a inicialização
    Update-UninstallComponentList
}

# Função para atualizar a lista de versões instaladas para desinstalar
function Update-UninstallVersionList {
    $component = $script:uninstallComponentComboBox.SelectedItem
    
    if (-not $component) {
        return
    }
    
    $script:uninstallVersionComboBox.Items.Clear()
    
    # Obter status do componente para listar versões instaladas
    $status = Get-ComponentStatus -Component $component
    
    if (-not $status.installed) {
        [System.Windows.Forms.MessageBox]::Show("$component não está instalado.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
        return
    }
    
    # Criar uma tabela hash para mapear versões exibidas para versões completas
    $script:uninstallVersionMap = @{}
    
    foreach ($fullVersion in $status.versions) {
        # Extrai apenas a parte da versão, removendo o nome do componente
        if ($component -eq "git" -and $fullVersion -like "git-*") {
            # Para git, formato é git-x.x.x
            $versionNumber = $fullVersion -replace '^git-', ''
        } elseif ($fullVersion -like "$component-*") {
            # Para outros componentes que seguem o padrão componente-x.x.x
            $versionNumber = $fullVersion -replace "^$component-", ''
        } elseif ($fullVersion -match '^\d') {
            # Se o nome já começa com dígito, pode ser apenas a versão
            $versionNumber = $fullVersion
        } else {
            # Para outros formatos, use o nome original
            $versionNumber = $fullVersion
        }
        
        # Adiciona versão formatada ao ComboBox
        [void]$script:uninstallVersionComboBox.Items.Add($versionNumber)
        
        # Armazena o mapeamento na tabela hash
        $script:uninstallVersionMap[$versionNumber] = $fullVersion
    }
    
    if ($script:uninstallVersionComboBox.Items.Count -gt 0) {
        $script:uninstallVersionComboBox.SelectedIndex = 0
    }
    
    Update-StatusMessage "Versões instaladas carregadas para $component"
}

# Função para atualizar a lista de componentes instalados para desinstalar
function Update-UninstallComponentList {
    $script:uninstallComponentComboBox.Items.Clear()
    
    # Obter status de todos os componentes
    $allComponents = Get-AllComponentsStatus
    
    # Filtrar apenas componentes instalados
    foreach ($comp in $allComponents.Keys) {
        if ($allComponents[$comp].installed) {
            [void]$script:uninstallComponentComboBox.Items.Add($comp)
        }
    }
    
    if ($script:uninstallComponentComboBox.Items.Count -gt 0) {
        $script:uninstallComponentComboBox.SelectedIndex = 0
        # Atualiza a lista de versões instaladas para o componente selecionado
        Update-UninstallVersionList
    } else {
        Update-StatusMessage "Nenhum componente instalado encontrado para desinstalação."
    }
}

# Configuração da Tab de Serviços
function Setup-ServicesTab {
    param($tabPage)
    
    # Criar controles para serviços
    $serviceLabel = New-Object System.Windows.Forms.Label
    $serviceLabel.Location = New-Object System.Drawing.Point(10, 10)
    $serviceLabel.Size = New-Object System.Drawing.Size(740, 23)
    $serviceLabel.Text = "Serviços (PHP-FPM, Nginx, MySQL)"
    $tabPage.Controls.Add($serviceLabel)
    
    # DataGridView para listar serviços
    $dataGrid = New-Object System.Windows.Forms.DataGridView
    $dataGrid.Location = New-Object System.Drawing.Point(10, 40)
    $dataGrid.Size = New-Object System.Drawing.Size(740, 300)
    $dataGrid.AutoSizeColumnsMode = [System.Windows.Forms.DataGridViewAutoSizeColumnsMode]::Fill
    $dataGrid.AllowUserToAddRows = $false
    $dataGrid.AllowUserToDeleteRows = $false
    $dataGrid.ReadOnly = $true
    $dataGrid.RowHeadersVisible = $false
    $dataGrid.SelectionMode = [System.Windows.Forms.DataGridViewSelectionMode]::FullRowSelect
    
    # Adicionar colunas
    $dataGrid.ColumnCount = 4
    $dataGrid.Columns[0].Name = "Componente"
    $dataGrid.Columns[1].Name = "Versão"
    $dataGrid.Columns[2].Name = "Status"
    $dataGrid.Columns[3].Name = "PID"
    
    $tabPage.Controls.Add($dataGrid)
    
    # Botões para controlar serviços
    $startButton = New-Object System.Windows.Forms.Button
    $startButton.Location = New-Object System.Drawing.Point(10, 350)
    $startButton.Size = New-Object System.Drawing.Size(100, 30)
    $startButton.Text = "Iniciar"
    $startButton.Add_Click({
        $selectedRows = $script:serviceDataGrid.SelectedRows
        
        if ($selectedRows.Count -eq 0) {
            [System.Windows.Forms.MessageBox]::Show("Selecione um serviço para iniciar.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
        
        $component = $selectedRows[0].Cells[0].Value
        $version = $selectedRows[0].Cells[1].Value
        
        Update-StatusMessage "Iniciando $component versão $version..."
        
        try {
            & "$PSScriptRoot\..\setup.ps1" start $component $version
            Update-StatusMessage "$component versão $version iniciado com sucesso."
            Update-ServicesList
        }
        catch {
            Update-StatusMessage "Erro ao iniciar serviço: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao iniciar $component versão ${version}: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($startButton)
    
    $stopButton = New-Object System.Windows.Forms.Button
    $stopButton.Location = New-Object System.Drawing.Point(120, 350)
    $stopButton.Size = New-Object System.Drawing.Size(100, 30)
    $stopButton.Text = "Parar"
    $stopButton.Add_Click({
        $selectedRows = $script:serviceDataGrid.SelectedRows
        
        if ($selectedRows.Count -eq 0) {
            [System.Windows.Forms.MessageBox]::Show("Selecione um serviço para parar.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
        
        $component = $selectedRows[0].Cells[0].Value
        $version = $selectedRows[0].Cells[1].Value
        
        Update-StatusMessage "Parando $component versão $version..."
        
        try {
            & "$PSScriptRoot\..\setup.ps1" stop $component $version
            Update-StatusMessage "$component versão $version parado com sucesso."
            Update-ServicesList
        }
        catch {
            Update-StatusMessage "Erro ao parar serviço: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao parar $component versão ${version}: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($stopButton)
    
    $restartButton = New-Object System.Windows.Forms.Button
    $restartButton.Location = New-Object System.Drawing.Point(230, 350)
    $restartButton.Size = New-Object System.Drawing.Size(100, 30)
    $restartButton.Text = "Reiniciar"
    $restartButton.Add_Click({
        $selectedRows = $script:serviceDataGrid.SelectedRows
        
        if ($selectedRows.Count -eq 0) {
            [System.Windows.Forms.MessageBox]::Show("Selecione um serviço para reiniciar.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
        
        $component = $selectedRows[0].Cells[0].Value
        $version = $selectedRows[0].Cells[1].Value
        
        Update-StatusMessage "Reiniciando $component versão $version..."
        
        try {
            & "$PSScriptRoot\..\setup.ps1" restart $component $version
            Update-StatusMessage "$component versão $version reiniciado com sucesso."
            Update-ServicesList
        }
        catch {
            Update-StatusMessage "Erro ao reiniciar serviço: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao reiniciar $component versão ${version}: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($restartButton)
    
    $refreshButton = New-Object System.Windows.Forms.Button
    $refreshButton.Location = New-Object System.Drawing.Point(340, 350)
    $refreshButton.Size = New-Object System.Drawing.Size(100, 30)
    $refreshButton.Text = "Atualizar"
    $refreshButton.Add_Click({
        Update-StatusMessage "Atualizando lista de serviços..."
        Update-ServicesList
        Update-StatusMessage "Lista de serviços atualizada."
    })
    
  $tabPage.Controls.Add($refreshButton)
    
    # Armazenar referência ao DataGridView
    $script:serviceDataGrid = $dataGrid
    
    # Inicializar a lista de serviços
    Update-ServicesList
}

# Função para atualizar a lista de serviços
function Update-ServicesList {
    $script:serviceDataGrid.Rows.Clear()
      # Verificar serviços em execução do PHP-FPM
    if (Test-Path $phpDir) { 
        $phpDirs = Get-ChildItem $phpDir -Directory
        
        foreach ($dir in $phpDirs) {
            # Extrair somente o número da versão (remover o prefixo "php-")
            $versionNumber = $dir.Name -replace '^php-', ''
            $phpProcesses = Get-Process -Name "php-*" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*$($dir.Name)*" }
            
            if ($phpProcesses -and $phpProcesses.Count -gt 0) {
                [void]$script:serviceDataGrid.Rows.Add("php", $versionNumber, "Em execução", $phpProcesses[0].Id)
                $rowIndex = $script:serviceDataGrid.Rows.Count - 1
                $script:serviceDataGrid.Rows[$rowIndex].DefaultCellStyle.ForeColor = [System.Drawing.Color]::Green
            } else {
                [void]$script:serviceDataGrid.Rows.Add("php", $versionNumber, "Parado", "-")
                $rowIndex = $script:serviceDataGrid.Rows.Count - 1
                $script:serviceDataGrid.Rows[$rowIndex].DefaultCellStyle.ForeColor = [System.Drawing.Color]::Red
            }
        }
    }
      # Verificar serviços em execução do Nginx
    if (Test-Path $nginxDir) {
        $nginxDirs = Get-ChildItem $nginxDir -Directory
        
        foreach ($dir in $nginxDirs) {
 
            # Extrair somente o número da versão (remover o prefixo "nginx-")
            $versionNumber = $dir.Name -replace '^nginx-', ''
            $nginxProcesses = Get-Process -Name "nginx*" -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "*$($dir.Name)*" }
            
            if ($nginxProcesses -and $nginxProcesses.Count -gt 0) {
                [void]$script:serviceDataGrid.Rows.Add("nginx", $versionNumber, "Em execução", $nginxProcesses[0].Id)                
                $rowIndex = $script:serviceDataGrid.Rows.Count - 1
                $script:serviceDataGrid.Rows[$rowIndex].DefaultCellStyle.ForeColor = [System.Drawing.Color]::Green
            } else {
                [void]$script:serviceDataGrid.Rows.Add("nginx", $versionNumber, "Parado", "-")
                $rowIndex = $script:serviceDataGrid.Rows.Count - 1
                $script:serviceDataGrid.Rows[$rowIndex].DefaultCellStyle.ForeColor = [System.Drawing.Color]::Red
            }
        }
    }
}

# Configuração da Tab de Configurações
function Setup-ConfigTab {
    param($tabPage)
    
    # Criar controles para configurações
    $pathLabel = New-Object System.Windows.Forms.Label
    $pathLabel.Location = New-Object System.Drawing.Point(10, 20)
    $pathLabel.Size = New-Object System.Drawing.Size(740, 23)
    $pathLabel.Text = "Gerenciamento de PATH"
    $tabPage.Controls.Add($pathLabel)
    
    # Botão para adicionar ao PATH
    $addPathButton = New-Object System.Windows.Forms.Button
    $addPathButton.Location = New-Object System.Drawing.Point(10, 50)
    $addPathButton.Size = New-Object System.Drawing.Size(200, 30)
    $addPathButton.Text = "Adicionar DevStack ao PATH"
    $addPathButton.Add_Click({
        Update-StatusMessage "Adicionando DevStack ao PATH do sistema..."
        
        try {
            Add-BinDirsToPath
            Update-StatusMessage "DevStack adicionado ao PATH com sucesso."
            [System.Windows.Forms.MessageBox]::Show("DevStack adicionado ao PATH do sistema com sucesso.", "Sucesso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
        }
        catch {
            Update-StatusMessage "Erro ao adicionar ao PATH: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao adicionar DevStack ao PATH: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($addPathButton)
    
    # Botão para configuração global
    $globalButton = New-Object System.Windows.Forms.Button
    $globalButton.Location = New-Object System.Drawing.Point(220, 50)
    $globalButton.Size = New-Object System.Drawing.Size(200, 30)
    $globalButton.Text = "Configuração Global"
    $globalButton.Add_Click({
        Update-StatusMessage "Aplicando configuração global..."
        
        try {
            & "$PSScriptRoot\..\setup.ps1" global
            Update-StatusMessage "Configuração global aplicada com sucesso."
        }
        catch {
            Update-StatusMessage "Erro ao aplicar configuração global: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao aplicar configuração global: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($globalButton)
    
    # Botão para executar diagnóstico
    $doctorButton = New-Object System.Windows.Forms.Button
    $doctorButton.Location = New-Object System.Drawing.Point(10, 100)
    $doctorButton.Size = New-Object System.Drawing.Size(200, 30)
    $doctorButton.Text = "Diagnóstico do Sistema"
    $doctorButton.Add_Click({
        Update-StatusMessage "Executando diagnóstico do sistema..."
        
        try {
            $diagForm = New-Object System.Windows.Forms.Form
            $diagForm.Text = "DevStack - Diagnóstico do Sistema"
            $diagForm.Size = New-Object System.Drawing.Size(800, 600)
            $diagForm.StartPosition = "CenterScreen"
            
            $outputBox = New-Object System.Windows.Forms.RichTextBox
            $outputBox.Location = New-Object System.Drawing.Point(10, 10)
            $outputBox.Size = New-Object System.Drawing.Size(760, 540)
            $outputBox.ReadOnly = $true
            $outputBox.BackColor = [System.Drawing.Color]::Black
            $outputBox.ForeColor = [System.Drawing.Color]::White
            $outputBox.Font = New-Object System.Drawing.Font("Consolas", 10)
            
            $diagForm.Controls.Add($outputBox)
            
            # Executar diagnóstico em segundo plano
            $job = Start-Job -ScriptBlock {
                param($PSScriptRoot)
                & "$PSScriptRoot\..\setup.ps1" doctor
            } -ArgumentList $PSScriptRoot
            
            $timer = New-Object System.Windows.Forms.Timer
            $timer.Interval = 500
            $timer.Add_Tick({
                if ($job.State -eq "Completed") {
                    $output = Receive-Job -Job $job
                    $outputBox.Text = $output -join [Environment]::NewLine
                    $timer.Stop()
                    $timer.Dispose()
                    Remove-Job -Job $job -Force
                    Update-StatusMessage "Diagnóstico concluído."
                }
            })
            
            $timer.Start()
            [void]$diagForm.ShowDialog()
        }
        catch {
            Update-StatusMessage "Erro ao executar diagnóstico: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao executar diagnóstico: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($doctorButton)
    
    # Botão para verificar dependências
    $depsButton = New-Object System.Windows.Forms.Button
    $depsButton.Location = New-Object System.Drawing.Point(220, 100)
    $depsButton.Size = New-Object System.Drawing.Size(200, 30)
    $depsButton.Text = "Verificar Dependências"
    $depsButton.Add_Click({
        Update-StatusMessage "Verificando dependências do sistema..."
        
        try {
            & "$PSScriptRoot\..\setup.ps1" deps
            Update-StatusMessage "Verificação de dependências concluída."
        }
        catch {
            Update-StatusMessage "Erro ao verificar dependências: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao verificar dependências: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($depsButton)
    
    # Botão para Limpar Logs/Temp
    $cleanButton = New-Object System.Windows.Forms.Button
    $cleanButton.Location = New-Object System.Drawing.Point(10, 150)
    $cleanButton.Size = New-Object System.Drawing.Size(200, 30)
    $cleanButton.Text = "Limpar Logs/Temp"
    $cleanButton.Add_Click({
        Update-StatusMessage "Limpando arquivos temporários e logs..."
        
        $result = [System.Windows.Forms.MessageBox]::Show(
            "Esta operação irá remover todos os logs e arquivos temporários do DevStack. Deseja continuar?",
            "Confirmação",
            [System.Windows.Forms.MessageBoxButtons]::YesNo,
            [System.Windows.Forms.MessageBoxIcon]::Warning
        )
        
        if ($result -eq [System.Windows.Forms.DialogResult]::Yes) {
            try {
                & "$PSScriptRoot\..\setup.ps1" clean
                Update-StatusMessage "Limpeza concluída com sucesso."
                [System.Windows.Forms.MessageBox]::Show("Limpeza concluída com sucesso.", "Sucesso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
            }
            catch {
                Update-StatusMessage "Erro ao limpar arquivos: $_"
                [System.Windows.Forms.MessageBox]::Show("Erro ao limpar arquivos: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
            }
        }
    })
    
    $tabPage.Controls.Add($cleanButton)
    
    # Botão para Backup
    $backupButton = New-Object System.Windows.Forms.Button
    $backupButton.Location = New-Object System.Drawing.Point(220, 150)
    $backupButton.Size = New-Object System.Drawing.Size(200, 30)
    $backupButton.Text = "Criar Backup"
    $backupButton.Add_Click({
        Update-StatusMessage "Criando backup das configurações..."
        
        try {
            & "$PSScriptRoot\..\setup.ps1" backup
            Update-StatusMessage "Backup criado com sucesso."
            [System.Windows.Forms.MessageBox]::Show("Backup das configurações criado com sucesso.", "Sucesso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
        }
        catch {
            Update-StatusMessage "Erro ao criar backup: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao criar backup: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($backupButton)
    
    # Botão para Atualizar DevStack
    $updateButton = New-Object System.Windows.Forms.Button
    $updateButton.Location = New-Object System.Drawing.Point(10, 200)
    $updateButton.Size = New-Object System.Drawing.Size(200, 30)
    $updateButton.Text = "Atualizar DevStack"
    $updateButton.Add_Click({
        Update-StatusMessage "Atualizando DevStack..."
        
        try {
            & "$PSScriptRoot\..\setup.ps1" self-update
            Update-StatusMessage "DevStack atualizado com sucesso."
            [System.Windows.Forms.MessageBox]::Show("DevStack atualizado com sucesso. Reinicie a aplicação para aplicar as alterações.", "Sucesso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
        }
        catch {
            Update-StatusMessage "Erro ao atualizar DevStack: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao atualizar DevStack: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($updateButton)
}

# Configuração da Tab de Sites
function Setup-SitesTab {
    param($tabPage)
    
    # Criar controles para gerenciamento de sites Nginx
    $siteLabel = New-Object System.Windows.Forms.Label
    $siteLabel.Location = New-Object System.Drawing.Point(10, 20)
    $siteLabel.Size = New-Object System.Drawing.Size(200, 23)
    $siteLabel.Text = "Criar configuração de site"
    $tabPage.Controls.Add($siteLabel)
    
    # Campo de domínio
    $domainLabel = New-Object System.Windows.Forms.Label
    $domainLabel.Location = New-Object System.Drawing.Point(10, 50)
    $domainLabel.Size = New-Object System.Drawing.Size(100, 23)
    $domainLabel.Text = "Domínio:"
    $tabPage.Controls.Add($domainLabel)
    
    $domainTextBox = New-Object System.Windows.Forms.TextBox
    $domainTextBox.Location = New-Object System.Drawing.Point(120, 50)
    $domainTextBox.Size = New-Object System.Drawing.Size(200, 23)
    $tabPage.Controls.Add($domainTextBox)
    
    # Campo de diretório raiz
    $rootLabel = New-Object System.Windows.Forms.Label
    $rootLabel.Location = New-Object System.Drawing.Point(10, 80)
    $rootLabel.Size = New-Object System.Drawing.Size(100, 23)
    $rootLabel.Text = "Diretório Raiz:"
    $tabPage.Controls.Add($rootLabel)
    
    $rootTextBox = New-Object System.Windows.Forms.TextBox
    $rootTextBox.Location = New-Object System.Drawing.Point(120, 80)
    $rootTextBox.Size = New-Object System.Drawing.Size(200, 23)
    $tabPage.Controls.Add($rootTextBox)
    
    $browseButton = New-Object System.Windows.Forms.Button
    $browseButton.Location = New-Object System.Drawing.Point(330, 80)
    $browseButton.Size = New-Object System.Drawing.Size(80, 23)
    $browseButton.Text = "Procurar"
    $browseButton.Add_Click({
        $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
        $folderBrowser.Description = "Selecione o diretório raiz do site"
        $folderBrowser.RootFolder = [System.Environment+SpecialFolder]::MyComputer
        
        if ($folderBrowser.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
            $rootTextBox.Text = $folderBrowser.SelectedPath
        }
    })
    $tabPage.Controls.Add($browseButton)
    
    # Campo de PHP Upstream
    $phpLabel = New-Object System.Windows.Forms.Label
    $phpLabel.Location = New-Object System.Drawing.Point(10, 110)
    $phpLabel.Size = New-Object System.Drawing.Size(100, 23)
    $phpLabel.Text = "PHP Upstream:"
    $tabPage.Controls.Add($phpLabel)
    
    $phpCombo = New-Object System.Windows.Forms.ComboBox
    $phpCombo.Location = New-Object System.Drawing.Point(120, 110)
    $phpCombo.Size = New-Object System.Drawing.Size(200, 23)
    $phpCombo.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    
    # Adicionar itens de PHP instalados
    if (Test-Path $phpDir) {
        $phpVersions = Get-ChildItem $phpDir -Directory | ForEach-Object { $_.Name -replace '^php-', '' }
        foreach ($v in $phpVersions) {
            [void]$phpCombo.Items.Add("php$v")
        }
        
        if ($phpCombo.Items.Count -gt 0) {
            $phpCombo.SelectedIndex = 0
        }
    }
    
    $tabPage.Controls.Add($phpCombo)
    
    # Campo de versão do Nginx
    $nginxLabel = New-Object System.Windows.Forms.Label
    $nginxLabel.Location = New-Object System.Drawing.Point(10, 140)
    $nginxLabel.Size = New-Object System.Drawing.Size(100, 23)
    $nginxLabel.Text = "Nginx Versão:"
    $tabPage.Controls.Add($nginxLabel)
    
    $nginxCombo = New-Object System.Windows.Forms.ComboBox
    $nginxCombo.Location = New-Object System.Drawing.Point(120, 140)
    $nginxCombo.Size = New-Object System.Drawing.Size(200, 23)
    $nginxCombo.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    
    # Adicionar versões de Nginx instaladas
    if (Test-Path $nginxDir) {
        $nginxVersions = Get-ChildItem $nginxDir -Directory | ForEach-Object { $_.Name -replace '^nginx-', '' }
        foreach ($v in $nginxVersions) {
            [void]$nginxCombo.Items.Add($v)
        }
        
        if ($nginxCombo.Items.Count -gt 0) {
            $nginxCombo.SelectedIndex = 0
        }
    }
    
    $tabPage.Controls.Add($nginxCombo)
    
    # Botão para criar site
    $createSiteButton = New-Object System.Windows.Forms.Button
    $createSiteButton.Location = New-Object System.Drawing.Point(120, 180)
    $createSiteButton.Size = New-Object System.Drawing.Size(150, 30)
    $createSiteButton.Text = "Criar Configuração"
    $createSiteButton.Add_Click({
        $domain = $domainTextBox.Text.Trim()
        $root = $rootTextBox.Text.Trim()
        $phpUpstream = $phpCombo.SelectedItem
        $nginxVersion = $nginxCombo.SelectedItem
        
        if ([string]::IsNullOrEmpty($domain)) {
            [System.Windows.Forms.MessageBox]::Show("Por favor, informe o domínio do site.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
        
        Update-StatusMessage "Criando configuração para o site $domain..."
        
        $args = @("site", $domain)
        
        if (-not [string]::IsNullOrEmpty($root)) {
            $args += "-root", $root
        }
        
        if ($phpUpstream) {
            $args += "-php", $phpUpstream
        }
        
        if ($nginxVersion) {
            $args += "-nginx", $nginxVersion
        }
        
        try {
            & "$PSScriptRoot\..\setup.ps1" @args
            
            Update-StatusMessage "Configuração para o site $domain criada com sucesso."
            [System.Windows.Forms.MessageBox]::Show(
                "Configuração para o site $domain criada com sucesso.`n`n" + 
                "Não se esqueça de adicionar uma entrada no arquivo hosts:`n" +
                "127.0.0.1    $domain",
                "Sucesso",
                [System.Windows.Forms.MessageBoxButtons]::OK,
                [System.Windows.Forms.MessageBoxIcon]::Information
            )
            
            # Limpar os campos
            $domainTextBox.Text = ""
            $rootTextBox.Text = ""
        }
        catch {
            Update-StatusMessage "Erro ao criar configuração do site: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao criar configuração do site: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($createSiteButton)
    
    # Botão para gerar certificado SSL
    $sslLabel = New-Object System.Windows.Forms.Label
    $sslLabel.Location = New-Object System.Drawing.Point(10, 230)
    $sslLabel.Size = New-Object System.Drawing.Size(200, 23)
    $sslLabel.Text = "Gerar certificado SSL"
    $tabPage.Controls.Add($sslLabel)
    
    $sslDomainLabel = New-Object System.Windows.Forms.Label
    $sslDomainLabel.Location = New-Object System.Drawing.Point(10, 260)
    $sslDomainLabel.Size = New-Object System.Drawing.Size(100, 23)
    $sslDomainLabel.Text = "Domínio SSL:"
    $tabPage.Controls.Add($sslDomainLabel)
    
    $sslDomainTextBox = New-Object System.Windows.Forms.TextBox
    $sslDomainTextBox.Location = New-Object System.Drawing.Point(120, 260)
    $sslDomainTextBox.Size = New-Object System.Drawing.Size(200, 23)
    $tabPage.Controls.Add($sslDomainTextBox)
    
    $generateSslButton = New-Object System.Windows.Forms.Button
    $generateSslButton.Location = New-Object System.Drawing.Point(120, 290)
    $generateSslButton.Size = New-Object System.Drawing.Size(150, 30)
    $generateSslButton.Text = "Gerar Certificado SSL"
    $generateSslButton.Add_Click({
        $domain = $sslDomainTextBox.Text.Trim()
        
        if ([string]::IsNullOrEmpty($domain)) {
            [System.Windows.Forms.MessageBox]::Show("Por favor, informe o domínio para o certificado SSL.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
            return
        }
        
        Update-StatusMessage "Gerando certificado SSL para $domain..."
        
        try {
            & "$PSScriptRoot\..\setup.ps1" ssl $domain
            
            Update-StatusMessage "Certificado SSL para $domain gerado com sucesso."
            [System.Windows.Forms.MessageBox]::Show("Certificado SSL para $domain gerado com sucesso.", "Sucesso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Information)
            
            # Limpar o campo de domínio
            $sslDomainTextBox.Text = ""
        }
        catch {
            Update-StatusMessage "Erro ao gerar certificado SSL: $_"
            [System.Windows.Forms.MessageBox]::Show("Erro ao gerar certificado SSL: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    
    $tabPage.Controls.Add($generateSslButton)
}

# Configuração da Tab de Utilitários
function Setup-UtilsTab {
    param($tabPage)
    
    # Reset das variáveis globais para esta aba
    $script:utilsOutputBox = $null
    $script:utilsCommandTextBox = $null
    
    # Terminal/Console embutido para executar comandos
    $consoleLabel = New-Object System.Windows.Forms.Label
    $consoleLabel.Location = New-Object System.Drawing.Point(10, 10)
    $consoleLabel.Size = New-Object System.Drawing.Size(740, 23)
    $consoleLabel.Text = "Console DevStack"
    $tabPage.Controls.Add($consoleLabel)
    
    $commandLabel = New-Object System.Windows.Forms.Label
    $commandLabel.Location = New-Object System.Drawing.Point(10, 40)
    $commandLabel.Size = New-Object System.Drawing.Size(70, 23)
    $commandLabel.Text = "Comando:"
    $tabPage.Controls.Add($commandLabel)
    
    # Adicionar variável global para o campo de texto de comando
    $commandTextBox = New-Object System.Windows.Forms.TextBox
    $commandTextBox.Location = New-Object System.Drawing.Point(90, 40)
    $commandTextBox.Size = New-Object System.Drawing.Size(560, 23)
    $tabPage.Controls.Add($commandTextBox)
    
    # Atribuir à variável global
    $script:utilsCommandTextBox = $commandTextBox
    
    # Criar a caixa de saída primeiro
    $outputBox = New-Object System.Windows.Forms.RichTextBox
    $outputBox.Location = New-Object System.Drawing.Point(10, 70)
    $outputBox.Size = New-Object System.Drawing.Size(740, 420)
    $outputBox.ReadOnly = $true
    $outputBox.BackColor = [System.Drawing.Color]::Black
    $outputBox.ForeColor = [System.Drawing.Color]::White
    $outputBox.Font = New-Object System.Drawing.Font("Consolas", 10)
    $tabPage.Controls.Add($outputBox)
    
    # Atribuir a caixa de texto de saída à variável global
    $script:utilsOutputBox = $outputBox
    
    # Adicionar sugestões de comandos populares ao iniciar
    try {
        $script:utilsOutputBox.AppendText("DevStackSetup GUI Console`r`n")
        $script:utilsOutputBox.AppendText("-------------------`r`n")
        $script:utilsOutputBox.AppendText("Comandos populares:`r`n")
        $script:utilsOutputBox.AppendText("  * status`r`n")
        $script:utilsOutputBox.AppendText("  * list --installed`r`n")
        $script:utilsOutputBox.AppendText("  * list <componente>`r`n")
        $script:utilsOutputBox.AppendText("  * test`r`n")
        $script:utilsOutputBox.AppendText("  * doctor`r`n")
        $script:utilsOutputBox.AppendText("-------------------`r`n`r`n")
    } catch {
        [System.Windows.Forms.MessageBox]::Show("Erro ao inicializar console: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
    }
    # Criar o botão de execução
    $executeButton = New-Object System.Windows.Forms.Button
    $executeButton.Location = New-Object System.Drawing.Point(660, 39)
    $executeButton.Size = New-Object System.Drawing.Size(90, 25)
    $executeButton.Text = "Executar"
    $executeButton.Add_Click({
        try {
            if (-not $script:utilsCommandTextBox) {
                [System.Windows.Forms.MessageBox]::Show("Erro: A caixa de comando não está inicializada.", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
                return
            }

            $command = $script:utilsCommandTextBox.Text.Trim()
            if ([string]::IsNullOrEmpty($command)) {
                [System.Windows.Forms.MessageBox]::Show("Por favor, informe um comando para executar.", "Aviso", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Warning)
                return
            }
            
            Update-StatusMessage "Executando comando: $command"
            
            # Verificar se o OutputBox está disponível
            if (-not $script:utilsOutputBox) {
                [System.Windows.Forms.MessageBox]::Show("Erro: A caixa de saída não está inicializada.", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
                return
            }
            
            # Garantir que a referência ao OutputBox ainda é válida
            $outputBox = $script:utilsOutputBox
            $outputBox.AppendText("> $command`r`n")
            
            try {
                $cmd = "& '$PSScriptRoot\..\setup.ps1' $command"
                $output = Invoke-Expression $cmd | Out-String
                $outputBox.AppendText($output)
                $outputBox.AppendText("`r`n")
                $outputBox.ScrollToCaret()
                Update-StatusMessage "Comando executado com sucesso."
            }
            catch {
                $outputBox.AppendText("Erro: $_`r`n")
                $outputBox.ScrollToCaret()
                Update-StatusMessage "Erro ao executar comando: $_"
            }
        } catch {
            [System.Windows.Forms.MessageBox]::Show("Erro ao processar comando: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    })
    $tabPage.Controls.Add($executeButton)
    
    # Atribuir à variável global para uso posterior
    $script:utilsExecuteButton = $executeButton
    
    # Permitir que Enter execute o comando
    $script:utilsCommandTextBox.Add_KeyDown({
        if ($_.KeyCode -eq [System.Windows.Forms.Keys]::Enter) {
            # Usar a referência global do botão
            if ($script:utilsExecuteButton) {
                $script:utilsExecuteButton.PerformClick()
                $_.SuppressKeyPress = $true
            }
        }
    })
}

# Função de acesso à GUI para ser chamada do setup.ps1
function Invoke-DevStackGUI {
    Start-DevStackGUI
}

# Função para atualizar todas as abas com dados frescos
function Update-AllTabs {
    # Atualizar lista de ferramentas instaladas (tab 1)
    Update-InstalledToolsList
    
    # Atualizar lista de componentes para desinstalar (tab 3)
    Update-UninstallComponentList
    
    # Atualizar lista de serviços (tab 4)
    Update-ServicesList
    
    # Verificar se o OutputBox do console de utilitários está inicializado
    if ($script:utilsOutputBox -eq $null) {
        # Nesse caso não fazemos nada, pois a aba não foi acessada ainda
    } else {
        # Adicionar uma mensagem indicando que os dados foram atualizados
        $script:utilsOutputBox.AppendText("[$(Get-Date -Format 'HH:mm:ss')] Dados do sistema atualizados.`r`n")
        $script:utilsOutputBox.ScrollToCaret()
    }
    
    # Outras atualizações podem ser adicionadas aqui conforme necessário
    Update-StatusMessage "Todas as informações foram atualizadas."
}
