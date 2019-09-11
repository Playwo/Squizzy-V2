using System.Threading.Tasks;
using Qmmands;
using Squizzy.Extensions;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class RequireMaintenance : HardCheckAttribute
    {
        public bool NeedsMaintenance { get; }

        public RequireMaintenance(bool needsMaintenance)
        {
            NeedsMaintenance = needsMaintenance;
        }

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            var ressourceAdministration = context.ServiceProvider.GetService<RessourceAdministrationService>();

            return ressourceAdministration.IsMaintenanceEnabled() == NeedsMaintenance
                ? CheckResult.Successful
                : CheckResult.Unsuccessful(NeedsMaintenance 
                    ? "This command can only be run while the bot is in maintenance mode!"
                    : "This command cannot be run while the bot is in maintenance mode!");
        }
    }
}
