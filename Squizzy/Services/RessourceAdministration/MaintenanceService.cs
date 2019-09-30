using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Squizzy.Services
{
    public class MaintenanceService : SquizzyService
    {
#pragma warning disable
        [Inject] private readonly DiscordShardedClient _client;
        [Inject] private readonly BlockingService _blocking;
#pragma warning restore
        private bool MaintenanceEnabled { get; set; }
        private object MaintenanceLock { get; set; }

        public override Task InitializeAsync()
        {
            MaintenanceEnabled = false;
            MaintenanceLock = new object();
            return base.InitializeAsync();
        }

        public async Task EnableMaintenanceAsync()
        {
            await _client.SetStatusAsync(UserStatus.DoNotDisturb);
            await _client.SetGameAsync("Maintenance Break!", type: ActivityType.Watching);
            await WaitForCommandsToEndAsync();

            lock (MaintenanceLock)
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

            lock (MaintenanceLock)
            {
                MaintenanceEnabled = false;
            }
        }

        public bool IsMaintenanceEnabled()
        {
            lock (MaintenanceLock)
            {
                return MaintenanceEnabled;
            }
        }

        private async Task WaitForCommandsToEndAsync()
        {
            while (_blocking.BlockingCount > 0)
            {
                await Task.Delay(100);
            }
        }
    }
}
