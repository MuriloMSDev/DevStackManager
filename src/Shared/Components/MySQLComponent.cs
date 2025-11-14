using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// MySQL database server component manager for DevStack.
    /// Handles MySQL installation and service management for relational database operations.
    /// MySQL runs as a single process with internal thread pooling.
    /// </summary>
    public class MySQLComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "mysql";
        
        /// <summary>
        /// Gets the display label for MySQL.
        /// </summary>
        public override string Label => "MySQL";
        
        /// <summary>
        /// Gets the MySQL installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.mysqlDir;
        
        /// <summary>
        /// Gets whether MySQL runs as a service.
        /// </summary>
        public override bool IsService => true;
        
        /// <summary>
        /// Gets the MySQL service executable pattern.
        /// </summary>
        public override string? ServicePattern => "mysqld.exe";
        
        /// <summary>
        /// Gets the maximum number of worker processes to track (1 as MySQL uses internal threading).
        /// </summary>
        public override int? MaxWorkers => 1;
    }
}
