using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PgSQLComponent : ComponentBase
    {
        public override string Name => "pgsql";
        public override string ToolDir => DevStackConfig.pgsqlDir;
        public override bool IsService => true;
        public override string? ServicePattern => "postgres.exe";
    }
}
