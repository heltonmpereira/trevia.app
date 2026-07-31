namespace TreviaApp.Application.Exercises.Commands.AddMediaToExercise;

using FluentValidation;
using TreviaApp.Shared.Enums;

public sealed class AddMediaToExerciseCommandValidator : AbstractValidator<AddMediaToExerciseCommand>
{
    private const long MaxImageBytes = 5 * 1024 * 1024;
    private const long MaxVideoBytes = 200 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif",
        "video/mp4", "video/webm"
    };

    public AddMediaToExerciseCommandValidator()
    {
        RuleFor(x => x.ExerciseId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
        RuleFor(x => x.ContentType)
            .NotEmpty()
            .Must(x => AllowedContentTypes.Contains(x))
            .WithMessage("Tipo de arquivo não permitido.");

        RuleFor(x => x.SizeBytes)
            .GreaterThan(0)
            .WithMessage("Tamanho do arquivo deve ser maior que zero.");

        RuleFor(x => x)
            .Must(x =>
            {
                var isImage = x.MediaType == MediaType.Image;
                var maxSize = isImage ? MaxImageBytes : MaxVideoBytes;
                return x.SizeBytes <= maxSize;
            })
            .WithMessage("Tamanho do arquivo excede o limite permitido.");

        RuleFor(x => x.MediaType).IsInEnum();
        RuleFor(x => x.Order).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Caption).MaximumLength(500);
    }
}
