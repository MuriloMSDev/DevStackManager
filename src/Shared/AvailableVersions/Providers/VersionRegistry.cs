using System;
using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Registro central de todos os provedores de versões
    /// </summary>
    public static class VersionRegistry
    {
        private static readonly Dictionary<string, IVersionProvider> _providers = new Dictionary<string, IVersionProvider>();
        
        static VersionRegistry()
        {
            // Registrar todos os provedores
            RegisterProvider(new AdminerVersionProvider());
            RegisterProvider(new ComposerVersionProvider());
            RegisterProvider(new DbeaverVersionProvider());
            RegisterProvider(new ElasticsearchVersionProvider());
            RegisterProvider(new GitVersionProvider());
            RegisterProvider(new GoVersionProvider());
            RegisterProvider(new MongodbVersionProvider());
            RegisterProvider(new MysqlVersionProvider());
            RegisterProvider(new NginxVersionProvider());
            RegisterProvider(new NodeVersionProvider());
            RegisterProvider(new OpensslVersionProvider());
            RegisterProvider(new PgsqlVersionProvider());
            RegisterProvider(new PhpVersionProvider());
            RegisterProvider(new PhpcsfixerVersionProvider());
            RegisterProvider(new PhpmyadminVersionProvider());
            RegisterProvider(new PythonVersionProvider());
            RegisterProvider(new WpcliVersionProvider());
        }
        
        /// <summary>
        /// Registra um provedor de versões
        /// </summary>
        private static void RegisterProvider(IVersionProvider provider)
        {
            if (provider != null && !string.IsNullOrEmpty(provider.ComponentId))
            {
                _providers[provider.ComponentId] = provider;
            }
        }
        
        /// <summary>
        /// Obtém um provedor pelo ID do componente
        /// </summary>
        public static IVersionProvider? GetProvider(string componentId)
        {
            return _providers.ContainsKey(componentId) ? _providers[componentId] : null;
        }
        
        /// <summary>
        /// Obtém todas as versões disponíveis de um componente
        /// </summary>
        public static List<VersionInfo> GetAvailableVersions(string componentId)
        {
            var provider = GetProvider(componentId);
            return provider?.GetAvailableVersions() ?? new List<VersionInfo>();
        }
        
        /// <summary>
        /// Obtém a versão mais recente de um componente
        /// </summary>
        public static VersionInfo? GetLatestVersion(string componentId)
        {
            var provider = GetProvider(componentId);
            return provider?.GetLatestVersion();
        }
        
        /// <summary>
        /// Obtém uma versão específica de um componente
        /// </summary>
        public static VersionInfo? GetVersion(string componentId, string version)
        {
            var provider = GetProvider(componentId);
            return provider?.GetVersion(version);
        }
        
        /// <summary>
        /// Retorna todos os IDs de componentes registrados
        /// </summary>
        public static string[] GetRegisteredComponents()
        {
            return _providers.Keys.ToArray();
        }
        
        /// <summary>
        /// Retorna todos os provedores registrados
        /// </summary>
        public static Dictionary<string, IVersionProvider> GetAllProviders()
        {
            return new Dictionary<string, IVersionProvider>(_providers);
        }
    }
}
