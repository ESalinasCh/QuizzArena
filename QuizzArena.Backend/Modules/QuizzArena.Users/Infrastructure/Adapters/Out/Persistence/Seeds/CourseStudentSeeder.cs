using Microsoft.EntityFrameworkCore;
using QuizzArena.Users.Domain.Entities;


namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;

internal sealed class CourseStudentSeeder
{
    public static async Task SeedAsync(UserDbContext context)
    {
        if (await context.CourseStudents.AnyAsync())
        {
            return;
        }

        var courseStudent = new CourseStudent
        {
            Id = Guid.NewGuid(),
            CourseId = UserConstants.CourseId,
            StudentId = UserConstants.StudentId,
        };

        context.CourseStudents.Add(courseStudent);

        await context.SaveChangesAsync();
    }
}
