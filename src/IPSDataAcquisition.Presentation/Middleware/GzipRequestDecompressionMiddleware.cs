using System.IO.Compression;

namespace IPSDataAcquisition.Presentation.Middleware;

public class GzipRequestDecompressionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GzipRequestDecompressionMiddleware> _logger;

    public GzipRequestDecompressionMiddleware(RequestDelegate next, ILogger<GzipRequestDecompressionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var contentEncoding = context.Request.Headers["Content-Encoding"].ToString();
        
        if (contentEncoding.Contains("gzip", StringComparison.OrdinalIgnoreCase))
        {
            var originalBody = context.Request.Body;
            
            try
            {
                _logger.LogInformation("Decompressing GZIP request from {Path}, Content-Length: {Length}", 
                    context.Request.Path, context.Request.ContentLength);
                
                using var decompressedStream = new GZipStream(originalBody, CompressionMode.Decompress, leaveOpen: true);
                using var memoryStream = new MemoryStream();
                
                await decompressedStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                
                var decompressedSize = memoryStream.Length;
                _logger.LogInformation("GZIP decompressed: {CompressedSize} bytes → {DecompressedSize} bytes", 
                    context.Request.ContentLength, decompressedSize);
                
                context.Request.Body = memoryStream;
                context.Request.Headers.Remove("Content-Encoding");
                context.Request.ContentLength = memoryStream.Length;
                
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to decompress GZIP request from {Path}. Error: {Error}", 
                    context.Request.Path, ex.Message);
                
                context.Response.StatusCode = 400;
                await context.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = $"Failed to decompress request: {ex.Message}",
                    data = (object?)null,
                    error = new
                    {
                        type = ex.GetType().Name,
                        details = ex.ToString()
                    }
                });
            }
            finally
            {
                context.Request.Body = originalBody;
            }
        }
        else
        {
            await _next(context);
        }
    }
}

public static class GzipRequestDecompressionMiddlewareExtensions
{
    public static IApplicationBuilder UseGzipRequestDecompression(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GzipRequestDecompressionMiddleware>();
    }
}

