using Microsoft.EntityFrameworkCore;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;

internal sealed class SqlCourseQueriesRepository(UserDbContext context) : ICourseQueriesRepository
{
    public async Task<List<Course>> GetCoursesByUserId(Guid studentId)
    {
        List<Course> courses = await context.Courses.Where(
            x => x.CourseStudents.Any(cs => cs.StudentId == studentId && !cs.Deleted) && !x.Deleted
        ).ToListAsync();

        return courses;
    }
    public async Task<List<Course>> GetCoursesByIds(List<Guid> coursesIds)
    {
        List<Course> courses = await context.Courses.Where(x =>
            coursesIds.Contains(x.Id)
       ).ToListAsync();

        return courses;
    }
}
