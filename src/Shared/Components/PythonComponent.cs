using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PythonComponent : ComponentBase
    {
        public override string Name => "python";
        public override string ToolDir => DevStackConfig.pythonDir;
        public override bool IsExecutable => true;
        public override bool IsCommandLine => true;
        public override string? ExecutablePattern => "python.exe";
        public override string? CreateBinShortcut => "python-{version}.exe";
    }
}
