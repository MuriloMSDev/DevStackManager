using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// Composer dependency manager component for DevStack.
    /// Handles Composer installation for managing PHP dependencies in Laravel, CakePHP,
    /// and other PHP projects. Composer is distributed as a single PHAR file.
    /// </summary>
    public class ComposerComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "composer";
        
        /// <summary>
        /// Gets the display label for Composer.
        /// </summary>
        public override string Label => "Composer";
        
        /// <summary>
        /// Gets the Composer installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.composerDir;
        
        /// <summary>
        /// Gets whether Composer is distributed as an archive.
        /// Returns false because Composer is a single PHAR file, not an archive.
        /// </summary>
        public override bool IsArchive => false;
    }
}
