namespace Host.ExceptionHandling.Handlers;

internal interface IErrorHandler
{
    Task HandleAsync(ErrorHandlerContext context);
}
