using FluentValidation;

namespace Host.ExceptionHandling.Handlers;

internal class ValidationExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is ValidationException validationEx)
        {
            context.ErrorMessages = validationEx.Errors
                .Select(e => e.ErrorMessage)
                .ToList();
            await BadRequest(context, context.ErrorMessages);
            context.Handled = true;
        }
    }
}
