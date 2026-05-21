using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Application.UseCases.User;
using QuizzArena.Users.Domain.Enums;
using QuizzArena.Users.Infraestructure.Adapters.Out.Persistence;
using QuizzArena.Users.Infrastructure.Adapters.In.Web;
using QuizzArena.Users.Infrastructure.Adapters.Out.ExternalServices;
using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;
using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence;
using Shared.Contracts;

namespace QuizzArena.Users
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddApplicationPart(typeof(IUsersInfrastructureMaker).Assembly);

            services.AddScoped<ISignUpUserUseCase, SignUpUserUseCase>();
            services.AddScoped<ILogInUserUseCase, LogInUserUseCase>();
            services.AddScoped<IUserRepository, SqlUserRepository>();
            services.AddScoped<IUsersContract, UsersContractImpl>();

            #region BDD
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

            dataSourceBuilder.MapEnum<UserRole>($"{UserConstants.Schema}.user_role");

            var dataSource = dataSourceBuilder.Build();

            services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(dataSource));
            #endregion

            return services;
        }
    }
}
