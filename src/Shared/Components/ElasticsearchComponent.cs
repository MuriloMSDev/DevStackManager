using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class ElasticsearchComponent : ComponentBase
    {
        public override string Name => "elasticsearch";
        public override string ToolDir => DevStackConfig.elasticDir;
        public override bool IsService => true;
        public override string? ServicePattern => "elasticsearch.exe";
        public override int? MaxWorkers => 1; // Elasticsearch normalmente roda em processo único com threads
    }
}
