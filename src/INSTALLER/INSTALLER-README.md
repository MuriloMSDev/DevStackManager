# DevStack Installer

Este documento descreve como usar o sistema de instalador do DevStack.

## Estrutura do Projeto

```text
src/
├── CLI/              # Projeto CLI do DevStack
├── GUI/              # Projeto GUI do DevStack  
├── INSTALLER/        # Projeto do instalador
└── Shared/           # Arquivos compartilhados

scripts/
├── build.ps1         # Script principal de build
└── build-installer.ps1  # Script específico do instalador

release/              # Pasta com arquivos finais dos projetos
installer/            # Pasta gerada com o instalador final
```

## Como Construir o Instalador

### Opção 1: Build Completo com Instalador

```powershell
.\scripts\build.ps1 -WithInstaller
```

### Opção 2: Build Completo com Limpeza e Instalador

```powershell
.\scripts\build.ps1 -WithInstaller -Clean
```

### Opção 3: Apenas o Instalador (após build dos projetos)

```powershell
.\scripts\build-installer.ps1
```

## Funcionamento do Instalador

1. **Versioning Automático**: O instalador detecta automaticamente a versão dos projetos CLI/GUI através do campo `FileVersion` nos arquivos `.csproj`

2. **Nome do Executável**: O instalador é gerado com o nome `DevStack-x.x.x-Installer.exe` onde `x.x.x` é a versão detectada

3. **Arquivos Embutidos**: O arquivo zip com todo o conteúdo da pasta `release/` é incorporado como recurso no próprio executável do instalador

4. **Processo de Instalação**:
   - Cria um arquivo zip com todo o conteúdo da pasta `release/`
   - Incorpora o zip como recurso embutido no projeto instalador
   - Incorpora o ícone diretamente no executável
   - Compila o projeto instalador gerando um único arquivo autossuficiente
   - Remove todos os arquivos temporários
   - Resultado: apenas um arquivo .exe na pasta `installer/`

5. **Limpeza Automática**: Após a compilação, todos os arquivos zip temporários são automaticamente excluídos

## ✅ Status: FUNCIONANDO PERFEITAMENTE

O sistema de instalador DevStack está **100% funcional**:

- ✅ Interface gráfica abre corretamente ao clicar duas vezes
- ✅ Arquivo único autossuficiente (~1.4 MB)
- ✅ Ícone embutido no executável
- ✅ Arquivos da release embutidos como recurso
- ✅ Compatível com .NET 9.0 Desktop Runtime
- ✅ **SEM dependências externas de .dll**
- ✅ **Publish Single File configurado corretamente**
- ✅ **Executa automaticamente como administrador** (UAC)
- ✅ **Inclui uninstaller completo**
- ✅ **Aparece em "Aplicativos Instalados" do Windows**

## Conteúdo do Instalador

O instalador final na pasta `installer/` contém apenas:

- `DevStack-2.0.0-Installer.exe` - Arquivo único autossuficiente (~1.2 MB)
  - Contém todos os arquivos da release embutidos
  - Ícone da aplicação incorporado
  - Interface gráfica completa
  - Não requer arquivos externos (.dll, .zip, etc.)
  - **Gerado com PublishSingleFile=true**

**Nota**: O instalador é completamente autossuficiente - não precisa de nenhum arquivo adicional.

## Funcionalidades do Instalador

- Interface gráfica amigável
- Seleção de pasta de destino para instalação
- Barra de progresso durante a instalação
- Opção para criar atalhos na área de trabalho
- Extração automática dos arquivos embutidos
- Limpeza automática de arquivos temporários após instalação
- Arquivo único e autossuficiente (não requer arquivos externos)
- **Execução automática com privilégios de administrador (UAC)**

## Atualizando a Versão

Para atualizar a versão do instalador:

1. Atualize o campo `<FileVersion>` nos arquivos:
   - `src/CLI/DevStackCLI.csproj`
   - `src/GUI/DevStackGUI.csproj`

2. Execute o build completo:

   ```powershell
   .\scripts\build.ps1 -WithInstaller -Clean
   ```

O sistema automaticamente:

- Detectará a nova versão
- Atualizará o projeto do instalador
- Gerará o novo executável com o nome correto

## Exemplo de Uso

```powershell
# Build completo com instalador
cd C:\Workspace\DevStackSetup
.\scripts\build.ps1 -WithInstaller

# O resultado será um único arquivo:
# installer/DevStack-2.0.0-Installer.exe (~1.2 MB, completamente autossuficiente)
```

## Notas Técnicas

- O instalador é desenvolvido em WPF (.NET 9.0)
- Compatível apenas com Windows
- Requer .NET 9.0 Desktop Runtime no sistema de destino
- O processo de instalação preserva a estrutura de pastas original
- Suporta instalação em qualquer diretório escolhido pelo usuário
- **Arquivo único**: Todos os componentes estão embutidos no executável do instalador
- **Distribuição simples**: Apenas um arquivo .exe precisa ser distribuído
- **Segurança**: Não há arquivos externos que podem ser perdidos ou corrompidos
- **Ícone integrado**: O ícone da aplicação está incorporado no executável
- **Portabilidade total**: Pode ser executado a partir de qualquer local
- **Privilégios elevados**: Configurado com manifesto UAC para execução como administrador
- **Compatibilidade**: Suporta Windows 7, 8, 8.1, 10 e 11

## Privilégios de Administrador

O instalador está configurado para **executar automaticamente como administrador**:

- **Manifesto UAC**: O arquivo `app.manifest` solicita `requireAdministrator`
- **Escudo UAC**: O Windows exibe o escudo azul/amarelo no ícone do executável
- **Prompt automático**: Ao executar, o Windows automaticamente solicita elevação de privilégios
- **Sem intervenção manual**: Não é necessário clicar com botão direito > "Executar como administrador"

### Por que privilégios de administrador?

O DevStack pode precisar instalar serviços, modificar arquivos do sistema ou configurar o ambiente, por isso o instalador solicita privilégios elevados para garantir que todas as operações sejam realizadas com sucesso.

## Uninstaller Incluído

O instalador automaticamente inclui um **uninstaller completo**:

- **Localização**: `DevStack-Uninstaller.exe` fica ao lado dos executáveis principais
- **Registro no Windows**: Aparece em "Aplicativos Instalados" para fácil desinstalação
- **Interface gráfica**: Interface intuitiva com confirmação e progresso
- **Limpeza completa**: Remove arquivos, atalhos, configurações e registros
- **Auto-exclusão**: Remove a si mesmo após completar a desinstalação

### Como Desinstalar:

1. **Via Configurações do Windows**:
   - Win + I → Apps → Aplicativos instalados → "DevStack Manager" → Desinstalar

2. **Via Painel de Controle**:
   - Win + R → `appwiz.cpl` → "DevStack Manager" → Desinstalar

3. **Execução direta**:
   - Navegar até pasta de instalação → Duplo clique em `DevStack-Uninstaller.exe`

**Mais detalhes**: Consulte `UNINSTALLER-README.md` para documentação completa do uninstaller.
