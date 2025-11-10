using System;
using System.Linq;

namespace DevStackManager
{
    public static class UninstallManager
    {
        public static void UninstallCommands(params string[] components)
        {
            if (!ValidateUninstallArgs(components, out var tool, out var version))
            {
                return;
            }

            var component = Components.ComponentsFactory.GetComponent(tool);
            if (component == null)
            {
                LogUnknownComponent(tool);
                LogUninstallFinished();
                return;
            }

            var shortcutPattern = component.CreateBinShortcut;
            component.Uninstall(version);

            TryRemoveShortcutIfNeeded(tool, version, shortcutPattern);
            LogUninstallFinished();
        }

        private static bool ValidateUninstallArgs(
            string[] components, 
            out string tool, 
            out string? version)
        {
            tool = string.Empty;
            version = null;

            if (components == null || components.Length == 0)
            {
                LogNoComponentSpecified();
                return false;
            }

            tool = components[0].ToLowerInvariant();
            version = components.Length > 1 ? components[1] : null;
            return true;
        }

        private static void TryRemoveShortcutIfNeeded(
            string tool, 
            string? version, 
            string? shortcutPattern)
        {
            if (string.IsNullOrEmpty(shortcutPattern) || string.IsNullOrEmpty(version))
            {
                return;
            }

            LogRemovingShortcut(tool);
            Components.ComponentBase.RemoveGlobalBinShortcut(tool, version, shortcutPattern);
        }

        private static void LogNoComponentSpecified()
        {
            DevStackConfig.WriteColoredLine(
                GetLocalizedString("shared.uninstall.no_component", 
                    "Nenhum componente especificado para desinstalar."),
                ConsoleColor.Red);
        }

        private static void LogUnknownComponent(string tool)
        {
            DevStackConfig.WriteColoredLine(
                GetLocalizedString("shared.uninstall.unknown_component", 
                    $"Componente desconhecido: {tool}", tool),
                ConsoleColor.Red);
        }

        private static void LogRemovingShortcut(string tool)
        {
            DevStackConfig.WriteColoredLine(
                GetLocalizedString("shared.uninstall.removing_shortcut", 
                    $"Removendo atalho para {tool}...", tool),
                ConsoleColor.Yellow);
        }

        private static void LogUninstallFinished()
        {
            DevStackConfig.WriteColoredLine(
                GetLocalizedString("shared.uninstall.finished", 
                    "Uninstall finalizado."),
                ConsoleColor.Green);
        }

        private static string GetLocalizedString(string key, string fallback, params object[] args)
        {
            return DevStackShared.LocalizationManager.Instance?.GetString(key, args) ?? fallback;
        }
    }
}
