namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;

internal sealed class SeedRunner
{
    public static async Task SeedAsync(UserDbContext context)
    {
        await UserSeeder.SeedAsync(context);
        await CourseSeeder.SeedAsync(context);
        await CourseStudentSeeder.SeedAsync(context);
        await SqlSeeder.SeedAsync(context);
    }
}
