using System;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Extensions;

namespace Squizzy.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class RequireFinishedCooldown : SoftCheckAttribute
    {
        public override string Description => "Requires your cooldown to be over";

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            var remainingCooldown = context.Player.Cooldown.GetRemainingCommandCooldown(Command);

            return remainingCooldown != TimeSpan.Zero
                ? CheckResult.Unsuccessful($"The cooldown is still running...\nTry again in {remainingCooldown.ToTimeString()}")
                : (ValueTask<CheckResult>) CheckResult.Successful;
        }
    }
}
