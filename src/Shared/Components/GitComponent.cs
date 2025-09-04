using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Compression;
using System;
using System.IO;

namespace DevStackManager.Components
{
    public class GitComponent : ComponentBase
    {
        public override string Name => "git";
        public override string ToolDir => DevStackConfig.gitDir;
        public override bool IsExecutable => true;
        public override bool IsCommandLine => true;
        public override string? ExecutablePattern => "git.exe";
        public override string? ExecutableFolder => Path.Combine("mingw64", "bin");
        public override string? CreateBinShortcut => "git-{version}.exe";
    }
}
