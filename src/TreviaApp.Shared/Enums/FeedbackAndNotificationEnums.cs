namespace TreviaApp.Shared.Enums;

using System.ComponentModel;

/// <summary>
/// Tom/sentimento do feedback enviado pelo professor.
/// </summary>
public enum FeedbackTone
{
    [Description("Neutro")]
    Neutral = 0,

    [Description("Incentivo")]
    Encouragement = 1,

    [Description("Construtivo")]
    Constructive = 2,

    [Description("Correção técnica")]
    TechnicalCorrection = 3
}

/// <summary>
/// Tipo de notificação interna persistida no banco.
/// </summary>
public enum NotificationType
{
    [Description("Feedback recebido")]
    FeedbackReceived = 0,

    [Description("Treino concluído")]
    WorkoutCompleted = 1,

    [Description("Ficha atribuída")]
    PlanAssigned = 2,

    [Description("Vínculo aceito")]
    LinkAccepted = 3,

    [Description("Vínculo encerrado")]
    LinkRevoked = 4,

    [Description("Mensagem do professor")]
    CoachMessage = 5
}

/// <summary>
/// Tipo de entidade referenciada em uma notificação.
/// </summary>
public enum NotificationReferenceType
{
    [Description("Sessão de treino")]
    WorkoutSession = 0,

    [Description("Exercício do treino")]
    WorkoutExercise = 1,

    [Description("Série do treino")]
    WorkoutSet = 2,

    [Description("Ficha de treino")]
    TrainingPlan = 3,

    [Description("Vínculo professor-aluno")]
    CoachStudentLink = 4,

    [Description("Feedback")]
    Feedback = 5
}

/// <summary>
/// Nível de granularidade de um feedback (para exibição e filtros).
/// </summary>
public enum FeedbackLevel
{
    [Description("Sessão de treino")]
    Session = 0,

    [Description("Exercício")]
    Exercise = 1,

    [Description("Série")]
    Set = 2
}
