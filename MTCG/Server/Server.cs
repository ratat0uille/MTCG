using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System;
using MTCG.Routing;
using HttpRequest = MTCG.Routing.HttpRequest;

namespace MTCG.Server
{
    internal class Server
    {
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
        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using (client)
                using (NetworkStream stream = client.GetStream())
                using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                { 
                    HttpRequest request = await Parser.ParseAsync(stream);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Client error: {ex.Message}");

            }
        }
    }
}

