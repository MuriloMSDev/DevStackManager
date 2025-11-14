using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// PHP CS Fixer (PHP Coding Standards Fixer) component manager for DevStack.
    /// Handles PHP CS Fixer installation for automated code style fixing in PHP projects.
    /// PHP CS Fixer is distributed as a single PHAR file.
    /// </summary>
    public class PHPCsFixerComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "phpcsfixer";
        
        /// <summary>
        /// Gets the display label for PHP CS Fixer.
        /// </summary>
        public override string Label => "PHP CS Fixer";
        
        /// <summary>
        /// Gets the PHP CS Fixer installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.phpcsfixerDir;
        
        /// <summary>
        /// Gets whether PHP CS Fixer is distributed as an archive.
        /// Returns false because PHP CS Fixer is a single PHAR file, not an archive.
        /// </summary>
        public override bool IsArchive => false;
    }
}
