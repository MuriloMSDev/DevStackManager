using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PythonComponent : ComponentBase
    {
        public override string Name => "python";
        public override string ToolDir => DevStackConfig.pythonDir;
    }
}
