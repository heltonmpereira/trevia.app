namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

/// <summary>
/// Defines the TrainingSplitType enumeration.
/// </summary>
public enum TrainingSplitType
{
    /// <summary>
    /// Personalizado (definido pelo professor)
    /// </summary>
    [Description("Personalizado (definido pelo professor)")]
    Custom = 0,

    /// <summary>
    /// Full body (corpo inteiro por sessÃ£o)
    /// </summary>
    [Description("Full body (corpo inteiro por sessÃ£o)")]
    FullBody = 1,

    /// <summary>
    /// Upper / Lower (2 dias)
    /// </summary>
    [Description("Upper / Lower (2 dias)")]
    UpperLower = 2,

    /// <summary>
    /// Push / Pull / Legs (3 dias)
    /// </summary>
    [Description("Push / Pull / Legs (3 dias)")]
    PushPullLegs = 3,

    /// <summary>
    /// A / B / C (3 dias)
    /// </summary>
    [Description("A / B / C (3 dias)")]
    ABC = 4,

    /// <summary>
    /// A / B / C / D (4 dias)
    /// </summary>
    [Description("A / B / C / D (4 dias)")]
    ABCD = 5,

    /// <summary>
    /// A / B / C / D / E (5 dias)
    /// </summary>
    [Description("A / B / C / D / E (5 dias)")]
    ABCDE = 6,

    /// <summary>
    /// Bro split: Peito / Costas / Ombros / BraÃ§os / Pernas (5 dias)
    /// </summary>
    [Description("Bro split: Peito / Costas / Ombros / BraÃ§os / Pernas (5 dias)")]
    BroSplit = 7,

    /// <summary>
    /// Arnold split: 2x corpo por semana com variaÃ§Ã£o
    /// </summary>
    [Description("Arnold split: 2x corpo por semana com variaÃ§Ã£o")]
    ArnoldSplit = 8,

    /// <summary>
    /// Funcional diÃ¡rio (HIIT/Circuit)
    /// </summary>
    [Description("Funcional diÃ¡rio (HIIT/Circuit)")]
    DailyFunctional = 9,

    /// <summary>
    /// Cardio + HIIT
    /// </summary>
    [Description("Cardio + HIIT")]
    CardioOnly = 10,

    /// <summary>
    /// Yoga / Mobilidade
    /// </summary>
    [Description("Yoga / Mobilidade")]
    MobilityYoga = 11
}
