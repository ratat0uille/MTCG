﻿
//using System.IO;
using System.Text;
//using System.Threading.Tasks;
using MTCG.Routing;
using Xunit;

//number of tests: 4

namespace MTCG.unitTests
{
        public class ParserUT
        {
            /*--------------------------------------------------------------------------*/

            [Fact]
            public async Task ParseAsync_ValidRequest_ReturnsHttpRequest()
            {
                var requestString = "GET /path?name=value HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\n\r\n";
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(requestString));

                var result = await Parser.ParseAsync(stream);

                Assert.Equal("GET", result.Method);
                Assert.Equal("/path", result.Path);
                Assert.Equal("HTTP/1.1", result.Version);
                Assert.Equal("localhost", result.Headers["Host"]);
                Assert.Equal("value", result.QueryParams["name"]);
            }

            /*--------------------------------------------------------------------------*/

            [Fact]
            public async Task ParseAsync_InvalidRequestLine_ThrowsArgumentException()
            {
                var requestString = "INVALID_REQUEST_LINE\r\n";
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(requestString));

                await Assert.ThrowsAsync<ArgumentException>(() => Parser.ParseAsync(stream));
            }

            /*--------------------------------------------------------------------------*/

            [Fact]
            public async Task ParseAsync_MissingRequestLine_ThrowsArgumentException()
            {
                var requestString = "\r\n";
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(requestString));

                await Assert.ThrowsAsync<ArgumentException>(() => Parser.ParseAsync(stream));
            }

            /*--------------------------------------------------------------------------*/

            [Fact]
            public async Task ParseAsync_ValidRequestWithBody_ReturnsHttpRequest()
            {
                var requestString = "POST /path HTTP/1.1\r\nHost: localhost\r\nContent-Length: 11\r\n\r\nHello World";
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(requestString));

                var result = await Parser.ParseAsync(stream);

                Assert.Equal("POST", result.Method);
                Assert.Equal("/path", result.Path);
                Assert.Equal("HTTP/1.1", result.Version);
                Assert.Equal("localhost", result.Headers["Host"]);
                Assert.Equal("Hello World", result.Body);
            }
        }
}