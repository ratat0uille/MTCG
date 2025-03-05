using System.Net;
using System.Net.Sockets;
using System.Text;
using MTCG.Routing;
using HttpRequest = MTCG.Models.HttpRequest;

namespace MTCG.Server
{
    internal class Server
    {
        private static readonly Router _router = new Router(); 

        /*----------------------------------START-ASYNC-------------------------------------*/
        public static async Task StartAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 10001);
            listener.Start();
            Console.WriteLine("Server started on port 10001...");

            try
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
            finally
            {
                listener.Stop();
                Console.WriteLine("Server stopped.");
            }
        }

        /*--------------------------------HANDLE-CLIENT-ASYNC---------------------------------------*/
        // u gotta implement the different responses
        // connect with database & whatnot 
        // also where do i put game logic etc lol find that out 
        //login logic also somewhere ? idk man
        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8)) //why do i never use this
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {

                    HttpRequest request = await Parser.ParseAsync(stream);

                    string responseBody = _router.Route(request);

                    string httpResponse = $"HTTP/1.1 200 OK\r\n" +
                                          $"Content-Length: {responseBody.Length}\r\n" +
                                          $"Content-Type: text/plain\r\n" +
                                          $"\r\n" +
                                          $"{responseBody}";

                    await writer.WriteAsync(httpResponse);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");
            }
        }
    }
}
