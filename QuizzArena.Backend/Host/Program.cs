using System.Text.Json.Serialization;
using Host.Extensions;
using Host.Security;
using MassTransit;
using Microsoft.AspNetCore.Http.Features;
using QuizzArena.DocumentProcessing;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Messaging.Configuration;
using QuizzArena.Quizzing;
using QuizzArena.Quizzing.Infrastructure.Adapters.Out.Messaging.Configuration;
using QuizzArena.Users;

namespace Host;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartHeadersLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = long.MaxValue;
                options.ValueLengthLimit = int.MaxValue;
            });

        builder.Services.AddMassTransit(x =>
        {
            DocumentProcessingMassTransit.AddConsumers(x);

            QuizzingMassTransit.AddConsumers(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    "localhost",
                    "/",
                    h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                cfg.ConfigureEndpoints(
                    context);
            });
        });

        WebApplication app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.ApplyMigrations();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseMiddleware<UserValidationMiddleware>();

        app.MapControllers();

        app.Run();
    }
}
