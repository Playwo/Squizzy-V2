using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Squizzy.Extensions
{
    public static partial class Extensions
    {
        public static async Task<IUser> GetOrDownloadUserAsync(this DiscordSocketClient client, ulong id)
            => client.GetUser(id) as IUser ?? await client.Rest.GetUserAsync(id);
    }
}
