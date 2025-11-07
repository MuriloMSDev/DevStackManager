using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class ComposerComponent : ComponentBase
    {
        public override string Name => "composer";
        public override string Label => "Composer";
        public override string ToolDir => DevStackConfig.composerDir;
        public override bool IsArchive => false;
    }
}
