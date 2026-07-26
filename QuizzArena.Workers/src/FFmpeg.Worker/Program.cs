using Azure.Identity;
using FFmpeg.Worker.Consumers;
using FFmpeg.Worker.Services;
using MassTransit;

DotNetEnv.Env.TraversePath().Load();
var builder = Host.CreateApplicationBuilder(args);

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Is Development: {builder.Environment.IsDevelopment()}");

if (!builder.Environment.IsDevelopment())
{
    var vaultUri = builder.Configuration["AzureKeyVault:VaultUri"];
    if (!string.IsNullOrEmpty(vaultUri))
    {
        var options = new DefaultAzureCredentialOptions
        {
            ExcludeVisualStudioCredential = true,
        };

        var credential = new DefaultAzureCredential(options);
        builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), credential);
    }
    else
    {
        Console.WriteLine("AzureKeyVault:VaultUri is not configured, skipping Azure Key Vault integration.");
    }
}

var rabbitConnectionString = builder.Configuration["RabbitMq:ConnectionString"];
var rabbitHost = builder.Configuration["RabbitMq:Host"];
var rabbitUsername = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitPassword = builder.Configuration["RabbitMq:Password"] ?? "guest";

if (string.IsNullOrWhiteSpace(rabbitConnectionString) && string.IsNullOrWhiteSpace(rabbitHost))
{
    throw new InvalidOperationException("Configuration 'RabbitMq:ConnectionString' or 'RabbitMq:Host' is required but not found.");
}

if (string.IsNullOrWhiteSpace(builder.Configuration["AzureStorage:AccountUrl"]))
{
    throw new InvalidOperationException("Configuration 'AzureStorage:AccountUrl' is required but not found.");
}

builder.Services.AddHttpClient<FileDownloader>();
builder.Services.AddSingleton<FfmpegProcessRunner>();
builder.Services.AddSingleton<BlobStorageService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CompressAudioJobConsumer>();
    // Futuro: x.AddConsumer<TranscodeVideoJobConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        if (!string.IsNullOrEmpty(rabbitConnectionString))
        {
            cfg.Host(new Uri(rabbitConnectionString));
        }
        else
        {
            cfg.Host(rabbitHost, "/", h =>
            {
                h.Username(rabbitUsername);
                h.Password(rabbitPassword);
            });
        }

        cfg.PrefetchCount = 1;
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();
