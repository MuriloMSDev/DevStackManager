using System.Collections.Generic;

namespace DevStackShared.Localization
{
    public class en_US : ILanguageProvider
    {
        public string LanguageCode => "en_US";
        public string LanguageName => "English (US)";

        public Dictionary<string, object> GetCommonTranslations()
        {
            return new Dictionary<string, object>
            {
                { "language_name", "English (US)" },
                { "unknown", "Unknown" },
                { "themes", new Dictionary<string, object>
                {
                    { "light", "Light" },
                    { "dark", "Dark" },
                    { "messages", new Dictionary<string, object>
                    {
                        { "theme_changed", "Theme changed to {0}" }
                    }
                    }
                }
                },
                { "buttons", new Dictionary<string, object>
                {
                    { "back", "← Back" },
                    { "next", "Next →" },
                    { "accept", "I Accept" },
                    { "install", "Install" },
                    { "finish", "Finish" },
                    { "cancel", "Cancel" },
                    { "continue", "Continue" },
                    { "uninstall", "🗑️ Uninstall" },
                    { "yes", "Yes" },
                    { "no", "No" },
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
                    { "no_component", "No component specified for uninstall." },
                    { "removing_shortcut", "Removing shortcut for {0}..." },
                    { "unknown_component", "Unknown component: {0}" },
                    { "finished", "Uninstall finished." }
                }
                },
                { "shortcuts", new Dictionary<string, object>
                {
                    { "created", "Shortcut {0} created pointing to {1}" },
                    { "error_creating", "Error creating symbolic link: {0}" },
                    { "fallback_copy", "Fallback: Copy {0} created in {1}" },
                    { "file_not_found", "Warning: file {0} not found to create shortcut" },
                    { "removed", "Shortcut {0} removed" },
                    { "not_found", "Shortcut {0} not found for removal" },
                    { "error_removing", "Error removing shortcut: {0}" }
                }
                },
                { "install", new Dictionary<string, object>
                {
                    { "already_installed", "{0} {1} is already installed." },
                    { "downloading", "Downloading {0} {1}..." },
                    { "running_installer", "Running installer {0} {1}..." },
                    { "installed_via_installer", "{0} {1} installed via installer in {2}" },
                    { "extracting", "Extracting..." },
                    { "installed", "{0} {1} installed." },
                    { "installed_in", "{0} {1} installed in {2}." },
                    { "error_installing", "Error installing {0} {1}: {2}" },
                    { "shortcut_creation_failed", "Warning: failed to create shortcut: {0}" },
                    { "component_installed", "{0} {1} installed." }
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
                    { "ready_status", "Ready" },
                    { "initialization_error", "Error initializing DevStack GUI: {0}" },
                    { "error_title", "DevStack Manager - Error" }
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
                            { "description", "System overview" }
                        }
                        },
                        { "installed", new Dictionary<string, object>
                        {
                            { "title", "Installed" },
                            { "description", "Installed tools" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Install" },
                            { "description", "Install new components" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Uninstall" },
                            { "description", "Remove components" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Services" },
                            { "description", "Service control" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Settings" },
                            { "description", "System settings" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Sites" },
                            { "description", "Manage Nginx sites" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Utilities" },
                            { "description", "Tools and console" }
                        }
                        }
                    }
                    },
                    { "refresh_tooltip", "Refresh all data" }
                }
                },
                { "dashboard_tab", new Dictionary<string, object>
                {
                    { "title", "📊 Dashboard" },
                    { "cards", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Components" },
                            { "subtitle", "Click to access" },
                            { "loading", "Loading..." },
                            { "installed_count", "{0}/{1} installed" },
                            { "none", "No components" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Install" },
                            { "subtitle", "Click to access" },
                            { "description", "Add components" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Services" },
                            { "subtitle", "Click to access" },
                            { "loading", "Loading..." },
                            { "active_count", "{0}/{1} active" },
                            { "none", "No active services" }
                        }
                        }
                    }
                    },
                    { "panels", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Installed Components" },
                            { "refresh_tooltip", "Refresh installed components" },
                            { "install_button", "📥 Install" },
                            { "uninstall_button", "🗑️ Uninstall" },
                            { "none", "No components installed" },
                            { "installed_default", "Installed" },
                            { "error_loading", "Error loading components" },
                            { "version_na", "N/A" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Services" },
                            { "refresh_tooltip", "Refresh services" },
                            { "start_all", "▶️ Start" },
                            { "stop_all", "⏹️ Stop" },
                            { "restart_all", "🔄 Restart" },
                            { "none", "No services found" },
                            { "loading", "Loading services..." },
                            { "status", new Dictionary<string, object>
                            {
                                { "active", "Active" },
                                { "stopped", "Stopped" },
                                { "na", "N/A" }
                            }
                            }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "📥 Quick Install" },
                            { "select_component", "Select a component to install." },
                            { "installing", "Installing {0}..." },
                            { "success", "{0} installed successfully!" },
                            { "error", "Error installing {0}: {1}" },
                            { "install_button", "📥 Install" },
                            { "go_to_install", "Go to Install" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "updating_components", "Updating components..." },
                        { "components_updated", "Components updated!" },
                        { "error_updating_components", "Error updating components: {0}" },
                        { "updating_services", "Updating services..." },
                        { "services_updated", "Services updated!" },
                        { "error_updating_services", "Error updating services: {0}" },
                        { "starting_all_services", "Starting all services..." },
                        { "all_services_started", "All services started!" },
                        { "error_starting_services", "Error starting services: {0}" },
                        { "stopping_all_services", "Stopping all services..." },
                        { "all_services_stopped", "All services stopped!" },
                        { "error_stopping_services", "Error stopping services: {0}" },
                        { "restarting_all_services", "Restarting all services..." },
                        { "all_services_restarted", "All services restarted!" },
                        { "error_restarting_services", "Error restarting services: {0}" },
                        { "select_component_install", "Select a component to install." },
                        { "installing_component", "Installing {0}..." },
                        { "component_installed", "{0} installed successfully!" },
                        { "error_installing_component", "Error installing {0}: {1}" },
                        { "opening_shell", "🚀 Opening interactive shell for {0} v{1}" },
                        { "executing_component", "🚀 Executing {0} v{1}" },
                        { "no_executable_found", "❌ No executable found for {0} v{1}" },
                        { "version_folder_not_found", "❌ Version folder not found: {0}" },
                        { "component_not_executable", "❌ Component {0} is not executable" },
                        { "error_executing", "❌ Error executing {0} v{1}: {2}" },
                        { "error_updating_component_data", "Error updating component data: {0}" },
                        { "error_updating_service_data", "Error updating service data: {0}" }
                    }
                    }
                }
                },
                { "installed_tab", new Dictionary<string, object>
                {
                    { "title", "Installed Tools" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "tool", "Tool" },
                        { "versions", "Installed Versions" },
                        { "status", "Status" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Refresh List" }
                    }
                    },
                    { "info", "Use the 'Install' and 'Uninstall' tabs to manage tools" },
                    { "loading", "Loading installed components..." },
                    { "loaded", "{0} components loaded" },
                    { "error", "Error loading components: {0}" }
                }
                },
                { "install_tab", new Dictionary<string, object>
                {
                    { "title", "Install New Tool" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Select tool:" },
                        { "select_version", "Select version (leave blank for latest):" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "install", "📥 Install" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Select a component to install." },
                        { "installing", "Installing {0}..." },
                        { "success", "{0} installed successfully!" },
                        { "error", "Error installing {0}" },
                        { "loading_versions", "Loading versions for {0}..." },
                        { "versions_loaded", "{0} versions loaded for {1}" },
                        { "versions_error", "Error loading versions: {0}" }
                    }
                    }
                }
                },
                { "uninstall_tab", new Dictionary<string, object>
                {
                    { "title", "Uninstall Tool" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Select tool:" },
                        { "select_version", "Select version:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "uninstall", "🗑️ Uninstall" },
                        { "refresh", "🔄 Refresh List" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Select a component to uninstall." },
                        { "select_version", "Select a version to uninstall." },
                        { "confirm", "Are you sure you want to uninstall {0}?" },
                        { "uninstalling", "Uninstalling {0}..." },
                        { "success", "{0} uninstalled successfully!" },
                        { "error", "Error uninstalling {0}" },
                        { "no_versions", "{0} has no installed versions." },
                        { "not_installed", "{0} is not installed" },
                        { "loading_components", "Loading installed components..." },
                        { "loading_versions", "Loading installed versions of {0}..." },
                        { "versions_loaded", "Versions loaded for {0}" },
                        { "versions_error", "Error loading versions for uninstallation: {0}" },
                        { "components_available", "{0} components available for uninstallation" },
                        { "reloading", "Reloading list of installed components..." }
                    }
                    },
                    { "warning", "Attention: This action cannot be undone!" },
                    { "status", new Dictionary<string, object>
                    {
                        { "uninstalling", "Uninstalling {0}..." },
                        { "success", "{0} uninstalled successfully!" },
                        { "error", "❌ Error uninstalling {0}: {1}" },
                        { "error_short", "Error uninstalling {0}" },
                        { "loading_versions", "Loading installed versions of {0}..." },
                        { "versions_loaded", "Versions loaded for {0}" },
                        { "not_installed", "{0} is not installed" },
                        { "error_loading_versions", "Error loading versions for uninstallation: {0}" },
                        { "loading_components", "Loading installed components..." },
                        { "components_count", "{0} components available for uninstallation" },
                        { "reloading", "Reloading list of installed components..." },
                        { "error_loading_components", "Error loading components: {0}" }
                    }
                    }
                }
                },
                { "services_tab", new Dictionary<string, object>
                {
                    { "title", "Service Management" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "component", "Component" },
                        { "version", "Version" },
                        { "status", "Status" },
                        { "pid", "PID" },
                        { "copy_pid", "Copy PID" },
                        { "actions", "Actions" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Refresh" },
                        { "start_all", "▶️ Start All" },
                        { "stop_all", "⏹️ Stop All" },
                        { "restart_all", "🔄 Restart All" },
                        { "start", "▶️" },
                        { "stop", "⏹️" },
                        { "restart", "🔄" },
                        { "copy_pid", "📋" }
                    }
                    },
                    { "tooltips", new Dictionary<string, object>
                    {
                        { "start", "Start" },
                        { "stop", "Stop" },
                        { "restart", "Restart" },
                        { "copy_pid", "Copy PID" }
                    }
                    },
                    { "status", new Dictionary<string, object>
                    {
                        { "running", "Running" },
                        { "stopped", "Stopped" }
                    }
                    },
                    { "types", new Dictionary<string, object>
                    {
                        { "php_fpm", "PHP-FPM" },
                        { "web_server", "Web Server" },
                        { "database", "Database" },
                        { "search_engine", "Search Engine" },
                        { "service", "Service" },
                        { "fastcgi", "FastCGI" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "loading", "Loading services..." },
                        { "loaded", "{0} services loaded" },
                        { "error", "Error loading services: {0}" },
                        { "starting", "Starting {0} version {1}..." },
                        { "started", "{0} started successfully" },
                        { "stopping", "Stopping {0} version {1}..." },
                        { "stopped", "{0} stopped successfully" },
                        { "restarting", "Restarting {0} version {1}..." },
                        { "restarted", "{0} restarted successfully" },
                        { "starting_all", "Starting all services..." },
                        { "started_all", "All services started" },
                        { "stopping_all", "Stopping all services..." },
                        { "stopped_all", "All services stopped" },
                        { "restarting_all", "Restarting all services..." },
                        { "restarted_all", "All services restarted" },
                        { "pid_copied", "PID {0} copied to clipboard" },
                        { "no_pid", "Service is not running, no PID to copy." },
                        { "error_copy_pid", "Error copying PID: {0}" },
                        { "error_start", "Error starting service: {0}" },
                        { "error_stop", "Error stopping service: {0}" },
                        { "error_restart", "Error restarting service: {0}" },
                        { "error_start_all", "Error starting all services: {0}" },
                        { "error_stop_all", "Error stopping all services: {0}" },
                        { "error_restart_all", "Error restarting all services: {0}" }
                    }
                    },
                    { "path_manager", new Dictionary<string, object>
                    {
                        { "not_initialized", "⚠️ PathManager not initialized - PATH not updated" }
                    }
                    },
                    { "debug", new Dictionary<string, object>
                    {
                        { "processes_found", "Processes found for debug: {0}" },
                        { "process_info", "  - {0} (PID: {1}) - Path: {2}" },
                        { "process_error", "  - {0} (PID: {1}) - Path: Error accessing ({2})" },
                        { "found_service_components", "Found {0} service components" },
                        { "component_dir_not_found", "Component {0} directory not found: {1}" },
                        { "component_versions_found", "Component {0}: {1} versions found: {2}" },
                        { "checking_component_version", "Checking {0} version {1}" },
                        { "service_process_found", "  - {0} process found: {1} (PID: {2}) - Path: {3}" },
                        { "service_running", "{0} {1} is running with PIDs: {2}" },
                        { "service_not_running", "{0} {1} is not running" },
                        { "no_service_pattern", "No service pattern defined for {0}" },
                        { "component_check_error", "Error checking {0} processes: {1}" },
                        { "php_dirs_found", "{0} PHP directories found: {1}" },
                        { "checking_php_version", "Checking PHP version {0} in directory {1}" },
                        { "php_process_found", "  - PHP process found: {0} (PID: {1}) - Path: {2}" },
                        { "process_check_error", "  - Error checking process {0}: {1}" },
                        { "php_running", "PHP {0} is running with PIDs: {1}" },
                        { "php_not_running", "PHP {0} is not running" },
                        { "php_check_error", "Error checking PHP processes: {0}" },
                        { "nginx_dirs_found", "{0} Nginx directories found: {1}" },
                        { "checking_nginx_version", "Checking Nginx version {0} in directory {1}" },
                        { "nginx_process_found", "  - Nginx process found: {0} (PID: {1}) - Path: {2}" },
                        { "nginx_running", "Nginx {0} is running with PID: {1}" },
                        { "nginx_not_running", "Nginx {0} is not running" },
                        { "nginx_check_error", "Error checking Nginx processes: {0}" },
                        { "load_services_error", "Error loading services in GUI: {0}" },
                        { "start_all_services_error", "Error starting all services in GUI: {0}" },
                        { "stop_all_services_error", "Error stopping all services in GUI: {0}" },
                        { "restart_all_services_error", "Error restarting all services in GUI: {0}" }
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
                            { "title", "Installed" },
                            { "description", "Installed tools" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Install" },
                            { "description", "Install new components" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Uninstall" },
                            { "description", "Remove components" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Services" },
                            { "description", "Service control" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Settings" },
                            { "description", "System settings" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Sites" },
                            { "description", "Manage Nginx sites" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Utilities" },
                            { "description", "Tools and console" }
                        }
                        }
                    }
                    }
                }
                },
                { "config_tab", new Dictionary<string, object>
                {
                    { "title", "Settings" },
                    { "path", new Dictionary<string, object>
                    {
                        { "title", "PATH Management" },
                        { "description", "Add tools to system PATH" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "add", "➕ Add to PATH" },
                            { "remove", "➖ Remove from PATH" }
                        }
                        }
                    }
                    },
                    { "directories", new Dictionary<string, object>
                    {
                        { "title", "Directories" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "devstack_manager", "📂 DevStack Manager" },
                            { "tools", "📂 Tools" }
                        }
                        }
                    }
                    },
                    { "languages", new Dictionary<string, object>
                    {
                        { "title", "Languages" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_language", "Interface Language" }
                        }
                        },
                        { "messages", new Dictionary<string, object>
                        {
                            { "language_changed", "Language changed to {0}" }
                        }
                        }
                    }
                    },
                    { "themes", new Dictionary<string, object>
                    {
                        { "title", "Themes" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_theme", "Interface Theme" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "path_updated", "PATH updated successfully" },
                        { "path_update_error", "Error updating PATH" },
                        { "path_cleaned", "PATH cleaned successfully" },
                        { "path_listed", "PATH listed" },
                        { "path_error", "Error adding to PATH: {0}" },
                        { "path_remove_error", "Error removing from PATH: {0}" },
                        { "path_clean_error", "Error cleaning PATH" },
                        { "path_list_error", "Error listing PATH: {0}" },
                        { "exe_folder_opened", "Executable folder opened" },
                        { "exe_folder_not_found", "Could not locate executable folder." },
                        { "exe_folder_error", "Error opening executable folder: {0}" },
                        { "tools_folder_opened", "Tools folder opened" },
                        { "tools_folder_not_found", "Could not locate tools folder." },
                        { "tools_folder_error", "Error opening tools folder: {0}" }
                    }
                    }
                }
                },
                { "sites_tab", new Dictionary<string, object>
                {
                    { "title", "Create Nginx Site Configuration" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "domain", "Site domain:" },
                        { "root_directory", "Root directory:" },
                        { "php_upstream", "PHP Upstream:" },
                        { "nginx_version", "Nginx version:" },
                        { "ssl_domain", "Domain for SSL:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "browse", "📁 Browse" },
                        { "create_site", "🌐 Create Site Configuration" },
                        { "generate_ssl", "🔒 Generate SSL Certificate" }
                    }
                    },
                    { "ssl", new Dictionary<string, object>
                    {
                        { "title", "SSL Certificates" },
                        { "generate_ssl", "Generate SSL" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_folder", "Select Site Folder" },
                        { "creating_site", "Creating configuration for site {0}..." },
                        { "site_created", "Site {0} created" },
                        { "site_error", "Error creating site {0}: {1}" },
                        { "site_config_error", "Error creating site configuration: {0}" },
                        { "enter_domain", "Enter a domain for the site." },
                        { "enter_root", "Enter a root directory for the site." },
                        { "select_php", "Select a PHP version for the site." },
                        { "select_nginx", "Select an Nginx version for the site." },
                        { "enter_ssl_domain", "Enter a domain to generate the SSL certificate." },
                        { "domain_not_exists", "The domain '{0}' does not exist or is not resolving to any IP." },
                        { "generating_ssl", "Generating SSL certificate for {0}..." },
                        { "ssl_generated", "SSL generation process for {0} finished." },
                        { "ssl_error", "Error generating SSL certificate: {0}" },
                        { "restarting_nginx", "Restarting Nginx services..." },
                        { "nginx_restarted", "Nginx v{0} restarted successfully" },
                        { "nginx_restart_error", "Error restarting Nginx v{0}: {1}" },
                        { "nginx_restart_general_error", "Error restarting Nginx: {0}" },
                        { "ssl_generation_completed", "SSL generation process for {0} finished." },
                        { "ssl_generation_error", "❌ Error generating SSL certificate: {0}" },
                        { "ssl_generation_error_status", "Error generating SSL for {0}" },
                        { "ssl_generation_error_dialog", "Error generating SSL certificate: {0}" },
                        { "no_nginx_restarted", "ℹ️ No Nginx version was restarted (may not be running)" },
                        { "no_nginx_found", "❌ No installed Nginx version found" }
                    }
                    },
                    { "info", "Configuration files will be created automatically" }
                }
                },
                { "utilities_tab", new Dictionary<string, object>
                {
                    { "title", "DevStack Console - Run commands directly" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "command", "Command:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "execute", "▶️ Execute" },
                        { "clear", "❌" },
                        { "clear_tooltip", "Clear Console" }
                    }
                    },
                    { "console_title", "DevStack Console - Run commands directly" },
                    { "command_label", "Command:" },
                    { "execute_button", "▶️ Execute" },
                    { "clear_console_tooltip", "Clear Console" },
                    { "status_button", "Status" },
                    { "installed_button", "Installed" },
                    { "diagnostic_button", "Diagnostic" },
                    { "test_button", "Test" },
                    { "help_button", "Help" },
                    { "console_header", "DevStack Manager Console" },
                    { "available_commands", "Available commands:" },
                    { "tip_message", "Tip: Type commands directly in the field above or use quick buttons" },
                    { "executing_command", "Executing: {0}" },
                    { "no_output", "(Command executed, no output generated)" },
                    { "devstack_not_found", "Error: Could not start DevStack.exe process" },
                    { "error", "ERROR" },
                    { "console_cleared", "Console cleared.\n\n" },
                    { "empty_command", "Empty command" },
                    { "command_execution_error", "Error executing command: {0}" },
                    { "status", new Dictionary<string, object>
                    {
                        { "executing", "Executing: {0}" },
                        { "executed", "Command executed" },
                        { "error", "Error executing command" },
                        { "cleared", "Console cleared" }
                    }
                    }
                }
                },
                { "console", new Dictionary<string, object>
                {
                    { "titles", new Dictionary<string, object>
                    {
                        { "install", "Console Output - Install" },
                        { "uninstall", "Console Output - Uninstall" },
                        { "sites", "Console Output - Sites" },
                        { "config", "Console Output - Settings" },
                        { "utilities", "Console Output" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "clear", "🗑️ Clear Console" }
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
                        { "warning", "Warning" },
                        { "error", "Error" },
                        { "info", "Information" },
                        { "confirmation", "Confirmation" }
                    }
                    }
                }
                },
                { "status_bar", new Dictionary<string, object>
                {
                    { "refresh_tooltip", "Refresh status" },
                    { "updating", "Updating..." },
                    { "updated", "Status updated" }
                }
                }
            };
        }

        public Dictionary<string, object> GetInstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Setup Wizard" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "cancel_title", "Cancel Installation" },
                    { "cancel_message", "Are you sure you want to cancel the installation?" },
                    { "installation_error_title", "Error" },
                    { "installation_error_message", "Installation failed: {0}" },
                    { "folder_dialog_title", "Select installation folder" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "Welcome to DevStack Manager" },
                    { "description", "This wizard will guide you through installing DevStack Manager on your computer." },
                    { "app_name", "DevStack Manager" },
                    { "version", "Version {0}" },
                    { "app_description", "DevStack Manager is a comprehensive development environment management tool that helps you install, configure, and manage various development tools and services.\n\nClick 'Next' to continue with the installation." },
                    { "language_label", "Installation language:" }
                }
                },
                { "license", new Dictionary<string, object>
                {
                    { "title", "License Agreement" },
                    { "description", "Please read the following license agreement carefully." },
                    { "label", "Please read and accept the license agreement:" },
                    { "text", "MIT License\n\nCopyright (c) 2025 DevStackManager\n\nPermission is hereby granted, free of charge, to any person obtaining a copy\nof this software and associated documentation files (the \"Software\"), to deal\nin the Software without restriction, including without limitation the rights\nto use, copy, modify, merge, publish, distribute, sublicense, and/or sell\ncopies of the Software, and to permit persons to whom the Software is\nfurnished to do so, subject to the following conditions:\n\nThe above copyright notice and this permission notice shall be included in all\ncopies or substantial portions of the Software.\n\nTHE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR\nIMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,\nFITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE\nAUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER\nLIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,\nOUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE\nSOFTWARE." }
                }
                },
                { "installation_path", new Dictionary<string, object>
                {
                    { "title", "Choose Installation Location" },
                    { "description", "Choose the folder where DevStack Manager will be installed." },
                    { "label", "Destination Folder:" },
                    { "browser", "Browse..." },
                    { "space_required", "Space required: {0} MB" },
                    { "space_available", "Space available: {0}" },
                    { "info", "DevStack Manager will be installed in this folder along with all its components and settings." }
                }
                },
                { "components", new Dictionary<string, object>
                {
                    { "title", "Select Additional Options" },
                    { "description", "Choose additional options for your DevStack Manager installation." },
                    { "label", "Additional Options:" },
                    { "desktop_shortcuts", "🖥️ Create desktop shortcuts" },
                    { "start_menu_shortcuts", "📂 Create Start Menu shortcuts" },
                    { "add_to_path", "⚡ Add DevStack to system PATH (recommended)" },
                    { "path_info", "Adding to PATH allows you to use DevStack commands directly from any terminal location." }
                }
                },
                { "ready_to_install", new Dictionary<string, object>
                {
                    { "title", "Ready to Install" },
                    { "description", "The wizard is ready to begin installation. Review your settings below." },
                    { "summary_label", "Installation Summary:" },
                    { "destination", "Destination folder:" },
                    { "components_header", "Components to install:" },
                    { "cli_component", "• DevStack CLI (Command Line Interface)" },
                    { "gui_component", "• DevStack GUI (Graphical Interface)" },
                    { "uninstaller_component", "• DevStack Uninstaller" },
                    { "config_component", "• Configuration files and components" },
                    { "options_header", "Additional options:" },
                    { "create_desktop", "• Create desktop shortcuts" },
                    { "create_start_menu", "• Create Start Menu shortcuts" },
                    { "add_path", "• Add to system PATH" },
                    { "space_required_summary", "Space required: {0} MB" }
                }
                },
                { "installing", new Dictionary<string, object>
                {
                    { "title", "Installing DevStack Manager" },
                    { "description", "Please wait while DevStack Manager is being installed..." },
                    { "preparing", "Preparing installation..." },
                    { "extracting", "Extracting embedded installation files..." },
                    { "creating_directory", "Creating installation directory..." },
                    { "installing_files", "Installing DevStack files..." },
                    { "registering", "Registering installation..." },
                    { "creating_desktop", "Creating desktop shortcuts..." },
                    { "creating_start_menu", "Creating Start Menu shortcuts..." },
                    { "adding_path", "Adding to system PATH..." },
                    { "completed", "Installation completed successfully!" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Installation Completed" },
                    { "description", "DevStack Manager has been successfully installed on your computer." },
                    { "success_icon", "✅" },
                    { "success_title", "Installation Successful!" },
                    { "success_message", "DevStack Manager has been installed successfully. You can now use the application to manage your development environment." },
                    { "install_location", "Installation Location:" },
                    { "launch_now", "🚀 Launch DevStack Manager now" }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Starting installation process" },
                    { "extracted", "Embedded files extracted successfully" },
                    { "creating_dir", "Creating directory: {0}" },
                    { "installing", "Installing application files" },
                    { "registering", "Registering installation in Windows" },
                    { "desktop_shortcuts", "Creating desktop shortcuts" },
                    { "start_menu_shortcuts", "Creating Start Menu shortcuts" },
                    { "adding_path", "Adding DevStack to system PATH" },
                    { "path_added", "Successfully added to user PATH" },
                    { "path_exists", "Already exists in PATH" },
                    { "completed_success", "Installation completed successfully!" },
                    { "cleanup", "Temporary files cleaned" },
                    { "cleanup_warning", "Warning: Could not delete temporary file: {0}" },
                    { "shortcuts_warning", "Warning: Could not create desktop shortcuts: {0}" },
                    { "start_menu_warning", "Warning: Could not create Start Menu shortcuts: {0}" },
                    { "path_warning", "Warning: Could not add to PATH: {0}" }
                }
                }
            };
        }

        public Dictionary<string, object> GetUninstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Uninstaller" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "uninstall_error_title", "Uninstall Error" },
                    { "uninstall_error_message", "Error during uninstallation: {0}" },
                    { "startup_error_title", "DevStack Uninstaller Error" },
                    { "startup_error_message", "Error starting uninstaller: {0}\n\nDetails: {1}" },
                    { "initialization_error_title", "Initialization Error" },
                    { "initialization_error_message", "Error initializing uninstaller window: {0}" },
                    { "cancel_title", "Cancel Uninstallation" },
                    { "cancel_message", "Are you sure you want to cancel the uninstallation?" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "DevStack Uninstaller" },
                    { "description", "This wizard will remove DevStack from your computer" },
                    { "app_name", "DevStack Uninstaller" },
                    { "version", "Version {0}" },
                    { "app_description", "This wizard will guide you through the complete removal process of DevStack from your system." },
                    { "language_label", "Language:" }
                }
                },
                { "confirmation", new Dictionary<string, object>
                {
                    { "title", "Uninstall Confirmation" },
                    { "description", "Please confirm you want to proceed with removing DevStack" },
                    { "warning_title", "⚠️ Attention - This action cannot be undone" },
                    { "warning_text", "Uninstalling will completely remove DevStack from your system, including:" },
                    { "items", new Dictionary<string, object>
                    {
                        { "program_files", "• All program files" },
                        { "user_data", "• User settings and data" },
                        { "shortcuts", "• Desktop and Start Menu shortcuts" },
                        { "registry", "• Windows registry entries" },
                        { "services", "• Related services and processes" },
                        { "path_variables", "• PATH environment variables" }
                    }
                    },
                    { "install_found", "📁 Installation folder found:" },
                    { "install_not_found", "❌ Installation folder not found automatically" },
                    { "install_not_found_desc", "DevStack may not be installed correctly or may have already been removed. Uninstallation will only clean up remaining records and shortcuts." },
                    { "space_to_free", "📊 Space to be freed: {0}" }
                }
                },
                { "uninstall_options", new Dictionary<string, object>
                {
                    { "title", "Uninstall Options" },
                    { "description", "Choose what to remove during uninstallation" },
                    { "label", "Select components to remove:" },
                    { "user_data", "🗂️ Remove user data and settings" },
                    { "user_data_desc", "Includes settings, logs, and data files saved by DevStack" },
                    { "registry", "🔧 Remove registry entries" },
                    { "registry_desc", "Removes registry keys and installation information" },
                    { "shortcuts", "🔗 Remove shortcuts" },
                    { "shortcuts_desc", "Removes desktop and Start Menu shortcuts" },
                    { "path", "🛤️ Remove from system PATH" },
                    { "path_desc", "Removes DevStack path from environment variables" },
                    { "info", "We recommend keeping all options selected for a complete system removal." }
                }
                },
                { "ready_to_uninstall", new Dictionary<string, object>
                {
                    { "title", "Ready to Uninstall" },
                    { "description", "Review the settings and click Uninstall to proceed" },
                    { "summary_label", "Uninstall summary:" },
                    { "components_header", "COMPONENTS TO BE REMOVED:" },
                    { "installation_location", "📁 Installation location:" },
                    { "not_found", "Not found" },
                    { "program_components", "🗂️ Program components:" },
                    { "executables", "  • Executable files (DevStack.exe, DevStackGUI.exe)" },
                    { "libraries", "  • Libraries and dependencies" },
                    { "config_files", "  • Configuration files" },
                    { "documentation", "  • Documentation and resources" },
                    { "selected_options", "SELECTED OPTIONS:" },
                    { "user_data_selected", "✓ User data will be removed" },
                    { "user_data_preserved", "✗ User data will be preserved" },
                    { "registry_selected", "✓ Registry entries will be removed" },
                    { "registry_preserved", "✗ Registry entries will be preserved" },
                    { "shortcuts_selected", "✓ Shortcuts will be removed" },
                    { "shortcuts_preserved", "✗ Shortcuts will be preserved" },
                    { "path_selected", "✓ Will be removed from system PATH" },
                    { "path_preserved", "✗ Will remain in system PATH" },
                    { "space_to_free", "💾 Space to be freed: {0}" }
                }
                },
                { "uninstalling", new Dictionary<string, object>
                {
                    { "title", "Uninstalling" },
                    { "description", "Please wait while DevStack is being removed from your system" },
                    { "preparing", "Preparing uninstallation..." },
                    { "stopping_services", "Stopping services..." },
                    { "removing_shortcuts", "Removing shortcuts..." },
                    { "cleaning_registry", "Cleaning registry..." },
                    { "removing_path", "Removing from PATH..." },
                    { "removing_files", "Removing files..." },
                    { "removing_user_data", "Removing user data..." },
                    { "finalizing", "Finalizing..." },
                    { "completed", "Uninstallation completed!" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Uninstallation Completed" },
                    { "description", "DevStack has been successfully removed from your system" },
                    { "success_icon", "✅" },
                    { "success_title", "Uninstallation Completed!" },
                    { "success_message", "DevStack has been successfully removed from your system. All selected components have been cleaned." },
                    { "summary_title", "📊 Uninstall summary:" },
                    { "files_removed", "• Files removed from: {0}" },
                    { "user_data_removed", "• User data removed" },
                    { "registry_cleaned", "• Registry entries cleaned" },
                    { "shortcuts_removed", "• Shortcuts removed" },
                    { "path_removed", "• Removed from system PATH" },
                    { "system_clean", "Your system is now free of DevStack." }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Starting uninstallation process" },
                    { "stopping_services", "Stopping DevStack services..." },
                    { "process_stopped", "Process {0} terminated" },
                    { "process_stop_warning", "Warning: Could not terminate {0}: {1}" },
                    { "stop_services_error", "Error stopping services: {0}" },
                    { "removing_shortcuts", "Removing shortcuts..." },
                    { "shortcut_removed", "Shortcut removed: {0}" },
                    { "start_menu_removed", "Start Menu folder removed: {0}" },
                    { "shortcuts_error", "Error removing shortcuts: {0}" },
                    { "cleaning_registry", "Cleaning registry entries..." },
                    { "user_registry_removed", "User registry entries removed" },
                    { "machine_registry_removed", "Machine registry entries removed" },
                    { "uninstall_registry_removed", "Programs and Features entry removed" },
                    { "registry_error", "Error cleaning registry: {0}" },
                    { "removing_path", "Removing from system PATH..." },
                    { "user_path_removed", "Removed from user PATH" },
                    { "system_path_removed", "Removed from system PATH" },
                    { "system_path_warning", "Warning: Could not remove from system PATH (requires administrator privileges)" },
                    { "path_error", "Error removing from PATH: {0}" },
                    { "removing_files", "Removing files from {0}..." },
                    { "install_not_found", "Installation folder not found" },
                    { "files_removed_count", "{0} files removed" },
                    { "dirs_removed_count", "{0} empty folders removed" },
                    { "file_remove_warning", "Warning: Could not remove {0}: {1}" },
                    { "files_error", "Error removing files: {0}" },
                    { "removing_user_data", "Removing user data..." },
                    { "user_data_removed", "User data removed: {0}" },
                    { "user_data_error", "Error removing user data: {0}" },
                    { "self_deletion_scheduled", "Scheduled automatic removal of uninstaller" },
                    { "self_deletion_warning", "Warning: Could not schedule self-removal: {0}" },
                    { "uninstall_success", "Uninstallation completed successfully!" }
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
