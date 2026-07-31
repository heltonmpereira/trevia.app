namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

public enum TrainingPlanStatus
{
    [Description("Rascunho — ainda não atribuído nem publicado")]
    Draft = 0,

    [Description("Atribuído ao aluno")]
    Assigned = 1,

    [Description("Ativo — atribuído e em andamento")]
    Active = 2,

    [Description("Concluído pelo aluno")]
    Completed = 3,

    [Description("Pausado pelo professor")]
    Paused = 4,

    [Description("Cancelado/revogado pelo professor")]
    Cancelled = 5,

    [Description("Publicado — disponível como template (se público)")]
    Published = 6,

    [Description("Arquivado")]
    Archived = 7
}
