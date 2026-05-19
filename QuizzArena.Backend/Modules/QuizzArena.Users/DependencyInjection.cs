using Microsoft.Extensions.DependencyInjection;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Application.UseCases.User;
using QuizzArena.Users.Infrastructure.Adapters.In.Web;
using QuizzArena.Users.Infrastructure.Adapters.Out;

namespace QuizzArena.Users
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(IUsersInfrastructureMaker).Assembly);

            services.AddScoped<ISignUpUserUseCase, SignUpUserUseCase>();
            services.AddScoped<ILogInUserUseCase, LogInUserUseCase>();
            services.AddScoped<IUserRepository, SqlUserRepository>();

            // TODO: Add DB Connection

            return services;
        }
    }
}
