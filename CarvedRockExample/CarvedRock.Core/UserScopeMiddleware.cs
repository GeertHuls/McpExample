using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CarvedRock.Core;

public class UserScopeMiddleware(RequestDelegate next, ILogger<UserScopeMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity is { IsAuthenticated: true })
        {
            var user = context.User;
            var subjectId = user.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "sub claim not found";

            using (logger.BeginScope("User:{user}, SubjectId:{subject}", user.Identity.Name??"", subjectId))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}
