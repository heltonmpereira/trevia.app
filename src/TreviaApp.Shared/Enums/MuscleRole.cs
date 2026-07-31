namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

public enum MuscleRole
{
    [Description("Agonista (musículo principal)")]
    Primary = 1,

    [Description("Sinergista auxiliar")]
    Secondary = 2,

    [Description("Estabilizador")]
    Stabilizer = 3,

    [Description("Sinergista (assistente)")]
    Synergist = 4,

    [Description("Antagonista")]
    Antagonist = 5
}
