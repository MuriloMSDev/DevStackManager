using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class MongoDBComponent : ComponentBase
    {
        public override string Name => "mongodb";
        public override string ToolDir => DevStackConfig.mongoDir;
    }
}
