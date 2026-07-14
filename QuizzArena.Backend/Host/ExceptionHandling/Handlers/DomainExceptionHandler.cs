using QuizzArena.Quizzing.Domain.Exceptions;

namespace Host.ExceptionHandling.Handlers;

internal sealed class DomainExceptionHandler : ErrorHandler
{
    public override async Task HandleAsync(ErrorHandlerContext context)
    {
        if (context.Exception is DomainException domainEx)
        {
            await BadRequest(context, domainEx.Code, domainEx.Message);
            context.Handled = true;
        }
    }
}
