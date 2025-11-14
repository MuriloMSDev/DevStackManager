using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// DBeaver database client component manager for DevStack.
    /// Handles DBeaver installation for universal database management and SQL client operations.
    /// DBeaver supports MySQL, PostgreSQL, MongoDB, SQL Server, Oracle, and many other databases.
    /// </summary>
    public class DBeaverComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "dbeaver";
        
        /// <summary>
        /// Gets the display label for DBeaver.
        /// </summary>
        public override string Label => "DBeaver";
        
        /// <summary>
        /// Gets the DBeaver installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.dbeaverDir;
        
        /// <summary>
        /// Gets whether DBeaver is executable from command line.
        /// </summary>
        public override bool IsExecutable => true;
        
        /// <summary>
        /// Gets the DBeaver executable pattern.
        /// </summary>
        public override string? ExecutablePattern => "dbeaver.exe";
        
        /// <summary>
        /// Gets the shortcut name pattern for DBeaver binaries.
        /// </summary>
        public override string? CreateBinShortcut => "dbeaver-{version}.exe";
    }
}
