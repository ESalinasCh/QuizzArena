namespace Host.ExceptionHandling.Handlers;

internal sealed record ErrorEntry(string Code, string Message);

internal sealed record ErrorResponse(IReadOnlyList<ErrorEntry> Errors, int Status);
