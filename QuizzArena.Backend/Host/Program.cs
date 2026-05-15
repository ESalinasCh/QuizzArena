using QuizzArena.Quizzing.Infraestructure.Adapters.In.Web;
using QuizzArena.Users.Infrastructure.Adapters.In.Web;

namespace QuizzArena.Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddApplicationPart(typeof(QuizController).Assembly);

            // TODO: ADD DB CONNECTION

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
