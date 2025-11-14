using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Represents comprehensive installation data for all DevStack components.
    /// Contains status information and detailed component lists with installation states.
    /// Used primarily for API responses and dashboard data aggregation.
    /// </summary>
    public class InstallationData
    {
        /// <summary>
        /// Gets or sets the overall status of the data retrieval operation.
        /// Typical values: "success" for successful retrieval, "error" for failures.
        /// Default value is "success".
        /// </summary>
        public string Status { get; set; } = "success";
        
        /// <summary>
        /// Gets or sets a descriptive message about the installation data retrieval.
        /// Contains error details on failure, or summary information on success.
        /// Empty string for successful operations without additional information.
        /// </summary>
        public string Message { get; set; } = "";
        
        /// <summary>
        /// Gets or sets the collection of component information objects.
        /// Each ComponentInfo contains name, installation status, executability, and version details.
        /// Includes all registered DevStack components regardless of installation state.
        /// </summary>
        public List<ComponentInfo> Components { get; set; } = new List<ComponentInfo>();
    }
}