namespace TreviaApp.Shared.Enums;

using System;
using System.ComponentModel;

[Flags]
public enum CoachPermissions
{
    [Description("Nenhuma permissão adicional")]
    None = 0,

    [Description("Visualizar histórico de peso do aluno")]
    CanViewWeightHistory = 1 << 0,

    [Description("Visualizar medidas corporais")]
    CanViewBodyMeasurements = 1 << 1,

    [Description("Visualizar fotos do perfil")]
    CanViewProfilePhotos = 1 << 2,

    [Description("Atribuir fichas de treino")]
    CanAssignTrainingPlans = 1 << 3,

    [Description("Visualizar histórico de treinos executados")]
    CanViewWorkoutHistory = 1 << 4,

    [Description("Enviar feedback em treinos")]
    CanSendFeedback = 1 << 5,

    [Description("Visualizar dados de avaliações físicas")]
    CanViewAssessments = 1 << 6,

    [Description("Convidar para grupos")]
    CanInviteToGroups = 1 << 7
}

public enum CoachRelationshipEndReason
{
    [Description("Cancelado por mútuo acordo")]
    MutualAgreement = 0,

    [Description("Encerrado pelo professor")]
    EndedByCoach = 1,

    [Description("Encerrado pelo aluno")]
    EndedByStudent = 2,

    [Description("Encerrado por administrador")]
    EndedByAdmin = 3,

    [Description("Período de contratação expirado")]
    Expired = 4,

    [Description("Outro motivo")]
    Other = 99
}

public enum CoachInviteDirection
{
    [Description("Professor convidou aluno")]
    CoachToStudent = 0,

    [Description("Aluno solicitou ao professor")]
    StudentToCoach = 1
}
