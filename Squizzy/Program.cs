using System;
using System.Threading.Tasks;
using Discord.Net;

namespace Squizzy
{
    public class Program
    {
        private static SquizzySetup Bot;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Stil", "IDE0060:Nicht verwendete Parameter entfernen", Justification = "<Ausstehend>")]
        private static void Main(string[] args)
        {
            Bot = new SquizzySetup();

            try
            {
                Task.Run(async () =>
                {
                    await Bot.InitializeAsync();
                    await Bot.RunAsync();
                })
                .GetAwaiter().GetResult();
            }
            catch (HttpException)
            {
                Console.WriteLine("Connection Failure!");
                Console.ReadLine();
            }
        }
    }
}
