# DevStackSetup

## O que é?

Scripts PowerShell para instalar, gerenciar e remover rapidamente um ambiente de desenvolvimento local (PHP, Node.js, Python, Nginx, MySQL, Composer, phpMyAdmin) no Windows.

## Como usar

Abra um terminal PowerShell como administrador e execute:

```pwsh
# Torne o DevStackSetup global (execute uma vez):
./setup.ps1 global
# Após isso, use 'devstack' ou 'setup.ps1' de qualquer pasta no terminal!

# Listar versões disponíveis
devstack list php
devstack list nodejs
devstack list python

# Instalar componentes
devstack install php-8.3.21 nginx mysql nodejs python composer phpmyadmin

# Remover componentes
devstack uninstall php-8.3.21 nginx mysql nodejs python composer phpmyadmin

# Adicionar diretórios ao PATH
devstack path

# Criar site Nginx
devstack site meuprojeto.localhost -root C:\Workspace\meuprojeto

# Iniciar/parar/reiniciar serviços
devstack start nginx 1.25.4

devstack stop php 8.3.21

devstack restart nginx 1.25.4

# Verificar status dos componentes
devstack status

# Testar funcionamento dos binários
devstack test

# Verificar dependências do sistema
devstack deps

# Atualizar para a última versão
devstack update php nodejs python nginx mysql composer phpmyadmin

# Criar alias/batch para um executável
devstack alias php 8.3.21

# Ver logs
Get-Content C:\devstack\devstack.log -Tail 50
```

## Troubleshooting

- Execute sempre como administrador para garantir permissões de PATH e hosts.
- Se um download falhar, tente novamente ou verifique sua conexão.
- O arquivo de log `C:\devstack\devstack.log` registra todas as operações.
- Se PATH não atualizar, reinicie o terminal.
- Se o alias 'devstack' não funcionar, feche e abra o PowerShell novamente ou rode `& $PROFILE` para recarregar o perfil.

## Como estender

- Adicione novos scripts em PowerShell para outros stacks.
- Use as funções helper para evitar duplicação.
- Adicione testes automatizados com [Pester](https://pester.dev/).

## Contribuição

- Siga o padrão modular dos scripts.
- Adicione exemplos de uso ao README.
- Faça PRs com testes automatizados.