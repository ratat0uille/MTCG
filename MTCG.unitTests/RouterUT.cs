//using System.Collections.Generic;
using MTCG.Routing;
using Xunit;

//number of tests: 9

namespace MTCG.unitTests
{
    public class RouterUT
    {
        private readonly Router _router = new();

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_ValidStaticRoute_ReturnsExpectedResult()
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Path = "/",
                Headers = new Dictionary<string, string>()
            };

            var result = _router.Route(request);

            Assert.Equal("Homepage", result);
        }

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_ValidDynamicRoute_ReturnsExpectedResult()
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Path = "/user/123",
                Headers = new Dictionary<string, string>()
            };

            var result = _router.Route(request);

            Assert.Equal("UserProfile", result);
        }

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_UnauthorizedRequest_ReturnsUnauthorized()
        {
            var request = new HttpRequest
            {
                Method = "POST",
                Path = "/register",
                Headers = new Dictionary<string, string>()
            };

            var result = _router.Route(request);

            Assert.Equal("Unauthorized", result);
        }

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_AuthorizedRequest_ReturnsExpectedResult()
        {
            var request = new HttpRequest
            {
                Method = "POST",
                Path = "/register",
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", "Bearer token" }
                }
            };

            var result = _router.Route(request);

            Assert.Equal("Register", result);
        }

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_UnsupportedMethod_ReturnsMethodNotAllowed()
        {
            var request = new HttpRequest
            {
                Method = "PATCH",
                Path = "/",
                Headers = new Dictionary<string, string>()
            };

            var result = _router.Route(request);

            Assert.Equal("MethodNotAllowed", result);
        }

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_InvalidPath_ReturnsNotFound()
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Path = "/invalidpath",
                Headers = new Dictionary<string, string>()
            };

            var result = _router.Route(request);

            Assert.Equal("NotFound", result);
        }

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_NullRequest_ReturnsBadRequest()
        {
            var result = _router.Route(null);

            Assert.Equal("BadRequest", result);
        }

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_EmptyMethod_ReturnsBadRequest()
        {
            var request = new HttpRequest
            {
                Method = "",
                Path = "/",
                Headers = new Dictionary<string, string>()
            };

            var result = _router.Route(request);

            Assert.Equal("BadRequest", result);
        }

        /*--------------------------------------------------------------------------*/

        [Fact]
        public void Route_EmptyPath_ReturnsBadRequest()
        {
            var request = new HttpRequest
            {
                Method = "GET",
                Path = "",
                Headers = new Dictionary<string, string>()
            };

            var result = _router.Route(request);

            Assert.Equal("BadRequest", result);
        }
    }
}
