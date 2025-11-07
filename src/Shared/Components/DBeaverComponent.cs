using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class DBeaverComponent : ComponentBase
    {
        public override string Name => "dbeaver";
        public override string Label => "DBeaver";
        public override string ToolDir => DevStackConfig.dbeaverDir;
        public override bool IsExecutable => true;
        public override string? ExecutablePattern => "dbeaver.exe";
        public override string? CreateBinShortcut => "dbeaver-{version}.exe";
    }
}
