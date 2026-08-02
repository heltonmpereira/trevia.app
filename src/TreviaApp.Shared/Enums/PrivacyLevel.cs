using System.ComponentModel;

namespace TreviaApp.Shared.Enums;

/// <summary>
/// Defines the PrivacyLevel enumeration.
/// </summary>
public enum PrivacyLevel
{
    /// <summary>
    /// Privado â€” apenas dono/admin/trainer vinculado
    /// </summary>
    [Description("Privado â€” apenas dono/admin/trainer vinculado")]
    Private = 0,

    /// <summary>
    /// Parcial â€” amigos/trainer
    /// </summary>
    [Description("Parcial â€” amigos/trainer")]
    FriendsOnly = 1,

    /// <summary>
    /// PÃºblico â€” qualquer usuÃ¡rio logado
    /// </summary>
    [Description("PÃºblico â€” qualquer usuÃ¡rio logado")]
    Public = 2
}
