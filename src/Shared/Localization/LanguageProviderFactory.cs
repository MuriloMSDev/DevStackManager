using System;
using System.Collections.Generic;
using System.Linq;

namespace DevStackShared.Localization
{
    /// <summary>
    /// Factory para criar provedores de idioma
    /// </summary>
    public static class LanguageProviderFactory
    {
        private static readonly Dictionary<string, ILanguageProvider> _providers = new Dictionary<string, ILanguageProvider>();

        static LanguageProviderFactory()
        {
            // Registrar todos os provedores de idioma disponíveis
            RegisterProvider(new pt_BR());
            RegisterProvider(new en_US());
            RegisterProvider(new de_DE());
            RegisterProvider(new es_ES());
            RegisterProvider(new fr_FR());
            RegisterProvider(new it_IT());
        }

        /// <summary>
        /// Registra um provedor de idioma
        /// </summary>
        public static void RegisterProvider(ILanguageProvider provider)
        {
            if (provider != null && !string.IsNullOrEmpty(provider.LanguageCode))
            {
                _providers[provider.LanguageCode] = provider;
            }
        }

        /// <summary>
        /// Obtém um provedor de idioma pelo código
        /// </summary>
        public static ILanguageProvider? GetProvider(string languageCode)
        {
            if (_providers.ContainsKey(languageCode))
            {
                return _providers[languageCode];
            }

            // Fallback para pt_BR se o idioma não for encontrado
            return _providers.ContainsKey("pt_BR") ? _providers["pt_BR"] : null;
        }

        /// <summary>
        /// Retorna todos os códigos de idioma disponíveis
        /// </summary>
        public static string[] GetAvailableLanguages()
        {
            return _providers.Keys.ToArray();
        }

        /// <summary>
        /// Retorna todos os provedores de idioma
        /// </summary>
        public static Dictionary<string, ILanguageProvider> GetAllProviders()
        {
            return new Dictionary<string, ILanguageProvider>(_providers);
        }
    }
}
