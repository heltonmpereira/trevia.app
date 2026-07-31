namespace TreviaApp.Application.Consents.Commands.RevokeConsent;
using FluentValidation;

public class RevokeConsentCommandValidator : AbstractValidator<RevokeConsentCommand>
{
    public RevokeConsentCommandValidator()
    {
        RuleFor(x => x.ConsentType)
            .IsInEnum()
            .WithMessage("Tipo de consentimento inválido.");

        RuleFor(x => x.Reason)
            .MaximumLength(200)
            .WithMessage("O motivo deve ter no máximo 200 caracteres.");
    }
}
