using System.Text.Json;

namespace Backend.Services
{
    public class MiddleWare
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MiddleWare> _logger;

        public MiddleWare(RequestDelegate next, ILogger<MiddleWare> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AppException ex)
            {
                _logger.LogWarning(ex, "Application error — {Code}: {Message}", ex.Code, ex.Message);

                context.Response.ContentType = "application/json";

                if (ex is ValidationException)
                    context.Response.StatusCode = 400;
                else if (ex is NotFoundException)
                    context.Response.StatusCode = 404;
                else
                    context.Response.StatusCode = 400;

                var response = new
                {
                    code = ex.Code,
                    message = ex.Message
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 500;

                var response = new
                {
                    code = "INTERNAL_ERROR",
                    message = "Errore interno del server"
                };

                var json = JsonSerializer.Serialize(response);
                await context.Response.WriteAsync(json);
            }
        }
    }

    public class AppException : Exception
    {
        public string Code { get; }

        public AppException(string code, string message) : base(message)
        {
            Code = code;
        }
    }

    public class ValidationException : AppException
    {
        public ValidationException(string message)
            : base("VALIDATION_ERROR", message) { }
    }

    public class NotFoundException : AppException
    {
        public NotFoundException(string message)
            : base("NOT_FOUND", message) { }
    }

}
