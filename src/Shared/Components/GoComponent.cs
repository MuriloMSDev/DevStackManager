using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class GoComponent : ComponentBase
    {
        public override string Name => "go";
        public override string Label => "Go";
        public override string ToolDir => DevStackConfig.goDir;
        public override bool IsExecutable => true;
        public override string? ExecutablePattern => "go.exe";
        public override string? ExecutableFolder => "bin";
        public override string? CreateBinShortcut => "go-{version}.exe";
    }
}
