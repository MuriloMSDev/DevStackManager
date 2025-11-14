using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// Go programming language component manager for DevStack.
    /// Handles Go installation and provides access to Go compiler and tools.
    /// Go binaries are located in the bin subdirectory.
    /// </summary>
    public class GoComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "go";
        
        /// <summary>
        /// Gets the display label for Go.
        /// </summary>
        public override string Label => "Go";
        
        /// <summary>
        /// Gets the Go installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.goDir;
        
        /// <summary>
        /// Gets whether Go is executable from command line.
        /// </summary>
        public override bool IsExecutable => true;
        
        /// <summary>
        /// Gets the Go executable pattern.
        /// </summary>
        public override string? ExecutablePattern => "go.exe";
        
        /// <summary>
        /// Gets the subfolder containing Go executables.
        /// Go stores binaries in the bin subdirectory.
        /// </summary>
        public override string? ExecutableFolder => "bin";
        
        /// <summary>
        /// Gets the shortcut name pattern for Go binaries.
        /// </summary>
        public override string? CreateBinShortcut => "go-{version}.exe";
    }
}
