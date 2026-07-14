namespace Host.ExceptionHandling.Handlers;

internal class InvalidOperationExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is InvalidOperationException)
        {
            await BadRequest(context, "INVALID_OPERATION", context.Exception.Message);
            context.Handled = true;
        }
    }
}
