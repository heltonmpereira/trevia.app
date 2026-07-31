namespace TreviaApp.Application.Consents.Commands.GiveConsentBatch;
using FluentValidation;

public class GiveConsentBatchCommandValidator : AbstractValidator<GiveConsentBatchCommand>
{
    public GiveConsentBatchCommandValidator()
    {
        RuleFor(x => x.Consents)
            .NotEmpty()
            .Must(c => c.Count() >= 1 && c.Count() <= 50)
            .WithMessage("A lista de consentimentos deve conter entre 1 e 50 itens.");

        RuleForEach(x => x.Consents).ChildRules(c =>
        {
            c.RuleFor(x => x.ConsentType)
                .IsInEnum()
                .WithMessage("Tipo de consentimento inválido.");

            c.RuleFor(x => x.ConsentVersion)
                .NotEmpty()
                .MaximumLength(20)
                .Matches(@"^\d+\.\d+(\.\d+)?$")
                .WithMessage("Versão deve estar no formato SemVer (ex: 1.0, 1.0.0).");
        });
    }
}
