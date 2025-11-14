using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Represents comprehensive information about a DevStack component.
    /// Contains installation status, executability flags, and version information for a single development tool.
    /// </summary>
    public class ComponentInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier name of the component (e.g., "php", "nginx", "mysql").
        /// </summary>
        public string Name { get; set; } = "";
        
        /// <summary>
        /// Gets or sets a value indicating whether the component is currently installed on the system.
        /// True if at least one version is installed; otherwise, false.
        /// </summary>
        public bool Installed { get; set; } = false;
        
        /// <summary>
        /// Gets or sets a value indicating whether the component is an executable program that can be run from command line.
        /// True for CLI tools and executables; false for libraries or non-executable components.
        /// </summary>
        public bool IsExecutable { get; set; } = false;
        
        /// <summary>
        /// Gets or sets the list of installed versions for this component.
        /// Contains version strings in ascending order (e.g., ["8.1.0", "8.2.5", "8.3.0"]).
        /// Empty list if no versions are installed.
        /// </summary>
        public List<string> Versions { get; set; } = new List<string>();
    }
}