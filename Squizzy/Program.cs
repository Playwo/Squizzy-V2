using System.Threading.Tasks;

namespace Squizzy
{
    public class Program
    {
        private static SquizzySetup Bot;

        private static void Main(string[] args)
        {
            Bot = new SquizzySetup();

            Task.Run(async () =>
            {
                await Bot.InitializeAsync();
                await Bot.RunAsync();
            })
            .GetAwaiter().GetResult();
        }
    }
}
