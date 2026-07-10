namespace Host.ExceptionHandling.Handlers;

internal class KeyNotFoundExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is KeyNotFoundException)
        {
            context.ErrorMessages.Add(context.Exception.Message);
            await NotFound(context);
            context.Handled = true;
        }
    }
}
