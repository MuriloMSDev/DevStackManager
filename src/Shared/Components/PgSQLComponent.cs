using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// PostgreSQL database component manager for DevStack.
    /// Handles PostgreSQL installation and service management for relational database operations.
    /// PostgreSQL uses a multi-process architecture with one postmaster and multiple backend processes.
    /// </summary>
    public class PgSQLComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "pgsql";
        
        /// <summary>
        /// Gets the display label for PostgreSQL.
        /// </summary>
        public override string Label => "PostgreSQL";
        
        /// <summary>
        /// Gets the PostgreSQL installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.pgsqlDir;
        
        /// <summary>
        /// Gets whether PostgreSQL runs as a Windows service.
        /// </summary>
        public override bool IsService => true;
        
        /// <summary>
        /// Gets the PostgreSQL service process pattern for monitoring.
        /// </summary>
        public override string? ServicePattern => "postgres.exe";
        
        /// <summary>
        /// Gets the maximum number of PostgreSQL processes to track.
        /// Value is 3 to account for 1 postmaster process + 2-3 backend worker processes.
        /// </summary>
        public override int? MaxWorkers => 3;
    }
}
