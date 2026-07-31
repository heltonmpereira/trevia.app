namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

public enum SetTechnique
{
    [Description("Standard — série padrão")]
    Standard = 0,

    [Description("Superset (2 exercícios sem descanso entre si)")]
    SuperSet = 1,

    [Description("Circuito")]
    Circuit = 2,

    [Description("Drop-set (reduzir carga imediatamente após falha)")]
    DropSet = 3,

    [Description("Rest-pause (pausas curtas entre repetições)")]
    RestPause = 4,

    [Description("Giant-set (4+ exercícios)")]
    GiantSet = 5,

    [Description("Pré-exaustão")]
    PreExhaust = 6,

    [Description("Cluster sets (séries divididas em blocos)")]
    Cluster = 7,

    [Description("Drop-set mecânico (mudança de ângulo/aparelho)")]
    MechanicalDropSet = 8,

    [Description("Sobrecarga excêntrica")]
    EccentricOverload = 9,

    [Description("Normal — sem técnica especial")]
    Normal = 10,

    [Description("Bi-set (2 exercícios mesmo grupamento muscular)")]
    BiSet = 11,

    [Description("Tri-set (3 exercícios sem descanso)")]
    TriSet = 12,

    [Description("Piramidal crescente/decrescente")]
    Pyramid = 13,

    [Description("FST-7 (7 séries de 8-12 repetições)")]
    FST7 = 14,

    [Description("Repetições forçadas")]
    ForcedReps = 15,

    [Description("Negativas (fase excêntrica lenta)")]
    Negatives = 16,

    [Description("Falha concêntrica")]
    Failure = 17,

    [Description("Meias repetições / 21s")]
    PartialReps = 18,

    [Description("Tempo sob tensão (TUT)")]
    TimeUnderTension = 19,

    [Description("AMRAP (tantas repetições quanto possível no conjunto)")]
    AMRAPSet = 20
}
