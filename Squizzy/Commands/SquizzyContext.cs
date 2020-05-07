using System;
using Discord.WebSocket;
using Qmmands;
using Squizzy.Entities;

namespace Squizzy.Commands
{
    public class SquizzyContext : CommandContext
    {
        public ISocketMessageChannel Channel { get; }
        public DiscordSocketClient Client { get; }
        public SocketGuild Guild { get; }
        public SocketUserMessage Message { get; }
        public SocketUser User { get; }
        public SquizzyPlayer Player { get; }
        public ulong CommandId => Message.Id;

        public SquizzyContext(DiscordSocketClient shard, SocketUserMessage msg, SocketGuild guild, SquizzyPlayer player, IServiceProvider provider)
            : base(provider)
        {
            Client = shard;
            Guild = guild;
            Channel = msg.Channel;
            User = msg.Author;
            Message = msg;
            Player = player;
        }
    }
}
