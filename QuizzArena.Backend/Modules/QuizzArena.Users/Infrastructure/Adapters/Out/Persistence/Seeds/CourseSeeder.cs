using Microsoft.EntityFrameworkCore;
using QuizzArena.Users.Domain.Entities;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;

internal class CourseSeeder
{
    public static async Task SeedAsync(UserDbContext context)
    {
        if (await context.Courses.AnyAsync())
        {
            return;
        }

        var course = new Course
        {
            Id = UserConstants.CourseId,
            Name = "Backend",
            Description = "Backend courses",
            TeacherId = UserConstants.TeacherId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Courses.Add(course);

        await context.SaveChangesAsync();
    }
}
