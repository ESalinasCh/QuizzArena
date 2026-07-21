using FluentValidation;
using Host.ExceptionHandling.Handlers;
using QuizzArena.Quizzing.Domain.Exceptions;

namespace Host.ExceptionHandling;

public class GlobalExceptionMiddleware(RequestDelegate next)
{
    private static readonly DomainExceptionHandler _domainHandler = new();

    private static readonly Dictionary<Type, IErrorHandler> _handlers = new()
    {
        [typeof(InvalidOperationException)] = new InvalidOperationExceptionHandler(),
        [typeof(UnauthorizedAccessException)] = new UnauthorizedAccessExceptionHandler(),
        [typeof(KeyNotFoundException)] = new KeyNotFoundExceptionHandler(),
        [typeof(ValidationException)] = new ValidationExceptionHandler(),
        [typeof(ArgumentException)] = new ArgumentExceptionHandler()
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

            if (_handlers.TryGetValue(ex.GetType(), out IErrorHandler? handler))
            {
                await handler.HandleAsync(errorContext);
            }
            else if (ex is DomainException)
            {
                await _domainHandler.HandleAsync(errorContext);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(
                    new Handlers.ErrorResponse([new Handlers.ErrorEntry("INTERNAL_ERROR", "An unexpected error occurred.")], StatusCodes.Status500InternalServerError));
            }
        }
    }
}
