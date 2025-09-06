using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class MySQLComponent : ComponentBase
    {
        public override string Name => "mysql";
        public override string ToolDir => DevStackConfig.mysqlDir;
        public override bool IsService => true;
        public override string? ServicePattern => "mysqld.exe";
        public override int? MaxWorkers => 1; // MySQL normalmente roda em processo único com threads internas
    }
}
