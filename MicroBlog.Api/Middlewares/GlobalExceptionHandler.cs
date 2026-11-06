using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MicroBlog.Api.Middlewares;

// Modern replacement for a hand-written try/catch middleware (.NET 8+):
// the framework's UseExceptionHandler() calls every registered IExceptionHandler in order,
// so the handler is a plain DI-friendly class and the response is a standard RFC 7807 ProblemDetails.
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // The client closed the connection: not a server fault, nothing to send back
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("Request {Method} {Path} was cancelled by the client",
                httpContext.Request.Method, httpContext.Request.Path);
            return true;
        }

        _logger.LogError(exception, "Unhandled exception on {Method} {Path}",
            httpContext.Request.Method, httpContext.Request.Path);

        // Only exceptions that are really the client's fault become 4xx; everything else is a 500
        var (status, title) = exception switch
        {
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Bad request"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = status == StatusCodes.Status500InternalServerError
                ? "https://tools.ietf.org/html/rfc9110#section-15.6.1"
                : "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            // Never leak internals in production: the real message only in Development
            Detail = _env.IsDevelopment()
                ? exception.Message
                : "حدث خطأ داخلي في الخادم. يرجى المحاولة لاحقاً.",
            Instance = httpContext.Request.Path
        };

        if (_env.IsDevelopment())
        {
            problem.Extensions["stackTrace"] = exception.StackTrace;
        }

        // Lets support find the matching log line
        problem.Extensions["traceId"] = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }
}
