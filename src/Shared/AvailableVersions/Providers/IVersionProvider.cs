using System.Collections.Generic;
using DevStackShared.AvailableVersions.Models;

namespace DevStackShared.AvailableVersions.Providers
{
    /// <summary>
    /// Interface para provedores de versões disponíveis de componentes
    /// </summary>
    public interface IVersionProvider
    {
        /// <summary>
        /// Nome do componente (ex: "PHP", "Node.js", "MySQL")
        /// </summary>
        string ComponentName { get; }
        
        /// <summary>
        /// Identificador único do componente (ex: "php", "node", "mysql")
        /// </summary>
        string ComponentId { get; }
        
        /// <summary>
        /// Retorna todas as versões disponíveis do componente
        /// </summary>
        List<VersionInfo> GetAvailableVersions();
        
        /// <summary>
        /// Retorna a versão mais recente disponível
        /// </summary>
        VersionInfo? GetLatestVersion();
        
        /// <summary>
        /// Busca uma versão específica pelo número
        /// </summary>
        VersionInfo? GetVersion(string version);
    }
}
