using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizzArena.Users.Domain.Entities;
using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;

internal class UserSeeder
{
    public static async Task SeedAsync(UserDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var hasher = new PasswordHasher<User>();

        var teacher = new User
        {
            Id = UserConstants.TeacherId,
            UserName = "teacher01",
            FirstName = "Samuel",
            LastName = "Teacher",
            Email = "teacher@test.com",
            Role = UserRole.Teacher,
            ExternalProvider = "Keycloak",
            ProviderId = UserConstants.TeacherId.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(teacher);

        var student = new User
        {
            Id = UserConstants.StudentId,
            UserName = "student01",
            FirstName = "Juan",
            LastName = "Student",
            Email = "user@test.com",
            Role = UserRole.Student,
            ExternalProvider = "Keycloak",
            ProviderId = UserConstants.StudentId.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(student);

        await context.SaveChangesAsync();
    }
}
