using System.Collections.Generic;

namespace DevStackShared.Localization
{
    public class pt_BR : ILanguageProvider
    {
        public string LanguageCode => "pt_BR";
        public string LanguageName => "Português (Brasil)";

        public Dictionary<string, object> GetCommonTranslations()
        {
            return new Dictionary<string, object>
            {
                { "language_name", "Português (Brasil)" },
                { "unknown", "Desconhecido" },
                { "themes", new Dictionary<string, object>
                {
                    { "light", "Claro" },
                    { "dark", "Escuro" },
                    { "messages", new Dictionary<string, object>
                    {
                        { "theme_changed", "Tema alterado para {0}" }
                    }
                    }
                }
                },
                { "buttons", new Dictionary<string, object>
                {
                    { "back", "← Voltar" },
                    { "next", "Avançar →" },
                    { "accept", "Eu Aceito" },
                    { "install", "Instalar" },
                    { "finish", "Concluir" },
                    { "cancel", "Cancelar" },
                    { "continue", "Continuar" },
                    { "uninstall", "🗑️ Desinstalar" },
                    { "yes", "Sim" },
                    { "no", "Não" },
                    { "ok", "OK" }
                }
                },
                { "dialogs", new Dictionary<string, object>
                {
                    { "default_title", "Mensagem" }
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
                    { "no_component", "Nenhum componente especificado para desinstalar." },
                    { "removing_shortcut", "Removendo atalho para {0}..." },
                    { "unknown_component", "Componente desconhecido: {0}" },
                    { "finished", "Uninstall finalizado." }
                }
                },
                { "shortcuts", new Dictionary<string, object>
                {
                    { "created", "Atalho {0} criado apontando para {1}" },
                    { "error_creating", "Erro ao criar atalho simbólico: {0}" },
                    { "fallback_copy", "Fallback: Cópia {0} criada em {1}" },
                    { "file_not_found", "Aviso: arquivo {0} não encontrado para criar atalho" },
                    { "removed", "Atalho {0} removido" },
                    { "not_found", "Atalho {0} não encontrado para remoção" },
                    { "error_removing", "Erro ao remover atalho: {0}" }
                }
                },
                { "install", new Dictionary<string, object>
                {
                    { "already_installed", "{0} {1} já está instalado." },
                    { "downloading", "Baixando {0} {1}..." },
                    { "running_installer", "Executando instalador {0} {1}..." },
                    { "installed_via_installer", "{0} {1} instalado via instalador em {2}" },
                    { "extracting", "Extraindo..." },
                    { "installed", "{0} {1} instalado." },
                    { "installed_in", "{0} {1} instalado em {2}." },
                    { "error_installing", "Erro ao instalar {0} {1}: {2}" },
                    { "shortcut_creation_failed", "Aviso: falha ao criar atalho: {0}" },
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
                    { "ready_status", "Pronto" },
                    { "initialization_error", "Erro ao inicializar DevStack GUI: {0}" },
                    { "error_title", "DevStack Manager - Erro" }
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
                            { "description", "Visão geral do sistema" }
                        }
                        },
                        { "installed", new Dictionary<string, object>
                        {
                            { "title", "Instalados" },
                            { "description", "Ferramentas instaladas" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Instalar" },
                            { "description", "Instalar novos componentes" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Desinstalar" },
                            { "description", "Remover componentes" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Serviços" },
                            { "description", "Controle de serviços" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Configurações" },
                            { "description", "Configurações do sistema" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Sites" },
                            { "description", "Gerenciar sites Nginx" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Utilitários" },
                            { "description", "Ferramentas e console" }
                        }
                        }
                    }
                    },
                    { "refresh_tooltip", "Atualizar todos os dados" }
                }
                },
                { "dashboard_tab", new Dictionary<string, object>
                {
                    { "title", "📊 Dashboard" },
                    { "cards", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Componentes" },
                            { "subtitle", "Clique para acessar" },
                            { "loading", "Carregando..." },
                            { "installed_count", "{0}/{1} instalados" },
                            { "none", "Nenhum componente" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Instalar" },
                            { "subtitle", "Clique para acessar" },
                            { "description", "Adicionar componentes" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Serviços" },
                            { "subtitle", "Clique para acessar" },
                            { "loading", "Carregando..." },
                            { "active_count", "{0}/{1} ativos" },
                            { "none", "Nenhum serviço ativo" }
                        }
                        }
                    }
                    },
                    { "panels", new Dictionary<string, object>
                    {
                        { "components", new Dictionary<string, object>
                        {
                            { "title", "Componentes Instalados" },
                            { "refresh_tooltip", "Atualizar componentes instalados" },
                            { "install_button", "📥 Instalar" },
                            { "uninstall_button", "🗑️ Desinstalar" },
                            { "none", "Nenhum componente instalado" },
                            { "installed_default", "Instalado" },
                            { "error_loading", "Erro ao carregar componentes" },
                            { "version_na", "N/A" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Serviços" },
                            { "refresh_tooltip", "Atualizar serviços" },
                            { "start_all", "▶️ Iniciar" },
                            { "stop_all", "⏹️ Parar" },
                            { "restart_all", "🔄 Reiniciar" },
                            { "none", "Nenhum serviço encontrado" },
                            { "loading", "Carregando serviços..." },
                            { "status", new Dictionary<string, object>
                            {
                                { "active", "Ativo" },
                                { "stopped", "Parado" },
                                { "na", "N/A" }
                            }
                            }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "📥 Instalação Rápida" },
                            { "select_component", "Selecione um componente para instalar." },
                            { "installing", "Instalando {0}..." },
                            { "success", "{0} instalado com sucesso!" },
                            { "error", "Erro ao instalar {0}: {1}" },
                            { "install_button", "📥 Instalar" },
                            { "go_to_install", "Ir para Instalar" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "updating_components", "Atualizando componentes..." },
                        { "components_updated", "Componentes atualizados!" },
                        { "error_updating_components", "Erro ao atualizar componentes: {0}" },
                        { "updating_services", "Atualizando serviços..." },
                        { "services_updated", "Serviços atualizados!" },
                        { "error_updating_services", "Erro ao atualizar serviços: {0}" },
                        { "starting_all_services", "Iniciando todos os serviços..." },
                        { "all_services_started", "Todos os serviços foram iniciados!" },
                        { "error_starting_services", "Erro ao iniciar serviços: {0}" },
                        { "stopping_all_services", "Parando todos os serviços..." },
                        { "all_services_stopped", "Todos os serviços foram parados!" },
                        { "error_stopping_services", "Erro ao parar serviços: {0}" },
                        { "restarting_all_services", "Reiniciando todos os serviços..." },
                        { "all_services_restarted", "Todos os serviços foram reiniciados!" },
                        { "error_restarting_services", "Erro ao reiniciar serviços: {0}" },
                        { "select_component_install", "Selecione um componente para instalar." },
                        { "installing_component", "Instalando {0}..." },
                        { "component_installed", "{0} instalado com sucesso!" },
                        { "error_installing_component", "Erro ao instalar {0}: {1}" },
                        { "opening_shell", "🚀 Abrindo shell interativo para {0} v{1}" },
                        { "executing", "🚀 Executando {0} v{1}" },
                        { "no_executable", "❌ Nenhum executável encontrado para {0} v{1}" },
                        { "version_folder_not_found", "❌ Pasta da versão não encontrada: {0}" },
                        { "component_not_executable", "❌ Componente {0} não é executável" },
                        { "error_executing", "❌ Erro ao executar {0} v{1}: {2}" },
                        { "error_updating_component_data", "Erro ao atualizar dados dos componentes: {0}" },
                        { "error_updating_service_data", "Erro ao atualizar dados dos serviços: {0}" }
                    }
                    }
                }
                },
                { "installed_tab", new Dictionary<string, object>
                {
                    { "title", "Ferramentas Instaladas" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "tool", "Ferramenta" },
                        { "versions", "Versões Instaladas" },
                        { "status", "Status" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Atualizar Lista" }
                    }
                    },
                    { "info", "Use as abas 'Instalar' e 'Desinstalar' para gerenciar as ferramentas" },
                    { "loading", "Carregando componentes instalados..." },
                    { "loaded", "Carregados {0} componentes" },
                    { "error", "Erro ao carregar componentes: {0}" }
                }
                },
                { "install_tab", new Dictionary<string, object>
                {
                    { "title", "Instalar Nova Ferramenta" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Selecione a ferramenta:" },
                        { "select_version", "Selecione a versão (deixe vazio para a mais recente):" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "install", "📥 Instalar" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Selecione um componente para instalar." },
                        { "installing", "Instalando {0}..." },
                        { "success", "{0} instalado com sucesso!" },
                        { "error", "Erro ao instalar {0}" },
                        { "loading_versions", "Carregando versões de {0}..." },
                        { "versions_loaded", "{0} versões carregadas para {1}" },
                        { "versions_error", "Erro ao carregar versões: {0}" }
                    }
                    }
                }
                },
                { "uninstall_tab", new Dictionary<string, object>
                {
                    { "title", "Desinstalar Ferramenta" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "select_tool", "Selecione a ferramenta:" },
                        { "select_version", "Selecione a versão:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "uninstall", "🗑️ Desinstalar" },
                        { "refresh", "🔄 Atualizar Lista" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_component", "Selecione um componente para desinstalar." },
                        { "select_version", "Selecione uma versão para desinstalar." },
                        { "confirm", "Tem certeza que deseja desinstalar {0}?" },
                        { "uninstalling", "Desinstalando {0}..." },
                        { "success", "{0} desinstalado com sucesso!" },
                        { "error", "Erro ao desinstalar {0}" },
                        { "no_versions", "{0} não possui versões instaladas." },
                        { "not_installed", "{0} não está instalado" },
                        { "loading_components", "Carregando componentes instalados..." },
                        { "loading_versions", "Carregando versões instaladas de {0}..." },
                        { "versions_loaded", "Versões carregadas para {0}" },
                        { "versions_error", "Erro ao carregar versões para desinstalação: {0}" },
                        { "components_available", "{0} componentes disponíveis para desinstalação" },
                        { "reloading", "Recarregando lista de componentes instalados..." }
                    }
                    },
                    { "warning", "Atenção: Esta ação não pode ser desfeita!" },
                    { "status", new Dictionary<string, object>
                    {
                        { "uninstalling", "Desinstalando {0}..." },
                        { "success", "{0} desinstalado com sucesso!" },
                        { "error", "❌ Erro ao desinstalar {0}: {1}" },
                        { "error_short", "Erro ao desinstalar {0}" },
                        { "loading_versions", "Carregando versões instaladas de {0}..." },
                        { "versions_loaded", "Versões carregadas para {0}" },
                        { "not_installed", "{0} não está instalado" },
                        { "error_loading_versions", "Erro ao carregar versões para desinstalação: {0}" },
                        { "loading_components", "Carregando componentes instalados..." },
                        { "components_count", "{0} componentes disponíveis para desinstalação" },
                        { "reloading", "Recarregando lista de componentes instalados..." },
                        { "error_loading_components", "Erro ao carregar componentes: {0}" }
                    }
                    }
                }
                },
                { "services_tab", new Dictionary<string, object>
                {
                    { "title", "Gerenciamento de Serviços" },
                    { "headers", new Dictionary<string, object>
                    {
                        { "component", "Componente" },
                        { "version", "Versão" },
                        { "status", "Status" },
                        { "pid", "PID" },
                        { "copy_pid", "Copiar PID" },
                        { "actions", "Ações" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "refresh", "🔄 Atualizar" },
                        { "start_all", "▶️ Iniciar Todos" },
                        { "stop_all", "⏹️ Parar Todos" },
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
                        { "stop", "Parar" },
                        { "restart", "Reiniciar" },
                        { "copy_pid", "Copiar PID" }
                    }
                    },
                    { "status", new Dictionary<string, object>
                    {
                        { "running", "Em execução" },
                        { "stopped", "Parado" }
                    }
                    },
                    { "types", new Dictionary<string, object>
                    {
                        { "php_fpm", "PHP-FPM" },
                        { "web_server", "Servidor Web" },
                        { "database", "Banco de Dados" },
                        { "search_engine", "Motor de Busca" },
                        { "service", "Serviço" },
                        { "fastcgi", "FastCGI" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "loading", "Carregando serviços..." },
                        { "loaded", "{0} serviços carregados" },
                        { "error", "Erro ao carregar serviços: {0}" },
                        { "starting", "Iniciando {0} versão {1}..." },
                        { "started", "{0} iniciado com sucesso" },
                        { "stopping", "Parando {0} versão {1}..." },
                        { "stopped", "{0} parado com sucesso" },
                        { "restarting", "Reiniciando {0} versão {1}..." },
                        { "restarted", "{0} reiniciado com sucesso" },
                        { "starting_all", "Iniciando todos os serviços..." },
                        { "started_all", "Todos os serviços iniciados" },
                        { "stopping_all", "Parando todos os serviços..." },
                        { "stopped_all", "Todos os serviços parados" },
                        { "restarting_all", "Reiniciando todos os serviços..." },
                        { "restarted_all", "Todos os serviços reiniciados" },
                        { "pid_copied", "PID {0} copiado para a área de transferência" },
                        { "no_pid", "Serviço não está em execução, não há PID para copiar." },
                        { "error_copy_pid", "Erro ao copiar PID: {0}" },
                        { "error_start", "Erro ao iniciar serviço: {0}" },
                        { "error_stop", "Erro ao parar serviço: {0}" },
                        { "error_restart", "Erro ao reiniciar serviço: {0}" },
                        { "error_start_all", "Erro ao iniciar todos os serviços: {0}" },
                        { "error_stop_all", "Erro ao parar todos os serviços: {0}" },
                        { "error_restart_all", "Erro ao reiniciar todos os serviços: {0}" }
                    }
                    },
                    { "path_manager", new Dictionary<string, object>
                    {
                        { "not_initialized", "⚠️ PathManager não foi inicializado - PATH não foi atualizado" }
                    }
                    },
                    { "debug", new Dictionary<string, object>
                    {
                        { "processes_found", "Processos encontrados para debug: {0}" },
                        { "process_info", "  - {0} (PID: {1}) - Path: {2}" },
                        { "process_error", "  - {0} (PID: {1}) - Path: Erro ao acessar ({2})" },
                        { "found_service_components", "Encontrados {0} componentes de serviço" },
                        { "component_dir_not_found", "Diretório do componente {0} não encontrado: {1}" },
                        { "component_versions_found", "Componente {0}: {1} versões encontradas: {2}" },
                        { "checking_component_version", "Verificando {0} versão {1}" },
                        { "service_process_found", "  - Processo {0} encontrado: {1} (PID: {2}) - Path: {3}" },
                        { "service_running", "{0} {1} está executando com PIDs: {2}" },
                        { "service_not_running", "{0} {1} não está executando" },
                        { "no_service_pattern", "Nenhum padrão de serviço definido para {0}" },
                        { "component_check_error", "Erro ao verificar processos {0}: {1}" },
                        { "php_dirs_found", "Encontrados {0} diretórios PHP: {1}" },
                        { "checking_php_version", "Verificando PHP versão {0} no diretório {1}" },
                        { "php_process_found", "  - Processo PHP encontrado: {0} (PID: {1}) - Path: {2}" },
                        { "process_check_error", "  - Erro ao verificar processo {0}: {1}" },
                        { "php_running", "PHP {0} está executando com PIDs: {1}" },
                        { "php_not_running", "PHP {0} não está executando" },
                        { "php_check_error", "Erro ao verificar processos PHP: {0}" },
                        { "nginx_dirs_found", "Encontrados {0} diretórios Nginx: {1}" },
                        { "checking_nginx_version", "Verificando Nginx versão {0} no diretório {1}" },
                        { "nginx_process_found", "  - Processo Nginx encontrado: {0} (PID: {1}) - Path: {2}" },
                        { "nginx_running", "Nginx {0} está executando com PID: {1}" },
                        { "nginx_not_running", "Nginx {0} não está executando" },
                        { "nginx_check_error", "Erro ao verificar processos Nginx: {0}" },
                        { "load_services_error", "Erro ao carregar serviços na GUI: {0}" },
                        { "start_all_services_error", "Erro ao iniciar todos os serviços na GUI: {0}" },
                        { "stop_all_services_error", "Erro ao parar todos os serviços na GUI: {0}" },
                        { "restart_all_services_error", "Erro ao reiniciar todos os serviços na GUI: {0}" }
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
                            { "title", "Instalados" },
                            { "description", "Ferramentas instaladas" }
                        }
                        },
                        { "install", new Dictionary<string, object>
                        {
                            { "title", "Instalar" },
                            { "description", "Instalar novos componentes" }
                        }
                        },
                        { "uninstall", new Dictionary<string, object>
                        {
                            { "title", "Desinstalar" },
                            { "description", "Remover componentes" }
                        }
                        },
                        { "services", new Dictionary<string, object>
                        {
                            { "title", "Serviços" },
                            { "description", "Controle de serviços" }
                        }
                        },
                        { "config", new Dictionary<string, object>
                        {
                            { "title", "Configurações" },
                            { "description", "Configurações do sistema" }
                        }
                        },
                        { "sites", new Dictionary<string, object>
                        {
                            { "title", "Sites" },
                            { "description", "Gerenciar sites Nginx" }
                        }
                        },
                        { "utilities", new Dictionary<string, object>
                        {
                            { "title", "Utilitários" },
                            { "description", "Ferramentas e console" }
                        }
                        }
                    }
                    }
                }
                },
                { "config_tab", new Dictionary<string, object>
                {
                    { "title", "Configurações" },
                    { "path", new Dictionary<string, object>
                    {
                        { "title", "Gerenciamento do PATH" },
                        { "description", "Adicionar ferramentas ao PATH do sistema" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "add", "➕ Adicionar ao PATH" },
                            { "remove", "➖ Remover do PATH" }
                        }
                        }
                    }
                    },
                    { "directories", new Dictionary<string, object>
                    {
                        { "title", "Diretórios" },
                        { "buttons", new Dictionary<string, object>
                        {
                            { "devstack_manager", "📂 DevStack Manager" },
                            { "tools", "📂 Ferramentas" }
                        }
                        }
                    }
                    },
                    { "languages", new Dictionary<string, object>
                    {
                        { "title", "Idiomas" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_language", "Idioma da Interface" }
                        }
                        },
                        { "messages", new Dictionary<string, object>
                        {
                            { "language_changed", "Idioma alterado para {0}" }
                        }
                        }
                    }
                    },
                    { "themes", new Dictionary<string, object>
                    {
                        { "title", "Temas" },
                        { "labels", new Dictionary<string, object>
                        {
                            { "interface_theme", "Tema da Interface" }
                        }
                        }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "path_updated", "PATH atualizado com sucesso" },
                        { "path_update_error", "Erro ao atualizar PATH" },
                        { "path_cleaned", "PATH limpo com sucesso" },
                        { "path_listed", "PATH listado" },
                        { "path_error", "Erro ao adicionar ao PATH: {0}" },
                        { "path_remove_error", "Erro ao remover do PATH: {0}" },
                        { "path_clean_error", "Erro ao limpar PATH" },
                        { "path_list_error", "Erro ao listar PATH: {0}" },
                        { "exe_folder_opened", "Pasta do executável aberta" },
                        { "exe_folder_not_found", "Não foi possível localizar a pasta do executável." },
                        { "exe_folder_error", "Erro ao abrir pasta do executável: {0}" },
                        { "tools_folder_opened", "Pasta de ferramentas aberta" },
                        { "tools_folder_not_found", "Não foi possível localizar a pasta de ferramentas." },
                        { "tools_folder_error", "Erro ao abrir pasta de ferramentas: {0}" }
                    }
                    }
                }
                },
                { "sites_tab", new Dictionary<string, object>
                {
                    { "title", "Criar Configuração de Site Nginx" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "domain", "Domínio do site:" },
                        { "root_directory", "Diretório raiz:" },
                        { "php_upstream", "PHP Upstream:" },
                        { "nginx_version", "Versão Nginx:" },
                        { "ssl_domain", "Domínio para SSL:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "browse", "📁 Procurar" },
                        { "create_site", "🌐 Criar Configuração de Site" },
                        { "generate_ssl", "🔒 Gerar Certificado SSL" }
                    }
                    },
                    { "ssl", new Dictionary<string, object>
                    {
                        { "title", "Certificados SSL" },
                        { "generate_ssl", "Gerar SSL" }
                    }
                    },
                    { "messages", new Dictionary<string, object>
                    {
                        { "select_folder", "Selecionar Pasta do Site" },
                        { "creating_site", "Criando configuração para o site {0}..." },
                        { "site_created", "Site {0} criado" },
                        { "site_error", "Erro ao criar site {0}: {1}" },
                        { "site_config_error", "Erro ao criar configuração do site: {0}" },
                        { "enter_domain", "Digite um domínio para o site." },
                        { "enter_root", "Digite um diretório raiz para o site." },
                        { "select_php", "Selecione uma versão do PHP para o site." },
                        { "select_nginx", "Selecione uma versão do Nginx para o site." },
                        { "enter_ssl_domain", "Digite um domínio para gerar o certificado SSL." },
                        { "domain_not_exists", "O domínio '{0}' não existe ou não está resolvendo para nenhum IP." },
                        { "generating_ssl", "Gerando certificado SSL para {0}..." },
                        { "ssl_generated", "Processo de geração de SSL para {0} finalizado." },
                        { "ssl_error", "Erro ao gerar certificado SSL: {0}" },
                        { "restarting_nginx", "Reiniciando serviços do Nginx..." },
                        { "nginx_restarted", "Nginx v{0} reiniciado com sucesso" },
                        { "nginx_restart_error", "Erro ao reiniciar Nginx v{0}: {1}" },
                        { "nginx_restart_general_error", "Erro ao reiniciar Nginx: {0}" },
                        { "ssl_generation_completed", "Processo de geração de SSL para {0} finalizado." },
                        { "ssl_generation_error", "❌ Erro ao gerar certificado SSL: {0}" },
                        { "ssl_generation_error_status", "Erro ao gerar SSL para {0}" },
                        { "ssl_generation_error_dialog", "Erro ao gerar certificado SSL: {0}" },
                        { "no_nginx_restarted", "ℹ️ Nenhuma versão do Nginx foi reiniciada (podem não estar em execução)" },
                        { "no_nginx_found", "❌ Nenhuma versão do Nginx instalada encontrada" }
                    }
                    },
                    { "info", "Os arquivos de configuração serão criados automaticamente" }
                }
                },
                { "utilities_tab", new Dictionary<string, object>
                {
                    { "title", "Console DevStack - Execute comandos diretamente" },
                    { "labels", new Dictionary<string, object>
                    {
                        { "command", "Comando:" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "execute", "▶️ Executar" },
                        { "clear", "❌" },
                        { "clear_tooltip", "Limpar Console" }
                    }
                    },
                    { "console_title", "Console DevStack - Execute comandos diretamente" },
                    { "command_label", "Comando:" },
                    { "execute_button", "▶️ Executar" },
                    { "clear_console_tooltip", "Limpar Console" },
                    { "status_button", "Status" },
                    { "installed_button", "Instalados" },
                    { "diagnostic_button", "Diagnóstico" },
                    { "test_button", "Testar" },
                    { "help_button", "Ajuda" },
                    { "console_header", "Console do DevStack Manager" },
                    { "available_commands", "Comandos disponíveis:" },
                    { "tip_message", "Dica: Digite comandos diretamente no campo acima ou use os botões rápidos" },
                    { "executing_command", "Executando: {0}" },
                    { "no_output", "(Comando executado, sem saída gerada)" },
                    { "devstack_not_found", "Erro: Não foi possível iniciar o processo DevStack.exe" },
                    { "error", "ERRO" },
                    { "console_cleared", "Console limpo.\n\n" },
                    { "empty_command", "Comando vazio" },
                    { "command_execution_error", "Erro ao executar comando: {0}" },
                    { "status", new Dictionary<string, object>
                    {
                        { "executing", "Executando: {0}" },
                        { "executed", "Comando executado" },
                        { "error", "Erro ao executar comando" },
                        { "cleared", "Console limpo" }
                    }
                    }
                }
                },
                { "console", new Dictionary<string, object>
                {
                    { "titles", new Dictionary<string, object>
                    {
                        { "install", "Saída do Console - Instalar" },
                        { "uninstall", "Saída do Console - Desinstalar" },
                        { "sites", "Saída do Console - Sites" },
                        { "config", "Saída do Console - Configurações" },
                        { "utilities", "Saída do Console" }
                    }
                    },
                    { "buttons", new Dictionary<string, object>
                    {
                        { "clear", "🗑️ Limpar Console" }
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
                        { "warning", "Aviso" },
                        { "error", "Erro" },
                        { "info", "Informação" },
                        { "confirmation", "Confirmação" }
                    }
                    }
                }
                },
                { "status_bar", new Dictionary<string, object>
                {
                    { "refresh_tooltip", "Atualizar status" },
                    { "updating", "Atualizando..." },
                    { "updated", "Status atualizado" }
                }
                }
            };
        }

        public Dictionary<string, object> GetInstallerTranslations()
        {
            return new Dictionary<string, object>
            {
                { "window_title", "DevStack Manager v{0} - Assistente de Instalação" },
                { "dialogs", new Dictionary<string, object>
                {
                    { "cancel_title", "Cancelar Instalação" },
                    { "cancel_message", "Tem certeza que deseja cancelar a instalação?" },
                    { "installation_error_title", "Erro" },
                    { "installation_error_message", "Falha na instalação: {0}" },
                    { "folder_dialog_title", "Selecione a pasta de instalação" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "Bem-vindo ao DevStack Manager" },
                    { "description", "Este assistente irá guiá-lo pela instalação do DevStack Manager em seu computador." },
                    { "app_name", "DevStack Manager" },
                    { "version", "Versão {0}" },
                    { "app_description", "DevStack Manager é uma ferramenta abrangente de gerenciamento de ambiente de desenvolvimento que ajuda você a instalar, configurar e gerenciar várias ferramentas e serviços de desenvolvimento.\n\nClique em 'Próximo' para continuar com a instalação." },
                    { "language_label", "Idioma da instalação:" }
                }
                },
                { "license", new Dictionary<string, object>
                {
                    { "title", "Contrato de Licença" },
                    { "description", "Por favor, leia o seguinte contrato de licença cuidadosamente." },
                    { "label", "Por favor, leia e aceite o contrato de licença:" },
                    { "text", "Licença MIT\n\nCopyright (c) 2025 DevStackManager\n\nPermissão é concedida, gratuitamente, a qualquer pessoa que obtenha uma cópia\ndeste software e arquivos de documentação associados (o \"Software\"), para negociar\nno Software sem restrição, incluindo, sem limitação, os direitos\nde usar, copiar, modificar, mesclar, publicar, distribuir, sublicenciar e/ou vender\ncópias do Software, e permitir que as pessoas a quem o Software é\nfornecido o façam, sujeito às seguintes condições:\n\nO aviso de copyright acima e este aviso de permissão devem ser incluídos em todas\nas cópias ou partes substanciais do Software.\n\nO SOFTWARE É FORNECIDO \"COMO ESTÁ\", SEM GARANTIA DE QUALQUER TIPO, EXPRESSA OU\nIMPLÍCITA, INCLUINDO, MAS NÃO SE LIMITANDO ÀS GARANTIAS DE COMERCIALIZAÇÃO,\nADEQUAÇÃO A UM PROPÓSITO ESPECÍFICO E NÃO VIOLAÇÃO. EM NENHUM CASO OS\nAUTORES OU DETENTORES DE DIREITOS AUTORAIS SERÃO RESPONSÁVEIS POR QUALQUER REIVINDICAÇÃO, DANOS OU OUTRA\nRESPONSABILIDADE, SEJA EM AÇÃO DE CONTRATO, DELITO OU DE OUTRA FORMA, DECORRENTE DE,\nFORA DE OU EM CONEXÃO COM O SOFTWARE OU O USO OU OUTRAS NEGOCIAÇÕES NO\nSOFTWARE." }
                }
                },
                { "installation_path", new Dictionary<string, object>
                {
                    { "title", "Escolha o Local de Instalação" },
                    { "description", "Escolha a pasta onde o DevStack Manager será instalado." },
                    { "label", "Pasta de Destino:" },
                    { "browser", "Procurar..." },
                    { "space_required", "Espaço necessário: {0} MB" },
                    { "space_available", "Espaço disponível: {0}" },
                    { "info", "O DevStack Manager será instalado nesta pasta junto com todos os seus componentes e configurações." }
                }
                },
                { "components", new Dictionary<string, object>
                {
                    { "title", "Selecionar Opções Adicionais" },
                    { "description", "Escolha as opções adicionais para sua instalação do DevStack Manager." },
                    { "label", "Opções Adicionais:" },
                    { "desktop_shortcuts", "🖥️ Criar atalhos na área de trabalho" },
                    { "start_menu_shortcuts", "📂 Criar atalhos no Menu Iniciar" },
                    { "add_to_path", "⚡ Adicionar DevStack ao PATH do sistema (recomendado)" },
                    { "path_info", "Adicionar ao PATH permite usar comandos do DevStack diretamente no terminal de qualquer local." }
                }
                },
                { "ready_to_install", new Dictionary<string, object>
                {
                    { "title", "Pronto para Instalar" },
                    { "description", "O assistente está pronto para iniciar a instalação. Revise suas configurações abaixo." },
                    { "summary_label", "Resumo da Instalação:" },
                    { "destination", "Pasta de destino:" },
                    { "components_header", "Componentes a instalar:" },
                    { "cli_component", "• DevStack CLI (Interface de Linha de Comando)" },
                    { "gui_component", "• DevStack GUI (Interface Gráfica)" },
                    { "uninstaller_component", "• Desinstalador do DevStack" },
                    { "config_component", "• Arquivos de configuração e componentes" },
                    { "options_header", "Opções adicionais:" },
                    { "create_desktop", "• Criar atalhos na área de trabalho" },
                    { "create_start_menu", "• Criar atalhos no Menu Iniciar" },
                    { "add_path", "• Adicionar ao PATH do sistema" },
                    { "space_required_summary", "Espaço necessário: {0} MB" }
                }
                },
                { "installing", new Dictionary<string, object>
                {
                    { "title", "Instalando DevStack Manager" },
                    { "description", "Por favor, aguarde enquanto o DevStack Manager está sendo instalado..." },
                    { "preparing", "Preparando instalação..." },
                    { "extracting", "Extraindo arquivos de instalação embarcados..." },
                    { "creating_directory", "Criando diretório de instalação..." },
                    { "installing_files", "Instalando arquivos do DevStack..." },
                    { "registering", "Registrando instalação..." },
                    { "creating_desktop", "Criando atalhos na área de trabalho..." },
                    { "creating_start_menu", "Criando atalhos no Menu Iniciar..." },
                    { "adding_path", "Adicionando ao PATH do sistema..." },
                    { "completed", "Instalação concluída com sucesso!" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Instalação Concluída" },
                    { "description", "O DevStack Manager foi instalado com sucesso no seu computador." },
                    { "success_icon", "✅" },
                    { "success_title", "Instalação Concluída com Sucesso!" },
                    { "success_message", "O DevStack Manager foi instalado com sucesso. Agora você pode usar a aplicação para gerenciar seu ambiente de desenvolvimento." },
                    { "install_location", "Local da Instalação:" },
                    { "launch_now", "🚀 Executar DevStack Manager agora" }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Iniciando processo de instalação" },
                    { "extracted", "Arquivos embarcados extraídos com sucesso" },
                    { "creating_dir", "Criando diretório: {0}" },
                    { "installing", "Instalando arquivos da aplicação" },
                    { "registering", "Registrando instalação no Windows" },
                    { "desktop_shortcuts", "Criando atalhos na área de trabalho" },
                    { "start_menu_shortcuts", "Criando atalhos no Menu Iniciar" },
                    { "adding_path", "Adicionando DevStack ao PATH do sistema" },
                    { "path_added", "Adicionado ao PATH do usuário com sucesso" },
                    { "path_exists", "Já existe no PATH" },
                    { "completed_success", "Instalação concluída com sucesso!" },
                    { "cleanup", "Arquivos temporários limpos" },
                    { "cleanup_warning", "Aviso: Não foi possível excluir arquivo temporário: {0}" },
                    { "shortcuts_warning", "Aviso: Não foi possível criar atalhos na área de trabalho: {0}" },
                    { "start_menu_warning", "Aviso: Não foi possível criar atalhos no Menu Iniciar: {0}" },
                    { "path_warning", "Aviso: Não foi possível adicionar ao PATH: {0}" }
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
                    { "uninstall_error_title", "Erro na Desinstalação" },
                    { "uninstall_error_message", "Erro durante a desinstalação: {0}" },
                    { "startup_error_title", "Erro no Desinstalador DevStack" },
                    { "startup_error_message", "Erro ao iniciar o desinstalador: {0}\n\nDetalhes: {1}" },
                    { "initialization_error_title", "Erro de Inicialização" },
                    { "initialization_error_message", "Erro ao inicializar a janela do desinstalador: {0}" },
                    { "cancel_title", "Cancelar Desinstalação" },
                    { "cancel_message", "Tem certeza que deseja cancelar a desinstalação?" }
                }
                },
                { "welcome", new Dictionary<string, object>
                {
                    { "title", "Desinstalador DevStack" },
                    { "description", "Este assistente irá remover o DevStack do seu computador" },
                    { "app_name", "Desinstalador DevStack" },
                    { "version", "Versão {0}" },
                    { "app_description", "Este assistente irá guiá-lo através do processo de remoção completa do DevStack do seu sistema." },
                    { "language_label", "Idioma:" }
                }
                },
                { "confirmation", new Dictionary<string, object>
                {
                    { "title", "Confirmação de Desinstalação" },
                    { "description", "Por favor, confirme que deseja prosseguir com a remoção do DevStack" },
                    { "warning_title", "⚠️ Atenção - Esta ação não pode ser desfeita" },
                    { "warning_text", "A desinstalação irá remover completamente o DevStack do seu sistema, incluindo:" },
                    { "items", new Dictionary<string, object>
                    {
                        { "program_files", "• Todos os arquivos de programa" },
                        { "user_data", "• Configurações e dados do usuário" },
                        { "shortcuts", "• Atalhos da área de trabalho e menu iniciar" },
                        { "registry", "• Entradas do registro do Windows" },
                        { "services", "• Serviços e processos relacionados" },
                        { "path_variables", "• Variáveis de ambiente PATH" }
                    }
                    },
                    { "install_found", "📁 Pasta de instalação encontrada:" },
                    { "install_not_found", "❌ Pasta de instalação não encontrada automaticamente" },
                    { "install_not_found_desc", "O DevStack pode não estar instalado corretamente ou já ter sido removido. A desinstalação irá apenas limpar registros e atalhos remanescentes." },
                    { "space_to_free", "📊 Espaço que será liberado: {0}" }
                }
                },
                { "uninstall_options", new Dictionary<string, object>
                {
                    { "title", "Opções de Desinstalação" },
                    { "description", "Escolha o que deseja remover durante a desinstalação" },
                    { "label", "Selecione os componentes para remover:" },
                    { "user_data", "🗂️ Remover dados e configurações do usuário" },
                    { "user_data_desc", "Inclui configurações, logs e arquivos de dados salvos pelo DevStack" },
                    { "registry", "🔧 Remover entradas do registro" },
                    { "registry_desc", "Remove chaves de registro e informações de instalação" },
                    { "shortcuts", "🔗 Remover atalhos" },
                    { "shortcuts_desc", "Remove atalhos da área de trabalho e menu iniciar" },
                    { "path", "🛤️ Remover do PATH do sistema" },
                    { "path_desc", "Remove o caminho do DevStack das variáveis de ambiente" },
                    { "info", "Recomendamos manter todas as opções selecionadas para uma remoção completa do sistema." }
                }
                },
                { "ready_to_uninstall", new Dictionary<string, object>
                {
                    { "title", "Pronto para Desinstalar" },
                    { "description", "Revise as configurações e clique em Desinstalar para prosseguir" },
                    { "summary_label", "Resumo da desinstalação:" },
                    { "components_header", "COMPONENTES A SEREM REMOVIDOS:" },
                    { "installation_location", "📁 Local da instalação:" },
                    { "not_found", "Não encontrado" },
                    { "program_components", "🗂️ Componentes do programa:" },
                    { "executables", "  • Arquivos executáveis (DevStack.exe, DevStackGUI.exe)" },
                    { "libraries", "  • Bibliotecas e dependências" },
                    { "config_files", "  • Arquivos de configuração" },
                    { "documentation", "  • Documentação e recursos" },
                    { "selected_options", "OPÇÕES SELECIONADAS:" },
                    { "user_data_selected", "✓ Dados do usuário serão removidos" },
                    { "user_data_preserved", "✗ Dados do usuário serão preservados" },
                    { "registry_selected", "✓ Entradas do registro serão removidas" },
                    { "registry_preserved", "✗ Entradas do registro serão preservadas" },
                    { "shortcuts_selected", "✓ Atalhos serão removidos" },
                    { "shortcuts_preserved", "✗ Atalhos serão preservados" },
                    { "path_selected", "✓ Será removido do PATH do sistema" },
                    { "path_preserved", "✗ Permanecerá no PATH do sistema" },
                    { "space_to_free", "💾 Espaço a ser liberado: {0}" }
                }
                },
                { "uninstalling", new Dictionary<string, object>
                {
                    { "title", "Desinstalando" },
                    { "description", "Por favor aguarde enquanto o DevStack é removido do seu sistema" },
                    { "preparing", "Preparando desinstalação..." },
                    { "stopping_services", "Parando serviços..." },
                    { "removing_shortcuts", "Removendo atalhos..." },
                    { "cleaning_registry", "Limpando registro..." },
                    { "removing_path", "Removendo do PATH..." },
                    { "removing_files", "Removendo arquivos..." },
                    { "removing_user_data", "Removendo dados do usuário..." },
                    { "finalizing", "Finalizando..." },
                    { "completed", "Desinstalação concluída!" }
                }
                },
                { "finished", new Dictionary<string, object>
                {
                    { "title", "Desinstalação Concluída" },
                    { "description", "O DevStack foi removido com sucesso do seu sistema" },
                    { "success_icon", "✅" },
                    { "success_title", "Desinstalação Concluída!" },
                    { "success_message", "O DevStack foi removido com sucesso do seu sistema. Todos os componentes selecionados foram limpos." },
                    { "summary_title", "📊 Resumo da desinstalação:" },
                    { "files_removed", "• Arquivos removidos de: {0}" },
                    { "user_data_removed", "• Dados do usuário removidos" },
                    { "registry_cleaned", "• Entradas do registro limpas" },
                    { "shortcuts_removed", "• Atalhos removidos" },
                    { "path_removed", "• Removido do PATH do sistema" },
                    { "system_clean", "O sistema está agora livre do DevStack." }
                }
                },
                { "log_messages", new Dictionary<string, object>
                {
                    { "starting", "Iniciando processo de desinstalação" },
                    { "stopping_services", "Parando serviços do DevStack..." },
                    { "process_stopped", "Processo {0} finalizado" },
                    { "process_stop_warning", "Aviso: Não foi possível finalizar {0}: {1}" },
                    { "stop_services_error", "Erro ao parar serviços: {0}" },
                    { "removing_shortcuts", "Removendo atalhos..." },
                    { "shortcut_removed", "Atalho removido: {0}" },
                    { "start_menu_removed", "Pasta do menu iniciar removida: {0}" },
                    { "shortcuts_error", "Erro ao remover atalhos: {0}" },
                    { "cleaning_registry", "Limpando entradas do registro..." },
                    { "user_registry_removed", "Entradas do registro do usuário removidas" },
                    { "machine_registry_removed", "Entradas do registro da máquina removidas" },
                    { "uninstall_registry_removed", "Entrada de programas e recursos removida" },
                    { "registry_error", "Erro ao limpar registro: {0}" },
                    { "removing_path", "Removendo do PATH do sistema..." },
                    { "user_path_removed", "Removido do PATH do usuário" },
                    { "system_path_removed", "Removido do PATH do sistema" },
                    { "system_path_warning", "Aviso: Não foi possível remover do PATH do sistema (requer privilégios de administrador)" },
                    { "path_error", "Erro ao remover do PATH: {0}" },
                    { "removing_files", "Removendo arquivos de {0}..." },
                    { "install_not_found", "Pasta de instalação não encontrada" },
                    { "files_removed_count", "{0} arquivos removidos" },
                    { "dirs_removed_count", "{0} pastas vazias removidas" },
                    { "file_remove_warning", "Aviso: Não foi possível remover {0}: {1}" },
                    { "files_error", "Erro ao remover arquivos: {0}" },
                    { "removing_user_data", "Removendo dados do usuário..." },
                    { "user_data_removed", "Dados do usuário removidos: {0}" },
                    { "user_data_error", "Erro ao remover dados do usuário: {0}" },
                    { "self_deletion_scheduled", "Agendada remoção automática do desinstalador" },
                    { "self_deletion_warning", "Aviso: Não foi possível agendar auto-remoção: {0}" },
                    { "uninstall_success", "Desinstalação concluída com sucesso!" }
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
