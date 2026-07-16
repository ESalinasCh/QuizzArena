using System.Text.Json.Serialization;
using Azure.Identity;
using Host.ExceptionHandling;
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
    public static async Task Main(string[] args)
    {
        DotNetEnv.Env.TraversePath().Load();
        var builder = WebApplication.CreateBuilder(args);

        // Configure Azure Key Vault and map secrets if not running in Development environment
        if (!builder.Environment.IsDevelopment())
        {
            var vaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
            if (!string.IsNullOrEmpty(vaultUri))
            {
                builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
            }

            // Map production Key Vault secrets to standard application configuration keys
            if (!string.IsNullOrEmpty(builder.Configuration["DbSettings:ConnectionString"]))
            {
                builder.Configuration["ConnectionStrings:DefaultConnection"] = builder.Configuration["DbSettings:ConnectionString"];
            }
            if (!string.IsNullOrEmpty(builder.Configuration["AzureStorage:BlobConnectionString"]))
            {
                builder.Configuration["ConnectionStrings:AzureBlobStorage"] = builder.Configuration["AzureStorage:BlobConnectionString"];
            }
            // if (!string.IsNullOrEmpty(builder.Configuration["WhisperService:Url"]))
            // {
            //     builder.Configuration["TranscriptionSettings:BaseUrl"] = builder.Configuration["WhisperService:Url"];
            // }
            if (!string.IsNullOrEmpty(builder.Configuration["WhisperService:ApiKey"]))
            {
                builder.Configuration["TranscriptionSettings:ApiKey"] = builder.Configuration["WhisperService:ApiKey"];
            }
            if (!string.IsNullOrEmpty(builder.Configuration["AiSettings:LlmApiKey"]))
            {
                builder.Configuration["TextGenerationSettings:ApiKey"] = builder.Configuration["AiSettings:LlmApiKey"];
            }
            if (!string.IsNullOrEmpty(builder.Configuration["AiSettings:EmbeddingApiKey"]))
            {
                builder.Configuration["EmbeddingSettings:ApiKey"] = builder.Configuration["AiSettings:EmbeddingApiKey"];
            }
        }

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

        var rabbitMqConnectionString = builder.Configuration["RabbitMQ:ConnectionStrings"];
        var rabbitHost = builder.Configuration["RabbitMq:Host"];
        var rabbitUsername = builder.Configuration["RabbitMq:Username"];
        var rabbitPassword = builder.Configuration["RabbitMq:Password"];

        if (string.IsNullOrWhiteSpace(rabbitMqConnectionString))
        {
            if (string.IsNullOrWhiteSpace(rabbitHost))
            {
                throw new InvalidOperationException("Configuration 'RabbitMq:Host' is required but was not found.");
            }

            if (string.IsNullOrWhiteSpace(rabbitUsername))
            {
                throw new InvalidOperationException("Configuration 'RabbitMq:Username' is required but was not found.");
            }

            if (string.IsNullOrWhiteSpace(rabbitPassword))
            {
                throw new InvalidOperationException("Configuration 'RabbitMq:Password' is required but was not found.");
            }
        }

        builder.Services.AddMassTransit(x =>
        {
            DocumentProcessingMassTransit.AddConsumers(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                if (!string.IsNullOrEmpty(rabbitMqConnectionString))
                {
                    cfg.Host(new Uri(rabbitMqConnectionString));
                }
                else
                {
                    cfg.Host(
                        rabbitHost,
                        "/",
                        h =>
                        {
                            h.Username(rabbitUsername);
                            h.Password(rabbitPassword);
                        }
                    );
                }
                cfg.ConfigureEndpoints(context);
            });
        });

        WebApplication app = builder.Build();

        await app.ApplyMigrations();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseMiddleware<GlobalExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseMiddleware<UserValidationMiddleware>();

        app.MapControllers();

        await app.RunAsync();
    }
}
