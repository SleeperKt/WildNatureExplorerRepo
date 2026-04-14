using System.Net;
using System.Text.Json;


namespace WildNatureExplorer.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<ExceptionHandlingMiddleware> logger)
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
            catch (UnauthorizedAccessException ex)
            {
                await WriteError(context, HttpStatusCode.Unauthorized, ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                await WriteError(context, HttpStatusCode.NotFound, ex.Message);
            }
            catch (ArgumentNullException ex)
            {
                await WriteError(context, HttpStatusCode.BadRequest, "Invalid input: " + ex.ParamName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception: {ExceptionType}", ex.GetType().Name);
                await WriteError(context, HttpStatusCode.InternalServerError,
                    "An unexpected error occurred");
            }
        }

        private static async Task WriteError(
            HttpContext context,
            HttpStatusCode statusCode,
            string message)
        {
            // Check if response has already started
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            var response = new
            {
                type = "error",
                title = message,
                status = (int)statusCode
            };

            try
            {
                await context.Response.WriteAsync(
                    JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                // Log if write fails, but don't throw
                System.Diagnostics.Debug.WriteLine($"Failed to write error response: {ex.Message}");
            }
        }
    }
}