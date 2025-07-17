using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevStackManager
{
    public static class UninstallManager
    {
        public static void UninstallCommands(params string[] components)
        {
            if (components == null || components.Length == 0)
            {
                DevStackConfig.WriteColoredLine("Nenhum componente especificado para desinstalar.", ConsoleColor.Red);
                return;
            }

            string tool = components[0].ToLowerInvariant();
            string? version = components.Length > 1 ? components[1] : null;

            var comp = Components.ComponentsFactory.GetComponent(tool);
            if (comp != null)
            {
                comp.Uninstall(version);
            }
            else
            {
                DevStackConfig.WriteColoredLine($"Componente desconhecido: {tool}", ConsoleColor.Red);
            }
            DevStackConfig.WriteColoredLine("Uninstall finalizado.", ConsoleColor.Green);
        }
    }
}
