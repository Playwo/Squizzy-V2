using System;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace Squizzy.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireBotPermissionAttribute : SoftCheckAttribute
    {
        public GuildPermission? GuildPermission { get; }
        public ChannelPermission? ChannelPermission { get; }

        public RequireBotPermissionAttribute(GuildPermission permission)
        {
            GuildPermission = permission;
            ChannelPermission = null;
        }

        public RequireBotPermissionAttribute(ChannelPermission permission)
        {
            ChannelPermission = permission;
            GuildPermission = null;
        }

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            IGuildUser guildUser = null;
            if (context.Guild != null)
            {
                guildUser = context.Guild.CurrentUser;
            }

            if (GuildPermission.HasValue)
            {
                if (guildUser == null)
                {
                    return CheckResult.Unsuccessful("Command must be used in a guild channel.");
                }

                if (!guildUser.GuildPermissions.Has(GuildPermission.Value))
                {
                    return CheckResult.Unsuccessful($"Bot requires guild permission {GuildPermission.Value}.");
                }
            }

            if (ChannelPermission.HasValue)
            {
                var perms = context.Channel is IGuildChannel guildChannel ? guildUser.GetPermissions(guildChannel) : ChannelPermissions.All(context.Channel);

                if (!perms.Has(ChannelPermission.Value))
                {
                    return CheckResult.Unsuccessful($"Bot requires channel permission {ChannelPermission.Value}.");
                }
            }

            return CheckResult.Successful;
        }
    }
}
