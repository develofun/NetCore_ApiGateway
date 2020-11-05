using Microsoft.AspNetCore.Http;
using NetCore_Gateway.Database;

namespace NetCore_Gateway.Models
{
    public class Destination
    {
        public string ApiVersion { get; set; }
        public string EndPoint { get; set; }
        public string RequestPath { get; set; }
        public string RoutePath { get; set; }

        public TblRoute RouteInfo { get; set; }

        public Destination(HttpRequest request)
        {
            string requestPath = request.Path.Value;
            string[] paths = requestPath.Substring(1).Split("/");
            ApiVersion = paths[1];
            EndPoint = paths[2];
            RequestPath = EndPoint == "auth" ? string.Join('/', paths[2..]) : string.Join('/', paths[3..]);

            RouteInfo = TblRoute.Find(EndPoint);
            RoutePath = RouteInfo.Path + "/" + RequestPath;
            if (request.QueryString.HasValue) RoutePath += request.QueryString.Value;
        }
    }
}
