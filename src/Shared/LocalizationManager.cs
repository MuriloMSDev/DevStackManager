using System.Text.Json;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace DevStackShared
{
    public enum ApplicationType
    {
        Installer,
        Uninstaller,
        GUI
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
                string resourceName = $"{assembly.GetName().Name}.locale.pt_BR.json";
                errorMessage += $"Expected resource path: {resourceName}\n";
                
                // Try to directly load pt_BR.json to see if it's available
                using var directStream = assembly.GetManifestResourceStream(resourceName);
                if (directStream != null)
                {
                    errorMessage += $"Successfully loaded default language resource directly!\n";
                }
                else
                {
                    errorMessage += $"Failed to load default language resource directly!\n";
                }
                
                // Check application directory for locale files
                try
                {
                    // Use AppContext.BaseDirectory as the reliable way to get the app's directory 
                    // in both single-file and traditional deployments
                    string baseDirectory = AppContext.BaseDirectory;
                    errorMessage += $"Application directory: {baseDirectory}\n";
                    
                    // Check if locale files exist on disk
                    string localeDirectory = Path.Combine(baseDirectory, "locale");
                    if (Directory.Exists(localeDirectory))
                    {
                        errorMessage += $"Locale directory exists on disk: {localeDirectory}\n";
                        var files = Directory.GetFiles(localeDirectory, "*.json");
                        errorMessage += $"Found {files.Length} JSON files in locale directory\n";
                        foreach (var file in files)
                        {
                            errorMessage += $"  - {Path.GetFileName(file)}\n";
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
            string logMessage = $"[LoadLanguage] Loading language: {languageCode}\n";
            
            try
            {
                _currentLanguage = languageCode;
                
                // Try multiple approaches to load the language file
                
                // 1. First try - embedded resource with full application name
                bool loaded = TryLoadFromEmbeddedResource(languageCode, logMessage);
                
                // 2. Second try - direct path using the assembly name but with specific LogicalName pattern 
                if (!loaded)
                {
                    string appPrefix = _applicationType == ApplicationType.Installer ? 
                        "DevStackInstaller" : (_applicationType == ApplicationType.Uninstaller ? "DevStackUninstaller" : "DevStackManager");
                    logMessage += $"[LoadLanguage] Trying with app prefix: {appPrefix}\n";
                    loaded = TryLoadFromSpecificEmbeddedResource($"{appPrefix}.locale.{languageCode}.json", logMessage);
                }
                
                // 3. Third try - external file in locale subfolder
                if (!loaded)
                {
                    logMessage += "[LoadLanguage] Trying to load from external file\n";
                    loaded = TryLoadFromExternalFile(languageCode, logMessage);
                }
                
                // 4. Fourth try - hardcoded basic translations
                if (!loaded)
                {
                    logMessage += "[LoadLanguage] Using hardcoded basic translations\n";
                    UseHardcodedTranslations(languageCode);
                    loaded = true;
                }
                
                // Write log to debug output and file
                System.Diagnostics.Debug.WriteLine(logMessage);
                AppendToLogFile(logMessage);

                if (!_suppressLanguageChangedEvent)
                {
                    try { LanguageChanged?.Invoke(this, _currentLanguage); } catch { }
                }
            }
            catch (Exception ex)
            {
                // If all else fails, create empty dictionary to prevent crashes
                _translations = new Dictionary<string, object>();
                logMessage += $"[LoadLanguage] Failed to load language {languageCode}: {ex.Message}\n";
                System.Diagnostics.Debug.WriteLine(logMessage);
                AppendToLogFile(logMessage);
                
                // Ensure we have at least basic translations
                UseHardcodedTranslations(languageCode);

                if (!_suppressLanguageChangedEvent)
                {
                    try { LanguageChanged?.Invoke(this, _currentLanguage); } catch { }
                }
            }
        }
        
        private bool TryLoadFromEmbeddedResource(string languageCode, string logMessage)
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string assemblyName = assembly.GetName().Name ?? "";
                string resourcePrefix = $"{assemblyName}.locale.";
                string resourceName = $"{resourcePrefix}{languageCode}.json";
                
                logMessage += $"[TryLoadFromEmbeddedResource] Trying to load resource: {resourceName}\n";
                
                using var stream = assembly.GetManifestResourceStream(resourceName);
                
                if (stream != null)
                {
                    logMessage += $"[TryLoadFromEmbeddedResource] Found resource stream: {resourceName}\n";
                    return LoadFromStream(stream, logMessage);
                }
                
                logMessage += $"[TryLoadFromEmbeddedResource] Resource not found: {resourceName}\n";
                
                // Try fallback to Portuguese (Brazil)
                if (languageCode != "pt_BR")
                {
                    logMessage += "[TryLoadFromEmbeddedResource] Trying fallback to pt_BR\n";
                    resourceName = $"{resourcePrefix}pt_BR.json";
                    using var fallbackStream = assembly.GetManifestResourceStream(resourceName);
                    
                    if (fallbackStream != null)
                    {
                        logMessage += "[TryLoadFromEmbeddedResource] Found fallback resource\n";
                        _currentLanguage = "pt_BR";
                        return LoadFromStream(fallbackStream, logMessage);
                    }
                    
                    logMessage += "[TryLoadFromEmbeddedResource] Fallback resource not found\n";
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
                // Use AppContext.BaseDirectory as the reliable way to get the app's directory
                string baseDir = AppContext.BaseDirectory;
                logMessage += $"[TryLoadFromExternalFile] Base directory: {baseDir}\n";
                
                // Try: ./locale/[lang].json
                string path1 = Path.Combine(baseDir, "locale", $"{languageCode}.json");
                logMessage += $"[TryLoadFromExternalFile] Checking path: {path1}\n";
                
                if (File.Exists(path1))
                {
                    logMessage += $"[TryLoadFromExternalFile] Found locale file: {path1}\n";
                    return LoadFromFile(path1, logMessage);
                }
                
                // Try: ../Shared/locale/[lang].json (relative to executable)
                string path2 = Path.Combine(baseDir, "..", "Shared", "locale", $"{languageCode}.json");
                path2 = Path.GetFullPath(path2);
                logMessage += $"[TryLoadFromExternalFile] Checking path: {path2}\n";
                
                if (File.Exists(path2))
                {
                    logMessage += $"[TryLoadFromExternalFile] Found locale file: {path2}\n";
                    return LoadFromFile(path2, logMessage);
                }
                
                // If not found and not pt_BR, try with pt_BR
                if (languageCode != "pt_BR")
                {
                    logMessage += "[TryLoadFromExternalFile] Trying fallback to pt_BR\n";
                    string fallbackPath1 = Path.Combine(baseDir, "locale", "pt_BR.json");
                    
                    if (File.Exists(fallbackPath1))
                    {
                        logMessage += $"[TryLoadFromExternalFile] Found fallback file: {fallbackPath1}\n";
                        _currentLanguage = "pt_BR";
                        return LoadFromFile(fallbackPath1, logMessage);
                    }
                    
                    string fallbackPath2 = Path.Combine(baseDir, "..", "Shared", "locale", "pt_BR.json");
                    fallbackPath2 = Path.GetFullPath(fallbackPath2);
                    
                    if (File.Exists(fallbackPath2))
                    {
                        logMessage += $"[TryLoadFromExternalFile] Found fallback file: {fallbackPath2}\n";
                        _currentLanguage = "pt_BR";
                        return LoadFromFile(fallbackPath2, logMessage);
                    }
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
                                   languageCode }
            };
            
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
            
            // Add to root
            _translations["buttons"] = buttons;
            _translations["common"] = common;

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
                // First try to get from application-specific section
                string applicationKey = _applicationType == ApplicationType.Installer ? "installer" : (_applicationType == ApplicationType.Uninstaller ? "uninstaller" : "gui");
                string fullKey = $"{applicationKey}.{key}";
                
                logMessage += $"[GetString] Trying with app-specific key '{fullKey}'\n";
                var value = GetNestedValue(_translations, fullKey);
                
                // If not found in application-specific section, try direct access for sections
                if (value == null && key.Contains('.'))
                {
                    logMessage += $"[GetString] App-specific key not found, trying direct section '{key}'\n";
                    value = GetNestedValue(_translations, key);
                }
                
                // If not found and it's a path with only one level, try global section
                if (value == null && !key.Contains('.'))
                {
                    logMessage += $"[GetString] Trying global section for '{key}'\n";
                    value = GetNestedValue(_translations, key);
                }
                
                // Try global buttons section
                if (value == null && !key.Contains('.'))
                {
                    string buttonKey = $"buttons.{key}";
                    logMessage += $"[GetString] Trying buttons section '{buttonKey}'\n";
                    value = GetNestedValue(_translations, buttonKey);
                }
                
                // Try common section
                if (value == null && !key.Contains('.'))
                {
                    string commonKey = $"common.{key}";
                    logMessage += $"[GetString] Trying common section '{commonKey}'\n";
                    value = GetNestedValue(_translations, commonKey);
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
                string[] keyParts = key.Split('.');
                object current = dict;
                
                // Debug the initial state of the dictionary
                if (dict.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("GetNestedValue: Dictionary is empty");
                    return null;
                }
                
                System.Diagnostics.Debug.WriteLine($"GetNestedValue: Dictionary has {dict.Count} root keys: {string.Join(", ", dict.Keys)}");
                
                foreach (string k in keyParts)
                {
                    System.Diagnostics.Debug.WriteLine($"GetNestedValue: Looking for part '{k}'");
                    
                    if (current is Dictionary<string, object> currentDict)
                    {
                        if (currentDict.ContainsKey(k))
                        {
                            System.Diagnostics.Debug.WriteLine($"GetNestedValue: Found '{k}' in Dictionary");
                            current = currentDict[k];
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"GetNestedValue: Key '{k}' not found in Dictionary. Available keys: {string.Join(", ", currentDict.Keys)}");
                            return null;
                        }
                    }
                    else if (current is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        if (jsonElement.TryGetProperty(k, out JsonElement property))
                        {
                            System.Diagnostics.Debug.WriteLine($"GetNestedValue: Found '{k}' in JsonElement");
                            current = property;
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"GetNestedValue: Key '{k}' not found in JsonElement");
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
                // First try to get languages from embedded resources
                var assembly = Assembly.GetExecutingAssembly();
                string assemblyName = assembly.GetName().Name ?? "";
                
                // Log assembly information for debugging
                System.Diagnostics.Debug.WriteLine($"Assembly name: {assemblyName}");
                
                // Try to list resources, but this might not work in single-file apps
                string[] resourceNames = new string[0];
                try
                {
                    var allResources = assembly.GetManifestResourceNames();
                    System.Diagnostics.Debug.WriteLine($"Found {allResources.Length} embedded resources");
                    
                    resourceNames = allResources
                        .Where(name => name.Contains(".locale.") && name.EndsWith(".json"))
                        .Select(name => {
                            int start = name.IndexOf(".locale.") + 8;
                            int end = name.Length - 5; // subtract ".json"
                            return name.Substring(start, end - start);
                        })
                        .ToArray();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error accessing embedded resources: {ex.Message}");
                }
                
                if (resourceNames.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Found embedded language resources: {string.Join(", ", resourceNames)}");
                    return resourceNames;
                }
                
                // If no embedded resources found, look for external files
                string localeDir = Path.Combine(AppContext.BaseDirectory, "locale");
                if (Directory.Exists(localeDir))
                {
                    var files = Directory.GetFiles(localeDir, "*.json");
                    var fileLanguages = files
                        .Select(file => Path.GetFileNameWithoutExtension(file) ?? "")
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToArray();
                    
                    if (fileLanguages.Length > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found external language files: {string.Join(", ", fileLanguages)}");
                        return fileLanguages;
                    }
                }
                
                // If nothing found, return default language
                return new[] { "pt_BR" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetAvailableLanguages: {ex.Message}");
                return new[] { "pt_BR" };
            }
        }

        public string GetLanguageName(string languageCode)
        {
            var currentLang = _currentLanguage;
            _suppressLanguageChangedEvent = true;
            LoadLanguage(languageCode);
            string name = GetString("language_name");
            LoadLanguage(currentLang); // Restore current language
            _suppressLanguageChangedEvent = false;
            return name;
        }

        public string CurrentLanguage => _currentLanguage;
        public ApplicationType ApplicationType => _applicationType;
    }
}
