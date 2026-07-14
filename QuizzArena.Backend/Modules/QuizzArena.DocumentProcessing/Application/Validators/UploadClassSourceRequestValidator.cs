
using FluentValidation;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;

namespace QuizzArena.DocumentProcessing.Application.Validators;

public class UploadClassSourceRequestValidator : AbstractValidator<UploadClassSourceRequestDto>
{

    private const string Mp3Extension = ".mp3";
    private const string Mp4Extension = ".mp4";
    private const string WavExtension = ".wav";
    private const string TxtExtension = ".txt";

    private static readonly string[] _allowedExtensions =
    [
        Mp3Extension,
        Mp4Extension,
        WavExtension,
        TxtExtension
    ];

    public UploadClassSourceRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(300);

        RuleFor(x => x.CourseId)
            .NotEmpty()
            ;

        RuleFor(x => x.File)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .Must(file => file.Length > 0)
            .WithMessage("File cannot be empty.");

        RuleFor(x => x.File.FileName)
            .Must(HaveValidExtension)
            .WithMessage("Invalid file extension.");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(500_000_000)
            .WithMessage("File size cannot exceed 500MB.");
    }

    private static bool HaveValidExtension(string fileName)
    {
        string extension = Path.GetExtension(fileName);

        return _allowedExtensions.Contains(
            extension,
            StringComparer.OrdinalIgnoreCase
        );
    }
}
