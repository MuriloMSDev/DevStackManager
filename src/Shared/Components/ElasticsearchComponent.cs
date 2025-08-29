using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class ElasticsearchComponent : ComponentBase
    {
        public override string Name => "elasticsearch";
        public override string ToolDir => DevStackConfig.elasticDir;
    }
}
