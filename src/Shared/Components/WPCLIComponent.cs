using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// WP-CLI (WordPress Command Line Interface) component manager for DevStack.
    /// Handles WP-CLI installation for managing WordPress sites from command line.
    /// WP-CLI is distributed as a single PHAR file.
    /// </summary>
    public class WPCLIComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "wpcli";
        
        /// <summary>
        /// Gets the display label for WP-CLI.
        /// </summary>
        public override string Label => "WP-CLI";
        
        /// <summary>
        /// Gets the WP-CLI installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.wpcliDir;
        
        /// <summary>
        /// Gets whether WP-CLI is distributed as an archive.
        /// Returns false because WP-CLI is a single PHAR file, not an archive.
        /// </summary>
        public override bool IsArchive => false;
    }
}
