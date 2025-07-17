using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Classe para representar o status de um componente
    /// </summary>
    public class ComponentStatus
    {
        public bool Installed { get; set; } = false;
        public List<string> Versions { get; set; } = new List<string>();
        public string Message { get; set; } = "";
        public List<bool>? RunningList { get; set; } // true/false para cada versão, na mesma ordem da lista Versions
    }
}