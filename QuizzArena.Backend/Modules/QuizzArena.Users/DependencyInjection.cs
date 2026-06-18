using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Application.UseCases.User;
using QuizzArena.Users.Application.Validators;
using QuizzArena.Users.Domain.Enums;
using QuizzArena.Users.Infrastructure.Adapters.In.ExternalServices;
using QuizzArena.Users.Infrastructure.Adapters.In.Web;
using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence;
using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;
using Shared.Contracts;

namespace QuizzArena.Users;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(IUsersInfrastructureMaker).Assembly);

        services.AddScoped<UserCreateDtoValidator>();
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);

        services.AddScoped<IUserRepository, SqlUserRepository>();
        services.AddScoped<IUserQueriesRepository, SqlUserQueriesRepository>();
        services.AddScoped<ICourseQueriesRepository, SqlCourseQueriesRepository>();
        services.AddScoped<ICourseContract, CourseContractImpl>();

        services.AddScoped<UserUseCase>();
        services.AddScoped<IUserUseCase>(sp => sp.GetRequiredService<UserUseCase>());
        services.AddHttpContextAccessor();

        services.AddScoped<ICurrentUser, CurrentUserUseCase>();

        #region BDD
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.MapEnum<UserRole>($"{UserConstants.Schema}.user_role");

        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<UserDbContext>(options =>
                options.UseNpgsql(
                    dataSource,
                    o => o.MapEnum<UserRole>(
                        "user_role",
                        UserConstants.Schema
                    )
                )
            );

        services.AddTransient<IModuleInitializer, UserModuleInitializer>();
        #endregion

        return services;
    }
}
