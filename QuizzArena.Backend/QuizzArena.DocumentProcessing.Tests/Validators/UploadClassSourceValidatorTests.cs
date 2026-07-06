using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Validators;

namespace QuizzArena.DocumentProcessing.Tests.Validators;

public class UploadClassSourceValidatorTests
{
    private readonly UploadClassSourceRequestValidator _validator = new();

    [Fact]
    public async Task Name_Empty_ShouldFailValidation()
    {
        var result = await _validator.TestValidateAsync(CreateDto(name: ""));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Name_TooShort_ShouldFailValidation()
    {
        var result = await _validator.TestValidateAsync(CreateDto(name: "A"));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Name_TooLong_ShouldFailValidation()
    {
        var result = await _validator.TestValidateAsync(CreateDto(name: new string('A', 301)));
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Name_Valid_ShouldPassValidation()
    {
        var result = await _validator.TestValidateAsync(CreateDto(name: "Class Test"));
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task CourseId_Empty_ShouldFailValidation()
    {
        var result = await _validator.TestValidateAsync(CreateDto(courseId: Guid.Empty));
        result.ShouldHaveValidationErrorFor(x => x.CourseId);
    }

    [Theory]
    [InlineData(".mp3")]
    [InlineData(".mp4")]
    [InlineData(".wav")]
    public async Task File_ValidExtension_ShouldPassValidation(string extension)
    {
        var result = await _validator.TestValidateAsync(CreateDto(fileName: $"class{extension}"));
        result.ShouldNotHaveValidationErrorFor(x => x.File.FileName);
    }

    [Theory]
    [InlineData(".pdf")]
    [InlineData(".txt")]
    [InlineData(".docx")]
    public async Task File_InvalidExtension_ShouldFailValidation(string extension)
    {
        var result = await _validator.TestValidateAsync(CreateDto(fileName: $"class{extension}"));
        result.ShouldHaveValidationErrorFor(x => x.File.FileName);
    }

    [Fact]
    public async Task File_EmptySize_ShouldFailValidation()
    {
        var result = await _validator.TestValidateAsync(CreateDto(fileLength: 0));
        result.ShouldHaveValidationErrorFor(x => x.File);
    }

    [Fact]
    public async Task File_ExceedsMaxSize_ShouldFailValidation()
    {
        var result = await _validator.TestValidateAsync(CreateDto(fileLength: 500_000_001));
        result.ShouldHaveValidationErrorFor(x => x.File.Length);
    }

    private static UploadClassSourceRequestDto CreateDto(
        string name = "Class Test",
        Guid? courseId = null,
        string fileName = "class.mp3",
        long fileLength = 1024)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(fileLength);

        return new UploadClassSourceRequestDto
        {
            Name = name,
            CourseId = courseId ?? Guid.NewGuid(),
            File = mockFile.Object
        };
    }
}
