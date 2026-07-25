// using FFmpeg.Worker;

// var builder = Host.CreateApplicationBuilder(args);
// builder.Services.AddHostedService<Worker>();

// var host = builder.Build();
// host.Run();

using FFmpeg.Worker.Consumers;
using FFmpeg.Worker.Services;
using MassTransit;

DotNetEnv.Env.TraversePath().Load();
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient<FileDownloader>();
builder.Services.AddSingleton<FfmpegProcessRunner>();
builder.Services.AddSingleton<BlobStorageService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<CompressAudioJobConsumer>();
    // Futuro: x.AddConsumer<TranscodeVideoJobConsumer>();
    // Futuro: x.AddConsumer<GenerateThumbnailJobConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMq:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMq:Password"] ?? "guest");
        });

        cfg.PrefetchCount = 1; // un job pesado a la vez por réplica -> memoria/CPU predecible

        // ConfigureEndpoints crea automáticamente una cola por cada consumer
        // registrado arriba. Agregar un nuevo tipo de job = una línea de AddConsumer,
        // no hay que tocar la configuración de endpoints.
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
await host.RunAsync();