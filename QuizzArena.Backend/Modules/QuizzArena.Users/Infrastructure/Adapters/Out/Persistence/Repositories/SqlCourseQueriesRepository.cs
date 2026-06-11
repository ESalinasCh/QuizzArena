using Microsoft.EntityFrameworkCore;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;

internal class SqlCourseQueriesRepository(UserDbContext context) : ICourseQueriesRepository
{
    public async Task<List<Course>> GetCoursesByUserId(Guid studentId)
    {
        List<Course> courses = await context.Courses.Where(
            x => x.CourseStudents.Any(cs => cs.StudentId == studentId && !cs.Deleted) && !x.Deleted
        ).ToListAsync();

        return courses;
    }
}
