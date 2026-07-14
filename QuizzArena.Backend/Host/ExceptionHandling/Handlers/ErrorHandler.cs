namespace Host.ExceptionHandling.Handlers;

internal abstract class ErrorHandler : IErrorHandler
{
    public abstract Task HandleAsync(ErrorHandlerContext context);

    protected static Task BadRequest(ErrorHandlerContext ctx, string code, string message)
        => BadRequest(ctx, [new ErrorEntry(code, message)]);

    protected static Task BadRequest(ErrorHandlerContext ctx, IReadOnlyList<ErrorEntry> errors)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return ctx.HttpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(errors, StatusCodes.Status400BadRequest));
    }

    protected static Task NotFound(ErrorHandlerContext ctx)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        return ctx.HttpContext.Response.WriteAsJsonAsync(
            new ErrorResponse([new ErrorEntry("NOT_FOUND", ctx.Exception.Message)], StatusCodes.Status404NotFound));
    }

    protected static Task Forbidden(ErrorHandlerContext ctx)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return ctx.HttpContext.Response.WriteAsJsonAsync(
            new ErrorResponse([new ErrorEntry("FORBIDDEN", ctx.Exception.Message)], StatusCodes.Status403Forbidden));
    }
}
