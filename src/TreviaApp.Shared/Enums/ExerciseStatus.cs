namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

/// <summary>
/// Defines the ExerciseStatus enumeration.
/// </summary>
public enum ExerciseStatus
{
    /// <summary>
    /// Rascunho â€” criado, nÃ£o enviado para moderaÃ§Ã£o
    /// </summary>
    [Description("Rascunho â€” criado, nÃ£o enviado para moderaÃ§Ã£o")]
    Draft = 0,

    /// <summary>
    /// Aguardando aprovaÃ§Ã£o do administrador
    /// </summary>
    [Description("Aguardando aprovaÃ§Ã£o do administrador")]
    AwaitingApproval = 1,

    /// <summary>
    /// Aprovado â€” disponÃ­vel globalmente
    /// </summary>
    [Description("Aprovado â€” disponÃ­vel globalmente")]
    Approved = 2,

    /// <summary>
    /// Reprovado â€” com motivo
    /// </summary>
    [Description("Reprovado â€” com motivo")]
    Rejected = 3,

    /// <summary>
    /// Arquivado
    /// </summary>
    [Description("Arquivado")]
    Archived = 4
}
