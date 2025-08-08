using System.Text.Json;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace DevStackInstaller
{
    public class LocalizationManager
    {
        private static LocalizationManager? _instance;
        private Dictionary<string, object> _translations = new();
        private string _currentLanguage = "pt_BR";

        public static LocalizationManager Instance => _instance ??= new LocalizationManager();

        private LocalizationManager()
        {
            LoadLanguage(_currentLanguage);
        }

        public void LoadLanguage(string languageCode)
        {
            try
            {
                _currentLanguage = languageCode;
                string resourceName = $"DevStackInstaller.locale.{languageCode}.json";
                
                var assembly = Assembly.GetExecutingAssembly();
                using var stream = assembly.GetManifestResourceStream(resourceName);
                
                if (stream == null)
                {
                    // Fallback to English if language not found
                    resourceName = "DevStackInstaller.locale.pt_BR.json";
                    using var fallbackStream = assembly.GetManifestResourceStream(resourceName);
                    if (fallbackStream != null)
                    {
                        using var reader = new StreamReader(fallbackStream);
                        string json = reader.ReadToEnd();
                        _translations = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                    }
                    _currentLanguage = "pt_BR";
                }
                else
                {
                    using var reader = new StreamReader(stream);
                    string json = reader.ReadToEnd();
                    _translations = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
                }
            }
            catch (Exception ex)
            {
                // If all else fails, create empty dictionary to prevent crashes
                _translations = new Dictionary<string, object>();
                System.Diagnostics.Debug.WriteLine($"Failed to load language {languageCode}: {ex.Message}");
            }
        }

        public string GetString(string key, params object[] args)
        {
            try
            {
                var value = GetNestedValue(_translations, key);
                if (value is JsonElement jsonElement)
                {
                    string text = jsonElement.GetString() ?? key;
                    return args.Length > 0 ? string.Format(text, args) : text;
                }
                return value?.ToString() ?? key;
            }
            catch
            {
                return key; // Return the key itself if translation fails
            }
        }

        private object? GetNestedValue(Dictionary<string, object> dict, string key)
        {
            string[] keys = key.Split('.');
            object current = dict;

            foreach (string k in keys)
            {
                if (current is Dictionary<string, object> currentDict && currentDict.ContainsKey(k))
                {
                    current = currentDict[k];
                }
                else if (current is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Object)
                {
                    if (jsonElement.TryGetProperty(k, out JsonElement property))
                    {
                        current = property;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }

            return current;
        }

        public string[] GetAvailableLanguages()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith("DevStackInstaller.locale.") && name.EndsWith(".json"))
                .Select(name => name.Replace("DevStackInstaller.locale.", "").Replace(".json", ""))
                .ToArray();

            return resourceNames.Length > 0 ? resourceNames : new[] { "pt_BR" };
        }

        public string GetLanguageName(string languageCode)
        {
            var currentLang = _currentLanguage;
            LoadLanguage(languageCode);
            string name = GetString("language_name");
            LoadLanguage(currentLang); // Restore current language
            return name;
        }

        public string CurrentLanguage => _currentLanguage;
    }
}
