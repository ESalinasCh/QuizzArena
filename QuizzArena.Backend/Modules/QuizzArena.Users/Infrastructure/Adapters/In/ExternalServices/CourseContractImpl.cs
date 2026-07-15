using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Users.Infrastructure.Adapters.In.ExternalServices;

internal sealed class CourseContractImpl(
    ICourseQueriesRepository courseQuerysRepository,
    IUserQueriesRepository userQuerysRepository
) : ICourseContract
{
    public async Task<List<CourseSummaryDTO>> GetCoursesByStudent(Guid studentId)
    {
        List<Course> courses = await courseQuerysRepository.GetCoursesByUserId(studentId);
        List<Guid> teacherIds = courses.Select(x => x.TeacherId).ToList();
        List<User> teachers = await userQuerysRepository.GetByIds(teacherIds);

        var teachersDictionary = teachers.ToDictionary(x => x.Id);
        List<CourseSummaryDTO> courseSummaries = courses.Select(x =>
        {
            User user = teachersDictionary[x.TeacherId];
            return new CourseSummaryDTO()
            {
                Id = x.Id,
                CourseName = x.Name,
                ProfessorName = user.FirstName + " " + user.LastName
            };
        }).ToList();

        return courseSummaries;
    }

    public async Task<List<CourseSummaryDTO>> GetCoursesByTeacherId(Guid teacherId)
    {
        List<Course> courses = await courseQuerysRepository.GetCoursesByTeacherId(teacherId);
        User? teacher = await userQuerysRepository.GetUserById(teacherId) ?? throw new InvalidOperationException($"Teacher with id {teacherId} not found");
        List<CourseSummaryDTO> courseSummaries = courses.Select(x => new CourseSummaryDTO()
        {
            Id = x.Id,
            CourseName = x.Name,
            ProfessorName = teacher.FirstName + " " + teacher.LastName
        }).ToList();

        return courseSummaries;
    }

    public async Task<List<CourseSummaryDTO>> GetCoursesByIds(List<Guid> courseIds)
    {
        List<Course> courses = await courseQuerysRepository.GetCoursesByIds(courseIds);
        List<CourseSummaryDTO> courseSummaries = courses.Select(x => new CourseSummaryDTO()
        {
            Id = x.Id,
            CourseName = x.Name,
        }).ToList();

        return courseSummaries;
    }

    public async Task<CourseDto?> GetCourseById(Guid courseId)
    {
        Course? course = await courseQuerysRepository.GetCourseById(courseId);
        return course == null ? null : new CourseDto()
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Deleted = course.Deleted,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            DeletedAt = course.DeletedAt,
            TeacherId = course.TeacherId
        };
    }
}
