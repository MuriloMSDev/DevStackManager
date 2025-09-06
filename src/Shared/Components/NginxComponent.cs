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
        public override string? ServicePattern => "nginx.exe";
        public override int? MaxWorkers => 1; // Mostrar apenas o processo master
    }
}
