namespace Host.ExceptionHandling.Handlers;

internal class InvalidOperationExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is InvalidOperationException)
        {
            context.ErrorMessages.Add(context.Exception.Message);
            await BadRequest(context, context.ErrorMessages);
            context.Handled = true;
        }
    }
}
