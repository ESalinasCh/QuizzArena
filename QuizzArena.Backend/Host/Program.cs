using System.Text.Json.Serialization;
using Host.Extensions;
using Host.Security;
using MassTransit;
using Microsoft.AspNetCore.Http.Features;
using QuizzArena.DocumentProcessing;
using QuizzArena.DocumentProcessing.Infrastructure.Adapters.Out.Messaging.Configuration;
using QuizzArena.Quizzing;
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

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(
                    builder.Configuration["RabbitMq:Host"] ?? "localhost",
                    "/",
                    h =>
                    {
                        h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
                        h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
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
