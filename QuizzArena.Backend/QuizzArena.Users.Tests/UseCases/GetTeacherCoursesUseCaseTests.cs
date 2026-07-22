using AutoMapper;
using FluentAssertions;
using Moq;
using QuizzArena.Users.Application.DTOs.Course;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Application.UseCases.Course;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Tests.UseCases;

public class GetTeacherCoursesUseCaseTests
{
    private readonly Mock<ICourseQueriesRepository> _mockCourseQueriesRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetTeacherCoursesUseCase _useCase;

    public GetTeacherCoursesUseCaseTests()
    {
        _mockCourseQueriesRepository = new Mock<ICourseQueriesRepository>();
        _mockMapper = new Mock<IMapper>();
        _useCase = new GetTeacherCoursesUseCase(_mockCourseQueriesRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Execute_WithTeacherId_FetchesCoursesForThatTeacherAndMaps()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var courses = new List<Course>
        {
            new() { Id = Guid.NewGuid(), Name = "Physics", Description = "Mechanics", TeacherId = teacherId }
        };
        var expected = new List<CourseDto>
        {
            new() { Id = courses[0].Id, Name = "Physics", Description = "Mechanics" }
        };
        _mockCourseQueriesRepository.Setup(r => r.GetCoursesByTeacherId(teacherId)).ReturnsAsync(courses);
        _mockMapper.Setup(m => m.Map<List<CourseDto>>(courses)).Returns(expected);

        // Act
        List<CourseDto> result = await _useCase.Execute(teacherId);

        // Assert
        result.Should().BeEquivalentTo(expected);
        _mockCourseQueriesRepository.Verify(r => r.GetCoursesByTeacherId(teacherId), Times.Once);
    }

    [Fact]
    public async Task Execute_NoCourses_ReturnsEmptyList()
    {
        // Arrange
        var teacherId = Guid.NewGuid();
        var courses = new List<Course>();
        _mockCourseQueriesRepository.Setup(r => r.GetCoursesByTeacherId(teacherId)).ReturnsAsync(courses);
        _mockMapper.Setup(m => m.Map<List<CourseDto>>(courses)).Returns(new List<CourseDto>());

        // Act
        List<CourseDto> result = await _useCase.Execute(teacherId);

        // Assert
        result.Should().BeEmpty();
    }
}
