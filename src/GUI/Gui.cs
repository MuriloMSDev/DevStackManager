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
    /// Modern WPF graphical interface for DevStack Manager.
    /// Provides visual interface for installing, uninstalling, and managing development stack components.
    /// Implements real-time service monitoring, component management, and multi-language support.
    /// Converted from original PowerShell GUI (gui.ps1) while maintaining functionality and layout.
    /// </summary>
    public partial class DevStackGui : Window, INotifyPropertyChanged
    {
        #region Constants
        /// <summary>
        /// Default window width in pixels.
        /// </summary>
        private const double WINDOW_WIDTH = 1200;
        
        /// <summary>
        /// Default window height in pixels.
        /// </summary>
        private const double WINDOW_HEIGHT = 840;
        
        /// <summary>
        /// Minimum allowed window width in pixels.
        /// </summary>
        private const double WINDOW_MIN_WIDTH = 1200;
        
        /// <summary>
        /// Minimum allowed window height in pixels.
        /// </summary>
        private const double WINDOW_MIN_HEIGHT = 840;
        #endregion
        
        #region Private Fields
        /// <summary>
        /// Localization manager for multi-language support.
        /// </summary>
        private readonly DevStackShared.LocalizationManager _localizationManager;
        
        /// <summary>
        /// Timer for periodic services status updates.
        /// </summary>
        private readonly System.Timers.Timer _servicesUpdateTimer;
        
        /// <summary>
        /// Cache for service status to improve performance.
        /// </summary>
        private readonly Dictionary<string, bool> _serviceStatusCache = new();
        
        /// <summary>
        /// Timestamp of last services cache update.
        /// </summary>
        private DateTime _lastServicesCacheUpdate = DateTime.MinValue;
        
        /// <summary>
        /// Status bar message displayed at the bottom of the window.
        /// </summary>
        private string _statusMessage = "";
        
        /// <summary>
        /// Collection of installed components displayed in the dashboard.
        /// </summary>
        private ObservableCollection<ComponentViewModel> _installedComponents = new();
        
        /// <summary>
        /// List of available components for installation.
        /// </summary>
        private ObservableCollection<string> _availableComponents = new();
        
        /// <summary>
        /// List of available versions for the selected component.
        /// </summary>
        private ObservableCollection<string> _availableVersions = new();
        
        /// <summary>
        /// Collection of running services with their status.
        /// </summary>
        private ObservableCollection<ServiceViewModel> _services = new();
        
        /// <summary>
        /// List of components with available shortcuts.
        /// </summary>
        private ObservableCollection<string> _shortcutComponents = new();
        
        /// <summary>
        /// List of versions for the selected shortcut component.
        /// </summary>
        private ObservableCollection<string> _shortcutVersions = new();
        
        /// <summary>
        /// Currently selected component for installation.
        /// </summary>
        private string _selectedComponent = "";
        
        /// <summary>
        /// Currently selected version for installation.
        /// </summary>
        private string _selectedVersion = "";
        
        /// <summary>
        /// Currently selected component for uninstallation.
        /// </summary>
        private string _selectedUninstallComponent = "";
        
        /// <summary>
        /// Currently selected version for uninstallation.
        /// </summary>
        private string _selectedUninstallVersion = "";
        
        /// <summary>
        /// Currently selected component for shortcut operations.
        /// </summary>
        private string _selectedShortcutComponent = "";
        
        /// <summary>
        /// Currently selected version for shortcut operations.
        /// </summary>
        private string _selectedShortcutVersion = "";
        
        /// <summary>
        /// Console output text displayed in the console panel.
        /// </summary>
        private string _consoleOutput = "";
        
        /// <summary>
        /// Indicates whether a component installation is in progress.
        /// </summary>
        private bool _isInstallingComponent = false;
        
        /// <summary>
        /// Indicates whether a component uninstallation is in progress.
        /// </summary>
        private bool _isUninstallingComponent = false;
        
        /// <summary>
        /// Indicates whether services are currently being loaded.
        /// </summary>
        private bool _isLoadingServices = false;
        
        /// <summary>
        /// Indicates whether a site is currently being created.
        /// </summary>
        private bool _isCreatingSite = false;
        
        /// <summary>
        /// Gets or sets whether a component installation is in progress.
        /// When installation completes, automatically reloads installed components and shortcuts.
        /// </summary>
        public bool IsInstallingComponent
        {
            get => _isInstallingComponent;
            set 
            { 
                var oldValue = _isInstallingComponent;
                _isInstallingComponent = value; 
                OnPropertyChanged();
                
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
        
        /// <summary>
        /// Gets or sets whether a component uninstallation is in progress.
        /// When uninstallation completes, automatically reloads installed components, services, and shortcuts.
        /// </summary>
        public bool IsUninstallingComponent
        {
            get => _isUninstallingComponent;
            set 
            { 
                var oldValue = _isUninstallingComponent;
                _isUninstallingComponent = value; 
                OnPropertyChanged();
                
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
        
        /// <summary>
        /// Gets or sets whether services are currently being loaded.
        /// </summary>
        public bool IsLoadingServices
        {
            get => _isLoadingServices;
            set { _isLoadingServices = value; OnPropertyChanged(); }
        }
        
        /// <summary>
        /// Gets or sets whether a site is currently being created.
        /// </summary>
        public bool IsCreatingSite
        {
            get => _isCreatingSite;
            set { _isCreatingSite = value; OnPropertyChanged(); }
        }
        
        /// <summary>
        /// Main content control that displays the current active tab.
        /// </summary>
        public ContentControl? _mainContent;
        
        /// <summary>
        /// Index of the currently selected navigation item (0=Dashboard, 1=Install, 2=Uninstall, etc.).
        /// </summary>
        private int _selectedNavIndex = 0;
        
        /// <summary>
        /// Gets the current theme colors.
        /// </summary>
        public DevStackShared.ThemeManager.ThemeColors CurrentTheme => DevStackShared.ThemeManager.CurrentTheme;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the localization manager for internationalization support.
        /// </summary>
        public DevStackShared.LocalizationManager LocalizationManager => _localizationManager;
        
        /// <summary>
        /// Gets or sets the status bar message displayed at the bottom of the window.
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the collection of installed components displayed in the dashboard.
        /// </summary>
        public ObservableCollection<ComponentViewModel> InstalledComponents
        {
            get => _installedComponents;
            set { _installedComponents = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the list of available components for installation.
        /// </summary>
        public ObservableCollection<string> AvailableComponents
        {
            get => _availableComponents;
            set { _availableComponents = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the list of available versions for the selected component.
        /// </summary>
        public ObservableCollection<string> AvailableVersions
        {
            get => _availableVersions;
            set { _availableVersions = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the collection of running services with their status.
        /// </summary>
        public ObservableCollection<ServiceViewModel> Services
        {
            get => _services;
            set { _services = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the list of components with available shortcuts.
        /// </summary>
        public ObservableCollection<string> ShortcutComponents
        {
            get => _shortcutComponents;
            set { _shortcutComponents = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the list of versions for the selected shortcut component.
        /// </summary>
        public ObservableCollection<string> ShortcutVersions
        {
            get => _shortcutVersions;
            set { _shortcutVersions = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the currently selected component for installation.
        /// When changed, automatically loads available versions.
        /// </summary>
        public string SelectedComponent
        {
            get => _selectedComponent;
            set { _selectedComponent = value; OnPropertyChanged(); _ = LoadVersionsForComponent(); }
        }

        /// <summary>
        /// Gets or sets the currently selected version for installation.
        /// </summary>
        public string SelectedVersion
        {
            get => _selectedVersion;
            set { _selectedVersion = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the currently selected component for uninstallation.
        /// When changed, automatically loads installed versions.
        /// </summary>
        public string SelectedUninstallComponent
        {
            get => _selectedUninstallComponent;
            set { _selectedUninstallComponent = value; OnPropertyChanged(); _ = LoadUninstallVersions(); }
        }

        /// <summary>
        /// Gets or sets the currently selected version for uninstallation.
        /// </summary>
        public string SelectedUninstallVersion
        {
            get => _selectedUninstallVersion;
            set { _selectedUninstallVersion = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the currently selected component for shortcut operations.
        /// When changed, automatically loads available versions.
        /// </summary>
        public string SelectedShortcutComponent
        {
            get => _selectedShortcutComponent;
            set { _selectedShortcutComponent = value; OnPropertyChanged(); _ = LoadShortcutVersions(); }
        }

        /// <summary>
        /// Gets or sets the currently selected version for shortcut operations.
        /// </summary>
        public string SelectedShortcutVersion
        {
            get => _selectedShortcutVersion;
            set { _selectedShortcutVersion = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Gets or sets the console output text displayed in the console panel.
        /// </summary>
        public string ConsoleOutput
        {
            get => _consoleOutput;
            set { _consoleOutput = value; OnPropertyChanged(); }
        }
        
        /// <summary>
        /// Gets or sets the selected navigation index (0=Dashboard, 1=Install, 2=Uninstall, etc.).
        /// When changed, navigates to the corresponding section.
        /// </summary>
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
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        
        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the DevStackGui window.
        /// Loads saved language and theme preferences, sets up service monitoring timer,
        /// and initializes component data when the window is shown.
        /// </summary>
        public DevStackGui()
        {
            _localizationManager = DevStackShared.LocalizationManager.Initialize(DevStackShared.ApplicationType.DevStack);
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

            DevStackShared.LocalizationManager.OnLanguageChangedStatic += OnLanguageChangedStatic;

            DevStackShared.ThemeManager.OnThemeChanged += OnThemeChanged;

            _servicesUpdateTimer = new System.Timers.Timer(5000);
            _servicesUpdateTimer.Elapsed += async (sender, e) => await LoadServices(false);
            _servicesUpdateTimer.AutoReset = true;
            _servicesUpdateTimer.Start();

            ProcessRegistry.LoadFromFile();

            InitializeComponent();
            DataContext = this;

            this.Loaded += (s, e) => 
            {
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
                                StatusMessage = _localizationManager.GetString("gui.status_bar.error_loading_initial", ex.Message);
                            });
                        }
                    });
                }
            };
        }
        #endregion

        #region Initialization Methods
        /// <summary>
        /// Initializes all WPF UI components and builds the window layout.
        /// Creates navigation panel, main content area, status bar, and console output panel.
        /// </summary>
        private void InitializeComponent()
        {
            var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStackGUI.exe");
            var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? _localizationManager.GetString("common.unknown");
            Title = _localizationManager.GetString("gui.window.title", version);
            Width = WINDOW_WIDTH;
            Height = WINDOW_HEIGHT;
            MinWidth = WINDOW_MIN_WIDTH;
            MinHeight = WINDOW_MIN_HEIGHT;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = CurrentTheme.FormBackground;
            Foreground = CurrentTheme.Foreground;

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            CreateMainContent(mainGrid);

            CreateStatusBar(mainGrid);

            Content = mainGrid;
        }

        /// <summary>
        /// Creates the main content area with navigation and content panels.
        /// Delegates to GuiMainContent helper class.
        /// </summary>
        /// <param name="mainGrid">Parent grid to add content to.</param>
        private void CreateMainContent(Grid mainGrid)
        {
            GuiMainContent.CreateMainContent(mainGrid, this);
        }

        /// <summary>
        /// Creates the status bar at the bottom of the window.
        /// Delegates to GuiStatusBar helper class.
        /// </summary>
        /// <param name="mainGrid">Parent grid to add status bar to.</param>
        private void CreateStatusBar(Grid mainGrid)
        {
            GuiStatusBar.CreateStatusBar(mainGrid, this);
        }
        
        /// <summary>
        /// Handles language change event from LocalizationManager.
        /// Rebuilds entire UI to reflect new language strings while preserving navigation state.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="newLang">New language code (e.g., "en_US", "pt_BR").</param>
        private void OnLanguageChanged(object? sender, string newLang)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Language changed to: {newLang}");
                    
                    var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStackGUI.exe");
                    var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? _localizationManager.GetString("common.unknown");
                    Title = _localizationManager.GetString("gui.window.title", version);

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
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Falling back to InitializeComponent");
                        InitializeComponent();
                    }
                    
                    StatusMessage = _localizationManager.GetString("gui.config_tab.languages.messages.language_changed", newLang);
                    
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Language change to {newLang} completed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChanged] Error during language change: {ex.Message}");
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

        /// <summary>
        /// Static handler for language change events.
        /// Rebuilds entire UI to reflect new language strings while preserving navigation state.
        /// </summary>
        /// <param name="newLang">New language code (e.g., "en_US", "pt_BR").</param>
        private void OnLanguageChangedStatic(string newLang)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Language changed to: {newLang}");
                    
                    var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStackGUI.exe");
                    var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? _localizationManager.GetString("common.unknown");
                    Title = _localizationManager.GetString("gui.window.title", version);

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
                        System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Falling back to InitializeComponent");
                        InitializeComponent();
                    }
                    
                    StatusMessage = _localizationManager.GetString("gui.config_tab.languages.messages.language_changed", newLang);
                    
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Language change to {newLang} completed successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[OnLanguageChangedStatic] Error during language change: {ex.Message}");
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

        /// <summary>
        /// Handles theme change event from ThemeManager.
        /// Updates window colors and rebuilds UI to apply new theme while preserving navigation state.
        /// </summary>
        private void OnThemeChanged()
        {
            Dispatcher.Invoke(() =>
            {
                Background = CurrentTheme.FormBackground;
                Foreground = CurrentTheme.Foreground;

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
                    InitializeComponent();
                }
                StatusMessage = _localizationManager.GetString("common.themes.messages.theme_changed", _localizationManager.GetString("common.themes." + DevStackShared.ThemeManager.CurrentThemeType.ToString().ToLower()));
            });
        }
        #endregion

        #region Data Loading Methods
        /// <summary>
        /// Loads the list of installed components asynchronously.
        /// Queries DataManager for installed versions and updates InstalledComponents collection.
        /// </summary>
        public async Task LoadInstalledComponents()
        {
            try
            {
                StatusMessage = LocalizationManager.GetString("gui.installed_tab.loading");
                DevStackConfig.WriteLog("Starting LoadInstalledComponents");
                
                var data = await Task.Run(() => 
                {
                    DevStackConfig.WriteLog("Calling DataManager.GetInstalledVersions()");
                    var result = DataManager.GetInstalledVersions();
                    DevStackConfig.WriteLog($"DataManager.GetInstalledVersions() returned {result.Components.Count} components");
                    return result;
                });
                
                DevStackConfig.WriteLog("Processing components to ViewModels");
                var components = new List<ComponentViewModel>();
                
                foreach (var comp in data.Components)
                {
                    var compDef = Components.ComponentsFactory.GetComponent(comp.Name);
                    components.Add(new ComponentViewModel
                    {
                        Name = comp.Name,
                        Label = compDef?.Label ?? comp.Name,
                        Installed = comp.Installed,
                        IsExecutable = comp.IsExecutable,
                        Versions = comp.Versions,
                        Status = LocalizationManager.GetString("gui.common.status." + (comp.Installed ? "ok" : "error")),
                        VersionsText = comp.Installed ? string.Join(", ", comp.Versions) : LocalizationManager.GetString("gui.common.status.na")
                    });
                }
                
                DevStackConfig.WriteLog("Sorting and updating UI");
                var ordered = components.OrderByDescending(c => c.Installed).ThenBy(c => c.Label).ToList();
                InstalledComponents = new ObservableCollection<ComponentViewModel>(ordered);
                StatusMessage = LocalizationManager.GetString("gui.installed_tab.loaded", ordered.Count);
                DevStackConfig.WriteLog($"LoadInstalledComponents completed with {ordered.Count} components");
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationManager.GetString("gui.installed_tab.error", ex.Message);
                DevStackConfig.WriteLog($"Error in LoadInstalledComponents: {ex}");
            }
        }

        /// <summary>
        /// Loads the list of available components for installation.
        /// Populates AvailableComponents collection from DevStackConfig.
        /// </summary>
        public async Task LoadAvailableComponents()
        {
            try
            {
                var components = await Task.Run(() => DevStackConfig.components.ToList());
                
                AvailableComponents.Clear();
                foreach (var component in components)
                {
                    AvailableComponents.Add(component);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = _localizationManager.GetString("gui.status_bar.error_loading_components", ex.Message);
            }
        }

        /// <summary>
        /// Loads available versions for the currently selected component.
        /// Queries component provider and populates AvailableVersions collection sorted by version number.
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

                var selectedComponent = SelectedComponent;
                var versionData = await Task.Run(() => GetVersionDataForComponent(selectedComponent));
                if (versionData.Status != "ok")
                {
                    throw new Exception(string.IsNullOrWhiteSpace(versionData.Message) ? _localizationManager.GetString("gui.install_tab.messages.failed_to_load_versions") : versionData.Message);
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
        /// Retrieves version data for a specific component.
        /// </summary>
        /// <param name="component">Component name to query.</param>
        /// <returns>VersionData object containing status, versions list, and error message if any.</returns>
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
                    return new VersionData { Status = "error", Versions = new System.Collections.Generic.List<string>(), Message = _localizationManager.GetString("gui.install_tab.messages.component_not_found", component) };
                }
            }
            catch (Exception ex)
            {
                return new VersionData { Status = "error", Versions = new System.Collections.Generic.List<string>(), Message = ex.Message };
            }
        }

        /// <summary>
        /// Loads services status with optional forced refresh.
        /// Uses hybrid approach: checks ProcessRegistry first for speed, then verifies running processes.
        /// Updates Services collection with running status and PIDs for each service version.
        /// </summary>
        /// <param name="status">Whether to update status message.</param>
        public async Task LoadServices(bool status = true)
        {
            try
            {
                if (status)
                    StatusMessage = LocalizationManager.GetString("gui.services_tab.messages.loading");
                
                var serviceList = new List<ServiceViewModel>();
                
                var serviceComponents = Components.ComponentsFactory.GetAll()
                    .Where(c => c.IsService && Directory.Exists(c.ToolDir))
                    .ToList();
                
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
                            
                            var registryType = typeof(ProcessRegistry);
                            var registeredProcessesField = registryType.GetField("_registeredProcesses", 
                                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                            
                            if (registeredProcessesField?.GetValue(null) is System.Collections.Concurrent.ConcurrentDictionary<string, object> processes)
                            {
                                isRunning = processes.ContainsKey(key);
                                
                                if (isRunning)
                                {
                                    try
                                    {
                                        var servicePids = ProcessRegistry.GetServicePids(component.Name, version, component.MaxWorkers);
                                        if (servicePids.Count > 0)
                                        {
                                            pids = string.Join(", ", servicePids);
                                        }
                                        else
                                        {
                                            pids = _localizationManager.GetString("gui.services_tab.status.active");
                                        }
                                    }
                                    catch
                                    {
                                        pids = _localizationManager.GetString("gui.services_tab.status.active");
                                    }
                                }
                            }
                            
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
                                        
                                        ProcessRegistry.RegisterServiceWithMultipleProcesses(component.Name, version, foundPids, serviceExe);
                                        
                                        pids = string.Join(", ", foundPids);
                                    }
                                }
                            }
                            
                            serviceList.Add(new ServiceViewModel 
                            { 
                                Name = component.Name, 
                                Label = component.Label,
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
                            serviceList.Add(new ServiceViewModel 
                            { 
                                Name = component.Name, 
                                Label = component.Label,
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
                
                var orderedServices = serviceList.OrderBy(s => s.Label).ThenBy(s => s.Version).ToList();
                
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
        /// Loads installed components that have CreateBinShortcut defined.
        /// Filters InstalledComponents for those with shortcut creation support and populates ShortcutComponents collection.
        /// </summary>
        /// <returns>Task representing the async load operation</returns>
        public async Task LoadShortcutComponents()
        {
            try
            {
                DevStackConfig.WriteLog("LoadShortcutComponents: Starting");
                
                await Dispatcher.InvokeAsync(() => ShortcutComponents.Clear());
                
                DevStackConfig.WriteLog($"LoadShortcutComponents: InstalledComponents.Count = {InstalledComponents.Count}");

                var installedComponents = InstalledComponents.Where(c => c.Installed).ToList();
                DevStackConfig.WriteLog($"LoadShortcutComponents: Installed components = {installedComponents.Count}");
                
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
                                DevStackConfig.WriteLog($"LoadShortcutComponents: Component {comp.Name} added (CreateBinShortcut = '{component.CreateBinShortcut}')");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        DevStackConfig.WriteLog($"Error checking component {comp.Name} for shortcuts: {ex}");
                    }
                }

                DevStackConfig.WriteLog($"LoadShortcutComponents: Total components with shortcut = {shortcutComponents.Count}");

                await Dispatcher.InvokeAsync(() =>
                {
                    foreach (var componentName in shortcutComponents)
                    {
                        ShortcutComponents.Add(componentName);
                    }
                });
                
                DevStackConfig.WriteLog($"LoadShortcutComponents: Completed. ShortcutComponents.Count = {ShortcutComponents.Count}");
            }
            catch (Exception ex)
            {
                StatusMessage = _localizationManager.GetString("gui.status_bar.error_loading_shortcuts", ex.Message);
                DevStackConfig.WriteLog($"Error loading components for shortcuts in GUI: {ex}");
            }
        }

        /// <summary>
        /// Loads installed versions of the selected component for shortcut creation.
        /// Sorts versions in descending order and updates ShortcutVersions collection.
        /// </summary>
        /// <returns>Task representing the async load operation</returns>
        public async Task LoadShortcutVersions()
        {
            if (string.IsNullOrEmpty(SelectedShortcutComponent))
            {
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
                        foreach (var version in status.Versions
                            .OrderByDescending(v => 
                            {
                                var versionNumber = v;
                                if (v.StartsWith($"{SelectedShortcutComponent}-"))
                                {
                                    versionNumber = v.Substring(SelectedShortcutComponent.Length + 1);
                                }
                                return Version.TryParse(versionNumber, out var parsed) ? parsed : new Version(0, 0);
                            }))
                        {
                            var versionNumber = version;
                            if (version.StartsWith($"{SelectedShortcutComponent}-"))
                            {
                                versionNumber = version.Substring(SelectedShortcutComponent.Length + 1);
                            }
                            versionsList.Add(versionNumber);
                        }
                    }

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
                        StatusMessage = _localizationManager.GetString("gui.status_bar.error_loading_versions", ex.Message);
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
                    await Task.Delay(200);
                    await Dispatcher.InvokeAsync(async () => await LoadUninstallComponents());
                    return;
                }
                
                componentCombo.Items.Clear();
                
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
                                foreach (var version in status.Versions
                                    .OrderByDescending(v => Version.TryParse(
                                        SelectedUninstallComponent == "git" && v.StartsWith("git-")
                                            ? v.Substring(4)
                                            : v.StartsWith($"{SelectedUninstallComponent}-")
                                                ? v.Substring(SelectedUninstallComponent.Length + 1)
                                                : v,
                                        out var parsed) ? parsed : new Version(0, 0)))
                                {
                                    var versionNumber = version;
                                    if (SelectedUninstallComponent == "git" && version.StartsWith("git-"))
                                    {
                                        versionNumber = version.Substring(4);
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

        /// <summary>
        /// Refreshes all data in the application including installed components, available components, services, and shortcuts.
        /// </summary>
        public async void RefreshAllData()
        {
            try
            {
                StatusMessage = LocalizationManager.GetString("gui.status_bar.loading_data");
                DevStackConfig.WriteLog("Iniciando RefreshAllData");
                
                StatusMessage = LocalizationManager.GetString("gui.status_bar.loading_installed");
                await LoadInstalledComponents();
                
                StatusMessage = LocalizationManager.GetString("gui.status_bar.loading_available");
                await LoadAvailableComponents();
                
                StatusMessage = LocalizationManager.GetString("gui.status_bar.loading_services");
                var task1 = LoadServices();
                var task2 = LoadShortcutComponents();
                var task3 = LoadUninstallComponents();
                
                await Task.WhenAll(task1, task2, task3);
                
                StatusMessage = LocalizationManager.GetString("gui.status_bar.loading_complete");
                DevStackConfig.WriteLog("RefreshAllData completed");
            }
            catch (Exception ex)
            {
                StatusMessage = LocalizationManager.GetString("gui.status_bar.loading_error", ex.Message);
                DevStackConfig.WriteLog($"Error in RefreshAllData: {ex}");
            }
        }
        
        /// <summary>
        /// Clears the service status cache and forces immediate update.
        /// Use after manually starting/stopping services.
        /// </summary>
        /// <returns>Task representing the async refresh operation</returns>
        public async Task RefreshServicesStatus()
        {
            ClearServicesCache();
            await LoadServices(false);
        }
        
        /// <summary>
        /// Clears the service status cache.
        /// Forces next LoadServices call to query actual service status instead of using cached data.
        /// </summary>
        public void ClearServicesCache()
        {
            _serviceStatusCache.Clear();
            _lastServicesCacheUpdate = DateTime.MinValue;
        }

        /// <summary>
        /// Handles the window closed event and performs cleanup operations.
        /// </summary>
        /// <param name="sender">The window that was closed.</param>
        /// <param name="e">Event arguments for the closed event.</param>
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
