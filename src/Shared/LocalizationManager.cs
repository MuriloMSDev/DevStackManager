using System.Text.Json;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Linq;

namespace DevStackShared
{
    /// <summary>
    /// Specifies the type of application for localization context.
    /// </summary>
    public enum ApplicationType
    {
        Installer,
        Uninstaller,
        DevStack
    }

    /// <summary>
    /// Manages application localization with support for multiple languages and JSON translation files.
    /// Provides translation loading from embedded resources, external files, and C# provider classes.
    /// Supports multi-level translation keys with dot notation and fallback mechanisms.
    /// </summary>
    public class LocalizationManager
    {
        /// <summary>
        /// Singleton instance of the LocalizationManager.
        /// </summary>
        private static LocalizationManager? _instance;
        
        /// <summary>
        /// Dictionary storing all loaded translations with multi-level key hierarchy.
        /// </summary>
        private Dictionary<string, object> _translations = new();
        
        /// <summary>
        /// Current active language code (e.g., "en_US", "pt_BR").
        /// </summary>
        private string _currentLanguage = "pt_BR";
        
        /// <summary>
        /// The type of application context for this localization manager.
        /// </summary>
        private readonly ApplicationType _applicationType;
        
        /// <summary>
        /// Path to the log file for diagnostic messages.
        /// </summary>
        private string _logPath = string.Empty;
        
        /// <summary>
        /// Event raised when the language is changed.
        /// </summary>
        public event EventHandler<string>? LanguageChanged;
        
        /// <summary>
        /// Flag to suppress LanguageChanged event during initialization or bulk operations.
        /// </summary>
        private bool _suppressLanguageChangedEvent = false;

        /// <summary>
        /// Static cached copy of the current language code for quick access.
        /// </summary>
        private static string _currentLanguageStatic = "pt_BR";
        
        /// <summary>
        /// Gets or sets the static current language code.
        /// Setting this property invokes the OnLanguageChangedStatic event.
        /// </summary>
        public static string CurrentLanguageStatic 
        { 
            get => _currentLanguageStatic; 
            set 
            { 
                _currentLanguageStatic = value;
                OnLanguageChangedStatic?.Invoke(value);
            } 
        }

        /// <summary>
        /// Gets the current active language code for this LocalizationManager instance.
        /// </summary>
        public string CurrentLanguage => _currentLanguage;
        
        /// <summary>
        /// Gets the application type context for this LocalizationManager.
        /// </summary>
        public ApplicationType ApplicationType => _applicationType;
        
        /// <summary>
        /// Static event raised when the language is changed via CurrentLanguageStatic setter.
        /// </summary>
        public static event Action<string>? OnLanguageChangedStatic;

        /// <summary>
        /// Gets the singleton instance of the LocalizationManager.
        /// </summary>
        public static LocalizationManager? Instance { get; private set; }

        /// <summary>
        /// Private constructor to enforce singleton pattern.
        /// </summary>
        /// <param name="applicationType">Type of application for loading appropriate translation files.</param>
        private LocalizationManager(ApplicationType applicationType)
        {
            _applicationType = applicationType;
            LoadLanguage(_currentLanguage);
        }

        /// <summary>
        /// Initializes the LocalizationManager singleton instance with diagnostic logging.
        /// Scans for embedded translation resources and available language providers.
        /// </summary>
        /// <param name="applicationType">Type of application context for loading appropriate translations.</param>
        /// <returns>The initialized LocalizationManager instance.</returns>
        public static LocalizationManager Initialize(ApplicationType applicationType)
        {
            var errorMessage = "**********************************************************************\n";
            errorMessage += $"Initializing LocalizationManager for {applicationType}\n";
            
            _instance = new LocalizationManager(applicationType);
            Instance = _instance;
            
            _currentLanguageStatic = _instance._currentLanguage;
            
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                errorMessage += $"Assembly name: {assembly.GetName().Name}\n";
                errorMessage += $"Assembly full name: {assembly.FullName}\n";
                
                var resources = assembly.GetManifestResourceNames();
                errorMessage += $"Found {resources.Length} embedded resources:\n";
                
                foreach (var resource in resources)
                {
                    errorMessage += $"  - {resource}\n";
                    
                    try
                    {
                        using var stream = assembly.GetManifestResourceStream(resource);
                        if (stream != null)
                        {
                            errorMessage += $"    Successfully opened stream for {resource}, Length: {stream.Length} bytes\n";
                            
                            if (resource.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    using var reader = new StreamReader(stream);
                                    var content = reader.ReadToEnd();
                                    errorMessage += $"    JSON content length: {content.Length} characters\n";
                                    errorMessage += $"    First 100 chars: {(content.Length > 100 ? content.Substring(0, 100) + "..." : content)}\n";
                                }
                                catch (Exception ex)
                                {
                                    errorMessage += $"    Error reading JSON: {ex.Message}\n";
                                }
                            }
                        }
                        else
                        {
                            errorMessage += $"    Failed to open stream for {resource}\n";
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage += $"    Error accessing resource: {ex.Message}\n";
                    }
                }
                
                string resourceBaseName = $"{assembly.GetName().Name}.locale.pt_BR.";
                errorMessage += $"Expected resource base: {resourceBaseName}\n";
                errorMessage += $"Expected files: common.json, shared.json, gui.json, installer.json, uninstaller.json\n";
                
                string[] expectedFiles = { "common.json", "shared.json", "gui.json", "installer.json", "uninstaller.json" };
                foreach (var fileName in expectedFiles)
                {
                    string resourceName = resourceBaseName + fileName;
                    using var directStream = assembly.GetManifestResourceStream(resourceName);
                    if (directStream != null)
                    {
                        errorMessage += $"Successfully loaded {fileName} resource directly!\n";
                    }
                    else
                    {
                        errorMessage += $"Failed to load {fileName} resource directly!\n";
                    }
                }
                
                try
                {
                    string baseDirectory = AppContext.BaseDirectory;
                    errorMessage += $"Application directory: {baseDirectory}\n";
                    
                    string localeDirectory = Path.Combine(baseDirectory, "locale");
                    if (Directory.Exists(localeDirectory))
                    {
                        errorMessage += $"Locale directory exists on disk: {localeDirectory}\n";
                        var dirs = Directory.GetDirectories(localeDirectory);
                        errorMessage += $"Found {dirs.Length} language directories\n";
                        foreach (var dir in dirs)
                        {
                            string langCode = Path.GetFileName(dir);
                            errorMessage += $"  - {langCode}/\n";
                            
                            try
                            {
                                var files = Directory.GetFiles(dir, "*.json");
                                foreach (var file in files)
                                {
                                    errorMessage += $"    - {Path.GetFileName(file)}\n";
                                }
                            }
                            catch (Exception ex)
                            {
                                errorMessage += $"    Error listing files: {ex.Message}\n";
                            }
                        }
                    }
                    else
                    {
                        errorMessage += $"Locale directory does not exist on disk: {localeDirectory}\n";
                    }
                }
                catch (Exception ex)
                {
                    errorMessage += $"Error checking assembly location: {ex.Message}\n";
                }
            }
            catch (Exception ex)
            {
                errorMessage += $"Error during resource inspection: {ex.Message}\n";
            }
            
            errorMessage += "**********************************************************************";
            
            System.Diagnostics.Debug.WriteLine(errorMessage);
            
            try
            {
                string logPath = Path.Combine(Path.GetTempPath(), $"devstack_localization_{applicationType}.log");
                File.WriteAllText(logPath, errorMessage);
                System.Diagnostics.Debug.WriteLine($"Log written to: {logPath}");
                
                _instance._logPath = logPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write log: {ex.Message}");
            }
            
            return _instance;
        }

        /// <summary>
        /// Loads translations for the specified language code.
        /// Attempts loading from C# providers, embedded resources, external files, or falls back to hardcoded translations.
        /// Fires LanguageChanged event upon successful load.
        /// </summary>
        /// <param name="languageCode">Language code to load (e.g., "pt_BR", "en_US").</param>
        public void LoadLanguage(string languageCode)
        {
            string logMessage = $"[LoadLanguage] Loading language: {languageCode} (current: {_currentLanguage})\n";
            
            try
            {
                if (_currentLanguage == languageCode && _translations.Count > 0)
                {
                    logMessage += $"[LoadLanguage] Language {languageCode} is already loaded and has translations. Forcing reload to ensure UI update.\n";
                    System.Diagnostics.Debug.WriteLine(logMessage);
                    AppendToLogFile(logMessage);
                    
                    _translations.Clear();
                }
                
                _currentLanguage = languageCode;
                
                bool loaded = TryLoadFromProvider(languageCode, ref logMessage);
                
                if (!loaded)
                {
                    logMessage += "[LoadLanguage] Provider not found, using hardcoded basic translations\n";
                    UseHardcodedTranslations(languageCode);
                    loaded = true;
                }
                
                logMessage += $"[LoadLanguage] Successfully loaded language {languageCode}. Translation sections: {string.Join(", ", _translations.Keys)}\n";
                
                System.Diagnostics.Debug.WriteLine(logMessage);
                AppendToLogFile(logMessage);

                if (!_suppressLanguageChangedEvent)
                {
                    logMessage += $"[LoadLanguage] Firing LanguageChanged event for {languageCode}\n";
                    System.Diagnostics.Debug.WriteLine(logMessage);
                    try { LanguageChanged?.Invoke(this, _currentLanguage); } catch { }
                }
                else
                {
                    logMessage += $"[LoadLanguage] LanguageChanged event suppressed for {languageCode}\n";
                    System.Diagnostics.Debug.WriteLine(logMessage);
                }
            }
            catch (Exception ex)
            {
                _translations = new Dictionary<string, object>();
                logMessage += $"[LoadLanguage] Failed to load language {languageCode}: {ex.Message}\n";
                System.Diagnostics.Debug.WriteLine(logMessage);
                AppendToLogFile(logMessage);
                
                UseHardcodedTranslations(languageCode);

                if (!_suppressLanguageChangedEvent)
                {
                    try { LanguageChanged?.Invoke(this, _currentLanguage); } catch { }
                }
            }
        }
        
        /// <summary>
        /// Attempts to load translations from a C# language provider class.
        /// Searches for a class named after the language code in the DevStackShared.Localization namespace.
        /// </summary>
        /// <param name="languageCode">Language code matching the provider class name.</param>
        /// <param name="logMessage">Diagnostic logging message reference.</param>
        /// <returns>True if provider loaded successfully, false otherwise.</returns>
        private bool TryLoadFromProvider(string languageCode, ref string logMessage)
        {
            try
            {
                logMessage += $"[TryLoadFromProvider] Looking for provider for {languageCode}\n";
                
                var assembly = Assembly.GetExecutingAssembly();
                
                logMessage += $"[TryLoadFromProvider] Searching for type: {languageCode}\n";
                
                var providerType = assembly.GetTypes()
                    .FirstOrDefault(t => t.Name.Equals(languageCode, StringComparison.OrdinalIgnoreCase) && 
                                        t.Namespace == "DevStackShared.Localization");
                
                if (providerType != null)
                {
                    logMessage += $"[TryLoadFromProvider] Found provider type: {providerType.FullName}\n";
                    
                    var provider = Activator.CreateInstance(providerType);
                    if (provider != null)
                    {
                        var method = providerType.GetMethod("GetAllTranslations");
                        if (method != null)
                        {
                            var translations = method.Invoke(provider, null) as Dictionary<string, object>;
                            if (translations != null && translations.Count > 0)
                            {
                                _translations = translations;
                                logMessage += $"[TryLoadFromProvider] Successfully loaded {translations.Count} translation sections from provider\n";
                                System.Diagnostics.Debug.WriteLine(logMessage);
                                AppendToLogFile(logMessage);
                                return true;
                            }
                            else
                            {
                                logMessage += $"[TryLoadFromProvider] Provider returned empty or null translations\n";
                            }
                        }
                        else
                        {
                            logMessage += $"[TryLoadFromProvider] GetAllTranslations method not found\n";
                        }
                    }
                    else
                    {
                        logMessage += $"[TryLoadFromProvider] Failed to create instance of provider\n";
                    }
                }
                else
                {
                    logMessage += $"[TryLoadFromProvider] No provider type found for {languageCode}\n";
                }
                
                System.Diagnostics.Debug.WriteLine(logMessage);
                AppendToLogFile(logMessage);
                return false;
            }
            catch (Exception ex)
            {
                logMessage += $"[TryLoadFromProvider] Error loading from provider: {ex.Message}\n{ex.StackTrace}\n";
                System.Diagnostics.Debug.WriteLine(logMessage);
                AppendToLogFile(logMessage);
                return false;
            }
        }
        
        /// <summary>
        /// Attempts to load translations from embedded resources in the assembly.
        /// </summary>
        /// <param name="languageCode">The language code to load translations for.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>True if translations were successfully loaded, false otherwise.</returns>
        private bool TryLoadFromEmbeddedResource(string languageCode, string logMessage)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string assemblyName = assembly.GetName().Name ?? "";
                
                var resources = assembly.GetManifestResourceNames();
                logMessage += $"[TryLoadFromEmbeddedResource] Assembly: {assemblyName}, Found {resources.Length} resources\n";
                foreach (var resource in resources)
                {
                    logMessage += $"[TryLoadFromEmbeddedResource] Resource: {resource}\n";
                }
                
                string[] localeFiles = { "common.json", "shared.json" };
                string[] appSpecificFiles = _applicationType switch
                {
                    ApplicationType.Installer => new[] { "installer.json" },
                    ApplicationType.Uninstaller => new[] { "uninstaller.json" },
                    ApplicationType.DevStack => new[] { "gui.json", "cli.json" },
                    _ => new[] { "gui.json", "cli.json" }
                };
                
                var mergedTranslations = new Dictionary<string, object>();
                bool anyFileLoaded = false;
                
                var langResources = resources.Where(r => r.Contains($".locale.{languageCode}")).ToList();
                logMessage += $"[TryLoadFromEmbeddedResource] Found {langResources.Count} resources for language {languageCode}\n";
                
                foreach (var fileName in localeFiles)
                {
                    var matchingResources = langResources.Where(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matchingResources.Count > 0)
                    {
                        string resourceName = matchingResources[0];
                        logMessage += $"[TryLoadFromEmbeddedResource] Loading resource: {resourceName}\n";
                        
                        using var stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            var fileTranslations = LoadDictionaryFromStream(stream, logMessage);
                            if (fileTranslations != null)
                            {
                                string sectionName = Path.GetFileNameWithoutExtension(fileName);
                                mergedTranslations[sectionName] = fileTranslations;
                                anyFileLoaded = true;
                                
                                logMessage += $"[TryLoadFromEmbeddedResource] Added section '{sectionName}' with {fileTranslations.Count} entries\n";
                            }
                        }
                    }
                    else
                    {
                        logMessage += $"[TryLoadFromEmbeddedResource] No matching resources found for file: {fileName}\n";
                    }
                }
                
                foreach (var appSpecificFile in appSpecificFiles)
                {
                    var appMatchingResources = langResources.Where(r => r.EndsWith(appSpecificFile, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (appMatchingResources.Count > 0)
                    {
                        string resourceName = appMatchingResources[0];
                        logMessage += $"[TryLoadFromEmbeddedResource] Loading app-specific resource: {resourceName}\n";
                        
                        using var stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            var appTranslations = LoadDictionaryFromStream(stream, logMessage);
                            if (appTranslations != null)
                            {
                                string appSectionName = Path.GetFileNameWithoutExtension(appSpecificFile);
                                mergedTranslations[appSectionName] = appTranslations;
                                anyFileLoaded = true;
                                
                                logMessage += $"[TryLoadFromEmbeddedResource] Added app-specific section '{appSectionName}' with {appTranslations.Count} entries\n";
                            }
                        }
                    }
                    else
                    {
                        logMessage += $"[TryLoadFromEmbeddedResource] No matching resources found for app-specific file: {appSpecificFile}\n";
                    }
                }
                
                if (anyFileLoaded)
                {
                    _translations = mergedTranslations;
                    logMessage += $"[TryLoadFromEmbeddedResource] Successfully loaded and merged translations for {languageCode}\n";
                    logMessage += $"[TryLoadFromEmbeddedResource] Root keys: {string.Join(", ", _translations.Keys)}\n";
                    return true;
                }
                
                if (languageCode != "pt_BR")
                {
                    logMessage += "[TryLoadFromEmbeddedResource] Trying fallback to pt_BR\n";
                    _currentLanguage = "pt_BR";
                    return TryLoadFromEmbeddedResource("pt_BR", logMessage);
                }
                
                return false;
            }
            catch (Exception ex)
            {
                logMessage += $"[TryLoadFromEmbeddedResource] Error: {ex.Message}\n";
                return false;
            }
        }
        
        /// <summary>
        /// Attempts to load translations from a specific embedded resource by full resource name.
        /// </summary>
        /// <param name="fullResourceName">The full name of the embedded resource.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>True if translations were successfully loaded, false otherwise.</returns>
        private bool TryLoadFromSpecificEmbeddedResource(string fullResourceName, string logMessage)
        {
            try
            {
                if (fullResourceName.Contains(".locale.") && fullResourceName.EndsWith(".json"))
                {
                    int localeIndex = fullResourceName.IndexOf(".locale.") + 8;
                    int jsonIndex = fullResourceName.LastIndexOf(".json");
                    if (localeIndex < jsonIndex)
                    {
                        string languageCode = fullResourceName.Substring(localeIndex, jsonIndex - localeIndex);
                        logMessage += $"[TryLoadFromSpecificEmbeddedResource] Extracted language code: {languageCode}, trying new structure\n";
                        return TryLoadFromEmbeddedResource(languageCode, logMessage);
                    }
                }
                
                var assembly = Assembly.GetExecutingAssembly();
                
                logMessage += $"[TryLoadFromSpecificEmbeddedResource] Trying to load specific resource: {fullResourceName}\n";
                
                using var stream = assembly.GetManifestResourceStream(fullResourceName);
                
                if (stream != null)
                {
                    logMessage += $"[TryLoadFromSpecificEmbeddedResource] Found specific resource stream\n";
                    return LoadFromStream(stream, logMessage);
                }
                
                logMessage += $"[TryLoadFromSpecificEmbeddedResource] Specific resource not found\n";
                return false;
            }
            catch (Exception ex)
            {
                logMessage += $"[TryLoadFromSpecificEmbeddedResource] Error: {ex.Message}\n";
                return false;
            }
        }
        
        /// <summary>
        /// Attempts to load translations from external JSON files on disk.
        /// </summary>
        /// <param name="languageCode">The language code to load translations for.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>True if translations were successfully loaded, false otherwise.</returns>
        private bool TryLoadFromExternalFile(string languageCode, string logMessage)
        {
            try
            {
                string baseDir = AppContext.BaseDirectory;
                logMessage += $"[TryLoadFromExternalFile] Base directory: {baseDir}\n";
                
                string[] localeFiles = { "common.json", "shared.json" };
                string[] appSpecificFiles = _applicationType switch
                {
                    ApplicationType.Installer => new[] { "installer.json" },
                    ApplicationType.Uninstaller => new[] { "uninstaller.json" },
                    ApplicationType.DevStack => new[] { "gui.json", "cli.json" },
                    _ => new[] { "gui.json", "cli.json" }
                };
                
                string langDir1 = Path.Combine(baseDir, "locale", languageCode);
                logMessage += $"[TryLoadFromExternalFile] Checking language directory: {langDir1}\n";
                
                if (Directory.Exists(langDir1))
                {
                    if (LoadFromLanguageDirectory(langDir1, localeFiles, appSpecificFiles, logMessage))
                    {
                        return true;
                    }
                }
                else
                {
                    logMessage += $"[TryLoadFromExternalFile] Directory does not exist: {langDir1}\n";
                }
                
                string langDir2 = Path.Combine(baseDir, "..", "Shared", "locale", languageCode);
                langDir2 = Path.GetFullPath(langDir2);
                logMessage += $"[TryLoadFromExternalFile] Checking language directory: {langDir2}\n";
                
                if (Directory.Exists(langDir2))
                {
                    if (LoadFromLanguageDirectory(langDir2, localeFiles, appSpecificFiles, logMessage))
                    {
                        return true;
                    }
                }
                else
                {
                    logMessage += $"[TryLoadFromExternalFile] Directory does not exist: {langDir2}\n";
                }
                
                string langDir3 = Path.Combine(baseDir, "src", "Shared", "locale", languageCode);
                langDir3 = Path.GetFullPath(langDir3);
                logMessage += $"[TryLoadFromExternalFile] Checking development language directory: {langDir3}\n";
                
                if (Directory.Exists(langDir3))
                {
                    if (LoadFromLanguageDirectory(langDir3, localeFiles, appSpecificFiles, logMessage))
                    {
                        return true;
                    }
                }
                else
                {
                    logMessage += $"[TryLoadFromExternalFile] Directory does not exist: {langDir3}\n";
                }
                
                string langDir4 = Path.Combine(baseDir, "..", "src", "Shared", "locale", languageCode);
                langDir4 = Path.GetFullPath(langDir4);
                logMessage += $"[TryLoadFromExternalFile] Checking alternative development directory: {langDir4}\n";
                
                if (Directory.Exists(langDir4))
                {
                    if (LoadFromLanguageDirectory(langDir4, localeFiles, appSpecificFiles, logMessage))
                    {
                        return true;
                    }
                }
                else
                {
                    logMessage += $"[TryLoadFromExternalFile] Directory does not exist: {langDir4}\n";
                }
                
                if (languageCode != "pt_BR")
                {
                    logMessage += "[TryLoadFromExternalFile] Trying fallback to pt_BR\n";
                    _currentLanguage = "pt_BR";
                    return TryLoadFromExternalFile("pt_BR", logMessage);
                }
                
                logMessage += "[TryLoadFromExternalFile] No locale files found on disk\n";
                return false;
            }
            catch (Exception ex)
            {
                logMessage += $"[TryLoadFromExternalFile] Error: {ex.Message}\n";
                return false;
            }
        }
        
        /// <summary>
        /// Loads translations from a specific language directory containing JSON files.
        /// </summary>
        /// <param name="languageDir">Path to the language directory.</param>
        /// <param name="localeFiles">Array of locale file names to load.</param>
        /// <param name="appSpecificFiles">Array of application-specific file names to load.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>True if translations were successfully loaded, false otherwise.</returns>
        private bool LoadFromLanguageDirectory(string languageDir, string[] localeFiles, string[] appSpecificFiles, string logMessage)
        {
            try
            {
                var mergedTranslations = new Dictionary<string, object>();
                bool anyFileLoaded = false;
                
                foreach (var fileName in localeFiles)
                {
                    string filePath = Path.Combine(languageDir, fileName);
                    logMessage += $"[LoadFromLanguageDirectory] Checking file: {filePath}\n";
                    
                    if (File.Exists(filePath))
                    {
                        logMessage += $"[LoadFromLanguageDirectory] Found file: {filePath}\n";
                        var fileTranslations = LoadDictionaryFromFile(filePath, logMessage);
                        if (fileTranslations != null)
                        {
                            string sectionName = Path.GetFileNameWithoutExtension(fileName);
                            
                            mergedTranslations[sectionName] = fileTranslations;
                            anyFileLoaded = true;
                            
                            logMessage += $"[LoadFromLanguageDirectory] Added section '{sectionName}' with {fileTranslations.Count} entries\n";
                        }
                    }
                    else
                    {
                        logMessage += $"[LoadFromLanguageDirectory] File not found: {filePath}\n";
                    }
                }
                
                foreach (var appSpecificFile in appSpecificFiles)
                {
                    string appFilePath = Path.Combine(languageDir, appSpecificFile);
                    string appSectionName = Path.GetFileNameWithoutExtension(appSpecificFile);
                    
                    logMessage += $"[LoadFromLanguageDirectory] Checking app-specific file: {appFilePath}\n";
                    
                    if (File.Exists(appFilePath))
                    {
                        logMessage += $"[LoadFromLanguageDirectory] Found app-specific file: {appFilePath}\n";
                        var appTranslations = LoadDictionaryFromFile(appFilePath, logMessage);
                        if (appTranslations != null)
                        {
                            mergedTranslations[appSectionName] = appTranslations;
                            anyFileLoaded = true;
                            
                            logMessage += $"[LoadFromLanguageDirectory] Added app-specific section '{appSectionName}' with {appTranslations.Count} entries\n";
                        }
                    }
                    else
                    {
                        logMessage += $"[LoadFromLanguageDirectory] App-specific file not found: {appFilePath}\n";
                    }
                }
                
                if (anyFileLoaded)
                {
                    _translations = mergedTranslations;
                    logMessage += $"[LoadFromLanguageDirectory] Successfully loaded and merged translations from {languageDir}\n";
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                logMessage += $"[LoadFromLanguageDirectory] Error: {ex.Message}\n";
                return false;
            }
        }
        
        /// <summary>
        /// Loads and deserializes a JSON translation dictionary from a file.
        /// </summary>
        /// <param name="filePath">Path to the JSON file.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>Dictionary containing translations, or null if loading failed.</returns>
        private Dictionary<string, object>? LoadDictionaryFromFile(string filePath, string logMessage)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                logMessage += $"[LoadDictionaryFromFile] Successfully read JSON content from {filePath} (length: {json.Length})\n";
                
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNameCaseInsensitive = true
                };
                
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                logMessage += $"[LoadDictionaryFromFile] Successfully deserialized JSON with {dictionary?.Count ?? 0} root items\n";
                
                return dictionary;
            }
            catch (Exception ex)
            {
                logMessage += $"[LoadDictionaryFromFile] Error: {ex.Message}\n";
                return null;
            }
        }
        
        /// <summary>
        /// Loads and deserializes a JSON translation dictionary from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>Dictionary containing translations, or null if loading failed.</returns>
        private Dictionary<string, object>? LoadDictionaryFromStream(Stream stream, string logMessage)
        {
            try
            {
                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();
                logMessage += $"[LoadDictionaryFromStream] Successfully read JSON content (length: {json.Length})\n";
                
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNameCaseInsensitive = true
                };
                
                var dictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options);
                logMessage += $"[LoadDictionaryFromStream] Successfully deserialized JSON with {dictionary?.Count ?? 0} root items\n";
                
                return dictionary;
            }
            catch (Exception ex)
            {
                logMessage += $"[LoadDictionaryFromStream] Error: {ex.Message}\n";
                return null;
            }
        }
        
        /// <summary>
        /// Recursively merges two translation dictionaries, combining nested objects.
        /// </summary>
        /// <param name="target">The target dictionary to merge into.</param>
        /// <param name="source">The source dictionary to merge from.</param>
        private void MergeDictionaries(Dictionary<string, object> target, Dictionary<string, object> source)
        {
            foreach (var kvp in source)
            {
                if (target.ContainsKey(kvp.Key))
                {
                    if (target[kvp.Key] is JsonElement targetElement && targetElement.ValueKind == JsonValueKind.Object &&
                        kvp.Value is JsonElement sourceElement && sourceElement.ValueKind == JsonValueKind.Object)
                    {
                        var targetDict = JsonSerializer.Deserialize<Dictionary<string, object>>(targetElement.GetRawText()) ?? new Dictionary<string, object>();
                        var sourceDict = JsonSerializer.Deserialize<Dictionary<string, object>>(sourceElement.GetRawText()) ?? new Dictionary<string, object>();
                        MergeDictionaries(targetDict, sourceDict);
                        target[kvp.Key] = JsonSerializer.SerializeToElement(targetDict);
                    }
                    else if (target[kvp.Key] is Dictionary<string, object> targetDict && kvp.Value is Dictionary<string, object> sourceDict)
                    {
                        MergeDictionaries(targetDict, sourceDict);
                    }
                    else
                    {
                        target[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    target[kvp.Key] = kvp.Value;
                }
            }
        }
        
        /// <summary>
        /// Loads translations from a stream containing JSON data.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>True if translations were successfully loaded, false otherwise.</returns>
        private bool LoadFromStream(Stream stream, string logMessage)
        {
            try
            {
                using var reader = new StreamReader(stream);
                string json = reader.ReadToEnd();
                logMessage += $"[LoadFromStream] Successfully read JSON content (length: {json.Length})\n";
                
                return DeserializeJson(json, logMessage);
            }
            catch (Exception ex)
            {
                logMessage += $"[LoadFromStream] Error: {ex.Message}\n";
                return false;
            }
        }
        
        /// <summary>
        /// Loads translations from a JSON file on disk.
        /// </summary>
        /// <param name="filePath">Path to the JSON file.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>True if translations were successfully loaded, false otherwise.</returns>
        private bool LoadFromFile(string filePath, string logMessage)
        {
            try
            {
                string json = File.ReadAllText(filePath);
                logMessage += $"[LoadFromFile] Successfully read JSON content from {filePath} (length: {json.Length})\n";
                
                return DeserializeJson(json, logMessage);
            }
            catch (Exception ex)
            {
                logMessage += $"[LoadFromFile] Error: {ex.Message}\n";
                return false;
            }
        }
        
        /// <summary>
        /// Deserializes JSON string into the translations dictionary.
        /// </summary>
        /// <param name="json">JSON string to deserialize.</param>
        /// <param name="logMessage">Log message string for debugging.</param>
        /// <returns>True if deserialization was successful, false otherwise.</returns>
        private bool DeserializeJson(string json, string logMessage)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    PropertyNameCaseInsensitive = true
                };
                
                _translations = JsonSerializer.Deserialize<Dictionary<string, object>>(json, options) 
                    ?? new Dictionary<string, object>();
                
                logMessage += $"[DeserializeJson] Successfully deserialized JSON with {_translations.Count} root items\n";
                foreach (var key in _translations.Keys)
                {
                    logMessage += $"[DeserializeJson] Root key: {key}, Type: {_translations[key].GetType().Name}\n";
                }
                
                return true;
            }
            catch (Exception ex)
            {
                logMessage += $"[DeserializeJson] Error: {ex.Message}\n";
                return false;
            }
        }
        
        /// <summary>
        /// Fallback method that uses hardcoded translations when loading from resources fails.
        /// </summary>
        /// <param name="languageCode">The language code for which to generate hardcoded translations.</param>
        private void UseHardcodedTranslations(string languageCode)
        {
            _translations = new Dictionary<string, object>();
            
            var buttons = new Dictionary<string, object>
            {
                { "back", "← Voltar" },
                { "next", "Próximo →" },
                { "accept", "Eu Aceito" },
                { "install", "Instalar" },
                { "finish", "Finalizar" },
                { "cancel", "Cancelar" }
            };
            
            var common = new Dictionary<string, object>
            {
                { "language_name", languageCode == "pt_BR" ? "Português (Brasil)" : 
                                   languageCode == "en_US" ? "English (US)" : 
                                   languageCode },
                { "unknown", "Desconhecido" }
            };
            
            _translations["common"] = common;
            
            var appStrings = new Dictionary<string, object>();
            
            var welcome = new Dictionary<string, object>
            {
                { "title", "Welcome" },
                { "description", "This wizard will guide you through the installation process." },
                { "app_name", "DevStack Manager" },
                { "version", "Version {0}" },
                { "language_label", "Language:" }
            };
            
            appStrings["welcome"] = welcome;
            appStrings["window_title"] = "DevStack Manager v{0}";
            
            var shared = new Dictionary<string, object>
            {
                { "unknown", "Desconhecido" }
            };
            _translations["shared"] = shared;

            if (_applicationType == ApplicationType.Installer)
            {
                _translations["installer"] = appStrings;
            }
            else if (_applicationType == ApplicationType.Uninstaller)
            {
                _translations["uninstaller"] = appStrings;
            }
            else
            {
                _translations["gui"] = appStrings;
            }
            
            System.Diagnostics.Debug.WriteLine($"[UseHardcodedTranslations] Created basic translations with {_translations.Count} root keys");
        }
        
        /// <summary>
        /// Appends a message to the localization debug log file.
        /// </summary>
        /// <param name="message">The message to append to the log.</param>
        private void AppendToLogFile(string message)
        {
            if (!string.IsNullOrEmpty(_logPath))
            {
                try
                {
                    File.AppendAllText(_logPath, message);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Retrieves a translated string for the specified key with optional formatting arguments.
        /// Supports dot-notation keys (e.g., "gui.config_tab.title") for nested translations.
        /// Falls back to the key itself if translation is not found.
        /// </summary>
        /// <param name="key">Translation key, potentially with dot notation for nested values.</param>
        /// <param name="args">Optional formatting arguments for string.Format.</param>
        /// <returns>The translated string, or the key if not found.</returns>
        public string GetString(string key, params object[] args)
        {
            string logMessage = $"[GetString] Looking for key '{key}'\n";
            
            try
            {
                object? value = null;
                
                if (key.Contains('.'))
                {
                    string[] parts = key.Split('.');
                    if (parts.Length >= 2)
                    {
                        string section = parts[0];
                        
                        if (section == "gui" || section == "installer" || section == "uninstaller" 
                            || section == "common" || section == "shared")
                        {
                            logMessage += $"[GetString] Processing file-section key '{key}' where '{section}' is a JSON file\n";
                            
                            if (_translations.ContainsKey(section))
                            {
                                logMessage += $"[GetString] Found file section '{section}' in root\n";
                                
                                string subPath = string.Join(".", parts.Skip(1));
                                
                                if (_translations[section] is Dictionary<string, object> sectionDict)
                                {
                                    value = GetNestedValue(sectionDict, subPath);
                                    if (value != null)
                                    {
                                        logMessage += $"[GetString] Found value for subpath '{subPath}' in section '{section}' dictionary\n";
                                    }
                                }
                                else if (_translations[section] is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                                {
                                    try
                                    {
                                        JsonElement currentElement = jsonElement;
                                        bool found = true;
                                        
                                        for (int i = 1; i < parts.Length; i++)
                                        {
                                            string part = parts[i];
                                            if (currentElement.TryGetProperty(part, out JsonElement property))
                                            {
                                                currentElement = property;
                                                
                                                if (i == parts.Length - 1)
                                                {
                                                    value = property;
                                                    logMessage += $"[GetString] Found value at end of path in JsonElement navigation\n";
                                                }
                                            }
                                            else
                                            {
                                                found = false;
                                                logMessage += $"[GetString] Could not find property '{part}' in JsonElement\n";
                                                break;
                                            }
                                        }
                                        
                                        if (found && value != null)
                                        {
                                            logMessage += $"[GetString] Found value for '{key}' in JsonElement navigation\n";
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        logMessage += $"[GetString] Error navigating JsonElement: {ex.Message}\n";
                                    }
                                }
                            }
                            else
                            {
                                logMessage += $"[GetString] File section '{section}' not found in root dictionary\n";
                            }
                        }
                        else if (_translations.ContainsKey(section))
                        {
                            logMessage += $"[GetString] Found section '{section}' in root, trying to access nested key\n";
                            
                            string remainingKey = string.Join(".", parts.Skip(1));
                            
                            if (_translations[section] is Dictionary<string, object> sectionDict)
                            {
                                value = GetNestedValue(sectionDict, remainingKey);
                            }
                            else if (_translations[section] is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                            {
                                try
                                {
                                    JsonElement currentElement = jsonElement;
                                    bool found = true;
                                    
                                    foreach (string part in parts.Skip(1))
                                    {
                                        if (currentElement.TryGetProperty(part, out JsonElement property))
                                        {
                                            currentElement = property;
                                        }
                                        else
                                        {
                                            found = false;
                                            break;
                                        }
                                    }
                                    
                                    if (found)
                                    {
                                        value = currentElement;
                                        logMessage += $"[GetString] Found value for '{key}' in JsonElement navigation\n";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logMessage += $"[GetString] Error navigating JsonElement: {ex.Message}\n";
                                }
                            }
                            
                            if (value != null)
                            {
                                logMessage += $"[GetString] Found value for '{key}' in section '{section}'\n";
                            }
                        }
                    }
                    
                    if (value == null && key.StartsWith("buttons."))
                    {
                        string buttonKey = key.Substring("buttons.".Length);
                        
                        if (_translations.ContainsKey("buttons"))
                        {
                            logMessage += $"[GetString] Looking for button '{buttonKey}' in buttons section\n";
                            
                            if (_translations["buttons"] is Dictionary<string, object> buttonsDict && buttonsDict.ContainsKey(buttonKey))
                            {
                                value = buttonsDict[buttonKey];
                                logMessage += $"[GetString] Found button '{buttonKey}' in buttons dictionary\n";
                            }
                            else if (_translations["buttons"] is JsonElement buttonsElement && 
                                    buttonsElement.ValueKind == JsonValueKind.Object &&
                                    buttonsElement.TryGetProperty(buttonKey, out JsonElement buttonValue))
                            {
                                value = buttonValue;
                                logMessage += $"[GetString] Found button '{buttonKey}' in buttons JsonElement\n";
                            }
                        }
                    }
                    
                    if (value == null && parts.Length >= 3)
                    {
                        string fileSection = parts[0];
                        string secondLevel = parts[1];
                        
                        if (_translations.ContainsKey(fileSection))
                        {
                            logMessage += $"[GetString] Trying multi-level key navigation in '{fileSection}' section\n";
                            
                            if (_translations[fileSection] is Dictionary<string, object> fileSectionDict)
                            {
                                Dictionary<string, object> currentDict = fileSectionDict;
                                bool success = true;
                                object? currentValue = null;
                                
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    string part = parts[i];
                                    
                                    if (currentDict.ContainsKey(part))
                                    {
                                        currentValue = currentDict[part];
                                        
                                        if (i == parts.Length - 1)
                                        {
                                            value = currentValue;
                                            logMessage += $"[GetString] Found value at end of path in '{fileSection}' section\n";
                                            break;
                                        }
                                        
                                        if (currentValue is Dictionary<string, object> nextDict)
                                        {
                                            currentDict = nextDict;
                                        }
                                        else if (currentValue is JsonElement nextElement && nextElement.ValueKind == JsonValueKind.Object)
                                        {
                                            try
                                            {
                                                JsonElement currentElement = nextElement;
                                                bool found = true;
                                                
                                                for (int j = i + 1; j < parts.Length; j++)
                                                {
                                                    if (currentElement.TryGetProperty(parts[j], out JsonElement property))
                                                    {
                                                        currentElement = property;
                                                        
                                                        if (j == parts.Length - 1)
                                                        {
                                                            value = currentElement;
                                                            logMessage += $"[GetString] Found value at end of JsonElement path\n";
                                                        }
                                                    }
                                                    else
                                                    {
                                                        found = false;
                                                        break;
                                                    }
                                                }
                                                
                                                if (found && value != null)
                                                {
                                                    break;
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                logMessage += $"[GetString] Error during JsonElement navigation: {ex.Message}\n";
                                                success = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            success = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                                
                                if (success && value != null)
                                {
                                    logMessage += $"[GetString] Successfully navigated multi-level key '{key}'\n";
                                }
                            }
                            else if (_translations[fileSection] is JsonElement rootElement && rootElement.ValueKind == JsonValueKind.Object)
                            {
                                try
                                {
                                    JsonElement currentElement = rootElement;
                                    
                                    for (int i = 1; i < parts.Length; i++)
                                    {
                                        if (currentElement.TryGetProperty(parts[i], out JsonElement property))
                                        {
                                            currentElement = property;
                                            
                                            if (i == parts.Length - 1)
                                            {
                                                value = currentElement;
                                                logMessage += $"[GetString] Found value at end of JsonElement path in '{fileSection}' section\n";
                                            }
                                        }
                                        else
                                        {
                                            logMessage += $"[GetString] Property '{parts[i]}' not found in JsonElement path\n";
                                            break;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logMessage += $"[GetString] Error during direct JsonElement navigation: {ex.Message}\n";
                                }
                            }
                        }
                    }
                    
                    if (value == null)
                    {
                        logMessage += $"[GetString] Trying direct access for dotted key '{key}'\n";
                        value = GetNestedValue(_translations, key);
                    }
                }
                else
                {
                    
                    logMessage += $"[GetString] Trying direct access for simple key '{key}'\n";
                    if (_translations.ContainsKey(key))
                    {
                        value = _translations[key];
                        logMessage += $"[GetString] Found value for '{key}' at root level\n";
                    }
                    
                    if (value == null && _translations.ContainsKey("common"))
                    {
                        logMessage += $"[GetString] Trying common section for '{key}'\n";
                        
                        if (_translations["common"] is Dictionary<string, object> commonDict && commonDict.ContainsKey(key))
                        {
                            value = commonDict[key];
                            logMessage += $"[GetString] Found value for '{key}' in common section\n";
                        }
                        else if (_translations["common"] is JsonElement commonElement && 
                                commonElement.ValueKind == JsonValueKind.Object &&
                                commonElement.TryGetProperty(key, out JsonElement commonValue))
                        {
                            value = commonValue;
                            logMessage += $"[GetString] Found value for '{key}' in common section (JsonElement)\n";
                        }
                    }
                    
                    if (value == null)
                    {
                        string applicationKey = _applicationType switch
                        {
                            ApplicationType.Installer => "installer",
                            ApplicationType.Uninstaller => "uninstaller",
                            _ => "gui"
                        };
                        
                        if (_translations.ContainsKey(applicationKey))
                        {
                            logMessage += $"[GetString] Trying app-specific section '{applicationKey}' for '{key}'\n";
                            
                            if (_translations[applicationKey] is Dictionary<string, object> appDict && appDict.ContainsKey(key))
                            {
                                value = appDict[key];
                                logMessage += $"[GetString] Found value for '{key}' in app-specific section\n";
                            }
                            else if (_translations[applicationKey] is JsonElement appElement && 
                                    appElement.ValueKind == JsonValueKind.Object &&
                                    appElement.TryGetProperty(key, out JsonElement appValue))
                            {
                                value = appValue;
                                logMessage += $"[GetString] Found value for '{key}' in app-specific section (JsonElement)\n";
                            }
                        }
                    }
                    
                    if (value == null && _translations.ContainsKey("shared"))
                    {
                        logMessage += $"[GetString] Trying shared section for '{key}'\n";
                        
                        if (_translations["shared"] is Dictionary<string, object> sharedDict && sharedDict.ContainsKey(key))
                        {
                            value = sharedDict[key];
                            logMessage += $"[GetString] Found value for '{key}' in shared section\n";
                        }
                        else if (_translations["shared"] is JsonElement sharedElement && 
                                sharedElement.ValueKind == JsonValueKind.Object &&
                                sharedElement.TryGetProperty(key, out JsonElement sharedValue))
                        {
                            value = sharedValue;
                            logMessage += $"[GetString] Found value for '{key}' in shared section (JsonElement)\n";
                        }
                    }
                    
                    if (value == null && _translations.ContainsKey("buttons"))
                    {
                        logMessage += $"[GetString] Trying buttons section for '{key}'\n";
                        
                        if (_translations["buttons"] is Dictionary<string, object> buttonsDict && buttonsDict.ContainsKey(key))
                        {
                            value = buttonsDict[key];
                            logMessage += $"[GetString] Found value for '{key}' in buttons section\n";
                        }
                        else if (_translations["buttons"] is JsonElement buttonsElement && 
                                buttonsElement.ValueKind == JsonValueKind.Object &&
                                buttonsElement.TryGetProperty(key, out JsonElement buttonValue))
                        {
                            value = buttonValue;
                            logMessage += $"[GetString] Found value for '{key}' in buttons section (JsonElement)\n";
                        }
                    }
                    
                    if (value == null)
                    {
                        string commonKey = $"common.{key}";
                        logMessage += $"[GetString] Trying old-style common section '{commonKey}'\n";
                        value = GetNestedValue(_translations, commonKey);
                        
                        if (value == null)
                        {
                            string applicationKey = _applicationType switch
                            {
                                ApplicationType.Installer => "installer",
                                ApplicationType.Uninstaller => "uninstaller",
                                _ => "gui"
                            };
                            string appKey = $"{applicationKey}.{key}";
                            logMessage += $"[GetString] Trying old-style app-specific key '{appKey}'\n";
                            value = GetNestedValue(_translations, appKey);
                        }
                        
                        if (value == null)
                        {
                            string sharedKey = $"shared.{key}";
                            logMessage += $"[GetString] Trying old-style shared section '{sharedKey}'\n";
                            value = GetNestedValue(_translations, sharedKey);
                        }
                        
                        if (value == null)
                        {
                            string buttonsKey = $"buttons.{key}";
                            logMessage += $"[GetString] Trying old-style buttons section '{buttonsKey}'\n";
                            value = GetNestedValue(_translations, buttonsKey);
                        }
                    }
                }
                
                if (value != null)
                {
                    logMessage += $"[GetString] Found value for '{key}'\n";
                    
                    string result;
                    if (value is JsonElement jsonElement)
                    {
                        if (jsonElement.ValueKind == JsonValueKind.String)
                        {
                            result = jsonElement.GetString() ?? key;
                        }
                        else
                        {
                            result = jsonElement.ToString() ?? key;
                            logMessage += $"[GetString] Warning: Value for '{key}' is not a string but {jsonElement.ValueKind}\n";
                        }
                    }
                    else if (value is Dictionary<string, object> dict)
                    {
                        result = key;
                        logMessage += $"[GetString] Warning: Value for '{key}' is a dictionary, not a string\n";
                    }
                    else
                    {
                        result = value.ToString() ?? key;
                    }
                    
                    if (args.Length > 0)
                    {
                        try
                        {
                            result = string.Format(result, args);
                        }
                        catch (FormatException ex)
                        {
                            logMessage += $"[GetString] Format error for '{key}': {ex.Message}\n";
                        }
                    }
                    
                    AppendToLogFile(logMessage);
                    return result;
                }
                else
                {
                    logMessage += $"[GetString] *** MISSING TRANSLATION: '{key}' ***\n";
                    logMessage += $"[GetString] Available root keys: {string.Join(", ", _translations.Keys)}\n";
                    
                    if (_translations.Count > 0)
                    {
                        logMessage += "[GetString] Translation structure:\n";
                        foreach (var rootKey in _translations.Keys)
                        {
                            logMessage += $"[GetString]   - {rootKey}: {_translations[rootKey]?.GetType().Name ?? "null"}\n";
                            
                            if (_translations[rootKey] is JsonElement rootElement && 
                                rootElement.ValueKind == JsonValueKind.Object)
                            {
                                try
                                {
                                    logMessage += "[GetString]     Properties:\n";
                                    foreach (var prop in rootElement.EnumerateObject())
                                    {
                                        logMessage += $"[GetString]       - {prop.Name}: {prop.Value.ValueKind}\n";
                                    }
                                }
                                catch (Exception ex)
                                {
                                    logMessage += $"[GetString]     Error enumerating properties: {ex.Message}\n";
                                }
                            }
                            else if (_translations[rootKey] is Dictionary<string, object> rootDict)
                            {
                                logMessage += "[GetString]     Properties:\n";
                                foreach (var prop in rootDict.Keys)
                                {
                                    logMessage += $"[GetString]       - {prop}\n";
                                }
                            }
                        }
                    }
                    
                    AppendToLogFile(logMessage);
                    return key;
                }
            }
            catch (Exception ex)
            {
                logMessage += $"[GetString] Error looking up '{key}': {ex.Message}\n";
                AppendToLogFile(logMessage);
                return key;
            }
        }

        /// <summary>
        /// Retrieves a nested value from a dictionary hierarchy using dot-notation key.
        /// Supports both Dictionary and JsonElement objects.
        /// </summary>
        /// <param name="dict">Root dictionary to search.</param>
        /// <param name="key">Dot-separated key path (e.g., "section.subsection.key").</param>
        /// <returns>The nested value if found, null otherwise.</returns>
        private object? GetNestedValue(Dictionary<string, object> dict, string key)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Looking for nested key '{key}'");
                
                if (dict.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("GetNestedValue: Dictionary is empty");
                    return null;
                }
                
                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Dictionary has {dict.Count} root keys: {string.Join(", ", dict.Keys)}");
                
                string[] keyParts = key.Split('.');
                object current = dict;
                
                foreach (string keyPart in keyParts)
                {
                    System.Diagnostics.Debug.WriteLine($"GetNestedValue: Looking for part '{keyPart}'");
                    
                    if (current is Dictionary<string, object> currentDict)
                    {
                        if (currentDict.ContainsKey(keyPart))
                        {
                            System.Diagnostics.Debug.WriteLine($"GetNestedValue: Found '{keyPart}' in Dictionary");
                            current = currentDict[keyPart];
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"GetNestedValue: Key '{keyPart}' not found in Dictionary. Available keys: {string.Join(", ", currentDict.Keys)}");
                            return null;
                        }
                    }
                    else if (current is JsonElement jsonElement)
                    {
                        if (jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            if (jsonElement.TryGetProperty(keyPart, out JsonElement property))
                            {
                                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Found '{keyPart}' in JsonElement");
                                current = property;
                            }
                            else
                            {
                                try 
                                {
                                    var availableKeys = new List<string>();
                                    foreach (var prop in jsonElement.EnumerateObject())
                                    {
                                        availableKeys.Add(prop.Name);
                                    }
                                    System.Diagnostics.Debug.WriteLine($"GetNestedValue: Key '{keyPart}' not found in JsonElement. Available keys: {string.Join(", ", availableKeys)}");
                                }
                                catch 
                                {
                                    System.Diagnostics.Debug.WriteLine($"GetNestedValue: Key '{keyPart}' not found in JsonElement");
                                }
                                return null;
                            }
                        }
                        else if (jsonElement.ValueKind == JsonValueKind.Array)
                        {
                            if (int.TryParse(keyPart, out int index) && index >= 0)
                            {
                                try
                                {
                                    var elements = jsonElement.EnumerateArray().ToArray();
                                    if (index < elements.Length)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"GetNestedValue: Found array index {index} in JsonElement");
                                        current = elements[index];
                                    }
                                    else
                                    {
                                        System.Diagnostics.Debug.WriteLine($"GetNestedValue: Array index {index} out of range (0-{elements.Length - 1})");
                                        return null;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"GetNestedValue: Error accessing array element: {ex.Message}");
                                    return null;
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Cannot access non-numeric key '{keyPart}' in array");
                                return null;
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"GetNestedValue: JsonElement is not an object or array: {jsonElement.ValueKind}");
                            return null;
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"GetNestedValue: Current object is not a dictionary or JsonElement: {current?.GetType().Name ?? "null"}");
                        return null;
                    }
                }
                
                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Successfully found value for key '{key}'");
                return current;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Error looking up '{key}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets an array of available language codes by scanning for C# provider classes.
        /// Falls back to a default list if no providers are found.
        /// </summary>
        /// <returns>Array of language codes (e.g., ["pt_BR", "en_US", "de_DE"]).</returns>
        public string[] GetAvailableLanguages()
        {
            try
            {
                HashSet<string> allLanguages = new HashSet<string>();
                
                try
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var providerTypes = assembly.GetTypes()
                        .Where(t => t.Namespace == "DevStackShared.Localization" && 
                                   !t.IsInterface && 
                                   !t.IsAbstract &&
                                   t.Name != "LanguageProviderFactory")
                        .ToArray();
                    
                    foreach (var providerType in providerTypes)
                    {
                        allLanguages.Add(providerType.Name);
                        System.Diagnostics.Debug.WriteLine($"Found language provider: {providerType.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error checking for language providers: {ex.Message}");
                }
                
                if (allLanguages.Count == 0)
                {
                    allLanguages = new HashSet<string> { "pt_BR", "en_US", "de_DE", "es_ES", "fr_FR", "it_IT" };
                    System.Diagnostics.Debug.WriteLine("No providers found, using default language list");
                }
                
                var result = allLanguages.OrderBy(l => l).ToArray();
                System.Diagnostics.Debug.WriteLine($"Final available languages: {string.Join(", ", result)}");
                
                return result.Length > 0 ? result : new[] { "pt_BR" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAvailableLanguages: {ex.Message}");
                return new[] { "pt_BR" };
            }
        }

        /// <summary>
        /// Gets the display name for a language code (e.g., "Português (Brasil)" for "pt_BR").
        /// Uses a default mapping if translation loading fails.
        /// </summary>
        /// <param name="languageCode">Language code to get name for.</param>
        /// <returns>The display name for the language.</returns>
        public string GetLanguageName(string languageCode)
        {
            System.Diagnostics.Debug.WriteLine($"[GetLanguageName] Getting language name for {languageCode}");
            
            Dictionary<string, string> defaultNames = new Dictionary<string, string>
            {
                { "pt_BR", "Português (Brasil)" },
                { "en_US", "English (US)" },
                { "de_DE", "Deutsch" },
                { "es_ES", "Español" },
                { "fr_FR", "Français" },
                { "it_IT", "Italiano" }
            };
            
            try
            {
                if (defaultNames.ContainsKey(languageCode))
                {
                    System.Diagnostics.Debug.WriteLine($"[GetLanguageName] Using default name for {languageCode}: {defaultNames[languageCode]}");
                    return defaultNames[languageCode];
                }
                
                System.Diagnostics.Debug.WriteLine($"[GetLanguageName] No default name found for {languageCode}, returning code itself");
                return languageCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetLanguageName] Error getting language name: {ex.Message}");
                
                if (defaultNames.ContainsKey(languageCode))
                {
                    return defaultNames[languageCode];
                }
                
                return languageCode;
            }
        }

        /// <summary>
        /// Applies a language change at runtime, updating the static language and firing events.
        /// Reloads translations for the specified language with fallback to Portuguese (Brazil) on error.
        /// </summary>
        /// <param name="languageCode">Language code to apply.</param>
        public static void ApplyLanguage(string languageCode)
        {
            string logMessage = $"[ApplyLanguage] Applying language: {languageCode}\n";
            try
            {
                if (_currentLanguageStatic != languageCode)
                {
                    _currentLanguageStatic = languageCode;
                    logMessage += $"[ApplyLanguage] Language set to: {_currentLanguageStatic}\n";
                    
                    if (Instance != null)
                    {
                        Instance._suppressLanguageChangedEvent = false;
                        Instance.LoadLanguage(languageCode);
                    }
                    
                    OnLanguageChangedStatic?.Invoke(languageCode);
                }
                System.Diagnostics.Debug.WriteLine(logMessage);
                Instance?.AppendToLogFile(logMessage);
            }
            catch (Exception ex)
            {
                logMessage += $"[ApplyLanguage] Failed to apply language {languageCode}: {ex.Message}\n";
                System.Diagnostics.Debug.WriteLine(logMessage);
                Instance?.AppendToLogFile(logMessage);
                
                _currentLanguageStatic = "pt_BR";
                OnLanguageChangedStatic?.Invoke(_currentLanguageStatic);
            }
        }
    }
}
