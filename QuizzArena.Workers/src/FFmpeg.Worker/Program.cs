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
    Console.WriteLine($"Azure Key Vault URI: {vaultUri}");

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
        throw new InvalidOperationException("AzureKeyVault:VaultUri is required in production but not found.");
    }

    // Sobrescribimos ConnectionStrings locales con los secretos de Key Vault
    if (!string.IsNullOrEmpty(builder.Configuration["AzureStorage:BlobConnectionString"]))
    {
        builder.Configuration["ConnectionStrings:AzureBlobStorage"] = builder.Configuration["AzureStorage:BlobConnectionString"];
    }

    if (!string.IsNullOrEmpty(builder.Configuration["RabbitMQ:ConnectionStrings"]))
    {
        builder.Configuration["ConnectionStrings:RabbitMq"] = builder.Configuration["RabbitMQ:ConnectionStrings"];
    }
}

// Lectura unificada de Connection Strings
var rabbitConnectionString = builder.Configuration.GetConnectionString("RabbitMq");
if (string.IsNullOrWhiteSpace(rabbitConnectionString))
{
    throw new InvalidOperationException("Connection string 'RabbitMq' is required but not found.");
}

var blobConnectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
if (string.IsNullOrWhiteSpace(blobConnectionString))
{
    throw new InvalidOperationException("Connection string 'AzureBlobStorage' is required but not found.");
}

builder.Services.AddHttpClient<FileDownloader>();
builder.Services.AddSingleton<FfmpegProcessRunner>();
builder.Services.AddSingleton<BlobStorageService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CompressAudioJobConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitConnectionString));
        cfg.PrefetchCount = 1;
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();