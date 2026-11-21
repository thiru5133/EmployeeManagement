using System.Diagnostics;

namespace EmployeeManagement.API.Middleware
{
    /// <summary>
    /// Custom middleware that logs every incoming HTTP request and its response.
    ///
    /// Day 7 concept: Middleware sits in the ASP.NET Core pipeline. Every request
    /// passes through it in order, and the response passes back through in reverse.
    ///
    /// Pipeline order (each arrow = "await _next(context)"):
    ///   Request  → RequestLoggingMiddleware → UseRouting → UseAuthorization → Controller
    ///   Response ← RequestLoggingMiddleware ← UseRouting ← UseAuthorization ← Controller
    /// </summary>
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        // ILogger<T> and RequestDelegate are injected by the DI container automatically.
        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // --- Before the next middleware/controller runs ---
            var stopwatch = Stopwatch.StartNew();
            var method = context.Request.Method;
            var path = context.Request.Path;

            _logger.LogInformation("➡️  Incoming Request: {Method} {Path}", method, path);

            // Pass control to the next middleware in the pipeline
            await _next(context);

            // --- After the controller has responded ---
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsed = stopwatch.ElapsedMilliseconds;

            // Log level depends on status code
            if (statusCode >= 500)
            {
                _logger.LogError(
                    "⬅️  Response: {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                    method, path, statusCode, elapsed);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning(
                    "⬅️  Response: {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                    method, path, statusCode, elapsed);
            }
            else
            {
                _logger.LogInformation(
                    "⬅️  Response: {Method} {Path} → {StatusCode} ({Elapsed}ms)",
                    method, path, statusCode, elapsed);
            }
        }
    }
}
