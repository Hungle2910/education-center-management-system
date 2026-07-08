using System.Net;
using System.Diagnostics;
using EducationCenter.Crm.Api.Contracts;
using Microsoft.AspNetCore.Diagnostics;

namespace EducationCenter.Crm.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        _logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}. TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            traceId);

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Fail(
            "Đã xảy ra lỗi hệ thống.",
            traceId: traceId);
        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}
