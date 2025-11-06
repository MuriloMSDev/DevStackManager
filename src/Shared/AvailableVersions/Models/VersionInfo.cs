using System;

namespace DevStackShared.AvailableVersions.Models
{
    /// <summary>
    /// Representa informações sobre uma versão disponível de um componente
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Número da versão (ex: "8.2.0", "20.10.0")
        /// </summary>
        public string Version { get; set; } = string.Empty;
        
        /// <summary>
        /// URL para download da versão
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// Informações adicionais sobre a versão (opcional)
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Tamanho do arquivo em bytes (opcional)
        /// </summary>
        public long? FileSize { get; set; }
        
        /// <summary>
        /// Hash SHA256 do arquivo (opcional)
        /// </summary>
        public string? Sha256 { get; set; }
        
        /// <summary>
        /// Data de lançamento (opcional)
        /// </summary>
        public DateTime? ReleaseDate { get; set; }
        
        public VersionInfo()
        {
        }
        
        public VersionInfo(string version, string url)
        {
            Version = version;
            Url = url;
        }
    }
}
