using System.Diagnostics;
using System.Text;

namespace IPSDataAcquisition.Presentation.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var requestQuery = context.Request.QueryString.ToString();

        // Log request
        _logger.LogInformation(
            "HTTP Request: {Method} {Path}{Query} | IP: {IP} | User: {User}",
            requestMethod,
            requestPath,
            requestQuery,
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            context.User?.Identity?.Name ?? "anonymous");

        // Capture response
        var originalBodyStream = context.Response.Body;
        
        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            stopwatch.Stop();
            
            var statusCode = context.Response.StatusCode;
            var duration = stopwatch.ElapsedMilliseconds;

            // Read response body (only for errors or if needed)
            string? responseContent = null;
            if (statusCode >= 400 && responseBody.Length > 0)
            {
                responseBody.Seek(0, SeekOrigin.Begin);
                responseContent = await new StreamReader(responseBody).ReadToEndAsync();
            }

            // Log response
            var logLevel = statusCode >= 500 ? LogLevel.Error :
                          statusCode >= 400 ? LogLevel.Warning :
                          LogLevel.Information;

            _logger.Log(logLevel,
                "HTTP Response: {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms | IP: {IP}" +
                (responseContent != null ? " | Response: {Response}" : ""),
                requestMethod,
                requestPath,
                statusCode,
                duration,
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                responseContent);

            // IMPORTANT: Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in request/response logging middleware");
            throw;
        }
        finally
        {
            // Restore original stream
            context.Response.Body = originalBodyStream;
        }
    }
}

public static class RequestResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
    }
}

