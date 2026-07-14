namespace Host.ExceptionHandling.Handlers;

internal abstract class ErrorHandler : IErrorHandler
{
    public abstract Task HandleAsync(ErrorHandlerContext context);

    protected static Task BadRequest(ErrorHandlerContext ctx, string code, string message)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return ctx.HttpContext.Response.WriteAsJsonAsync(
            new ErrorResponse(code, message, StatusCodes.Status400BadRequest));
    }

    protected static Task NotFound(ErrorHandlerContext ctx)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        return ctx.HttpContext.Response.WriteAsJsonAsync(
            new ErrorResponse("NOT_FOUND", ctx.Exception.Message, StatusCodes.Status404NotFound));
    }

    protected static Task Forbidden(ErrorHandlerContext ctx)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return ctx.HttpContext.Response.WriteAsJsonAsync(
            new ErrorResponse("FORBIDDEN", ctx.Exception.Message, StatusCodes.Status403Forbidden));
    }
}
