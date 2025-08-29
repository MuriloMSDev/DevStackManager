using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class GoComponent : ComponentBase
    {
        public override string Name => "go";
        public override string ToolDir => DevStackConfig.goDir;
    }
}
