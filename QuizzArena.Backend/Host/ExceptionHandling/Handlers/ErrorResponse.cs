namespace Host.ExceptionHandling.Handlers;

internal sealed record ErrorResponse(string Code, string Message, int Status);
