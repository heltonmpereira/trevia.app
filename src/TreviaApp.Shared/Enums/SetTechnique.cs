namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

/// <summary>
/// Defines the SetTechnique enumeration.
/// </summary>
public enum SetTechnique
{
    /// <summary>
    /// Standard â€” sÃ©rie padrÃ£o
    /// </summary>
    [Description("Standard â€” sÃ©rie padrÃ£o")]
    Standard = 0,

    /// <summary>
    /// Superset (2 exercÃ­cios sem descanso entre si)
    /// </summary>
    [Description("Superset (2 exercÃ­cios sem descanso entre si)")]
    SuperSet = 1,

    /// <summary>
    /// Circuito
    /// </summary>
    [Description("Circuito")]
    Circuit = 2,

    /// <summary>
    /// Drop-set (reduzir carga imediatamente apÃ³s falha)
    /// </summary>
    [Description("Drop-set (reduzir carga imediatamente apÃ³s falha)")]
    DropSet = 3,

    /// <summary>
    /// Rest-pause (pausas curtas entre repetiÃ§Ãµes)
    /// </summary>
    [Description("Rest-pause (pausas curtas entre repetiÃ§Ãµes)")]
    RestPause = 4,

    /// <summary>
    /// Giant-set (4+ exercÃ­cios)
    /// </summary>
    [Description("Giant-set (4+ exercÃ­cios)")]
    GiantSet = 5,

    /// <summary>
    /// PrÃ©-exaustÃ£o
    /// </summary>
    [Description("PrÃ©-exaustÃ£o")]
    PreExhaust = 6,

    /// <summary>
    /// Cluster sets (sÃ©ries divididas em blocos)
    /// </summary>
    [Description("Cluster sets (sÃ©ries divididas em blocos)")]
    Cluster = 7,

    /// <summary>
    /// Drop-set mecÃ¢nico (mudanÃ§a de Ã¢ngulo/aparelho)
    /// </summary>
    [Description("Drop-set mecÃ¢nico (mudanÃ§a de Ã¢ngulo/aparelho)")]
    MechanicalDropSet = 8,

    /// <summary>
    /// Sobrecarga excÃªntrica
    /// </summary>
    [Description("Sobrecarga excÃªntrica")]
    EccentricOverload = 9,

    /// <summary>
    /// Normal â€” sem tÃ©cnica especial
    /// </summary>
    [Description("Normal â€” sem tÃ©cnica especial")]
    Normal = 10,

    /// <summary>
    /// Bi-set (2 exercÃ­cios mesmo grupamento muscular)
    /// </summary>
    [Description("Bi-set (2 exercÃ­cios mesmo grupamento muscular)")]
    BiSet = 11,

    /// <summary>
    /// Tri-set (3 exercÃ­cios sem descanso)
    /// </summary>
    [Description("Tri-set (3 exercÃ­cios sem descanso)")]
    TriSet = 12,

    /// <summary>
    /// Piramidal crescente/decrescente
    /// </summary>
    [Description("Piramidal crescente/decrescente")]
    Pyramid = 13,

    /// <summary>
    /// FST-7 (7 sÃ©ries de 8-12 repetiÃ§Ãµes)
    /// </summary>
    [Description("FST-7 (7 sÃ©ries de 8-12 repetiÃ§Ãµes)")]
    FST7 = 14,

    /// <summary>
    /// RepetiÃ§Ãµes forÃ§adas
    /// </summary>
    [Description("RepetiÃ§Ãµes forÃ§adas")]
    ForcedReps = 15,

    /// <summary>
    /// Negativas (fase excÃªntrica lenta)
    /// </summary>
    [Description("Negativas (fase excÃªntrica lenta)")]
    Negatives = 16,

    /// <summary>
    /// Falha concÃªntrica
    /// </summary>
    [Description("Falha concÃªntrica")]
    Failure = 17,

    /// <summary>
    /// Meias repetiÃ§Ãµes / 21s
    /// </summary>
    [Description("Meias repetiÃ§Ãµes / 21s")]
    PartialReps = 18,

    /// <summary>
    /// Tempo sob tensÃ£o (TUT)
    /// </summary>
    [Description("Tempo sob tensÃ£o (TUT)")]
    TimeUnderTension = 19,

    /// <summary>
    /// AMRAP (tantas repetiÃ§Ãµes quanto possÃ­vel no conjunto)
    /// </summary>
    [Description("AMRAP (tantas repetiÃ§Ãµes quanto possÃ­vel no conjunto)")]
    AMRAPSet = 20
}
