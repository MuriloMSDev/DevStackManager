using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Represents version listing data for a specific DevStack component.
    /// Contains available versions, installation status, and display configuration for version tables and lists.
    /// Used by ListManager for rendering formatted version output in CLI and GUI interfaces.
    /// </summary>
    public class VersionData
    {
        /// <summary>
        /// Gets or sets the overall status of the version data retrieval operation.
        /// Typical values: "success" for successful retrieval, "error" for failures or invalid components.
        /// Default value is "success".
        /// </summary>
        public string Status { get; set; } = "success";
        
        /// <summary>
        /// Gets or sets a descriptive message about the version data operation.
        /// Contains error descriptions on failure (e.g., "Component not found"), 
        /// or summary information on success (e.g., "10 versions found for PHP").
        /// Empty string for successful operations without additional information.
        /// </summary>
        public string Message { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the header text to display above the version table or list.
        /// Typically contains the component name and context (e.g., "Available versions for PHP").
        /// Used for formatting console output and UI table titles.
        /// </summary>
        public string Header { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the list of all available versions that can be installed.
        /// Contains version strings from the version provider (e.g., ["8.1.0", "8.2.0", "8.3.0"]).
        /// Empty list if the component has no available versions or doesn't exist.
        /// </summary>
        public List<string> Versions { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the list of versions currently installed on the local system.
        /// Contains version strings from the installed directory scan (e.g., ["8.2.0", "8.3.0"]).
        /// Used to highlight installed versions in the display output with different colors.
        /// Empty list if no versions are installed.
        /// </summary>
        public List<string> Installed { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets a value indicating whether versions should be displayed in descending order.
        /// True to show newest versions first (default), false for ascending order (oldest first).
        /// Null to use the provider's default ordering without modification.
        /// </summary>
        public bool? OrderDescending { get; set; } = true;
    }
}