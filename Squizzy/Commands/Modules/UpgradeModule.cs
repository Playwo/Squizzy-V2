using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class UpgradeModule : SquizzyModule
    {
        [Command("UnlockMultiplayer", "MultiplayerUnlock", "MUnlock", "UnlockM", "BuyMultiplayer", "MultiplayerAccess", "AccessMultiplayer")]
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
    }
}
