using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using InteractivityAddon;
using InteractivityAddon.Confirmation;
using InteractivityAddon.Pagination;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class UpgradeModule : SquizzyModule
    {
        public UpgradeService Upgrade { get; set; }
        public InteractivityService Interactivity { get; set; }
        public DbService Db { get; set; }

        [Command("UnlockMultiplayer", "MultiplayerUnlock", "MUnlock", "UnlockM", "BuyMultiplayer", "MultiplayerAccess", "AccessMultiplayer")]
        [Description("Buy the access to multiplayer modes for 500 <:magnet:440898600738750465>")]
        [RequireFreeRessources(RessourceType.User)]
        [RequireMultiplayer(false)]
        [RequireMagnets(minimum: 500)]
        [Save]
        public Task UnlockMultiplayerAsync()
        {
            Context.Player.Magnets -= 500;
            Context.Player.HasMultiplayer = true;

            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Success)
                .WithAuthor(Context.User)
                .WithTitle($"Congratulations, {Context.User} :confetti_ball: :confetti_ball: ")
                .WithDescription("You just got access to the multiplayer section of the bot.\n" +
                                 "Enjoy playing against your friends and enemies to see who is better at quizzing :D")
                .Build();

            return ReplyAsync(embed: embed);
        }

        [Command("ListUpgrades", "Upgrades", "GetUpgrades", "AllUpgrades", "UpgradeList")]
        [Description("Get a list of all available upgrades")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        public Task ListUpgradesAsync()
        {
            var upgrades = Upgrade.GetAllMagnetUpgrades();

            var pages = new List<PageBuilder>();

            foreach(var upgrade in upgrades)
            {
                pages.Add(new PageBuilder()
                    .WithColor(EmbedColor.Statistic)
                    .WithTitle($"{upgrade.Name} # {upgrade.Id}")
                    .WithDescription($"{upgrade.Description}")
                    .AddField("Shortcuts", string.Join(", ", upgrade.NameShortcuts)));
            }

            var paginator = new PaginatorBuilder()
                .WithPages(pages.ToArray())
                .WithUsers(Context.User)
                .Build();

            return Interactivity.SendPaginatorAsync(paginator, Context.Channel);
        }

        [Command("Upgrade", "Buy")]
        [Description("Increase the level of a magnet upgrade")]
        [RequireFreeRessources(RessourceType.User | RessourceType.Channel)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Save]
        public async Task UpgradeAsync([Remainder][Name("Upgrade")]MagnetUpgrade upgrade)
        {
            int level = Context.Player.GetUpgradeLevel(upgrade);
            int cost = upgrade.GetCost(level);

            if (Context.Player.Magnets < cost)
            {
                var embed = new EmbedBuilder()
                    .WithColor(EmbedColor.Failed)
                    .WithTitle("You can't afford that!")
                    .WithDescription($"You need at least {cost} <:magnet:440898600738750465>")
                    .Build();

                await ReplyAsync(embed: embed);
                return;
            }

            var page = new PageBuilder()
                .WithColor(EmbedColor.Question)
                .WithTitle($"Do you want to upgrade {upgrade.Name}")
                .WithDescription($"This will cost you {cost} <:magnet:440898600738750465>");

            var confirmation = new ConfirmationBuilder()
                .WithContent(page)
                .WithUsers(Context.User)
                .WithDeletion(DeletionOption.Invalids | DeletionOption.AfterCapturedContext)
                .Build();

            var result = await Interactivity.SendConfirmationAsync(confirmation, Context.Channel);

            if (result.Value)
            {
                level++;
                Context.Player.Magnets -= cost;
                Context.Player.SetUpgradeLevel(upgrade, level);

                if (upgrade.RequireRecalculation)
                {
                    await Db.RecalculatePlayerTrophiesAsync(Context.Player);
                }

                var embed = new EmbedBuilder()
                    .WithColor(EmbedColor.Success)
                    .WithTitle("Success!")
                    .WithDescription($"The {upgrade.Name} upgrade is now level {level + 1}") //Level is 0 based
                    .Build();

                await ReplyAsync(embed: embed);
            }
        }

        [Command("Levels", "GetLevels")]
        [Description("Get the upgrade levels of a specific player")]
        [Priority(1)]
        public Task GetUpgradeLevels([Remainder][Name("Player")]SquizzyPlayer player)
        {
            var pages = new List<PageBuilder>();

            foreach(var upgrade in Upgrade.GetAllMagnetUpgrades())
            {
                int level = player.GetUpgradeLevel(upgrade);

                pages.Add(new PageBuilder()
                    .WithColor(EmbedColor.Statistic)
                    .WithTitle($"{player}'s Magnet Upgrade Levels")
                    .AddField($"{upgrade.Name} # {upgrade.Id}", $"{upgrade.Description}")
                    .AddField("Level", level + 1, true)
                    .AddField("Current Value", upgrade.GetCurrentValue(level), true));
            }

            var paginator = new PaginatorBuilder()
                .WithPages(pages.ToArray())
                .WithUsers(Context.User)
                .Build();

            return Interactivity.SendPaginatorAsync(paginator, Context.Channel);
        }

        [Command("Levels", "GetLevels", "Mylevels")]
        [Description("Get your upgrade levels")]
        public Task GetPersonalUpgradeLevels() 
            => GetUpgradeLevels(Context.Player);
    }
}
