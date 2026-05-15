using QuizzArena.Quizzing.Infraestructure.Adapters.In.Web;
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
