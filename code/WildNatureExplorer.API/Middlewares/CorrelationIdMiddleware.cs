using Serilog.Context;

namespace WildNatureExplorer.API.Middlewares;

/// <summary>Adds correlation id headers and Serilog enrichment for traceability.</summary>
public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task Invoke(HttpContext ctx)
    {
        var raw = ctx.Request.Headers[HeaderName].FirstOrDefault();
        var id = string.IsNullOrWhiteSpace(raw)
            ? Guid.NewGuid().ToString("n")
            : raw.Trim();

        ctx.Response.Headers[HeaderName] = id;

        using (LogContext.PushProperty("CorrelationId", id))
            await _next(ctx);
    }
}
