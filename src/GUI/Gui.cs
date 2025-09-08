using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DevStackManager
{
    /// <summary>
    /// Interface gráfica moderna WPF para o DevStackManager
    /// Convertida do arquivo gui.ps1 original mantendo funcionalidades e layout
    /// </summary>
    public partial class DevStackGui : Window, INotifyPropertyChanged
    {
        #region Private Fields
        private readonly DevStackShared.LocalizationManager _localizationManager;
        private readonly System.Timers.Timer _servicesUpdateTimer;
        private readonly Dictionary<string, bool> _serviceStatusCache = new(); // Cache para status dos serviços
        private DateTime _lastServicesCacheUpdate = DateTime.MinValue;
        private string _statusMessage = "";
        private ObservableCollection<ComponentViewModel> _installedComponents = new();
        private ObservableCollection<string> _availableComponents = new();
        private ObservableCollection<string> _availableVersions = new();
        private ObservableCollection<ServiceViewModel> _services = new();
        private ObservableCollection<string> _shortcutComponents = new();
        private ObservableCollection<string> _shortcutVersions = new();
        private string _selectedComponent = "";
        private string _selectedVersion = "";
        private string _selectedUninstallComponent = "";
        private string _selectedUninstallVersion = "";
        private string _selectedShortcutComponent = "";
        private string _selectedShortcutVersion = "";
        private string _consoleOutput = "";
        private bool _isInstallingComponent = false;
        private bool _isUninstallingComponent = false;
        private bool _isLoadingServices = false;
        private bool _isCreatingSite = false;
        public bool IsInstallingComponent
        {
            get => _isInstallingComponent;
            set 
            { 
                var oldValue = _isInstallingComponent;
                _isInstallingComponent = value; 
                OnPropertyChanged();
                
                // Se mudou de true para false (instalação concluída), recarregar dados
                if (oldValue && !value)
                {
                    _ = Task.Run(async () =>
                    {
                        await LoadInstalledComponents();
                        await LoadShortcutComponents();
                    });
                }
            }
        }
        public bool IsUninstallingComponent
        {
            get => _isUninstallingComponent;
            set 
            { 
                var oldValue = _isUninstallingComponent;
                _isUninstallingComponent = value; 
                OnPropertyChanged();
                
                // Se mudou de true para false (desinstalação concluída), recarregar dados
                if (oldValue && !value)
                {
                    _ = Task.Run(async () =>
                    {
                        await LoadInstalledComponents();
                        await LoadServices();
                        await LoadShortcutComponents();
                    });
                }
            }
        }
        public bool IsLoadingServices
        {
            get => _isLoadingServices;
            set { _isLoadingServices = value; OnPropertyChanged(); }
        }
        public bool IsCreatingSite
        {
            get => _isCreatingSite;
            set { _isCreatingSite = value; OnPropertyChanged(); }
        }
        public ContentControl? _mainContent;
        private int _selectedNavIndex = 0;
        public DevStackShared.ThemeManager.ThemeColors CurrentTheme => DevStackShared.ThemeManager.CurrentTheme;
        #endregion

        #region Properties
        public DevStackShared.LocalizationManager LocalizationManager => _localizationManager;
        
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ComponentViewModel> InstalledComponents
        {
            get => _installedComponents;
            set { _installedComponents = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableComponents
        {
            get => _availableComponents;
            set { _availableComponents = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AvailableVersions
        {
            get => _availableVersions;
            set { _availableVersions = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ServiceViewModel> Services
        {
            get => _services;
            set { _services = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ShortcutComponents
        {
            get => _shortcutComponents;
            set { _shortcutComponents = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ShortcutVersions
        {
            get => _shortcutVersions;
            set { _shortcutVersions = value; OnPropertyChanged(); }
        }

        public string SelectedComponent
        {
            get => _selectedComponent;
            set { _selectedComponent = value; OnPropertyChanged(); _ = LoadVersionsForComponent(); }
        }

        public string SelectedVersion
        {
            get => _selectedVersion;
            set { _selectedVersion = value; OnPropertyChanged(); }
        }

        public string SelectedUninstallComponent
        {
            get => _selectedUninstallComponent;
            set { _selectedUninstallComponent = value; OnPropertyChanged(); _ = LoadUninstallVersions(); }
        }

        public string SelectedUninstallVersion
        {
            get => _selectedUninstallVersion;
            set { _selectedUninstallVersion = value; OnPropertyChanged(); }
        }

        public string SelectedShortcutComponent
        {
            get => _selectedShortcutComponent;
            set { _selectedShortcutComponent = value; OnPropertyChanged(); _ = LoadShortcutVersions(); }
        }

        public string SelectedShortcutVersion
        {
            get => _selectedShortcutVersion;
            set { _selectedShortcutVersion = value; OnPropertyChanged(); }
        }

        public string ConsoleOutput
        {
            get => _consoleOutput;
            set { _consoleOutput = value; OnPropertyChanged(); }
        }
        
        public int SelectedNavIndex
        {
            get => _selectedNavIndex;
            set { 
                _selectedNavIndex = value; 
                OnPropertyChanged();

                GuiNavigation.NavigateToSection(this, value);
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Constructor
        public DevStackGui()
        {
            // Initialize localization for GUI (carregando idioma persistido se existir)
            _localizationManager = DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.GUI);
            try
            {
                var settings = DevStackConfig.GetSetting(new string[] { "language", "theme" }) as System.Collections.Generic.Dictionary<string, object?>;
                var lang = settings != null && settings.TryGetValue("language", out var l) ? l?.ToString() : null;
                var theme = settings != null && settings.TryGetValue("theme", out var t) ? t?.ToString() : null;
                if (!string.IsNullOrWhiteSpace(lang))
                    DevStackShared.LocalizationManager.ApplyLanguage(lang);
                if (!string.IsNullOrWhiteSpace(theme))
                    DevStackShared.ThemeManager.ApplyTheme(theme.Equals("light", StringComparison.OrdinalIgnoreCase) ? DevStackShared.ThemeManager.ThemeType.Light : DevStackShared.ThemeManager.ThemeType.Dark);
            }
            catch { }
            _statusMessage = _localizationManager.GetString("gui.window.ready_status");
            _localizationManager.LanguageChanged += OnLanguageChanged;

            // Listener para troca de idioma em tempo real (similar ao tema)
            DevStackShared.LocalizationManager.OnLanguageChangedStatic += OnLanguageChangedStatic;

            // Listener para troca de tema em tempo real
            DevStackShared.ThemeManager.OnThemeChanged += OnThemeChanged;

            // Inicializar timer para atualização periódica dos serviços (mais frequente para detectar mudanças)
            _servicesUpdateTimer = new System.Timers.Timer(5000); // 5 segundos para detectar rapidamente mudanças de status
            _servicesUpdateTimer.Elapsed += async (sender, e) => await LoadServices(false);
            _servicesUpdateTimer.AutoReset = true;
            _servicesUpdateTimer.Start();

            // Inicializar o ProcessRegistry (carregar dados salvos)
            ProcessRegistry.LoadFromFile();

            InitializeComponent();
            DataContext = this;

            // Carregar dados iniciais após a janela estar pronta
            this.Loaded += (s, e) => 
            {
                // Se estamos no Dashboard (índice 0), carregar dados
                if (SelectedNavIndex == 0)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await LoadInstalledComponents();
                            await LoadServices();
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                StatusMessage = $"Erro ao carregar dados iniciais: {ex.Message}";
                            });
                        }
                    });
                }
            };
        }
        #endregion

        #region Initialization Methods
        private void InitializeComponent()
        {
            var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStackGUI.exe");
            var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? _localizationManager.GetString("common.unknown");
            Title = _localizationManager.GetString("gui.window.title", version);
            Width = 1200;
            Height = 840;
            MinWidth = 1200;
            MinHeight = 840;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = CurrentTheme.FormBackground;
            Foreground = CurrentTheme.Foreground;

            // Grid principal
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Conteúdo principal
            CreateMainContent(mainGrid);

            // Barra de status
            CreateStatusBar(mainGrid);

            Content = mainGrid;
        }

        private void CreateMainContent(Grid mainGrid)
        {
            GuiMainContent.CreateMainContent(mainGrid, this);
        }

        private void CreateStatusBar(Grid mainGrid)
        {
            GuiStatusBar.CreateStatusBar(mainGrid, this);
        }
        
        private void OnLanguageChanged(object? sender, string newLang)
        {
            // Rebuild main window texts and content to reflect new language
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Language changed to: {newLang}");
                    
                    var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStackGUI.exe");
                    var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? _localizationManager.GetString("common.unknown");
                    Title = _localizationManager.GetString("gui.window.title", version);

                    // Recreate main layout while preserving selected index
                    int currentIndex = SelectedNavIndex;
                    var mainGrid = Content as Grid;
                    if (mainGrid != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Clearing and recreating main grid content");
                        
                        mainGrid.Children.Clear();
                        mainGrid.RowDefinitions.Clear();
                        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        CreateMainContent(mainGrid);
                        CreateStatusBar(mainGrid);
                        SelectedNavIndex = currentIndex;
                        
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Main grid recreated successfully");
                    }
                    else
                    {
                        // Fallback: rebuild the whole content
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Falling back to InitializeComponent");
                        InitializeComponent();
                    }
                    
                    StatusMessage = _localizationManager.GetString("gui.config_tab.languages.messages.language_changed", newLang);
                    
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Language change to {newLang} completed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Error during language change: {ex.Message}");
                    // Fallback to InitializeComponent in case of any error
                    try
                    {
                        InitializeComponent();
                    }
                    catch (Exception initEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Error during InitializeComponent fallback: {initEx.Message}");
                    }
                }
            });
        }

        private void OnLanguageChangedStatic(string newLang)
        {
            // Método estático para mudança de idioma (similar ao OnThemeChanged)
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Language changed to: {newLang}");
                    
                    var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStackGUI.exe");
                    var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? _localizationManager.GetString("common.unknown");
                    Title = _localizationManager.GetString("gui.window.title", version);

                    // Recreate main layout while preserving selected index
                    int currentIndex = SelectedNavIndex;
                    var mainGrid = Content as Grid;
                    if (mainGrid != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Clearing and recreating main grid content");
                        
                        mainGrid.Children.Clear();
                        mainGrid.RowDefinitions.Clear();
                        mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                        mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                        CreateMainContent(mainGrid);
                        CreateStatusBar(mainGrid);
                        SelectedNavIndex = currentIndex;
                        
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Main grid recreated successfully");
                    }
                    else
                    {
                        // Fallback: rebuild the whole content
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Falling back to InitializeComponent");
                        InitializeComponent();
                    }
                    
                    StatusMessage = _localizationManager.GetString("gui.config_tab.languages.messages.language_changed", newLang);
                    
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Language change to {newLang} completed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Error during language change: {ex.Message}");
                    // Fallback to InitializeComponent in case of any error
                    try
                    {
                        InitializeComponent();
                    }
                    catch (Exception initEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Error during InitializeComponent fallback: {initEx.Message}");
                    }
                }
            });
        }

        private void OnThemeChanged()
        {
            // Atualiza o tema da interface em tempo real
            Dispatcher.Invoke(() =>
            {
                Background = CurrentTheme.FormBackground;
                Foreground = CurrentTheme.Foreground;

                // Recria layout principal para aplicar novas cores
                int currentIndex = SelectedNavIndex;
                var mainGrid = Content as Grid;
                if (mainGrid != null)
                {
                    mainGrid.Children.Clear();
                    mainGrid.RowDefinitions.Clear();
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                    mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    CreateMainContent(mainGrid);
                    CreateStatusBar(mainGrid);
                    SelectedNavIndex = currentIndex;
                }
                else
                {
                    // Fallback: rebuild the whole content
                    InitializeComponent();
                }
                StatusMessage = _localizationManager.GetString("common.themes.messages.theme_changed", _localizationManager.GetString("common.themes." + DevStackShared.ThemeManager.CurrentThemeType.ToString().ToLower()));
            });
        }
        #endregion

        #region Data Loading Methods
        /// <summary>
        /// Carrega a lista de componentes instalados de forma assíncrona
        /// </summary>
        public async Task LoadInstalledComponents()
        {
            try
            {
                StatusMessage = LocalizationManager.GetString("gui.installed_tab.loading");
                DevStackConfig.WriteLog("Iniciando LoadInstalledComponents");
                
                var data = await Task.Run(() => 
                {
                    DevStackConfig.WriteLog("Chamando DataManager.GetInstalledVersions()");
                    var result = DataManager.GetInstalledVersions();
                    DevStackConfig.WriteLog($"DataManager.GetInstalledVersions() retornou {result.Components.Count} componentes");
                    return result;
                });
                
                DevStackConfig.WriteLog("Processando componentes para ViewModels");
                var components = new List<ComponentViewModel>();
                
                foreach (var comp in data.Components)
                {
                    components.Add(new ComponentViewModel
                    {
                        Name = comp.Name,
                        Installed = comp.Installed,
                        IsExecutable = comp.IsExecutable,
                        Versions = comp.Versions,
                        Status = LocalizationManager.GetString("gui.common.status." + (comp.Installed ? "ok" : "error")),
                        VersionsText = comp.Installed ? string.Join(", ", comp.Versions) : LocalizationManager.GetString("gui.common.status.na")
                    });
                }
                
                DevStackConfig.WriteLog("Ordenando e atualizando UI");
                // Ordena: instalados primeiro
                var ordered = components.OrderByDescending(c => c.Installed).ThenBy(c => c.Name).ToList();
                InstalledComponents = new ObservableCollection<ComponentViewModel>(ordered);
                StatusMessage = LocalizationManager.GetString("gui.installed_tab.loaded", ordered.Count);
                DevStackConfig.WriteLog($"LoadInstalledComponents concluído com {ordered.Count} componentes");
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationManager.GetString("gui.installed_tab.error", ex.Message);
                DevStackConfig.WriteLog($"Erro em LoadInstalledComponents: {ex}");
            }
        }

        /// <summary>
        /// Carrega a lista de componentes disponíveis para instalação
        /// </summary>
        public async Task LoadAvailableComponents()
        {
            try
            {
                // Carregar dados em background
                var components = await Task.Run(() => DevStackConfig.components.ToList());
                
                // Atualizar UI no thread principal
                AvailableComponents.Clear();
                foreach (var component in components)
                {
                    AvailableComponents.Add(component);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao carregar componentes: {ex.Message}";
            }
        }

        /// <summary>
        /// Carrega as versões disponíveis para o componente selecionado
        /// </summary>
        public async Task LoadVersionsForComponent()
        {
            if (string.IsNullOrEmpty(SelectedComponent))
            {
                AvailableVersions.Clear();
                return;
            }

            try
            {
                StatusMessage = LocalizationManager.GetString("gui.install_tab.messages.loading_versions", SelectedComponent);

                var selectedComponent = SelectedComponent; // capture safely
                var versionData = await Task.Run(() => GetVersionDataForComponent(selectedComponent));
                if (versionData.Status != "ok")
                {
                    throw new Exception(string.IsNullOrWhiteSpace(versionData.Message) ? "Failed to load versions" : versionData.Message);
                }

                AvailableVersions.Clear();
                foreach (var version in versionData.Versions
                    .OrderByDescending(v =>
                        Version.TryParse(v, out var parsed) ? parsed : new Version(0, 0)))
                {
                    AvailableVersions.Add(version);
                }

                StatusMessage = LocalizationManager.GetString("gui.install_tab.messages.versions_loaded", AvailableVersions.Count, selectedComponent);
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationManager.GetString("gui.install_tab.messages.versions_error", ex.Message);
                DevStackConfig.WriteLog(LocalizationManager.GetString("gui.install_tab.messages.versions_error", ex));
            }
        }

        /// <summary>
        /// Obtém os dados de versão para um componente específico
        /// </summary>
        private VersionData GetVersionDataForComponent(string component)
        {
            try
            {
                var comp = Components.ComponentsFactory.GetComponent(component);
                if (comp != null)
                {
                    var versions = comp.ListAvailable();
                    return new VersionData
                    {
                        Status = "ok",
                        Versions = versions,
                        Message = string.Empty
                    };
                }
                else
                {
                    return new VersionData { Status = "error", Versions = new System.Collections.Generic.List<string>(), Message = $"Component '{component}' not found" };
                }
            }
            catch (Exception ex)
            {
                return new VersionData { Status = "error", Versions = new System.Collections.Generic.List<string>(), Message = ex.Message };
            }
        }

        /// <summary>
        /// Carrega a lista de serviços de forma instantânea com atualização forçada quando necessário
        /// </summary>
        public async Task LoadServices(bool status = true)
        {
            try
            {
                if (status)
                    StatusMessage = LocalizationManager.GetString("gui.services_tab.messages.loading");
                
                // Fazer tudo de forma super-rápida
                var serviceList = new List<ServiceViewModel>();
                
                // Obter apenas componentes que são serviços
                var serviceComponents = Components.ComponentsFactory.GetAll()
                    .Where(c => c.IsService && Directory.Exists(c.ToolDir))
                    .ToList();
                
                // Usar híbrido: ProcessRegistry primeiro, verificação real se necessário
                foreach (var component in serviceComponents)
                {
                    var installedVersions = component.ListInstalled();
                    
                    foreach (var version in installedVersions)
                    {
                        try
                        {
                            var key = $"{component.Name.ToLowerInvariant()}-{version}";
                            bool isRunning = false;
                            string pids = "-";
                            
                            // Primeiro: verificar ProcessRegistry (ultra-rápido)
                            var registryType = typeof(ProcessRegistry);
                            var registeredProcessesField = registryType.GetField("_registeredProcesses", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                            
                            if (registeredProcessesField?.GetValue(null) is System.Collections.Concurrent.ConcurrentDictionary<string, object> processes)
                            {
                                isRunning = processes.ContainsKey(key);
                                
                                if (isRunning)
                                {
                                    // Se está no registry, obter PIDs dos processos principais registrados
                                    try
                                    {
                                        var servicePids = ProcessRegistry.GetServicePids(component.Name, version, component.MaxWorkers);
                                        if (servicePids.Count > 0)
                                        {
                                            // Mostrar todos os PIDs dos processos principais registrados
                                            pids = string.Join(", ", servicePids);
                                        }
                                        else
                                        {
                                            pids = "Ativo";
                                        }
                                    }
                                    catch
                                    {
                                        pids = "Ativo";
                                    }
                                }
                            }
                            
                            // Se não está no registry, fazer verificação rápida por nome de processo
                            if (!isRunning && component.ServicePattern != null)
                            {
                                var processName = Path.GetFileNameWithoutExtension(component.ServicePattern);
                                var serviceExe = Path.Combine(component.ToolDir, $"{component.Name}-{version}", component.ServicePattern);
                                
                                if (File.Exists(serviceExe))
                                {
                                    var runningProcesses = Process.GetProcessesByName(processName);
                                    var foundPids = new List<int>();
                                    
                                    foreach (var proc in runningProcesses)
                                    {
                                        try
                                        {
                                            var processPath = proc.MainModule?.FileName;
                                            if (processPath?.Equals(serviceExe, StringComparison.OrdinalIgnoreCase) == true)
                                            {
                                                foundPids.Add(proc.Id);
                                            }
                                        }
                                        catch { }
                                        finally { proc.Dispose(); }
                                    }
                                    
                                    if (foundPids.Count > 0)
                                    {
                                        isRunning = true;
                                        
                                        // Registrar todos os processos encontrados como processos principais
                                        ProcessRegistry.RegisterServiceWithMultipleProcesses(component.Name, version, foundPids, serviceExe);
                                        
                                        // Mostrar todos os PIDs
                                        pids = string.Join(", ", foundPids);
                                    }
                                }
                            }
                            
                            serviceList.Add(new ServiceViewModel 
                            { 
                                Name = component.Name, 
                                Version = version,
                                Status = isRunning ? LocalizationManager.GetString("gui.services_tab.status.running") : LocalizationManager.GetString("gui.services_tab.status.stopped"), 
                                Type = component.GetServiceType(LocalizationManager), 
                                Description = component.GetServiceDescription(version, LocalizationManager),
                                Pid = pids,
                                IsRunning = isRunning
                            });
                        }
                        catch
                        {
                            // Em caso de erro, adicionar como parado
                            serviceList.Add(new ServiceViewModel 
                            { 
                                Name = component.Name, 
                                Version = version,
                                Status = LocalizationManager.GetString("gui.services_tab.status.stopped"), 
                                Type = component.GetServiceType(LocalizationManager), 
                                Description = component.GetServiceDescription(version, LocalizationManager),
                                Pid = "-",
                                IsRunning = false
                            });
                        }
                    }
                }
                
                // Ordenar lista
                var orderedServices = serviceList.OrderBy(s => s.Name).ThenBy(s => s.Version).ToList();
                
                // Atualizar UI
                await Dispatcher.InvokeAsync(() =>
                {
                    Services.Clear();
                    foreach (var service in orderedServices)
                    {
                        Services.Add(service);
                    }
                    if (status)
                        StatusMessage = LocalizationManager.GetString("gui.services_tab.messages.loaded", new object[] { Services.Count });
                });
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                {
                    if (status)
                        StatusMessage = LocalizationManager.GetString("gui.services_tab.messages.error", ex.Message ?? string.Empty);
                    DevStackConfig.WriteLog(LocalizationManager.GetString("gui.services_tab.debug.load_services_error", ex));
                });
            }
        }
        #endregion

        #region Shortcut Methods
        /// <summary>
        /// Carrega os componentes instalados que têm CreateBinShortcut definido
        /// </summary>
        public async Task LoadShortcutComponents()
        {
            try
            {
                DevStackConfig.WriteLog("LoadShortcutComponents: Iniciando");
                
                // Limpar a coleção antes de carregar
                await Dispatcher.InvokeAsync(() => ShortcutComponents.Clear());
                
                DevStackConfig.WriteLog($"LoadShortcutComponents: InstalledComponents.Count = {InstalledComponents.Count}");

                // Obter componentes instalados que têm CreateBinShortcut definido
                var installedComponents = InstalledComponents.Where(c => c.Installed).ToList();
                DevStackConfig.WriteLog($"LoadShortcutComponents: Componentes instalados = {installedComponents.Count}");
                
                var shortcutComponents = new List<string>();

                foreach (var comp in installedComponents)
                {
                    try
                    {
                        var component = await Task.Run(() => Components.ComponentsFactory.GetComponent(comp.Name));
                        if (component != null)
                        {
                            if (!string.IsNullOrEmpty(component.CreateBinShortcut))
                            {
                                shortcutComponents.Add(comp.Name);
                                DevStackConfig.WriteLog($"LoadShortcutComponents: Componente {comp.Name} adicionado (CreateBinShortcut = '{component.CreateBinShortcut}')");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DevStackConfig.WriteLog($"Erro ao verificar componente {comp.Name} para atalhos: {ex}");
                    }
                }

                DevStackConfig.WriteLog($"LoadShortcutComponents: Total de componentes com shortcut = {shortcutComponents.Count}");

                // Atualizar a ObservableCollection na UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var componentName in shortcutComponents)
                    {
                        ShortcutComponents.Add(componentName);
                    }
                });
                
                DevStackConfig.WriteLog($"LoadShortcutComponents: Concluído. ShortcutComponents.Count = {ShortcutComponents.Count}");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao carregar componentes para atalhos: {ex.Message}";
                DevStackConfig.WriteLog($"Erro ao carregar componentes para atalhos na GUI: {ex}");
            }
        }

        /// <summary>
        /// Carrega as versões instaladas do componente selecionado para criação de atalho
        /// </summary>
        public async Task LoadShortcutVersions()
        {
            if (string.IsNullOrEmpty(SelectedShortcutComponent))
            {
                // Limpar versões se nenhum componente selecionado
                await Dispatcher.InvokeAsync(() => ShortcutVersions.Clear());
                return;
            }

            await Task.Run(async () =>
            {
                try
                {
                    var status = DataManager.GetComponentStatus(SelectedShortcutComponent);
                    var versionsList = new List<string>();

                    if (status.Installed && status.Versions.Any())
                    {
                        // Ordena as versões em ordem decrescente
                        foreach (var version in status.Versions
                            .OrderByDescending(v => 
                            {
                                // Extrair apenas a parte da versão, removendo o nome do componente se presente
                                var versionNumber = v;
                                if (v.StartsWith($"{SelectedShortcutComponent}-"))
                                {
                                    versionNumber = v.Substring(SelectedShortcutComponent.Length + 1);
                                }
                                return Version.TryParse(versionNumber, out var parsed) ? parsed : new Version(0, 0);
                            }))
                        {
                            // Extrair apenas a parte da versão, removendo o nome do componente
                            var versionNumber = version;
                            if (version.StartsWith($"{SelectedShortcutComponent}-"))
                            {
                                versionNumber = version.Substring(SelectedShortcutComponent.Length + 1);
                            }
                            versionsList.Add(versionNumber);
                        }
                    }

                    // Atualizar a ObservableCollection na UI thread
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ShortcutVersions.Clear();
                        foreach (var version in versionsList)
                        {
                            ShortcutVersions.Add(version);
                        }
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.InvokeAsync(() =>
                    {
                        StatusMessage = $"Erro ao carregar versões para atalho: {ex.Message}";
                        DevStackConfig.WriteLog($"Erro ao carregar versões para atalho na GUI: {ex}");
                    });
                }
            });
        }

        /// <summary>
        /// Carrega os componentes disponíveis para desinstalação
        /// </summary>
        public async Task LoadUninstallComponents()
        {
            try
            {
                StatusMessage = LocalizationManager.GetString("gui.uninstall_tab.status.loading_components");
                
                var componentCombo = GuiHelpers.FindChild<ComboBox>(this, "UninstallComponentCombo");
                if (componentCombo == null)
                {
                    // Se não encontrou o combo, tentar novamente após um delay
                    await Task.Delay(200);
                    await Dispatcher.InvokeAsync(async () => await LoadUninstallComponents());
                    return;
                }
                
                componentCombo.Items.Clear();
                
                // Obter componentes instalados
                var installedComponents = InstalledComponents.Where(c => c.Installed).ToList();
                
                if (installedComponents.Any())
                {
                    foreach (var comp in installedComponents)
                    {
                        componentCombo.Items.Add(comp.Name);
                    }

                    componentCombo.SelectedIndex = -1;
                    
                    StatusMessage = LocalizationManager.GetString("gui.uninstall_tab.status.components_count", componentCombo.Items.Count);
                }
                else
                {
                    // Sem componentes instalados: não reagendar carregamento infinito
                    componentCombo.Items.Clear();
                    componentCombo.SelectedIndex = -1;
                    StatusMessage = LocalizationManager.GetString("gui.uninstall_tab.status.components_count", 0);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationManager.GetString("gui.uninstall_tab.status.error_loading_components", ex.Message);
                DevStackConfig.WriteLog($"Erro ao carregar componentes para desinstalação na GUI: {ex}");
            }
        }

        /// <summary>
        /// Carrega as versões instaladas do componente selecionado para desinstalação
        /// </summary>
        public async Task LoadUninstallVersions()
        {
            if (string.IsNullOrEmpty(SelectedUninstallComponent))
            {
                // Limpar versões se nenhum componente selecionado
                var versionCombo = GuiHelpers.FindChild<ComboBox>(this, "UninstallVersionCombo");
                if (versionCombo != null)
                {
                    versionCombo.Items.Clear();
                }
                return;
            }

            await Task.Run(() =>
            {
                try
                {
                    var statusMessage = LocalizationManager.GetString("gui.uninstall_tab.status.loading_versions", SelectedUninstallComponent);
                    var status = DataManager.GetComponentStatus(SelectedUninstallComponent);
                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage = statusMessage;
                        var versionCombo = GuiHelpers.FindChild<ComboBox>(this, "UninstallVersionCombo");
                        if (versionCombo != null)
                        {
                            versionCombo.Items.Clear();
                            if (status.Installed && status.Versions.Any())
                            {
                                // Ordena as versões em ordem decrescente
                                foreach (var version in status.Versions
                                    .OrderByDescending(v => Version.TryParse(
                                        SelectedUninstallComponent == "git" && v.StartsWith("git-")
                                            ? v.Substring(4)
                                            : v.StartsWith($"{SelectedUninstallComponent}-")
                                                ? v.Substring(SelectedUninstallComponent.Length + 1)
                                                : v,
                                        out var parsed) ? parsed : new Version(0, 0)))
                                {
                                    // Extrair apenas a parte da versão, removendo o nome do componente
                                    var versionNumber = version;
                                    if (SelectedUninstallComponent == "git" && version.StartsWith("git-"))
                                    {
                                        versionNumber = version.Substring(4); // Remove "git-"
                                    }
                                    else if (version.StartsWith($"{SelectedUninstallComponent}-"))
                                    {
                                        versionNumber = version.Substring(SelectedUninstallComponent.Length + 1);
                                    }
                                    versionCombo.Items.Add(versionNumber);
                                }
                            }
                            else
                            {
                                DevStackShared.ThemeManager.CreateStyledMessageBox(
                                    LocalizationManager.GetString("gui.uninstall_tab.messages.no_versions", SelectedUninstallComponent), 
                                    LocalizationManager.GetString("gui.common.dialogs.info"), 
                                    MessageBoxButton.OK, 
                                    MessageBoxImage.Information);
                            }
                        }
                        StatusMessage = status.Installed ?
                            LocalizationManager.GetString("gui.uninstall_tab.status.versions_loaded", SelectedUninstallComponent) :
                            LocalizationManager.GetString("gui.uninstall_tab.status.not_installed", SelectedUninstallComponent);
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        StatusMessage = LocalizationManager.GetString("gui.uninstall_tab.status.error_loading_versions", ex.Message);
                        DevStackConfig.WriteLog($"Erro ao carregar versões para desinstalação na GUI: {ex}");
                    });
                }
            });
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Obtém a linha de comando de um processo específico
        /// </summary>
        private string GetProcessCommandLine(int processId)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processId}"))
                {
                    using (var objects = searcher.Get())
                    {
                        var commandLine = objects.Cast<ManagementBaseObject>()
                            .FirstOrDefault()?["CommandLine"]?.ToString();
                        return commandLine ?? string.Empty;
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public async void RefreshAllData()
        {
            try
            {
                StatusMessage = "Iniciando carregamento de dados...";
                DevStackConfig.WriteLog("Iniciando RefreshAllData");
                
                // Carregar componentes instalados primeiro (crítico para o funcionamento)
                StatusMessage = "Carregando componentes instalados...";
                await LoadInstalledComponents();
                
                // Carregar componentes disponíveis
                StatusMessage = "Carregando componentes disponíveis...";
                await LoadAvailableComponents();
                
                // Carregar serviços em paralelo com shortcuts e uninstall
                StatusMessage = "Carregando serviços e outras opções...";
                var task1 = LoadServices();
                var task2 = LoadShortcutComponents();
                var task3 = LoadUninstallComponents();
                
                await Task.WhenAll(task1, task2, task3);
                
                StatusMessage = "Todos os dados carregados com sucesso";
                DevStackConfig.WriteLog("RefreshAllData concluído");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Erro ao carregar dados: {ex.Message}";
                DevStackConfig.WriteLog($"Erro em RefreshAllData: {ex}");
            }
        }
        
        /// <summary>
        /// Limpa o cache de status dos serviços e força atualização imediata
        /// </summary>
        public async Task RefreshServicesStatus()
        {
            ClearServicesCache();
            await LoadServices(false);
        }
        
        /// <summary>
        /// Limpa o cache de status dos serviços (usar quando serviços são iniciados/parados manualmente)
        /// </summary>
        public void ClearServicesCache()
        {
            _serviceStatusCache.Clear();
            _lastServicesCacheUpdate = DateTime.MinValue;
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _servicesUpdateTimer?.Stop();
            _servicesUpdateTimer?.Dispose();
            ClearServicesCache();
            base.OnClosed(e);
        }
        #endregion
    }
}
