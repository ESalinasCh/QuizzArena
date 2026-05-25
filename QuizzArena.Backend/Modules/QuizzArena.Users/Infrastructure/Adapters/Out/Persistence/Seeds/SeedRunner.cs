using System;
using System.Collections.Generic;
using System.Text;

namespace QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds
{
    internal class SeedRunner
    {
        public static async Task SeedAsync(UserDbContext context)
        {
            await UserSeeder.SeedAsync(context);
            await CourseSeeder.SeedAsync(context);
            await CourseStudentSeeder.SeedAsync(context);
        }
    }
}
