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
            var blocking = context.ServiceProvider.GetService<BlockingService>();


            if (!Type.HasFlag(RessourceType.User) || !blocking.IsUserBlocked(context.User.Id))
            {
                blocking.BlockUser(context.CommandId, context.User.Id);
            }
            else
            {
                return CheckResult.Unsuccessful("You can only run one command at a time!");
            }

            if (!Type.HasFlag(RessourceType.Channel) || !blocking.IsChannelBlocked(context.Channel.Id))
            {
                blocking.BlockChannel(context.CommandId, context.Channel.Id);
            }
            else
            {
                return CheckResult.Unsuccessful("Somebody else is using this channel at the moment!");
            }

            if (!Type.HasFlag(RessourceType.Guild) || !blocking.IsGuildBlocked(context.Guild.Id))
            {
                blocking.BlockGuild(context.CommandId, context.Guild.Id);
            }
            else
            {
                return CheckResult.Unsuccessful("This guild is currently occupied!");
            }

            if (!Type.HasFlag(RessourceType.Global) || !blocking.IsGlobalBlocked())
            {
                blocking.BlockGlobal(context.CommandId);
            }
            else
            {
                return CheckResult.Unsuccessful("This command can only run once at a time");
            }

            return CheckResult.Successful;
        }
    }
}
