namespace Host.ExceptionHandling.Handlers;

internal record ErrorResponse(string Code, string Message, int Status);
