using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Extensions;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class PlayerParser : SquizzyParser<SquizzyPlayer>
    {
        public override async ValueTask<TypeParserResult<SquizzyPlayer>> ParseAsync(Parameter param, string value, SquizzyContext context)
        {
            var db = context.ServiceProvider.GetService<DbService>();

            var users = context.Guild.Users.OfType<SocketUser>().ToList();

            SocketUser user = null;

            if (ulong.TryParse(value, out ulong id) || MentionUtils.TryParseUser(value, out id))
            {
                user = users.FirstOrDefault(x => x.Id == id);
            }

            if (user is null)
            {
                user = users.FirstOrDefault(x => x.ToString().EqualsIgnoreCase(value));
            }

            if (user is null)
            {
                var match = users.Where(x =>
                    x.Username.EqualsIgnoreCase(value)
                    || (x as SocketGuildUser).Nickname.EqualsIgnoreCase(value)).ToList();
                if (match.Count > 1)
                {
                    return TypeParserResult<SquizzyPlayer>.Unsuccessful(
                        "Multiple players found, try mentioning the user or using their ID.");
                }

                user = match.FirstOrDefault();
            }

            return user is null
                ? TypeParserResult<SquizzyPlayer>.Unsuccessful("Player not found.")
                : TypeParserResult<SquizzyPlayer>.Successful(await db.LoadPlayerAsync(user));
        }
    }
}
