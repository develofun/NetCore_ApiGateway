using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NetCore_Gateway
{
    public class Default : IRoute
    {
        public async Task Invoke(HttpContext context)
        {
            await context.Response.WriteAsync($"Welcome to API Gateway Server. {DateTime.UtcNow}");
        }
    }
}
