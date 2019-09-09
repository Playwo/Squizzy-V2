using System.Threading.Tasks;
using Qmmands;

namespace Squizzy.Commands
{
    public abstract class SquizzyCheck : CheckAttribute
    {
        public abstract ValueTask<CheckResult> CheckAsync(SquizzyContext context);

        public override ValueTask<CheckResult> CheckAsync(CommandContext context)
            => CheckAsync(context as SquizzyContext);
    }
}
