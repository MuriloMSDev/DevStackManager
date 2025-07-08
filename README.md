# DevStackManager

![DevStackManager Banner](https://img.shields.io/badge/PowerShell-DevStack-blue?style=for-the-badge&logo=powershell)

> **Ambiente de desenvolvimento local completo para Windows, com um só comando.**

---

## 🚀 O que é?
Scripts PowerShell para instalar, gerenciar e remover rapidamente um ambiente de desenvolvimento local moderno (PHP, Node.js, Python, Nginx, MySQL, Composer, phpMyAdmin, MongoDB, Redis, PostgreSQL, Docker, e mais) no Windows.

---

## ⚡ Como usar

### Compilar e usar (recomendado)

1. **Compile o projeto:**
```powershell
cd scripts
.\build.ps1
.\deploy.ps1
```

2. **Use os executáveis:**
```powershell
cd release
.\DevStack.exe [comando] [argumentos]    # CLI
.\DevStackGUI.exe                        # Interface gráfica
```

### Comandos Disponíveis (usando CLI)

| Comando                                                    | Descrição                                               |
|------------------------------------------------------------|--------------------------------------------------------|
| `.\DevStack.exe install <componente> [versão]`            | Instala uma ferramenta ou versão específica            |
| `.\DevStack.exe uninstall <componente> [versão]`          | Remove uma ferramenta ou versão específica             |
| `.\DevStack.exe list <componente\|--installed>`           | Lista versões disponíveis ou instaladas                |
| `.\DevStack.exe path [add\|remove\|list\|help]`           | Gerencia PATH das ferramentas instaladas               |
| `.\DevStack.exe status`                                    | Mostra status de todas as ferramentas                  |
| `.\DevStack.exe test`                                      | Testa todas as ferramentas instaladas                  |
| `.\DevStack.exe update <componente>`                       | Atualiza uma ferramenta para a última versão           |
| `.\DevStack.exe deps`                                      | Verifica dependências do sistema                       |
| `.\DevStack.exe alias <componente> <versão>`              | Cria um alias .bat para a versão da ferramenta         |
| `.\DevStack.exe global`                                    | Adiciona DevStack ao PATH e cria alias global          |
| `.\DevStack.exe self-update`                               | Atualiza o DevStackManager                              |
| `.\DevStack.exe clean`                                     | Remove logs e arquivos temporários                     |
| `.\DevStack.exe backup`                                    | Cria backup das configs e logs                         |
| `.\DevStack.exe logs`                                      | Exibe as últimas linhas do log                         |
| `.\DevStack.exe enable <serviço>`                          | Ativa um serviço do Windows                            |
| `.\DevStack.exe disable <serviço>`                         | Desativa um serviço do Windows                         |
| `.\DevStack.exe config`                                    | Abre o diretório de configuração                       |
| `.\DevStack.exe reset <componente>`                        | Remove e reinstala uma ferramenta                      |
| `.\DevStack.exe proxy [set <url>\|unset\|show]`           | Gerencia variáveis de proxy                            |
| `.\DevStack.exe ssl <domínio> [-openssl <versão>]`        | Gera certificado SSL autoassinado                      |
| `.\DevStack.exe db <mysql\|pgsql\|mongo> <comando> [args...]` | Gerencia bancos de dados básicos                   |
| `.\DevStack.exe service`                                   | Lista serviços DevStack (Windows)                      |
| `.\DevStack.exe doctor`                                    | Diagnóstico do ambiente DevStack                       |
| `.\DevStack.exe site <domínio> [opções]`                  | Cria configuração de site nginx                        |
| `.\DevStack.exe help`                                      | Exibe esta ajuda                                       |

---

## 🛠️ Troubleshooting

- Execute sempre como **administrador** para garantir permissões de PATH e hosts.
- Se um download falhar, tente novamente ou verifique sua conexão.
- O arquivo de log `C:\devstack\devstack.log` registra todas as operações.
- Se PATH não atualizar, reinicie o terminal.
- Se o alias 'devstack' não funcionar, feche e abra o PowerShell novamente ou rode `& $PROFILE` para recarregar o perfil.

---

## 🧩 Como estender

- Adicione novos scripts em PowerShell para outros stacks.
- Use as funções helper para evitar duplicação.
- Adicione testes automatizados com [Pester](https://pester.dev/).

---

## 🤝 Contribuição

- Siga o padrão modular do código (separação CLI/GUI/Shared).
- Adicione exemplos de uso ao README.
- Faça PRs com testes automatizados.
- Sugestões e issues são bem-vindos!

---

## 📂 Estrutura do Projeto

```text
DevStackSetup/
│   README.md
│
├───src/
│   ├───CLI/                   # Projeto da interface de linha de comando
│   │       DevStackCLI.csproj
│   │       Program.cs
│   │
│   ├───GUI/                   # Projeto da interface gráfica
│   │       DevStackGUI.csproj
│   │       Program.cs
│   │       Gui*.cs
│   │
│   └───Shared/               # Código compartilhado
│           DevStackConfig.cs
│           DataManager.cs
│           InstallManager.cs
│           UninstallManager.cs
│           ListManager.cs
│           PathManager.cs
│           ProcessManager.cs
│           DevStack.ico
│
├───scripts/
│   ├───build.ps1             # Script de compilação
│   └───deploy.ps1            # Script de deploy
│
└───release/                  # Pasta de distribuição
    ├───configs/              # Configurações (nginx, php, etc.)
    ├───DevStack.exe          # CLI compilado
    ├───DevStackGUI.exe       # GUI compilado
    └───...                   # Dependências e arquivos de runtime
```

---

## 💡 Dica

> Use `.\DevStack.exe doctor` para checar rapidamente se tudo está funcionando!

---

## Licença

MIT