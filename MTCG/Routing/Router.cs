
using System.Text.RegularExpressions;
using MTCG.Models;

namespace MTCG.Routing
{
    public class Router
    {
        /*----------------------------------STATIC ROUTING-------------------------------------*/
        private readonly Dictionary<(string Path, string Method), string> _routes = new()
        {
            { ("/", "GET"), "Homepage" },
            { ("/register", "POST"), "Register" },
            { ("/login", "POST"), "Login" }
        };

        /*----------------------------------SUPPORTED METHODS-------------------------------------*/
        private static readonly HashSet<string> SupportedMethods = new() { "GET", "POST", "PUT", "DELETE" };

        /*----------------------------------DYNAMIC ROUTES-------------------------------------*/
        private readonly Dictionary<string, string> _dynamicRoutes = new()
        {
            { "^/user/\\d+$", "UserProfile" }, 
            { "^/post/\\d+$", "PostDetails" } 
        };

        /*----------------------------------MIDDLEWARE-------------------------------------*/
        private readonly Dictionary<string, Func<HttpRequest, string>> _middleware = new()
        {
            { "/register", Authenticate },
            { "/login", Authenticate }
        };

        /*----------------------------------ROUTE METHOD-------------------------------------*/
        public string Route(HttpRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Method) || string.IsNullOrEmpty(request.Path))
                return "BadRequest";

            if (!SupportedMethods.Contains(request.Method))
                return "MethodNotAllowed";

            string basePath = request.Path;

            if (_middleware.TryGetValue(basePath, out var middlewareFunc))
            {
                string authResult = middlewareFunc(request);
                if (authResult == "Unauthorized")
                    return "Unauthorized";  
            }

            if (_routes.TryGetValue((basePath, request.Method), out var result))
                return result;

            foreach (var routePattern in _dynamicRoutes.Keys)
            {
                if (Regex.IsMatch(basePath, routePattern))
                    return _dynamicRoutes[routePattern];
            }

            return "NotFound";
        }

        /*----------------------------------AUTHENTICATE METHOD-------------------------------------*/
        private static string Authenticate(HttpRequest request)
        {
            return request.Headers.ContainsKey("Authorization") ? "Authorized" : "Unauthorized";
        }
    }
}
