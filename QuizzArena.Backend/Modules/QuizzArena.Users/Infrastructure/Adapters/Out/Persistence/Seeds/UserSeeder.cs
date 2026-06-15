using QuizzArena.Users.Domain.Entities;
using QuizzArena.Users.Domain.Enums;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;

internal class UserSeeder
{
    public static async Task SeedAsync(UserDbContext context)
    {
        // Only add users that are already on Keycloak
        var users = new[]
        {
            new User
            {
                Id = Guid.Parse("d7f65af9-57a1-4b5b-9685-815770faea7d"),
                UserName = "pedro",
                FirstName = "Pedro",
                LastName = "Villca",
                Email = "pedro.student@test.com",
                Role = UserRole.Student,
                ExternalProvider = "Keycloak",
                ProviderId = "d7f65af9-57a1-4b5b-9685-815770faea7d",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new User
            {
                Id = Guid.Parse("686cdcdc-f33d-4274-b2aa-60f1d2ddd578"),
                UserName = "maria",
                FirstName = "Maria",
                LastName = "Perez",
                Email = "maria@example.com",
                Role = UserRole.Teacher,
                ExternalProvider = "Keycloak",
                ProviderId = "686cdcdc-f33d-4274-b2aa-60f1d2ddd578",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        foreach (var user in users)
        {
            var trackedUser = await context.Users.FindAsync(user.Id);
            if (trackedUser == null)
            {
                context.Users.Add(user);
            }
            else
            {
                context.Entry(trackedUser).CurrentValues.SetValues(user);
                trackedUser.UpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();
    }
}
