
namespace MTCG
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            await Server.Server.StartAsync();
        }
    }
}
