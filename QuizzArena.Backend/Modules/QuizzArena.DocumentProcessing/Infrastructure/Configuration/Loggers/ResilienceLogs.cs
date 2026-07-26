using Microsoft.Extensions.Logging;

namespace QuizzArena.DocumentProcessing.Infrastructure.Configuration.Loggers;

internal static partial class ResilienceLogs
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Warning,
        Message = "[RESILIENCE] {Provider} - Rate limit Hit (429). Retry {Attempt}/{MaxAttempts} scheduled in {DelaySeconds}s."
    )]
    public static partial void LogResilienceRetry(ILogger logger, string provider, int attempt, int maxAttempts, double delaySeconds);
}
