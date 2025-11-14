using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Compression;
using System;
using System.IO;

namespace DevStackManager.Components
{
    /// <summary>
    /// Git version control system component manager for DevStack.
    /// Handles Git installation and provides access to git commands from mingw64 binaries.
    /// Git for Windows uses MinGW64 environment for Unix-like shell and utilities.
    /// </summary>
    public class GitComponent : ComponentBase
    {
        /// <summary>
        /// Gets the component name identifier.
        /// </summary>
        public override string Name => "git";
        
        /// <summary>
        /// Gets the display label for Git.
        /// </summary>
        public override string Label => "Git";
        
        /// <summary>
        /// Gets the Git installation base directory.
        /// </summary>
        public override string ToolDir => DevStackConfig.gitDir;
        
        /// <summary>
        /// Gets whether Git is executable from command line.
        /// </summary>
        public override bool IsExecutable => true;
        
        /// <summary>
        /// Gets whether Git is a command-line tool.
        /// </summary>
        public override bool IsCommandLine => true;
        
        /// <summary>
        /// Gets the Git executable pattern.
        /// </summary>
        public override string? ExecutablePattern => "git.exe";
        
        /// <summary>
        /// Gets the subfolder containing Git executables.
        /// Git for Windows stores binaries in mingw64/bin.
        /// </summary>
        public override string? ExecutableFolder => Path.Combine("mingw64", "bin");
        
        /// <summary>
        /// Gets the shortcut name pattern for Git binaries.
        /// </summary>
        public override string? CreateBinShortcut => "git-{version}.exe";
    }
}
