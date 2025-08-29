using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevStackManager.Components
{
    public class NginxComponent : ComponentBase
    {
        public override string Name => "nginx";
        public override string ToolDir => DevStackConfig.nginxDir;
        public override bool IsService => true;
    }
}
