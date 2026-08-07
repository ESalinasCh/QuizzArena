using FluentAssertions;
using Moq;
using QuizzArena.DocumentProcessing.Application.Ports.Out;
using QuizzArena.DocumentProcessing.Application.UseCases.ClassSources;
using QuizzArena.DocumentProcessing.Domain.Entities;
using QuizzArena.DocumentProcessing.Domain.Enums;
using Shared.Contracts.DTOs;

namespace QuizzArena.DocumentProcessing.Tests.UseCases;

public class GetClassSourcesUseCaseTests
{
    private readonly Mock<IClassSourceRepository> _mockClassSourceRepository;
    private readonly GetClassSourcesUseCase _useCase;

    public GetClassSourcesUseCaseTests()
    {
        _mockClassSourceRepository = new Mock<IClassSourceRepository>();
        _useCase = new GetClassSourcesUseCase(_mockClassSourceRepository.Object);
    }

    [Fact]
    public async Task Execute_MapsEveryRepositoryResultToResponseDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new PagedRequest { Page = 1, PageSize = 10 };
        var jobIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var source = new ClassSource
        {
            Id = Guid.NewGuid(),
            Name = "Clase 1",
            Status = SourceStatus.Completed,
            CreatedAt = DateTimeOffset.UtcNow,
            CourseId = Guid.NewGuid(),
            UserId = userId
        };

        _mockClassSourceRepository
            .Setup(r => r.GetByUserIdAsync(userId, query))
            .ReturnsAsync([(source, jobIds)]);

        // Act
        var result = await _useCase.Execute(userId, query);

        // Assert
        result.Should().ContainSingle();
        var dto = result[0];
        dto.Id.Should().Be(source.Id);
        dto.Name.Should().Be(source.Name);
        dto.Status.Should().Be(source.Status);
        dto.CreatedAt.Should().Be(source.CreatedAt);
        dto.CourseId.Should().Be(source.CourseId);
        dto.ProcessingJobsIds.Should().BeEquivalentTo(jobIds);
    }

    [Fact]
    public async Task Execute_PreservesRepositoryOrderAndCount()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new PagedRequest();
        var first = new ClassSource { Id = Guid.NewGuid(), Name = "Clase 1" };
        var second = new ClassSource { Id = Guid.NewGuid(), Name = "Clase 2" };

        _mockClassSourceRepository
            .Setup(r => r.GetByUserIdAsync(userId, query))
            .ReturnsAsync([(first, []), (second, [])]);

        // Act
        var result = await _useCase.Execute(userId, query);

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().ContainInOrder("Clase 1", "Clase 2");
    }

    [Fact]
    public async Task Execute_WithNoSources_ReturnsEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new PagedRequest();

        _mockClassSourceRepository
            .Setup(r => r.GetByUserIdAsync(userId, query))
            .ReturnsAsync([]);

        // Act
        var result = await _useCase.Execute(userId, query);

        // Assert
        result.Should().BeEmpty();
        _mockClassSourceRepository.Verify(r => r.GetByUserIdAsync(userId, query), Times.Once);
    }

    [Fact]
    public async Task Execute_WithNoProcessingJobs_ReturnsEmptyJobIds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new PagedRequest();
        var source = new ClassSource { Id = Guid.NewGuid(), Name = "Clase sin jobs" };

        _mockClassSourceRepository
            .Setup(r => r.GetByUserIdAsync(userId, query))
            .ReturnsAsync([(source, [])]);

        // Act
        var result = await _useCase.Execute(userId, query);

        // Assert
        result.Should().ContainSingle().Which.ProcessingJobsIds.Should().BeEmpty();
    }

    [Fact]
    public async Task Execute_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new PagedRequest();

        _mockClassSourceRepository
            .Setup(r => r.GetByUserIdAsync(userId, query))
            .ThrowsAsync(new InvalidOperationException("database unavailable"));

        // Act
        Func<Task> act = () => _useCase.Execute(userId, query);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("database unavailable");
    }
}
