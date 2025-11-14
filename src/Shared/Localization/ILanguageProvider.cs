using System.Collections.Generic;

namespace DevStackShared.Localization
{
    /// <summary>
    /// Interface for language-specific translation providers.
    /// Defines the contract for implementing localization support across all DevStack applications.
    /// Each language provider must supply translations organized by application section (common, shared, GUI, installer, uninstaller).
    /// </summary>
    public interface ILanguageProvider
    {
        /// <summary>
        /// Gets the language code identifier (e.g., "pt_BR" for Brazilian Portuguese, "en_US" for US English).
        /// Must follow the standard language_COUNTRY format for consistency.
        /// </summary>
        string LanguageCode { get; }

        /// <summary>
        /// Gets the human-readable display name of the language (e.g., "Português (Brasil)", "English (US)").
        /// Used in language selection dropdowns and UI displays.
        /// </summary>
        string LanguageName { get; }

        /// <summary>
        /// Returns common translations shared across all applications.
        /// Includes button labels (OK, Cancel, Yes, No), common dialogs, and universal UI elements.
        /// </summary>
        /// <returns>Dictionary mapping translation keys to their localized string values.</returns>
        Dictionary<string, object> GetCommonTranslations();

        /// <summary>
        /// Returns shared translations used by multiple application components.
        /// Includes installation messages, uninstall confirmations, and cross-component notifications.
        /// </summary>
        /// <returns>Dictionary mapping translation keys to their localized string values.</returns>
        Dictionary<string, object> GetSharedTranslations();

        /// <summary>
        /// Returns GUI-specific translations for the DevStack graphical interface.
        /// Includes window titles, menu items, dashboard labels, tabs, and GUI-specific messages.
        /// </summary>
        /// <returns>Dictionary mapping translation keys to their localized string values.</returns>
        Dictionary<string, object> GetGuiTranslations();

        /// <summary>
        /// Returns installer-specific translations for the DevStack installation wizard.
        /// Includes welcome screens, license agreements, installation progress, and setup wizard steps.
        /// </summary>
        /// <returns>Dictionary mapping translation keys to their localized string values.</returns>
        Dictionary<string, object> GetInstallerTranslations();

        /// <summary>
        /// Returns uninstaller-specific translations for the DevStack removal wizard.
        /// Includes uninstallation confirmations, progress messages, and removal wizard steps.
        /// </summary>
        /// <returns>Dictionary mapping translation keys to their localized string values.</returns>
        Dictionary<string, object> GetUninstallerTranslations();

        /// <summary>
        /// Returns all translations from all sections merged into a single dictionary.
        /// Organizes translations by section name (common, shared, gui, installer, uninstaller) as top-level keys.
        /// Useful for loading complete language packs at once.
        /// </summary>
        /// <returns>Dictionary mapping section names to their respective translation dictionaries.</returns>
        Dictionary<string, object> GetAllTranslations();
    }
}
