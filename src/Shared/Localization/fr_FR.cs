using System.Collections.Generic;

namespace DevStackShared.Localization
{
    public class fr_FR : ILanguageProvider
    {
        public string LanguageCode => "fr_FR";
        public string LanguageName => "Français";

        public Dictionary<string, object> GetCommonTranslations()
        {
            return new Dictionary<string, object>
            {
                { "language_name", "Français" },
                { "unknown", "Inconnu" },
                { "themes", new Dictionary<string, object>
                {
                    { "light", "Clair" },
                    { "dark", "Sombre" },
                    { "messages", new Dictionary<string, object>
                    {
                        { "theme_changed", "Thème changé en {0}" }
                    }
                    }
                }
                },
                { "buttons", new Dictionary<string, object>
                {
                    { "back", "← Retour" },
                    { "next", "Suivant →" },
                    { "accept", "J'accepte" },
                    { "install", "Installer" },
                    { "finish", "Terminer" },
                    { "cancel", "Annuler" },
                    { "continue", "Continuer" },
                    { "uninstall", "🗑️ Désinstaller" },
                    { "yes", "Oui" },
                    { "no", "Non" },
                    { "ok", "OK" }
                }
                },
                { "dialogs", new Dictionary<string, object>
                {
                    { "default_title", "Message" }
                }
                }
            };
        }

        public Dictionary<string, object> GetSharedTranslations()
        {
            return new Dictionary<string, object>
            {
                { "uninstall", new Dictionary<string, object>
                {
                    { "no_component", "Aucun composant spécifié pour la désinstallation." },
                    { "removing_shortcut", "Suppression du raccourci pour {0}..." },
                    { "unknown_component", "Composant inconnu : {0}" },
                    { "finished", "Désinstallation terminée." }
                }
                },
                { "shortcuts", new Dictionary<string, object>
                {
                    { "created", "Raccourci {0} créé pointant vers {1}" },
                    { "error_creating", "Erreur lors de la création du lien symbolique : {0}" },
                    { "fallback_copy", "Alternative : Copie {0} créée dans {1}" },
                    { "file_not_found", "Attention : fichier {0} introuvable pour créer le raccourci" },
                    { "removed", "Raccourci {0} supprimé" },
                    { "not_found", "Raccourci {0} introuvable pour suppression" },
                    { "error_removing", "Erreur lors de la suppression du raccourci : {0}" }
                }
                },
                { "install", new Dictionary<string, object>
                {
                    { "already_installed", "{0} {1} est déjà installé." },
                    { "downloading", "Téléchargement de {0} {1}..." },
                    { "running_installer", "Exécution de l'installeur {0} {1}..." },
                    { "installed_via_installer", "{0} {1} installé via installeur dans {2}" },
                    { "extracting", "Extraction..." },
                    { "installed", "{0} {1} installé." },
                    { "installed_in", "{0} {1} installé dans {2}." },
                    { "error_installing", "Erreur lors de l'installation de {0} {1} : {2}" },
                    { "shortcut_creation_failed", "Attention : échec de la création du raccourci : {0}" },
                    { "component_installed", "{0} {1} installé." }
                }
                }
            };
        }

        public Dictionary<string, object> GetGuiTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window", new Dictionary<string, object>
                {
                    { "title", "DevStack Manager v{0}" },
                    { "ready_status", "Prêt" },
                    { "initialization_error", "Erreur lors de l'initialisation de DevStack GUI : {0}" },
                    { "error_title", "DevStack Manager - Erreur" }
                }
                },
                { "navigation", new Dictionary<string, object>
                {
                    { "title", "DevStack Manager" },
                    { "items", new Dictionary<string, object>
                    {
                        { "dashboard", new Dictionary<string, object>
                        {
                            { "title", "Tableau de bord" },
                            { "description", "Vue d'ensemble du système" }
                        }
                        },
                        { "installed", new Dictionary<string, object>
                        {
                            { "title", "Installés" },
                            { "description", "Outils installés" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Installer" },
                            { "description", "Installer de nouveaux composants" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Désinstaller" },
                            { "description", "Supprimer des composants" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Services" },
                            { "description", "Gestion des services" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Paramètres" },
                            { "description", "Paramètres système" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Sites" },
                            { "description", "Gérer les sites Nginx" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Utilitaires" },
                            { "description", "Outils et console" }
                        }
                        }
                    }
                    },
                    { "refresh_tooltip", "Actualiser toutes les données" }
                }
                },
                { "dashboard_tab", new Dictionary<string, object>
                {
                    { "title", "📊 Tableau de bord" },
                    { "cards", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Composants" },
                            { "subtitle", "Cliquez pour accéder" },
                            { "loading", "Chargement..." },
                            { "installed_count", "{0}/{1} installés" },
                            { "none", "Aucun composant" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Installer" },
                            { "subtitle", "Cliquez pour accéder" },
                            { "description", "Ajouter des composants" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Services" },
                            { "subtitle", "Cliquez pour accéder" },
                            { "loading", "Chargement..." },
                            { "active_count", "{0}/{1} actifs" },
                            { "none", "Aucun service actif" }
                        }
                        }
                    }
                    },
                    { "panels", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Composants installés" },
                            { "refresh_tooltip", "Actualiser les composants installés" },
                            { "install_button", "📥 Installer" },
                            { "uninstall_button", "🗑️ Désinstaller" },
                            { "none", "Aucun composant installé" },
                            { "installed_default", "Installé" },
                            { "error_loading", "Erreur lors du chargement des composants" },
                            { "version_na", "N/A" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Services" },
                            { "refresh_tooltip", "Actualiser les services" },
                            { "start_all", "▶️ Démarrer" },
                            { "stop_all", "⏹️ Arrêter" },
                            { "restart_all", "🔄 Redémarrer" },
                            { "none", "Aucun service trouvé" },
                            { "loading", "Chargement des services..." },
                            { "status", new Dictionary<string, object>
                            {
                                { "active", "Actif" },
                                { "stopped", "Arrêté" },
                                { "na", "N/A" }
                            }
                            }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "📥 Installation Rapide" },
                            { "select_component", "Sélectionnez un composant à installer." },
                            { "installing", "Installation de {0}..." },
                            { "success", "{0} installé avec succès !" },
                            { "error", "Erreur lors de l'installation de {0} : {1}" },
                            { "install_button", "📥 Installer" },
                            { "go_to_install", "Aller à l'Installation" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "updating_components", "Mise à jour des composants..." },
                        { "components_updated", "Composants mis à jour !" },
                        { "error_updating_components", "Erreur lors de la mise à jour des composants : {0}" },
                        { "updating_services", "Mise à jour des services..." },
                        { "services_updated", "Services mis à jour !" },
                        { "error_updating_services", "Erreur lors de la mise à jour des services : {0}" },
                        { "starting_all_services", "Démarrage de tous les services..." },
                        { "all_services_started", "Tous les services ont été démarrés !" },
                        { "error_starting_services", "Erreur lors du démarrage des services : {0}" },
                        { "stopping_all_services", "Arrêt de tous les services..." },
                        { "all_services_stopped", "Tous les services ont été arrêtés !" },
                        { "error_stopping_services", "Erreur lors de l'arrêt des services : {0}" },
                        { "restarting_all_services", "Redémarrage de tous les services..." },
                        { "all_services_restarted", "Tous les services ont été redémarrés !" },
                        { "error_restarting_services", "Erreur lors du redémarrage des services : {0}" },
                        { "select_component_install", "Sélectionnez un composant à installer." },
                        { "installing_component", "Installation de {0}..." },
                        { "component_installed", "{0} installé avec succès !" },
                        { "error_installing_component", "Erreur lors de l'installation de {0} : {1}" },
                        { "opening_shell", "🚀 Ouverture du shell interactif pour {0} v{1}" },
                        { "executing_component", "🚀 Exécution de {0} v{1}" },
                        { "no_executable_found", "❌ Aucun exécutable trouvé pour {0} v{1}" },
                        { "version_folder_not_found", "❌ Dossier de version introuvable : {0}" },
                        { "component_not_executable", "❌ Le composant {0} n'est pas exécutable" },
                        { "error_executing", "❌ Erreur lors de l'exécution de {0} v{1} : {2}" },
                        { "error_updating_component_data", "Erreur lors de la mise à jour des données des composants : {0}" },
                        { "error_updating_service_data", "Erreur lors de la mise à jour des données des services : {0}" }
                    }
                    }
                }
                },
                { "installed_tab", new Dictionary<string, object>
                {
                    { "title", "Outils installés" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "tool", "Outil" },
                        { "versions", "Versions installées" },
                        { "status", "Statut" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Actualiser la liste" }
                    }
                    },
                    { "info", "Utilisez les onglets 'Installer' et 'Désinstaller' pour gérer les outils" },
                    { "loading", "Chargement des composants installés..." },
                    { "loaded", "{0} composants chargés" },
                    { "error", "Erreur lors du chargement des composants : {0}" }
                }
                },
                { "install_tab", new Dictionary<string, object>
                {
                    { "title", "Installer un nouvel outil" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Sélectionnez l'outil :" },
                        { "select_version", "Sélectionnez la version (laisser vide pour la plus récente) :" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "install", "📥 Installer" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Sélectionnez un composant à installer." },
                        { "installing", "Installation de {0}..." },
                        { "success", "{0} installé avec succès !" },
                        { "error", "Erreur lors de l'installation de {0}" },
                        { "loading_versions", "Chargement des versions de {0}..." },
                        { "versions_loaded", "{0} versions chargées pour {1}" },
                        { "versions_error", "Erreur lors du chargement des versions : {0}" }
                    }
                    }
                }
                },
                { "uninstall_tab", new Dictionary<string, object>
                {
                    { "title", "Désinstaller un outil" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Sélectionnez l'outil :" },
                        { "select_version", "Sélectionnez la version :" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "uninstall", "🗑️ Désinstaller" },
                        { "refresh", "🔄 Actualiser la liste" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Sélectionnez un composant à désinstaller." },
                        { "select_version", "Sélectionnez une version à désinstaller." },
                        { "confirm", "Êtes-vous sûr de vouloir désinstaller {0} ?" },
                        { "uninstalling", "Désinstallation de {0}..." },
                        { "success", "{0} désinstallé avec succès !" },
                        { "error", "Erreur lors de la désinstallation de {0}" },
                        { "no_versions", "{0} n'a pas de versions installées." },
                        { "not_installed", "{0} n'est pas installé" },
                        { "loading_components", "Chargement des composants installés..." },
                        { "loading_versions", "Chargement des versions installées de {0}..." },
                        { "versions_loaded", "Versions chargées pour {0}" },
                        { "versions_error", "Erreur lors du chargement des versions pour la désinstallation : {0}" },
                        { "components_available", "{0} composants disponibles pour la désinstallation" },
                        { "reloading", "Rechargement de la liste des composants installés..." }
                    }
                    },
                    { "warning", "Attention : Cette action est irréversible !" },
                    { "status", new Dictionary<string, object>
                    {
                        { "uninstalling", "Désinstallation de {0}..." },
                        { "success", "{0} désinstallé avec succès !" },
                        { "error", "❌ Erreur lors de la désinstallation de {0} : {1}" },
                        { "error_short", "Erreur lors de la désinstallation de {0}" },
                        { "loading_versions", "Chargement des versions installées de {0}..." },
                        { "versions_loaded", "Versions chargées pour {0}" },
                        { "not_installed", "{0} n'est pas installé" },
                        { "error_loading_versions", "Erreur lors du chargement des versions pour la désinstallation : {0}" },
                        { "loading_components", "Chargement des composants installés..." },
                        { "components_count", "{0} composants disponibles pour la désinstallation" },
                        { "reloading", "Rechargement de la liste des composants installés..." },
                        { "error_loading_components", "Erreur lors du chargement des composants : {0}" }
                    }
                    }
                }
                },
                { "services_tab", new Dictionary<string, object>
                {
                    { "title", "Gestion des services" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "component", "Composant" },
                        { "version", "Version" },
                        { "status", "Statut" },
                        { "pid", "PID" },
                        { "copy_pid", "Copier PID" },
                        { "actions", "Actions" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Actualiser" },
                        { "start_all", "▶️ Démarrer tout" },
                        { "stop_all", "⏹️ Arrêter tout" },
                        { "restart_all", "🔄 Redémarrer tout" },
                        { "start", "▶️" },
                        { "stop", "⏹️" },
                        { "restart", "🔄" },
                        { "copy_pid", "📋" }
                    }
                    },
                    { "tooltips", new Dictionary<string, object>
                    {
                        { "start", "Démarrer" },
                        { "stop", "Arrêter" },
                        { "restart", "Redémarrer" },
                        { "copy_pid", "Copier PID" }
                    }
                    },
                    { "status", new Dictionary<string, object>
                    {
                        { "running", "En cours d'exécution" },
                        { "stopped", "Arrêté" }
                    }
                    },
                    { "types", new Dictionary<string, object>
                    {
                        { "php_fpm", "PHP-FPM" },
                        { "web_server", "Serveur Web" },
                        { "database", "Base de Données" },
                        { "search_engine", "Moteur de Recherche" },
                        { "service", "Service" },
                        { "fastcgi", "FastCGI" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "loading", "Chargement des services..." },
                        { "loaded", "{0} services chargés" },
                        { "error", "Erreur lors du chargement des services : {0}" },
                        { "starting", "Démarrage de {0} version {1}..." },
                        { "started", "{0} démarré avec succès" },
                        { "stopping", "Arrêt de {0} version {1}..." },
                        { "stopped", "{0} arrêté avec succès" },
                        { "restarting", "Redémarrage de {0} version {1}..." },
                        { "restarted", "{0} redémarré avec succès" },
                        { "starting_all", "Démarrage de tous les services..." },
                        { "started_all", "Tous les services démarrés" },
                        { "stopping_all", "Arrêt de tous les services..." },
                        { "stopped_all", "Tous les services arrêtés" },
                        { "restarting_all", "Redémarrage de tous les services..." },
                        { "restarted_all", "Tous les services redémarrés" },
                        { "pid_copied", "PID {0} copié dans le presse-papiers" },
                        { "no_pid", "Le service n'est pas en cours d'exécution, aucun PID à copier." },
                        { "error_copy_pid", "Erreur lors de la copie du PID : {0}" },
                        { "error_start", "Erreur lors du démarrage du service : {0}" },
                        { "error_stop", "Erreur lors de l'arrêt du service : {0}" },
                        { "error_restart", "Erreur lors du redémarrage du service : {0}" },
                        { "error_start_all", "Erreur lors du démarrage de tous les services : {0}" },
                        { "error_stop_all", "Erreur lors de l'arrêt de tous les services : {0}" },
                        { "error_restart_all", "Erreur lors du redémarrage de tous les services : {0}" }
                    }
                    },
                    { "path_manager", new Dictionary<string, object>
                    {
                        { "not_initialized", "⚠️ PathManager n'a pas été initialisé - PATH non mis à jour" }
                    }
                    },
                    { "debug", new Dictionary<string, object>
                    {
                        { "processes_found", "Processus trouvés pour le debug : {0}" },
                        { "process_info", "  - {0} (PID : {1}) - Chemin : {2}" },
                        { "process_error", "  - {0} (PID : {1}) - Chemin : Erreur d'accès ({2})" },
                        { "found_service_components", "{0} composants de service trouvés" },
                        { "component_dir_not_found", "Répertoire du composant {0} non trouvé : {1}" },
                        { "component_versions_found", "Composant {0} : {1} versions trouvées : {2}" },
                        { "checking_component_version", "Vérification de {0} version {1}" },
                        { "service_process_found", "  - Processus {0} trouvé : {1} (PID : {2}) - Chemin : {3}" },
                        { "service_running", "{0} {1} fonctionne avec les PID : {2}" },
                        { "service_not_running", "{0} {1} n'est pas en cours d'exécution" },
                        { "no_service_pattern", "Aucun modèle de service défini pour {0}" },
                        { "component_check_error", "Erreur lors de la vérification des processus {0} : {1}" },
                        { "php_dirs_found", "{0} dossiers PHP trouvés : {1}" },
                        { "checking_php_version", "Vérification de PHP version {0} dans le dossier {1}" },
                        { "php_process_found", "  - Processus PHP trouvé : {0} (PID : {1}) - Chemin : {2}" },
                        { "process_check_error", "  - Erreur lors de la vérification du processus {0} : {1}" },
                        { "php_running", "PHP {0} fonctionne avec les PID : {1}" },
                        { "php_not_running", "PHP {0} n'est pas en cours d'exécution" },
                        { "php_check_error", "Erreur lors de la vérification des processus PHP : {0}" },
                        { "nginx_dirs_found", "{0} dossiers Nginx trouvés : {1}" },
                        { "checking_nginx_version", "Vérification de Nginx version {0} dans le dossier {1}" },
                        { "nginx_process_found", "  - Processus Nginx trouvé : {0} (PID : {1}) - Chemin : {2}" },
                        { "nginx_running", "Nginx {0} fonctionne avec PID : {1}" },
                        { "nginx_not_running", "Nginx {0} n'est pas en cours d'exécution" },
                        { "nginx_check_error", "Erreur lors de la vérification des processus Nginx : {0}" },
                        { "load_services_error", "Erreur lors du chargement des services dans la GUI : {0}" },
                        { "start_all_services_error", "Erreur lors du démarrage de tous les services dans la GUI : {0}" },
                        { "stop_all_services_error", "Erreur lors de l'arrêt de tous les services dans la GUI : {0}" },
                        { "restart_all_services_error", "Erreur lors du redémarrage de tous les services dans la GUI : {0}" }
                    }
                    }
                }
                },
                { "sidebar", new Dictionary<string, object>
                {
                    { "title", "DevStack Manager" },
                    { "navigation_items", new Dictionary<string, object>
                    {
                        { "installed", new Dictionary<string, object>
                        {
                            { "title", "Installés" },
                            { "description", "Outils installés" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Installer" },
                            { "description", "Installer de nouveaux composants" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Désinstaller" },
                            { "description", "Supprimer des composants" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Services" },
                            { "description", "Gestion des services" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Paramètres" },
                            { "description", "Paramètres système" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Sites" },
                            { "description", "Gérer les sites Nginx" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Utilitaires" },
                            { "description", "Outils et console" }
                        }
                        }
                    }
                    }
                }
                },
                { "config_tab", new Dictionary<string, object>
                {
                    { "title", "Paramètres" },
                    { "path", new Dictionary<string, object>
                    {
                        { "title", "Gestion du PATH" },
                        { "description", "Ajouter des outils au PATH système" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "add", "➕ Ajouter au PATH" },
                            { "remove", "➖ Retirer du PATH" }
                        }
                        }
                    }
                    },
                    { "directories", new Dictionary<string, object>
                    {
                        { "title", "Dossiers" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "devstack_manager", "📂 DevStack Manager" },
                            { "tools", "📂 Outils" }
                        }
                        }
                    }
                    },
                    { "languages", new Dictionary<string, object>
                    {
                        { "title", "Langues" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_language", "Langue de l'interface" }
                        }
                        },
                        { "messages", new Dictionary<string, object>
                        {
                            { "language_changed", "Langue changée en {0}" }
                        }
                        }
                    }
                    },
                    { "themes", new Dictionary<string, object>
                    {
                        { "title", "Thèmes" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_theme", "Thème de l'interface" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "path_updated", "PATH mis à jour avec succès" },
                        { "path_update_error", "Erreur lors de la mise à jour du PATH" },
                        { "path_cleaned", "PATH nettoyé avec succès" },
                        { "path_listed", "PATH listé" },
                        { "path_error", "Erreur lors de l'ajout au PATH : {0}" },
                        { "path_remove_error", "Erreur lors du retrait du PATH : {0}" },
                        { "path_clean_error", "Erreur lors du nettoyage du PATH" },
                        { "path_list_error", "Erreur lors de la liste du PATH : {0}" },
                        { "exe_folder_opened", "Dossier de l'exécutable ouvert" },
                        { "exe_folder_not_found", "Impossible de localiser le dossier de l'exécutable." },
                        { "exe_folder_error", "Erreur lors de l'ouverture du dossier de l'exécutable : {0}" },
                        { "tools_folder_opened", "Dossier des outils ouvert" },
                        { "tools_folder_not_found", "Impossible de localiser le dossier des outils." },
                        { "tools_folder_error", "Erreur lors de l'ouverture du dossier des outils : {0}" }
                    }
                    }
                }
                },
                { "sites_tab", new Dictionary<string, object>
                {
                    { "title", "Créer une configuration de site Nginx" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "domain", "Domaine du site :" },
                        { "root_directory", "Dossier racine :" },
                        { "php_upstream", "PHP Upstream :" },
                        { "nginx_version", "Version Nginx :" },
                        { "ssl_domain", "Domaine pour SSL :" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "browse", "📁 Parcourir" },
                        { "create_site", "🌐 Créer la configuration du site" },
                        { "generate_ssl", "🔒 Générer le certificat SSL" }
                    }
                    },
                    { "ssl", new Dictionary<string, object>
                    {
                        { "title", "Certificats SSL" },
                        { "generate_ssl", "Générer SSL" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_folder", "Sélectionner le dossier du site" },
                        { "creating_site", "Création de la configuration pour le site {0}..." },
                        { "site_created", "Site {0} créé" },
                        { "site_error", "Erreur lors de la création du site {0} : {1}" },
                        { "site_config_error", "Erreur lors de la création de la configuration du site : {0}" },
                        { "enter_domain", "Entrez un domaine pour le site." },
                        { "enter_root", "Entrez un dossier racine pour le site." },
                        { "select_php", "Sélectionnez une version de PHP pour le site." },
                        { "select_nginx", "Sélectionnez une version de Nginx pour le site." },
                        { "enter_ssl_domain", "Entrez un domaine pour générer le certificat SSL." },
                        { "domain_not_exists", "Le domaine '{0}' n'existe pas ou ne résout aucun IP." },
                        { "generating_ssl", "Génération du certificat SSL pour {0}..." },
                        { "ssl_generated", "Processus de génération SSL pour {0} terminé." },
                        { "ssl_error", "Erreur lors de la génération du certificat SSL : {0}" },
                        { "restarting_nginx", "Redémarrage des services Nginx..." },
                        { "nginx_restarted", "Nginx v{0} redémarré avec succès" },
                        { "nginx_restart_error", "Erreur lors du redémarrage de Nginx v{0} : {1}" },
                        { "nginx_restart_general_error", "Erreur lors du redémarrage de Nginx : {0}" },
                        { "ssl_generation_completed", "Processus de génération SSL pour {0} terminé." },
                        { "ssl_generation_error", "❌ Erreur lors de la génération du certificat SSL : {0}" },
                        { "ssl_generation_error_status", "Erreur lors de la génération du SSL pour {0}" },
                        { "ssl_generation_error_dialog", "Erreur lors de la génération du certificat SSL : {0}" },
                        { "no_nginx_restarted", "ℹ️ Aucune version de Nginx n'a été redémarrée (peut-être non en cours d'exécution)" },
                        { "no_nginx_found", "❌ Aucune version de Nginx installée trouvée" }
                    }
                    },
                    { "info", "Les fichiers de configuration seront créés automatiquement" }
                }
                },
                { "utilities_tab", new Dictionary<string, object>
                {
                    { "title", "Console DevStack - Exécutez des commandes directement" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "command", "Commande :" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "execute", "▶️ Exécuter" },
                        { "clear", "❌" },
                        { "clear_tooltip", "Nettoyer la console" }
                    }
                    },
                    { "console_title", "Console DevStack - Exécutez des commandes directement" },
                    { "command_label", "Commande :" },
                    { "execute_button", "▶️ Exécuter" },
                    { "clear_console_tooltip", "Nettoyer la console" },
                    { "status_button", "Statut" },
                    { "installed_button", "Installés" },
                    { "diagnostic_button", "Diagnostic" },
                    { "test_button", "Tester" },
                    { "help_button", "Aide" },
                    { "console_header", "Console DevStack Manager" },
                    { "available_commands", "Commandes disponibles :" },
                    { "tip_message", "Astuce : Entrez des commandes directement dans le champ ci-dessus ou utilisez les boutons rapides" },
                    { "executing_command", "Exécution : {0}" },
                    { "no_output", "(Commande exécutée, aucune sortie générée)" },
                    { "devstack_not_found", "Erreur : Impossible de démarrer le processus DevStack.exe" },
                    { "error", "ERREUR" },
                    { "console_cleared", "Console nettoyée.\n\n" },
                    { "empty_command", "Commande vide" },
                    { "command_execution_error", "Erreur lors de l'exécution de la commande : {0}" },
                    { "status", new Dictionary<string, object>
                    {
                        { "executing", "Exécution : {0}" },
                        { "executed", "Commande exécutée" },
                        { "error", "Erreur lors de l'exécution de la commande" },
                        { "cleared", "Console nettoyée" }
                    }
                    }
                }
                },
                { "console", new Dictionary<string, object>
                {
                    { "titles", new Dictionary<string, object>
                    {
                        { "install", "Sortie console - Installer" },
                        { "uninstall", "Sortie console - Désinstaller" },
                        { "sites", "Sortie console - Sites" },
                        { "config", "Sortie console - Paramètres" },
                        { "utilities", "Sortie console" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "clear", "🗑️ Nettoyer la console" }
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
                        { "warning", "Attention" },
                        { "error", "Erreur" },
                        { "info", "Information" },
                        { "confirmation", "Confirmation" }
                    }
                    }
                }
                },
                { "status_bar", new Dictionary<string, object>
                {
                    { "refresh_tooltip", "Actualiser le statut" },
                    { "updating", "Mise à jour..." },
                    { "updated", "Statut mis à jour" }
                }
                }
            };
        }

        public Dictionary<string, object> GetInstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Assistant d'installation" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "cancel_title", "Annuler l'installation" },
                    { "cancel_message", "Êtes-vous sûr de vouloir annuler l'installation ?" },
                    { "installation_error_title", "Erreur" },
                    { "installation_error_message", "L'installation a échoué : {0}" },
                    { "folder_dialog_title", "Sélectionner le dossier d'installation" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "Bienvenue dans DevStack Manager" },
                    { "description", "Cet assistant va vous guider dans l'installation de DevStack Manager sur votre ordinateur." },
                    { "app_name", "DevStack Manager" },
                    { "version", "Version {0}" },
                    { "app_description", "DevStack Manager est un outil complet de gestion d'environnement de développement qui vous aide à installer, configurer et gérer divers outils et services de développement.\n\nCliquez sur 'Suivant' pour continuer l'installation." },
                    { "language_label", "Langue de l'installation :" }
                }
                },
                { "license", new Dictionary<string, object>
                {
                    { "title", "Contrat de Licence" },
                    { "description", "Veuillez lire attentivement le contrat de licence suivant." },
                    { "label", "Veuillez lire et accepter le contrat de licence :" },
                    { "text", "Licence MIT\n\nCopyright (c) 2025 DevStackManager\n\nLa permission est accordée, gratuitement, à toute personne obtenant une copie\nde ce logiciel et des fichiers de documentation associés (le \"Logiciel\"), d'utiliser\nle Logiciel sans restriction, y compris sans limitation les droits\nd'utiliser, copier, modifier, fusionner, publier, distribuer, sous-licencier et/ou vendre\ndes copies du Logiciel, et de permettre aux personnes à qui le Logiciel est\nfourni de le faire, sous réserve des conditions suivantes :\n\nL'avis de copyright ci-dessus et cet avis de permission doivent être inclus dans toutes\nles copies ou parties substantielles du Logiciel.\n\nLE LOGICIEL EST FOURNI \"EN L'ÉTAT\", SANS GARANTIE D'AUCUNE SORTE, EXPRESSE OU\nIMPLICITE, Y COMPRIS MAIS SANS S'Y LIMITER LES GARANTIES DE QUALITÉ MARCHANDE,\nD'ADAPTATION À UN USAGE PARTICULIER ET DE NON-VIOLATION. EN AUCUN CAS LES\nAUTEURS OU DÉTENTEURS DES DROITS D'AUTEUR NE SERONT RESPONSABLES DE TOUTE RÉCLAMATION, DOMMAGE OU AUTRE\nRESPONSABILITÉ, QUE CE SOIT DANS UNE ACTION CONTRACTUELLE, DÉLICTUELLE OU AUTRE, DÉCOULANT DE,\nHORS DE OU EN RELATION AVEC LE LOGICIEL OU L'UTILISATION OU D'AUTRES TRAITEMENTS DANS\nLE LOGICIEL." }
                }
                },
                { "installation_path", new Dictionary<string, object>
                {
                    { "title", "Choisir le dossier d'installation" },
                    { "description", "Choisissez le dossier où DevStack Manager sera installé." },
                    { "label", "Dossier de destination :" },
                    { "browser", "Parcourir..." },
                    { "space_required", "Espace requis : {0} Mo" },
                    { "space_available", "Espace disponible : {0}" },
                    { "info", "DevStack Manager sera installé dans ce dossier avec tous ses composants et configurations." }
                }
                },
                { "components", new Dictionary<string, object>
                {
                    { "title", "Sélectionner des options supplémentaires" },
                    { "description", "Choisissez les options supplémentaires pour votre installation de DevStack Manager." },
                    { "label", "Options supplémentaires :" },
                    { "desktop_shortcuts", "🖥️ Créer des raccourcis sur le bureau" },
                    { "start_menu_shortcuts", "📂 Créer des raccourcis dans le menu Démarrer" },
                    { "add_to_path", "⚡ Ajouter DevStack au PATH système (recommandé)" },
                    { "path_info", "Ajouter au PATH permet d'utiliser les commandes DevStack directement dans le terminal depuis n'importe quel emplacement." }
                }
                },
                { "ready_to_install", new Dictionary<string, object>
                {
                    { "title", "Prêt à installer" },
                    { "description", "L'assistant est prêt à commencer l'installation. Vérifiez vos paramètres ci-dessous." },
                    { "summary_label", "Résumé de l'installation :" },
                    { "destination", "Dossier de destination :" },
                    { "components_header", "Composants à installer :" },
                    { "cli_component", "• DevStack CLI (Interface en ligne de commande)" },
                    { "gui_component", "• DevStack GUI (Interface graphique)" },
                    { "uninstaller_component", "• Désinstalleur DevStack" },
                    { "config_component", "• Fichiers de configuration et composants" },
                    { "options_header", "Options supplémentaires :" },
                    { "create_desktop", "• Créer des raccourcis sur le bureau" },
                    { "create_start_menu", "• Créer des raccourcis dans le menu Démarrer" },
                    { "add_path", "• Ajouter au PATH système" },
                    { "space_required_summary", "Espace requis : {0} Mo" }
                }
                },
                { "installing", new Dictionary<string, object>
                {
                    { "title", "Installation de DevStack Manager" },
                    { "description", "Veuillez patienter pendant l'installation de DevStack Manager..." },
                    { "preparing", "Préparation de l'installation..." },
                    { "extracting", "Extraction des fichiers d'installation embarqués..." },
                    { "creating_directory", "Création du dossier d'installation..." },
                    { "installing_files", "Installation des fichiers DevStack..." },
                    { "registering", "Enregistrement de l'installation..." },
                    { "creating_desktop", "Création des raccourcis sur le bureau..." },
                    { "creating_start_menu", "Création des raccourcis dans le menu Démarrer..." },
                    { "adding_path", "Ajout au PATH système..." },
                    { "completed", "Installation terminée avec succès !" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Installation terminée" },
                    { "description", "DevStack Manager a été installé avec succès sur votre ordinateur." },
                    { "success_icon", "✅" },
                    { "success_title", "Installation réussie !" },
                    { "success_message", "DevStack Manager a été installé avec succès. Vous pouvez maintenant utiliser l'application pour gérer votre environnement de développement." },
                    { "install_location", "Emplacement d'installation :" },
                    { "launch_now", "🚀 Lancer DevStack Manager maintenant" }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Démarrage du processus d'installation" },
                    { "extracted", "Fichiers embarqués extraits avec succès" },
                    { "creating_dir", "Création du dossier : {0}" },
                    { "installing", "Installation des fichiers de l'application" },
                    { "registering", "Enregistrement de l'installation sous Windows" },
                    { "desktop_shortcuts", "Création des raccourcis sur le bureau" },
                    { "start_menu_shortcuts", "Création des raccourcis dans le menu Démarrer" },
                    { "adding_path", "Ajout de DevStack au PATH système" },
                    { "path_added", "Ajouté au PATH utilisateur avec succès" },
                    { "path_exists", "Déjà présent dans le PATH" },
                    { "completed_success", "Installation terminée avec succès !" },
                    { "cleanup", "Fichiers temporaires nettoyés" },
                    { "cleanup_warning", "Attention : Impossible de supprimer le fichier temporaire : {0}" },
                    { "shortcuts_warning", "Attention : Impossible de créer les raccourcis sur le bureau : {0}" },
                    { "start_menu_warning", "Attention : Impossible de créer les raccourcis dans le menu Démarrer : {0}" },
                    { "path_warning", "Attention : Impossible d'ajouter au PATH : {0}" }
                }
                }
            };
        }

        public Dictionary<string, object> GetUninstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Désinstallateur" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "uninstall_error_title", "Erreur de désinstallation" },
                    { "uninstall_error_message", "Erreur lors de la désinstallation : {0}" },
                    { "startup_error_title", "Erreur du désinstallateur DevStack" },
                    { "startup_error_message", "Erreur lors du démarrage du désinstallateur : {0}\n\nDétails : {1}" },
                    { "initialization_error_title", "Erreur d'initialisation" },
                    { "initialization_error_message", "Erreur lors de l'initialisation de la fenêtre du désinstallateur : {0}" },
                    { "cancel_title", "Annuler la désinstallation" },
                    { "cancel_message", "Êtes-vous sûr de vouloir annuler la désinstallation ?" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "Désinstalleur DevStack" },
                    { "description", "Cet assistant va supprimer DevStack de votre ordinateur" },
                    { "app_name", "Désinstalleur DevStack" },
                    { "version", "Version {0}" },
                    { "app_description", "Cet assistant va vous guider dans le processus de suppression complète de DevStack de votre système." },
                    { "language_label", "Langue :" }
                }
                },
                { "confirmation", new Dictionary<string, object>
                {
                    { "title", "Confirmation de désinstallation" },
                    { "description", "Veuillez confirmer que vous souhaitez procéder à la suppression de DevStack" },
                    { "warning_title", "⚠️ Attention - Cette action est irréversible" },
                    { "warning_text", "La désinstallation supprimera complètement DevStack de votre système, y compris :" },
                    { "items", new Dictionary<string, object>
                    {
                        { "program_files", "• Tous les fichiers du programme" },
                        { "user_data", "• Paramètres et données utilisateur" },
                        { "shortcuts", "• Raccourcis du bureau et du menu Démarrer" },
                        { "registry", "• Entrées du registre Windows" },
                        { "services", "• Services et processus associés" },
                        { "path_variables", "• Variables d'environnement PATH" }
                    }
                    },
                    { "install_found", "📁 Dossier d'installation trouvé :" },
                    { "install_not_found", "❌ Dossier d'installation introuvable automatiquement" },
                    { "install_not_found_desc", "DevStack peut ne pas être installé correctement ou déjà supprimé. La désinstallation ne nettoiera que les registres et raccourcis restants." },
                    { "space_to_free", "📊 Espace à libérer : {0}" }
                }
                },
                { "uninstall_options", new Dictionary<string, object>
                {
                    { "title", "Options de désinstallation" },
                    { "description", "Choisissez ce que vous souhaitez supprimer lors de la désinstallation" },
                    { "label", "Sélectionnez les composants à supprimer :" },
                    { "user_data", "🗂️ Supprimer les données et paramètres utilisateur" },
                    { "user_data_desc", "Inclut les paramètres, journaux et fichiers de données enregistrés par DevStack" },
                    { "registry", "🔧 Supprimer les entrées du registre" },
                    { "registry_desc", "Supprime les clés de registre et informations d'installation" },
                    { "shortcuts", "🔗 Supprimer les raccourcis" },
                    { "shortcuts_desc", "Supprime les raccourcis du bureau et du menu Démarrer" },
                    { "path", "🛤️ Supprimer du PATH système" },
                    { "path_desc", "Supprime le chemin DevStack des variables d'environnement" },
                    { "info", "Nous recommandons de garder toutes les options sélectionnées pour une suppression complète du système." }
                }
                },
                { "ready_to_uninstall", new Dictionary<string, object>
                {
                    { "title", "Prêt à désinstaller" },
                    { "description", "Vérifiez les paramètres et cliquez sur Désinstaller pour continuer" },
                    { "summary_label", "Résumé de la désinstallation :" },
                    { "components_header", "COMPOSANTS À SUPPRIMER :" },
                    { "installation_location", "📁 Emplacement d'installation :" },
                    { "not_found", "Non trouvé" },
                    { "program_components", "🗂️ Composants du programme :" },
                    { "executables", "  • Fichiers exécutables (DevStack.exe, DevStackGUI.exe)" },
                    { "libraries", "  • Bibliothèques et dépendances" },
                    { "config_files", "  • Fichiers de configuration" },
                    { "documentation", "  • Documentation et ressources" },
                    { "selected_options", "OPTIONS SÉLECTIONNÉES :" },
                    { "user_data_selected", "✓ Les données utilisateur seront supprimées" },
                    { "user_data_preserved", "✗ Les données utilisateur seront conservées" },
                    { "registry_selected", "✓ Les entrées du registre seront supprimées" },
                    { "registry_preserved", "✗ Les entrées du registre seront conservées" },
                    { "shortcuts_selected", "✓ Les raccourcis seront supprimés" },
                    { "shortcuts_preserved", "✗ Les raccourcis seront conservés" },
                    { "path_selected", "✓ Sera supprimé du PATH système" },
                    { "path_preserved", "✗ Restera dans le PATH système" },
                    { "space_to_free", "💾 Espace à libérer : {0}" }
                }
                },
                { "uninstalling", new Dictionary<string, object>
                {
                    { "title", "Désinstallation" },
                    { "description", "Veuillez patienter pendant la suppression de DevStack de votre système" },
                    { "preparing", "Préparation de la désinstallation..." },
                    { "stopping_services", "Arrêt des services..." },
                    { "removing_shortcuts", "Suppression des raccourcis..." },
                    { "cleaning_registry", "Nettoyage du registre..." },
                    { "removing_path", "Suppression du PATH..." },
                    { "removing_files", "Suppression des fichiers..." },
                    { "removing_user_data", "Suppression des données utilisateur..." },
                    { "finalizing", "Finalisation..." },
                    { "completed", "Désinstallation terminée !" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Désinstallation terminée" },
                    { "description", "DevStack a été supprimé avec succès de votre système" },
                    { "success_icon", "✅" },
                    { "success_title", "Désinstallation réussie !" },
                    { "success_message", "DevStack a été supprimé avec succès de votre système. Tous les composants sélectionnés ont été nettoyés." },
                    { "summary_title", "📊 Résumé de la désinstallation :" },
                    { "files_removed", "• Fichiers supprimés de : {0}" },
                    { "user_data_removed", "• Données utilisateur supprimées" },
                    { "registry_cleaned", "• Entrées du registre nettoyées" },
                    { "shortcuts_removed", "• Raccourcis supprimés" },
                    { "path_removed", "• Supprimé du PATH système" },
                    { "system_clean", "Le système est maintenant débarrassé de DevStack." }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Démarrage du processus de désinstallation" },
                    { "stopping_services", "Arrêt des services DevStack..." },
                    { "process_stopped", "Processus {0} arrêté" },
                    { "process_stop_warning", "Attention : Impossible d'arrêter {0} : {1}" },
                    { "stop_services_error", "Erreur lors de l'arrêt des services : {0}" },
                    { "removing_shortcuts", "Suppression des raccourcis..." },
                    { "shortcut_removed", "Raccourci supprimé : {0}" },
                    { "start_menu_removed", "Dossier du menu Démarrer supprimé : {0}" },
                    { "shortcuts_error", "Erreur lors de la suppression des raccourcis : {0}" },
                    { "cleaning_registry", "Nettoyage des entrées du registre..." },
                    { "user_registry_removed", "Entrées du registre utilisateur supprimées" },
                    { "machine_registry_removed", "Entrées du registre machine supprimées" },
                    { "uninstall_registry_removed", "Entrée programmes et fonctionnalités supprimée" },
                    { "registry_error", "Erreur lors du nettoyage du registre : {0}" },
                    { "removing_path", "Suppression du PATH système..." },
                    { "user_path_removed", "Supprimé du PATH utilisateur" },
                    { "system_path_removed", "Supprimé du PATH système" },
                    { "system_path_warning", "Attention : Impossible de supprimer du PATH système (nécessite les droits administrateur)" },
                    { "path_error", "Erreur lors de la suppression du PATH : {0}" },
                    { "removing_files", "Suppression des fichiers de {0}..." },
                    { "install_not_found", "Dossier d'installation introuvable" },
                    { "files_removed_count", "{0} fichiers supprimés" },
                    { "dirs_removed_count", "{0} dossiers vides supprimés" },
                    { "file_remove_warning", "Attention : Impossible de supprimer {0} : {1}" },
                    { "files_error", "Erreur lors de la suppression des fichiers : {0}" },
                    { "removing_user_data", "Suppression des données utilisateur..." },
                    { "user_data_removed", "Données utilisateur supprimées : {0}" },
                    { "user_data_error", "Erreur lors de la suppression des données utilisateur : {0}" },
                    { "self_deletion_scheduled", "Suppression automatique du désinstalleur programmée" },
                    { "self_deletion_warning", "Attention : Impossible de programmer l'auto-suppression : {0}" },
                    { "uninstall_success", "Désinstallation terminée avec succès !" }
                }
                }
            };
        }

        public Dictionary<string, object> GetAllTranslations()
        {
            var all = new Dictionary<string, object>();
            
            // Adicionar todas as seções
            all["common"] = GetCommonTranslations();
            all["shared"] = GetSharedTranslations();
            all["gui"] = GetGuiTranslations();
            all["installer"] = GetInstallerTranslations();
            all["uninstaller"] = GetUninstallerTranslations();
            
            return all;
        }
    }
}
