using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class PhpMyAdminComponent : ComponentBase
    {
        public override string Name => "phpmyadmin";
        public override string Label => "phpMyAdmin";
        public override string ToolDir => DevStackConfig.pmaDir;
    }
}
