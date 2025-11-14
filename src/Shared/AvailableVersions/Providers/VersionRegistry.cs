using System;
using System.Collections.Generic;
using System.Linq;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Central registry for all version providers in DevStack.
    /// Manages registration and lookup of version providers for all supported components.
    /// Automatically registers all built-in version providers during static initialization.
    /// Provides a unified interface for querying version information across all components.
    /// </summary>
    public static class VersionRegistry
    {
        /// <summary>
        /// Dictionary storing component ID to version provider mappings.
        /// </summary>
        private static readonly Dictionary<string, IVersionProvider> _providers = new Dictionary<string, IVersionProvider>();
        
        /// <summary>
        /// Static constructor that automatically registers all built-in version providers.
        /// Initializes providers for Adminer, Composer, DBeaver, Elasticsearch, Git, Go, MongoDB,
        /// MySQL, Nginx, Node.js, OpenSSL, PostgreSQL, PHP, PHP-CS-Fixer, phpMyAdmin, Python, and WP-CLI.
        /// </summary>
        static VersionRegistry()
        {
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
        /// Registers a version provider in the registry.
        /// Adds or updates the provider for the specified component ID.
        /// Silently ignores null providers or providers with empty component IDs.
        /// </summary>
        /// <param name="provider">The version provider instance to register. Must have a valid ComponentId.</param>
        private static void RegisterProvider(IVersionProvider provider)
        {
            if (provider != null && !string.IsNullOrEmpty(provider.ComponentId))
            {
                _providers[provider.ComponentId] = provider;
            }
        }
        
        /// <summary>
        /// Retrieves a version provider by component ID.
        /// Performs case-sensitive lookup of the component identifier.
        /// </summary>
        /// <param name="componentId">The unique component identifier (e.g., "php", "node", "mysql").</param>
        /// <returns>The version provider for the specified component, or null if not registered.</returns>
        public static IVersionProvider? GetProvider(string componentId)
        {
            return _providers.ContainsKey(componentId) ? _providers[componentId] : null;
        }
        
        /// <summary>
        /// Retrieves all available versions for a specific component.
        /// Delegates to the component's version provider to fetch version information.
        /// </summary>
        /// <param name="componentId">The unique component identifier (e.g., "php", "node", "mysql").</param>
        /// <returns>List of available versions with download URLs and metadata, or empty list if component not found.</returns>
        public static List<VersionInfo> GetAvailableVersions(string componentId)
        {
            var provider = GetProvider(componentId);
            return provider?.GetAvailableVersions() ?? new List<VersionInfo>();
        }
        
        /// <summary>
        /// Retrieves the latest available version for a specific component.
        /// Useful for "install latest" operations and automatic version detection.
        /// </summary>
        /// <param name="componentId">The unique component identifier (e.g., "php", "node", "mysql").</param>
        /// <returns>VersionInfo for the latest version, or null if component not found or no versions available.</returns>
        public static VersionInfo? GetLatestVersion(string componentId)
        {
            var provider = GetProvider(componentId);
            return provider?.GetLatestVersion();
        }
        
        /// <summary>
        /// Retrieves information for a specific version of a component.
        /// Searches the component's available versions for an exact version match.
        /// </summary>
        /// <param name="componentId">The unique component identifier (e.g., "php", "node", "mysql").</param>
        /// <param name="version">The version number to search for (e.g., "8.2.5", "20.10.0").</param>
        /// <returns>VersionInfo for the specified version, or null if component or version not found.</returns>
        public static VersionInfo? GetVersion(string componentId, string version)
        {
            var provider = GetProvider(componentId);
            return provider?.GetVersion(version);
        }
        
        /// <summary>
        /// Returns all registered component IDs in the registry.
        /// Useful for enumerating supported components and validating component names.
        /// </summary>
        /// <returns>Array of component identifiers (e.g., ["php", "node", "mysql", ...]).</returns>
        public static string[] GetRegisteredComponents()
        {
            return _providers.Keys.ToArray();
        }
        
        /// <summary>
        /// Returns all registered version providers with their component IDs.
        /// Creates a defensive copy to prevent external modification of the internal registry.
        /// Useful for debugging and administrative interfaces that need full provider access.
        /// </summary>
        /// <returns>Dictionary mapping component IDs to their version provider instances.</returns>
        public static Dictionary<string, IVersionProvider> GetAllProviders()
        {
            return new Dictionary<string, IVersionProvider>(_providers);
        }
    }
}
