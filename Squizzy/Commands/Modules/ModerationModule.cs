using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class ModerationModule : SquizzyModule
    {
        public DbService Db { get; set; }
        public RessourceAdministrationService RessourceAdministration { get; set; }

        [Command("Recalculate", "Recalc")]
        [RequireMaintenance(true)]
        [RequireHelper]
        public async Task RecalculateTrophiesAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithTitle("Recalculation Trophies for all players...")
                .Build();

            await ReplyAsync(embed: embed);

            await Db.RecalculateTrophiesAsync();

            embed = new EmbedBuilder()
                .WithColor(EmbedColor.Success)
                .WithTitle("Recalulate complete!")
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("EnableMaintenance", "StartMaintenance")]
        [RequireMaintenance(false)]
        [RequireHelper]
        public async Task EnableMaintenanceAsync()
        {
            RessourceAdministration.EnableMaintenance();

            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Success)
                .WithTitle("Maintenance mode enabled!")
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("DisableMaintenance", "StopMaintenance")]
        [RequireMaintenance(true)]
        [RequireHelper]
        public async Task DisableMaintenanceAsync()
        {
            RessourceAdministration.DisableMaintenance();

            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Success)
                .WithTitle("Maintenance mode disabled!")
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
