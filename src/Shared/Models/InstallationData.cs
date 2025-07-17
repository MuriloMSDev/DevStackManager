using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Classe para representar dados de instalação
    /// </summary>
    public class InstallationData
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = "";
        public List<ComponentInfo> Components { get; set; } = new List<ComponentInfo>();
    }
}