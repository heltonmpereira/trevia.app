namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

public enum TrainingSplitType
{
    [Description("Personalizado (definido pelo professor)")]
    Custom = 0,

    [Description("Full body (corpo inteiro por sessão)")]
    FullBody = 1,

    [Description("Upper / Lower (2 dias)")]
    UpperLower = 2,

    [Description("Push / Pull / Legs (3 dias)")]
    PushPullLegs = 3,

    [Description("A / B / C (3 dias)")]
    ABC = 4,

    [Description("A / B / C / D (4 dias)")]
    ABCD = 5,

    [Description("A / B / C / D / E (5 dias)")]
    ABCDE = 6,

    [Description("Bro split: Peito / Costas / Ombros / Braços / Pernas (5 dias)")]
    BroSplit = 7,

    [Description("Arnold split: 2x corpo por semana com variação")]
    ArnoldSplit = 8,

    [Description("Funcional diário (HIIT/Circuit)")]
    DailyFunctional = 9,

    [Description("Cardio + HIIT")]
    CardioOnly = 10,

    [Description("Yoga / Mobilidade")]
    MobilityYoga = 11
}
