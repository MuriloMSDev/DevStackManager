using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        private string _statusMessage = "";
        private ObservableCollection<ComponentViewModel> _installedComponents = new();
        private ObservableCollection<string> _availableComponents = new();
        private ObservableCollection<string> _availableVersions = new();
        private ObservableCollection<ServiceViewModel> _services = new();
        private string _selectedComponent = "";
        private string _selectedVersion = "";
        private string _selectedUninstallComponent = "";
        private string _selectedUninstallVersion = "";
        private string _consoleOutput = "";
        private bool _isInstallingComponent = false;
        private bool _isUninstallingComponent = false;
        private bool _isLoadingServices = false;
        private bool _isCreatingSite = false;
        public bool IsInstallingComponent
        {
            get => _isInstallingComponent;
            set { _isInstallingComponent = value; OnPropertyChanged(); }
        }
        public bool IsUninstallingComponent
        {
            get => _isUninstallingComponent;
            set { _isUninstallingComponent = value; OnPropertyChanged(); }
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

        public string SelectedComponent
        {
            get => _selectedComponent;
            set { _selectedComponent = value; OnPropertyChanged(); _ = Task.Run(async () => await GuiInstallTab.LoadVersionsForComponent(this)); }
        }

        public string SelectedVersion
        {
            get => _selectedVersion;
            set { _selectedVersion = value; OnPropertyChanged(); }
        }

        public string SelectedUninstallComponent
        {
            get => _selectedUninstallComponent;
            set { _selectedUninstallComponent = value; OnPropertyChanged(); _ = Task.Run(async () => await GuiUninstallTab.LoadUninstallVersions(this)); }
        }

        public string SelectedUninstallVersion
        {
            get => _selectedUninstallVersion;
            set { _selectedUninstallVersion = value; OnPropertyChanged(); }
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
                var settingsPath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "settings.conf");
                if (System.IO.File.Exists(settingsPath))
                {
                    using (var sr = new StreamReader(settingsPath))
                    using (var reader = new Newtonsoft.Json.JsonTextReader(sr))
                    {
                        string? lang = null;
                        string? theme = null;
                        while (reader.Read())
                        {
                            if (reader.TokenType == Newtonsoft.Json.JsonToken.PropertyName &&
                                reader.Value?.ToString() == "language")
                            {
                                reader.Read();
                                lang = reader.Value?.ToString();
                            }
                            else if (reader.TokenType == Newtonsoft.Json.JsonToken.PropertyName &&
                                     reader.Value?.ToString() == "theme")
                            {
                                reader.Read();
                                theme = reader.Value?.ToString();
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(lang))
                            _localizationManager.LoadLanguage(lang);

                        if (!string.IsNullOrWhiteSpace(theme))
                            DevStackShared.ThemeManager.ApplyTheme(theme.Equals("light", StringComparison.OrdinalIgnoreCase) ? DevStackShared.ThemeManager.ThemeType.Light : DevStackShared.ThemeManager.ThemeType.Dark);
                    }
                }
            }
            catch { }
            _statusMessage = _localizationManager.GetString("gui.window.ready_status");
            _localizationManager.LanguageChanged += OnLanguageChanged;

            // Listener para troca de tema em tempo real
            DevStackShared.ThemeManager.OnThemeChanged += OnThemeChanged;
            
            InitializeComponent();
            DataContext = this;
            
            // Inicializar dados
            _ = Task.Run(async () => await GuiInstalledTab.LoadInstalledComponents(this));
            _ = Task.Run(async () => await GuiServicesTab.LoadServices(this));
        }
        #endregion

        #region Initialization Methods
        private void InitializeComponent()
        {
            var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStackGUI.exe");
            var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? _localizationManager.GetString("gui.common.unknown");
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
                var exePath = System.IO.Path.Combine(System.AppContext.BaseDirectory, "DevStackGUI.exe");
                var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(exePath).FileVersion ?? _localizationManager.GetString("gui.common.unknown");
                Title = _localizationManager.GetString("gui.window.title", version);

                // Recreate main layout while preserving selected index
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
                StatusMessage = _localizationManager.GetString("gui.config_tab.languages.messages.language_changed", _localizationManager.GetLanguageName(newLang));
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

        #region Helper Methods
        public void RefreshAllData()
        {
            _ = Task.Run(async () =>
            {
                await GuiInstalledTab.LoadInstalledComponents(this);
                await GuiServicesTab.LoadServices(this);
            });
        }
        #endregion
    }
}
