using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// Adminer database management tool component for DevStack.
    /// Handles Adminer installation for web-based database administration.
    /// Adminer is a lightweight PHP alternative to phpMyAdmin, distributed as a single PHP file.
    /// </summary>
    public class AdminerComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "adminer";
        
        /// <summary>
        /// Gets the display label for Adminer.
        /// </summary>
        public override string Label => "Adminer";
        
        /// <summary>
        /// Gets the Adminer installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.adminerDir;
        
        /// <summary>
        /// Gets whether Adminer is distributed as an archive.
        /// Returns false because Adminer is a single PHP file, not an archive.
        /// </summary>
        public override bool IsArchive => false;
    }
}
