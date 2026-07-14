using FluentValidation;

namespace Host.ExceptionHandling.Handlers;

internal class ValidationExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is ValidationException validationEx)
        {
            string message = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage));
            await BadRequest(context, "VALIDATION_ERROR", message);
            context.Handled = true;
        }
    }
}
