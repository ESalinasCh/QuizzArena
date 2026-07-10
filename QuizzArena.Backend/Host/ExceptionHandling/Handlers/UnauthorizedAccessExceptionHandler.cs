namespace Host.ExceptionHandling.Handlers;

internal class UnauthorizedAccessExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is UnauthorizedAccessException)
        {
            context.ErrorMessages.Add(context.Exception.Message);
            await Forbidden(context);
            context.Handled = true;
        }
    }
}
