namespace Host.ExceptionHandling.Handlers;

internal abstract class ErrorHandler : IErrorHandler
{
    public abstract Task HandleAsync(ErrorHandlerContext context);

    protected static Task BadRequest(ErrorHandlerContext ctx, List<string> errors)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        return ctx.HttpContext.Response.WriteAsJsonAsync(errors);
    }

    protected static Task NotFound(ErrorHandlerContext ctx)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        return ctx.HttpContext.Response.WriteAsJsonAsync(new List<string> { ctx.Exception.Message });
    }

    protected static Task Forbidden(ErrorHandlerContext ctx)
    {
        ctx.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return ctx.HttpContext.Response.WriteAsJsonAsync(new List<string> { ctx.Exception.Message });
    }
}
