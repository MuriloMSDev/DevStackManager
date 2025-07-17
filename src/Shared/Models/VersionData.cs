using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DevStackManager
{
    /// <summary>
    /// Classe para representar dados de versão
    /// </summary>
    public class VersionData
    {
        public string Status { get; set; } = "success";
        public string Message { get; set; } = "";
        public string Header { get; set; } = "";
        public List<string> Versions { get; set; } = new List<string>();
        public List<string> Installed { get; set; } = new List<string>();
        public bool? OrderDescending { get; set; } = true;
    }
}