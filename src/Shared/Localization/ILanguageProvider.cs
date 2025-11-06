using System.Collections.Generic;

namespace DevStackShared.Localization
{
    /// <summary>
    /// Interface para provedores de tradução por idioma
    /// </summary>
    public interface ILanguageProvider
    {
        /// <summary>
        /// Código do idioma (ex: pt_BR, en_US)
        /// </summary>
        string LanguageCode { get; }

        /// <summary>
        /// Nome do idioma (ex: Português (Brasil))
        /// </summary>
        string LanguageName { get; }

        /// <summary>
        /// Retorna as traduções comuns (botões, temas, etc)
        /// </summary>
        Dictionary<string, object> GetCommonTranslations();

        /// <summary>
        /// Retorna as traduções compartilhadas entre aplicações
        /// </summary>
        Dictionary<string, object> GetSharedTranslations();

        /// <summary>
        /// Retorna as traduções específicas da GUI
        /// </summary>
        Dictionary<string, object> GetGuiTranslations();

        /// <summary>
        /// Retorna as traduções específicas do Instalador
        /// </summary>
        Dictionary<string, object> GetInstallerTranslations();

        /// <summary>
        /// Retorna as traduções específicas do Desinstalador
        /// </summary>
        Dictionary<string, object> GetUninstallerTranslations();

        /// <summary>
        /// Retorna todas as traduções em um único dicionário
        /// </summary>
        Dictionary<string, object> GetAllTranslations();
    }
}
