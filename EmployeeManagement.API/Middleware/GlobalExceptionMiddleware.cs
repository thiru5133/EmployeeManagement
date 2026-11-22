using System.Net;
using System.Text.Json;

namespace EmployeeManagement.API.Middleware
{
    /// <summary>
    /// Day 9 — Global Exception Handling Middleware.
    ///
    /// Catches ANY unhandled exception thrown anywhere in the pipeline and
    /// returns a consistent RFC 7807 ProblemDetails JSON response instead of:
    ///   ❌ Raw ASP.NET HTML error page
    ///   ❌ Empty 500 response
    ///
    /// Why first in the pipeline?
    ///   Registered BEFORE all other middleware so it wraps everything —
    ///   even errors from routing, auth, or other middleware are caught.
    ///
    /// Pipeline order:
    ///   Request  → GlobalExceptionMiddleware → RequestLoggingMiddleware → ... → Controller
    ///   Response ← GlobalExceptionMiddleware ← RequestLoggingMiddleware ← ... ← Controller
    ///                     ↑ catches any exception thrown on the way back ↑
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception for {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Map specific exception types to meaningful status codes
            var (statusCode, title) = exception switch
            {
                ArgumentNullException      => (HttpStatusCode.BadRequest,            "Bad Request"),
                ArgumentException          => (HttpStatusCode.BadRequest,            "Bad Request"),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized,         "Unauthorized"),
                KeyNotFoundException       => (HttpStatusCode.NotFound,              "Not Found"),
                NotImplementedException    => (HttpStatusCode.NotImplemented,        "Not Implemented"),
                _                          => (HttpStatusCode.InternalServerError,   "Internal Server Error")
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode  = (int)statusCode;

            var problemDetails = new
            {
                type     = $"https://httpstatuses.io/{(int)statusCode}",
                title    = title,
                status   = (int)statusCode,
                detail   = exception.Message,
                instance = context.Request.Path.Value
            };

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
