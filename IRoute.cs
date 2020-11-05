using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace NetCore_Gateway
{
    public interface IRoute
    {
        Task Invoke(HttpContext context);
    }
}
