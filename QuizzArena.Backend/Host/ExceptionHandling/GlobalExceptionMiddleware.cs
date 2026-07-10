using FluentValidation;
using Host.ExceptionHandling.Handlers;

namespace Host.ExceptionHandling;

public class GlobalExceptionMiddleware(RequestDelegate next)
{
    private static readonly Dictionary<Type, IErrorHandler> Handlers = new()
    {
        [typeof(InvalidOperationException)]   = new InvalidOperationExceptionHandler(),
        [typeof(UnauthorizedAccessException)] = new UnauthorizedAccessExceptionHandler(),
        [typeof(KeyNotFoundException)]        = new KeyNotFoundExceptionHandler(),
        [typeof(ValidationException)]         = new ValidationExceptionHandler(),
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            ErrorHandlerContext errorContext = new(ex, context);

            if (Handlers.TryGetValue(ex.GetType(), out IErrorHandler? handler))
            {
                await handler.HandleAsync(errorContext);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new List<string> { "An unexpected error occurred." });
            }
        }
    }
}
