using Asp.Versioning;
using QuizzArena.DocumentProcessing;
using QuizzArena.Quizzing;
using QuizzArena.Users;
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
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            }).AddMvc();

            builder.Services.AddUsersModule();
            builder.Services.AddQuizzingModule();
            builder.Services.AddDocumentProcessingModule();

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
