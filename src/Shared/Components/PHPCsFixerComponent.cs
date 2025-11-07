using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PHPCsFixerComponent : ComponentBase
    {
        public override string Name => "phpcsfixer";
        public override string Label => "PHP CS Fixer";
        public override string ToolDir => DevStackConfig.phpcsfixerDir;
        public override bool IsArchive => false;
    }
}
