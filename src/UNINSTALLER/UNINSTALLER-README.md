# DevStack Uninstaller

Este documento descreve o funcionamento do sistema de desinstalação do DevStack.

## ✅ Status: IMPLEMENTADO E FUNCIONAL

O uninstaller DevStack está **100% implementado**:

- ✅ Interface gráfica intuitiva com aviso claro sobre a desinstalação
- ✅ Arquivo único autossuficiente (~379 KB)
- ✅ Executa automaticamente como administrador (UAC)
- ✅ Detecta automaticamente o caminho de instalação
- ✅ Remove todos os arquivos, configurações e registros
- ✅ **Aparece em "Aplicativos Instalados" do Windows**
- ✅ Auto-exclusão após desinstalação completa

## Como Funciona

### 1. **Detecção da Instalação**
- Procura no registro do Windows (`HKCU\Software\DevStack`)
- Verifica o diretório atual do executável
- Localiza automaticamente os arquivos do DevStack

### 2. **Processo de Desinstalação**
1. **Para processos**: Finaliza DevStack.exe e DevStackGUI.exe em execução
2. **Remove atalhos**: Limpa atalhos da área de trabalho e menu iniciar
3. **Limpa registro**: Remove entradas do Registry do Windows
4. **Remove arquivos**: Deleta todos os arquivos exceto o próprio uninstaller
5. **Auto-exclusão**: Agenda a própria remoção via batch script

### 3. **Registro no Windows**
O instalador registra o programa em:
- `HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DevStack`
- `HKCU\Software\DevStack`

Isso faz com que o DevStack apareça em:
- **Configurações do Windows** > Apps > Aplicativos instalados
- **Painel de Controle** > Programas e Recursos

## Localização dos Arquivos

### **Durante a Instalação**
```
[Pasta de Instalação]/
├── DevStack.exe                    # Aplicação CLI
├── DevStackGUI.exe                 # Aplicação GUI  
├── DevStack-Uninstaller.exe        # Uninstaller ← AQUI
├── DevStack.deps.json
├── DevStack.dll
├── ...outros arquivos...
```

### **No Release (para desenvolvedores)**
```
release/
├── DevStack-Uninstaller.exe        # Uninstaller gerado
├── DevStack.exe
├── DevStackGUI.exe
├── ...outros arquivos...
```

## Scripts de Build

### **Build apenas do Uninstaller**
```powershell
.\scripts\build-uninstaller.ps1
```

### **Build completo (Instalador + Uninstaller)**
```powershell
.\scripts\build-installer.ps1 -Clean
```

O uninstaller é automaticamente incluído no arquivo zip embutido no instalador.

## Interface do Uninstaller

### **Características da UI**
- Header escuro com ícone de lixeira
- Aviso destacado em amarelo sobre o que será removido
- Lista clara dos itens que serão deletados:
  - Todos os arquivos de programa
  - Configurações e dados do usuário  
  - Atalhos da área de trabalho e menu iniciar
  - Entradas do registro do Windows
  - Serviços instalados
- Caminho de instalação detectado automaticamente
- Barra de progresso durante o processo
- Botões "Cancelar" e "Desinstalar" com cores apropriadas

### **Processo Visual**
1. Mostra aviso e solicita confirmação
2. Exibe progresso em tempo real:
   - Parando serviços... (10%)
   - Removendo atalhos... (30%)
   - Limpando registro... (50%)
   - Removendo arquivos... (70%)
   - Finalizando... (90%)
   - Desinstalação concluída! (100%)

## Funcionalidades Técnicas

### **Privilégios Elevados**
- Manifesto UAC configurado para `requireAdministrator`
- Necessário para remover arquivos do sistema e limpar registros
- Windows solicita automaticamente elevação ao executar

### **Detecção Inteligente**
- Busca no registro do usuário primeiro
- Fallback para o diretório atual do executável
- Verifica a presença de arquivos DevStack para confirmar instalação

### **Limpeza Completa**
- **Processos**: Para DevStack.exe e DevStackGUI.exe
- **Atalhos**: Remove .lnk da área de trabalho e menu iniciar
- **Registro**: Limpa HKCU e HKLM (se tiver permissão)
- **Arquivos**: Remove tudo exceto o próprio uninstaller
- **Auto-remoção**: Agenda exclusão própria via batch script

### **Segurança**
- Confirmação obrigatória antes da desinstalação
- Não remove arquivos fora do diretório de instalação
- Tratamento de erros robusto (continua mesmo se alguma etapa falhar)

## Integração com Windows

### **Aparece em "Aplicativos Instalados"**
Quando o instalador executa, ele registra o DevStack com as seguintes informações:

```registry
[HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\DevStack]
"DisplayName" = "DevStack Manager"
"DisplayVersion" = "2.0.0"
"Publisher" = "DevStackManager"
"InstallLocation" = "C:\Program Files\DevStack"
"UninstallString" = "C:\Program Files\DevStack\DevStack-Uninstaller.exe"
"NoModify" = 1
"NoRepair" = 1
"EstimatedSize" = [tamanho calculado em KB]
```

### **Como Acessar**
1. **Configurações do Windows**:
   - Win + I → Apps → Aplicativos instalados
   - Procurar "DevStack" → Clicar "..." → Desinstalar

2. **Painel de Controle**:
   - Win + R → `appwiz.cpl` → Enter
   - Encontrar "DevStack Manager" → Desinstalar

3. **Executar diretamente**:
   - Navegar até a pasta de instalação
   - Duplo clique em `DevStack-Uninstaller.exe`

## Notas de Desenvolvimento

### **Warnings Resolvidos**
- Substituído `Assembly.GetExecutingAssembly().Location` por `AppContext.BaseDirectory`
- Compatível com single-file deployment
- Sem dependências externas

### **Estrutura do Código**
- Namespace: `DevStackUninstaller`
- Arquivo principal: `Program.cs`
- Interface WPF nativa
- Operações assíncronas para não bloquear UI

### **Configuração Single-File**
```xml
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>false</SelfContained>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
```

## Exemplo de Uso Completo

```powershell
# 1. Build completo
.\scripts\build.ps1 -WithInstaller -Clean

# 2. Instalar (como administrador)
.\installer\DevStack-2.0.0-Installer.exe

# 3. Desinstalar via Windows:
#    Configurações > Apps > DevStack Manager > Desinstalar
#    
#    OU execução direta:
#    C:\Program Files\DevStack\DevStack-Uninstaller.exe
```

O sistema está totalmente funcional e segue as melhores práticas do Windows para instalação/desinstalação de aplicações!
