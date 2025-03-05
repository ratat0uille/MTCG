using MTCG.Models;
using System.Text;

namespace MTCG.Routing
{

    public class Parser
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

                string[] pathParts = requestLineParts[1].Split('?');
                request.Path = pathParts[0];

                if (pathParts.Length > 1)
                    request.QueryParams = ParseQueryString(pathParts[1]);

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

                request.Body = await ReadBodyAsync(reader, request.Headers);
                return request;
            }
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var queryParams = new Dictionary<string, string>();

            foreach (var param in query.Split('&'))
            {
                var parts = param.Split('=');
                if (parts.Length == 2)
                    queryParams[parts[0]] = Uri.UnescapeDataString(parts[1]); 
            }

            return queryParams;
        }

        private static async Task<string> ReadBodyAsync(StreamReader reader, Dictionary<string, string> headers)
        {
            if (headers.TryGetValue("Content-Length", out string? contentLengthValue) &&
                int.TryParse(contentLengthValue, out int contentLength) && contentLength > 0)
            {
                char[] buffer = new char[contentLength];
                int totalRead = 0;

                while (totalRead < contentLength)
                {
                    int read = await reader.ReadAsync(buffer, totalRead, contentLength - totalRead);
                    if (read == 0) throw new EndOfStreamException("Unexpected end of stream while reading body.");
                    totalRead += read;
                }

                return new string(buffer, 0, totalRead);
            }
            else if (headers.TryGetValue("Transfer-Encoding", out string? transferEncoding) &&
                     transferEncoding.Equals("chunked", StringComparison.OrdinalIgnoreCase))
            {
                return await ReadChunkedBodyAsync(reader);
            }
            else
            {
                return await ReadUntilEOFAsync(reader);
            }
        }

        private static async Task<string> ReadChunkedBodyAsync(StreamReader reader)
        {
            StringBuilder bodyBuilder = new StringBuilder();

            while (true)
            {
                string? chunkSizeLine = await reader.ReadLineAsync();
                if (chunkSizeLine == null) throw new EndOfStreamException("Unexpected end of stream while reading chunk size.");

                if (!int.TryParse(chunkSizeLine, System.Globalization.NumberStyles.HexNumber, null, out int chunkSize))
                    throw new ArgumentException($"Invalid chunk size: {chunkSizeLine}");

                if (chunkSize == 0) break; 

                char[] chunkBuffer = new char[chunkSize];
                int totalChunkRead = 0;
                while (totalChunkRead < chunkSize)
                {
                    int read = await reader.ReadAsync(chunkBuffer, totalChunkRead, chunkSize - totalChunkRead);
                    if (read == 0) throw new EndOfStreamException("Unexpected end of stream while reading chunk.");
                    totalChunkRead += read;
                }

                bodyBuilder.Append(chunkBuffer, 0, totalChunkRead);
                await reader.ReadLineAsync(); 
            }

            return bodyBuilder.ToString();
        }

        private static async Task<string> ReadUntilEOFAsync(StreamReader reader)
        {
            char[] buffer = new char[1024]; 
            StringBuilder bodyBuilder = new StringBuilder();
            int read;
            while ((read = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                bodyBuilder.Append(buffer, 0, read);
            }

            return bodyBuilder.Length > 0 ? bodyBuilder.ToString() : string.Empty;
        }
    }
}
