using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Application.Ports.Out;

public interface ICourseQueriesRepository
{
    Task<List<Course>> GetCoursesByUserId(Guid studentId);
}
