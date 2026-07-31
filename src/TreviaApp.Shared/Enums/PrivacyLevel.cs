using System.ComponentModel;

namespace TreviaApp.Shared.Enums;

public enum PrivacyLevel
{
    [Description("Privado — apenas dono/admin/trainer vinculado")]
    Private = 0,

    [Description("Parcial — amigos/trainer")]
    FriendsOnly = 1,

    [Description("Público — qualquer usuário logado")]
    Public = 2
}
