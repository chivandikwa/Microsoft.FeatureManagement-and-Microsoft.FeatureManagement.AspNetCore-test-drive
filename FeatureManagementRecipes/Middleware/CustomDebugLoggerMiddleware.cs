using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace FeatureManagementRecipes.Middleware
{
    public class CustomDebugLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        public CustomDebugLoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            Debug.WriteLine("**Handling request: " + context.Request.Path + "**");
            await _next.Invoke(context);
            Debug.WriteLine("**Finished handling request.**");
        }
    }

    public static class CustomDebugLoggerMiddlewareExtension
    {
        public static IApplicationBuilder UseCustomDebugLogger(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomDebugLoggerMiddleware>();
        }
    }
}
