using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuizzArena.Quizzing.Application.DTOs.Match;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out.Repositories;
using QuizzArena.Quizzing.Application.UseCases.MatchUseCases;
using QuizzArena.Quizzing.Application.Validators.FiltersValidators;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;
using QuizzArena.Users.Infrastructure.Adapters.Out.Persistence.Repositories;
using Shared.Contracts;

namespace QuizzArena.Quizzing;

public static class DependencyInjection
{
    public static IServiceCollection AddQuizzingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(IQuizzingInfrastructureMarker).Assembly);

        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);

        services.AddScoped<IGetMatchesUseCase, GetMatchesUseCase>();
        services.AddScoped<IStartAttemptUseCase, StartAttemptUseCase>();

        services.AddScoped<IMatchRepository, SqlMatchRepository>();
        services.AddScoped<IQuizRepository, SqlQuizRepository>();
        services.AddScoped<IMatchAttemptRepository, SqlMatchAttemptRepository>();
        services.AddScoped<IQuizQuestionRepository, SqlQuizQuestionRepository>();

        services.AddScoped<IValidator<MatchQueryParametersDto>, MatchQueryParametersValidator>();
        services.AddScoped<IMatchQueriesRepository, SqlMatchQueriesRepository>();
        services.AddScoped<IGetMatchAttemptsByStudent, GetMatchAttemptsByStudent>();

        services.AddScoped<IGetMatchAttemptDetail, GetMatchAttemptDetail>();
        services.AddScoped<IQuestionQueriesRepository, SqlQuestionQueriesRepository>();

        services.AddScoped<MatchAttemptFiltersValidator>();


        #region BDD
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.MapEnum<MatchMode>($"{QuizzingConstants.Schema}.match_mode");
        dataSourceBuilder.MapEnum<MatchStatus>($"{QuizzingConstants.Schema}.match_status");
        dataSourceBuilder.MapEnum<QuestionStatus>($"{QuizzingConstants.Schema}.question_status");
        dataSourceBuilder.MapEnum<QuestionType>($"{QuizzingConstants.Schema}.question_type");
        dataSourceBuilder.MapEnum<QuizAttemptStatus>($"{QuizzingConstants.Schema}.quiz_attempt_status");
        dataSourceBuilder.MapEnum<QuizStatus>($"{QuizzingConstants.Schema}.quiz_status");
        dataSourceBuilder.MapEnum<QuizOrigin>($"{QuizzingConstants.Schema}.quiz_origin");
        dataSourceBuilder.MapEnum<QuestionOrigin>($"{QuizzingConstants.Schema}.question_origin");


        var dataSource = dataSourceBuilder.Build();

        services.AddDbContext<QuizzingDbContext>(options =>
                options.UseNpgsql(
                    dataSource,
                    o =>
                    {
                        o.MapEnum<MatchMode>(
                            "match_mode",
                            QuizzingConstants.Schema
                                );
                        o.MapEnum<MatchStatus>(
                            "match_status",
                            QuizzingConstants.Schema
                            );
                        o.MapEnum<QuestionStatus>(
                            "question_status",
                            QuizzingConstants.Schema
                            );
                        o.MapEnum<QuestionType>(
                            "question_type",
                            QuizzingConstants.Schema
                            );
                        o.MapEnum<QuizAttemptStatus>(
                            "quiz_attempt_status",
                            QuizzingConstants.Schema
                            );
                        o.MapEnum<QuizStatus>(
                            "quiz_status",
                            QuizzingConstants.Schema
                            );
                        o.MapEnum<QuizOrigin>(
                            "quiz_origin",
                            QuizzingConstants.Schema
                            );
                        o.MapEnum<QuestionOrigin>(
                            "question_origin",
                            QuizzingConstants.Schema
                            );
                    }
                )
            );

        services.AddTransient<IModuleInitializer, QuizzingModuleInitializer>();
        #endregion

        return services;
    }
}
