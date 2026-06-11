using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Domain.Entities;
using Shared.Contracts;
using Shared.Contracts.DTOs;

namespace QuizzArena.Users.Infrastructure.Adapters.In.ExternalServices;

internal class CourseContractImpl(
    ICourseQueriesRepository courseQuerysRepository,
    IUserQueriesRepository userQuerysRepository
) : ICourseContract
{

    public async Task<List<CourseSummaryDTO>> GetCoursesByStudent(Guid studentId)
    {

        List<Course> courses = await courseQuerysRepository.GetCoursesByUserId(studentId);
        List<Guid> teacherIds = courses.Select(x => x.TeacherId).ToList();
        List<User> users = await userQuerysRepository.GetByIds(teacherIds);

        List<CourseSummaryDTO> courseSummaries = courses.Select(x =>
        {
            User user = users.First(y => y.Id == x.TeacherId);
            return new CourseSummaryDTO()
            {
                Id = x.Id,
                CourseName = x.Name,
                ProfessorName = user.FirstName + " " + user.LastName
            };
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
}
