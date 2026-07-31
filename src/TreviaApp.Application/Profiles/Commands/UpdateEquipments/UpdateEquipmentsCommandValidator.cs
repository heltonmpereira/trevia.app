namespace TreviaApp.Application.Profiles.Commands.UpdateEquipments;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class UpdateEquipmentsCommandValidator : AbstractValidator<UpdateEquipmentsCommand>
{
    public UpdateEquipmentsCommandValidator()
    {
        RuleFor(x => x.Equipments)
            .NotNull()
            .Must(eqs => eqs.Count() < 100)
            .WithMessage("Número máximo de equipamentos é 99.");

        RuleForEach(x => x.Equipments)
            .IsInEnum()
            .WithMessage("Equipamento inválido.");
    }
}
