using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace NetCore_Gateway
{
    public class RouteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RouteMiddleware> _log;

        private readonly Dictionary<string, IRoute> routes;

        public RouteMiddleware(RequestDelegate next, ILogger<RouteMiddleware> log)
        {
            _next = next;
            _log = log;

            routes = new Dictionary<string, IRoute>
            {
                { "/api", new Route() },
                { "/", new Default() }
            };
        }

        public async Task Invoke(HttpContext context)
        {
            var endPoint = GenerateEndPoint(context.Request.Path.Value);

            if (routes.TryGetValue(endPoint, out IRoute route))
                await route.Invoke(context);
            else
                await _next.Invoke(context);
        }

        private string GenerateEndPoint(string path)
        {
            if (path.Split("/").Length <= 2)
                return path;
            else
                return path.Substring(0, path.IndexOf("/", 1));
        }
    }
}
