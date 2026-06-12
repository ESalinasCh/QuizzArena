using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using QuizzArena.Quizzing.Application.Ports.In;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.UseCases.Quiz;
using QuizzArena.Quizzing.Application.Validators.Option;
using QuizzArena.Quizzing.Application.Validators.Question;
using QuizzArena.Quizzing.Application.Validators.Quiz;
using QuizzArena.Quizzing.Domain.Enums;
using QuizzArena.Quizzing.Infrastructure.Adapters.In.Web;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Persistence.Repositories;
using Shared.Contracts;

namespace QuizzArena.Quizzing;

public static class DependencyInjection
{
    public static IServiceCollection AddQuizzingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(IQuizzingInfrastructureMarker).Assembly);

        services.AddScoped<CreateQuizDtoValidator>();
        services.AddScoped<CreateQuestionDtoValidator>();
        services.AddScoped<CreateQuestionsDtoValidator>();
        services.AddScoped<CreateOptionDtoValidator>();
        services.AddScoped<CreateOptionsDtoValidator>();
        services.AddAutoMapper(cfg => { }, typeof(DependencyInjection).Assembly);

        services.AddScoped<ICreateQuizUseCase, CreateQuizUseCase>();
        services.AddScoped<ICreateQuestionsUseCase, CreateQuestionsUseCase>();
        services.AddScoped<ICreateOptionsUseCase, CreateOptionsUseCase>();
        services.AddScoped<IQuizRepository, SqlQuizRepository>();
        services.AddScoped<IQuestionRepository, SqlQuestionRepository>();
        services.AddScoped<IOptionRepository, SqlOptionRepository>();

        #region BDD
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);

        dataSourceBuilder.MapEnum<MatchMode>($"{QuizzingConstants.Schema}.match_mode");
        dataSourceBuilder.MapEnum<MatchStatus>($"{QuizzingConstants.Schema}.match_status");
        dataSourceBuilder.MapEnum<QuestionStatus>($"{QuizzingConstants.Schema}.question_status");
        dataSourceBuilder.MapEnum<QuestionType>($"{QuizzingConstants.Schema}.question_type");
        dataSourceBuilder.MapEnum<QuizAttemptStatus>($"{QuizzingConstants.Schema}.quiz_attempt_status");
        dataSourceBuilder.MapEnum<QuizStatus>($"{QuizzingConstants.Schema}.quiz_status");

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
                    }
                )
            );

        services.AddTransient<IModuleInitializer, QuizzingModuleInitializer>();
        #endregion

        return services;
    }
}
