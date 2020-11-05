using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NetCore_Gateway.Managers;
using NetCore_Gateway.Models;
using Newtonsoft.Json;

namespace NetCore_Gateway
{
    public class Route : IRoute
    {
        private Http _http;

        public Route()
        {
            _http = new Http();
        }

        public async Task Invoke(HttpContext context)
        {
            var responseMessage = await RouteRequest(context.Request);
            context.Response.StatusCode = (int)responseMessage.StatusCode;
            await context.Response.WriteAsync(await responseMessage.Content.ReadAsStringAsync());
        }

        public async Task<HttpResponseMessage> RouteRequest(HttpRequest request)
        {
            var destination = new Destination(request);

            if (destination.RouteInfo.RequireAuth)
            {
                var authResponse = await _http.RequestAuthAsync(request.Headers["Authorization"]);

                if (!authResponse.IsSuccessStatusCode)
                {
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("Authentication failed.") };
                }
                else
                {
                    var permission = await authResponse.Content.ReadAsStringAsync();

                    if (!AuthorizationManager.CheckEndPointAuth(destination.RequestPath, request.Method, permission))
                        return new HttpResponseMessage { StatusCode = HttpStatusCode.Unauthorized, Content = new StringContent("Have no authorization.") };
                }
            }

            return await _http.RequestAsync(request, destination);
        }
    }
}
