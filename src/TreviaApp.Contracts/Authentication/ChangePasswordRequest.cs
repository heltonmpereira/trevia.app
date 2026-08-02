namespace TreviaApp.Contracts.Authentication;

/// <summary>
/// Represents the ChangePasswordRequest contract.
/// </summary>
/// <param name="CurrentPassword">Current Password value.</param>
/// <param name="NewPassword">New Password value.</param>
/// <param name="ConfirmNewPassword">Confirm New Password value.</param>
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);
