using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Qmmands;

namespace Squizzy.Commands
{
    public abstract class SquizzyModule : ModuleBase<SquizzyContext>
    {
        public async Task<RestUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => await Context.Channel.SendMessageAsync(text, isTTS, embed, options);
    }
}
