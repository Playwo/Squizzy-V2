using Squizzy.Extensions;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Services;
using Discord.WebSocket;

namespace Squizzy.Commands
{
    public class RequireFreeRessources : SoftCheckAttribute
    {
        public bool Guild { get; }
        public bool Channel { get; }
        public bool User { get; }

        public RequireFreeRessources(bool guild = false, bool channel = false, bool user = false)
        {
            Guild = guild;
            Channel = channel;
            User = user;
        }

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            var ressourceAdministration = context.ServiceProvider.GetService<RessourceAdministrationService>();

            if (Guild)
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

            if (Channel) 
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

            if (User)
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
