using AutoMapper;
using FluentAssertions;
using FluentValidation;
using Moq;
using QuizzArena.Users.Application.DTOs.User;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Application.UseCases.User;
using QuizzArena.Users.Application.Validators;
using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Tests.UseCases;

public class UserUseCaseTests
{
    private readonly Mock<IUserRepository> _mockRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UserUseCase _useCase;

    public UserUseCaseTests()
    {
        _mockRepository = new Mock<IUserRepository>();
        _mockMapper = new Mock<IMapper>();

        _useCase = new UserUseCase(
            _mockRepository.Object,
            _mockMapper.Object,
            new UserCreateDtoValidator()
        );
    }

    private static CreateUserDto BuildValidDto() => new()
    {
        Id = Guid.NewGuid(),
        UserName = "druize",
        FirstName = "David",
        LastName = "Ruiz",
        Email = "david.ruiz@jala.university",
        ExternalProvider = "keycloak",
        Role = UserRole.Teacher,
        ProviderId = Guid.NewGuid().ToString()
    };

    [Fact]
    public async Task ExistsAsync_WhenRepositoryReturnsTrue_ReturnsTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid().ToString();
        _mockRepository.Setup(r => r.ExistsAsync(providerId)).ReturnsAsync(true);

        // Act
        bool exists = await _useCase.ExistsAsync(providerId);

        // Assert
        exists.Should().BeTrue();
        _mockRepository.Verify(r => r.ExistsAsync(providerId), Times.Once);
    }

    [Fact]
    public async Task ExistsAsync_WhenRepositoryReturnsFalse_ReturnsFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid().ToString();
        _mockRepository.Setup(r => r.ExistsAsync(providerId)).ReturnsAsync(false);

        // Act
        bool exists = await _useCase.ExistsAsync(providerId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task Register_WithValidDto_MapsPersistsAndReturnsUserDto()
    {
        // Arrange
        var dto = BuildValidDto();
        var entity = new Users.Domain.Entities.User { Id = dto.Id, UserName = dto.UserName, Email = dto.Email };
        var expected = new UserDto { UserName = dto.UserName, Email = dto.Email, Role = dto.Role };

        _mockMapper.Setup(m => m.Map<Users.Domain.Entities.User>(dto)).Returns(entity);
        _mockMapper.Setup(m => m.Map<UserDto>(entity)).Returns(expected);

        // Act
        UserDto result = await _useCase.Register(dto);

        // Assert
        result.Should().BeSameAs(expected);
        _mockRepository.Verify(r => r.Register(entity), Times.Once);
    }

    [Fact]
    public async Task Register_WithInvalidDto_ThrowsValidationExceptionAndDoesNotPersist()
    {
        // Arrange
        var dto = BuildValidDto();
        dto.Email = "not-an-email";

        // Act
        Func<Task> act = () => _useCase.Register(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        _mockRepository.Verify(r => r.Register(It.IsAny<Users.Domain.Entities.User>()), Times.Never);
        _mockMapper.Verify(m => m.Map<Users.Domain.Entities.User>(It.IsAny<CreateUserDto>()), Times.Never);
    }

    [Fact]
    public async Task Register_WhenRepositoryThrows_PropagatesException()
    {
        // Arrange
        var dto = BuildValidDto();
        var entity = new Users.Domain.Entities.User { Id = dto.Id };

        _mockMapper.Setup(m => m.Map<Users.Domain.Entities.User>(dto)).Returns(entity);
        _mockRepository
            .Setup(r => r.Register(entity))
            .ThrowsAsync(new InvalidOperationException("duplicate provider id"));

        // Act
        Func<Task> act = () => _useCase.Register(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("duplicate provider id");
    }
}
