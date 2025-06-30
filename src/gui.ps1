# DevStack GUI
# Este arquivo implementa uma interface gráfica para o DevStackSetup
# utilizando Windows Forms e as funções existentes nos outros arquivos

Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
[System.Windows.Forms.Application]::EnableVisualStyles()

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


# Modern Material-inspired themes (no icons)
$ModernLightTheme = @{
    FormBackColor = [System.Drawing.Color]::FromArgb(231, 232, 233)
    ForeColor = [System.Drawing.Color]::FromArgb(33, 37, 41)
    ControlBackColor = [System.Drawing.Color]::FromArgb(245, 246, 247)
    ButtonBackColor = [System.Drawing.Color]::FromArgb(0, 123, 255)
    ButtonForeColor = [System.Drawing.Color]::White
    ButtonHoverColor = [System.Drawing.Color]::FromArgb(0, 86, 179)
    AccentColor = [System.Drawing.Color]::FromArgb(40, 167, 69)
    GridBackColor = [System.Drawing.Color]::White
    GridForeColor = [System.Drawing.Color]::FromArgb(33, 37, 41)
    GridHeaderBackColor = [System.Drawing.Color]::FromArgb(233, 236, 239)
    GridHeaderForeColor = [System.Drawing.Color]::FromArgb(73, 80, 87)
    StatusBackColor = [System.Drawing.Color]::FromArgb(231, 232, 233)
    StatusForeColor = [System.Drawing.Color]::FromArgb(108, 117, 125)
    BorderColor = [System.Drawing.Color]::FromArgb(222, 226, 230)
    ShadowColor = [System.Drawing.Color]::FromArgb(50, 0, 0, 0)
    ContentBackColor = [System.Drawing.Color]::FromArgb(253, 254, 255)
}

$ModernDarkTheme = @{
    FormBackColor = [System.Drawing.Color]::FromArgb(33, 37, 41)
    ForeColor = [System.Drawing.Color]::FromArgb(248, 249, 250)
    ControlBackColor = [System.Drawing.Color]::FromArgb(52, 58, 64)
    ButtonBackColor = [System.Drawing.Color]::FromArgb(0, 123, 255)
    ButtonForeColor = [System.Drawing.Color]::White
    ButtonHoverColor = [System.Drawing.Color]::FromArgb(0, 86, 179)
    AccentColor = [System.Drawing.Color]::FromArgb(40, 167, 69)
    GridBackColor = [System.Drawing.Color]::FromArgb(52, 58, 64)
    GridForeColor = [System.Drawing.Color]::FromArgb(248, 249, 250)
    GridHeaderBackColor = [System.Drawing.Color]::FromArgb(73, 80, 87)
    GridHeaderForeColor = [System.Drawing.Color]::FromArgb(248, 249, 250)
    StatusBackColor = [System.Drawing.Color]::FromArgb(33, 37, 41)
    StatusForeColor = [System.Drawing.Color]::FromArgb(173, 181, 189)
    BorderColor = [System.Drawing.Color]::FromArgb(73, 80, 87)
    ShadowColor = [System.Drawing.Color]::FromArgb(80, 0, 0, 0)
    ContentBackColor = [System.Drawing.Color]::FromArgb(44, 48, 54)
}
# Tema e fonte modernos
$script:CurrentTheme = "dark"
try {
    $script:modernFont = New-Object System.Drawing.Font("Segoe UI", 10)
    $script:modernFontBold = New-Object System.Drawing.Font("Segoe UI", 10, [System.Drawing.FontStyle]::Bold)
    $script:iconFont = New-Object System.Drawing.Font("Segoe MDL2 Assets", 16)
}
catch {
    # Fallback para fontes padrão em caso de erro
    $script:modernFont = New-Object System.Drawing.Font("Microsoft Sans Serif", 10)
    $script:modernFontBold = New-Object System.Drawing.Font("Microsoft Sans Serif", 10, [System.Drawing.FontStyle]::Bold)
    $script:iconFont = New-Object System.Drawing.Font("Microsoft Sans Serif", 16)
    Write-Warning "Erro ao carregar fontes customizadas, usando fontes padrão"
}


# Modern theme application (no icons)
function Apply-ModernTheme {
    param($form)
    try {
        if (-not $form) {
            Write-Warning "Form is null - cannot apply theme"
            return
        }
        
        $theme = if ($script:CurrentTheme -eq "dark") { $ModernDarkTheme } else { $ModernLightTheme }
        if (-not $theme) {
            Write-Warning "Theme not found - using default"
            return
        }
        
        $form.BackColor = $theme.FormBackColor
        $form.ForeColor = $theme.ForeColor
        if ($script:modernFont) {
            $form.Font = $script:modernFont
        }
        
        if ($form.Controls) {
            Apply-ThemeToControls $form.Controls $theme
        }
    }
    catch {
        Write-Host "Erro ao aplicar tema: $($_.Exception.Message)" -ForegroundColor Red
    }
}

function Apply-ThemeToControls {
    param($controls, $theme)
    try {
        if (-not $controls -or -not $theme) { return }
        
        foreach ($ctrl in $controls) {
            if (-not $ctrl) { continue }
            
            try {
                if ((-not $ctrl.Font -or $ctrl.Font.Name -eq "Microsoft Sans Serif") -and $script:modernFont) {
                    $ctrl.Font = $script:modernFont
                }
                
                switch ($ctrl.GetType().Name) {
                    "Button" {
                        $ctrl.BackColor = $theme.ButtonBackColor
                        $ctrl.ForeColor = $theme.ButtonForeColor
                        $ctrl.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
                        $ctrl.FlatAppearance.BorderSize = 1
                        $ctrl.FlatAppearance.BorderColor = $theme.BorderColor
                    }
                    "ListBox" {
                        $ctrl.BackColor = $theme.ControlBackColor
                        $ctrl.ForeColor = $theme.ForeColor
                        $ctrl.BorderStyle = [System.Windows.Forms.BorderStyle]::None
                        $ctrl.Font = $script:modernFont
                    }
                    "DataGridView" {
                        $ctrl.BackgroundColor = $theme.GridBackColor
                        $ctrl.ForeColor = $theme.GridForeColor
                        $ctrl.GridColor = [System.Drawing.Color]::White
                        $ctrl.DefaultCellStyle.BackColor = $theme.GridBackColor
                        $ctrl.DefaultCellStyle.ForeColor = $theme.GridForeColor
                        $ctrl.DefaultCellStyle.SelectionBackColor = $theme.AccentColor
                        $ctrl.DefaultCellStyle.SelectionForeColor = [System.Drawing.Color]::White
                        $ctrl.ColumnHeadersDefaultCellStyle.BackColor = $theme.GridHeaderBackColor
                        $ctrl.ColumnHeadersDefaultCellStyle.SelectionBackColor = $theme.GridHeaderBackColor
                        $ctrl.ColumnHeadersDefaultCellStyle.ForeColor = $theme.GridHeaderForeColor
                        $ctrl.EnableHeadersVisualStyles = $false
                        $ctrl.BorderStyle = [System.Windows.Forms.BorderStyle]::None
                    }
                    "TextBox" {
                        $ctrl.BackColor = $theme.ControlBackColor
                        $ctrl.ForeColor = $theme.ForeColor
                        $ctrl.BorderStyle = [System.Windows.Forms.BorderStyle]::FixedSingle
                    }
                    "ComboBox" {
                        $ctrl.BackColor = $theme.ControlBackColor
                        $ctrl.ForeColor = $theme.ForeColor
                        $ctrl.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
                    }
                    "RichTextBox" {
                        $ctrl.BackColor = [System.Drawing.Color]::FromArgb(30, 30, 30)
                        $ctrl.ForeColor = [System.Drawing.Color]::White
                    }
                    "Label" {
                        if ($ctrl.Parent -and $ctrl.Parent.GetType().Name -eq "Button") {
                            # Se o label está dentro de um botão, manter branco
                            $ctrl.ForeColor = $theme.ButtonForeColor
                        } else {
                            $ctrl.ForeColor = $theme.ForeColor
                        }
                    }
                    "StatusStrip" {
                        $ctrl.BackColor = $theme.StatusBackColor
                        if ($ctrl.Items) {
                            foreach ($item in $ctrl.Items) { 
                                if ($item) { $item.ForeColor = $theme.StatusForeColor }
                            }
                        }
                    }
                    "Form" {
                        $ctrl.BackColor = $theme.FormBackColor
                    }
                }
                
                if ($ctrl.Controls -and $ctrl.Controls.Count -gt 0) {
                    Apply-ThemeToControls $ctrl.Controls $theme
                }
            }
            catch {
                Write-Warning "Erro ao aplicar tema ao controle $($ctrl.GetType().Name): $($_.Exception.Message)"
            }
        }
    }
    catch {
        Write-Host "Erro geral ao aplicar tema aos controles: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Modernized main GUI function (no icons)
function Start-DevStackGUI {
    $script:mainForm = New-Object System.Windows.Forms.Form
    $script:mainForm.Text = "DevStack Manager"
    $script:mainForm.Size = New-Object System.Drawing.Size(1000, 650)
    $script:mainForm.StartPosition = "CenterScreen"
    $script:mainForm.MinimumSize = New-Object System.Drawing.Size(1000, 650)
    $script:mainForm.Font = $script:modernFont

    # Top panel with app name and theme/refresh buttons
    $topPanel = New-Object System.Windows.Forms.Panel
    $topPanel.Height = 60
    $topPanel.Dock = [System.Windows.Forms.DockStyle]::Top
    $topPanel.BackColor = [System.Drawing.Color]::Transparent
    $script:topPanel = $topPanel
    $titleLabel = New-Object System.Windows.Forms.Label
    $titleLabel.Text = "DevStack Manager"
    $titleLabel.Font = New-Object System.Drawing.Font($script:modernFont.FontFamily, 18, [System.Drawing.FontStyle]::Bold)
    $titleLabel.Location = New-Object System.Drawing.Point(20, 15)
    $titleLabel.Size = New-Object System.Drawing.Size(400, 35)
    $topPanel.Controls.Add($titleLabel)

    # Botão Tema com ícone grande e texto pequeno
    $themeButton = New-Object System.Windows.Forms.Button
    $themeButton.Size = New-Object System.Drawing.Size(120, 35)
    $themeButton.Location = New-Object System.Drawing.Point(850, 15)
    $themeButton.BackColor = [System.Drawing.Color]::Transparent
    $themeButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $script:themeButton = $themeButton
    $topPanel.Add_Resize({
        $script:themeButton.Location = New-Object System.Drawing.Point(
            ($script:topPanel.Width - $script:themeButton.Width - 15), 
            15
        )
    })
    $iconLabel = New-Object System.Windows.Forms.Label
    $iconLabel.Text = [char]0xE706 # Segoe MDL2 Assets: 'Theme'
    $iconLabel.Font = $script:iconFont
    $iconLabel.AutoSize = $true
    $iconLabel.Location = New-Object System.Drawing.Point(15, 6)
    $iconLabel.Size = New-Object System.Drawing.Size(32, 32)
    $iconLabel.TextAlign = 'MiddleLeft'
    $iconLabel.ForeColor = $ModernDarkTheme.ButtonBackColor
    $iconLabel.BackColor = [System.Drawing.Color]::Transparent

    $textLabel = New-Object System.Windows.Forms.Label
    $textLabel.Text = "Tema"
    $textLabel.Font = $script:modernFont
    $textLabel.AutoSize = $true
    $textLabel.Location = New-Object System.Drawing.Point(47, 6)
    $textLabel.Size = New-Object System.Drawing.Size(60, 20)
    $textLabel.TextAlign = 'MiddleLeft'
    $textLabel.ForeColor = $ModernDarkTheme.ButtonBackColor
    $textLabel.BackColor = [System.Drawing.Color]::Transparent

    # Clique em qualquer parte do painel ou labels
    $themeClick = {
        try {
            $script:CurrentTheme = if ($script:CurrentTheme -eq "light") { "dark" } else { "light" }
            
            # Encontrar o ícone dentro do botão de tema
            $themeBtn = $this.Parent
            if (-not $themeBtn -or $themeBtn.GetType().Name -ne "Button") {
                $themeBtn = $this.Parent.Controls | Where-Object { $_.GetType().Name -eq "Button" }
                if ($themeBtn -is [System.Collections.IEnumerable]) {
                    $themeBtn = $themeBtn | Select-Object -First 1
                }
            }
            if (-not $themeBtn -or $themeBtn.GetType().Name -ne "Button") { return }
            $iconLbl = $themeBtn.Controls | Where-Object { $_.GetType().Name -eq "Label" -and $_.Text -match "[\uE706\uE708]" }
            
            if ($iconLbl) {
                $iconLbl.Text = if ($script:CurrentTheme -eq "light") { [char]0xE708 } else { [char]0xE706 }
                $iconLbl.Location = if ($script:CurrentTheme -eq "light") { New-Object System.Drawing.Point(15, 4) } else { New-Object System.Drawing.Point(15, 6) }
            }
            
            Apply-ModernTheme $script:mainForm
        }
        catch {
            Write-Host "Erro ao alternar tema: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    $themeButton.Add_Click($themeClick)
    $iconLabel.Add_Click($themeClick)
    $textLabel.Add_Click($themeClick)

    $themeButton.Controls.Add($iconLabel)
    $themeButton.Controls.Add($textLabel)
    $topPanel.Controls.Add($themeButton)

    $statusStrip = New-Object System.Windows.Forms.StatusStrip
    $statusLabel = New-Object System.Windows.Forms.ToolStripStatusLabel
    $statusLabel.Text = "Pronto"
    $statusLabel.Font = $script:modernFont
    $statusStrip.Items.Add($statusLabel)
    $script:statusLabel = $statusLabel

    $script:mainForm.Controls.Add($statusStrip)
    $script:mainForm.Controls.Add($topPanel)

    # --- Modern ListBox Navigation ---
    $navListBox = New-Object System.Windows.Forms.ListBox
    $navListBox.Width = 220
    $navListBox.Top = 60
    # Calcular altura do ListBox entre o topo e o status bar
    $statusStripHeight = 22
    if ($statusStrip -and $statusStrip.Height) { $statusStripHeight = $statusStrip.Height }
    $navListBox.Height = $script:mainForm.ClientSize.Height - $statusStripHeight
    $navListBox.Anchor = 'Top,Left,Bottom'
    $navListBox.DrawMode = [System.Windows.Forms.DrawMode]::OwnerDrawFixed
    $navListBox.ItemHeight = 48
    $navListBox.BorderStyle = [System.Windows.Forms.BorderStyle]::None
    $navListBox.BackColor = $ModernDarkTheme.ControlBackColor
    $navListBox.ForeColor = $ModernDarkTheme.ForeColor
    $navListBox.Font = $script:modernFont

    $navItems = @(
        @{ Name = "Ferramentas Instaladas"; Icon = [char]0xE9D5; Setup = 'Setup-InstalledToolsTab' },
        @{ Name = "Instalar"; Icon = [char]0xE896; Setup = 'Setup-InstallTab' },
        @{ Name = "Desinstalar"; Icon = [char]0xE74D; Setup = 'Setup-UninstallTab' },
        @{ Name = "Serviços"; Icon = [char]0xEC57; Setup = 'Setup-ServicesTab' },
        @{ Name = "Configurações"; Icon = [char]0xE713; Setup = 'Setup-ConfigTab' },
        @{ Name = "Sites"; Icon = [char]0xE774; Setup = 'Setup-SitesTab' },
        @{ Name = "Utilitários"; Icon = [char]0xE912; Setup = 'Setup-UtilsTab' }
    )
    foreach ($item in $navItems) {
        $navListBox.Items.Add($item)
    }
    $navListBox.Add_DrawItem({
        param($sender, $e)
        $g = $e.Graphics
        $item = $sender.Items[$e.Index]
        $rect = $e.Bounds
        $theme = if ($script:CurrentTheme -eq "dark") { $ModernDarkTheme } else { $ModernLightTheme }
        $isSelected = ($e.Index -eq $sender.SelectedIndex)
        $backColor = if ($isSelected) { $theme.AccentColor } else { $theme.ControlBackColor }
        $foreColor = if ($isSelected) { $theme.ButtonForeColor } else { $theme.ForeColor }
        # Corrige: limpa o fundo corretamente para cada item
        $g.FillRectangle([System.Drawing.SolidBrush]::new($backColor), $rect)
        $iconFont = $script:iconFont
        $icon = $item.Icon
        $g.DrawString($icon, $iconFont, [System.Drawing.SolidBrush]::new($foreColor), $rect.Left+16, $rect.Top+12)
        $textFont = $script:modernFontBold
        $g.DrawString($item.Name, $textFont, [System.Drawing.SolidBrush]::new($foreColor), $rect.Left+56, $rect.Top+14)
        # Remove highlight de seleção padrão do ListBox
        $e.DrawFocusRectangle()
    })
    $script:mainForm.Controls.Add($navListBox)

    # --- Main Content Panel ---
    $contentPanel = New-Object System.Windows.Forms.Panel
    $contentPanel.Dock = [System.Windows.Forms.DockStyle]::Fill
    $contentPanel.BackColor = $ModernDarkTheme.ContentBackColor

    # Atualizar o BackColor do contentPanel ao trocar o tema
    $updateContentPanelTheme = {
        $theme = if ($script:CurrentTheme -eq "dark") { $ModernDarkTheme } else { $ModernLightTheme }
        $contentPanel.BackColor = $theme.ContentBackColor
    }
    $themeButton.Add_Click($updateContentPanelTheme)
    $iconLabel.Add_Click($updateContentPanelTheme)
    $textLabel.Add_Click($updateContentPanelTheme)
    $script:mainForm.Controls.Add($contentPanel)

    # --- Section Panels (one per nav item) ---
    $sectionPanels = @{}
    for ($i=0; $i -lt $navItems.Count; $i++) {
        $item = $navItems[$i]
        $panel = New-Object System.Windows.Forms.Panel
        $panel.Dock = [System.Windows.Forms.DockStyle]::Fill
        $panel.Visible = $false
        $contentPanel.Controls.Add($panel)
        $sectionPanels[$item.Name] = $panel
        & $item.Setup $panel
    }
    $sectionPanels[$navItems[0].Name].Visible = $true
    $navListBox.SelectedIndex = 0

    $navListBox.Add_SelectedIndexChanged({
        for ($i=0; $i -lt $navItems.Count; $i++) {
            $name = $navItems[$i].Name
            $sectionPanels[$name].Visible = ($i -eq $navListBox.SelectedIndex)
        }
        $navListBox.Invalidate() # Força a repintura para corrigir o AccentColor
        Update-StatusMessage "${($navItems[$navListBox.SelectedIndex].Name)} selecionado"
        switch ($navItems[$navListBox.SelectedIndex].Name) {
            "Ferramentas Instaladas" { Update-InstalledToolsList }
            "Desinstalar" { Update-UninstallComponentList }
            "Serviços" { Update-ServicesList }
        }
    })

    $script:mainForm.Add_Shown({
        $script:mainForm.Activate()
        Update-InstalledToolsList
        Update-StatusMessage "Interface moderna carregada com sucesso!"
    })

    Apply-ModernTheme $script:mainForm
    [void]$script:mainForm.ShowDialog()
}

# Esta seção foi movida para o início do arquivo

# Configuração da Tab de Ferramentas Instaladas
function Setup-InstalledToolsTab {
    param($tabPage)

    # Criar DataGridView para listar ferramentas instaladas
    $dataGrid = New-Object System.Windows.Forms.DataGridView
    $dataGrid.Location = New-Object System.Drawing.Point(240, 80)
    $dataGrid.Size = New-Object System.Drawing.Size(730, 450)
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
    $refreshButton.Location = New-Object System.Drawing.Point(240, 540)
    $refreshButton.Size = New-Object System.Drawing.Size(120, 30)
    $refreshButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $refreshButton.BackColor = [System.Drawing.Color]::Transparent
    $refreshButton.Cursor = [System.Windows.Forms.Cursors]::Hand

    $refreshIcon2 = New-Object System.Windows.Forms.Label
    $refreshIcon2.Text = [char]0xE72C # 'Refresh'
    $refreshIcon2.Font = $script:iconFont
    $refreshIcon2.AutoSize = $true
    $refreshIcon2.Location = New-Object System.Drawing.Point(5, 3)
    $refreshIcon2.Size = New-Object System.Drawing.Size(24, 24)
    $refreshIcon2.TextAlign = 'MiddleLeft'
    $refreshIcon2.ForeColor = $ModernDarkTheme.ButtonBackColor
    $refreshIcon2.BackColor = [System.Drawing.Color]::Transparent

    $refreshText2 = New-Object System.Windows.Forms.Label
    $refreshText2.Text = "Atualizar"
    $refreshText2.Font = $script:modernFont
    $refreshText2.AutoSize = $true
    $refreshText2.Location = New-Object System.Drawing.Point(32, 4)
    $refreshText2.Size = New-Object System.Drawing.Size(70, 20)
    $refreshText2.TextAlign = 'MiddleLeft'
    $refreshText2.ForeColor = $ModernDarkTheme.ButtonBackColor
    $refreshText2.BackColor = [System.Drawing.Color]::Transparent

    $refreshButton.Controls.Add($refreshIcon2)
    $refreshButton.Controls.Add($refreshText2)
    $refreshClick = {
        try {
            Update-StatusMessage "Atualizando lista de ferramentas instaladas..."
            Update-InstalledToolsList
            Update-StatusMessage "Lista atualizada."
        }
        catch {
            Write-Host "Erro ao atualizar lista de ferramentas: $($_.Exception.Message)" -ForegroundColor Red
            [System.Windows.Forms.MessageBox]::Show("Erro ao atualizar lista de ferramentas: $($_.Exception.Message)", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    }
    $refreshButton.Add_Click($refreshClick)
    $refreshIcon2.Add_Click($refreshClick)
    $refreshText2.Add_Click($refreshClick)
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
    $componentLabel.Location = New-Object System.Drawing.Point(240, 80)
    $componentLabel.Size = New-Object System.Drawing.Size(100, 23)
    $componentLabel.Text = "Componente:"
    $tabPage.Controls.Add($componentLabel)
    
    $componentCombo = New-Object System.Windows.Forms.ComboBox
    $componentCombo.Location = New-Object System.Drawing.Point(350, 80)
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
    $versionLabel.Location = New-Object System.Drawing.Point(240, 120)
    $versionLabel.Size = New-Object System.Drawing.Size(100, 23)
    $versionLabel.Text = "Versão:"
    $tabPage.Controls.Add($versionLabel)
    
    $versionCombo = New-Object System.Windows.Forms.ComboBox
    $versionCombo.Location = New-Object System.Drawing.Point(350, 120)
    $versionCombo.Size = New-Object System.Drawing.Size(200, 23)
    $versionCombo.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    $tabPage.Controls.Add($versionCombo)
    
    # Armazenar referência em variável global antes de usá-la
    $script:uninstallVersionComboBox = $versionCombo
    
    # Botão de instalação
    $installButton = New-Object System.Windows.Forms.Button
    $installButton.Location = New-Object System.Drawing.Point(350, 160)
    $installButton.Size = New-Object System.Drawing.Size(130, 30)
    $installButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $installButton.BackColor = [System.Drawing.Color]::Transparent
    $installButton.Cursor = [System.Windows.Forms.Cursors]::Hand

    # Ícone Segoe MDL2 Assets (Download: U+E896)
    $installIcon = New-Object System.Windows.Forms.Label
    $installIcon.Text = [char]0xE896
    $installIcon.Font = $script:iconFont
    $installIcon.AutoSize = $true
    $installIcon.Location = New-Object System.Drawing.Point(10, 3)
    $installIcon.Size = New-Object System.Drawing.Size(24, 24)
    $installIcon.TextAlign = 'MiddleLeft'
    $installIcon.ForeColor = $ModernDarkTheme.ButtonBackColor
    $installIcon.BackColor = [System.Drawing.Color]::Transparent

    $installText = New-Object System.Windows.Forms.Label
    $installText.Text = "Instalar"
    $installText.Font = $script:modernFont
    $installText.AutoSize = $true
    $installText.Location = New-Object System.Drawing.Point(38, 4)
    $installText.Size = New-Object System.Drawing.Size(70, 20)
    $installText.TextAlign = 'MiddleLeft'
    $installText.ForeColor = $ModernDarkTheme.ButtonBackColor
    $installText.BackColor = [System.Drawing.Color]::Transparent

    $installButton.Controls.Add($installIcon)
    $installButton.Controls.Add($installText)
    $installClick = {
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
    }
    $installButton.Add_Click($installClick)
    $installIcon.Add_Click($installClick)
    $installText.Add_Click($installClick)
    $tabPage.Controls.Add($installButton)
    
    # Tela de console para mostrar o progresso
    $outputBox = New-Object System.Windows.Forms.RichTextBox
    $outputBox.Location = New-Object System.Drawing.Point(240, 210)
    $outputBox.Size = New-Object System.Drawing.Size(730, 350)
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
    $componentLabel.Location = New-Object System.Drawing.Point(240, 80)
    $componentLabel.Size = New-Object System.Drawing.Size(100, 23)
    $componentLabel.Text = "Componente:"
    $tabPage.Controls.Add($componentLabel)
    $componentCombo = New-Object System.Windows.Forms.ComboBox
    $componentCombo.Location = New-Object System.Drawing.Point(350, 80)
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
    $versionLabel.Location = New-Object System.Drawing.Point(240, 120)
    $versionLabel.Size = New-Object System.Drawing.Size(100, 23)
    $versionLabel.Text = "Versão:"
    $tabPage.Controls.Add($versionLabel)
    
    $versionCombo = New-Object System.Windows.Forms.ComboBox
    $versionCombo.Location = New-Object System.Drawing.Point(350, 120)
    $versionCombo.Size = New-Object System.Drawing.Size(200, 23)
    $versionCombo.DropDownStyle = [System.Windows.Forms.ComboBoxStyle]::DropDownList
    $tabPage.Controls.Add($versionCombo)
    
    # Armazenar referência em variável global antes de usá-la
    $script:uninstallVersionComboBox = $versionCombo
    
    # Botão de desinstalação
    $uninstallButton = New-Object System.Windows.Forms.Button
    $uninstallButton.Location = New-Object System.Drawing.Point(350, 160)
    $uninstallButton.Size = New-Object System.Drawing.Size(130, 30)
    $uninstallButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $uninstallButton.BackColor = [System.Drawing.Color]::Transparent
    $uninstallButton.Cursor = [System.Windows.Forms.Cursors]::Hand

    # Ícone Segoe MDL2 Assets (Delete: U+E74D)
    $uninstallIcon = New-Object System.Windows.Forms.Label
    $uninstallIcon.Text = [char]0xE74D
    $uninstallIcon.Font = $script:iconFont
    $uninstallIcon.AutoSize = $true
    $uninstallIcon.Location = New-Object System.Drawing.Point(10, 3)
    $uninstallIcon.Size = New-Object System.Drawing.Size(24, 24)
    $uninstallIcon.TextAlign = 'MiddleLeft'
    $uninstallIcon.ForeColor = $ModernDarkTheme.ButtonBackColor
    $uninstallIcon.BackColor = [System.Drawing.Color]::Transparent

    $uninstallText = New-Object System.Windows.Forms.Label
    $uninstallText.Text = "Desinstalar"
    $uninstallText.Font = $script:modernFont
    $uninstallText.AutoSize = $true
    $uninstallText.Location = New-Object System.Drawing.Point(38, 4)
    $uninstallText.Size = New-Object System.Drawing.Size(90, 20)
    $uninstallText.TextAlign = 'MiddleLeft'
    $uninstallText.ForeColor = $ModernDarkTheme.ButtonBackColor
    $uninstallText.BackColor = [System.Drawing.Color]::Transparent

    $uninstallButton.Controls.Add($uninstallIcon)
    $uninstallButton.Controls.Add($uninstallText)
    $uninstallClick = {
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
    }
    $uninstallButton.Add_Click($uninstallClick)
    $uninstallIcon.Add_Click($uninstallClick)
    $uninstallText.Add_Click($uninstallClick)

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
    
    # DataGridView para listar serviços
    $dataGrid = New-Object System.Windows.Forms.DataGridView
    $dataGrid.Location = New-Object System.Drawing.Point(240, 100)
    $dataGrid.Size = New-Object System.Drawing.Size(730, 300)
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
    # Ícones Segoe MDL2 Assets
    $iconStart      = [char]0xE768 # Play
    $iconStop       = [char]0xE71A # Stop
    $iconRestart    = [char]0xE895 # Refresh/Restart
    $iconRefresh    = [char]0xE72C # Refresh
    $iconStartAll   = [char]0xE768 # Play
    $iconStopAll    = [char]0xE71A # Stop
    $iconRestartAll = [char]0xE895 # Refresh/Restart

    function Add-IconButton {
        param(
            [System.Windows.Forms.Button]$button,
            [ScriptBlock]$onClick,
            [string]$icon,
            [string]$text
        )
        $iconLabel = New-Object System.Windows.Forms.Label
        $iconLabel.Text = $icon
        $iconLabel.Font = $script:iconFont
        $iconLabel.AutoSize = $true
        $iconLabel.Location = New-Object System.Drawing.Point(10, 3)
        $iconLabel.Size = New-Object System.Drawing.Size(24, 24)
        $iconLabel.TextAlign = 'MiddleLeft'
        $iconLabel.ForeColor = $ModernDarkTheme.ButtonBackColor
        $iconLabel.BackColor = [System.Drawing.Color]::Transparent

        $textLabel = New-Object System.Windows.Forms.Label
        $textLabel.Text = $text
        $textLabel.Font = $script:modernFont
        $textLabel.AutoSize = $true
        $textLabel.Location = New-Object System.Drawing.Point(38, 4)
        $textLabel.Size = New-Object System.Drawing.Size(90, 20)
        $textLabel.TextAlign = 'MiddleLeft'
        $textLabel.ForeColor = $ModernDarkTheme.ButtonBackColor
        $textLabel.BackColor = [System.Drawing.Color]::Transparent

        $iconLabel.Add_Click($onClick)
        $textLabel.Add_Click($onClick)
        $button.Controls.Add($iconLabel)
        $button.Controls.Add($textLabel)
    }

    $startButton = New-Object System.Windows.Forms.Button
    $startButton.Location = New-Object System.Drawing.Point(240, 410)
    $startButton.Size = New-Object System.Drawing.Size(150, 30)
    $startButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $startButton.BackColor = [System.Drawing.Color]::Transparent
    $startButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $startClick = {
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
    }
    Add-IconButton $startButton $startClick $iconStart "Iniciar"
    $startButton.Add_Click($startClick)
    $tabPage.Controls.Add($startButton)

    $stopButton = New-Object System.Windows.Forms.Button
    $stopButton.Location = New-Object System.Drawing.Point(400, 410)
    $stopButton.Size = New-Object System.Drawing.Size(150, 30)
    $stopButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $stopButton.BackColor = [System.Drawing.Color]::Transparent
    $stopButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $stopClick = {
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
    }
    Add-IconButton $stopButton $stopClick $iconStop "Parar"
    $stopButton.Add_Click($stopClick)
    $tabPage.Controls.Add($stopButton)

    $restartButton = New-Object System.Windows.Forms.Button
    $restartButton.Location = New-Object System.Drawing.Point(560, 410)
    $restartButton.Size = New-Object System.Drawing.Size(150, 30)
    $restartButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $restartButton.BackColor = [System.Drawing.Color]::Transparent
    $restartButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $restartClick = {
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
    }
    Add-IconButton $restartButton $restartClick $iconRestart "Reiniciar"
    $restartButton.Add_Click($restartClick)
    $tabPage.Controls.Add($restartButton)

    $refreshButton = New-Object System.Windows.Forms.Button
    $refreshButton.Location = New-Object System.Drawing.Point(720, 410)
    $refreshButton.Size = New-Object System.Drawing.Size(150, 30)
    $refreshButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $refreshButton.BackColor = [System.Drawing.Color]::Transparent
    $refreshButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $refreshClick = {
        Update-StatusMessage "Atualizando lista de serviços..."
        Update-ServicesList
        Update-StatusMessage "Lista de serviços atualizada."
    }
    Add-IconButton $refreshButton $refreshClick $iconRefresh "Atualizar"
    $refreshButton.Add_Click($refreshClick)
    $tabPage.Controls.Add($refreshButton)

    # Adicionar botões para controlar todos os serviços de uma vez
    $startAllButton = New-Object System.Windows.Forms.Button
    $startAllButton.Location = New-Object System.Drawing.Point(240, 450)
    $startAllButton.Size = New-Object System.Drawing.Size(150, 30)
    $startAllButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $startAllButton.BackColor = [System.Drawing.Color]::Transparent
    $startAllButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $startAllClick = {
        Update-StatusMessage "Iniciando todos os serviços..."

        try {
            # Executar o comando start --all
            & "$PSScriptRoot\..\setup.ps1" start --all
            Update-StatusMessage "Todos os serviços foram iniciados com sucesso."
            Update-ServicesList
        }
        catch {
            $errorMsg = $_
            Update-StatusMessage "Erro ao iniciar todos os serviços: $errorMsg"
            [System.Windows.Forms.MessageBox]::Show("Erro ao iniciar todos os serviços: $errorMsg", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    }
    Add-IconButton $startAllButton $startAllClick $iconStartAll "Iniciar Todos"
    $startAllButton.Add_Click($startAllClick)
    $tabPage.Controls.Add($startAllButton)

    $stopAllButton = New-Object System.Windows.Forms.Button
    $stopAllButton.Location = New-Object System.Drawing.Point(400, 450)
    $stopAllButton.Size = New-Object System.Drawing.Size(150, 30)
    $stopAllButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $stopAllButton.BackColor = [System.Drawing.Color]::Transparent
    $stopAllButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $stopAllClick = {
        Update-StatusMessage "Parando todos os serviços..."

        try {
            # Executar o comando stop --all
            & "$PSScriptRoot\..\setup.ps1" stop --all
            Update-StatusMessage "Todos os serviços foram parados com sucesso."
            Update-ServicesList
        }
        catch {
            $errorMsg = $_
            Update-StatusMessage "Erro ao parar todos os serviços: $errorMsg"
            [System.Windows.Forms.MessageBox]::Show("Erro ao parar todos os serviços: $errorMsg", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    }
    Add-IconButton $stopAllButton $stopAllClick $iconStopAll "Parar Todos"
    $stopAllButton.Add_Click($stopAllClick)
    $tabPage.Controls.Add($stopAllButton)

    $restartAllButton = New-Object System.Windows.Forms.Button
    $restartAllButton.Location = New-Object System.Drawing.Point(560, 450)
    $restartAllButton.Size = New-Object System.Drawing.Size(150, 30)
    $restartAllButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $restartAllButton.BackColor = [System.Drawing.Color]::Transparent
    $restartAllButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $restartAllClick = {
        Update-StatusMessage "Reiniciando todos os serviços..."

        try {
            # Executar o comando restart --all
            & "$PSScriptRoot\..\setup.ps1" restart --all
            Update-StatusMessage "Todos os serviços foram reiniciados com sucesso."
            Update-ServicesList
        }
        catch {
            $errorMsg = $_
            Update-StatusMessage "Erro ao reiniciar todos os serviços: $errorMsg"
            [System.Windows.Forms.MessageBox]::Show("Erro ao reiniciar todos os serviços: $errorMsg", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
        }
    }
    Add-IconButton $restartAllButton $restartAllClick $iconRestartAll "Reiniciar Todos"
    $restartAllButton.Add_Click($restartAllClick)
    $tabPage.Controls.Add($restartAllButton)
    
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
    $pathLabel.Location = New-Object System.Drawing.Point(240, 80)
    $pathLabel.Size = New-Object System.Drawing.Size(882, 23)
    $pathLabel.Text = "Gerenciamento de PATH"
    $tabPage.Controls.Add($pathLabel)
    
    # Botão para adicionar ao PATH
    $addPathButton = New-Object System.Windows.Forms.Button
    $addPathButton.Location = New-Object System.Drawing.Point(240, 110)
    $addPathButton.Size = New-Object System.Drawing.Size(200, 30)
    $addPathButton.Text = "Adicionar DevStack ao PATH"
    $addPathButton.Cursor = [System.Windows.Forms.Cursors]::Hand
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
    $globalButton.Location = New-Object System.Drawing.Point(450, 110)
    $globalButton.Size = New-Object System.Drawing.Size(200, 30)
    $globalButton.Text = "Configuração Global"
    $globalButton.Cursor = [System.Windows.Forms.Cursors]::Hand
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
    $doctorButton.Location = New-Object System.Drawing.Point(240, 160)
    $doctorButton.Size = New-Object System.Drawing.Size(200, 30)
    $doctorButton.Text = "Diagnóstico do Sistema"
    $doctorButton.Cursor = [System.Windows.Forms.Cursors]::Hand
    $doctorButton.Add_Click({
        Update-StatusMessage "Executando diagnóstico do sistema..."
        
        try {
            $diagForm = New-Object System.Windows.Forms.Form
            $diagForm.Text = "DevStack - Diagnóstico do Sistema"
            $diagForm.Size = New-Object System.Drawing.Size(800, 600)
            $diagForm.StartPosition = "CenterScreen"
            
            $outputBox = New-Object System.Windows.Forms.RichTextBox
            $outputBox.Location = New-Object System.Drawing.Point(10, 10)
            $outputBox.Size = New-Object System.Drawing.Size(765, 540)
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
    $depsButton.Location = New-Object System.Drawing.Point(450, 160)
    $depsButton.Size = New-Object System.Drawing.Size(200, 30)
    $depsButton.Text = "Verificar Dependências"
    $depsButton.Cursor = [System.Windows.Forms.Cursors]::Hand
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
    $cleanButton.Location = New-Object System.Drawing.Point(240, 210)
    $cleanButton.Size = New-Object System.Drawing.Size(200, 30)
    $cleanButton.Text = "Limpar Logs/Temp"
    $cleanButton.Cursor = [System.Windows.Forms.Cursors]::Hand
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
    $backupButton.Location = New-Object System.Drawing.Point(450, 210)
    $backupButton.Size = New-Object System.Drawing.Size(200, 30)
    $backupButton.Text = "Criar Backup"
    $backupButton.Cursor = [System.Windows.Forms.Cursors]::Hand
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
    $updateButton.Location = New-Object System.Drawing.Point(240, 260)
    $updateButton.Size = New-Object System.Drawing.Size(200, 30)
    $updateButton.Text = "Atualizar DevStack"
    $updateButton.Cursor = [System.Windows.Forms.Cursors]::Hand
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
    $siteLabel.Location = New-Object System.Drawing.Point(240, 80)
    $siteLabel.Size = New-Object System.Drawing.Size(300, 23)
    $siteLabel.Text = "Criar configuração de site"
    $tabPage.Controls.Add($siteLabel)
    
    # Campo de domínio
    $domainLabel = New-Object System.Windows.Forms.Label
    $domainLabel.Location = New-Object System.Drawing.Point(240, 110)
    $domainLabel.Size = New-Object System.Drawing.Size(200, 23)
    $domainLabel.Text = "Domínio:"
    $tabPage.Controls.Add($domainLabel)
    
    $domainTextBox = New-Object System.Windows.Forms.TextBox
    $domainTextBox.Location = New-Object System.Drawing.Point(450, 110)
    $domainTextBox.Size = New-Object System.Drawing.Size(200, 23)
    $tabPage.Controls.Add($domainTextBox)
    
    # Campo de diretório raiz
    $rootLabel = New-Object System.Windows.Forms.Label
    $rootLabel.Location = New-Object System.Drawing.Point(240, 140)
    $rootLabel.Size = New-Object System.Drawing.Size(200, 23)
    $rootLabel.Text = "Diretório Raiz:"
    $tabPage.Controls.Add($rootLabel)
    
    $rootTextBox = New-Object System.Windows.Forms.TextBox
    $rootTextBox.Location = New-Object System.Drawing.Point(450, 140)
    $rootTextBox.Size = New-Object System.Drawing.Size(200, 23)
    $tabPage.Controls.Add($rootTextBox)
    # Torna rootTextBox acessível no evento
    $script:siteRootTextBox = $rootTextBox

    $browseButton = New-Object System.Windows.Forms.Button
    $browseButton.Location = New-Object System.Drawing.Point(650, 140)
    $browseButton.Size = New-Object System.Drawing.Size(120, 24)
    $browseButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $browseButton.BackColor = [System.Drawing.Color]::Transparent
    $browseButton.Cursor = [System.Windows.Forms.Cursors]::Hand

    $browseIcon = New-Object System.Windows.Forms.Label
    $browseIcon.Text = [char]0xED25
    $browseIcon.Font = $script:iconFont
    $browseIcon.AutoSize = $true
    $browseIcon.Location = New-Object System.Drawing.Point(10, 0)
    $browseIcon.Size = New-Object System.Drawing.Size(24, 24)
    $browseIcon.TextAlign = 'MiddleLeft'
    $browseIcon.ForeColor = $ModernDarkTheme.ButtonBackColor
    $browseIcon.BackColor = [System.Drawing.Color]::Transparent

    $browseText = New-Object System.Windows.Forms.Label
    $browseText.Text = "Procurar"
    $browseText.Font = $script:modernFont
    $browseText.AutoSize = $true
    $browseText.Location = New-Object System.Drawing.Point(38, 1)
    $browseText.Size = New-Object System.Drawing.Size(70, 20)
    $browseText.TextAlign = 'MiddleLeft'
    $browseText.ForeColor = $ModernDarkTheme.ButtonBackColor
    $browseText.BackColor = [System.Drawing.Color]::Transparent

    $browseButton.Controls.Add($browseIcon)
    $browseButton.Controls.Add($browseText)
    $browseClick = {
        $folderBrowser = New-Object System.Windows.Forms.FolderBrowserDialog
        $folderBrowser.Description = "Selecione o diretório raiz do site"
        $folderBrowser.RootFolder = [System.Environment+SpecialFolder]::MyComputer
        
        if ($folderBrowser.ShowDialog() -eq [System.Windows.Forms.DialogResult]::OK) {
            $script:siteRootTextBox.Text = $folderBrowser.SelectedPath
        }
    }
    $browseButton.Add_Click($browseClick)
    $browseIcon.Add_Click($browseClick)
    $browseText.Add_Click($browseClick)
    $tabPage.Controls.Add($browseButton)
    
    # Campo de PHP Upstream
    $phpLabel = New-Object System.Windows.Forms.Label
    $phpLabel.Location = New-Object System.Drawing.Point(240, 170)
    $phpLabel.Size = New-Object System.Drawing.Size(200, 23)
    $phpLabel.Text = "PHP Upstream:"
    $tabPage.Controls.Add($phpLabel)
    
    $phpCombo = New-Object System.Windows.Forms.ComboBox
    $phpCombo.Location = New-Object System.Drawing.Point(450, 170)
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
    $nginxLabel.Location = New-Object System.Drawing.Point(240, 200)
    $nginxLabel.Size = New-Object System.Drawing.Size(200, 23)
    $nginxLabel.Text = "Nginx Versão:"
    $tabPage.Controls.Add($nginxLabel)
    
    $nginxCombo = New-Object System.Windows.Forms.ComboBox
    $nginxCombo.Location = New-Object System.Drawing.Point(450, 200)
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
    $createSiteButton.Location = New-Object System.Drawing.Point(450, 240)
    $createSiteButton.Size = New-Object System.Drawing.Size(180, 30)
    $createSiteButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $createSiteButton.BackColor = [System.Drawing.Color]::Transparent
    $createSiteButton.Cursor = [System.Windows.Forms.Cursors]::Hand

    # Ícone Segoe MDL2 Assets (Add: U+E710)
    $siteIcon = New-Object System.Windows.Forms.Label
    $siteIcon.Text = [char]0xE710
    $siteIcon.Font = $script:iconFont
    $siteIcon.AutoSize = $true
    $siteIcon.Location = New-Object System.Drawing.Point(10, 3)
    $siteIcon.Size = New-Object System.Drawing.Size(24, 24)
    $siteIcon.TextAlign = 'MiddleLeft'
    $siteIcon.ForeColor = $ModernDarkTheme.ButtonBackColor
    $siteIcon.BackColor = [System.Drawing.Color]::Transparent

    $siteText = New-Object System.Windows.Forms.Label
    $siteText.Text = "Criar Configuração"
    $siteText.Font = $script:modernFont
    $siteText.AutoSize = $true
    $siteText.Location = New-Object System.Drawing.Point(38, 4)
    $siteText.Size = New-Object System.Drawing.Size(120, 20)
    $siteText.TextAlign = 'MiddleLeft'
    $siteText.ForeColor = $ModernDarkTheme.ButtonBackColor
    $siteText.BackColor = [System.Drawing.Color]::Transparent

    $createSiteButton.Controls.Add($siteIcon)
    $createSiteButton.Controls.Add($siteText)
    $createSiteClick = {
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
    }
    $createSiteButton.Add_Click($createSiteClick)
    $siteIcon.Add_Click($createSiteClick)
    $siteText.Add_Click($createSiteClick)

    $tabPage.Controls.Add($createSiteButton)
    
    # Botão para gerar certificado SSL
    $sslLabel = New-Object System.Windows.Forms.Label
    $sslLabel.Location = New-Object System.Drawing.Point(240, 290)
    $sslLabel.Size = New-Object System.Drawing.Size(200, 23)
    $sslLabel.Text = "Gerar certificado SSL"
    $tabPage.Controls.Add($sslLabel)
    
    $sslDomainLabel = New-Object System.Windows.Forms.Label
    $sslDomainLabel.Location = New-Object System.Drawing.Point(240, 320)
    $sslDomainLabel.Size = New-Object System.Drawing.Size(200, 23)
    $sslDomainLabel.Text = "Domínio SSL:"
    $tabPage.Controls.Add($sslDomainLabel)
    
    $sslDomainTextBox = New-Object System.Windows.Forms.TextBox
    $sslDomainTextBox.Location = New-Object System.Drawing.Point(450, 320)
    $sslDomainTextBox.Size = New-Object System.Drawing.Size(200, 23)
    $tabPage.Controls.Add($sslDomainTextBox)
    
    $generateSslButton = New-Object System.Windows.Forms.Button
    $generateSslButton.Location = New-Object System.Drawing.Point(450, 350)
    $generateSslButton.Size = New-Object System.Drawing.Size(180, 30)
    $generateSslButton.FlatStyle = [System.Windows.Forms.FlatStyle]::Flat
    $generateSslButton.BackColor = [System.Drawing.Color]::Transparent
    $generateSslButton.Cursor = [System.Windows.Forms.Cursors]::Hand

    # Ícone Segoe MDL2 Assets (Add: U+E710)
    $sslIcon = New-Object System.Windows.Forms.Label
    $sslIcon.Text = [char]0xE710
    $sslIcon.Font = $script:iconFont
    $sslIcon.AutoSize = $true
    $sslIcon.Location = New-Object System.Drawing.Point(10, 3)
    $sslIcon.Size = New-Object System.Drawing.Size(24, 24)
    $sslIcon.TextAlign = 'MiddleLeft'
    $sslIcon.ForeColor = $ModernDarkTheme.ButtonBackColor
    $sslIcon.BackColor = [System.Drawing.Color]::Transparent

    $sslText = New-Object System.Windows.Forms.Label
    $sslText.Text = "Gerar Certificado SSL"
    $sslText.Font = $script:modernFont
    $sslText.AutoSize = $true
    $sslText.Location = New-Object System.Drawing.Point(38, 4)
    $sslText.Size = New-Object System.Drawing.Size(120, 20)
    $sslText.TextAlign = 'MiddleLeft'
    $sslText.ForeColor = $ModernDarkTheme.ButtonBackColor
    $sslText.BackColor = [System.Drawing.Color]::Transparent

    $generateSslButton.Controls.Add($sslIcon)
    $generateSslButton.Controls.Add($sslText)
    $generateSslClick = {
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
    }
    $generateSslButton.Add_Click($generateSslClick)
    $sslIcon.Add_Click($generateSslClick)
    $sslText.Add_Click($generateSslClick)

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
    $consoleLabel.Location = New-Object System.Drawing.Point(240, 70)
    $consoleLabel.Size = New-Object System.Drawing.Size(600, 23)
    $consoleLabel.Text = "Console DevStack"
    $tabPage.Controls.Add($consoleLabel)
    
    $commandLabel = New-Object System.Windows.Forms.Label
    $commandLabel.Location = New-Object System.Drawing.Point(240, 103)
    $commandLabel.Size = New-Object System.Drawing.Size(75, 23)
    $commandLabel.Text = "Comando:"
    $tabPage.Controls.Add($commandLabel)
    
    # Adicionar variável global para o campo de texto de comando
    $commandTextBox = New-Object System.Windows.Forms.TextBox
    $commandTextBox.Location = New-Object System.Drawing.Point(325, 100)
    $commandTextBox.Size = New-Object System.Drawing.Size(550, 23)
    $tabPage.Controls.Add($commandTextBox)
    
    # Atribuir à variável global
    $script:utilsCommandTextBox = $commandTextBox
    # Criar a caixa de saída primeiro
    $outputBox = New-Object System.Windows.Forms.RichTextBox
    $outputBox.Location = New-Object System.Drawing.Point(240, 130)
    $outputBox.Size = New-Object System.Drawing.Size(730, 420)
    $outputBox.ReadOnly = $true
    $outputBox.BackColor = [System.Drawing.Color]::Black
    $outputBox.ForeColor = [System.Drawing.Color]::White
    # Usar uma fonte monospace com bom suporte a UTF-8
    $outputBox.Font = New-Object System.Drawing.Font("Consolas", 10)
    $outputBox.Multiline = $true
    $outputBox.AcceptsTab = $true
    $outputBox.WordWrap = $true
    # Configurar RichEdit para suporte completo a caracteres internacionais
    $outputBox.Text = ""
    # Definir a propriedade RightToLeft como No para garantir exibição correta da esquerda para a direita
    $outputBox.RightToLeft = [System.Windows.Forms.RightToLeft]::No
    $tabPage.Controls.Add($outputBox)
    
    # Atribuir a caixa de texto de saída à variável global
    $script:utilsOutputBox = $outputBox
    
    # Adicionar sugestões de comandos populares ao iniciar
    try {
        # Definir o título e informações iniciais com suporte a UTF-8
        Add-UTF8Text -textBox $script:utilsOutputBox -text "DevStackSetup GUI Console`r`n" -color ([System.Drawing.Color]::Cyan)
        Add-UTF8Text -textBox $script:utilsOutputBox -text "-------------------`r`n"
        Add-UTF8Text -textBox $script:utilsOutputBox -text "Comandos populares:`r`n"
        Add-UTF8Text -textBox $script:utilsOutputBox -text "  * status`r`n"
        Add-UTF8Text -textBox $script:utilsOutputBox -text "  * list --installed`r`n"
        Add-UTF8Text -textBox $script:utilsOutputBox -text "  * list <componente>`r`n"
        Add-UTF8Text -textBox $script:utilsOutputBox -text "  * test`r`n"
        Add-UTF8Text -textBox $script:utilsOutputBox -text "  * doctor`r`n"
        Add-UTF8Text -textBox $script:utilsOutputBox -text "-------------------`r`n"
    } catch {
        [System.Windows.Forms.MessageBox]::Show("Erro ao inicializar console: $_", "Erro", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
    }
    # Criar o botão de execução
    $executeButton = New-Object System.Windows.Forms.Button
    $executeButton.Location = New-Object System.Drawing.Point(880, 99)
    $executeButton.Size = New-Object System.Drawing.Size(90, 25)
    $executeButton.Text = "Executar"
    $executeButton.Cursor = [System.Windows.Forms.Cursors]::Hand
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
            # Colorir o prompt em azul usando a função Add-UTF8Text
            Add-UTF8Text -textBox $outputBox -text "> $command`r`n" -color ([System.Drawing.Color]::Cyan)
            
            try {
                # Path para o script setup.ps1
                $setupPath = "$PSScriptRoot\..\setup.ps1"
                # Preparar para capturar a saída
                $pinfo = New-Object System.Diagnostics.ProcessStartInfo
                $pinfo.FileName = "pwsh.exe"
                # Tentar usar PowerShell 7 primeiro, que tem melhor suporte a Unicode
                if (-not (Get-Command "pwsh.exe" -ErrorAction SilentlyContinue)) {
                    # Fallback para PowerShell Windows padrão se o Core não estiver disponível
                    $pinfo.FileName = "powershell.exe"
                }
                
                # Configurar argumentos do PowerShell com encoding explícito
                $pinfo.Arguments = "-NoProfile -ExecutionPolicy Bypass -Command `"& { `$OutputEncoding = [System.Text.Encoding]::UTF8; [Console]::OutputEncoding = [System.Text.Encoding]::UTF8; . '$setupPath' $command }`""
                $pinfo.RedirectStandardOutput = $true
                $pinfo.RedirectStandardError = $true
                $pinfo.UseShellExecute = $false
                $pinfo.CreateNoWindow = $true
                $pinfo.StandardOutputEncoding = [System.Text.Encoding]::UTF8
                $pinfo.StandardErrorEncoding = [System.Text.Encoding]::UTF8
                
                $process = New-Object System.Diagnostics.Process
                $process.StartInfo = $pinfo
                $process.Start() | Out-Null
                # Capturar saída padrão e de erro diretamente com StreamReader para melhor controle de encoding
                $stdoutReader = $process.StandardOutput
                $stderrReader = $process.StandardError
                
                # Ler a saída linha por linha, preservando o encoding
                $stdoutBuilder = New-Object System.Text.StringBuilder
                $stderrBuilder = New-Object System.Text.StringBuilder
                
                while (!$stdoutReader.EndOfStream) {
                    $line = $stdoutReader.ReadLine()
                    [void]$stdoutBuilder.AppendLine($line)
                }
                
                while (!$stderrReader.EndOfStream) {
                    $line = $stderrReader.ReadLine()
                    [void]$stderrBuilder.AppendLine($line)
                }
                
                $process.WaitForExit()
                
                $stdout = $stdoutBuilder.ToString()
                $stderr = $stderrBuilder.ToString()
                # Exibir os resultados
                if ($stdout) {
                    # Usar a função Add-UTF8Text para garantir que o texto seja exibido corretamente
                    Add-UTF8Text -textBox $outputBox -text $stdout -color ([System.Drawing.Color]::White)
                }
                
                if ($stderr) {
                    # Usar a função Add-UTF8Text para o texto de erro com cor vermelha
                    Add-UTF8Text -textBox $outputBox -text "ERROS:`r`n" -color ([System.Drawing.Color]::Red)
                    Add-UTF8Text -textBox $outputBox -text $stderr -color ([System.Drawing.Color]::Red)
                }
                if (-not $stdout -and -not $stderr) {
                    Add-UTF8Text -textBox $outputBox -text "(Comando executado, sem saída gerada)`r`n" -color ([System.Drawing.Color]::Gray)
                }
                
                Add-UTF8Text -textBox $outputBox -text "`r`n" -color ([System.Drawing.Color]::White)
                Update-StatusMessage "Comando executado com sucesso (código de saída: $($process.ExitCode))."
                
                # Limpar a caixa de comando para facilitar a entrada do próximo comando
                $script:utilsCommandTextBox.Text = ""
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
        # Adicionar uma mensagem indicando que os dados foram atualizados usando nossa função UTF-8
        Add-UTF8Text -textBox $script:utilsOutputBox -text "[$(Get-Date -Format 'HH:mm:ss')] Dados do sistema atualizados.`r`n" -color ([System.Drawing.Color]::Gray)
    }
    
    # Outras atualizações podem ser adicionadas aqui conforme necessário
    Update-StatusMessage "Todas as informações foram atualizadas."
}

# Função auxiliar para adicionar texto UTF-8 ao RichTextBox
function Add-UTF8Text {
    param(
        [System.Windows.Forms.RichTextBox]$textBox,
        [string]$text,
        [System.Drawing.Color]$color = [System.Drawing.Color]::White
    )
    
    if (-not $textBox -or [string]::IsNullOrEmpty($text)) {
        return
    }
    # Salvar a posição atual do cursor e a cor de seleção
    $currentPosition = $textBox.SelectionStart
    $currentColor = $textBox.SelectionColor
    
    # Garantir que estamos no final do texto antes de adicionar
    $textBox.SelectionStart = $textBox.TextLength
    
    # Definir a cor do texto a ser adicionado
    $textBox.SelectionColor = $color
    
    try {
        # Substituições diretas para caracteres problemáticos comuns em português
        $fixedText = $text
        
        # Aplicar texto ao RichTextBox
        $textBox.AppendText($fixedText)
        
        # Rolar para a última linha adicionada
        $textBox.ScrollToCaret()
    }
    catch {
        # Se falhar, tente com o texto original diretamente
        $textBox.AppendText($text)
        $textBox.ScrollToCaret()
    }
    
    # Restaurar a cor original
    $textBox.SelectionColor = $currentColor
}
