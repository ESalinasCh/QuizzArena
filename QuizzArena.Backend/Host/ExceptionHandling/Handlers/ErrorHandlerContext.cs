namespace Host.ExceptionHandling.Handlers;

internal class ErrorHandlerContext
{
    public Exception Exception { get; }
    public bool Handled { get; set; }
    public List<string> ErrorMessages { get; set; } = [];
    public HttpContext HttpContext { get; }

    public ErrorHandlerContext(Exception exception, HttpContext httpContext)
    {
        Exception = exception;
        HttpContext = httpContext;
    }
}
