using System;
using System.Collections.Generic;
using System.Linq;

namespace DevStackShared.Localization
{
    /// <summary>
    /// Factory for creating and managing language provider instances.
    /// Provides centralized registration and retrieval of localization providers for supported languages.
    /// Automatically registers all built-in language providers (Portuguese, English, German, Spanish, French, Italian).
    /// </summary>
    public static class LanguageProviderFactory
    {
        /// <summary>
        /// Dictionary storing language code to language provider mappings.
        /// </summary>
        private static readonly Dictionary<string, ILanguageProvider> _providers = new Dictionary<string, ILanguageProvider>();

        /// <summary>
        /// Static constructor that automatically registers all built-in language providers.
        /// Initializes support for pt_BR, en_US, de_DE, es_ES, fr_FR, and it_IT.
        /// </summary>
        static LanguageProviderFactory()
        {
            RegisterProvider(new pt_BR());
            RegisterProvider(new en_US());
            RegisterProvider(new de_DE());
            RegisterProvider(new es_ES());
            RegisterProvider(new fr_FR());
            RegisterProvider(new it_IT());
        }

        /// <summary>
        /// Registers a language provider with the factory.
        /// Adds or updates the provider for the specified language code.
        /// Silently ignores null providers or providers with empty language codes.
        /// </summary>
        /// <param name="provider">The language provider instance to register. Must have a valid LanguageCode.</param>
        public static void RegisterProvider(ILanguageProvider provider)
        {
            if (provider != null && !string.IsNullOrEmpty(provider.LanguageCode))
            {
                _providers[provider.LanguageCode] = provider;
            }
        }

        /// <summary>
        /// Retrieves a language provider by its language code.
        /// Falls back to Portuguese (pt_BR) if the requested language is not found.
        /// </summary>
        /// <param name="languageCode">The language code to retrieve (e.g., "en_US", "pt_BR").</param>
        /// <returns>The language provider for the specified code, the pt_BR provider as fallback, or null if neither exists.</returns>
        public static ILanguageProvider? GetProvider(string languageCode)
        {
            if (_providers.ContainsKey(languageCode))
            {
                return _providers[languageCode];
            }

            return _providers.ContainsKey("pt_BR") ? _providers["pt_BR"] : null;
        }

        /// <summary>
        /// Returns all available language codes that have registered providers.
        /// </summary>
        /// <returns>Array of language codes (e.g., ["pt_BR", "en_US", "de_DE", ...]).</returns>
        public static string[] GetAvailableLanguages()
        {
            return _providers.Keys.ToArray();
        }

        /// <summary>
        /// Returns all registered language providers with their language codes.
        /// Creates a defensive copy to prevent external modification of the internal provider registry.
        /// </summary>
        /// <returns>Dictionary mapping language codes to their provider instances.</returns>
        public static Dictionary<string, ILanguageProvider> GetAllProviders()
        {
            return new Dictionary<string, ILanguageProvider>(_providers);
        }
    }
}
