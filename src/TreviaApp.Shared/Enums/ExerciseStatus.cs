namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

public enum ExerciseStatus
{
    [Description("Rascunho — criado, não enviado para moderação")]
    Draft = 0,

    [Description("Aguardando aprovação do administrador")]
    AwaitingApproval = 1,

    [Description("Aprovado — disponível globalmente")]
    Approved = 2,

    [Description("Reprovado — com motivo")]
    Rejected = 3,

    [Description("Arquivado")]
    Archived = 4
}
