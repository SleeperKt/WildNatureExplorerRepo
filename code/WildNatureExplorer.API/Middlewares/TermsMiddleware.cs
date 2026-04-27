using System.Security.Claims;
using System.Net;
using WildNatureExplorer.Application.Interfaces.Repositories;
using WildNatureExplorer.Application.Common;
using WildNatureExplorer.Domain.Entities;

namespace WildNatureExplorer.API.Middlewares;

public class TermsMiddleware
{
    private readonly RequestDelegate _next;

    public TermsMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context, IUserRepository userRepository)
    {
        var endpoint = context.GetEndpoint();
        
        var path = context.Request.Path.Value?.ToLower();

            var bypassPaths = new[]
            {
                "/api/auth/login",
                "/api/auth/register",
                "/api/auth/accept-terms"
            };

            if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
            {
                await _next(context);
                return;
            }

            if (bypassPaths.Any(p => path?.StartsWith(p) == true))
            {
                await _next(context);
                return;
            }

        // пропускаем анонимные эндпоинты (login/register)
        if (endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.AllowAnonymousAttribute>() != null)
        {
            await _next(context);
            return;
        }

        // если нет пользователя — дальше не идём
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            await _next(context);
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid token");
            return;
        }

        var user = await userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("User not found");
            return;
        }

        // 🔥 CORE CHECK
        if (!user.AcceptedTerms || user.TermsVersion != Terms.CurrentVersion)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;

            await context.Response.WriteAsJsonAsync(new
            {
                message = "Terms not accepted",
                status = 403
            });

            return;
        }

        await _next(context);
    }
}