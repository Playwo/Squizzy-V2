using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Squizzy.Services
{
    public class LoggerService : SquizzyService
    {
#pragma warning disable
        [Inject] private readonly DiscordShardedClient _client;
        [Inject] private readonly IConfigurationRoot _config;
        [Inject] private readonly EmbedService _embed;
#pragma warning restore

        public override Task InitializeAsync()
        {
            _client.Log += LogAsync;
            return base.InitializeAsync();
        }

        public Task LogAsync(LogMessage arg)
        {
            switch (arg.Severity)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
            }
            Console.WriteLine(arg.ToString(builder: null));
            return Task.CompletedTask;
        }

        public async Task ReportErrorAsync(SocketMessage msg, Exception ex)
        {
            var errorLogChannel = _client.GetChannel(ulong.Parse(_config["errorLogChannel"])) as ISocketMessageChannel;
            var errorEmbed = _embed.GetExceptionEmbed(msg, ex);
            await errorLogChannel.SendMessageAsync(embed: errorEmbed);

            var sorryEmbed = _embed.GetSorryEmbed();
            await msg.Channel.SendMessageAsync(embed: sorryEmbed);
        }
    }
}
