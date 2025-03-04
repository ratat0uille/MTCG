

using System.Reflection.PortableExecutable;
using System.Text;

namespace MTCG.Routing
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }
    }

    public class Parser // converts raw request into object
    {
        public static async Task<HttpRequest> ParseAsync(Stream stream)
        {
            using (var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true))
            {
                var request = new HttpRequest();
                var requestLine = await reader.ReadLineAsync() ?? throw new ArgumentException("Invalid HTTP request: missing request line.");
                var requestLineParts = requestLine.Split(' ', 3);
                if (requestLineParts.Length != 3)
                    throw new ArgumentException("Invalid HTTP request: invalid request line.");

                request.Method = requestLineParts[0];
                request.Path = requestLineParts[1];
                request.Version = requestLineParts[2];

                string line;
                while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
                {
                    int separatorIndex = line.IndexOf(": ", StringComparison.OrdinalIgnoreCase);
                    if (separatorIndex == -1)
                        throw new ArgumentException($"Invalid HTTP header: {line}");

                    string headerName = line[..separatorIndex]; 
                    string headerValue = line[(separatorIndex + 2)..]; 

                    request.Headers.TryAdd(headerName, headerValue); 
                }

                if (request.Headers.TryGetValue("Content-Length", out string? contentLengthValue) &&
                    int.TryParse(contentLengthValue, out int contentLength) && contentLength > 0)
                {
                    if (contentLength < 0)
                        throw new ArgumentException("Invalid Content-Length value.");

                    char[] buffer = new char[contentLength];
                    int totalRead = 0;

                    while (totalRead < contentLength)
                    {
                        int read = await reader.ReadAsync(buffer, totalRead, contentLength - totalRead);
                        if (read == 0) throw new EndOfStreamException("Unexpected end of stream while reading body.");
                        totalRead += read;
                    }

                    request.Body = new string(buffer, 0, totalRead);
                }
                else
                {
                    request.Body = string.Empty;
                } 

                return request;
            } 
            
        }
    }
}