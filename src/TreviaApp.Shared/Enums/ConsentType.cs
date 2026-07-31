using System.ComponentModel;

namespace TreviaApp.Shared.Enums;

public enum ConsentType
{
    [Description("Termos de Serviço")]
    TermsOfService = 0,

    [Description("Política de Privacidade")]
    PrivacyPolicy = 1,

    [Description("Comunicações de Marketing")]
    MarketingCommunications = 2,

    [Description("Tratamento de Dados")]
    DataProcessing = 3,

    [Description("Tratamento de Dados de Saúde")]
    HealthDataProcessing = 4,

    [Description("Comunicação de Marketing (singular)")]
    MarketingCommunication = 5,

    [Description("Compartilhamento com Terceiros")]
    ThirdPartySharing = 6,

    [Description("Preferências de Cookies")]
    CookiePreferences = 7
}
