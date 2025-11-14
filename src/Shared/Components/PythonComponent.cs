using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    /// <summary>
    /// Python runtime component manager for DevStack.
    /// Handles Python installation and provides access to Python interpreter,
    /// pip package manager, and virtual environment tools.
    /// </summary>
    public class PythonComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "python";
        
        /// <summary>
        /// Gets the display label for Python.
        /// </summary>
        public override string Label => "Python";
        
        /// <summary>
        /// Gets the Python installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.pythonDir;
        
        /// <summary>
        /// Gets whether Python is executable from command line.
        /// </summary>
        public override bool IsExecutable => true;
        
        /// <summary>
        /// Gets whether Python is a command-line tool.
        /// </summary>
        public override bool IsCommandLine => true;
        
        /// <summary>
        /// Gets the Python executable pattern.
        /// </summary>
        public override string? ExecutablePattern => "python.exe";
        
        /// <summary>
        /// Gets the shortcut name pattern for Python binaries.
        /// </summary>
        public override string? CreateBinShortcut => "python-{version}.exe";
    }
}
