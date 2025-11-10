using System.Text.Json;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Linq;

namespace DevStackShared
{
    public enum ApplicationType
    {
        Installer,
        Uninstaller,
        DevStack  // Usado tanto para CLI quanto para GUI
    }

    public class LocalizationManager
    {
        private static LocalizationManager? _instance;
        private Dictionary<string, object> _translations = new();
        private string _currentLanguage = "pt_BR";
        private readonly ApplicationType _applicationType;
        private string _logPath = string.Empty;
        
        public event EventHandler<string>? LanguageChanged;
        private bool _suppressLanguageChangedEvent = false;

        private static string _currentLanguageStatic = "pt_BR";
        public static string CurrentLanguageStatic 
        { 
            get => _currentLanguageStatic; 
            set 
            { 
                _currentLanguageStatic = value;
                OnLanguageChangedStatic?.Invoke(value);
            } 
        }

        public string CurrentLanguage => _currentLanguage;
        public ApplicationType ApplicationType => _applicationType;
        public static event Action<string>? OnLanguageChangedStatic;

        public static LocalizationManager? Instance { get; private set; }

        // Private constructor to prevent direct instantiation
        private LocalizationManager(ApplicationType applicationType)
        {
            _applicationType = applicationType;
            LoadLanguage(_currentLanguage);
        }

        // Static method to create and initialize the instance
        public static LocalizationManager Initialize(ApplicationType applicationType)
        {
            // Debug information
            var errorMessage = "**********************************************************************\n";
            errorMessage += $"Initializing LocalizationManager for {applicationType}\n";
            
            // Create an instance with basic initialization
            _instance = new LocalizationManager(applicationType);
            Instance = _instance;
            
            // Inicializar idioma estático com o valor da instância
            _currentLanguageStatic = _instance._currentLanguage;
            
            // Log detailed information about embedded resources
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
                    
                    // Try to load the resource
                    try
                    {
                        using var stream = assembly.GetManifestResourceStream(resource);
                        if (stream != null)
                        {
                            errorMessage += $"    Successfully opened stream for {resource}, Length: {stream.Length} bytes\n";
                            
                            // If this is a JSON file, try reading its content
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
                
                // Display expected resource path for the current language
                string resourceBaseName = $"{assembly.GetName().Name}.locale.pt_BR.";
                errorMessage += $"Expected resource base: {resourceBaseName}\n";
                errorMessage += $"Expected files: common.json, shared.json, gui.json, installer.json, uninstaller.json\n";
                
                // Try to directly load pt_BR resources to see if they're available
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
                
                // Check application directory for locale folders
                try
                {
                    // Use AppContext.BaseDirectory as the reliable way to get the app's directory 
                    // in both single-file and traditional deployments
                    string baseDirectory = AppContext.BaseDirectory;
                    errorMessage += $"Application directory: {baseDirectory}\n";
                    
                    // Check if locale language directories exist on disk
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
                            
                            // List files in each language directory
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
            
            // Write to debug output
            System.Diagnostics.Debug.WriteLine(errorMessage);
            
            // Also create a log file
            try
            {
                string logPath = Path.Combine(Path.GetTempPath(), $"devstack_localization_{applicationType}.log");
                File.WriteAllText(logPath, errorMessage);
                System.Diagnostics.Debug.WriteLine($"Log written to: {logPath}");
                
                // Set _logPath for further logging
                _instance._logPath = logPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write log: {ex.Message}");
            }
            
            return _instance;
        }

        public void LoadLanguage(string languageCode)
        {
            string logMessage = $"[LoadLanguage] Loading language: {languageCode} (current: {_currentLanguage})\n";
            
            try
            {
                // Check if we're already using this language
                if (_currentLanguage == languageCode && _translations.Count > 0)
                {
                    logMessage += $"[LoadLanguage] Language {languageCode} is already loaded and has translations. Forcing reload to ensure UI update.\n";
                    System.Diagnostics.Debug.WriteLine(logMessage);
                    AppendToLogFile(logMessage);
                    
                    // Clear existing translations to force a reload
                    _translations.Clear();
                }
                
                _currentLanguage = languageCode;
                
                // NOVA ABORDAGEM: Usar provedores de idioma em C# (primeira tentativa)
                bool loaded = TryLoadFromProvider(languageCode, ref logMessage);
                
                // Fallback para traduções básicas hardcoded se o provedor não estiver disponível
                if (!loaded)
                {
                    logMessage += "[LoadLanguage] Provider not found, using hardcoded basic translations\n";
                    UseHardcodedTranslations(languageCode);
                    loaded = true;
                }
                
                logMessage += $"[LoadLanguage] Successfully loaded language {languageCode}. Translation sections: {string.Join(", ", _translations.Keys)}\n";
                
                // Escrever log para saída de depuração e arquivo
                System.Diagnostics.Debug.WriteLine(logMessage);
                AppendToLogFile(logMessage);

                // Always fire the language changed event
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
                // Se tudo falhar, crie um dicionário vazio para evitar falhas
                _translations = new Dictionary<string, object>();
                logMessage += $"[LoadLanguage] Failed to load language {languageCode}: {ex.Message}\n";
                System.Diagnostics.Debug.WriteLine(logMessage);
                AppendToLogFile(logMessage);
                
                // Garantir que temos pelo menos traduções básicas
                UseHardcodedTranslations(languageCode);

                if (!_suppressLanguageChangedEvent)
                {
                    try { LanguageChanged?.Invoke(this, _currentLanguage); } catch { }
                }
            }
        }
        
        /// <summary>
        /// Tenta carregar traduções de um provedor C# (nova abordagem)
        /// </summary>
        private bool TryLoadFromProvider(string languageCode, ref string logMessage)
        {
            try
            {
                logMessage += $"[TryLoadFromProvider] Looking for provider for {languageCode}\n";
                
                // Procurar por classe de provedor usando reflection
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
                        // Tentar obter método GetAllTranslations
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
        
        private bool TryLoadFromEmbeddedResource(string languageCode, string logMessage)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string assemblyName = assembly.GetName().Name ?? "";
                
                // Log embedded resources for debugging
                var resources = assembly.GetManifestResourceNames();
                logMessage += $"[TryLoadFromEmbeddedResource] Assembly: {assemblyName}, Found {resources.Length} resources\n";
                foreach (var resource in resources)
                {
                    logMessage += $"[TryLoadFromEmbeddedResource] Resource: {resource}\n";
                }
                
                // List of files to load for each language
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
                
                // Find all resources for the specified language code
                var langResources = resources.Where(r => r.Contains($".locale.{languageCode}")).ToList();
                logMessage += $"[TryLoadFromEmbeddedResource] Found {langResources.Count} resources for language {languageCode}\n";
                
                // Try to load each known file type
                foreach (var fileName in localeFiles)
                {
                    // Look for resources that match the pattern and end with the file name
                    var matchingResources = langResources.Where(r => r.EndsWith(fileName, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (matchingResources.Count > 0)
                    {
                        string resourceName = matchingResources[0]; // Use the first matching resource
                        logMessage += $"[TryLoadFromEmbeddedResource] Loading resource: {resourceName}\n";
                        
                        using var stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            var fileTranslations = LoadDictionaryFromStream(stream, logMessage);
                            if (fileTranslations != null)
                            {
                                // Add as separate section based on filename
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
                
                // Load application-specific files (pode haver múltiplos para DevStack)
                foreach (var appSpecificFile in appSpecificFiles)
                {
                    var appMatchingResources = langResources.Where(r => r.EndsWith(appSpecificFile, StringComparison.OrdinalIgnoreCase)).ToList();
                    if (appMatchingResources.Count > 0)
                    {
                        string resourceName = appMatchingResources[0]; // Use the first matching resource
                        logMessage += $"[TryLoadFromEmbeddedResource] Loading app-specific resource: {resourceName}\n";
                        
                        using var stream = assembly.GetManifestResourceStream(resourceName);
                        if (stream != null)
                        {
                            var appTranslations = LoadDictionaryFromStream(stream, logMessage);
                            if (appTranslations != null)
                            {
                                // Add app section with its own key
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
                
                // Try fallback to Portuguese (Brazil)
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
        
        private bool TryLoadFromSpecificEmbeddedResource(string fullResourceName, string logMessage)
        {
            try
            {
                // This method is used as a fallback for the old naming convention
                // We'll try to extract the language code from the full resource name and use the new method
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
        
        private bool TryLoadFromExternalFile(string languageCode, string logMessage)
        {
            try
            {
                // Use AppContext.BaseDirectory como a maneira confiável de obter o diretório do aplicativo
                string baseDir = AppContext.BaseDirectory;
                logMessage += $"[TryLoadFromExternalFile] Base directory: {baseDir}\n";
                
                // Lista de arquivos para carregar para cada idioma
                string[] localeFiles = { "common.json", "shared.json" };
                string[] appSpecificFiles = _applicationType switch
                {
                    ApplicationType.Installer => new[] { "installer.json" },
                    ApplicationType.Uninstaller => new[] { "uninstaller.json" },
                    ApplicationType.DevStack => new[] { "gui.json", "cli.json" },
                    _ => new[] { "gui.json", "cli.json" }
                };
                
                // Tenta: ./locale/[lang]/
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
                
                // Tenta: ../Shared/locale/[lang]/ (relativo ao executável)
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
                
                // Tenta: ./src/Shared/locale/[lang]/ (ambiente de desenvolvimento)
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
                
                // Adicional: tenta o diretório raiz do workspace diretamente em desenvolvimento
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
                
                // Se não encontrado e não pt_BR, tenta com pt_BR
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
        
        private bool LoadFromLanguageDirectory(string languageDir, string[] localeFiles, string[] appSpecificFiles, string logMessage)
        {
            try
            {
                var mergedTranslations = new Dictionary<string, object>();
                bool anyFileLoaded = false;
                
                // Load common files first
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
                            // Get the section name from filename (without extension)
                            string sectionName = Path.GetFileNameWithoutExtension(fileName);
                            
                            // Add the translations to the root dictionary with the section name as the key
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
                
                // Load application-specific files (pode haver múltiplos para DevStack)
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
                            // Add the application-specific translations with the proper section name
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
                
                // Return the dictionary directly for the new file structure
                return dictionary;
            }
            catch (Exception ex)
            {
                logMessage += $"[LoadDictionaryFromFile] Error: {ex.Message}\n";
                return null;
            }
        }
        
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
                
                // Return the dictionary directly for the new file structure
                return dictionary;
            }
            catch (Exception ex)
            {
                logMessage += $"[LoadDictionaryFromStream] Error: {ex.Message}\n";
                return null;
            }
        }
        
        private void MergeDictionaries(Dictionary<string, object> target, Dictionary<string, object> source)
        {
            foreach (var kvp in source)
            {
                if (target.ContainsKey(kvp.Key))
                {
                    // If both values are dictionaries, merge them recursively
                    if (target[kvp.Key] is JsonElement targetElement && targetElement.ValueKind == JsonValueKind.Object &&
                        kvp.Value is JsonElement sourceElement && sourceElement.ValueKind == JsonValueKind.Object)
                    {
                        // For JsonElement dictionaries, we need to convert them to regular dictionaries first
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
                        // Overwrite with source value
                        target[kvp.Key] = kvp.Value;
                    }
                }
                else
                {
                    target[kvp.Key] = kvp.Value;
                }
            }
        }
        
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
        
        private void UseHardcodedTranslations(string languageCode)
        {
            // Create basic translations in memory as a last resort
            _translations = new Dictionary<string, object>();
            
            // Common buttons (always needed)
            var buttons = new Dictionary<string, object>
            {
                { "back", "← Voltar" },
                { "next", "Próximo →" },
                { "accept", "Eu Aceito" },
                { "install", "Instalar" },
                { "finish", "Finalizar" },
                { "cancel", "Cancelar" }
            };
            
            // Common items
            var common = new Dictionary<string, object>
            {
                { "language_name", languageCode == "pt_BR" ? "Português (Brasil)" : 
                                   languageCode == "en_US" ? "English (US)" : 
                                   languageCode },
                { "unknown", "Desconhecido" }
            };
            
            // Add to root dictionary as sections
            _translations["common"] = common;
            
            // App-specific strings (minimal set)
            var appStrings = new Dictionary<string, object>();
            
            // Add welcome section with basic items
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
            
            // Add shared section
            var shared = new Dictionary<string, object>
            {
                { "unknown", "Desconhecido" }
            };
            _translations["shared"] = shared;

            // Add app-specific section based on application type
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
                    // Ignore errors when writing to log file
                }
            }
        }

        public string GetString(string key, params object[] args)
        {
            string logMessage = $"[GetString] Looking for key '{key}'\n";
            
            try
            {
                object? value = null;
                
                // With the new merged structure, handle dotted and non-dotted keys
                if (key.Contains('.'))
                {
                    // For dot-notation keys like "common.unknown" or "buttons.next"
                    string[] parts = key.Split('.');
                    if (parts.Length >= 2)
                    {
                        string section = parts[0];
                        
                        // CRITICAL: Special handling for JSON file sections like "gui.config_tab.themes.title"
                        // In this case, "gui" is the actual name of a JSON file (gui.json) 
                        // that has been loaded into the translations dictionary
                        if (section == "gui" || section == "installer" || section == "uninstaller" 
                            || section == "common" || section == "shared")
                        {
                            logMessage += $"[GetString] Processing file-section key '{key}' where '{section}' is a JSON file\n";
                            
                            // Try to get the file section from root
                            if (_translations.ContainsKey(section))
                            {
                                logMessage += $"[GetString] Found file section '{section}' in root\n";
                                
                                // For example, in "gui.config_tab.themes.title", get "config_tab.themes.title"
                                string subPath = string.Join(".", parts.Skip(1));
                                
                                if (_translations[section] is Dictionary<string, object> sectionDict)
                                {
                                    // Navigate through the section dictionary with the subpath
                                    value = GetNestedValue(sectionDict, subPath);
                                    if (value != null)
                                    {
                                        logMessage += $"[GetString] Found value for subpath '{subPath}' in section '{section}' dictionary\n";
                                    }
                                }
                                else if (_translations[section] is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                                {
                                    // Special handling for multi-level keys in JsonElement
                                    try
                                    {
                                        JsonElement currentElement = jsonElement;
                                        bool found = true;
                                        
                                        // Navigate through the remaining parts of the key after the section
                                        for (int i = 1; i < parts.Length; i++)
                                        {
                                            string part = parts[i];
                                            if (currentElement.TryGetProperty(part, out JsonElement property))
                                            {
                                                currentElement = property;
                                                
                                                // If this is the last part, we found the value
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
                        // Handle other section types normally
                        else if (_translations.ContainsKey(section))
                        {
                            logMessage += $"[GetString] Found section '{section}' in root, trying to access nested key\n";
                            
                            // Extract the remaining key parts after the section
                            string remainingKey = string.Join(".", parts.Skip(1));
                            
                            if (_translations[section] is Dictionary<string, object> sectionDict)
                            {
                                value = GetNestedValue(sectionDict, remainingKey);
                            }
                            else if (_translations[section] is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                            {
                                // Special handling for multi-level keys in JsonElement
                                try
                                {
                                    JsonElement currentElement = jsonElement;
                                    bool found = true;
                                    
                                    // Navigate through each part of the key
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
                    
                    // Special handling for common button patterns (buttons.xxx)
                    if (value == null && key.StartsWith("buttons."))
                    {
                        string buttonKey = key.Substring("buttons.".Length);
                        
                        // Look for root-level buttons object
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
                    
                    // Special handling for file-specific sections (like gui.config_tab.themes.title)
                    if (value == null && parts.Length >= 3)
                    {
                        string fileSection = parts[0]; // For example "gui"
                        string secondLevel = parts[1]; // For example "config_tab"
                        
                        // Try to navigate directly in the JSON structure
                        if (_translations.ContainsKey(fileSection))
                        {
                            logMessage += $"[GetString] Trying multi-level key navigation in '{fileSection}' section\n";
                            
                            if (_translations[fileSection] is Dictionary<string, object> fileSectionDict)
                            {
                                // Try to navigate through all remaining parts
                                Dictionary<string, object> currentDict = fileSectionDict;
                                bool success = true;
                                object? currentValue = null;
                                
                                // Start from the second part (skip the file section name)
                                for (int i = 1; i < parts.Length; i++)
                                {
                                    string part = parts[i];
                                    
                                    if (currentDict.ContainsKey(part))
                                    {
                                        currentValue = currentDict[part];
                                        
                                        // If this is the last part, we've found our value
                                        if (i == parts.Length - 1)
                                        {
                                            value = currentValue;
                                            logMessage += $"[GetString] Found value at end of path in '{fileSection}' section\n";
                                            break;
                                        }
                                        
                                        // If not the last part, the value should be another dictionary
                                        if (currentValue is Dictionary<string, object> nextDict)
                                        {
                                            currentDict = nextDict;
                                        }
                                        else if (currentValue is JsonElement nextElement && nextElement.ValueKind == JsonValueKind.Object)
                                        {
                                            // If it's a JsonElement, we need to change our approach
                                            try
                                            {
                                                JsonElement currentElement = nextElement;
                                                bool found = true;
                                                
                                                // Continue from the current position in the path
                                                for (int j = i + 1; j < parts.Length; j++)
                                                {
                                                    if (currentElement.TryGetProperty(parts[j], out JsonElement property))
                                                    {
                                                        currentElement = property;
                                                        
                                                        // If this is the last part, we've found our value
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
                                                    break; // We found the value, exit the outer loop
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
                                            // Not a dictionary or object JsonElement, can't continue navigation
                                            success = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        // Key not found at this level
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
                                // Direct JsonElement navigation for all parts
                                try
                                {
                                    JsonElement currentElement = rootElement;
                                    
                                    // Start from the second part
                                    for (int i = 1; i < parts.Length; i++)
                                    {
                                        if (currentElement.TryGetProperty(parts[i], out JsonElement property))
                                        {
                                            currentElement = property;
                                            
                                            // If this is the last part, we've found our value
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
                    
                    // If still not found, try direct access as fallback
                    if (value == null)
                    {
                        logMessage += $"[GetString] Trying direct access for dotted key '{key}'\n";
                        value = GetNestedValue(_translations, key);
                    }
                }
                else
                {
                    // For simple keys without dots, try various sections
                    
                    // Try direct access first (might be a root key)
                    logMessage += $"[GetString] Trying direct access for simple key '{key}'\n";
                    if (_translations.ContainsKey(key))
                    {
                        value = _translations[key];
                        logMessage += $"[GetString] Found value for '{key}' at root level\n";
                    }
                    
                    // Try common section first (most likely for simple keys)
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
                    
                    // Try application-specific section
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
                    
                    // Try shared section
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
                    
                    // Try buttons section as a special case for common UI elements
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
                    
                    // If still not found, try with the old nested access method
                    if (value == null)
                    {
                        // Try common section
                        string commonKey = $"common.{key}";
                        logMessage += $"[GetString] Trying old-style common section '{commonKey}'\n";
                        value = GetNestedValue(_translations, commonKey);
                        
                        // Try application-specific section
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
                        
                        // Try shared section
                        if (value == null)
                        {
                            string sharedKey = $"shared.{key}";
                            logMessage += $"[GetString] Trying old-style shared section '{sharedKey}'\n";
                            value = GetNestedValue(_translations, sharedKey);
                        }
                        
                        // Try buttons section
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
                            // Not a string value
                            result = jsonElement.ToString() ?? key;
                            logMessage += $"[GetString] Warning: Value for '{key}' is not a string but {jsonElement.ValueKind}\n";
                        }
                    }
                    else if (value is Dictionary<string, object> dict)
                    {
                        // Value is a dictionary, not a string
                        result = key;
                        logMessage += $"[GetString] Warning: Value for '{key}' is a dictionary, not a string\n";
                    }
                    else
                    {
                        result = value.ToString() ?? key;
                    }
                    
                    // Format if needed
                    if (args.Length > 0)
                    {
                        try
                        {
                            result = string.Format(result, args);
                        }
                        catch (FormatException ex)
                        {
                            logMessage += $"[GetString] Format error for '{key}': {ex.Message}\n";
                            // Return original if formatting fails
                        }
                    }
                    
                    AppendToLogFile(logMessage);
                    return result;
                }
                else
                {
                    // Special case for debugging - log missing translations in detail
                    logMessage += $"[GetString] *** MISSING TRANSLATION: '{key}' ***\n";
                    logMessage += $"[GetString] Available root keys: {string.Join(", ", _translations.Keys)}\n";
                    
                    // Print translation dictionary structure for debugging
                    if (_translations.Count > 0)
                    {
                        logMessage += "[GetString] Translation structure:\n";
                        foreach (var rootKey in _translations.Keys)
                        {
                            logMessage += $"[GetString]   - {rootKey}: {_translations[rootKey]?.GetType().Name ?? "null"}\n";
                            
                            // For top-level dictionaries, show second level keys
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
                return key; // Return the key itself if translation fails
            }
        }

        private object? GetNestedValue(Dictionary<string, object> dict, string key)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Looking for nested key '{key}'");
                
                // Special case for empty dictionaries
                if (dict.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("GetNestedValue: Dictionary is empty");
                    return null;
                }
                
                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Dictionary has {dict.Count} root keys: {string.Join(", ", dict.Keys)}");
                
                // Split the key by dots
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
                        // Check the value kind before attempting to access properties
                        if (jsonElement.ValueKind == JsonValueKind.Object)
                        {
                            if (jsonElement.TryGetProperty(keyPart, out JsonElement property))
                            {
                                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Found '{keyPart}' in JsonElement");
                                current = property;
                            }
                            else
                            {
                                // Print available properties for debugging
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
                            // Handle array indexing if the key part is a number
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

        public string[] GetAvailableLanguages()
        {
            try
            {
                // Coletar idiomas disponíveis a partir dos provedores C#
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
                        // O nome da classe é o código do idioma (ex: pt_BR, en_US)
                        allLanguages.Add(providerType.Name);
                        System.Diagnostics.Debug.WriteLine($"Found language provider: {providerType.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error checking for language providers: {ex.Message}");
                }
                
                // Se nenhum provedor foi encontrado, retornar lista padrão
                if (allLanguages.Count == 0)
                {
                    allLanguages = new HashSet<string> { "pt_BR", "en_US", "de_DE", "es_ES", "fr_FR", "it_IT" };
                    System.Diagnostics.Debug.WriteLine("No providers found, using default language list");
                }
                
                // Ordenar idiomas e remover duplicatas
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

        public string GetLanguageName(string languageCode)
        {
            System.Diagnostics.Debug.WriteLine($"[GetLanguageName] Getting language name for {languageCode}");
            
            // Mapa de nomes de idiomas padrão caso a tradução falhe
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
                // Para evitar chamadas recursivas e interferências, vamos usar apenas os nomes padrão por enquanto
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
                
                // Em caso de erro, use os nomes padrão
                if (defaultNames.ContainsKey(languageCode))
                {
                    return defaultNames[languageCode];
                }
                
                return languageCode;
            }
        }

        /// <summary>
        /// Aplica um idioma em tempo real, dispara evento e faz fallback se necessário
        /// Implementação idêntica ao ApplyTheme do ThemeManager
        /// </summary>
        public static void ApplyLanguage(string languageCode)
        {
            string logMessage = $"[ApplyLanguage] Applying language: {languageCode}\n";
            try
            {
                if (_currentLanguageStatic != languageCode)
                {
                    _currentLanguageStatic = languageCode;
                    logMessage += $"[ApplyLanguage] Language set to: {_currentLanguageStatic}\n";
                    
                    // Chama LoadLanguage na instância se existir
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
                
                // Fallback para idioma padrão
                _currentLanguageStatic = "pt_BR";
                OnLanguageChangedStatic?.Invoke(_currentLanguageStatic);
            }
        }
    }
}
