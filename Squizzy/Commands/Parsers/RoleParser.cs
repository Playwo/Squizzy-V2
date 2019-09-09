using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Extensions;

namespace Squizzy.Commands
{
    public sealed class RoleParser : SquizzyParser<IRole>
    {
        public override ValueTask<TypeParserResult<IRole>> ParseAsync(Parameter param, string value, SquizzyContext context)
        {
            var roles = context.Guild.Roles.ToList();
            IRole role = null;

            if (ulong.TryParse(value, out ulong id) || MentionUtils.TryParseRole(value, out id))
            {
                role = context.Guild.GetRole(id) as IRole;
            }

            if (role is null)
            {
                var match = roles.Where(x =>
                    x.Name.EqualsIgnoreCase(value));
                if (match.Count() > 1)
                {
                    return TypeParserResult<IRole>.Unsuccessful(
                        "Multiple roles found, try mentioning the role or using its ID.");
                }

                role = match.FirstOrDefault();
            }
            return role is null
                ? TypeParserResult<IRole>.Unsuccessful("Role not found.")
                : TypeParserResult<IRole>.Successful(role);
        }
    }
}
