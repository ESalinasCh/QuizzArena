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
using QuizzArena.Quizzing.Infraestructure.Adapters.Out;
using QuizzArena.Users.Application.Ports.In;
using QuizzArena.Users.Application.Ports.Out;
using QuizzArena.Users.Application.UseCases.User;
using QuizzArena.Users.Infrastructure.Adapters.In.Web;
using QuizzArena.Users.Infrastructure.Adapters.Out;
using System.Text.Json.Serialization;

namespace QuizzArena.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Adding Controllers
            builder.Services.AddControllers()
            .AddApplicationPart(typeof(IQuizzingInfrastructureMarker).Assembly)
            .AddApplicationPart(typeof(IUsersInfrastructureMaker).Assembly)
            .AddJsonOptions(options => {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            // TODO: ADD DB CONNECTION


            // Dependency Injection
            builder.Services.AddScoped<ISignUpUserUseCase, SignUpUserUseCase>();
            builder.Services.AddScoped<ILogInUserUseCase, LogInUserUseCase>();
            builder.Services.AddScoped<IUserRepository, SqlUserRepository>();

            builder.Services.AddScoped<ISetAnswerUseCase, SetAnswerUseCase>();
            builder.Services.AddScoped<IAnswerRepository, SqlAnswerRepository>();

            builder.Services.AddScoped<ICreateQuizUseCase, CreateQuizUseCase>();
            builder.Services.AddScoped<IUpdateQuizUseCase, UpdateQuizUseCase>();
            builder.Services.AddScoped<IDeleteQuizUseCase, DeleteQuizUseCase>();
            builder.Services.AddScoped<IQuizRepository, SqlQuizRepository>();

            builder.Services.AddScoped<ICreateQuestionUseCase, CreateQuestionUseCase>();
            builder.Services.AddScoped<IUpdateQuestionUseCase, UpdateQuestionUseCase>();
            builder.Services.AddScoped<IDeleteQuestionUseCase, DeleteQuestionUseCase>();
            builder.Services.AddScoped<IQuestionRepository, SqlQuestionRepository>();

            builder.Services.AddScoped<IStartQuizAttemptUseCase, StartQuizAttemptUseCase>();
            builder.Services.AddScoped<IEndQuizAttemptUseCase, EndQuizAttemptUseCase>();
            builder.Services.AddScoped<IQuizAttemptRepository, SqlQuizAttemptRepository>();



            WebApplication app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
