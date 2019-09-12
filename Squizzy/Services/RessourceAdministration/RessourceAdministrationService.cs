using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Squizzy.Commands;

namespace Squizzy.Services
{
    public class RessourceAdministrationService : SquizzyService
    {
#pragma warning disable
        [Inject] private readonly DiscordShardedClient _client;
#pragma warning restore

        private ConcurrentDictionary<ulong, bool> BlockedUsers { get; set; }
        private ConcurrentDictionary<ulong, bool> BlockedChannels { get; set; }
        private ConcurrentDictionary<ulong, bool> BlockedGuilds { get; set; }
        private bool MaintenanceEnabled { get; set; }
        private readonly object maintenanceLock = new object();

        public int RunningOccupations => BlockedUsers.Count + BlockedChannels.Count + BlockedGuilds.Count;


        public override Task InitializeAsync()
        {
            MaintenanceEnabled = false;
            BlockedUsers = new ConcurrentDictionary<ulong, bool>();
            BlockedChannels = new ConcurrentDictionary<ulong, bool>();
            BlockedGuilds = new ConcurrentDictionary<ulong, bool>();
            return Task.CompletedTask;
        }

        public bool IsUserOccupied(SocketUser user) 
            => BlockedUsers.TryGetValue(user.Id, out _);

        public bool IsChannelOccupied(SocketChannel channel)
            => BlockedChannels.TryGetValue(channel.Id, out _);

        public bool IsGuildOccupied(SocketGuild guild)
            => BlockedGuilds.TryGetValue(guild.Id, out _);

        public void OccupieUser(SocketUser user)
            => BlockedUsers.TryAdd(user.Id, true);

        public void OccupieChannel(SocketChannel channel)
            => BlockedChannels.TryAdd(channel.Id, true);

        public void OccupieGuild(SocketGuild guild)
            => BlockedGuilds.TryAdd(guild.Id, true);

        public void UnOccupieUser(SocketUser user)
            => BlockedUsers.TryRemove(user.Id, out _);

        public void UnOccupieChannel(SocketChannel channel)
            => BlockedChannels.TryRemove(channel.Id, out _);

        public void UnOccupieGuild(SocketGuild guild)
            => BlockedGuilds.TryRemove(guild.Id, out _);

        public async Task EnableMaintenanceAsync()
        {
            await _client.SetStatusAsync(UserStatus.DoNotDisturb);
            await _client.SetGameAsync("Maintenance Break!", type: ActivityType.Watching);
            await WaitForCommandsToEndAsync();

            lock (maintenanceLock)
            {
                MaintenanceEnabled = true;
            }
        }

        public async Task DisableMaintenanceAsync()
        {
            if (!MaintenanceEnabled)
            {
                return;
            }

            await _client.SetGameAsync("Answering Questions", type: ActivityType.Playing);
            await _client.SetStatusAsync(UserStatus.Online);

            lock (maintenanceLock)
            {
                MaintenanceEnabled = false;
            }
        }

        public bool IsMaintenanceEnabled()
        {
            lock (maintenanceLock)
            {
                return MaintenanceEnabled;
            }    
        }

        public void UnOccupieContext(SquizzyContext context)
        {
            UnOccupieGuild(context.Guild);
            UnOccupieChannel(context.Channel as SocketChannel);
            UnOccupieUser(context.User);
        }

        private async Task WaitForCommandsToEndAsync()
        {
            while (RunningOccupations > 0)
            {
                await Task.Delay(100);
            }
        }
    }
}
