using Microsoft.Extensions.DependencyInjection;
using QuizzArena.Quizzing.Application.Ports.In.Answer;
using QuizzArena.Quizzing.Application.Ports.In.Question;
using QuizzArena.Quizzing.Application.Ports.In.Quiz;
using QuizzArena.Quizzing.Application.Ports.In.QuizAttempt;
using QuizzArena.Quizzing.Application.Ports.Out;
using QuizzArena.Quizzing.Application.UseCases.Answer;
using QuizzArena.Quizzing.Application.UseCases.Question;
using QuizzArena.Quizzing.Application.UseCases.Quiz;
using QuizzArena.Quizzing.Application.UseCases.QuizAttempt;
using QuizzArena.Quizzing.Infraestructure.Adapters.In.Web;
using QuizzArena.Quizzing.Infraestructure.Adapters.Out.Persistence;

namespace QuizzArena.Quizzing;

public static class DependencyInjection
{
    public static IServiceCollection AddQuizzingModule(this IServiceCollection services)
    {
        services.AddControllers()
            .AddApplicationPart(typeof(IQuizzingInfrastructureMarker).Assembly);

        services.AddScoped<ISetAnswerUseCase, SetAnswerUseCase>();
        services.AddScoped<IAnswerRepository, SqlAnswerRepository>();

        services.AddScoped<ICreateQuizUseCase, CreateQuizUseCase>();
        services.AddScoped<IUpdateQuizUseCase, UpdateQuizUseCase>();
        services.AddScoped<IDeleteQuizUseCase, DeleteQuizUseCase>();
        services.AddScoped<IQuizRepository, SqlQuizRepository>();

        services.AddScoped<ICreateQuestionUseCase, CreateQuestionUseCase>();
        services.AddScoped<IUpdateQuestionUseCase, UpdateQuestionUseCase>();
        services.AddScoped<IDeleteQuestionUseCase, DeleteQuestionUseCase>();
        services.AddScoped<IQuestionRepository, SqlQuestionRepository>();

        services.AddScoped<IStartQuizAttemptUseCase, StartQuizAttemptUseCase>();
        services.AddScoped<IEndQuizAttemptUseCase, EndQuizAttemptUseCase>();
        services.AddScoped<IQuizAttemptRepository, SqlQuizAttemptRepository>();

        // TODO: Add DB Connection

        return services;
    }
}
