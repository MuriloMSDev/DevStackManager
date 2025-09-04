using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class AdminerComponent : ComponentBase
    {
        public override string Name => "adminer";
        public override string ToolDir => DevStackConfig.adminerDir;
        public override bool IsArchive => false;
    }
}
