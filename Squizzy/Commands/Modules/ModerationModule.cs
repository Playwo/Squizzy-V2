using System.Threading.Tasks;
using Discord;
using InteractivityAddon;
using InteractivityAddon.Confirmation;
using Qmmands;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Moderation :pushpin: ")]
    public class ModerationModule : SquizzyModule
    {
        public DbService Db { get; set; }
        public InteractivityService Interactivity { get; set; }
        public RessourceAdministrationService RessourceAdministration { get; set; }

        [Command("Recalculate", "Recalc")]
        [Description("Recalculate all Trophies (After DB Change)")]
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

        [Command("EnableMaintenance", "StartMaintenance", "EnMain", "EnableMain", "EM")]
        [Description("Enters the Maintenance Mode")]
        [RequireMaintenance(false)]
        [RequireHelper]
        public async Task EnableMaintenanceAsync()
        {
            var content = new PageBuilder()
                .WithColor(EmbedColor.Question)
                .WithTitle("Do you want to enable Maintenance Mode?")
                .WithDescription("Only helpers will be able to run commands until you disable it!");

            var confirmation = new ConfirmationBuilder()
                .WithContent(content)
                .WithUsers(Context.User)
                .WithDeletion(DeletionOption.AfterCapturedContext)
                .Build();

            var result = await Interactivity.GetUserConfirmationAsync(confirmation, Context.Channel);

            if (result.Value)
            {
                var msg = await ReplyAsync("Waiting for all commands to end...");

                await RessourceAdministration.EnableMaintenanceAsync();

                var embed = new EmbedBuilder()
                    .WithColor(EmbedColor.Success)
                    .WithTitle("Maintenance mode enabled!")
                    .Build();

                await msg.ModifyAsync(x =>
                {
                    x.Content = "";
                    x.Embed = embed;
                });
            }

        }

        [Command("DisableMaintenance", "StopMaintenance", "DisMain", "DisableMain", "DM")]
        [Description("Disables the Maintenance Mode")]
        [RequireMaintenance(true)]
        [RequireHelper]
        public async Task DisableMaintenanceAsync()
        {
            await RessourceAdministration.DisableMaintenanceAsync();

            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Success)
                .WithTitle("Maintenance mode disabled!")
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
