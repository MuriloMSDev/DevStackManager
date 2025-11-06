#!/usr/bin/env python3
"""
DevStack Version Manager - Python Version

Script para verificar e atualizar as versões disponíveis dos componentes DevStack.

Este script permite:
- Verificar URLs existentes nos arquivos CS
- Remover URLs quebradas/inválidas
- Buscar novas versões disponíveis
- Atualizar os arquivos CS com novas versões
- Criar backups antes das alterações
- Reordenar versões em ordem crescente

Uso:
    python update_versions.py [opções]

Opções:
    --component COMPONENTE    Componente específico para atualizar
    --check-only             Apenas verifica URLs existentes sem atualizar
    --update-all            Atualiza todos os componentes automaticamente
    --clear-cache           Limpa o cache de versões falhadas
    --clear-backups         Limpa backups antigos (mais de 30 dias)
    --show-backups          Mostra informações dos backups
    --help                  Mostra esta ajuda

Exemplos:
    python update_versions.py --check-only
    python update_versions.py --component php --check-only
    python update_versions.py --component php
    python update_versions.py --update-all
    python update_versions.py --clear-cache
    python update_versions.py --component php --clear-cache
    python update_versions.py --clear-backups
    python update_versions.py --component php --clear-backups
    python update_versions.py --show-backups
"""

import os
import sys
import json
import time
import argparse
import asyncio
import aiohttp
import http.client
import http.cookiejar
from datetime import datetime, timedelta
from pathlib import Path
from typing import List, Dict, Any, Optional, Tuple
from urllib.parse import urlparse
import random
import re
import shutil
import signal
import urllib.error
import urllib.parse
import urllib.request
from urllib.request import Request, urlopen
from urllib.error import URLError, HTTPError

# Tenta importar tqdm para barras de progresso aprimoradas
try:
    from tqdm import tqdm
    TQDM_AVAILABLE = True
except ImportError:
    TQDM_AVAILABLE = False

# Configurações globais
AVAILABLE_VERSIONS_PATH = Path(__file__).parent.parent / "src" / "Shared" / "AvailableVersions" / "Providers"
BACKUP_PATH = Path(__file__).parent.parent / "src" / "Shared" / "AvailableVersions" / "backup"
CACHE_PATH = BACKUP_PATH / "cache"
MAX_WORKERS = 50  # Máximo para performance otimizada
TIMEOUT_SECONDS = 30

# Headers para evitar detecção como bot - baseados no código C#
HEADERS = {
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
    'Accept': '*/*',
    'Accept-Language': 'en-US,en;q=0.9',
    'Accept-Encoding': 'gzip, deflate',
    'Connection': 'keep-alive',
    'Upgrade-Insecure-Requests': '1',
    'Sec-Fetch-Site': 'cross-site',
    'Sec-Fetch-Mode': 'no-cors',
    'Sec-Fetch-Dest': 'document'
}

# Lista de User-Agents para rotação
USER_AGENTS = [
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/119.0.0.0 Safari/537.36',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/121.0',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/120.0',
    'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0'
]

# Headers específicos por domínio para anti-detecção
DOMAIN_SPECIFIC_HEADERS = {
    'mysql.com': {
        'Referer': 'https://dev.mysql.com/downloads/mysql/',
        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
        'Accept-Language': 'en-US,en;q=0.5',
        'Cache-Control': 'max-age=0'
    },
    'dev.mysql.com': {
        'Referer': 'https://dev.mysql.com/downloads/mysql/',
        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
        'Accept-Language': 'en-US,en;q=0.5',
        'Cache-Control': 'max-age=0'
    },
    'github.com': {
        'Referer': 'https://github.com/',
        'Accept': 'application/vnd.github.v3+json',
        'Accept-Language': 'en-US,en;q=0.9',
        'Authorization': 'token ' + os.environ.get('GITHUB_TOKEN', '')
    },
    'api.github.com': {
        'Referer': 'https://github.com/',
        'Accept': 'application/vnd.github.v3+json',
        'Accept-Language': 'en-US,en;q=0.9',
        'Authorization': 'token ' + os.environ.get('GITHUB_TOKEN', '')
    },
    'nodejs.org': {
        'Referer': 'https://nodejs.org/',
        'Accept': 'application/json,text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8',
        'Accept-Language': 'en-US,en;q=0.9'
    },
    'windows.php.net': {
        'Referer': 'https://windows.php.net/download',
        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
        'Accept-Language': 'en-US,en;q=0.5'
    },
    'nginx.org': {
        'Referer': 'https://nginx.org/',
        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
        'Accept-Language': 'en-US,en;q=0.5'
    },
    'enterprisedb.com': {
        'Referer': 'https://www.enterprisedb.com/download-postgresql-binaries',
        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
        'Accept-Language': 'en-US,en;q=0.5'
    }
}

class UrlCheckResult:
    def __init__(self, url: str):
        self.url = url
        self.is_valid = False
        self.error_message = ""
        self.status_code = 0
        self.content_error = ""  # Erro específico relacionado ao conteúdo/download
        self.content_length = 0

class NewVersionResult:
    def __init__(self, component: str):
        self.component = component
        self.new_versions = []
        self.error_message = ""

def print_colored(text: str, color: str = "white", end: str = "\n"):
    """Imprime texto colorido no terminal"""
    colors = {
        "black": "\033[30m",
        "red": "\033[31m",
        "green": "\033[32m",
        "yellow": "\033[33m",
        "blue": "\033[34m",
        "magenta": "\033[35m",
        "cyan": "\033[36m",
        "white": "\033[37m",
        "gray": "\033[90m",
        "bright_red": "\033[91m",
        "bright_green": "\033[92m",
        "bright_yellow": "\033[93m",
        "bright_blue": "\033[94m",
        "bright_magenta": "\033[95m",
        "bright_cyan": "\033[96m",
        "bright_white": "\033[97m",
        "reset": "\033[0m"
    }

    color_code = colors.get(color.lower(), colors["white"])
    print(f"{color_code}{text}{colors['reset']}", end=end)

# Mostra aviso se tqdm não estiver disponível
if not TQDM_AVAILABLE:
    print_colored("Aviso: tqdm não está instalado. Instale com 'pip install tqdm' para barras de progresso aprimoradas.", "yellow")

# Cookie jar para preservar cookies entre requests (como no código C#)
cookie_jar = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cookie_jar))
urllib.request.install_opener(opener)

def create_browser_request(url: str, referer: str = None, is_preflight: bool = False, domain_specific: bool = False) -> Request:
    """Cria um request com headers similares ao código C# CreateBrowserGetRequest"""
    # Seleciona User-Agent aleatório para rotação
    user_agent = random.choice(USER_AGENTS)

    # Headers base
    headers = {
        'User-Agent': user_agent,
        'Accept': '*/*',
        'Accept-Language': 'en-US,en;q=0.9',
        'Accept-Encoding': 'gzip, deflate',
        'Connection': 'keep-alive',
        'Upgrade-Insecure-Requests': '1',
        'Sec-Fetch-Site': 'cross-site',
        'Sec-Fetch-Mode': 'no-cors',
        'Sec-Fetch-Dest': 'document'
    }

    # Headers específicos por domínio se solicitado
    if domain_specific:
        parsed_url = urllib.parse.urlparse(url)
        domain = parsed_url.netloc.lower()

        # Procura por domínio específico ou domínio base
        domain_headers = None
        for domain_key, headers_config in DOMAIN_SPECIFIC_HEADERS.items():
            if domain_key in domain:
                domain_headers = headers_config
                break

        if domain_headers:
            headers.update(domain_headers)

    if referer:
        headers['Referer'] = referer

    # Para preflight, usa apenas headers básicos
    if is_preflight:
        headers['Accept'] = '*/*'
        headers['Accept-Language'] = 'en-US,en;q=0.9'

    req = Request(url, headers=headers)

    # Define method como HEAD para preflight (similar ao ResponseHeadersRead do C#)
    if is_preflight:
        req.get_method = lambda: 'HEAD'

    return req

def make_http_request(url: str, timeout: int = TIMEOUT_SECONDS, max_retries: int = 3) -> Tuple[Optional[bytes], Optional[str], int]:
    """
    Faz uma requisição HTTP simples e eficiente usando urllib básico
    """
    for attempt in range(max_retries):
        try:
            req = Request(url)
            with urlopen(req, timeout=timeout) as response:
                content = response.read()
                return content, None, response.getcode()

        except HTTPError as e:
            if attempt == max_retries - 1:  # Última tentativa
                return None, f"HTTP {e.code}: {e.reason}", e.code

        except URLError as e:
            if attempt == max_retries - 1:  # Última tentativa
                return None, str(e.reason), 0

        except Exception as e:
            if attempt == max_retries - 1:  # Última tentativa
                return None, str(e), 0

        # Pequeno delay entre tentativas
        if attempt < max_retries - 1:
            time.sleep(1)

    return None, "Unknown error", 0

def fetch_github_releases(api_url: str, timeout: int = TIMEOUT_SECONDS) -> Tuple[Optional[bytes], Optional[str], int]:
    """Faz request simples para GitHub API usando urllib básico"""
    for attempt in range(3):  # Menos tentativas para GitHub
        try:
            req = Request(api_url)
            with urlopen(req, timeout=timeout) as response:
                content = response.read()
                return content, None, response.getcode()

        except HTTPError as e:
            if e.code == 403:  # Rate limit
                if attempt < 2:
                    time.sleep(2)  # Espera um pouco para rate limit
                    continue
            return None, f"GitHub API HTTP {e.code}: {e.reason}", e.code

        except URLError as e:
            if attempt == 2:  # Última tentativa
                return None, str(e.reason), 0

        except Exception as e:
            if attempt == 2:  # Última tentativa
                return None, str(e), 0

        # Delay entre tentativas
        if attempt < 2:
            time.sleep(1)

    return None, "GitHub API error", 0

def create_progress_bar(total: int, description: str = "Processando"):
    """Cria uma barra de progresso (com tqdm ou fallback simples)"""
    if TQDM_AVAILABLE:
        return tqdm(total=total, desc=description, unit="url", ncols=80, bar_format='{l_bar}{bar}| {n_fmt}/{total_fmt} [{elapsed}<{remaining}, {rate_fmt}]')
    else:
        # Fallback para barra simples
        return SimpleProgressBar(total, description)

class SimpleProgressBar:
    """Barra de progresso simples como fallback quando tqdm não está disponível"""
    def __init__(self, total: int, description: str = "Processando"):
        self.total = total
        self.current = 0
        self.description = description
        self.start_time = time.time()
        self.last_update = 0
        self.is_closed = False

    def update(self, n: int = 1):
        """Atualiza a barra de progresso"""
        if self.is_closed:
            return

        self.current += n
        current_time = time.time()

        # Atualiza apenas a cada 0.1 segundos para não sobrecarregar
        if current_time - self.last_update >= 0.1:
            self._display()
            self.last_update = current_time

    def _display(self):
        """Exibe a barra de progresso"""
        if self.is_closed:
            return

        percentage = min((self.current / self.total) * 100, 100.0)  # Garante que não passe de 100%
        elapsed = time.time() - self.start_time
        rate = self.current / elapsed if elapsed > 0 else 0

        # Cria barra visual simples
        bar_width = 40
        filled = min(int(bar_width * self.current / self.total), bar_width)  # Garante que não passe da largura
        bar = "█" * filled + "░" * (bar_width - filled)

        print(f"\r{self.description}: [{bar}] {min(self.current, self.total)}/{self.total} ({percentage:.1f}%) [{elapsed:.1f}s, {rate:.1f} url/s]", end="", flush=True)

    def close(self):
        """Finaliza a barra de progresso"""
        if self.is_closed:
            return

        self.is_closed = True

        # Garante que a barra chegue exatamente a 100%
        if self.current < self.total:
            self.current = self.total
            self._display()

        # Pequena pausa para visualização da barra completa
        time.sleep(0.3)

        # Exibe uma última vez com 100%
        percentage = 100.0
        elapsed = time.time() - self.start_time
        rate = self.total / elapsed if elapsed > 0 else 0

        bar_width = 40
        bar = "█" * bar_width
        print(f"\r{self.description}: [{bar}] {self.total}/{self.total} ({percentage:.1f}%) [{elapsed:.1f}s, {rate:.1f} url/s]", end="", flush=True)

        print()  # Nova linha

def create_backup(file_path: Path) -> None:
    """Cria backup do arquivo"""
    if not BACKUP_PATH.exists():
        BACKUP_PATH.mkdir(parents=True)

    file_name = file_path.name
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    backup_file = BACKUP_PATH / f"{file_name}_{timestamp}.bak"

    import shutil
    shutil.copy2(file_path, backup_file)

    print_colored(f"Backup criado: {backup_file}", "green")

    # Limpa backups antigos automaticamente (mantém apenas os 10 mais recentes)
    clear_old_backups(file_name)

def clear_old_backups(file_name: str, keep_count: int = 10) -> None:
    """Limpa backups antigos automaticamente"""
    if not BACKUP_PATH.exists():
        return

    base_file_name = file_name.replace('.cs', '')
    backup_files = list(BACKUP_PATH.glob(f"{base_file_name}*.bak"))
    backup_files.sort(key=lambda x: x.stat().st_mtime, reverse=True)

    if len(backup_files) > keep_count:
        files_to_remove = backup_files[keep_count:]
        for file in files_to_remove:
            file.unlink()
            print_colored(f"  Backup antigo removido: {file.name}", "gray")
        print_colored(f"  {len(files_to_remove)} backups antigos removidos (mantidos {keep_count} mais recentes)", "gray")

def parse_cs_versions(file_path: Path) -> List[Dict]:
    """Extrai versões de um arquivo CS"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        versions = []
        # Procura por padrão: new VersionInfo("version", "url")
        pattern = r'new\s+VersionInfo\s*\(\s*"([^"]+)"\s*,\s*"([^"]+)"\s*\)'
        matches = re.findall(pattern, content)
        
        for version, url in matches:
            versions.append({
                'version': version,
                'url': url
            })
        
        return versions
    except Exception as e:
        print_colored(f"Erro ao ler arquivo CS: {e}", "red")
        return []

def get_cs_component_info(file_path: Path) -> Tuple[str, str]:
    """Extrai ComponentName e ComponentId de um arquivo CS"""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        component_name = ""
        component_id = ""
        
        # Procura por ComponentName
        name_match = re.search(r'public\s+string\s+ComponentName\s*=>\s*"([^"]+)"', content)
        if name_match:
            component_name = name_match.group(1)
        
        # Procura por ComponentId
        id_match = re.search(r'public\s+string\s+ComponentId\s*=>\s*"([^"]+)"', content)
        if id_match:
            component_id = id_match.group(1)
        
        return component_name, component_id
    except Exception as e:
        print_colored(f"Erro ao ler informações do componente: {e}", "red")
        return "", ""

def write_cs_versions(file_path: Path, versions: List[Dict], component_name: str = "", component_id: str = "") -> None:
    """Escreve versões em um arquivo CS mantendo a estrutura"""
    try:
        # Lê o arquivo original para manter a estrutura
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Se não tiver component_name/id, tenta extrair do arquivo
        if not component_name or not component_id:
            component_name, component_id = get_cs_component_info(file_path)
        
        # Gera o novo conteúdo da lista de versões
        version_lines = []
        for version in versions:
            version_lines.append(f'            new VersionInfo("{version["version"]}", "{version["url"]}")')
        
        version_list_content = ',\n'.join(version_lines)
        
        # Substitui o conteúdo da lista de versões
        # Padrão: private static readonly List<VersionInfo> _versions = new List<VersionInfo> { ... };
        pattern = r'(private\s+static\s+readonly\s+List<VersionInfo>\s+_versions\s*=\s*new\s+List<VersionInfo>\s*\{).*?(\s*\};)'
        replacement = rf'\1\n{version_list_content}        \2'
        
        new_content = re.sub(pattern, replacement, content, flags=re.DOTALL)
        
        # Salva o arquivo atualizado
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(new_content)
        
    except Exception as e:
        print_colored(f"Erro ao escrever arquivo CS: {e}", "red")

def get_failed_versions_cache(component_name: str) -> List[Dict]:
    """Obtém cache de versões falhadas"""
    if not CACHE_PATH.exists():
        CACHE_PATH.mkdir(parents=True)

    cache_file = CACHE_PATH / f"{component_name}-failed.json"

    if cache_file.exists():
        try:
            with open(cache_file, 'r', encoding='utf-8') as f:
                cache_content = json.load(f)

            # Remove entradas antigas (mais de 7 dias)
            cutoff_date = datetime.now() - timedelta(days=7)
            valid_cache = [
                entry for entry in cache_content
                if datetime.fromisoformat(entry['FailedDate']) > cutoff_date
            ]

            # Se houve remoção de entradas antigas, atualiza o cache
            if len(valid_cache) != len(cache_content):
                if len(valid_cache) == 0:
                    cache_file.unlink()
                    print_colored("  Cache de versões falhadas removido (expirado)", "gray")
                else:
                    with open(cache_file, 'w', encoding='utf-8') as f:
                        json.dump(valid_cache, f, indent=2, ensure_ascii=False)
                    print_colored(f"  Cache de versões falhadas limpo ({len(cache_content) - len(valid_cache)} entradas expiradas removidas)", "gray")

            return valid_cache
        except Exception as e:
            print_colored(f"Erro ao ler cache de versões falhadas: {e}", "yellow")
            return []

    return []

def save_failed_versions_cache(component_name: str, failed_versions: List[Dict]) -> None:
    """Salva versões falhadas no cache"""
    if not failed_versions:
        return

    if not CACHE_PATH.exists():
        CACHE_PATH.mkdir(parents=True)

    cache_file = CACHE_PATH / f"{component_name}-failed.json"

    # Carrega cache existente
    existing_cache = get_failed_versions_cache(component_name)

    # Adiciona novas versões falhadas
    current_date = datetime.now().isoformat()
    new_failed_entries = []
    for version in failed_versions:
        new_failed_entries.append({
            'Version': version['version'],
            'Url': version['url'],
            'FailedDate': current_date,
            'ErrorMessage': getattr(version, 'ErrorMessage', version.get('ErrorMessage', ''))
        })

    # Combina com cache existente (evita duplicatas)
    all_cached_versions = existing_cache[:]
    for new_entry in new_failed_entries:
        existing = next(
            (entry for entry in all_cached_versions
             if entry['Version'] == new_entry['Version'] and entry['Url'] == new_entry['Url']),
            None
        )
        if not existing:
            all_cached_versions.append(new_entry)

    # Salva cache atualizado
    with open(cache_file, 'w', encoding='utf-8') as f:
        json.dump(all_cached_versions, f, indent=2, ensure_ascii=False)

    print_colored(f"  Cache de versões falhadas atualizado: {len(new_failed_entries)} novas entradas", "gray")

def get_backup_info(component_name: str = "") -> List[Dict]:
    """Obtém informações dos backups"""
    if not BACKUP_PATH.exists():
        return []

    filter_pattern = f"{component_name}*.bak" if component_name else "*.bak"
    backup_files = list(BACKUP_PATH.glob(filter_pattern))
    backup_files = [f for f in backup_files if not f.name.endswith("failed.json")]

    backup_info = []
    for file in backup_files:
        # Extrai informações do nome do arquivo: componente_yyyyMMdd_HHmmss.bak
        match = re.match(r'^(.+?)_(\d{8})_(\d{6})\.bak$', file.name)
        if match:
            component = match.group(1)
            date_string = match.group(2)
            time_string = match.group(3)

            try:
                backup_date = datetime.strptime(f"{date_string}{time_string}", "%Y%m%d%H%M%S")
                days_old = (datetime.now() - backup_date).days

                backup_info.append({
                    'Component': component,
                    'FileName': file.name,
                    'FullPath': str(file),
                    'BackupDate': backup_date,
                    'DaysOld': days_old,
                    'SizeKB': round(file.stat().st_size / 1024, 2)
                })
            except ValueError:
                # Se não conseguir parsear a data, adiciona com data desconhecida
                backup_info.append({
                    'Component': component,
                    'FileName': file.name,
                    'FullPath': str(file),
                    'BackupDate': file.stat().st_mtime,
                    'DaysOld': (datetime.now() - datetime.fromtimestamp(file.stat().st_mtime)).days,
                    'SizeKB': round(file.stat().st_size / 1024, 2)
                })

    return sorted(backup_info, key=lambda x: (x['Component'], x['BackupDate']), reverse=True)

def show_backup_info(component_name: str = "") -> None:
    """Mostra informações dos backups"""
    title = f"=== Backups de {component_name} ===" if component_name else "=== Todos os Backups ==="
    print_colored(f"\n{title}", "cyan")

    backups = get_backup_info(component_name)

    if not backups:
        print_colored("Nenhum backup encontrado", "gray")
        return

    current_component = ""
    total_size = 0

    for backup in backups:
        if backup['Component'] != current_component:
            if current_component:
                print()
            print_colored(f"--- {backup['Component']} ---", "yellow")
            current_component = backup['Component']

        age_status = " (ANTIGO)" if backup['DaysOld'] > 30 else ""
        date_formatted = backup['BackupDate'].strftime("%d/%m/%Y %H:%M:%S")

        print_colored(f"  • {backup['FileName']} - {date_formatted} ({backup['DaysOld']} dias) - {backup['SizeKB']} KB{age_status}", "gray")
        total_size += backup['SizeKB']

    print_colored(f"\nTotal: {len(backups)} backups - {round(total_size, 2)} KB", "green")

def clear_old_backups_manual(component_name: str = "", days_old: int = 30) -> None:
    """Limpa backups antigos manualmente"""
    backups = get_backup_info(component_name)
    old_backups = [b for b in backups if b['DaysOld'] > days_old]

    if not old_backups:
        scope_text = f"de {component_name} " if component_name else ""
        print_colored(f"Nenhum backup {scope_text}encontrado com mais de {days_old} dias", "gray")
        return

    print_colored("\nBackups que serão removidos:", "yellow")
    for backup in old_backups:
        print_colored(f"  • {backup['Component']}: {backup['FileName']} ({backup['DaysOld']} dias)", "gray")

    total_size = sum(b['SizeKB'] for b in old_backups)
    print_colored(f"\nTotal a ser removido: {len(old_backups)} arquivos - {round(total_size, 2)} KB", "yellow")

    confirm = input("\nConfirma a remoção? (s/N): ").strip().lower()
    if confirm == "s":
        for backup in old_backups:
            Path(backup['FullPath']).unlink()
        print_colored(f"{len(old_backups)} backups removidos com sucesso", "green")

def clear_all_backups(component_name: str = "") -> None:
    """Limpa todos os backups"""
    backups = get_backup_info(component_name)

    if not backups:
        scope_text = f"de {component_name} " if component_name else ""
        print_colored(f"Nenhum backup {scope_text}encontrado", "gray")
        return

    print_colored("\nBackups que serão removidos:", "yellow")
    for backup in backups:
        print_colored(f"  • {backup['Component']}: {backup['FileName']} ({backup['DaysOld']} dias)", "gray")

    total_size = sum(b['SizeKB'] for b in backups)
    print_colored(f"\nTotal a ser removido: {len(backups)} arquivos - {round(total_size, 2)} KB", "yellow")

    confirm = input("\nConfirma a remoção de TODOS os backups? (s/N): ").strip().lower()
    if confirm == "s":
        for backup in backups:
            Path(backup['FullPath']).unlink()
        print_colored(f"{len(backups)} backups removidos com sucesso", "green")

def show_failed_versions_cache() -> None:
    """Mostra cache de versões falhadas"""
    print_colored("\n=== Cache de Versões Falhadas ===", "cyan")

    if not CACHE_PATH.exists():
        print_colored("Nenhum cache encontrado", "gray")
        return

    cache_files = list(CACHE_PATH.glob("*-failed.json"))

    if not cache_files:
        print_colored("Nenhum cache de versões falhadas encontrado", "gray")
        return

    for cache_file in cache_files:
        component_name = cache_file.stem.replace('-failed', '')
        print_colored(f"\n--- {component_name} ---", "yellow")

        try:
            with open(cache_file, 'r', encoding='utf-8') as f:
                cache_content = json.load(f)

            if not cache_content:
                print_colored("  Cache vazio", "gray")
                continue

            for entry in cache_content:
                days_since = (datetime.now() - datetime.fromisoformat(entry['FailedDate'])).days
                status = " (EXPIRADO)" if days_since > 7 else ""
                print_colored(f"  • {entry['Version']} - Falhou há {days_since} dias{status}", "gray")
                print_colored(f"    URL: {entry['Url']}", "gray")
                print_colored(f"    Erro: {entry['ErrorMessage']}", "gray")

        except Exception as e:
            print_colored(f"Erro ao ler cache de {component_name}: {e}", "yellow")

def clear_component_cache() -> None:
    """Limpa cache de um componente específico"""
    if not CACHE_PATH.exists():
        print_colored("Nenhum cache encontrado", "gray")
        return

    cache_files = list(CACHE_PATH.glob("*-failed.json"))

    if not cache_files:
        print_colored("Nenhum cache de versões falhadas encontrado", "gray")
        return

    print_colored("\nComponentes com cache disponível:", "yellow")
    for i, cache_file in enumerate(cache_files):
        component_name = cache_file.stem.replace('-failed', '')
        print_colored(f"{i + 1}. {component_name}")

    try:
        choice = int(input(f"\nEscolha o componente (1-{len(cache_files)}): ")) - 1
    except ValueError:
        print_colored("Escolha inválida", "red")
        return

    if 0 <= choice < len(cache_files):
        selected_file = cache_files[choice]
        component_name = selected_file.stem.replace('-failed', '')

        confirm = input(f"\nTem certeza que deseja limpar o cache de '{component_name}'? (s/N): ").strip().lower()
        if confirm == "s":
            selected_file.unlink()
            print_colored(f"Cache de '{component_name}' removido com sucesso", "green")
    else:
        print_colored("Escolha inválida", "red")

def clear_all_cache() -> None:
    """Limpa todo o cache"""
    if not CACHE_PATH.exists():
        print_colored("Nenhum cache encontrado", "gray")
        return

    confirm = input("\nTem certeza que deseja limpar TODO o cache de versões falhadas? (s/N): ").strip().lower()
    if confirm == "s":
        cache_files = list(CACHE_PATH.glob("*-failed.json"))

        if cache_files:
            for file in cache_files:
                file.unlink()
            print_colored(f"Todo o cache foi removido ({len(cache_files)} arquivos)", "green")
        else:
            print_colored("Nenhum cache encontrado para remover", "gray")

def clear_expired_cache() -> None:
    """Limpa cache expirado"""
    if not CACHE_PATH.exists():
        print_colored("Nenhum cache encontrado", "gray")
        return

    cache_files = list(CACHE_PATH.glob("*-failed.json"))

    if not cache_files:
        print_colored("Nenhum cache encontrado", "gray")
        return

    cutoff_date = datetime.now() - timedelta(days=7)
    removed_count = 0
    total_files = 0

    for cache_file in cache_files:
        component_name = cache_file.stem.replace('-failed', '')

        try:
            with open(cache_file, 'r', encoding='utf-8') as f:
                cache_content = json.load(f)

            original_count = len(cache_content)

            # Remove entradas expiradas
            valid_cache = [
                entry for entry in cache_content
                if datetime.fromisoformat(entry['FailedDate']) > cutoff_date
            ]

            if len(valid_cache) != original_count:
                if not valid_cache:
                    cache_file.unlink()
                    print_colored(f"Cache de {component_name} removido completamente (todas as entradas expiraram)", "yellow")
                    total_files += 1
                else:
                    with open(cache_file, 'w', encoding='utf-8') as f:
                        json.dump(valid_cache, f, indent=2, ensure_ascii=False)
                    print_colored(f"Cache de {component_name}: {original_count - len(valid_cache)} entradas expiradas removidas", "yellow")

                removed_count += (original_count - len(valid_cache))

        except Exception as e:
            print_colored(f"Erro ao processar cache de {component_name}: {e}", "yellow")

    if removed_count == 0 and total_files == 0:
        print_colored("Nenhuma entrada expirada encontrada", "green")
    else:
        print_colored("\nLimpeza concluída:", "green")
        if removed_count > 0:
            print_colored(f"  • {removed_count} entradas expiradas removidas", "green")
        if total_files > 0:
            print_colored(f"  • {total_files} arquivos de cache removidos completamente", "green")

def show_component_backup_menu() -> None:
    """Mostra menu de gerenciamento de backups por componente"""
    # Obtém lista de componentes com backups
    backups = get_backup_info()
    components = {}
    for backup in backups:
        comp = backup['Component']
        if comp not in components:
            components[comp] = []
        components[comp].append(backup)

    if not components:
        print_colored("Nenhum componente com backups encontrado", "gray")
        return

    while True:
        print_colored("\n=== Gerenciamento de Backups por Componente ===", "cyan")
        print_colored("Componentes disponíveis:")

        component_list = list(components.keys())
        for i, component_name in enumerate(component_list):
            backup_list = components[component_name]
            backup_count = len(backup_list)
            oldest_backup = max(backup_list, key=lambda x: x['DaysOld'])['DaysOld']
            total_size = sum(b['SizeKB'] for b in backup_list)

            print_colored(f"{i + 1}. {component_name} ({backup_count} backups, mais antigo: {oldest_backup} dias, {round(total_size, 2)} KB)")

        print_colored(f"{len(component_list) + 1}. Voltar")

        try:
            choice = int(input(f"\nEscolha o componente (1-{len(component_list) + 1}): ")) - 1
        except ValueError:
            print_colored("Escolha inválida", "red")
            continue

        if choice == len(component_list):
            return
        elif 0 <= choice < len(component_list):
            selected_component = component_list[choice]
            show_single_component_backup_menu(selected_component)
        else:
            print_colored("Escolha inválida", "red")

def show_single_component_backup_menu(component_name: str) -> None:
    """Mostra menu de um componente específico"""
    while True:
        print_colored(f"\n=== Backups de {component_name} ===", "cyan")
        print_colored("1. Visualizar backups")
        print_colored("2. Limpar backups antigos (mais de 30 dias)")
        print_colored("3. Limpar todos os backups")
        print_colored("4. Restaurar backup")
        print_colored("5. Voltar")

        try:
            choice = input("\nEscolha uma opção (1-5): ").strip()
        except KeyboardInterrupt:
            return

        if choice == "1":
            show_backup_info(component_name)
        elif choice == "2":
            clear_old_backups_manual(component_name)
        elif choice == "3":
            clear_all_backups(component_name)
        elif choice == "4":
            restore_backup(component_name)
        elif choice == "5":
            return
        else:
            print_colored("Opção inválida", "yellow")

def show_cache_management_menu() -> None:
    """Mostra menu de gerenciamento de cache e backups"""
    while True:
        print_colored("\n=== Gerenciamento de Cache e Backups ===", "cyan")
        print_colored("--- Cache de Versões Falhadas ---")
        print_colored("1. Visualizar cache de versões falhadas")
        print_colored("2. Limpar cache de um componente específico")
        print_colored("3. Limpar todo o cache")
        print_colored("4. Limpar cache expirado (mais de 7 dias)")
        print_colored("")
        print_colored("--- Gerenciamento de Backups ---")
        print_colored("5. Visualizar backups")
        print_colored("6. Limpar backups antigos (mais de 30 dias)")
        print_colored("7. Limpar todos os backups")
        print_colored("8. Restaurar backup")
        print_colored("9. Gerenciar backups por componente")
        print_colored("")
        print_colored("10. Voltar ao menu principal")

        try:
            choice = input("\nEscolha uma opção (1-10): ").strip()
        except KeyboardInterrupt:
            return

        if choice == "1":
            show_failed_versions_cache()
        elif choice == "2":
            clear_component_cache()
        elif choice == "3":
            clear_all_cache()
        elif choice == "4":
            clear_expired_cache()
        elif choice == "5":
            show_backup_info()
        elif choice == "6":
            clear_old_backups_manual()
        elif choice == "7":
            clear_all_backups()
        elif choice == "8":
            restore_backup()
        elif choice == "9":
            show_component_backup_menu()
        elif choice == "10":
            return
        else:
            print_colored("Opção inválida", "yellow")

def restore_backup(component_name: str = "") -> None:
    """Restaura backup"""
    backups = get_backup_info(component_name)

    if not backups:
        scope_text = f"de {component_name} " if component_name else ""
        print_colored(f"Nenhum backup {scope_text}encontrado", "gray")
        return

    print_colored("\nBackups disponíveis:", "yellow")
    for i, backup in enumerate(backups):
        date_formatted = backup['BackupDate'].strftime("%d/%m/%Y %H:%M:%S")
        print_colored(f"{i + 1}. {backup['Component']} - {date_formatted} ({backup['DaysOld']} dias)", "gray")

    try:
        choice = int(input(f"\nEscolha o backup para restaurar (1-{len(backups)}): ")) - 1
    except ValueError:
        print_colored("Escolha inválida", "red")
        return

    if 0 <= choice < len(backups):
        selected_backup = backups[choice]
        original_file = AVAILABLE_VERSIONS_PATH / f"{selected_backup['Component']}.json"

        print_colored("\nRestaurar:", "yellow")
        print_colored(f"  De: {selected_backup['FileName']}", "gray")
        print_colored(f"  Para: {selected_backup['Component']}.json", "gray")

        if original_file.exists():
            print_colored("  ⚠ O arquivo atual será sobrescrito", "red")

        confirm = input("\nConfirma a restauração? (s/N): ").strip().lower()
        if confirm == "s":
            # Cria backup do arquivo atual antes de restaurar
            if original_file.exists():
                timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
                pre_restore_backup = BACKUP_PATH / f"{selected_backup['Component']}_pre_restore_{timestamp}.bak"
                import shutil
                shutil.copy2(original_file, pre_restore_backup)
                print_colored(f"Backup pré-restauração criado: {pre_restore_backup}", "green")

            # Restaura o backup
            import shutil
            shutil.copy2(selected_backup['FullPath'], original_file)
            print_colored("Backup restaurado com sucesso!", "green")
    else:
        print_colored("Escolha inválida", "red")

def normalize_version(version_string: str, component_name: str = "") -> Optional[str]:
    """Normaliza versão"""
    # Remove prefixos comuns como v, V, node-v, go, php-, mysql-, etc.
    version = re.sub(r'^[vV]', '', version_string)  # Remove v ou V no início
    version = re.sub(r'^node-v?', '', version)  # Remove node-v ou node- no início
    version = re.sub(r'^go', '', version)  # Remove go no início

    # PREFIXOS MAIS ESPECÍFICOS PRIMEIRO (para evitar conflitos)
    version = re.sub(r'^mongodb-windows-x86_64-', '', version)  # Remove mongodb-windows-x86_64- no início
    version = re.sub(r'^php-cs-fixer-?v?', '', version)  # Remove php-cs-fixer-, php-cs-fixer-v ou php-cs-fixerv no início
    version = re.sub(r'^phpMyAdmin-', '', version)  # Remove phpMyAdmin- no início
    version = re.sub(r'^Win64OpenSSL-', '', version)  # Remove Win64OpenSSL- no início
    version = re.sub(r'^elasticsearch-', '', version)  # Remove elasticsearch- no início
    version = re.sub(r'^postgresql-', '', version)  # Remove postgresql- no início
    version = re.sub(r'^dbeaver-ce-', '', version)  # Remove dbeaver-ce- no início
    version = re.sub(r'^wp-cli-', '', version)  # Remove wp-cli- no início
    version = re.sub(r'^MinGit-', '', version)  # Remove MinGit- no início

    # PREFIXOS GENÉRICOS POR ÚLTIMO (para não interferir nos específicos)
    version = re.sub(r'^adminer-', '', version)  # Remove adminer- no início
    version = re.sub(r'^composer-', '', version)  # Remove composer- no início
    version = re.sub(r'^python-', '', version)  # Remove python- no início
    version = re.sub(r'^mysql-', '', version)  # Remove mysql- no início
    version = re.sub(r'^nginx-', '', version)  # Remove nginx- no início
    version = re.sub(r'^php-', '', version)  # Remove php- no início (GENÉRICO - por último)

    # Remove sufixos comuns como -winx64, -win-x64, -windows-x86_64, -all-languages, etc.
    version = re.sub(r'-winx64.*$', '', version)  # Remove -winx64 e tudo após
    version = re.sub(r'-win-x64.*$', '', version)  # Remove -win-x64 e tudo após
    version = re.sub(r'-win32\.win32\.x86_64.*$', '', version)  # Remove -win32.win32.x86_64 e tudo após
    version = re.sub(r'-windows-x86_64.*$', '', version)  # Remove -windows-x86_64 e tudo após
    version = re.sub(r'-all-languages.*$', '', version)  # Remove -all-languages e tudo após
    version = re.sub(r'-amd64.*$', '', version)  # Remove -amd64 e tudo após
    version = re.sub(r'-embed-amd64.*$', '', version)  # Remove -embed-amd64 e tudo após
    version = re.sub(r'-64-bit.*$', '', version)  # Remove -64-bit e tudo após
    version = re.sub(r'\.zip$', '', version)  # Remove .zip no final
    version = re.sub(r'\.exe$', '', version)  # Remove .exe no final
    version = re.sub(r'\.phar$', '', version)  # Remove .phar no final
    version = re.sub(r'\.php$', '', version)  # Remove .php no final
    version = re.sub(r'-Win32-vs1[67]-x64.*$', '', version)  # Remove -Win32-vs16-x64 ou -Win32-vs17-x64 e tudo após
    version = re.sub(r'\.windows\.(\d+)', '', version)  # Remove apenas 'windows.X' (mantém o resto da versão)
    version = re.sub(r'_(\d+)_(\d+)', r'.\1.\2', version)  # Converte underscores em pontos para OpenSSL (3_5_1 -> 3.5.1)

    # Remove sufixos de pre-release e build como alpha, beta, rc, etc.
    version = re.sub(r'-?(alpha|beta|rc|dev|snapshot)\d*.*$', '', version)  # Remove alpha, beta, rc, dev, snapshot

    # Extrai a versão principal - suporta de 2 a 4 partes (X.Y, X.Y.Z, X.Y.Z.W)
    match = re.match(r'^(\d+\.\d+(?:\.\d+)?(?:\.\d+)?)', version)
    if match:
        extracted_version = match.group(1)

        # Componentes que normalmente usam 4 dígitos (x.y.z.w)
        four_digit_components = ["phpmyadmin"]

        # Se o componente normalmente usa 4 dígitos e a versão tem apenas 3 dígitos, adiciona .0
        if component_name.lower() in four_digit_components and re.match(r'^\d+\.\d+\.\d+$', extracted_version):
            extracted_version = f"{extracted_version}.0"

        return extracted_version

    return None

async def test_url_valid_async(url: str) -> UrlCheckResult:
    """Verifica se uma URL é válida usando aiohttp (versão assíncrona)"""
    result = UrlCheckResult(url)

    try:
        timeout = aiohttp.ClientTimeout(total=TIMEOUT_SECONDS)
        async with aiohttp.ClientSession(timeout=timeout) as session:
            async with session.head(url) as response:
                result.status_code = response.status
                result.is_valid = response.status < 400

                # Obtém tamanho do conteúdo se disponível
                content_length = response.headers.get('Content-Length')
                if content_length:
                    result.content_length = int(content_length)

    except asyncio.TimeoutError:
        result.is_valid = False
        result.error_message = "Timeout"
        result.status_code = 408
    except aiohttp.ClientError as e:
        result.is_valid = False
        result.error_message = str(e)
        result.status_code = 0
    except Exception as e:
        result.is_valid = False
        result.error_message = str(e)

    return result

def test_url_valid(url: str) -> UrlCheckResult:
    """Verifica se uma URL é válida de forma síncrona"""
    result = UrlCheckResult(url)
    
    try:
        # Faz uma requisição HEAD simples
        req = Request(url)
        req.get_method = lambda: 'HEAD'
        
        with urlopen(req, timeout=TIMEOUT_SECONDS) as response:
            result.status_code = response.getcode()
            result.is_valid = response.status < 400
            
            # Obtém tamanho do conteúdo se disponível
            content_length = response.headers.get('Content-Length')
            if content_length:
                result.content_length = int(content_length)
    except HTTPError as e:
        result.is_valid = False
        result.error_message = f"HTTP {e.code}: {e.reason}"
        result.status_code = e.code
    except URLError as e:
        result.is_valid = False
        result.error_message = str(e.reason)
        result.status_code = 0
    except Exception as e:
        result.is_valid = False
        result.error_message = str(e)
        result.status_code = 0
    
    return result

def test_single_url(url: str) -> bool:
    """Verifica uma URL específica"""
    print_colored(f"  Verificando: {url}", "gray")
    result = test_url_valid(url)

    if result.is_valid:
        status_msg = f"✓ URL válida (Status: {result.status_code})"
        if result.content_length > 0:
            status_msg += f" - Tamanho: {result.content_length:,} bytes"
        print_colored(f"  {status_msg}", "green")

        if result.content_error:
            print_colored(f"  Aviso de conteúdo: {result.content_error}", "yellow")

        return True
    else:
        error_msg = f"✗ URL inválida: {result.error_message}"
        if result.content_error:
            error_msg += f" | Conteúdo: {result.content_error}"
        print_colored(f"  {error_msg}", "red")
        return False

async def test_new_version_urls_async(new_versions: List[Dict], component_name: str = "") -> List[Dict]:
    """Verifica URLs de novas versões em paralelo com asyncio"""
    if not new_versions:
        return []

    # Carrega cache de versões falhadas
    failed_cache = []
    if component_name:
        failed_cache = get_failed_versions_cache(component_name)
        if failed_cache:
            print_colored(f"  Cache carregado: {len(failed_cache)} versões falhadas conhecidas", "gray")

    # Filtra versões que já falharam anteriormente
    versions_to_check = []
    skipped_versions = []

    for version in new_versions:
        cached_failure = next(
            (entry for entry in failed_cache
             if entry['Version'] == version['version'] and entry['Url'] == version['url']),
            None
        )

        if cached_failure:
            skipped_versions.append(version)
            print_colored(f"  ⚠ Pulando {version['version']} (falhou em {cached_failure['FailedDate']}): {version['url']}", "yellow")
        else:
            versions_to_check.append(version)

    if skipped_versions:
        print_colored(f"  {len(skipped_versions)} versões puladas (cache de falhas)", "yellow")

    if not versions_to_check:
        print_colored("  Nenhuma nova versão para verificar (todas no cache de falhas)", "gray")
        return []

    urls = [v['url'] for v in versions_to_check]
    print_colored(f"  Verificando {len(urls)} URLs de novas versões com asyncio...", "yellow")

    # Cria barra de progresso
    progress_bar = create_progress_bar(len(urls), "Verificando URLs")

    async def check_with_delay(url):
        result = await test_url_valid_async(url)
        progress_bar.update(1)
        # Delay entre requests para evitar detecção
        await asyncio.sleep(random.uniform(1, 3))
        return result

    # Cria tarefas com limite de concorrência
    semaphore = asyncio.Semaphore(MAX_WORKERS)

    async def limited_check(url):
        async with semaphore:
            return await check_with_delay(url)

    # Executa todas as verificações
    tasks = [limited_check(url) for url in urls]
    results = await asyncio.gather(*tasks, return_exceptions=True)

    # Processa resultados
    processed_results = []
    for i, result in enumerate(results):
        if isinstance(result, Exception):
            error_result = UrlCheckResult(urls[i])
            error_result.is_valid = False
            error_result.error_message = str(result)
            processed_results.append(error_result)
        else:
            processed_results.append(result)

    # Fecha barra de progresso
    progress_bar.close()

    # Separa versões válidas e inválidas
    valid_versions = []
    failed_versions = []

    for version in versions_to_check:
        url_result = next((r for r in processed_results if r.url == version['url']), None)
        if url_result and url_result.is_valid:
            success_msg = f"  ✓ {version['version']}: {version['url']}"
            if url_result.content_length > 0:
                success_msg += f" ({url_result.content_length:,} bytes)"
            print_colored(success_msg, "green")

            if url_result.content_error:
                print_colored(f"     Aviso: {url_result.content_error}", "yellow")

            valid_versions.append(version)
        else:
            error_msg = f"  ✗ {version['version']}: {version['url']}"
            if url_result:
                if url_result.error_message:
                    error_msg += f" - {url_result.error_message}"
                if url_result.content_error:
                    error_msg += f" | {url_result.content_error}"
            else:
                error_msg += " - Resultado não encontrado"
            print_colored(error_msg, "red")
            # Adiciona informação do erro para o cache
            if url_result:
                version['ErrorMessage'] = f"{url_result.error_message} | {url_result.content_error}".strip(" | ")
            else:
                version['ErrorMessage'] = "Resultado não encontrado"
            failed_versions.append(version)

    # Salva versões falhadas no cache
    if component_name and failed_versions:
        save_failed_versions_cache(component_name, failed_versions)

    return valid_versions

async def test_urls_parallel_async(urls: List[str]) -> List[UrlCheckResult]:
    """Verifica URLs em paralelo usando asyncio"""
    if not urls:
        return []

    print_colored(f"Verificando {len(urls)} URLs com asyncio...", "yellow")

    # Cria barra de progresso
    progress_bar = create_progress_bar(len(urls), "Verificando URLs")

    # Cria semaphore para limitar concorrência
    semaphore = asyncio.Semaphore(MAX_WORKERS)

    async def check_single_url(url):
        async with semaphore:
            result = await test_url_valid_async(url)
            progress_bar.update(1)
            # Pequeno delay para não sobrecarregar servidores
            await asyncio.sleep(random.uniform(0.1, 0.3))
            return result

    # Executa todas as verificações com limite de concorrência
    results = await asyncio.gather(*[check_single_url(url) for url in urls], return_exceptions=True)

    # Processa resultados e trata exceções
    processed_results = []
    for i, result in enumerate(results):
        if isinstance(result, Exception):
            # Se houve uma exceção, cria um resultado de erro
            error_result = UrlCheckResult(urls[i])
            error_result.is_valid = False
            error_result.error_message = str(result)
            processed_results.append(error_result)
        else:
            processed_results.append(result)

    # Fecha barra de progresso
    progress_bar.close()

    return processed_results

# Funções para buscar novas versões de diferentes componentes
async def get_git_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do Git usando asyncio"""
    try:
        api_url = "https://api.github.com/repos/git-for-windows/git/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do Git: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "git")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            # Procura o asset MinGit
            asset = next(
                (a for a in release.get('assets', [])
                 if 'MinGit' in a['name'] and re.search(r'64-bit\.zip$', a['name'])),
                None
            )
            if asset:
                new_versions.append({
                    'version': version,
                    'url': asset['browser_download_url']
                })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return await test_new_version_urls_async(new_versions, "git")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do Git: {e}", "yellow")
        return []

async def get_node_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do Node.js usando asyncio"""
    try:
        api_url = "https://nodejs.org/dist/index.json"
        content, error, status_code = make_http_request(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do Node.js: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            version = normalize_version(release['version'], "node")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://nodejs.org/dist/{release['version']}/node-{release['version']}-win-x64.zip"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return await test_new_version_urls_async(new_versions, "node")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do Node.js: {e}", "yellow")
        return []

async def get_php_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do PHP usando asyncio"""
    try:
        base_url = "https://windows.php.net"
        release_url = base_url + "/downloads/releases"
        archive_url = release_url + "/archives/"

        new_versions = []

        for url in [release_url, archive_url]:
            try:
                content, error, status_code = make_http_request(url)

                if not content:
                    print_colored(f"Erro ao buscar de {url}: {error}", "yellow")
                    continue

                # Usa BeautifulSoup para parsing HTML
                try:
                    from bs4 import BeautifulSoup
                    soup = BeautifulSoup(content.decode('utf-8'), 'html.parser')
                    links = soup.find_all('a', href=True)

                    for link in links:
                        href = link['href']
                        if 'php-' in href and 'Win32' in href and re.search(r'x64\.zip$', href):
                            match = re.search(r'php-(\d+\.\d+\.\d+)-Win32', href)
                            if match:
                                version_string = match.group(1)
                                version = normalize_version(version_string, "php")

                                if not version:
                                    continue
                                if any(v['version'] == version for v in existing_versions):
                                    continue

                                download_url = href if href.startswith('http') else base_url + href

                                new_versions.append({
                                    'version': version,
                                    'url': download_url
                                })
                except ImportError:
                    print_colored("BeautifulSoup não disponível, pulando busca de PHP", "yellow")

            except Exception as e:
                print_colored(f"Erro ao buscar de {url}: {e}", "yellow")

        # Verifica URLs em paralelo e retorna apenas as válidas
        return await test_new_version_urls_async(new_versions, "php")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do PHP: {e}", "yellow")
        return []

async def get_python_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do Python usando asyncio"""
    try:
        api_url = "https://api.github.com/repos/python/cpython/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do Python: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "python")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://www.python.org/ftp/python/{version}/python-{version}-amd64.zip"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return await test_new_version_urls_async(new_versions, "python")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do Python: {e}", "yellow")
        return []

# Async wrappers for remaining functions
async def get_mysql_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for MySQL version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_mysql_new_versions, existing_versions)

async def get_go_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for Go version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_go_new_versions, existing_versions)

async def get_mongodb_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for MongoDB version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_mongodb_new_versions, existing_versions)

async def get_nginx_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for Nginx version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_nginx_new_versions, existing_versions)

async def get_elasticsearch_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for Elasticsearch version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_elasticsearch_new_versions, existing_versions)

async def get_composer_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for Composer version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_composer_new_versions, existing_versions)

async def get_adminer_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for Adminer version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_adminer_new_versions, existing_versions)

async def get_dbeaver_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for DBeaver version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_dbeaver_new_versions, existing_versions)

async def get_openssl_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for OpenSSL version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_openssl_new_versions, existing_versions)

async def get_pgsql_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for PostgreSQL version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_pgsql_new_versions, existing_versions)

async def get_phpcsfixer_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for PHP CS Fixer version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_phpcsfixer_new_versions, existing_versions)

async def get_phpmyadmin_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for phpMyAdmin version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_phpmyadmin_new_versions, existing_versions)

async def get_wpcli_new_versions_async(existing_versions: List[Dict]) -> List[Dict]:
    """Async wrapper for WP-CLI version fetching"""
    loop = asyncio.get_event_loop()
    return await loop.run_in_executor(None, get_wpcli_new_versions, existing_versions)

def run_async_test_new_versions(new_versions: List[Dict], component_name: str) -> List[Dict]:
    """Helper function to run async test_new_version_urls from sync context"""
    import asyncio
    loop = asyncio.new_event_loop()
    asyncio.set_event_loop(loop)
    try:
        return loop.run_until_complete(test_new_version_urls_async(new_versions, component_name))
    finally:
        loop.close()

def get_mysql_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do MySQL usando urllib com anti-detecção"""
    try:
        # Busca releases do GitHub oficial do MySQL
        api_url = "https://api.github.com/repos/mysql/mysql-server/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do MySQL: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            # Extrai versão do tag (ex: mysql-8.4.5 -> 8.4.5)
            match = re.match(r'^mysql-(\d+\.\d+\.\d+)$', release['tag_name'])
            if match:
                version = match.group(1)
            else:
                # Tenta normalizar outras variações
                version = normalize_version(release['tag_name'], "mysql")
                if not version:
                    continue

            if any(v['version'] == version for v in existing_versions):
                continue

            # Constrói URL baseada no padrão do MySQL
            # Extrai versão principal (ex: 8.4.5 -> 8.4)
            match = re.match(r'^(\d+)\.(\d+)\.(\d+)', version)
            if match:
                major_minor = f"{match.group(1)}.{match.group(2)}"
                download_url = f"https://dev.mysql.com/get/Downloads/MySQL-{major_minor}/mysql-{version}-winx64.zip"

                new_versions.append({
                    'version': version,
                    'url': download_url
                })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "mysql")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do MySQL: {e}", "yellow")
        return []

def get_go_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do Go usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/golang/go/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do Go: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "go")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://go.dev/dl/go{version}.windows-amd64.zip"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "go")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do Go: {e}", "yellow")
        return []

def get_mongodb_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do MongoDB usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/mongodb/mongo/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do MongoDB: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "mongodb")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://fastdl.mongodb.org/windows/mongodb-windows-x86_64-{version}.zip"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "mongodb")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do MongoDB: {e}", "yellow")
        return []

def get_nginx_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do Nginx usando urllib com anti-detecção"""
    try:
        base_url = "https://nginx.org/download/"
        content, error, status_code = make_http_request(base_url)

        if not content:
            print_colored(f"Erro ao buscar versões do Nginx: {error}", "yellow")
            return []

        # Usa BeautifulSoup para parsing HTML
        try:
            from bs4 import BeautifulSoup
            soup = BeautifulSoup(content.decode('utf-8'), 'html.parser')
            links = soup.find_all('a', href=True)

            new_versions = []

            for link in links:
                href = link['href']
                match = re.search(r'nginx-(\d+\.\d+\.\d+)\.zip$', href)
                if match:
                    version_string = match.group(1)
                    version = normalize_version(version_string, "nginx")

                    if not version:
                        continue
                    if any(v['version'] == version for v in existing_versions):
                        continue

                    new_versions.append({
                        'version': version,
                        'url': f"https://nginx.org/download/{href}"
                    })

            # Verifica URLs em paralelo e retorna apenas as válidas
            return run_async_test_new_versions(new_versions, "nginx")
        except ImportError:
            print_colored("BeautifulSoup não disponível, pulando busca de Nginx", "yellow")
            return []
    except Exception as e:
        print_colored(f"Erro ao buscar versões do Nginx: {e}", "yellow")
        return []

def get_elasticsearch_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do Elasticsearch usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/elastic/elasticsearch/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do Elasticsearch: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "elasticsearch")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://artifacts.elastic.co/downloads/elasticsearch/elasticsearch-{version}-windows-x86_64.zip"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "elasticsearch")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do Elasticsearch: {e}", "yellow")
        return []

def get_composer_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do Composer usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/composer/composer/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do Composer: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "composer")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://getcomposer.org/download/{version}/composer.phar"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "composer")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do Composer: {e}", "yellow")
        return []

def get_adminer_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do Adminer usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/vrana/adminer/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do Adminer: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "adminer")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://github.com/vrana/adminer/releases/download/v{version}/adminer-{version}.php"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "adminer")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do Adminer: {e}", "yellow")
        return []

def get_dbeaver_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do DBeaver usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/dbeaver/dbeaver/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do DBeaver: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "dbeaver")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://dbeaver.io/files/{version}/dbeaver-ce-{version}-win32.win32.x86_64.zip"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "dbeaver")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do DBeaver: {e}", "yellow")
        return []

def get_openssl_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do OpenSSL usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/openssl/openssl/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do OpenSSL: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "openssl")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            # Converte versão para formato do Shining Light (ex: 3.1.0 -> 3_1_0)
            version_formatted = version.replace('.', '_')
            new_versions.append({
                'version': version,
                'url': f"https://slproweb.com/download/Win64OpenSSL-{version_formatted}.exe"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "openssl")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do OpenSSL: {e}", "yellow")
        return []

def get_pgsql_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do PostgreSQL usando urllib com anti-detecção"""
    try:
        new_versions = []

        # Busca informações da página de download oficial da EnterpriseDB
        download_page_url = "https://www.enterprisedb.com/download-postgresql-binaries"
        content, error, status_code = make_http_request(download_page_url)

        if not content:
            print_colored(f"Erro ao buscar versões do PostgreSQL: {error}", "yellow")
            return []

        # Extrai informações das versões disponíveis
        # Padrão: Version X.Y [Windows x86-64](https://sbp.enterprisedb.com/getfile.jsp?fileid=XXXXXX)
        content_str = content.decode('utf-8')

        # Busca por padrões de versão e URLs
        version_pattern = r'Version\s+(\d+\.\d+(?:\.\d+)?)\s+.*?Windows\s+x86-64.*?fileid=(\d+)'
        matches = re.findall(version_pattern, content_str, re.IGNORECASE)

        for match in matches:
            version_string = match[0]
            file_id = match[1]

            # Normaliza a versão (adiciona .0 se necessário para manter formato X.Y.Z)
            if re.match(r'^\d+\.\d+$', version_string):
                version = f"{version_string}.0"
            else:
                version = version_string

            if any(v['version'] == version for v in existing_versions):
                continue

            download_url = f"https://sbp.enterprisedb.com/getfile.jsp?fileid={file_id}"

            new_versions.append({
                'version': version,
                'url': download_url
            })

            print_colored(f"  Encontrada versão: {version} (FileID: {file_id})", "green")

        if new_versions:
            print_colored(f"  Total de novas versões encontradas: {len(new_versions)}", "yellow")
        else:
            print_colored("  Nenhuma nova versão encontrada", "gray")

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "pgsql")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do PostgreSQL: {e}", "yellow")
        return []

def get_phpcsfixer_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do PHP CS Fixer usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/PHP-CS-Fixer/PHP-CS-Fixer/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do PHP CS Fixer: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "phpcsfixer")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://github.com/PHP-CS-Fixer/PHP-CS-Fixer/releases/download/v{version}/php-cs-fixer.phar"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "phpcsfixer")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do PHP CS Fixer: {e}", "yellow")
        return []

def get_phpmyadmin_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do phpMyAdmin usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/phpmyadmin/phpmyadmin/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do phpMyAdmin: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "phpmyadmin")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://files.phpmyadmin.net/phpMyAdmin/{version}/phpMyAdmin-{version}-all-languages.zip"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "phpmyadmin")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do phpMyAdmin: {e}", "yellow")
        return []

def get_wpcli_new_versions(existing_versions: List[Dict]) -> List[Dict]:
    """Busca novas versões do WP-CLI usando urllib com anti-detecção"""
    try:
        api_url = "https://api.github.com/repos/wp-cli/wp-cli/releases"
        content, error, status_code = fetch_github_releases(api_url)

        if not content:
            print_colored(f"Erro ao buscar versões do WP-CLI: {error}", "yellow")
            return []

        releases = json.loads(content.decode('utf-8'))

        new_versions = []

        for release in releases:
            if release.get('prerelease') or release.get('draft'):
                continue

            version = normalize_version(release['tag_name'], "wpcli")
            if not version:
                continue

            if any(v['version'] == version for v in existing_versions):
                continue

            new_versions.append({
                'version': version,
                'url': f"https://github.com/wp-cli/wp-cli/releases/download/v{version}/wp-cli-{version}.phar"
            })

        # Verifica URLs em paralelo e retorna apenas as válidas
        return run_async_test_new_versions(new_versions, "wpcli")
    except Exception as e:
        print_colored(f"Erro ao buscar versões do WP-CLI: {e}", "yellow")
        return []

async def get_new_versions_for_component_async(component_name: str, existing_versions: List[Dict]) -> List[Dict]:
    """Função genérica para buscar novas versões de forma assíncrona"""
    component_functions = {
        "git": get_git_new_versions_async,
        "node": get_node_new_versions_async,
        "php": get_php_new_versions_async,
        "python": get_python_new_versions_async,
        "mysql": get_mysql_new_versions_async,
        "go": get_go_new_versions_async,
        "mongodb": get_mongodb_new_versions_async,
        "nginx": get_nginx_new_versions_async,
        "elasticsearch": get_elasticsearch_new_versions_async,
        "composer": get_composer_new_versions_async,
        "adminer": get_adminer_new_versions_async,
        "dbeaver": get_dbeaver_new_versions_async,
        "openssl": get_openssl_new_versions_async,
        "pgsql": get_pgsql_new_versions_async,
        "phpcsfixer": get_phpcsfixer_new_versions_async,
        "phpmyadmin": get_phpmyadmin_new_versions_async,
        "wpcli": get_wpcli_new_versions_async
    }

    func = component_functions.get(component_name.lower())
    if func:
        return await func(existing_versions)
    else:
        print_colored(f"Busca de novas versões não implementada para: {component_name}", "yellow")
        return []

def sort_versions(versions: List[Dict]) -> List[Dict]:
    """Ordena versões"""
    def version_key(version_dict):
        version = version_dict['version']
        parts = version.split('.')
        major = int(re.sub(r'\D', '', parts[0])) if parts else 0
        minor = int(re.sub(r'\D', '', parts[1])) if len(parts) > 1 else 0
        patch = int(re.sub(r'\D', '', parts[2])) if len(parts) > 2 else 0
        build = int(re.sub(r'\D', '', parts[3])) if len(parts) > 3 else 0

        # Cria um número para ordenação
        return (major * 1000000) + (minor * 10000) + (patch * 100) + build

    return sorted(versions, key=version_key)

async def process_component(component_name: str, file_path: Path, check_only: bool = False) -> None:
    """Processa um componente de forma assíncrona"""
    print_colored(f"\n=== Processando {component_name} ===", "cyan")

    if not file_path.exists():
        print_colored(f"Arquivo não encontrado: {file_path}", "yellow")
        return

    try:
        # Lê versões do arquivo CS
        cs_content = parse_cs_versions(file_path)

        print_colored(f"Carregadas {len(cs_content)} versões existentes", "green")

        # Verifica URLs existentes
        urls = [item['url'] for item in cs_content]
        results = await test_urls_parallel_async(urls)

        valid_urls = len([r for r in results if r.is_valid])
        invalid_urls = len([r for r in results if not r.is_valid])

        print_colored(f"URLs válidas: {valid_urls}", "green")
        print_colored(f"URLs inválidas: {invalid_urls}", "red")

        if invalid_urls > 0:
            print_colored("\nURLs inválidas encontradas:", "yellow")
            for result in results:
                if not result.is_valid:
                    error_msg = f"  - {result.url} (Status: {result.status_code})"
                    if result.error_message:
                        error_msg += f" - Erro: {result.error_message}"
                    if result.content_error:
                        error_msg += f" - Conteúdo: {result.content_error}"
                    print_colored(error_msg, "red")

        # Busca novas versões (tanto para CheckOnly quanto para atualização)
        print_colored("\nBuscando novas versões...", "yellow")
        new_versions = await get_new_versions_for_component_async(component_name, cs_content)

        if new_versions:
            print_colored(f"Encontradas {len(new_versions)} novas versões:", "green")

            for new_version in new_versions:
                print_colored(f"  + {new_version['version']}: {new_version['url']}", "cyan")
        else:
            print_colored("Nenhuma nova versão encontrada", "yellow")

        if check_only:
            print_colored("\n[MODO VERIFICAÇÃO] - Nenhuma alteração foi salva", "magenta")
            return

        # Remove URLs inválidas
        if invalid_urls > 0:
            invalid_urls_list = [r.url for r in results if not r.is_valid]
            valid_entries = [item for item in cs_content if item['url'] not in invalid_urls_list]

            if len(valid_entries) < len(cs_content):
                print_colored(f"Removendo {len(cs_content) - len(valid_entries)} entradas com URLs inválidas...", "yellow")
                cs_content = valid_entries

        if new_versions:
            # Adiciona novas versões
            all_versions = cs_content + new_versions

            # Ordena em ordem crescente
            sorted_versions = sort_versions(all_versions)

            # Cria backup
            create_backup(file_path)

            # Extrai informações do componente
            comp_name, comp_id = get_cs_component_info(file_path)

            # Salva arquivo atualizado
            write_cs_versions(file_path, sorted_versions, comp_name, comp_id)

            print_colored(f"Arquivo atualizado com {len(all_versions)} versões (ordem crescente)", "green")
        else:
            if invalid_urls > 0:
                # Mesmo sem novas versões, salva se removeu URLs inválidas
                sorted_versions = sort_versions(cs_content)
                create_backup(file_path)
                
                # Extrai informações do componente
                comp_name, comp_id = get_cs_component_info(file_path)
                
                write_cs_versions(file_path, sorted_versions, comp_name, comp_id)
                print_colored("Arquivo atualizado (removidas URLs inválidas, ordem crescente)", "green")

    except Exception as e:
        print_colored(f"Erro ao processar {component_name}: {e}", "red")

async def main():
    """Função principal assíncrona"""
    parser = argparse.ArgumentParser(
        description="DevStack Version Manager - Python Version",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Exemplos:
  python update_versions.py --check-only
  python update_versions.py --component php --check-only
  python update_versions.py --component php
  python update_versions.py --update-all
  python update_versions.py --clear-cache
  python update_versions.py --component php --clear-cache
  python update_versions.py --clear-backups
  python update_versions.py --component php --clear-backups
  python update_versions.py --show-backups
        """
    )

    parser.add_argument('--component', '-c', help='Componente específico para atualizar')
    parser.add_argument('--check-only', action='store_true', help='Apenas verifica URLs existentes sem atualizar')
    parser.add_argument('--update-all', action='store_true', help='Atualiza todos os componentes automaticamente')
    parser.add_argument('--clear-cache', action='store_true', help='Limpa o cache de versões falhadas')
    parser.add_argument('--clear-backups', action='store_true', help='Limpa backups antigos (mais de 30 dias)')
    parser.add_argument('--show-backups', action='store_true', help='Mostra informações dos backups')

    args = parser.parse_args()

    print_colored("=== DevStack Version Manager ===", "cyan")
    print_colored(f"Data: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}", "gray")

    # Verifica se a pasta existe
    if not AVAILABLE_VERSIONS_PATH.exists():
        print_colored(f"Pasta não encontrada: {AVAILABLE_VERSIONS_PATH}", "red")
        return

    # Obtém lista de componentes (arquivos CS)
    cs_files = list(AVAILABLE_VERSIONS_PATH.glob("*VersionProvider.cs"))
    cs_files = [f for f in cs_files if f.name not in ["IVersionProvider.cs", "VersionRegistry.cs"]]

    if not cs_files:
        print_colored(f"Nenhum arquivo CS de provider encontrado em: {AVAILABLE_VERSIONS_PATH}", "yellow")
        return

    print_colored("Componentes disponíveis:", "gray")
    for file in cs_files:
        # Extrai o nome do componente (ex: PhpVersionProvider -> php)
        component_name = file.stem.replace("VersionProvider", "").lower()
        print_colored(f"  - {component_name}", "gray")

    # Processa argumentos
    if args.clear_cache:
        # Limpeza de cache
        if args.component:
            # Cache de componente específico
            cache_file = CACHE_PATH / f"{args.component}-failed.json"
            if cache_file.exists():
                cache_file.unlink()
                print_colored(f"Cache de '{args.component}' removido com sucesso", "green")
            else:
                print_colored(f"Cache de '{args.component}' não encontrado", "yellow")
        else:
            # Todo o cache
            if CACHE_PATH.exists():
                cache_files = list(CACHE_PATH.glob("*-failed.json"))
                if cache_files:
                    for file in cache_files:
                        file.unlink()
                    print_colored(f"Todo o cache foi removido ({len(cache_files)} arquivos)", "green")
                else:
                    print_colored("Nenhum cache encontrado para remover", "gray")
            else:
                print_colored("Pasta de cache não existe", "gray")
        return

    elif args.clear_backups:
        # Limpeza de backups
        clear_old_backups_manual(args.component, 30)
        return

    elif args.show_backups:
        # Mostrar backups
        show_backup_info(args.component)
        return

    elif args.component:
        # Componente específico
        file = next((f for f in cs_files if f.stem.replace("VersionProvider", "").lower() == args.component.lower()), None)
        if file:
            component_name = file.stem.replace("VersionProvider", "").lower()
            await process_component(component_name, file, args.check_only)
        else:
            print_colored(f"Componente '{args.component}' não encontrado", "yellow")

    elif args.update_all:
        # Todos os componentes automaticamente
        for file in cs_files:
            component_name = file.stem.replace("VersionProvider", "").lower()
            await process_component(component_name, file, args.check_only)

    elif args.check_only:
        # Apenas verificação de todos os componentes
        for file in cs_files:
            component_name = file.stem.replace("VersionProvider", "").lower()
            await process_component(component_name, file, True)

    else:
        # Menu interativo
        while True:
            print_colored("\n=== Menu Principal ===", "cyan")
            print_colored("1. Verificar todos os componentes (apenas verificação)")
            print_colored("2. Verificar um componente específico (apenas verificação)")
            print_colored("3. Atualizar um componente específico")
            print_colored("4. Atualizar todos os componentes")
            print_colored("5. Gerenciar cache e backups")
            print_colored("6. Sair")

            try:
                choice = input("\nEscolha uma opção (1-6): ").strip()
            except KeyboardInterrupt:
                print_colored("\nSaindo...", "green")
                return

            if choice == "1":
                for file in cs_files:
                    component_name = file.stem.replace("VersionProvider", "").lower()
                    await process_component(component_name, file, True)
            elif choice == "2":
                print_colored("\nComponentes disponíveis:")
                for i, file in enumerate(cs_files):
                    component_name = file.stem.replace("VersionProvider", "").lower()
                    print_colored(f"{i + 1}. {component_name}")

                try:
                    component_choice = int(input(f"\nEscolha o componente (1-{len(cs_files)}): ")) - 1
                except ValueError:
                    print_colored("Escolha inválida", "red")
                    continue

                if 0 <= component_choice < len(cs_files):
                    selected_file = cs_files[component_choice]
                    component_name = selected_file.stem.replace("VersionProvider", "").lower()
                    await process_component(component_name, selected_file, True)
                else:
                    print_colored("Escolha inválida", "red")
            elif choice == "3":
                print_colored("\nComponentes disponíveis:")
                for i, file in enumerate(cs_files):
                    component_name = file.stem.replace("VersionProvider", "").lower()
                    print_colored(f"{i + 1}. {component_name}")

                try:
                    component_choice = int(input(f"\nEscolha o componente (1-{len(cs_files)}): ")) - 1
                except ValueError:
                    print_colored("Escolha inválida", "red")
                    continue

                if 0 <= component_choice < len(cs_files):
                    selected_file = cs_files[component_choice]
                    component_name = selected_file.stem.replace("VersionProvider", "").lower()
                    await process_component(component_name, selected_file, False)
                else:
                    print_colored("Escolha inválida", "red")
            elif choice == "4":
                confirm = input("\nTem certeza que deseja atualizar TODOS os componentes? (s/N): ").strip().lower()
                if confirm == "s":
                    for file in cs_files:
                        component_name = file.stem.replace("VersionProvider", "").lower()
                        await process_component(component_name, file, False)
            elif choice == "5":
                show_cache_management_menu()
            elif choice == "6":
                print_colored("Saindo...", "green")
                return
            else:
                print_colored("Opção inválida", "yellow")

    print_colored("\n=== Processamento concluído ===", "green")

if __name__ == "__main__":
    asyncio.run(main())
