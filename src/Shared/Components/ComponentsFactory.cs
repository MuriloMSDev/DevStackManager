
using System;
using System.Collections.Generic;

namespace DevStackManager.Components
{
    /// <summary>
    /// Factory class for creating and retrieving DevStack component instances.
    /// Provides centralized component registration and lookup by name.
    /// </summary>
    public static class ComponentsFactory
    {
        /// <summary>
        /// Dictionary mapping component names to their corresponding implementations.
        /// Uses case-insensitive comparison for component name lookups.
        /// </summary>
        private static readonly Dictionary<string, ComponentInterface> components = new(StringComparer.OrdinalIgnoreCase)
        {
            { "nginx", new NginxComponent() },
            { "php", new PHPComponent() },
            { "mysql", new MySQLComponent() },
            { "node", new NodeComponent() },
            { "python", new PythonComponent() },
            { "composer", new ComposerComponent() },
            { "phpmyadmin", new PhpMyAdminComponent() },
            { "git", new GitComponent() },
            { "mongodb", new MongoDBComponent() },
            { "pgsql", new PgSQLComponent() },
            { "elasticsearch", new ElasticsearchComponent() },
            { "wpcli", new WPCLIComponent() },
            { "adminer", new AdminerComponent() },
            { "go", new GoComponent() },
            { "openssl", new OpenSSLComponent() },
            { "phpcsfixer", new PHPCsFixerComponent() },
            { "dbeaver", new DBeaverComponent() }
        };

        /// <summary>
        /// Retrieves a component instance by name.
        /// </summary>
        /// <param name="name">The name of the component to retrieve (case-insensitive).</param>
        /// <returns>The component instance if found; otherwise, null.</returns>
        public static ComponentInterface? GetComponent(string name)
        {
            components.TryGetValue(name, out var comp);
            return comp;
        }

        /// <summary>
        /// Retrieves all registered component instances.
        /// </summary>
        /// <returns>An enumerable collection of all component instances.</returns>
        public static IEnumerable<ComponentInterface> GetAll()
        {
            return components.Values;
        }
    }
}
