namespace TreviaApp.Shared.Enums;

using System;
using System.ComponentModel;

/// <summary>
/// Defines the CoachPermissions enumeration.
/// </summary>
[Flags]
public enum CoachPermissions
{
    /// <summary>
    /// Nenhuma permissÃ£o adicional
    /// </summary>
    [Description("Nenhuma permissÃ£o adicional")]
    None = 0,

    /// <summary>
    /// Visualizar histÃ³rico de peso do aluno
    /// </summary>
    [Description("Visualizar histÃ³rico de peso do aluno")]
    CanViewWeightHistory = 1 << 0,

    /// <summary>
    /// Visualizar medidas corporais
    /// </summary>
    [Description("Visualizar medidas corporais")]
    CanViewBodyMeasurements = 1 << 1,

    /// <summary>
    /// Visualizar fotos do perfil
    /// </summary>
    [Description("Visualizar fotos do perfil")]
    CanViewProfilePhotos = 1 << 2,

    /// <summary>
    /// Atribuir fichas de treino
    /// </summary>
    [Description("Atribuir fichas de treino")]
    CanAssignTrainingPlans = 1 << 3,

    /// <summary>
    /// Visualizar histÃ³rico de treinos executados
    /// </summary>
    [Description("Visualizar histÃ³rico de treinos executados")]
    CanViewWorkoutHistory = 1 << 4,

    /// <summary>
    /// Enviar feedback em treinos
    /// </summary>
    [Description("Enviar feedback em treinos")]
    CanSendFeedback = 1 << 5,

    /// <summary>
    /// Visualizar dados de avaliaÃ§Ãµes fÃ­sicas
    /// </summary>
    [Description("Visualizar dados de avaliaÃ§Ãµes fÃ­sicas")]
    CanViewAssessments = 1 << 6,

    /// <summary>
    /// Convidar para grupos
    /// </summary>
    [Description("Convidar para grupos")]
    CanInviteToGroups = 1 << 7
}

/// <summary>
/// Defines the CoachRelationshipEndReason enumeration.
/// </summary>
public enum CoachRelationshipEndReason
{
    /// <summary>
    /// Cancelado por mÃºtuo acordo
    /// </summary>
    [Description("Cancelado por mÃºtuo acordo")]
    MutualAgreement = 0,

    /// <summary>
    /// Encerrado pelo professor
    /// </summary>
    [Description("Encerrado pelo professor")]
    EndedByCoach = 1,

    /// <summary>
    /// Encerrado pelo aluno
    /// </summary>
    [Description("Encerrado pelo aluno")]
    EndedByStudent = 2,

    /// <summary>
    /// Encerrado por administrador
    /// </summary>
    [Description("Encerrado por administrador")]
    EndedByAdmin = 3,

    /// <summary>
    /// PerÃ­odo de contrataÃ§Ã£o expirado
    /// </summary>
    [Description("PerÃ­odo de contrataÃ§Ã£o expirado")]
    Expired = 4,

    /// <summary>
    /// Outro motivo
    /// </summary>
    [Description("Outro motivo")]
    Other = 99
}

/// <summary>
/// Defines the CoachInviteDirection enumeration.
/// </summary>
public enum CoachInviteDirection
{
    /// <summary>
    /// Professor convidou aluno
    /// </summary>
    [Description("Professor convidou aluno")]
    CoachToStudent = 0,

    /// <summary>
    /// Aluno solicitou ao professor
    /// </summary>
    [Description("Aluno solicitou ao professor")]
    StudentToCoach = 1
}
