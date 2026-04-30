using System.Net;
using System.Text.Json;
using WildNatureExplorer.API.Middlewares;
using WildNatureExplorer.Application.Common;

namespace WildNatureExplorer.API.Middlewares;

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
        catch (SafetyPolicyException ex)
        {
            await WriteError(context, HttpStatusCode.BadRequest, ex.Message, "SAFETY_POLICY_VIOLATION", "SafetyPolicyException");
        }
        catch (InvalidAiSessionException ex)
        {
            await WriteError(context, HttpStatusCode.BadRequest, ex.Message, "AI_SESSION_INVALID", "InvalidAiSessionException");
        }
        catch (ValidationException ex)
        {
            await WriteError(context, HttpStatusCode.UnprocessableEntity, ex.Message, ex.ErrorCode ?? "VALIDATION_ERROR", "ValidationException");
        }
        catch (ResourceNotFoundException ex)
        {
            await WriteError(context, HttpStatusCode.NotFound, ex.Message, "RESOURCE_NOT_FOUND", "ResourceNotFoundException");
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteError(context, HttpStatusCode.Unauthorized, ex.Message, "UNAUTHORIZED", "UnauthorizedAccessException");
        }
        catch (KeyNotFoundException ex)
        {
            await WriteError(context, HttpStatusCode.NotFound, ex.Message, "NOT_FOUND", "KeyNotFoundException");
        }
        catch (ArgumentException ex)
        {
            await WriteError(context, HttpStatusCode.BadRequest, ex.Message, "INVALID_ARGUMENT", "ArgumentException");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {ExceptionType}", ex.GetType().Name);

            await WriteError(
                context,
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred",
                "INTERNAL_SERVER_ERROR",
                ex.GetType().Name);
        }
    }

    private static async Task WriteError(
        HttpContext context,
        HttpStatusCode statusCode,
        string message,
        string? errorCode = null,
        string? errorType = null)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse
        {
            Message = message,
            Status = (int)statusCode,
            TraceId = context.TraceIdentifier,
            ErrorCode = errorCode,
            ErrorType = errorType
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}