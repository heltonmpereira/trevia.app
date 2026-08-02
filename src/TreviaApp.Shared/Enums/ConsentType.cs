using System.ComponentModel;

namespace TreviaApp.Shared.Enums;

/// <summary>
/// Defines the ConsentType enumeration.
/// </summary>
public enum ConsentType
{
    /// <summary>
    /// Termos de ServiÃ§o
    /// </summary>
    [Description("Termos de ServiÃ§o")]
    TermsOfService = 0,

    /// <summary>
    /// PolÃ­tica de Privacidade
    /// </summary>
    [Description("PolÃ­tica de Privacidade")]
    PrivacyPolicy = 1,

    /// <summary>
    /// ComunicaÃ§Ãµes de Marketing
    /// </summary>
    [Description("ComunicaÃ§Ãµes de Marketing")]
    MarketingCommunications = 2,

    /// <summary>
    /// Tratamento de Dados
    /// </summary>
    [Description("Tratamento de Dados")]
    DataProcessing = 3,

    /// <summary>
    /// Tratamento de Dados de SaÃºde
    /// </summary>
    [Description("Tratamento de Dados de SaÃºde")]
    HealthDataProcessing = 4,

    /// <summary>
    /// ComunicaÃ§Ã£o de Marketing (singular)
    /// </summary>
    [Description("ComunicaÃ§Ã£o de Marketing (singular)")]
    MarketingCommunication = 5,

    /// <summary>
    /// Compartilhamento com Terceiros
    /// </summary>
    [Description("Compartilhamento com Terceiros")]
    ThirdPartySharing = 6,

    /// <summary>
    /// PreferÃªncias de Cookies
    /// </summary>
    [Description("PreferÃªncias de Cookies")]
    CookiePreferences = 7
}
