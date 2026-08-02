namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

/// <summary>
/// Defines the MuscleRole enumeration.
/// </summary>
public enum MuscleRole
{
    /// <summary>
    /// Agonista (musÃ­culo principal)
    /// </summary>
    [Description("Agonista (musÃ­culo principal)")]
    Primary = 1,

    /// <summary>
    /// Sinergista auxiliar
    /// </summary>
    [Description("Sinergista auxiliar")]
    Secondary = 2,

    /// <summary>
    /// Estabilizador
    /// </summary>
    [Description("Estabilizador")]
    Stabilizer = 3,

    /// <summary>
    /// Sinergista (assistente)
    /// </summary>
    [Description("Sinergista (assistente)")]
    Synergist = 4,

    /// <summary>
    /// Antagonista
    /// </summary>
    [Description("Antagonista")]
    Antagonist = 5
}
