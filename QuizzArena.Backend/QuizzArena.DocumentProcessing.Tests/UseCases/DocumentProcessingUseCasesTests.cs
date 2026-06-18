using AutoMapper;
using FluentAssertions;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Moq;
using QuizzArena.DocumentProcessing.Application.DTOs.ClassSource;
using QuizzArena.DocumentProcessing.Application.Messaging.Events;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Application.UseCases;
using QuizzArena.DocumentProcessing.Application.Validators;
using QuizzArena.DocumentProcessing.Domain.Entities;

namespace QuizzArena.DocumentProcessing.Tests.UseCases;

public class DocumentProcessingUseCasesTests
{
    //Mocks
    private readonly Mock<IClassSourceRepository> _mockClassSourceRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IStorageServiceRepository> _mockStorageServiceRepository;
    private readonly Mock<IPublishEndpoint> _mockPublishEndpoint;

    //Real
    private readonly UploadClassSourceRequestValidator _uploadClassSourceValidator;

    //target unit test
    private readonly UploadSourceUseCase _uploadClassSource;

    public DocumentProcessingUseCasesTests()
    {
        _mockClassSourceRepository = new Mock<IClassSourceRepository>();
        _mockMapper = new Mock<IMapper>();
        _mockStorageServiceRepository = new Mock<IStorageServiceRepository>();
        _mockPublishEndpoint = new Mock<IPublishEndpoint>();
        _uploadClassSourceValidator = new UploadClassSourceRequestValidator();

        _uploadClassSource = new UploadSourceUseCase(
            _uploadClassSourceValidator,
            _mockMapper.Object,
            _mockStorageServiceRepository.Object,
            _mockClassSourceRepository.Object,
            _mockPublishEndpoint.Object);
    }

    [Fact]
    public async Task Execute_ValidClassSource_CallsRepositoryAndPublishesEvent()
    {
        // Arrange
        var dto = ValidClassSourceDto();
        var classSource = new ClassSource { Id = Guid.NewGuid(), FileUrl = "https://storage/class.mp3" };
        var response = new UploadClassSourceResponseDto { Id = classSource.Id };

        _mockMapper.Setup(m => m.Map<ClassSource>(dto)).Returns(classSource);
        _mockStorageServiceRepository
            .Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("https://storage/class.mp3");
        _mockClassSourceRepository
            .Setup(r => r.CreateAsync(It.IsAny<ClassSource>()))
            .ReturnsAsync(classSource);
        _mockMapper.Setup(m => m.Map<UploadClassSourceResponseDto>(classSource)).Returns(response);

        // Act
        var result = await _uploadClassSource.Execute(dto);

        // Assert
        _mockClassSourceRepository.Verify(r => r.CreateAsync(It.IsAny<ClassSource>()), Times.Once);
        _mockPublishEndpoint.Verify(p => p.Publish(It.IsAny<TranscriptionRequestEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        result.Id.Should().Be(classSource.Id);
    }


    private static UploadClassSourceRequestDto ValidClassSourceDto()
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(f => f.FileName).Returns("class.mp3");
        mockFile.Setup(f => f.Length).Returns(1024);
        mockFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[1024]));

        return new UploadClassSourceRequestDto
        {
            Name = "Class Test",
            CourseId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            File = mockFile.Object,
        };
    }
}
