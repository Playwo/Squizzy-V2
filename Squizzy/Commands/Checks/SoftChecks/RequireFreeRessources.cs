using Squizzy.Extensions;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Services;
using Discord.WebSocket;

namespace Squizzy.Commands
{
    public class RequireFreeRessources : SoftCheckAttribute
    {
        public RessourceType Type { get; }

        public override string Description => $"The {Type} needs to be free from use";

        public RequireFreeRessources(RessourceType type)
        {
            Type = type;
        }

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            var ressourceAdministration = context.ServiceProvider.GetService<RessourceAdministrationService>();

            if (Type.HasFlag(RessourceType.Guild))
            {
                if (!ressourceAdministration.IsGuildOccupied(context.Guild))
                {
                    ressourceAdministration.OccupieGuild(context.Guild);
                }
                else
                {
                    return CheckResult.Unsuccessful("This guild is currently occupied!");
                }
            }

            if (Type.HasFlag(RessourceType.Channel)) 
            {
                if (!ressourceAdministration.IsChannelOccupied(context.Channel as SocketChannel))
                {
                    ressourceAdministration.OccupieChannel(context.Channel as SocketChannel);
                }
                else
                {
                    return CheckResult.Unsuccessful("Somebody else is using this channel at the moment!");
                }
            }

            if (Type.HasFlag(RessourceType.User))
            {
                if (!ressourceAdministration.IsUserOccupied(context.User))
                {
                    ressourceAdministration.OccupieUser(context.User);
                }
                else
                {
                    return CheckResult.Unsuccessful("You can only run one command at once!");
                }
                
            }

            return CheckResult.Successful;
        }
    }
}
