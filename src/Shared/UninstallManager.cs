using System;
using System.Linq;

namespace DevStackManager
{
    /// <summary>
    /// Manages component uninstallation operations.
    /// Handles component removal, shortcut cleanup, and uninstall workflow coordination.
    /// </summary>
    public static class UninstallManager
    {
        /// <summary>
        /// Uninstalls a component with optional version specification.
        /// Removes the component files, cleans up shortcuts if applicable, and logs the process.
        /// </summary>
        /// <param name="components">Component name and optional version arguments.</param>
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

        /// <summary>
        /// Validates uninstall command arguments.
        /// </summary>
        /// <param name="components">Command arguments array.</param>
        /// <param name="tool">Extracted component/tool name.</param>
        /// <param name="version">Optional extracted version string.</param>
        /// <returns>True if arguments are valid, false otherwise.</returns>
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

        /// <summary>
        /// Attempts to remove global bin shortcuts for a component if configured.
        /// </summary>
        /// <param name="tool">Component name.</param>
        /// <param name="version">Component version.</param>
        /// <param name="shortcutPattern">Shortcut pattern from component configuration.</param>
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

        /// <summary>
        /// Logs an error message when no component is specified for uninstallation.
        /// </summary>
        private static void LogNoComponentSpecified()
        {
            DevStackConfig.WriteColoredLine(
                GetLocalizedString("shared.uninstall.no_component", 
                    "No component specified for uninstallation."),
                ConsoleColor.Red);
        }

        /// <summary>
        /// Logs an error message for an unknown component.
        /// </summary>
        /// <param name="tool">The unknown component name.</param>
        private static void LogUnknownComponent(string tool)
        {
            DevStackConfig.WriteColoredLine(
                GetLocalizedString("shared.uninstall.unknown_component", 
                    $"Unknown component: {tool}", tool),
                ConsoleColor.Red);
        }

        /// <summary>
        /// Logs a message indicating shortcut removal is in progress.
        /// </summary>
        /// <param name="tool">Component name whose shortcut is being removed.</param>
        private static void LogRemovingShortcut(string tool)
        {
            DevStackConfig.WriteColoredLine(
                GetLocalizedString("shared.uninstall.removing_shortcut", 
                    $"Removing shortcut for {tool}...", tool),
                ConsoleColor.Yellow);
        }

        /// <summary>
        /// Logs a completion message after uninstall finishes.
        /// </summary>
        private static void LogUninstallFinished()
        {
            DevStackConfig.WriteColoredLine(
                GetLocalizedString("shared.uninstall.finished", 
                    "Uninstall finished."),
                ConsoleColor.Green);
        }

        /// <summary>
        /// Retrieves a localized string from the LocalizationManager with a fallback.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <param name="fallback">Fallback string if localization is unavailable.</param>
        /// <param name="args">Optional formatting arguments.</param>
        /// <returns>The localized string or fallback.</returns>
        private static string GetLocalizedString(string key, string fallback, params object[] args)
        {
            return DevStackShared.LocalizationManager.Instance?.GetString(key, args) ?? fallback;
        }
    }
}
