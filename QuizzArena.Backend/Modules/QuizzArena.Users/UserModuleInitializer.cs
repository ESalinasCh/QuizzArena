//using Microsoft.Extensions.DependencyInjection;
//using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence;
//using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Seeds;
//using Microsoft.EntityFrameworkCore;

//namespace QuizzArena.Users
//{
//    public static class UserModuleInitializer
//    {
//        public static async Task InitializeUsersModuleAsync(this IServiceProvider serviceProvider)
//        {
//            using var scope = serviceProvider.CreateScope();

//            var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

//            await context.Database.MigrateAsync();

//            await SeedRunner.SeedAsync(context);
//        }
//    }
//}
