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
                DevStackConfig.WriteColoredLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.uninstall.no_component") ?? "Nenhum componente especificado para desinstalar.", ConsoleColor.Red);
                return;
            }

            string tool = components[0].ToLowerInvariant();
            string? version = components.Length > 1 ? components[1] : null;

            var comp = Components.ComponentsFactory.GetComponent(tool);
            if (comp != null)
            {
                // Verificar se o componente tem CreateBinShortcut definido antes da desinstalação
                string? shortcutPattern = comp.CreateBinShortcut;
                
                comp.Uninstall(version);
                
                // Remover atalho se o componente tinha CreateBinShortcut definido
                if (!string.IsNullOrEmpty(shortcutPattern) && !string.IsNullOrEmpty(version))
                {
                    DevStackConfig.WriteColoredLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.uninstall.removing_shortcut", tool) ?? $"Removendo atalho para {tool}...", ConsoleColor.Yellow);
                    Components.ComponentBase.RemoveGlobalBinShortcut(tool, version, shortcutPattern);
                }
            }
            else
            {
                DevStackConfig.WriteColoredLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.uninstall.unknown_component", tool) ?? $"Componente desconhecido: {tool}", ConsoleColor.Red);
            }
            DevStackConfig.WriteColoredLine(DevStackShared.LocalizationManager.Instance?.GetString("shared.uninstall.finished") ?? "Uninstall finalizado.", ConsoleColor.Green);
        }
    }
}
