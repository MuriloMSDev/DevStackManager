
using System;
using System.Collections.Generic;

namespace DevStackManager.Components
{
    public static class ComponentsFactory
    {
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
            { "phpcsfixer", new PHPCsFixerComponent() }
        };

        public static ComponentInterface? GetComponent(string name)
        {
            components.TryGetValue(name, out var comp);
            return comp;
        }

        public static IEnumerable<ComponentInterface> GetAll()
        {
            return components.Values;
        }
    }
}
