using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// phpMyAdmin database management tool component for DevStack.
    /// Handles phpMyAdmin installation for web-based MySQL/MariaDB database administration.
    /// phpMyAdmin is a PHP-based application for managing MySQL databases through a web interface.
    /// </summary>
    public class PhpMyAdminComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "phpmyadmin";
        
        /// <summary>
        /// Gets the display label for phpMyAdmin.
        /// </summary>
        public override string Label => "phpMyAdmin";
        
        /// <summary>
        /// Gets the phpMyAdmin installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.pmaDir;
    }
}
