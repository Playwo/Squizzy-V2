using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Qmmands;
using Squizzy.Extensions;

namespace Squizzy.Commands
{
    public class RequireHelper : HardCheckAttribute
    {
        public override string Description => $"This command can only be executed by helpers";

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
            => IsHelper(context.User, context.ServiceProvider)
                ? CheckResult.Successful
                : CheckResult.Unsuccessful("This command is restricted to helpers!");

        public static bool IsHelper(SocketUser user, IServiceProvider provider)
        {
            var config = provider.GetService<IConfigurationRoot>();
            ulong[] helpers = JsonConvert.DeserializeObject<ulong[]>(config["helpers"]);
            return helpers.Where(x => x == user.Id).Any();
        }
    }
}
