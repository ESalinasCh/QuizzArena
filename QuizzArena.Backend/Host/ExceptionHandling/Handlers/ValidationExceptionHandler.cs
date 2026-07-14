using FluentValidation;

namespace Host.ExceptionHandling.Handlers;

internal sealed class ValidationExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is ValidationException validationEx)
        {
            IReadOnlyList<ErrorEntry> errors = validationEx.Errors
                .Select(e => new ErrorEntry("VALIDATION_ERROR", e.ErrorMessage))
                .ToList();
            await BadRequest(context, errors);
            context.Handled = true;
        }
    }
}
