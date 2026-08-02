namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

/// <summary>
/// Defines the TrainingPlanStatus enumeration.
/// </summary>
public enum TrainingPlanStatus
{
    /// <summary>
    /// Rascunho â€” ainda nÃ£o atribuÃ­do nem publicado
    /// </summary>
    [Description("Rascunho â€” ainda nÃ£o atribuÃ­do nem publicado")]
    Draft = 0,

    /// <summary>
    /// AtribuÃ­do ao aluno
    /// </summary>
    [Description("AtribuÃ­do ao aluno")]
    Assigned = 1,

    /// <summary>
    /// Ativo â€” atribuÃ­do e em andamento
    /// </summary>
    [Description("Ativo â€” atribuÃ­do e em andamento")]
    Active = 2,

    /// <summary>
    /// ConcluÃ­do pelo aluno
    /// </summary>
    [Description("ConcluÃ­do pelo aluno")]
    Completed = 3,

    /// <summary>
    /// Pausado pelo professor
    /// </summary>
    [Description("Pausado pelo professor")]
    Paused = 4,

    /// <summary>
    /// Cancelado/revogado pelo professor
    /// </summary>
    [Description("Cancelado/revogado pelo professor")]
    Cancelled = 5,

    /// <summary>
    /// Publicado â€” disponÃ­vel como template (se pÃºblico)
    /// </summary>
    [Description("Publicado â€” disponÃ­vel como template (se pÃºblico)")]
    Published = 6,

    /// <summary>
    /// Arquivado
    /// </summary>
    [Description("Arquivado")]
    Archived = 7
}
