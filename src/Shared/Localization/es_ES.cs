using System.Collections.Generic;

namespace DevStackShared.Localization
{
    public class es_ES : ILanguageProvider
    {
        public string LanguageCode => "es_ES";
        public string LanguageName => "Español";

        public Dictionary<string, object> GetCommonTranslations()
        {
            return new Dictionary<string, object>
            {
                { "language_name", "Español" },
                { "unknown", "Desconocido" },
                { "themes", new Dictionary<string, object>
                {
                    { "light", "Claro" },
                    { "dark", "Oscuro" },
                    { "messages", new Dictionary<string, object>
                    {
                        { "theme_changed", "Tema cambiado a {0}" }
                    }
                    }
                }
                },
                { "buttons", new Dictionary<string, object>
                {
                    { "back", "← Atrás" },
                    { "next", "Siguiente →" },
                    { "accept", "Acepto" },
                    { "install", "Instalar" },
                    { "finish", "Finalizar" },
                    { "cancel", "Cancelar" },
                    { "continue", "Continuar" },
                    { "uninstall", "🗑️ Desinstalar" },
                    { "yes", "Sí" },
                    { "no", "No" },
                    { "ok", "OK" }
                }
                },
                { "dialogs", new Dictionary<string, object>
                {
                    { "default_title", "Mensaje" }
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
                    { "no_component", "Ningún componente especificado para desinstalar." },
                    { "removing_shortcut", "Eliminando acceso directo para {0}..." },
                    { "unknown_component", "Componente desconocido: {0}" },
                    { "finished", "Desinstalación finalizada." }
                }
                },
                { "shortcuts", new Dictionary<string, object>
                {
                    { "created", "Acceso directo {0} creado apuntando a {1}" },
                    { "error_creating", "Error al crear enlace simbólico: {0}" },
                    { "fallback_copy", "Alternativo: Copia {0} creada en {1}" },
                    { "file_not_found", "Advertencia: archivo {0} no encontrado para crear acceso directo" },
                    { "removed", "Acceso directo {0} eliminado" },
                    { "not_found", "Acceso directo {0} no encontrado para eliminación" },
                    { "error_removing", "Error al eliminar acceso directo: {0}" }
                }
                },
                { "install", new Dictionary<string, object>
                {
                    { "already_installed", "{0} {1} ya está instalado." },
                    { "downloading", "Descargando {0} {1}..." },
                    { "running_installer", "Ejecutando instalador {0} {1}..." },
                    { "installed_via_installer", "{0} {1} instalado vía instalador en {2}" },
                    { "extracting", "Extrayendo..." },
                    { "installed", "{0} {1} instalado." },
                    { "installed_in", "{0} {1} instalado en {2}." },
                    { "error_installing", "Error al instalar {0} {1}: {2}" },
                    { "shortcut_creation_failed", "Advertencia: falló al crear acceso directo: {0}" },
                    { "component_installed", "{0} {1} instalado." }
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
                    { "ready_status", "Listo" },
                    { "initialization_error", "Error al inicializar DevStack GUI: {0}" },
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
                            { "title", "Panel" },
                            { "description", "Vista general del sistema" }
                        }
                        },
                        { "installed", new Dictionary<string, object>
                        {
                            { "title", "Instalados" },
                            { "description", "Herramientas instaladas" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Instalar" },
                            { "description", "Instalar nuevos componentes" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Desinstalar" },
                            { "description", "Eliminar componentes" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Servicios" },
                            { "description", "Control de servicios" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Configuración" },
                            { "description", "Configuración del sistema" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Sitios" },
                            { "description", "Gestionar sitios Nginx" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Utilidades" },
                            { "description", "Herramientas y consola" }
                        }
                        }
                    }
                    },
                    { "refresh_tooltip", "Actualizar todos los datos" }
                }
                },
                { "dashboard_tab", new Dictionary<string, object>
                {
                    { "title", "📊 Panel" },
                    { "cards", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Componentes" },
                            { "subtitle", "Haga clic para acceder" },
                            { "loading", "Cargando..." },
                            { "installed_count", "{0}/{1} instalados" },
                            { "none", "Ningún componente" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Instalar" },
                            { "subtitle", "Haga clic para acceder" },
                            { "description", "Agregar componentes" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Servicios" },
                            { "subtitle", "Haga clic para acceder" },
                            { "loading", "Cargando..." },
                            { "active_count", "{0}/{1} activos" },
                            { "none", "Ningún servicio activo" }
                        }
                        }
                    }
                    },
                    { "panels", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Componentes Instalados" },
                            { "refresh_tooltip", "Actualizar componentes instalados" },
                            { "install_button", "📥 Instalar" },
                            { "uninstall_button", "🗑️ Desinstalar" },
                            { "none", "Ningún componente instalado" },
                            { "installed_default", "Instalado" },
                            { "error_loading", "Error al cargar componentes" },
                            { "version_na", "N/A" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Servicios" },
                            { "refresh_tooltip", "Actualizar servicios" },
                            { "start_all", "▶️ Iniciar" },
                            { "stop_all", "⏹️ Detener" },
                            { "restart_all", "🔄 Reiniciar" },
                            { "none", "Ningún servicio encontrado" },
                            { "loading", "Cargando servicios..." },
                            { "status", new Dictionary<string, object>
                            {
                                { "active", "Activo" },
                                { "stopped", "Detenido" },
                                { "na", "N/A" }
                            }
                            }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "📥 Instalación Rápida" },
                            { "select_component", "Selecciona un componente para instalar." },
                            { "installing", "Instalando {0}..." },
                            { "success", "¡{0} instalado con éxito!" },
                            { "error", "Error al instalar {0}: {1}" },
                            { "install_button", "📥 Instalar" },
                            { "go_to_install", "Ir a Instalar" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "updating_components", "Actualizando componentes..." },
                        { "components_updated", "¡Componentes actualizados!" },
                        { "error_updating_components", "Error al actualizar componentes: {0}" },
                        { "updating_services", "Actualizando servicios..." },
                        { "services_updated", "¡Servicios actualizados!" },
                        { "error_updating_services", "Error al actualizar servicios: {0}" },
                        { "starting_all_services", "Iniciando todos los servicios..." },
                        { "all_services_started", "¡Todos los servicios han sido iniciados!" },
                        { "error_starting_services", "Error al iniciar servicios: {0}" },
                        { "stopping_all_services", "Deteniendo todos los servicios..." },
                        { "all_services_stopped", "¡Todos los servicios han sido detenidos!" },
                        { "error_stopping_services", "Error al detener servicios: {0}" },
                        { "restarting_all_services", "Reiniciando todos los servicios..." },
                        { "all_services_restarted", "¡Todos los servicios han sido reiniciados!" },
                        { "error_restarting_services", "Error al reiniciar servicios: {0}" },
                        { "select_component_install", "Seleccione un componente para instalar." },
                        { "installing_component", "Instalando {0}..." },
                        { "component_installed", "¡{0} instalado exitosamente!" },
                        { "error_installing_component", "Error al instalar {0}: {1}" },
                        { "opening_shell", "🚀 Abriendo shell interactivo para {0} v{1}" },
                        { "executing_component", "🚀 Ejecutando {0} v{1}" },
                        { "no_executable_found", "❌ No se encontró ejecutable para {0} v{1}" },
                        { "version_folder_not_found", "❌ Carpeta de versión no encontrada: {0}" },
                        { "component_not_executable", "❌ El componente {0} no es ejecutable" },
                        { "error_executing", "❌ Error al ejecutar {0} v{1}: {2}" },
                        { "error_updating_component_data", "Error al actualizar datos de componentes: {0}" },
                        { "error_updating_service_data", "Error al actualizar datos de servicios: {0}" }
                    }
                    }
                }
                },
                { "installed_tab", new Dictionary<string, object>
                {
                    { "title", "Herramientas Instaladas" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "tool", "Herramienta" },
                        { "versions", "Versiones Instaladas" },
                        { "status", "Estado" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Actualizar Lista" }
                    }
                    },
                    { "info", "Use las pestañas 'Instalar' y 'Desinstalar' para gestionar las herramientas" },
                    { "loading", "Cargando componentes instalados..." },
                    { "loaded", "{0} componentes cargados" },
                    { "error", "Error al cargar componentes: {0}" }
                }
                },
                { "install_tab", new Dictionary<string, object>
                {
                    { "title", "Instalar Nueva Herramienta" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Seleccione la herramienta:" },
                        { "select_version", "Seleccione la versión (deje vacío para la más reciente):" },
                        { "installed_component", "Componente Instalado:" },
                        { "installed_version", "Versión Instalada:" }
                    }
                    },
                    { "sections", new Dictionary<string, object>
                    {
                        { "install_component", "Instalar Componente" },
                        { "create_shortcuts", "Crear Accesos Directos para Componentes Instalados" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "install", "📥 Instalar" },
                        { "create_shortcut", "Crear Acceso Directo" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Seleccione un componente para instalar." },
                        { "select_component_warning", "Seleccione un componente" },
                        { "select_version_warning", "Seleccione una versión" },
                        { "installing", "Instalando {0}..." },
                        { "success", "¡{0} instalado correctamente!" },
                        { "error", "Error al instalar {0}" },
                        { "loading_versions", "Cargando versiones de {0}..." },
                        { "versions_loaded", "{0} versiones cargadas para {1}" },
                        { "versions_error", "Error al cargar versiones: {0}" },
                        { "component_not_found", "Componente '{0}' no encontrado" },
                        { "failed_to_load_versions", "Error al cargar versiones" },
                        { "shortcut_component_not_found", "Componente '{0}' no encontrado" },
                        { "shortcut_not_supported", "El componente '{0}' no admite la creación de accesos directos" },
                        { "shortcut_install_dir_not_found", "Directorio de instalación no encontrado: {0}" }
                    }
                    }
                }
                },
                { "uninstall_tab", new Dictionary<string, object>
                {
                    { "title", "Desinstalar Herramienta" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Seleccione la herramienta:" },
                        { "select_version", "Seleccione la versión:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "uninstall", "🗑️ Desinstalar" },
                        { "refresh", "🔄 Actualizar Lista" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Seleccione un componente para desinstalar." },
                        { "select_version", "Seleccione una versión para desinstalar." },
                        { "confirm", "¿Está seguro de que desea desinstalar {0}?" },
                        { "uninstalling", "Desinstalando {0}..." },
                        { "success", "¡{0} desinstalado correctamente!" },
                        { "error", "Error al desinstalar {0}" },
                        { "no_versions", "{0} no tiene versiones instaladas." },
                        { "not_installed", "{0} no está instalado" },
                        { "loading_components", "Cargando componentes instalados..." },
                        { "loading_versions", "Cargando versiones instaladas de {0}..." },
                        { "versions_loaded", "Versiones cargadas para {0}" },
                        { "versions_error", "Error al cargar versiones para desinstalación: {0}" },
                        { "components_available", "{0} componentes disponibles para desinstalación" },
                        { "reloading", "Recargando lista de componentes instalados..." }
                    }
                    },
                    { "warning", "¡Atención: Esta acción no se puede deshacer!" },
                    { "status", new Dictionary<string, object>
                    {
                        { "uninstalling", "Desinstalando {0}..." },
                        { "success", "¡{0} desinstalado correctamente!" },
                        { "error", "❌ Error al desinstalar {0}: {1}" },
                        { "error_short", "Error al desinstalar {0}" },
                        { "loading_versions", "Cargando versiones instaladas de {0}..." },
                        { "versions_loaded", "Versiones cargadas para {0}" },
                        { "not_installed", "{0} no está instalado" },
                        { "error_loading_versions", "Error al cargar versiones para desinstalación: {0}" },
                        { "loading_components", "Cargando componentes instalados..." },
                        { "components_count", "{0} componentes disponibles para desinstalación" },
                        { "reloading", "Recargando lista de componentes instalados..." },
                        { "error_loading_components", "Error al cargar componentes: {0}" }
                    }
                    }
                }
                },
                { "services_tab", new Dictionary<string, object>
                {
                    { "title", "Gestión de Servicios" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "component", "Componente" },
                        { "version", "Versión" },
                        { "status", "Estado" },
                        { "pid", "PID" },
                        { "copy_pid", "Copiar PID" },
                        { "actions", "Acciones" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Actualizar" },
                        { "start_all", "▶️ Iniciar Todos" },
                        { "stop_all", "⏹️ Detener Todos" },
                        { "restart_all", "🔄 Reiniciar Todos" },
                        { "start", "▶️" },
                        { "stop", "⏹️" },
                        { "restart", "🔄" },
                        { "copy_pid", "📋" }
                    }
                    },
                    { "tooltips", new Dictionary<string, object>
                    {
                        { "start", "Iniciar" },
                        { "stop", "Detener" },
                        { "restart", "Reiniciar" },
                        { "copy_pid", "Copiar PID" }
                    }
                    },
                    { "status", new Dictionary<string, object>
                    {
                        { "running", "En ejecución" },
                        { "stopped", "Detenido" },
                        { "active", "Activo" }
                    }
                    },
                    { "types", new Dictionary<string, object>
                    {
                        { "php_fpm", "PHP-FPM" },
                        { "web_server", "Servidor Web" },
                        { "database", "Base de Datos" },
                        { "search_engine", "Motor de Búsqueda" },
                        { "service", "Servicio" },
                        { "fastcgi", "FastCGI" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "loading", "Cargando servicios..." },
                        { "loaded", "{0} servicios cargados" },
                        { "error", "Error al cargar servicios: {0}" },
                        { "starting", "Iniciando {0} versión {1}..." },
                        { "started", "{0} iniciado correctamente" },
                        { "stopping", "Deteniendo {0} versión {1}..." },
                        { "stopped", "{0} detenido correctamente" },
                        { "restarting", "Reiniciando {0} versión {1}..." },
                        { "restarted", "{0} reiniciado correctamente" },
                        { "starting_all", "Iniciando todos los servicios..." },
                        { "started_all", "Todos los servicios iniciados" },
                        { "stopping_all", "Deteniendo todos los servicios..." },
                        { "stopped_all", "Todos los servicios detenidos" },
                        { "restarting_all", "Reiniciando todos los servicios..." },
                        { "restarted_all", "Todos los servicios reiniciados" },
                        { "pid_copied", "PID {0} copiado al portapapeles" },
                        { "no_pid", "El servicio no está en ejecución, no hay PID para copiar." },
                        { "error_copy_pid", "Error al copiar PID: {0}" },
                        { "error_start", "Error al iniciar servicio: {0}" },
                        { "error_stop", "Error al detener servicio: {0}" },
                        { "error_restart", "Error al reiniciar servicio: {0}" },
                        { "error_start_all", "Error al iniciar todos los servicios: {0}" },
                        { "error_stop_all", "Error al detener todos los servicios: {0}" },
                        { "error_restart_all", "Error al reiniciar todos los servicios: {0}" }
                    }
                    },
                    { "path_manager", new Dictionary<string, object>
                    {
                        { "not_initialized", "⚠️ PathManager no ha sido inicializado - PATH no actualizado" }
                    }
                    },
                    { "debug", new Dictionary<string, object>
                    {
                        { "processes_found", "Procesos encontrados para depuración: {0}" },
                        { "process_info", "  - {0} (PID: {1}) - Ruta: {2}" },
                        { "process_error", "  - {0} (PID: {1}) - Ruta: Error al acceder ({2})" },
                        { "found_service_components", "{0} componentes de servicio encontrados" },
                        { "component_dir_not_found", "Directorio del componente {0} no encontrado: {1}" },
                        { "component_versions_found", "Componente {0}: {1} versiones encontradas: {2}" },
                        { "checking_component_version", "Verificando {0} versión {1}" },
                        { "service_process_found", "  - Proceso {0} encontrado: {1} (PID: {2}) - Ruta: {3}" },
                        { "service_running", "{0} {1} está ejecutándose con PIDs: {2}" },
                        { "service_not_running", "{0} {1} no está ejecutándose" },
                        { "no_service_pattern", "Ningún patrón de servicio definido para {0}" },
                        { "component_check_error", "Error al verificar procesos {0}: {1}" },
                        { "php_dirs_found", "{0} directorios PHP encontrados: {1}" },
                        { "checking_php_version", "Verificando PHP versión {0} en el directorio {1}" },
                        { "php_process_found", "  - Proceso PHP encontrado: {0} (PID: {1}) - Ruta: {2}" },
                        { "process_check_error", "  - Error al verificar proceso {0}: {1}" },
                        { "php_running", "PHP {0} está ejecutándose con PIDs: {1}" },
                        { "php_not_running", "PHP {0} no está ejecutándose" },
                        { "php_check_error", "Error al verificar procesos PHP: {0}" },
                        { "nginx_dirs_found", "{0} directorios Nginx encontrados: {1}" },
                        { "checking_nginx_version", "Verificando Nginx versión {0} en el directorio {1}" },
                        { "nginx_process_found", "  - Proceso Nginx encontrado: {0} (PID: {1}) - Ruta: {2}" },
                        { "nginx_running", "Nginx {0} está ejecutándose con PID: {1}" },
                        { "nginx_not_running", "Nginx {0} no está ejecutándose" },
                        { "nginx_check_error", "Error al verificar procesos Nginx: {0}" },
                        { "load_services_error", "Error al cargar servicios en la GUI: {0}" },
                        { "start_all_services_error", "Error al iniciar todos los servicios en la GUI: {0}" },
                        { "stop_all_services_error", "Error al detener todos los servicios en la GUI: {0}" },
                        { "restart_all_services_error", "Error al reiniciar todos los servicios en la GUI: {0}" }
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
                            { "description", "Resumen del sistema" }
                        }
                        },
                        { "installed", new Dictionary<string, object>
                        {
                            { "title", "Instalados" },
                            { "description", "Herramientas instaladas" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Instalar" },
                            { "description", "Instalar nuevos componentes" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Desinstalar" },
                            { "description", "Eliminar componentes" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Servicios" },
                            { "description", "Control de servicios" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Configuración" },
                            { "description", "Configuración del sistema" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Sitios" },
                            { "description", "Gestionar sitios Nginx" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Utilidades" },
                            { "description", "Herramientas y consola" }
                        }
                        }
                    }
                    }
                }
                },
                { "config_tab", new Dictionary<string, object>
                {
                    { "title", "Configuración" },
                    { "path", new Dictionary<string, object>
                    {
                        { "title", "Gestión del PATH" },
                        { "description", "Añadir herramientas al PATH del sistema" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "add", "➕ Añadir al PATH" },
                            { "remove", "➖ Eliminar del PATH" }
                        }
                        }
                    }
                    },
                    { "directories", new Dictionary<string, object>
                    {
                        { "title", "Directorios" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "devstack_manager", "📂 DevStack Manager" },
                            { "tools", "📂 Herramientas" }
                        }
                        }
                    }
                    },
                    { "languages", new Dictionary<string, object>
                    {
                        { "title", "Idiomas" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_language", "Idioma de la Interfaz" }
                        }
                        },
                        { "messages", new Dictionary<string, object>
                        {
                            { "language_changed", "Idioma cambiado a {0}" }
                        }
                        }
                    }
                    },
                    { "themes", new Dictionary<string, object>
                    {
                        { "title", "Temas" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_theme", "Tema de la Interfaz" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "path_updated", "PATH actualizado correctamente" },
                        { "path_update_error", "Error al actualizar PATH" },
                        { "path_cleaned", "PATH limpiado correctamente" },
                        { "path_listed", "PATH listado" },
                        { "path_error", "Error al añadir al PATH: {0}" },
                        { "path_remove_error", "Error al eliminar del PATH: {0}" },
                        { "path_clean_error", "Error al limpiar PATH" },
                        { "path_list_error", "Error al listar PATH: {0}" },
                        { "exe_folder_opened", "Carpeta del ejecutable abierta" },
                        { "exe_folder_not_found", "No se pudo localizar la carpeta del ejecutable." },
                        { "exe_folder_error", "Error al abrir la carpeta del ejecutable: {0}" },
                        { "tools_folder_opened", "Carpeta de herramientas abierta" },
                        { "tools_folder_not_found", "No se pudo localizar la carpeta de herramientas." },
                        { "tools_folder_error", "Error al abrir la carpeta de herramientas: {0}" }
                    }
                    }
                }
                },
                { "sites_tab", new Dictionary<string, object>
                {
                    { "title", "Crear Configuración de Sitio Nginx" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "domain", "Dominio del sitio:" },
                        { "root_directory", "Directorio raíz:" },
                        { "php_upstream", "PHP Upstream:" },
                        { "nginx_version", "Versión de Nginx:" },
                        { "ssl_domain", "Dominio para SSL:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "browse", "📁 Buscar" },
                        { "create_site", "🌐 Crear Configuración de Sitio" },
                        { "generate_ssl", "🔒 Generar Certificado SSL" }
                    }
                    },
                    { "ssl", new Dictionary<string, object>
                    {
                        { "title", "Certificados SSL" },
                        { "generate_ssl", "Generar SSL" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_folder", "Seleccionar carpeta del sitio" },
                        { "creating_site", "Creando configuración para el sitio {0}..." },
                        { "site_created", "Sitio {0} creado" },
                        { "site_error", "Error al crear el sitio {0}: {1}" },
                        { "site_config_error", "Error al crear la configuración del sitio: {0}" },
                        { "enter_domain", "Ingrese un dominio para el sitio." },
                        { "enter_root", "Ingrese un directorio raíz para el sitio." },
                        { "select_php", "Seleccione una versión de PHP para el sitio." },
                        { "select_nginx", "Seleccione una versión de Nginx para el sitio." },
                        { "enter_ssl_domain", "Ingrese un dominio para generar el certificado SSL." },
                        { "domain_not_exists", "El dominio '{0}' no existe o no resuelve a ninguna IP." },
                        { "generating_ssl", "Generando certificado SSL para {0}..." },
                        { "ssl_generated", "Proceso de generación de SSL para {0} finalizado." },
                        { "ssl_error", "Error al generar el certificado SSL: {0}" },
                        { "restarting_nginx", "Reiniciando servicios de Nginx..." },
                        { "nginx_restarted", "Nginx v{0} reiniciado correctamente" },
                        { "nginx_restart_error", "Error al reiniciar Nginx v{0}: {1}" },
                        { "ssl_generation_completed", "Proceso de generación de SSL para {0} finalizado." },
                        { "ssl_generation_error", "❌ Error al generar certificado SSL: {0}" },
                        { "ssl_generation_error_status", "Error al generar SSL para {0}" },
                        { "ssl_generation_error_dialog", "Error al generar certificado SSL: {0}" },
                        { "no_nginx_restarted", "ℹ️ No se reinició ninguna versión de Nginx (puede que no estén en ejecución)" },
                        { "no_nginx_found", "❌ No se encontró ninguna versión de Nginx instalada" },
                        { "nginx_restart_general_error", "Error al reiniciar Nginx: {0}" }
                    }
                    },
                    { "info", "Los archivos de configuración se crearán automáticamente" }
                }
                },
                { "utilities_tab", new Dictionary<string, object>
                {
                    { "title", "Consola DevStack - Ejecute comandos directamente" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "command", "Comando:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "execute", "▶️ Ejecutar" },
                        { "clear", "❌" },
                        { "clear_tooltip", "Limpiar Consola" }
                    }
                    },
                    { "console_title", "Consola DevStack - Ejecute comandos directamente" },
                    { "command_label", "Comando:" },
                    { "execute_button", "▶️ Ejecutar" },
                    { "clear_console_tooltip", "Limpiar Consola" },
                    { "status_button", "Estado" },
                    { "installed_button", "Instalados" },
                    { "diagnostic_button", "Diagnóstico" },
                    { "test_button", "Probar" },
                    { "help_button", "Ayuda" },
                    { "console_header", "Consola de DevStack Manager" },
                    { "available_commands", "Comandos disponibles:" },
                    { "tip_message", "Consejo: Escriba comandos directamente en el campo superior o use los botones rápidos" },
                    { "executing_command", "Ejecutando: {0}" },
                    { "no_output", "(Comando ejecutado, sin salida generada)" },
                    { "devstack_not_found", "Error: No se pudo iniciar el proceso DevStack.exe" },
                    { "error", "ERROR" },
                    { "console_cleared", "Consola limpiada.\n\n" },
                    { "empty_command", "Comando vacío" },
                    { "command_execution_error", "Error al ejecutar comando: {0}" },
                    { "status", new Dictionary<string, object>
                    {
                        { "executing", "Ejecutando: {0}" },
                        { "executed", "Comando ejecutado" },
                        { "error", "Error al ejecutar comando" },
                        { "cleared", "Consola limpiada" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "list_usage", "Uso: list --installed o list <componente>" },
                        { "command_not_recognized", "Comando '{0}' no reconocido. Use 'help' para ver comandos disponibles." }
                    }
                    }
                }
                },
                { "console", new Dictionary<string, object>
                {
                    { "titles", new Dictionary<string, object>
                    {
                        { "install", "Salida de Consola - Instalar" },
                        { "uninstall", "Salida de Consola - Desinstalar" },
                        { "sites", "Salida de Consola - Sitios" },
                        { "config", "Salida de Consola - Configuración" },
                        { "utilities", "Salida de Consola" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "clear", "🗑️ Limpiar Consola" }
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
                        { "warning", "Advertencia" },
                        { "error", "Error" },
                        { "info", "Información" },
                        { "confirmation", "Confirmación" }
                    }
                    }
                }
                },
                { "status_bar", new Dictionary<string, object>
                {
                    { "refresh_tooltip", "Actualizar estado" },
                    { "updating", "Actualizando..." },
                    { "updated", "Estado actualizado" },
                    { "loading_data", "Iniciando carga de datos..." },
                    { "loading_installed", "Cargando componentes instalados..." },
                    { "loading_available", "Cargando componentes disponibles..." },
                    { "loading_services", "Cargando servicios y otras opciones..." },
                    { "loading_complete", "Todos los datos cargados correctamente" },
                    { "loading_error", "Error al cargar datos: {0}" },
                    { "shortcut_created", "Acceso directo creado con éxito para {0} {1}" },
                    { "shortcut_error", "Error al crear acceso directo para {0}" },
                    { "shortcut_create_error", "Error al crear acceso directo: {0}" },
                    { "creating_shortcut", "Creando acceso directo para {0} {1}..." },
                    { "error_loading_initial", "Error al cargar datos iniciales: {0}" },
                    { "error_loading_components", "Error al cargar componentes: {0}" },
                    { "error_loading_shortcuts", "Error al cargar componentes para accesos directos: {0}" },
                    { "error_loading_versions", "Error al cargar versiones para acceso directo: {0}" },
                    { "error_loading_dashboard", "Error al cargar datos del Dashboard: {0}" },
                    { "opening_shell", "Abriendo shell interactivo para {0} versión {1}" },
                    { "executing_component", "Ejecutando {0} versión {1}: {2}" },
                    { "no_executable_found", "No se encontró ejecutable en {0}" },
                    { "version_folder_not_found", "Carpeta de versión no encontrada: {0}" },
                    { "component_not_executable", "El componente {0} no es ejecutable o no está instalado." },
                    { "component_not_available", "No se pudo obtener el componente para ejecución." },
                    { "version_not_available", "No se pudo obtener la versión para ejecución." },
                    { "error_executing_component", "Error al ejecutar componente: {0}" }
                }
                }
            };
        }

        public Dictionary<string, object> GetInstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Asistente de Instalación" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "cancel_title", "Cancelar Instalación" },
                    { "cancel_message", "¿Está seguro de que desea cancelar la instalación?" },
                    { "installation_error_title", "Error" },
                    { "installation_error_message", "Error en la instalación: {0}" },
                    { "folder_dialog_title", "Seleccionar carpeta de instalación" },
                    { "startup_error_title", "Error del Instalador DevStack" },
                    { "startup_error_message", "Error al iniciar el instalador: {0}\n\nDetalles: {1}" },
                    { "initialization_error_title", "Error de Inicialización" },
                    { "initialization_error_message", "Error al inicializar ventana del instalador: {0}" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "Bienvenido a DevStack Manager" },
                    { "description", "Este asistente le guiará a través de la instalación de DevStack Manager en su ordenador." },
                    { "app_name", "DevStack Manager" },
                    { "version", "Versión {0}" },
                    { "app_description", "DevStack Manager es una herramienta integral de gestión de entornos de desarrollo que le ayuda a instalar, configurar y administrar varias herramientas y servicios de desarrollo.\n\nHaga clic en 'Siguiente' para continuar con la instalación." },
                    { "language_label", "Idioma de la instalación:" }
                }
                },
                { "license", new Dictionary<string, object>
                {
                    { "title", "Acuerdo de Licencia" },
                    { "description", "Por favor, lea cuidadosamente el siguiente acuerdo de licencia." },
                    { "label", "Por favor, lea y acepte el acuerdo de licencia:" },
                    { "text", "Licencia MIT\n\nCopyright (c) 2025 DevStackManager\n\nSe concede permiso, de forma gratuita, a cualquier persona que obtenga una copia\nde este software y los archivos de documentación asociados (el \"Software\"), para negociar\nel Software sin restricción, incluyendo, sin limitación, los derechos\nde usar, copiar, modificar, fusionar, publicar, distribuir, sublicenciar y/o vender\ncopias del Software, y permitir que las personas a quienes se les proporcione el Software\nlo hagan, sujeto a las siguientes condiciones:\n\nEl aviso de copyright anterior y este aviso de permiso deben incluirse en todas\nlas copias o partes sustanciales del Software.\n\nEL SOFTWARE SE PROPORCIONA \"TAL CUAL\", SIN GARANTÍA DE NINGÚN TIPO, EXPRESA O\nIMPLÍCITA, INCLUYENDO PERO NO LIMITADO A LAS GARANTÍAS DE COMERCIALIZACIÓN,\nIDONEIDAD PARA UN PROPÓSITO PARTICULAR Y NO INFRACCIÓN. EN NINGÚN CASO LOS\nAUTORES O TITULARES DE LOS DERECHOS DE AUTOR SERÁN RESPONSABLES DE NINGUNA RECLAMACIÓN, DAÑOS U OTRA\nRESPONSABILIDAD, YA SEA EN UNA ACCIÓN DE CONTRATO, AGRAVIO O DE OTRO TIPO, QUE SURJA DE,\nFUERA DE O EN CONEXIÓN CON EL SOFTWARE O EL USO U OTRAS NEGOCIACIONES EN EL\nSOFTWARE." }
                }
                },
                { "installation_path", new Dictionary<string, object>
                {
                    { "title", "Elegir Ubicación de Instalación" },
                    { "description", "Elija la carpeta donde se instalará DevStack Manager." },
                    { "label", "Carpeta de destino:" },
                    { "browser", "Buscar..." },
                    { "space_required", "Espacio requerido: {0} MB" },
                    { "space_available", "Espacio disponible: {0}" },
                    { "info", "DevStack Manager se instalará en esta carpeta junto con todos sus componentes y configuraciones." }
                }
                },
                { "components", new Dictionary<string, object>
                {
                    { "title", "Seleccionar Opciones Adicionales" },
                    { "description", "Elija las opciones adicionales para su instalación de DevStack Manager." },
                    { "label", "Opciones adicionales:" },
                    { "desktop_shortcuts", "🖥️ Crear accesos directos en el escritorio" },
                    { "start_menu_shortcuts", "📂 Crear accesos directos en el Menú Inicio" },
                    { "add_to_path", "⚡ Añadir DevStack al PATH del sistema (recomendado)" },
                    { "path_info", "Añadir al PATH permite usar comandos de DevStack directamente en la terminal desde cualquier ubicación." }
                }
                },
                { "ready_to_install", new Dictionary<string, object>
                {
                    { "title", "Listo para Instalar" },
                    { "description", "El asistente está listo para comenzar la instalación. Revise sus configuraciones abajo." },
                    { "summary_label", "Resumen de la instalación:" },
                    { "destination", "Carpeta de destino:" },
                    { "components_header", "Componentes a instalar:" },
                    { "cli_component", "• DevStack CLI (Interfaz de Línea de Comandos)" },
                    { "gui_component", "• DevStack GUI (Interfaz Gráfica)" },
                    { "uninstaller_component", "• Desinstalador de DevStack" },
                    { "config_component", "• Archivos de configuración y componentes" },
                    { "options_header", "Opciones adicionales:" },
                    { "create_desktop", "• Crear accesos directos en el escritorio" },
                    { "create_start_menu", "• Crear accesos directos en el Menú Inicio" },
                    { "add_path", "• Añadir al PATH del sistema" },
                    { "space_required_summary", "Espacio requerido: {0} MB" }
                }
                },
                { "installing", new Dictionary<string, object>
                {
                    { "title", "Instalando DevStack Manager" },
                    { "description", "Por favor, espere mientras se instala DevStack Manager..." },
                    { "preparing", "Preparando instalación..." },
                    { "extracting", "Extrayendo archivos de instalación..." },
                    { "downloading_sdk", "Descargando .NET SDK..." },
                    { "compiling_projects", "Compilando proyectos de DevStack..." },
                    { "creating_directory", "Creando directorio de instalación..." },
                    { "installing_files", "Instalando archivos de DevStack..." },
                    { "registering", "Registrando instalación..." },
                    { "creating_desktop", "Creando accesos directos en el escritorio..." },
                    { "creating_start_menu", "Creando accesos directos en el Menú Inicio..." },
                    { "adding_path", "Añadiendo al PATH del sistema..." },
                    { "completed", "¡Instalación completada con éxito!" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Instalación Completada" },
                    { "description", "DevStack Manager se ha instalado correctamente en su ordenador." },
                    { "success_icon", "✅" },
                    { "success_title", "¡Instalación Completada con Éxito!" },
                    { "success_message", "DevStack Manager se ha instalado correctamente. Ahora puede usar la aplicación para gestionar su entorno de desarrollo." },
                    { "install_location", "Ubicación de instalación:" },
                    { "launch_now", "🚀 Ejecutar DevStack Manager ahora" }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Iniciando proceso de instalación" },
                    { "extracted", "Archivos extraídos correctamente" },
                    { "source_extracted", "Archivos fuente extraídos" },
                    { "downloading_sdk", "Descargando .NET SDK para compilación..." },
                    { "sdk_downloaded", ".NET SDK descargado y extraído" },
                    { "compiling", "Compilando proyectos de DevStack..." },
                    { "compilation_complete", "Compilación completada con éxito" },
                    { "creating_dir", "Creando directorio: {0}" },
                    { "installing", "Instalando archivos de la aplicación" },
                    { "registering", "Registrando instalación en Windows" },
                    { "desktop_shortcuts", "Creando accesos directos en el escritorio" },
                    { "start_menu_shortcuts", "Creando accesos directos en el Menú Inicio" },
                    { "adding_path", "Añadiendo DevStack al PATH del sistema" },
                    { "path_added", "Añadido al PATH del usuario correctamente" },
                    { "path_exists", "Ya existe en el PATH" },
                    { "completed_success", "¡Instalación completada con éxito!" },
                    { "cleanup", "Archivos temporales eliminados" },
                    { "cleanup_warning", "Advertencia: No se pudo eliminar el archivo temporal: {0}" },
                    { "shortcuts_warning", "Advertencia: No se pudieron crear accesos directos en el escritorio: {0}" },
                    { "start_menu_warning", "Advertencia: No se pudieron crear accesos directos en el Menú Inicio: {0}" },
                    { "path_warning", "Advertencia: No se pudo añadir al PATH: {0}" }
                }
                }
            };
        }

        public Dictionary<string, object> GetUninstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Desinstalador" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "uninstall_error_title", "Error de Desinstalación" },
                    { "uninstall_error_message", "Error durante la desinstalación: {0}" },
                    { "startup_error_title", "Error del Desinstalador DevStack" },
                    { "startup_error_message", "Error al iniciar el desinstalador: {0}\n\nDetalles: {1}" },
                    { "initialization_error_title", "Error de Inicialización" },
                    { "initialization_error_message", "Error al inicializar la ventana del desinstalador: {0}" },
                    { "cancel_title", "Cancelar Desinstalación" },
                    { "cancel_message", "¿Está seguro de que desea cancelar la desinstalación?" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "Desinstalador DevStack" },
                    { "description", "Este asistente eliminará DevStack de su ordenador" },
                    { "app_name", "Desinstalador DevStack" },
                    { "version", "Versión {0}" },
                    { "app_description", "Este asistente le guiará a través del proceso de eliminación completa de DevStack de su sistema." },
                    { "language_label", "Idioma:" }
                }
                },
                { "confirmation", new Dictionary<string, object>
                {
                    { "title", "Confirmación de Desinstalación" },
                    { "description", "Por favor, confirme que desea continuar con la eliminación de DevStack" },
                    { "warning_title", "⚠️ Atención - Esta acción no se puede deshacer" },
                    { "warning_text", "La desinstalación eliminará completamente DevStack de su sistema, incluyendo:" },
                    { "items", new Dictionary<string, object>
                    {
                        { "program_files", "• Todos los archivos del programa" },
                        { "user_data", "• Configuraciones y datos de usuario" },
                        { "shortcuts", "• Accesos directos del escritorio y menú inicio" },
                        { "registry", "• Entradas del registro de Windows" },
                        { "services", "• Servicios y procesos relacionados" },
                        { "path_variables", "• Variables de entorno PATH" }
                    }
                    },
                    { "install_found", "📁 Carpeta de instalación encontrada:" },
                    { "install_not_found", "❌ Carpeta de instalación no encontrada automáticamente" },
                    { "install_not_found_desc", "DevStack puede no estar instalado correctamente o ya haber sido eliminado. La desinstalación solo limpiará registros y accesos directos restantes." },
                    { "space_to_free", "📊 Espacio que se liberará: {0}" }
                }
                },
                { "uninstall_options", new Dictionary<string, object>
                {
                    { "title", "Opciones de Desinstalación" },
                    { "description", "Elija qué desea eliminar durante la desinstalación" },
                    { "label", "Seleccione los componentes a eliminar:" },
                    { "user_data", "🗂️ Eliminar datos y configuraciones de usuario" },
                    { "user_data_desc", "Incluye configuraciones, registros y archivos de datos guardados por DevStack" },
                    { "registry", "🔧 Eliminar entradas del registro" },
                    { "registry_desc", "Elimina claves de registro e información de instalación" },
                    { "shortcuts", "🔗 Eliminar accesos directos" },
                    { "shortcuts_desc", "Elimina accesos directos del escritorio y menú inicio" },
                    { "path", "🛤️ Eliminar del PATH del sistema" },
                    { "path_desc", "Elimina la ruta de DevStack de las variables de entorno" },
                    { "info", "Recomendamos mantener todas las opciones seleccionadas para una eliminación completa del sistema." }
                }
                },
                { "ready_to_uninstall", new Dictionary<string, object>
                {
                    { "title", "Listo para Desinstalar" },
                    { "description", "Revise las configuraciones y haga clic en Desinstalar para continuar" },
                    { "summary_label", "Resumen de la desinstalación:" },
                    { "components_header", "COMPONENTES A ELIMINAR:" },
                    { "installation_location", "📁 Ubicación de instalación:" },
                    { "not_found", "No encontrado" },
                    { "program_components", "🗂️ Componentes del programa:" },
                    { "executables", "  • Archivos ejecutables (DevStack.exe, DevStackGUI.exe)" },
                    { "libraries", "  • Bibliotecas y dependencias" },
                    { "config_files", "  • Archivos de configuración" },
                    { "documentation", "  • Documentación y recursos" },
                    { "selected_options", "OPCIONES SELECCIONADAS:" },
                    { "user_data_selected", "✓ Los datos de usuario serán eliminados" },
                    { "user_data_preserved", "✗ Los datos de usuario serán preservados" },
                    { "registry_selected", "✓ Las entradas del registro serán eliminadas" },
                    { "registry_preserved", "✗ Las entradas del registro serán preservadas" },
                    { "shortcuts_selected", "✓ Los accesos directos serán eliminados" },
                    { "shortcuts_preserved", "✗ Los accesos directos serán preservados" },
                    { "path_selected", "✓ Se eliminará del PATH del sistema" },
                    { "path_preserved", "✗ Permanecerá en el PATH del sistema" },
                    { "space_to_free", "💾 Espacio a liberar: {0}" }
                }
                },
                { "uninstalling", new Dictionary<string, object>
                {
                    { "title", "Desinstalando" },
                    { "description", "Por favor, espere mientras DevStack se elimina de su sistema" },
                    { "preparing", "Preparando desinstalación..." },
                    { "stopping_services", "Deteniendo servicios..." },
                    { "removing_shortcuts", "Eliminando accesos directos..." },
                    { "cleaning_registry", "Limpiando registro..." },
                    { "removing_path", "Eliminando del PATH..." },
                    { "removing_files", "Eliminando archivos..." },
                    { "removing_user_data", "Eliminando datos de usuario..." },
                    { "finalizing", "Finalizando..." },
                    { "completed", "¡Desinstalación completada!" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Desinstalación Completada" },
                    { "description", "DevStack se ha eliminado correctamente de su sistema" },
                    { "success_icon", "✅" },
                    { "success_title", "¡Desinstalación Completada!" },
                    { "success_message", "DevStack se ha eliminado correctamente de su sistema. Todos los componentes seleccionados han sido limpiados." },
                    { "summary_title", "📊 Resumen de la desinstalación:" },
                    { "files_removed", "• Archivos eliminados de: {0}" },
                    { "user_data_removed", "• Datos de usuario eliminados" },
                    { "registry_cleaned", "• Entradas del registro limpiadas" },
                    { "shortcuts_removed", "• Accesos directos eliminados" },
                    { "path_removed", "• Eliminado del PATH del sistema" },
                    { "system_clean", "El sistema ahora está libre de DevStack." }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Iniciando proceso de desinstalación" },
                    { "stopping_services", "Deteniendo servicios de DevStack..." },
                    { "process_stopped", "Proceso {0} finalizado" },
                    { "process_stop_warning", "Advertencia: No se pudo finalizar {0}: {1}" },
                    { "stop_services_error", "Error al detener servicios: {0}" },
                    { "removing_shortcuts", "Eliminando accesos directos..." },
                    { "shortcut_removed", "Acceso directo eliminado: {0}" },
                    { "start_menu_removed", "Carpeta del menú inicio eliminada: {0}" },
                    { "shortcuts_error", "Error al eliminar accesos directos: {0}" },
                    { "cleaning_registry", "Limpiando entradas del registro..." },
                    { "user_registry_removed", "Entradas del registro de usuario eliminadas" },
                    { "machine_registry_removed", "Entradas del registro de máquina eliminadas" },
                    { "uninstall_registry_removed", "Entrada de programas y recursos eliminada" },
                    { "registry_error", "Error al limpiar el registro: {0}" },
                    { "removing_path", "Eliminando del PATH del sistema..." },
                    { "user_path_removed", "Eliminado del PATH del usuario" },
                    { "system_path_removed", "Eliminado del PATH del sistema" },
                    { "system_path_warning", "Advertencia: No se pudo eliminar del PATH del sistema (requiere privilegios de administrador)" },
                    { "path_error", "Error al eliminar del PATH: {0}" },
                    { "removing_files", "Eliminando archivos de {0}..." },
                    { "install_not_found", "Carpeta de instalación no encontrada" },
                    { "files_removed_count", "{0} archivos eliminados" },
                    { "dirs_removed_count", "{0} carpetas vacías eliminadas" },
                    { "file_remove_warning", "Advertencia: No se pudo eliminar {0}: {1}" },
                    { "files_error", "Error al eliminar archivos: {0}" },
                    { "removing_user_data", "Eliminando datos de usuario..." },
                    { "user_data_removed", "Datos de usuario eliminados: {0}" },
                    { "user_data_error", "Error al eliminar datos de usuario: {0}" },
                    { "self_deletion_scheduled", "Eliminación automática del desinstalador programada" },
                    { "self_deletion_warning", "Advertencia: No se pudo programar la auto-eliminación: {0}" },
                    { "uninstall_success", "¡Desinstalación completada con éxito!" }
                }
                }
            };
        }

        public Dictionary<string, object> GetCliTranslations()
        {
            return new Dictionary<string, object>
            {
                { "shell", new Dictionary<string, object>
                {
                    { "interactive_prompt", "Shell Interactivo DevStack. Escriba 'help' para ayuda o 'exit' para salir." },
                    { "prompt", "DevStack> " },
                    { "exit_code", "(código de salida: {0})" },
                    { "command_requires_admin", "El comando '{0}' requiere privilegios de administrador." },
                    { "run_as_admin_hint", "Ejecute DevStack como administrador o use 'DevStack.exe {0}' en un símbolo del sistema de administrador." }
                }
                },
                { "commands", new Dictionary<string, object>
                {
                    { "unknown", "Comando desconocido: {0}" },
                    { "help_title", "DevStack CLI - Comandos disponibles:" },
                    { "gui_hint", "Para la interfaz gráfica, use: DevStackGUI.exe" },
                    { "table_header_cmd", "Comando" },
                    { "table_header_desc", "Descripción" },
                    { "help_install", "Instala una herramienta o versión específica." },
                    { "help_uninstall", "Elimina una herramienta o versión específica." },
                    { "help_list", "Lista versiones disponibles o instaladas." },
                    { "help_path", "Gestiona PATH para herramientas instaladas." },
                    { "help_status", "Muestra el estado de todas las herramientas." },
                    { "help_test", "Prueba todas las herramientas instaladas." },
                    { "help_update", "Actualiza una herramienta a la última versión." },
                    { "help_deps", "Verifica las dependencias del sistema." },
                    { "help_alias", "Crea un alias .bat para la versión de la herramienta." },
                    { "help_global", "Agrega DevStack al PATH y crea alias global." },
                    { "help_self_update", "Actualiza DevStackManager." },
                    { "help_clean", "Elimina logs y archivos temporales." },
                    { "help_backup", "Crea copia de seguridad de configs y logs." },
                    { "help_logs", "Muestra las últimas líneas del log." },
                    { "help_enable", "Activa un servicio de Windows." },
                    { "help_disable", "Desactiva un servicio de Windows." },
                    { "help_config", "Abre el directorio de configuración." },
                    { "help_reset", "Elimina y reinstala una herramienta." },
                    { "help_ssl", "Genera certificado SSL autofirmado." },
                    { "help_db", "Gestiona bases de datos básicas." },
                    { "help_service", "Lista servicios DevStack (Windows)." },
                    { "help_doctor", "Diagnóstico del entorno DevStack." },
                    { "help_language", "Lista o cambia el idioma de la interfaz." },
                    { "help_site", "Crea configuración de sitio nginx." },
                    { "help_help", "Muestra esta ayuda." }
                }
                },
                { "status", new Dictionary<string, object>
                {
                    { "title", "Estado de DevStack:" },
                    { "installed", "{0} instalado(s):" },
                    { "running", "[ejecutando]" },
                    { "stopped", "[detenido]" },
                    { "installed_versions", "{0} instalado(s):" }
                }
                },
                { "test", new Dictionary<string, object>
                {
                    { "title", "Probando herramientas instaladas:" },
                    { "not_found", "{0}: no encontrado." },
                    { "error_executing", "{0}: error al ejecutar {1}" },
                    { "tool_output", "{0}: {1}" }
                }
                },
                { "deps", new Dictionary<string, object>
                {
                    { "title", "Verificando dependencias del sistema..." },
                    { "missing_admin", "Permiso de administrador" },
                    { "all_present", "Todas las dependencias están presentes." },
                    { "missing_deps", "Dependencias faltantes: {0}" }
                }
                },
                { "usage", new Dictionary<string, object>
                {
                    { "list", "Uso: DevStackManager list <php|node|python|composer|mysql|nginx|phpmyadmin|git|mongodb|pgsql|elasticsearch|wpcli|adminer|go|openssl|phpcsfixer|--installed>" },
                    { "site", "Uso: DevStackManager site <dominio> -Root <directorio> -PHP <php-upstream> -Nginx <nginx-version>" },
                    { "site_error_domain", "Error: el dominio es obligatorio." },
                    { "site_error_root", "Error: Root es obligatorio." },
                    { "site_error_php", "Error: PHP es obligatorio." },
                    { "site_error_nginx", "Error: Nginx es obligatorio." },
                    { "start", "Uso: DevStackManager start <nginx|php|--all> [<x.x.x>]" },
                    { "start_version", "Uso: DevStackManager start <nginx|php> <x.x.x>" },
                    { "stop", "Uso: DevStackManager stop <nginx|php|--all> [<x.x.x>]" },
                    { "stop_version", "Uso: DevStackManager stop <nginx|php> <x.x.x>" },
                    { "restart", "Uso: DevStackManager restart <nginx|php|--all> [<x.x.x>]" },
                    { "restart_version", "Uso: DevStackManager restart <nginx|php> <x.x.x>" },
                    { "alias", "Uso: DevStackManager alias <componente> <versión>" },
                    { "enable", "Uso: DevStackManager enable <servicio>" },
                    { "disable", "Uso: DevStackManager disable <servicio>" },
                    { "reset", "Uso: DevStackManager reset <componente>" },
                    { "db", "Uso: DevStackManager db <mysql|pgsql|mongo> <comando> [args...]" }
                }
                },
                { "logs", new Dictionary<string, object>
                {
                    { "last_lines", "Últimas {0} líneas de {1}:" },
                    { "not_found", "Archivo de registro no encontrado." }
                }
                },
                { "service", new Dictionary<string, object>
                {
                    { "enabled", "Servicio {0} activado." },
                    { "disabled", "Servicio {0} desactivado." },
                    { "error_enable", "Error al activar el servicio {0}: {1}" },
                    { "error_disable", "Error al desactivar el servicio {0}: {1}" },
                    { "none_found", "No se encontraron servicios DevStack." },
                    { "list_header", "Nombre               Estado           DisplayName" }
                }
                },
                { "config", new Dictionary<string, object>
                {
                    { "opened", "Directorio de configuración abierto." },
                    { "not_found", "Directorio de configuración no encontrado." }
                }
                },
                { "reset", new Dictionary<string, object>
                {
                    { "resetting", "Reiniciando {0}..." },
                    { "completed", "{0} reiniciado." }
                }
                },
                { "db", new Dictionary<string, object>
                {
                    { "mysql_not_found", "mysql.exe no encontrado." },
                    { "pgsql_not_found", "psql.exe no encontrado." },
                    { "mongo_not_found", "mongo.exe no encontrado." },
                    { "unknown_command_mysql", "Comando de base de datos MySQL desconocido." },
                    { "unknown_command_pgsql", "Comando de base de datos PostgreSQL desconocido." },
                    { "unknown_command_mongo", "Comando de base de datos MongoDB desconocido." },
                    { "unsupported_db", "Base de datos no soportada: {0}" }
                }
                },
                { "doctor", new Dictionary<string, object>
                {
                    { "title", "Diagnóstico del entorno DevStack:" },
                    { "path_synced", "PATH sincronizado con la configuración del usuario." },
                    { "path_header", "PATH (Proceso + Usuario + DevStack)" },
                    { "user_header", "Usuario" },
                    { "system_header", "Sistema" }
                }
                },
                { "global", new Dictionary<string, object>
                {
                    { "added", "Directorio {0} agregado al PATH del usuario." },
                    { "already_exists", "El directorio {0} ya está en el PATH del usuario." },
                    { "run_anywhere", "Ahora puede ejecutar 'DevStackManager' desde cualquier lugar en la terminal." }
                }
                },
                { "language", new Dictionary<string, object>
                {
                    { "available_title", "Idiomas disponibles:" },
                    { "current_marker", " (actual)" },
                    { "change_hint", "Para cambiar el idioma, use: DevStack language <código>" },
                    { "example", "Ejemplo: DevStack language es_ES" },
                    { "not_found", "Idioma '{0}' no encontrado." },
                    { "available_list", "Idiomas disponibles:" },
                    { "changed", "Idioma cambiado a: {0} ({1})" },
                    { "note_gui", "Nota: El cambio de idioma afectará principalmente a la interfaz gráfica (GUI)." },
                    { "note_cli", "Algunos comandos de la CLI pueden no estar completamente traducidos." },
                    { "error_changing", "Error al cambiar el idioma: {0}" }
                }
                },
                { "self_update", new Dictionary<string, object>
                {
                    { "updating", "Actualizando mediante git pull..." },
                    { "success", "DevStackManager actualizado correctamente." },
                    { "error", "Error al actualizar mediante git: {0}" },
                    { "not_git_repo", "No es un repositorio git. Actualice manualmente copiando archivos del repositorio." }
                }
                },
                { "clean", new Dictionary<string, object>
                {
                    { "completed", "Limpieza completada. ({0} elementos eliminados)" }
                }
                },
                { "backup", new Dictionary<string, object>
                {
                    { "created", "Copia de seguridad creada en {0}" }
                }
                },
                { "path", new Dictionary<string, object>
                {
                    { "help_title", "Uso del comando path:" },
                    { "help_add", "  path         - Agregar directorios de herramientas al PATH" },
                    { "help_add_explicit", "  path add     - Agregar directorios de herramientas al PATH" },
                    { "help_remove", "  path remove  - Eliminar todos los directorios DevStack del PATH" },
                    { "help_remove_specific", "  path remove <dir1> <dir2> ... - Eliminar directorios específicos del PATH" },
                    { "help_list", "  path list    - Listar todos los directorios en el PATH del usuario" },
                    { "help_help", "  path help    - Mostrar esta ayuda" },
                    { "unknown_subcommand", "Subcomando desconocido: {0}" },
                    { "use_help", "Use 'path help' para ver los comandos disponibles." },
                    { "manager_not_initialized", "PathManager no inicializado." }
                }
                },
                { "alias", new Dictionary<string, object>
                {
                    { "created", "Alias creado: {0}" },
                    { "executable_not_found", "Ejecutable no encontrado para {0} {1}" }
                }
                },
                { "directories", new Dictionary<string, object>
                {
                    { "nginx_not_found", "Directorio de nginx no encontrado. Omitiendo." },
                    { "php_not_found", "Directorio de PHP no encontrado. Omitiendo." }
                }
                },
                { "error", new Dictionary<string, object>
                {
                    { "unexpected", "Error inesperado: {0}" },
                    { "admin_request", "Error al solicitar privilegios de administrador: {0}" },
                    { "list_services", "Error al listar servicios: {0}" }
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
            all["cli"] = GetCliTranslations();
            all["installer"] = GetInstallerTranslations();
            all["uninstaller"] = GetUninstallerTranslations();
            
            return all;
        }
    }
}
