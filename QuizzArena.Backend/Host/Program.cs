using System.Text.Json.Serialization;
using Host.Security;
using QuizzArena.DocumentProcessing;
using QuizzArena.Quizzing;
using QuizzArena.Users;

namespace Host;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Adding Controllers
        builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
        }).AddMvc();

        builder.Services.AddJwtAuthentication(builder.Configuration);

        builder.Services.AddUsersModule(builder.Configuration);
        builder.Services.AddQuizzingModule(builder.Configuration);
        builder.Services.AddDocumentProcessingModule(builder.Configuration);

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.ApplyMigrations();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseMiddleware<UserValidationMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
