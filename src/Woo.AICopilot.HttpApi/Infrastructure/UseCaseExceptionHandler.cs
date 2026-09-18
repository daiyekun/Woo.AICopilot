using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Woo.AICopilot.Services.Common.Exceptions;

namespace Woo.AICopilot.HttpApi.Infrastructure;

public class UseCaseExceptionHandler : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers = new()
    {
        { typeof(ForbiddenException), HandleForbiddenExceptionAsync }
    };

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        if (!_exceptionHandlers.TryGetValue(exceptionType, out var handler)) return false;

        await handler.Invoke(httpContext, exception);
        return true;
    }

    private static async Task HandleForbiddenExceptionAsync(HttpContext httpContext, Exception exception)
    {
        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Type = "https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Status/403",
            Detail = exception.Message
        });
    }
}