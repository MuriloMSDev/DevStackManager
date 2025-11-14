using System;

namespace DevStackShared.AvailableVersions.Models
{
    /// <summary>
    /// Represents comprehensive information about an available version of a DevStack component.
    /// Contains version metadata including download URLs, file integrity hashes, release dates, and file sizes.
    /// Used by version providers to communicate available versions to the installation system.
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// Gets or sets the version number string (e.g., "8.2.0", "20.10.0", "1.21.5").
        /// Must follow semantic versioning or the component's specific versioning scheme.
        /// Used as the unique identifier for this version within the component.
        /// </summary>
        public string Version { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the direct download URL for the version archive or installer.
        /// Must be a valid HTTP/HTTPS URL pointing to the installable package.
        /// Used by the installation system to download the component.
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets additional notes or information about this version (optional).
        /// May contain release notes, breaking changes, or special installation instructions.
        /// Displayed to users during version selection.
        /// </summary>
        public string? Notes { get; set; }
        
        /// <summary>
        /// Gets or sets the download file size in bytes (optional).
        /// Used for progress indication during downloads and disk space validation.
        /// Null if size information is unavailable from the version provider.
        /// </summary>
        public long? FileSize { get; set; }
        
        /// <summary>
        /// Gets or sets the SHA256 hash of the download file for integrity verification (optional).
        /// Used to validate download integrity and detect corrupted files.
        /// Null if hash information is unavailable from the version provider.
        /// </summary>
        public string? Sha256 { get; set; }
        
        /// <summary>
        /// Gets or sets the official release date of this version (optional).
        /// Used for sorting versions by release date and displaying version age.
        /// Null if release date is unavailable from the version provider.
        /// </summary>
        public DateTime? ReleaseDate { get; set; }
        
        /// <summary>
        /// Initializes a new instance of the VersionInfo class with default values.
        /// Creates an empty version info object that must be populated before use.
        /// </summary>
        public VersionInfo()
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the VersionInfo class with required version and URL.
        /// Convenience constructor for creating basic version info with minimal metadata.
        /// </summary>
        /// <param name="version">The version number string.</param>
        /// <param name="url">The download URL for this version.</param>
        public VersionInfo(string version, string url)
        {
            Version = version;
            Url = url;
        }
    }
}
