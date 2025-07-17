using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Informações sobre um componente
    /// </summary>
    public class ComponentInfo
    {
        public string Name { get; set; } = "";
        public bool Installed { get; set; } = false;
        public List<string> Versions { get; set; } = new List<string>();
    }
}