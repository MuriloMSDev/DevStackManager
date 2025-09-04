using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class WPCLIComponent : ComponentBase
    {
        public override string Name => "wpcli";
        public override string ToolDir => DevStackConfig.wpcliDir;
        public override bool IsArchive => false;
    }
}
