using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace TheUnraveller.API.Middleware;

public class AdminMiddleware
{
    private readonly RequestDelegate _next;

    public AdminMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply to /api/Admin endpoints
        if (context.Request.Path.StartsWithSegments("/api/Admin"))
        {
            var user = context.User;
            if (user?.Identity?.IsAuthenticated != true || 
                !user.HasClaim(ClaimTypes.Role, "Admin"))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = "Access denied. Admin privileges required." });
                return;
            }
        }

        await _next(context);
    }
}
