using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Confirmation;
using Microsoft.Extensions.Configuration;
using Qmmands;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Moderation :pushpin: ")]
    public class ModerationModule : SquizzyModule
    {
        public DbService Db { get; set; }
        public InteractivityService Interactivity { get; set; }
        public MaintenanceService Maintenance { get; set; }
        public IConfigurationRoot Config { get; set; }


        [Command("Recalculate", "Recalc")]
        [Description("Recalculate all Trophies (After DB Change)")]
        [RequireMaintenance(true)]
        [RequireFreeRessources(RessourceType.Global)]
        [RequireHelper]
        public async Task RecalculateTrophiesAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithTitle("Recalculation Trophies for all players...")
                .Build();

            await ReplyAsync(embed: embed);

            await Db.RecalculateAllPlayerTrophiesAsync();

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
                .WithDeletion(DeletionOptions.AfterCapturedContext)
                .Build();

            var result = await Interactivity.SendConfirmationAsync(confirmation, Context.Channel);

            if (result.Value)
            {
                var msg = await ReplyAsync("Waiting for all commands to end...");

                await Maintenance.EnableMaintenanceAsync();

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
            await Maintenance.DisableMaintenanceAsync();

            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Success)
                .WithTitle("Maintenance mode disabled!")
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("Report", "Feedback", "BugReport", "SendFeedback", "GiveFeedback")]
        [Description("Sends feedback to the developer")]
        [RequireFinishedCooldown]
        [Save]
        public async Task ReportAsync([Name("Message")][Remainder]string message)
        {
            var reportChannel = Context.Client.GetChannel(ulong.Parse(Config["feedbackChannel"])) as ISocketMessageChannel;

            var content = new PageBuilder()
                .WithColor(EmbedColor.Question)
                .WithTitle("Send report to the Developer?")
                .WithDescription("Please dont use this feature to spam!");

            var request = new ConfirmationBuilder()
                .WithContent(content)
                .WithUsers(Context.User)
                .WithDeletion(DeletionOptions.AfterCapturedContext | DeletionOptions.Invalids)
                .WithCancelledEmbed(new EmbedBuilder()
                    .WithColor(EmbedColor.Failed)
                    .WithTitle("Report cancelled!")
                    .WithDescription(message))
                .Build();

            var result = await Interactivity.SendConfirmationAsync(request, Context.Channel);

            if (result.Value)
            {
                await reportChannel.SendMessageAsync(embed: new EmbedBuilder()
                    .WithTitle($"Report by {Context.User.Username}")
                    .WithDescription(message)
                    .Build());
                await ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(EmbedColor.Success)
                    .WithTitle("Your report has been transmitted! :white_check_mark:")
                    .WithDescription(message)
                    .Build());

                Context.Player.Cooldown.SetCommandCooldown(Context.Command, TimeSpan.FromMinutes(5));
            }
        }
    }
}
