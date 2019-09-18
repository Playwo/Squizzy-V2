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
            Console.ForegroundColor = arg.Severity switch
            {
                LogSeverity.Critical => ConsoleColor.DarkRed,
                LogSeverity.Error => ConsoleColor.Red,
                LogSeverity.Warning => ConsoleColor.Yellow,
                LogSeverity.Info => ConsoleColor.Blue,
                _ => ConsoleColor.Green,
            };
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
