using System.Threading.Tasks;
using Qmmands;

namespace Squizzy.Commands
{
    public class RequireOwner : HardCheckAttribute
    {
        public override async ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            var appInfo = await context.Client.GetApplicationInfoAsync();

            return appInfo.Owner.Id != context.User.Id
                ? CheckResult.Unsuccessful("This command is restricted to the Owner!")
                : CheckResult.Successful;
        }
    }
}
