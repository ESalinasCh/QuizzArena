namespace Host.ExceptionHandling.Handlers;

internal sealed class ArgumentExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is ArgumentException)
        {
            await BadRequest(context, "INVALID_ARGUMENT", context.Exception.Message);
            context.Handled = true;
        }
    }
}
