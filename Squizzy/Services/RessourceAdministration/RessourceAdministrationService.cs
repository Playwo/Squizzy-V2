using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;
using Squizzy.Commands;

namespace Squizzy.Services
{
    public class RessourceAdministrationService : SquizzyService
    {
        private ConcurrentDictionary<ulong, bool> OccupiedUsers { get; set; }
        private ConcurrentDictionary<ulong, bool> OccupiedChannels { get; set; }
        private ConcurrentDictionary<ulong, bool> OccupiedGuilds { get; set; }
        private bool MaintenanceEnabled { get; set; }
        private readonly object maintenanceLock = new object();

        public override Task InitializeAsync()
        {
            MaintenanceEnabled = false;
            OccupiedUsers = new ConcurrentDictionary<ulong, bool>();
            OccupiedChannels = new ConcurrentDictionary<ulong, bool>();
            OccupiedGuilds = new ConcurrentDictionary<ulong, bool>();
            return Task.CompletedTask;
        }

        public bool IsUserOccupied(SocketUser user) 
            => OccupiedUsers.TryGetValue(user.Id, out _);

        public bool IsChannelOccupied(SocketChannel channel)
            => OccupiedChannels.TryGetValue(channel.Id, out _);

        public bool IsGuildOccupied(SocketGuild guild)
            => OccupiedGuilds.TryGetValue(guild.Id, out _);

        public void OccupieUser(SocketUser user)
            => OccupiedUsers.TryAdd(user.Id, true);

        public void OccupieChannel(SocketChannel channel)
            => OccupiedChannels.TryAdd(channel.Id, true);

        public void OccupieGuild(SocketGuild guild)
            => OccupiedGuilds.TryAdd(guild.Id, true);

        public void UnOccupieUser(SocketUser user)
            => OccupiedUsers.TryRemove(user.Id, out _);

        public void UnOccupieChannel(SocketChannel channel)
            => OccupiedChannels.TryRemove(channel.Id, out _);

        public void UnOccupieGuild(SocketGuild guild)
            => OccupiedGuilds.TryRemove(guild.Id, out _);

        public void EnableMaintenance()
        {
            lock (maintenanceLock)
            {
                MaintenanceEnabled = true;
            }
        }

        public void DisableMaintenance()
        {
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
    }
}
