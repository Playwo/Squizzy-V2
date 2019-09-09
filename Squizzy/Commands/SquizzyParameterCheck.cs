using System.Threading.Tasks;
using Qmmands;

namespace Squizzy.Commands
{
    public abstract class SquizzyParameterCheck : ParameterCheckAttribute
    {
        public abstract ValueTask<CheckResult> CheckAsync(object argument, SquizzyContext context);

        public override ValueTask<CheckResult> CheckAsync(object argument, CommandContext context)
            => CheckAsync(argument, context as SquizzyContext);
    }
}
