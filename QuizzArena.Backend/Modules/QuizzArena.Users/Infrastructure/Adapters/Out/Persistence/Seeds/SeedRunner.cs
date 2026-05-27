namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;

internal class SeedRunner
{
    internal static async Task SeedAsync(UserDbContext context)
    {
        await UserSeeder.SeedAsync(context);
        await CourseSeeder.SeedAsync(context);
        await CourseStudentSeeder.SeedAsync(context);
    }
}
