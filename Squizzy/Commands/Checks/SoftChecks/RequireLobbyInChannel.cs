using System.Threading.Tasks;
using Qmmands;
using Squizzy.Extensions;
using Squizzy.Services;

namespace Squizzy.Commands
{
    internal class RequireLobbyInChannel : SquizzyCheck
    {
        public override string Description => "Requires a Multiplayer Lobby to be open in the current channel";

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            var minigame = context.ServiceProvider.GetService<MinigameService>();
            var lobby = minigame.GetLobby(context.Channel);

            return lobby == null
                ? CheckResult.Unsuccessful("Requires a Multiplayer Lobby to be open in the current channel")
                : CheckResult.Successful;
        }
    }
}
