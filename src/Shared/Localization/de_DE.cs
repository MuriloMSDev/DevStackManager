using System.Collections.Generic;

namespace DevStackShared.Localization
{
    /// <summary>
    /// German (Germany) language provider for DevStack localization.
    /// </summary>
    public class de_DE : ILanguageProvider
    {
        /// <summary>
        /// Gets the language code identifier (de_DE).
        /// </summary>
        public string LanguageCode => "de_DE";
        
        /// <summary>
        /// Gets the display name of the language.
        /// </summary>
        public string LanguageName => "Deutsch";

        /// <summary>
        /// Gets common translations used across all DevStack applications.
        /// </summary>
        /// <returns>Dictionary containing common translation strings.</returns>
        public Dictionary<string, object> GetCommonTranslations()
        {
            return new Dictionary<string, object>
            {
                { "language_name", "Deutsch" },
                { "unknown", "Unbekannt" },
                { "themes", new Dictionary<string, object>
                {
                    { "light", "Hell" },
                    { "dark", "Dunkel" },
                    { "messages", new Dictionary<string, object>
                    {
                        { "theme_changed", "Thema geändert zu {0}" }
                    }
                    }
                }
                },
                { "buttons", new Dictionary<string, object>
                {
                    { "back", "← Zurück" },
                    { "next", "Weiter →" },
                    { "accept", "Ich akzeptiere" },
                    { "install", "Installieren" },
                    { "finish", "Fertigstellen" },
                    { "cancel", "Abbrechen" },
                    { "continue", "Fortfahren" },
                    { "uninstall", "🗑️ Deinstallieren" },
                    { "yes", "Ja" },
                    { "no", "Nein" },
                    { "ok", "OK" }
                }
                },
                { "dialogs", new Dictionary<string, object>
                {
                    { "default_title", "Nachricht" }
                }
                }
            };
        }

        /// <summary>
        /// Gets shared translations used by CLI, GUI, and utilities.
        /// </summary>
        /// <returns>Dictionary containing shared translation strings.</returns>
        public Dictionary<string, object> GetSharedTranslations()
        {
            return new Dictionary<string, object>
            {
                { "uninstall", new Dictionary<string, object>
                {
                    { "no_component", "Keine Komponente für Deinstallation angegeben." },
                    { "removing_shortcut", "Verknüpfung für {0} wird entfernt..." },
                    { "unknown_component", "Unbekannte Komponente: {0}" },
                    { "finished", "Deinstallation abgeschlossen." }
                }
                },
                { "shortcuts", new Dictionary<string, object>
                {
                    { "created", "Verknüpfung {0} erstellt, die auf {1} zeigt" },
                    { "error_creating", "Fehler beim Erstellen der symbolischen Verknüpfung: {0}" },
                    { "fallback_copy", "Fallback: Kopie {0} erstellt in {1}" },
                    { "file_not_found", "Warnung: Datei {0} nicht gefunden zum Erstellen der Verknüpfung" },
                    { "removed", "Verknüpfung {0} entfernt" },
                    { "not_found", "Verknüpfung {0} nicht zum Entfernen gefunden" },
                    { "error_removing", "Fehler beim Entfernen der Verknüpfung: {0}" }
                }
                },
                { "install", new Dictionary<string, object>
                {
                    { "already_installed", "{0} {1} ist bereits installiert." },
                    { "downloading", "{0} {1} wird heruntergeladen..." },
                    { "running_installer", "Installer {0} {1} wird ausgeführt..." },
                    { "installed_via_installer", "{0} {1} über Installer installiert in {2}" },
                    { "extracting", "Extrahieren..." },
                    { "installed", "{0} {1} installiert." },
                    { "installed_in", "{0} {1} installiert in {2}." },
                    { "error_installing", "Fehler beim Installieren von {0} {1}: {2}" },
                    { "shortcut_creation_failed", "Warnung: Erstellen der Verknüpfung fehlgeschlagen: {0}" },
                    { "component_installed", "{0} {1} installiert." }
                }
                }
            };
        }

        /// <summary>
        /// Gets translations specific to the DevStack GUI application.
        /// </summary>
        /// <returns>Dictionary containing GUI-specific translation strings.</returns>
        public Dictionary<string, object> GetGuiTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window", new Dictionary<string, object>
                {
                    { "title", "DevStack Manager v{0}" },
                    { "ready_status", "Bereit" },
                    { "initialization_error", "Fehler beim Initialisieren der DevStack GUI: {0}" },
                    { "error_title", "DevStack Manager - Fehler" }
                }
                },
                { "navigation", new Dictionary<string, object>
                {
                    { "title", "DevStack Manager" },
                    { "items", new Dictionary<string, object>
                    {
                        { "dashboard", new Dictionary<string, object>
                        {
                            { "title", "Dashboard" },
                            { "description", "Systemübersicht" }
                        }
                        },
                        { "installed", new Dictionary<string, object>
                        {
                            { "title", "Installiert" },
                            { "description", "Installierte Werkzeuge" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Installieren" },
                            { "description", "Neue Komponenten installieren" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Deinstallieren" },
                            { "description", "Komponenten entfernen" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Dienste" },
                            { "description", "Dienststeuerung" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Einstellungen" },
                            { "description", "Systemeinstellungen" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Websites" },
                            { "description", "Nginx-Websites verwalten" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Dienstprogramme" },
                            { "description", "Werkzeuge und Konsole" }
                        }
                        }
                    }
                    },
                    { "refresh_tooltip", "Alle Daten aktualisieren" }
                }
                },
                { "dashboard_tab", new Dictionary<string, object>
                {
                    { "title", "📊 Dashboard" },
                    { "cards", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Komponenten" },
                            { "subtitle", "Klicken Sie zum Zugriff" },
                            { "loading", "Wird geladen..." },
                            { "installed_count", "{0}/{1} installiert" },
                            { "none", "Keine Komponenten" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Installieren" },
                            { "subtitle", "Klicken Sie zum Zugriff" },
                            { "description", "Komponenten hinzufügen" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Dienste" },
                            { "subtitle", "Klicken Sie zum Zugriff" },
                            { "loading", "Wird geladen..." },
                            { "active_count", "{0}/{1} aktiv" },
                            { "none", "Keine aktiven Dienste" }
                        }
                        }
                    }
                    },
                    { "panels", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Installierte Komponenten" },
                            { "refresh_tooltip", "Installierte Komponenten aktualisieren" },
                            { "install_button", "📥 Installieren" },
                            { "uninstall_button", "🗑️ Deinstallieren" },
                            { "none", "Keine Komponenten installiert" },
                            { "installed_default", "Installiert" },
                            { "error_loading", "Fehler beim Laden der Komponenten" },
                            { "version_na", "N/A" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Dienste" },
                            { "refresh_tooltip", "Dienste aktualisieren" },
                            { "start_all", "▶️ Starten" },
                            { "stop_all", "⏹️ Stoppen" },
                            { "restart_all", "🔄 Neustarten" },
                            { "none", "Keine Dienste gefunden" },
                            { "loading", "Dienste werden geladen..." },
                            { "status", new Dictionary<string, object>
                            {
                                { "active", "Aktiv" },
                                { "stopped", "Gestoppt" },
                                { "na", "N/A" }
                            }
                            }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "📥 Schnelle Installation" },
                            { "select_component", "Wählen Sie eine Komponente zur Installation." },
                            { "installing", "Installiere {0}..." },
                            { "success", "{0} erfolgreich installiert!" },
                            { "error", "Fehler beim Installieren von {0}: {1}" },
                            { "install_button", "📥 Installieren" },
                            { "go_to_install", "Zur Installation" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "updating_components", "Komponenten werden aktualisiert..." },
                        { "components_updated", "Komponenten aktualisiert!" },
                        { "error_updating_components", "Fehler beim Aktualisieren der Komponenten: {0}" },
                        { "updating_services", "Dienste werden aktualisiert..." },
                        { "services_updated", "Dienste aktualisiert!" },
                        { "error_updating_services", "Fehler beim Aktualisieren der Dienste: {0}" },
                        { "starting_all_services", "Alle Dienste werden gestartet..." },
                        { "all_services_started", "Alle Dienste wurden gestartet!" },
                        { "error_starting_services", "Fehler beim Starten der Dienste: {0}" },
                        { "stopping_all_services", "Alle Dienste werden gestoppt..." },
                        { "all_services_stopped", "Alle Dienste wurden gestoppt!" },
                        { "error_stopping_services", "Fehler beim Stoppen der Dienste: {0}" },
                        { "restarting_all_services", "Alle Dienste werden neu gestartet..." },
                        { "all_services_restarted", "Alle Dienste wurden neu gestartet!" },
                        { "error_restarting_services", "Fehler beim Neustarten der Dienste: {0}" },
                        { "select_component_install", "Wählen Sie eine Komponente zum Installieren aus." },
                        { "installing_component", "{0} wird installiert..." },
                        { "component_installed", "{0} erfolgreich installiert!" },
                        { "error_installing_component", "Fehler beim Installieren von {0}: {1}" },
                        { "opening_shell", "🚀 Interaktive Shell für {0} v{1} wird geöffnet" },
                        { "executing_component", "🚀 {0} v{1} wird ausgeführt" },
                        { "no_executable_found", "❌ Keine ausführbare Datei für {0} v{1} gefunden" },
                        { "version_folder_not_found", "❌ Versionsordner nicht gefunden: {0}" },
                        { "component_not_executable", "❌ Komponente {0} ist nicht ausführbar" },
                        { "error_executing", "❌ Fehler beim Ausführen von {0} v{1}: {2}" },
                        { "error_updating_component_data", "Fehler beim Aktualisieren der Komponentendaten: {0}" },
                        { "error_updating_service_data", "Fehler beim Aktualisieren der Dienstdaten: {0}" }
                    }
                    }
                }
                },
                { "installed_tab", new Dictionary<string, object>
                {
                    { "title", "Installierte Werkzeuge" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "tool", "Werkzeug" },
                        { "versions", "Installierte Versionen" },
                        { "status", "Status" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Liste aktualisieren" }
                    }
                    },
                    { "info", "Verwenden Sie die Registerkarten 'Installieren' und 'Deinstallieren', um Werkzeuge zu verwalten" },
                    { "loading", "Installierte Komponenten werden geladen..." },
                    { "loaded", "{0} Komponenten geladen" },
                    { "error", "Fehler beim Laden der Komponenten: {0}" }
                }
                },
                { "install_tab", new Dictionary<string, object>
                {
                    { "title", "Neues Werkzeug installieren" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Werkzeug auswählen:" },
                        { "select_version", "Version auswählen (leer für die neueste):" },
                        { "installed_component", "Installierte Komponente:" },
                        { "installed_version", "Installierte Version:" }
                    }
                    },
                    { "sections", new Dictionary<string, object>
                    {
                        { "install_component", "Komponente installieren" },
                        { "create_shortcuts", "Verknüpfungen für installierte Komponenten erstellen" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "install", "📥 Installieren" },
                        { "create_shortcut", "Verknüpfung erstellen" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Wählen Sie eine Komponente zum Installieren aus." },
                        { "select_component_warning", "Wählen Sie eine Komponente aus" },
                        { "select_version_warning", "Wählen Sie eine Version aus" },
                        { "installing", "{0} wird installiert..." },
                        { "success", "{0} erfolgreich installiert!" },
                        { "error", "Fehler bei der Installation von {0}" },
                        { "loading_versions", "Versionen von {0} werden geladen..." },
                        { "versions_loaded", "{0} Versionen für {1} geladen" },
                        { "versions_error", "Fehler beim Laden der Versionen: {0}" },
                        { "component_not_found", "Komponente '{0}' nicht gefunden" },
                        { "failed_to_load_versions", "Fehler beim Laden der Versionen" },
                        { "shortcut_component_not_found", "Komponente '{0}' nicht gefunden" },
                        { "shortcut_not_supported", "Komponente '{0}' unterstützt keine Verknüpfungserstellung" },
                        { "shortcut_install_dir_not_found", "Installationsverzeichnis nicht gefunden: {0}" }
                    }
                    }
                }
                },
                { "uninstall_tab", new Dictionary<string, object>
                {
                    { "title", "Werkzeug deinstallieren" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Werkzeug auswählen:" },
                        { "select_version", "Version auswählen:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "uninstall", "🗑️ Deinstallieren" },
                        { "refresh", "🔄 Liste aktualisieren" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Wählen Sie eine Komponente zum Deinstallieren aus." },
                        { "select_version", "Wählen Sie eine Version zum Deinstallieren aus." },
                        { "confirm", "Sind Sie sicher, dass Sie {0} deinstallieren möchten?" },
                        { "uninstalling", "{0} wird deinstalliert..." },
                        { "success", "{0} erfolgreich deinstalliert!" },
                        { "error", "Fehler bei der Deinstallation von {0}" },
                        { "no_versions", "{0} hat keine installierten Versionen." },
                        { "not_installed", "{0} ist nicht installiert" },
                        { "loading_components", "Installierte Komponenten werden geladen..." },
                        { "loading_versions", "Installierte Versionen von {0} werden geladen..." },
                        { "versions_loaded", "Versionen für {0} geladen" },
                        { "versions_error", "Fehler beim Laden der Versionen zur Deinstallation: {0}" },
                        { "components_available", "{0} Komponenten verfügbar zur Deinstallation" },
                        { "reloading", "Liste der installierten Komponenten wird neu geladen..." }
                    }
                    },
                    { "warning", "Achtung: Diese Aktion kann nicht rückgängig gemacht werden!" },
                    { "status", new Dictionary<string, object>
                    {
                        { "uninstalling", "{0} wird deinstalliert..." },
                        { "success", "{0} erfolgreich deinstalliert!" },
                        { "error", "❌ Fehler bei der Deinstallation von {0}: {1}" },
                        { "error_short", "Fehler bei der Deinstallation von {0}" },
                        { "loading_versions", "Installierte Versionen von {0} werden geladen..." },
                        { "versions_loaded", "Versionen für {0} geladen" },
                        { "not_installed", "{0} ist nicht installiert" },
                        { "error_loading_versions", "Fehler beim Laden der Versionen zur Deinstallation: {0}" },
                        { "loading_components", "Installierte Komponenten werden geladen..." },
                        { "components_count", "{0} Komponenten verfügbar zur Deinstallation" },
                        { "reloading", "Liste der installierten Komponenten wird neu geladen..." },
                        { "error_loading_components", "Fehler beim Laden der Komponenten: {0}" }
                    }
                    }
                }
                },
                { "services_tab", new Dictionary<string, object>
                {
                    { "title", "Dienstverwaltung" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "component", "Komponente" },
                        { "version", "Version" },
                        { "status", "Status" },
                        { "pid", "PID" },
                        { "copy_pid", "PID kopieren" },
                        { "actions", "Aktionen" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Aktualisieren" },
                        { "start_all", "▶️ Alle starten" },
                        { "stop_all", "⏹️ Alle stoppen" },
                        { "restart_all", "🔄 Alle neu starten" },
                        { "start", "▶️" },
                        { "stop", "⏹️" },
                        { "restart", "🔄" },
                        { "copy_pid", "📋" }
                    }
                    },
                    { "tooltips", new Dictionary<string, object>
                    {
                        { "start", "Starten" },
                        { "stop", "Stoppen" },
                        { "restart", "Neustarten" },
                        { "copy_pid", "PID kopieren" }
                    }
                    },
                    { "status", new Dictionary<string, object>
                    {
                        { "running", "Läuft" },
                        { "stopped", "Gestoppt" },
                        { "active", "Aktiv" }
                    }
                    },
                    { "types", new Dictionary<string, object>
                    {
                        { "php_fpm", "PHP-FPM" },
                        { "web_server", "Webserver" },
                        { "database", "Datenbank" },
                        { "search_engine", "Suchmaschine" },
                        { "service", "Dienst" },
                        { "fastcgi", "FastCGI" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "loading", "Dienste werden geladen..." },
                        { "loaded", "{0} Dienste geladen" },
                        { "error", "Fehler beim Laden der Dienste: {0}" },
                        { "starting", "{0} Version {1} wird gestartet..." },
                        { "started", "{0} erfolgreich gestartet" },
                        { "stopping", "{0} Version {1} wird gestoppt..." },
                        { "stopped", "{0} erfolgreich gestoppt" },
                        { "restarting", "{0} Version {1} wird neu gestartet..." },
                        { "restarted", "{0} erfolgreich neu gestartet" },
                        { "starting_all", "Alle Dienste werden gestartet..." },
                        { "started_all", "Alle Dienste gestartet" },
                        { "stopping_all", "Alle Dienste werden gestoppt..." },
                        { "stopped_all", "Alle Dienste gestoppt" },
                        { "restarting_all", "Alle Dienste werden neu gestartet..." },
                        { "restarted_all", "Alle Dienste neu gestartet" },
                        { "pid_copied", "PID {0} in die Zwischenablage kopiert" },
                        { "no_pid", "Dienst läuft nicht, kein PID zum Kopieren." },
                        { "error_copy_pid", "Fehler beim Kopieren des PID: {0}" },
                        { "error_start", "Fehler beim Starten des Dienstes: {0}" },
                        { "error_stop", "Fehler beim Stoppen des Dienstes: {0}" },
                        { "error_restart", "Fehler beim Neustarten des Dienstes: {0}" },
                        { "error_start_all", "Fehler beim Starten aller Dienste: {0}" },
                        { "error_stop_all", "Fehler beim Stoppen aller Dienste: {0}" },
                        { "error_restart_all", "Fehler beim Neustarten aller Dienste: {0}" }
                    }
                    },
                    { "path_manager", new Dictionary<string, object>
                    {
                        { "not_initialized", "⚠️ PathManager wurde nicht initialisiert - PATH wurde nicht aktualisiert" }
                    }
                    },
                    { "debug", new Dictionary<string, object>
                    {
                        { "processes_found", "Gefundene Prozesse für Debug: {0}" },
                        { "process_info", "  - {0} (PID: {1}) - Pfad: {2}" },
                        { "process_error", "  - {0} (PID: {1}) - Pfad: Fehler beim Zugriff ({2})" },
                        { "found_service_components", "{0} Dienstkomponenten gefunden" },
                        { "component_dir_not_found", "Komponentenverzeichnis {0} nicht gefunden: {1}" },
                        { "component_versions_found", "Komponente {0}: {1} Versionen gefunden: {2}" },
                        { "checking_component_version", "{0} Version {1} wird geprüft" },
                        { "service_process_found", "  - {0} Prozess gefunden: {1} (PID: {2}) - Pfad: {3}" },
                        { "service_running", "{0} {1} läuft mit PIDs: {2}" },
                        { "service_not_running", "{0} {1} läuft nicht" },
                        { "no_service_pattern", "Kein Dienstmuster für {0} definiert" },
                        { "component_check_error", "Fehler beim Prüfen der {0} Prozesse: {1}" },
                        { "php_dirs_found", "{0} PHP-Verzeichnisse gefunden: {1}" },
                        { "checking_php_version", "PHP Version {0} im Verzeichnis {1} wird geprüft" },
                        { "php_process_found", "  - PHP-Prozess gefunden: {0} (PID: {1}) - Pfad: {2}" },
                        { "process_check_error", "  - Fehler beim Prüfen des Prozesses {0}: {1}" },
                        { "php_running", "PHP {0} läuft mit PIDs: {1}" },
                        { "php_not_running", "PHP {0} läuft nicht" },
                        { "php_check_error", "Fehler beim Prüfen der PHP-Prozesse: {0}" },
                        { "nginx_dirs_found", "{0} Nginx-Verzeichnisse gefunden: {1}" },
                        { "checking_nginx_version", "Nginx Version {0} im Verzeichnis {1} wird geprüft" },
                        { "nginx_process_found", "  - Nginx-Prozess gefunden: {0} (PID: {1}) - Pfad: {2}" },
                        { "nginx_running", "Nginx {0} läuft mit PID: {1}" },
                        { "nginx_not_running", "Nginx {0} läuft nicht" },
                        { "nginx_check_error", "Fehler beim Prüfen der Nginx-Prozesse: {0}" },
                        { "load_services_error", "Fehler beim Laden der Dienste in der GUI: {0}" },
                        { "start_all_services_error", "Fehler beim Starten aller Dienste in der GUI: {0}" },
                        { "stop_all_services_error", "Fehler beim Stoppen aller Dienste in der GUI: {0}" },
                        { "restart_all_services_error", "Fehler beim Neustarten aller Dienste in der GUI: {0}" }
                    }
                    }
                }
                },
                { "sidebar", new Dictionary<string, object>
                {
                    { "title", "DevStack Manager" },
                    { "navigation_items", new Dictionary<string, object>
                    {
                        { "dashboard", new Dictionary<string, object>
                        {
                            { "title", "Dashboard" },
                            { "description", "Systemübersicht" }
                        }
                        },
                        { "installed", new Dictionary<string, object>
                        {
                            { "title", "Installiert" },
                            { "description", "Installierte Werkzeuge" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Installieren" },
                            { "description", "Neue Komponenten installieren" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Deinstallieren" },
                            { "description", "Komponenten entfernen" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Dienste" },
                            { "description", "Dienststeuerung" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Einstellungen" },
                            { "description", "Systemeinstellungen" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Websites" },
                            { "description", "Nginx-Websites verwalten" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Dienstprogramme" },
                            { "description", "Werkzeuge und Konsole" }
                        }
                        }
                    }
                    }
                }
                },
                { "config_tab", new Dictionary<string, object>
                {
                    { "title", "Einstellungen" },
                    { "path", new Dictionary<string, object>
                    {
                        { "title", "PATH-Verwaltung" },
                        { "description", "Werkzeuge zum System-PATH hinzufügen" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "add", "➕ Zum PATH hinzufügen" },
                            { "remove", "➖ Aus PATH entfernen" }
                        }
                        }
                    }
                    },
                    { "directories", new Dictionary<string, object>
                    {
                        { "title", "Verzeichnisse" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "devstack_manager", "📂 DevStack Manager" },
                            { "tools", "📂 Werkzeuge" }
                        }
                        }
                    }
                    },
                    { "languages", new Dictionary<string, object>
                    {
                        { "title", "Sprachen" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_language", "Oberflächensprache" }
                        }
                        },
                        { "messages", new Dictionary<string, object>
                        {
                            { "language_changed", "Sprache geändert zu {0}" }
                        }
                        }
                    }
                    },
                    { "themes", new Dictionary<string, object>
                    {
                        { "title", "Themen" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_theme", "Thema der Benutzeroberfläche" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "path_updated", "PATH erfolgreich aktualisiert" },
                        { "path_update_error", "Fehler beim Aktualisieren des PATH" },
                        { "path_cleaned", "PATH erfolgreich bereinigt" },
                        { "path_listed", "PATH angezeigt" },
                        { "path_error", "Fehler beim Hinzufügen zum PATH: {0}" },
                        { "path_remove_error", "Fehler beim Entfernen aus PATH: {0}" },
                        { "path_clean_error", "Fehler beim Bereinigen des PATH" },
                        { "path_list_error", "Fehler beim Anzeigen des PATH: {0}" },
                        { "exe_folder_opened", "Ausführungsordner geöffnet" },
                        { "exe_folder_not_found", "Ausführungsordner konnte nicht gefunden werden." },
                        { "exe_folder_error", "Fehler beim Öffnen des Ausführungsordners: {0}" },
                        { "tools_folder_opened", "Werkzeuge-Ordner geöffnet" },
                        { "tools_folder_not_found", "Werkzeuge-Ordner konnte nicht gefunden werden." },
                        { "tools_folder_error", "Fehler beim Öffnen des Werkzeuge-Ordners: {0}" }
                    }
                    }
                }
                },
                { "sites_tab", new Dictionary<string, object>
                {
                    { "title", "Nginx-Website-Konfiguration erstellen" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "domain", "Domain der Website:" },
                        { "root_directory", "Stammverzeichnis:" },
                        { "php_upstream", "PHP Upstream:" },
                        { "nginx_version", "Nginx-Version:" },
                        { "ssl_domain", "Domain für SSL:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "browse", "📁 Durchsuchen" },
                        { "create_site", "🌐 Website-Konfiguration erstellen" },
                        { "generate_ssl", "🔒 SSL-Zertifikat generieren" }
                    }
                    },
                    { "ssl", new Dictionary<string, object>
                    {
                        { "title", "SSL-Zertifikate" },
                        { "generate_ssl", "SSL generieren" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_folder", "Website-Ordner auswählen" },
                        { "creating_site", "Konfiguration für die Website {0} wird erstellt..." },
                        { "site_created", "Website {0} erstellt" },
                        { "site_error", "Fehler beim Erstellen der Website {0}: {1}" },
                        { "site_config_error", "Fehler beim Erstellen der Website-Konfiguration: {0}" },
                        { "enter_domain", "Geben Sie eine Domain für die Website ein." },
                        { "enter_root", "Geben Sie ein Stammverzeichnis für die Website ein." },
                        { "select_php", "Wählen Sie eine PHP-Version für die Website aus." },
                        { "select_nginx", "Wählen Sie eine Nginx-Version für die Website aus." },
                        { "enter_ssl_domain", "Geben Sie eine Domain zum Generieren des SSL-Zertifikats ein." },
                        { "domain_not_exists", "Die Domain '{0}' existiert nicht oder löst auf keine IP auf." },
                        { "generating_ssl", "SSL-Zertifikat für {0} wird generiert..." },
                        { "ssl_generated", "SSL-Generierung für {0} abgeschlossen." },
                        { "ssl_error", "Fehler beim Generieren des SSL-Zertifikats: {0}" },
                        { "restarting_nginx", "Nginx-Dienste werden neu gestartet..." },
                        { "nginx_restarted", "Nginx v{0} erfolgreich neu gestartet" },
                        { "nginx_restart_error", "Fehler beim Neustarten von Nginx v{0}: {1}" },
                        { "nginx_restart_general_error", "Fehler beim Neustarten von Nginx: {0}" },
                        { "ssl_generation_completed", "SSL-Generierung für {0} abgeschlossen." },
                        { "ssl_generation_error", "❌ Fehler beim Generieren des SSL-Zertifikats: {0}" },
                        { "ssl_generation_error_status", "Fehler beim Generieren von SSL für {0}" },
                        { "ssl_generation_error_dialog", "Fehler beim Generieren des SSL-Zertifikats: {0}" },
                        { "no_nginx_restarted", "ℹ️ Keine Nginx-Version wurde neu gestartet (möglicherweise nicht aktiv)" },
                        { "no_nginx_found", "❌ Keine installierte Nginx-Version gefunden" }
                    }
                    },
                    { "info", "Die Konfigurationsdateien werden automatisch erstellt" }
                }
                },
                { "utilities_tab", new Dictionary<string, object>
                {
                    { "title", "DevStack-Konsole - Befehle direkt ausführen" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "command", "Befehl:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "execute", "▶️ Ausführen" },
                        { "clear", "❌" },
                        { "clear_tooltip", "Konsole leeren" }
                    }
                    },
                    { "console_title", "DevStack-Konsole - Befehle direkt ausführen" },
                    { "command_label", "Befehl:" },
                    { "execute_button", "▶️ Ausführen" },
                    { "clear_console_tooltip", "Konsole leeren" },
                    { "status_button", "Status" },
                    { "installed_button", "Installiert" },
                    { "diagnostic_button", "Diagnose" },
                    { "test_button", "Testen" },
                    { "help_button", "Hilfe" },
                    { "console_header", "DevStack Manager Konsole" },
                    { "available_commands", "Verfügbare Befehle:" },
                    { "tip_message", "Tipp: Geben Sie Befehle direkt oben ein oder verwenden Sie die Schnellzugriffstasten" },
                    { "executing_command", "Wird ausgeführt: {0}" },
                    { "no_output", "(Befehl ausgeführt, keine Ausgabe erzeugt)" },
                    { "devstack_not_found", "Fehler: DevStack.exe konnte nicht gestartet werden" },
                    { "error", "FEHLER" },
                    { "console_cleared", "Konsole geleert.\n\n" },
                    { "empty_command", "Leerer Befehl" },
                    { "command_execution_error", "Fehler beim Ausführen des Befehls: {0}" },
                    { "status", new Dictionary<string, object>
                    {
                        { "executing", "Wird ausgeführt: {0}" },
                        { "executed", "Befehl ausgeführt" },
                        { "error", "Fehler beim Ausführen des Befehls" },
                        { "cleared", "Konsole geleert" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "list_usage", "Verwendung: list --installed oder list <Komponente>" },
                        { "command_not_recognized", "Befehl '{0}' nicht erkannt. Verwenden Sie 'help', um verfügbare Befehle anzuzeigen." }
                    }
                    }
                }
                },
                { "console", new Dictionary<string, object>
                {
                    { "titles", new Dictionary<string, object>
                    {
                        { "install", "Konsolenausgabe - Installation" },
                        { "uninstall", "Konsolenausgabe - Deinstallation" },
                        { "sites", "Konsolenausgabe - Websites" },
                        { "config", "Konsolenausgabe - Einstellungen" },
                        { "utilities", "Konsolenausgabe" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "clear", "🗑️ Konsole leeren" }
                    }
                    }
                }
                },
                { "common", new Dictionary<string, object>
                {
                    { "status", new Dictionary<string, object>
                    {
                        { "ok", "✔️" },
                        { "error", "❌" },
                        { "na", "N/A" }
                    }
                    },
                    { "dialogs", new Dictionary<string, object>
                    {
                        { "warning", "Warnung" },
                        { "error", "Fehler" },
                        { "info", "Information" },
                        { "confirmation", "Bestätigung" }
                    }
                    }
                }
                },
                { "status_bar", new Dictionary<string, object>
                {
                    { "refresh_tooltip", "Status aktualisieren" },
                    { "updating", "Aktualisiere..." },
                    { "updated", "Status aktualisiert" },
                    { "loading_data", "Starte Daten laden..." },
                    { "loading_installed", "Lade installierte Komponenten..." },
                    { "loading_available", "Lade verfügbare Komponenten..." },
                    { "loading_services", "Lade Dienste und andere Optionen..." },
                    { "loading_complete", "Alle Daten erfolgreich geladen" },
                    { "loading_error", "Fehler beim Laden der Daten: {0}" },
                    { "shortcut_created", "Verknüpfung erfolgreich erstellt für {0} {1}" },
                    { "shortcut_error", "Fehler beim Erstellen der Verknüpfung für {0}" },
                    { "shortcut_create_error", "Fehler beim Erstellen der Verknüpfung: {0}" },
                    { "creating_shortcut", "Erstelle Verknüpfung für {0} {1}..." },
                    { "error_loading_initial", "Fehler beim Laden der Anfangsdaten: {0}" },
                    { "error_loading_components", "Fehler beim Laden der Komponenten: {0}" },
                    { "error_loading_shortcuts", "Fehler beim Laden der Komponenten für Verknüpfungen: {0}" },
                    { "error_loading_versions", "Fehler beim Laden der Versionen für Verknüpfung: {0}" },
                    { "error_loading_dashboard", "Fehler beim Laden der Dashboard-Daten: {0}" },
                    { "opening_shell", "Öffne interaktive Shell für {0} Version {1}" },
                    { "executing_component", "Führe {0} Version {1} aus: {2}" },
                    { "no_executable_found", "Keine ausführbare Datei gefunden in {0}" },
                    { "version_folder_not_found", "Versionsordner nicht gefunden: {0}" },
                    { "component_not_executable", "Komponente {0} ist nicht ausführbar oder nicht installiert." },
                    { "component_not_available", "Komponente konnte nicht für Ausführung abgerufen werden." },
                    { "version_not_available", "Version konnte nicht für Ausführung abgerufen werden." },
                    { "error_executing_component", "Fehler beim Ausführen der Komponente: {0}" }
                }
                }
            };
        }

        /// <summary>
        /// Gets translations specific to the DevStack installer application.
        /// </summary>
        /// <returns>Dictionary containing installer-specific translation strings.</returns>
        public Dictionary<string, object> GetInstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Installationsassistent" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "cancel_title", "Installation abbrechen" },
                    { "cancel_message", "Sind Sie sicher, dass Sie die Installation abbrechen möchten?" },
                    { "installation_error_title", "Fehler" },
                    { "installation_error_message", "Installation fehlgeschlagen: {0}" },
                    { "folder_dialog_title", "Installationsordner auswählen" },
                    { "startup_error_title", "DevStack Installationsfehler" },
                    { "startup_error_message", "Fehler beim Starten des Installationsprogramms: {0}\n\nDetails: {1}" },
                    { "initialization_error_title", "Initialisierungsfehler" },
                    { "initialization_error_message", "Fehler beim Initialisieren des Installationsfensters: {0}" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "Willkommen beim DevStack Manager" },
                    { "description", "Dieser Assistent führt Sie durch die Installation des DevStack Managers auf Ihrem Computer." },
                    { "app_name", "DevStack Manager" },
                    { "version", "Version {0}" },
                    { "app_description", "DevStack Manager ist ein umfassendes Tool zur Verwaltung von Entwicklungsumgebungen, das Ihnen hilft, verschiedene Entwicklungswerkzeuge und -dienste zu installieren, konfigurieren und verwalten.\n\nKlicken Sie auf 'Weiter', um mit der Installation fortzufahren." },
                    { "language_label", "Installationssprache:" }
                }
                },
                { "license", new Dictionary<string, object>
                {
                    { "title", "Lizenzvereinbarung" },
                    { "description", "Bitte lesen Sie die folgende Lizenzvereinbarung sorgfältig durch." },
                    { "label", "Bitte lesen und akzeptieren Sie die Lizenzvereinbarung:" },
                    { "text", "MIT-Lizenz\n\nCopyright (c) 2025 DevStackManager\n\nHiermit wird unentgeltlich jeder Person, die eine Kopie der Software und der zugehörigen Dokumentationsdateien (die \"Software\") erhält, die Erlaubnis erteilt, uneingeschränkt mit der Software zu handeln, einschließlich und ohne Einschränkung des Rechts, sie zu verwenden, zu kopieren, zu ändern, zusammenzuführen, zu veröffentlichen, zu verbreiten, unterzulizenzieren und/oder zu verkaufen, und Personen, denen die Software bereitgestellt wird, dies zu ermöglichen, unter den folgenden Bedingungen:\n\nDer obige Urheberrechtshinweis und dieser Erlaubnishinweis müssen in allen Kopien oder wesentlichen Teilen der Software enthalten sein.\n\nDIE SOFTWARE WIRD OHNE JEGLICHE GARANTIE, WEDER AUSDRÜCKLICH NOCH STILLSCHWEIGEND, BEREITGESTELLT, EINSCHLIESSLICH ABER NICHT BESCHRÄNKT AUF DIE GARANTIEN DER MARKTGÄNGIGKEIT, EIGNUNG FÜR EINEN BESTIMMTEN ZWECK UND NICHTVERLETZUNG. IN KEINEM FALL SIND DIE AUTOREN ODER URHEBERRECHTSINHABER FÜR ANSPRÜCHE, SCHÄDEN ODER SONSTIGE HAFTUNGEN VERANTWORTLICH, OB AUS VERTRAG, UNERLAUBTER HANDLUNG ODER ANDERWEITIG, DIE AUS DER SOFTWARE ODER DER BENUTZUNG ODER SONSTIGEN GESCHÄFTEN MIT DER SOFTWARE ENTSTEHEN." }
                }
                },
                { "installation_path", new Dictionary<string, object>
                {
                    { "title", "Installationsort wählen" },
                    { "description", "Wählen Sie den Ordner, in dem DevStack Manager installiert werden soll." },
                    { "label", "Zielordner:" },
                    { "browser", "Durchsuchen..." },
                    { "space_required", "Benötigter Speicherplatz: {0} MB" },
                    { "space_available", "Verfügbarer Speicherplatz: {0}" },
                    { "info", "DevStack Manager wird in diesem Ordner zusammen mit allen Komponenten und Einstellungen installiert." }
                }
                },
                { "components", new Dictionary<string, object>
                {
                    { "title", "Zusätzliche Optionen auswählen" },
                    { "description", "Wählen Sie zusätzliche Optionen für Ihre DevStack Manager Installation." },
                    { "label", "Zusätzliche Optionen:" },
                    { "desktop_shortcuts", "🖥️ Desktop-Verknüpfungen erstellen" },
                    { "start_menu_shortcuts", "📂 Startmenü-Verknüpfungen erstellen" },
                    { "add_to_path", "⚡ DevStack zum System-PATH hinzufügen (empfohlen)" },
                    { "path_info", "Das Hinzufügen zum PATH ermöglicht die Verwendung von DevStack-Befehlen direkt im Terminal von jedem Ort aus." }
                }
                },
                { "ready_to_install", new Dictionary<string, object>
                {
                    { "title", "Bereit zur Installation" },
                    { "description", "Der Assistent ist bereit, die Installation zu starten. Überprüfen Sie Ihre Einstellungen unten." },
                    { "summary_label", "Installationsübersicht:" },
                    { "destination", "Zielordner:" },
                    { "components_header", "Zu installierende Komponenten:" },
                    { "cli_component", "• DevStack CLI (Kommandozeilenschnittstelle)" },
                    { "gui_component", "• DevStack GUI (Grafische Oberfläche)" },
                    { "uninstaller_component", "• DevStack Deinstallationsprogramm" },
                    { "config_component", "• Konfigurationsdateien und Komponenten" },
                    { "options_header", "Zusätzliche Optionen:" },
                    { "create_desktop", "• Desktop-Verknüpfungen erstellen" },
                    { "create_start_menu", "• Startmenü-Verknüpfungen erstellen" },
                    { "add_path", "• Zum System-PATH hinzufügen" },
                    { "space_required_summary", "Benötigter Speicherplatz: {0} MB" }
                }
                },
                { "installing", new Dictionary<string, object>
                {
                    { "title", "DevStack Manager wird installiert" },
                    { "description", "Bitte warten Sie, während DevStack Manager installiert wird..." },
                    { "preparing", "Installation wird vorbereitet..." },
                    { "extracting", "Installationsdateien werden extrahiert..." },
                    { "downloading_sdk", ".NET SDK wird heruntergeladen..." },
                    { "compiling_projects", "DevStack-Projekte werden kompiliert..." },
                    { "creating_directory", "Installationsverzeichnis wird erstellt..." },
                    { "installing_files", "DevStack-Dateien werden installiert..." },
                    { "registering", "Installation wird registriert..." },
                    { "creating_desktop", "Desktop-Verknüpfungen werden erstellt..." },
                    { "creating_start_menu", "Startmenü-Verknüpfungen werden erstellt..." },
                    { "adding_path", "Zum System-PATH wird hinzugefügt..." },
                    { "completed", "Installation erfolgreich abgeschlossen!" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Installation abgeschlossen" },
                    { "description", "DevStack Manager wurde erfolgreich auf Ihrem Computer installiert." },
                    { "success_icon", "✅" },
                    { "success_title", "Installation erfolgreich abgeschlossen!" },
                    { "success_message", "DevStack Manager wurde erfolgreich installiert. Sie können die Anwendung jetzt zur Verwaltung Ihrer Entwicklungsumgebung verwenden." },
                    { "install_location", "Installationsort:" },
                    { "launch_now", "🚀 DevStack Manager jetzt starten" }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Installationsprozess wird gestartet" },
                    { "extracted", "Installationsdateien erfolgreich extrahiert" },
                    { "source_extracted", "Quelldateien extrahiert" },
                    { "downloading_sdk", ".NET SDK wird für die Kompilierung heruntergeladen..." },
                    { "sdk_downloaded", ".NET SDK heruntergeladen und extrahiert" },
                    { "compiling", "DevStack-Projekte werden kompiliert..." },
                    { "compilation_complete", "Kompilierung erfolgreich abgeschlossen" },
                    { "creating_dir", "Verzeichnis wird erstellt: {0}" },
                    { "installing", "Anwendungsdateien werden installiert" },
                    { "registering", "Installation wird unter Windows registriert" },
                    { "desktop_shortcuts", "Desktop-Verknüpfungen werden erstellt" },
                    { "start_menu_shortcuts", "Startmenü-Verknüpfungen werden erstellt" },
                    { "adding_path", "DevStack wird zum System-PATH hinzugefügt" },
                    { "path_added", "Erfolgreich zum Benutzer-PATH hinzugefügt" },
                    { "path_exists", "Bereits im PATH vorhanden" },
                    { "completed_success", "Installation erfolgreich abgeschlossen!" },
                    { "cleanup", "Temporäre Dateien bereinigt" },
                    { "cleanup_warning", "Warnung: Temporäre Datei konnte nicht gelöscht werden: {0}" },
                    { "shortcuts_warning", "Warnung: Desktop-Verknüpfungen konnten nicht erstellt werden: {0}" },
                    { "start_menu_warning", "Warnung: Startmenü-Verknüpfungen konnten nicht erstellt werden: {0}" },
                    { "path_warning", "Warnung: PATH konnte nicht hinzugefügt werden: {0}" }
                }
                }
            };
        }

        /// <summary>
        /// Gets translations specific to the DevStack uninstaller application.
        /// </summary>
        /// <returns>Dictionary containing uninstaller-specific translation strings.</returns>
        public Dictionary<string, object> GetUninstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Deinstallationsprogramm" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "uninstall_error_title", "Deinstallationsfehler" },
                    { "uninstall_error_message", "Fehler bei der Deinstallation: {0}" },
                    { "startup_error_title", "DevStack Deinstallationsfehler" },
                    { "startup_error_message", "Fehler beim Starten der Deinstallation: {0}\n\nDetails: {1}" },
                    { "initialization_error_title", "Initialisierungsfehler" },
                    { "initialization_error_message", "Fehler beim Initialisieren des Deinstallationsfensters: {0}" },
                    { "cancel_title", "Deinstallation abbrechen" },
                    { "cancel_message", "Sind Sie sicher, dass Sie die Deinstallation abbrechen möchten?" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "DevStack Deinstallationsprogramm" },
                    { "description", "Dieser Assistent entfernt DevStack von Ihrem Computer" },
                    { "app_name", "DevStack Deinstallationsprogramm" },
                    { "version", "Version {0}" },
                    { "app_description", "Dieser Assistent führt Sie durch den vollständigen Entfernungsprozess von DevStack von Ihrem System." },
                    { "language_label", "Sprache:" }
                }
                },
                { "confirmation", new Dictionary<string, object>
                {
                    { "title", "Deinstallationsbestätigung" },
                    { "description", "Bitte bestätigen Sie, dass Sie mit der Entfernung von DevStack fortfahren möchten" },
                    { "warning_title", "⚠️ Achtung - Diese Aktion kann nicht rückgängig gemacht werden" },
                    { "warning_text", "Die Deinstallation entfernt DevStack vollständig von Ihrem System, einschließlich:" },
                    { "items", new Dictionary<string, object>
                    {
                        { "program_files", "• Alle Programmdateien" },
                        { "user_data", "• Benutzerkonfigurationen und -daten" },
                        { "shortcuts", "• Desktop- und Startmenü-Verknüpfungen" },
                        { "registry", "• Windows-Registrierungseinträge" },
                        { "services", "• Zugehörige Dienste und Prozesse" },
                        { "path_variables", "• PATH-Umgebungsvariablen" }
                    }
                    },
                    { "install_found", "📁 Installationsordner gefunden:" },
                    { "install_not_found", "❌ Installationsordner nicht automatisch gefunden" },
                    { "install_not_found_desc", "DevStack ist möglicherweise nicht korrekt installiert oder wurde bereits entfernt. Die Deinstallation bereinigt nur verbleibende Einträge und Verknüpfungen." },
                    { "space_to_free", "📊 Freizugebender Speicherplatz: {0}" }
                }
                },
                { "uninstall_options", new Dictionary<string, object>
                {
                    { "title", "Deinstallationsoptionen" },
                    { "description", "Wählen Sie aus, was während der Deinstallation entfernt werden soll" },
                    { "label", "Wählen Sie die zu entfernenden Komponenten:" },
                    { "user_data", "🗂️ Benutzerdateien und Einstellungen entfernen" },
                    { "user_data_desc", "Enthält Einstellungen, Protokolle und vom DevStack gespeicherte Daten" },
                    { "registry", "🔧 Registrierungseinträge entfernen" },
                    { "registry_desc", "Entfernt Registrierungsschlüssel und Installationsinformationen" },
                    { "shortcuts", "🔗 Verknüpfungen entfernen" },
                    { "shortcuts_desc", "Entfernt Desktop- und Startmenü-Verknüpfungen" },
                    { "path", "🛤️ Aus dem System-PATH entfernen" },
                    { "path_desc", "Entfernt den DevStack-Pfad aus den Umgebungsvariablen" },
                    { "info", "Wir empfehlen, alle Optionen für eine vollständige Entfernung ausgewählt zu lassen." }
                }
                },
                { "ready_to_uninstall", new Dictionary<string, object>
                {
                    { "title", "Bereit zur Deinstallation" },
                    { "description", "Überprüfen Sie die Einstellungen und klicken Sie auf Deinstallieren, um fortzufahren" },
                    { "summary_label", "Deinstallationsübersicht:" },
                    { "components_header", "ZU ENTFERNENDE KOMPONENTEN:" },
                    { "installation_location", "📁 Installationsort:" },
                    { "not_found", "Nicht gefunden" },
                    { "program_components", "🗂️ Programmkomponenten:" },
                    { "executables", "  • Ausführbare Dateien (DevStack.exe, DevStackGUI.exe)" },
                    { "libraries", "  • Bibliotheken und Abhängigkeiten" },
                    { "config_files", "  • Konfigurationsdateien" },
                    { "documentation", "  • Dokumentation und Ressourcen" },
                    { "selected_options", "AUSGEWÄHLTE OPTIONEN:" },
                    { "user_data_selected", "✓ Benutzerdaten werden entfernt" },
                    { "user_data_preserved", "✗ Benutzerdaten werden behalten" },
                    { "registry_selected", "✓ Registrierungseinträge werden entfernt" },
                    { "registry_preserved", "✗ Registrierungseinträge werden behalten" },
                    { "shortcuts_selected", "✓ Verknüpfungen werden entfernt" },
                    { "shortcuts_preserved", "✗ Verknüpfungen werden behalten" },
                    { "path_selected", "✓ Aus dem System-PATH wird entfernt" },
                    { "path_preserved", "✗ Bleibt im System-PATH" },
                    { "space_to_free", "💾 Freizugebender Speicherplatz: {0}" }
                }
                },
                { "uninstalling", new Dictionary<string, object>
                {
                    { "title", "Deinstallation läuft" },
                    { "description", "Bitte warten Sie, während DevStack von Ihrem System entfernt wird" },
                    { "preparing", "Deinstallation wird vorbereitet..." },
                    { "stopping_services", "Dienste werden gestoppt..." },
                    { "removing_shortcuts", "Verknüpfungen werden entfernt..." },
                    { "cleaning_registry", "Registrierung wird bereinigt..." },
                    { "removing_path", "Aus dem PATH wird entfernt..." },
                    { "removing_files", "Dateien werden entfernt..." },
                    { "removing_user_data", "Benutzerdaten werden entfernt..." },
                    { "finalizing", "Abschluss..." },
                    { "completed", "Deinstallation abgeschlossen!" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Deinstallation abgeschlossen" },
                    { "description", "DevStack wurde erfolgreich von Ihrem System entfernt" },
                    { "success_icon", "✅" },
                    { "success_title", "Deinstallation abgeschlossen!" },
                    { "success_message", "DevStack wurde erfolgreich von Ihrem System entfernt. Alle ausgewählten Komponenten wurden bereinigt." },
                    { "summary_title", "📊 Deinstallationsübersicht:" },
                    { "files_removed", "• Dateien entfernt von: {0}" },
                    { "user_data_removed", "• Benutzerdaten entfernt" },
                    { "registry_cleaned", "• Registrierungseinträge bereinigt" },
                    { "shortcuts_removed", "• Verknüpfungen entfernt" },
                    { "path_removed", "• Aus dem System-PATH entfernt" },
                    { "system_clean", "Das System ist jetzt frei von DevStack." }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Deinstallationsprozess wird gestartet" },
                    { "stopping_services", "DevStack-Dienste werden gestoppt..." },
                    { "process_stopped", "Prozess {0} beendet" },
                    { "process_stop_warning", "Warnung: Prozess {0} konnte nicht beendet werden: {1}" },
                    { "stop_services_error", "Fehler beim Stoppen der Dienste: {0}" },
                    { "removing_shortcuts", "Verknüpfungen werden entfernt..." },
                    { "shortcut_removed", "Verknüpfung entfernt: {0}" },
                    { "start_menu_removed", "Startmenü-Ordner entfernt: {0}" },
                    { "shortcuts_error", "Fehler beim Entfernen von Verknüpfungen: {0}" },
                    { "cleaning_registry", "Registrierungseinträge werden bereinigt..." },
                    { "user_registry_removed", "Benutzer-Registrierungseinträge entfernt" },
                    { "machine_registry_removed", "Maschinen-Registrierungseinträge entfernt" },
                    { "uninstall_registry_removed", "Programme & Features-Eintrag entfernt" },
                    { "registry_error", "Fehler beim Bereinigen der Registrierung: {0}" },
                    { "removing_path", "Aus dem System-PATH wird entfernt..." },
                    { "user_path_removed", "Aus dem Benutzer-PATH entfernt" },
                    { "system_path_removed", "Aus dem System-PATH entfernt" },
                    { "system_path_warning", "Warnung: Aus dem System-PATH konnte nicht entfernt werden (Administratorrechte erforderlich)" },
                    { "path_error", "Fehler beim Entfernen aus dem PATH: {0}" },
                    { "removing_files", "Dateien werden entfernt aus {0}..." },
                    { "install_not_found", "Installationsordner nicht gefunden" },
                    { "files_removed_count", "{0} Dateien entfernt" },
                    { "dirs_removed_count", "{0} leere Ordner entfernt" },
                    { "file_remove_warning", "Warnung: {0} konnte nicht entfernt werden: {1}" },
                    { "files_error", "Fehler beim Entfernen von Dateien: {0}" },
                    { "removing_user_data", "Benutzerdaten werden entfernt..." },
                    { "user_data_removed", "Benutzerdaten entfernt: {0}" },
                    { "user_data_error", "Fehler beim Entfernen von Benutzerdaten: {0}" },
                    { "self_deletion_scheduled", "Automatische Entfernung des Deinstallers geplant" },
                    { "self_deletion_warning", "Warnung: Automatische Entfernung konnte nicht geplant werden: {0}" },
                    { "uninstall_success", "Deinstallation erfolgreich abgeschlossen!" }
                }
                }
            };
        }

        /// <summary>
        /// Gets translations specific to the DevStack CLI application.
        /// </summary>
        /// <returns>Dictionary containing CLI-specific translation strings.</returns>
        public Dictionary<string, object> GetCliTranslations()
        {
            return new Dictionary<string, object>
            {
                { "shell", new Dictionary<string, object>
                {
                    { "interactive_prompt", "DevStack Interaktive Shell. Tippen Sie 'help' für Hilfe oder 'exit' zum Beenden." },
                    { "prompt", "DevStack> " },
                    { "exit_code", "(Exit-Code: {0})" },
                    { "command_requires_admin", "Der Befehl '{0}' erfordert Administratorrechte." },
                    { "run_as_admin_hint", "Führen Sie DevStack als Administrator aus oder verwenden Sie 'DevStack.exe {0}' in einer Administrator-Eingabeaufforderung." }
                }
                },
                { "commands", new Dictionary<string, object>
                {
                    { "unknown", "Unbekannter Befehl: {0}" },
                    { "help_title", "DevStack CLI - Verfügbare Befehle:" },
                    { "gui_hint", "Für die grafische Oberfläche verwenden: DevStackGUI.exe" },
                    { "table_header_cmd", "Befehl" },
                    { "table_header_desc", "Beschreibung" },
                    { "help_install", "Installiert ein Tool oder eine bestimmte Version." },
                    { "help_uninstall", "Entfernt ein Tool oder eine bestimmte Version." },
                    { "help_list", "Listet verfügbare oder installierte Versionen auf." },
                    { "help_path", "Verwaltet PATH für installierte Tools." },
                    { "help_status", "Zeigt den Status aller Tools." },
                    { "help_test", "Testet alle installierten Tools." },
                    { "help_update", "Aktualisiert ein Tool auf die neueste Version." },
                    { "help_deps", "Überprüft Systemabhängigkeiten." },
                    { "help_alias", "Erstellt einen .bat-Alias für die Tool-Version." },
                    { "help_global", "Fügt DevStack zum PATH hinzu und erstellt globalen Alias." },
                    { "help_self_update", "Aktualisiert DevStackManager." },
                    { "help_clean", "Entfernt Logs und temporäre Dateien." },
                    { "help_backup", "Erstellt Backup von Konfigurationen und Logs." },
                    { "help_logs", "Zeigt die letzten Zeilen des Logs." },
                    { "help_enable", "Aktiviert einen Windows-Dienst." },
                    { "help_disable", "Deaktiviert einen Windows-Dienst." },
                    { "help_config", "Öffnet das Konfigurationsverzeichnis." },
                    { "help_reset", "Entfernt und installiert ein Tool neu." },
                    { "help_ssl", "Generiert selbstsigniertes SSL-Zertifikat." },
                    { "help_db", "Verwaltet grundlegende Datenbanken." },
                    { "help_service", "Listet DevStack-Dienste auf (Windows)." },
                    { "help_doctor", "DevStack-Umgebungsdiagnose." },
                    { "help_language", "Listet auf oder ändert die Sprache der Benutzeroberfläche." },
                    { "help_site", "Erstellt nginx-Site-Konfiguration." },
                    { "help_help", "Zeigt diese Hilfe." }
                }
                },
                { "status", new Dictionary<string, object>
                {
                    { "title", "DevStack Status:" },
                    { "installed", "{0} installiert:" },
                    { "running", "[läuft]" },
                    { "stopped", "[gestoppt]" },
                    { "installed_versions", "{0} installiert:" }
                }
                },
                { "test", new Dictionary<string, object>
                {
                    { "title", "Testen installierter Tools:" },
                    { "not_found", "{0}: nicht gefunden." },
                    { "error_executing", "{0}: Fehler beim Ausführen von {1}" },
                    { "tool_output", "{0}: {1}" }
                }
                },
                { "deps", new Dictionary<string, object>
                {
                    { "title", "Überprüfen von Systemabhängigkeiten..." },
                    { "missing_admin", "Administratorberechtigung" },
                    { "all_present", "Alle Abhängigkeiten sind vorhanden." },
                    { "missing_deps", "Fehlende Abhängigkeiten: {0}" }
                }
                },
                { "usage", new Dictionary<string, object>
                {
                    { "list", "Verwendung: DevStackManager list <php|node|python|composer|mysql|nginx|phpmyadmin|git|mongodb|pgsql|elasticsearch|wpcli|adminer|go|openssl|phpcsfixer|--installed>" },
                    { "site", "Verwendung: DevStackManager site <domain> -Root <verzeichnis> -PHP <php-upstream> -Nginx <nginx-version>" },
                    { "site_error_domain", "Fehler: Domain ist erforderlich." },
                    { "site_error_root", "Fehler: Root ist erforderlich." },
                    { "site_error_php", "Fehler: PHP ist erforderlich." },
                    { "site_error_nginx", "Fehler: Nginx ist erforderlich." },
                    { "start", "Verwendung: DevStackManager start <nginx|php|--all> [<x.x.x>]" },
                    { "start_version", "Verwendung: DevStackManager start <nginx|php> <x.x.x>" },
                    { "stop", "Verwendung: DevStackManager stop <nginx|php|--all> [<x.x.x>]" },
                    { "stop_version", "Verwendung: DevStackManager stop <nginx|php> <x.x.x>" },
                    { "restart", "Verwendung: DevStackManager restart <nginx|php|--all> [<x.x.x>]" },
                    { "restart_version", "Verwendung: DevStackManager restart <nginx|php> <x.x.x>" },
                    { "alias", "Verwendung: DevStackManager alias <komponente> <version>" },
                    { "enable", "Verwendung: DevStackManager enable <dienst>" },
                    { "disable", "Verwendung: DevStackManager disable <dienst>" },
                    { "reset", "Verwendung: DevStackManager reset <komponente>" },
                    { "db", "Verwendung: DevStackManager db <mysql|pgsql|mongo> <befehl> [args...]" }
                }
                },
                { "logs", new Dictionary<string, object>
                {
                    { "last_lines", "Letzte {0} Zeilen von {1}:" },
                    { "not_found", "Protokolldatei nicht gefunden." }
                }
                },
                { "service", new Dictionary<string, object>
                {
                    { "enabled", "Dienst {0} aktiviert." },
                    { "disabled", "Dienst {0} deaktiviert." },
                    { "error_enable", "Fehler beim Aktivieren des Dienstes {0}: {1}" },
                    { "error_disable", "Fehler beim Deaktivieren des Dienstes {0}: {1}" },
                    { "none_found", "Keine DevStack-Dienste gefunden." },
                    { "list_header", "Name                 Status           DisplayName" }
                }
                },
                { "config", new Dictionary<string, object>
                {
                    { "opened", "Konfigurationsverzeichnis geöffnet." },
                    { "not_found", "Konfigurationsverzeichnis nicht gefunden." }
                }
                },
                { "reset", new Dictionary<string, object>
                {
                    { "resetting", "Zurücksetzen von {0}..." },
                    { "completed", "{0} zurückgesetzt." }
                }
                },
                { "db", new Dictionary<string, object>
                {
                    { "mysql_not_found", "mysql.exe nicht gefunden." },
                    { "pgsql_not_found", "psql.exe nicht gefunden." },
                    { "mongo_not_found", "mongo.exe nicht gefunden." },
                    { "unknown_command_mysql", "Unbekannter MySQL-DB-Befehl." },
                    { "unknown_command_pgsql", "Unbekannter PostgreSQL-DB-Befehl." },
                    { "unknown_command_mongo", "Unbekannter MongoDB-DB-Befehl." },
                    { "unsupported_db", "Nicht unterstützte Datenbank: {0}" }
                }
                },
                { "doctor", new Dictionary<string, object>
                {
                    { "title", "DevStack-Umgebungsdiagnose:" },
                    { "path_synced", "PATH mit Benutzereinstellungen synchronisiert." },
                    { "path_header", "PATH (Prozess + Benutzer + DevStack)" },
                    { "user_header", "Benutzer" },
                    { "system_header", "System" }
                }
                },
                { "global", new Dictionary<string, object>
                {
                    { "added", "Verzeichnis {0} zum Benutzer-PATH hinzugefügt." },
                    { "already_exists", "Verzeichnis {0} ist bereits im Benutzer-PATH." },
                    { "run_anywhere", "Sie können jetzt 'DevStackManager' von überall im Terminal ausführen." }
                }
                },
                { "language", new Dictionary<string, object>
                {
                    { "available_title", "Verfügbare Sprachen:" },
                    { "current_marker", " (aktuell)" },
                    { "change_hint", "Um die Sprache zu ändern, verwenden Sie: DevStack language <code>" },
                    { "example", "Beispiel: DevStack language de_DE" },
                    { "not_found", "Sprache '{0}' nicht gefunden." },
                    { "available_list", "Verfügbare Sprachen:" },
                    { "changed", "Sprache geändert zu: {0} ({1})" },
                    { "note_gui", "Hinweis: Die Sprachänderung wirkt sich hauptsächlich auf die grafische Oberfläche (GUI) aus." },
                    { "note_cli", "Einige CLI-Befehle sind möglicherweise nicht vollständig übersetzt." },
                    { "error_changing", "Fehler beim Ändern der Sprache: {0}" }
                }
                },
                { "self_update", new Dictionary<string, object>
                {
                    { "updating", "Aktualisierung über git pull..." },
                    { "success", "DevStackManager erfolgreich aktualisiert." },
                    { "error", "Fehler beim Aktualisieren über git: {0}" },
                    { "not_git_repo", "Kein Git-Repository. Aktualisieren Sie manuell, indem Sie Dateien aus dem Repository kopieren." }
                }
                },
                { "clean", new Dictionary<string, object>
                {
                    { "completed", "Bereinigung abgeschlossen. ({0} Elemente entfernt)" }
                }
                },
                { "backup", new Dictionary<string, object>
                {
                    { "created", "Backup erstellt unter {0}" }
                }
                },
                { "path", new Dictionary<string, object>
                {
                    { "help_title", "Verwendung des path-Befehls:" },
                    { "help_add", "  path         - Tool-Verzeichnisse zum PATH hinzufügen" },
                    { "help_add_explicit", "  path add     - Tool-Verzeichnisse zum PATH hinzufügen" },
                    { "help_remove", "  path remove  - Alle DevStack-Verzeichnisse aus dem PATH entfernen" },
                    { "help_remove_specific", "  path remove <dir1> <dir2> ... - Bestimmte Verzeichnisse aus dem PATH entfernen" },
                    { "help_list", "  path list    - Alle Verzeichnisse im Benutzer-PATH auflisten" },
                    { "help_help", "  path help    - Diese Hilfe anzeigen" },
                    { "unknown_subcommand", "Unbekannter Unterbefehl: {0}" },
                    { "use_help", "Verwenden Sie 'path help', um verfügbare Befehle anzuzeigen." },
                    { "manager_not_initialized", "PathManager nicht initialisiert." }
                }
                },
                { "alias", new Dictionary<string, object>
                {
                    { "created", "Alias erstellt: {0}" },
                    { "executable_not_found", "Ausführbare Datei nicht gefunden für {0} {1}" }
                }
                },
                { "directories", new Dictionary<string, object>
                {
                    { "nginx_not_found", "Nginx-Verzeichnis nicht gefunden. Wird übersprungen." },
                    { "php_not_found", "PHP-Verzeichnis nicht gefunden. Wird übersprungen." }
                }
                },
                { "error", new Dictionary<string, object>
                {
                    { "unexpected", "Unerwarteter Fehler: {0}" },
                    { "admin_request", "Fehler beim Anfordern von Administratorrechten: {0}" },
                    { "list_services", "Fehler beim Auflisten der Dienste: {0}" }
                }
                }
            };
        }

        /// <summary>
        /// Gets all translations combined (common, shared, GUI, installer, uninstaller, and CLI).
        /// </summary>
        /// <returns>Dictionary containing all translation strings merged together.</returns>
        public Dictionary<string, object> GetAllTranslations()
        {
            var all = new Dictionary<string, object>();
            
            all["common"] = GetCommonTranslations();
            all["shared"] = GetSharedTranslations();
            all["gui"] = GetGuiTranslations();
            all["cli"] = GetCliTranslations();
            all["installer"] = GetInstallerTranslations();
            all["uninstaller"] = GetUninstallerTranslations();
            
            return all;
        }
    }
}
