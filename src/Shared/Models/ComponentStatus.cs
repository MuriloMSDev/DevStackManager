using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Represents the current status of a DevStack component including installation state and running services.
    /// Provides detailed status information for both service and non-service components with version-specific running states.
    /// </summary>
    public class ComponentStatus
    {
        /// <summary>
        /// Gets or sets a value indicating whether the component is installed on the system.
        /// True if at least one version is installed; otherwise, false.
        /// </summary>
        public bool Installed { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the list of installed versions for this component.
        /// Contains version strings in ascending order (e.g., ["1.20.0", "1.21.5", "1.22.1"]).
        /// Empty list if the component is not installed.
        /// </summary>
        public List<string> Versions { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets a descriptive status message about the component.
        /// Contains installation confirmation messages, error descriptions, or runtime status information.
        /// Examples: "nginx installed", "PHP 8.2 is not installed", "Component unknown".
        /// </summary>
        public string Message { get; set; } = "";
        
        /// <summary>
        /// Gets or sets a dictionary mapping installed version strings to their running status for service components.
        /// Key: Version string (e.g., "8.2.5"), Value: True if the service is currently running, false if stopped.
        /// Null for non-service components or if no versions are running.
        /// Only populated for components where IsService is true (e.g., PHP-FPM, Nginx, MySQL).
        /// </summary>
        public Dictionary<string, bool>? RunningList { get; set; }
    }
}