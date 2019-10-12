using System.Threading.Tasks;
using Qmmands;

namespace Squizzy.Commands
{
    public class RequireMultiplayer : HardCheckAttribute
    {
        public bool NeedsMultiplayer { get; }

        public override string Description => throw new System.NotImplementedException();

        public RequireMultiplayer(bool needsMultiplayer)
        {
            NeedsMultiplayer = needsMultiplayer;
        }

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context) 
            => context.Player.HasMultiplayer == NeedsMultiplayer
                ? CheckResult.Successful
                : NeedsMultiplayer
                    ? CheckResult.Unsuccessful("You need to have access to multiplayer to use that command!")
                    : CheckResult.Unsuccessful("Your multiplayer access is already unlocked!");
    }
}
