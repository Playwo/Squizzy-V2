using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace Squizzy.Commands
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireBotPermissionAttribute : SoftCheckAttribute
    {
        public GuildPermission? GuildPerms { get; }
        public ChannelPermission? ChannelPerms { get; }

        public override string Description
        {
            get {

                var builder = new StringBuilder();

                if (GuildPerms.HasValue)
                {
                    builder.Append(GuildPerms.Value);
                    if (ChannelPerms.HasValue)
                    {
                        builder.Append(", ");
                    }
                }

                if (ChannelPerms.HasValue)
                {
                    builder.Append(ChannelPerms.Value);
                }

                return $"I need the {builder} permission(s)";
            }
        }

        public RequireBotPermissionAttribute(GuildPermission permission)
        {
            GuildPerms = permission;
            ChannelPerms = null;
        }

        public RequireBotPermissionAttribute(ChannelPermission permission)
        {
            ChannelPerms = permission;
            GuildPerms = null;
        }

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            IGuildUser guildUser = null;
            if (context.Guild != null)
            {
                guildUser = context.Guild.CurrentUser;
            }

            if (GuildPerms.HasValue)
            {
                if (guildUser == null)
                {
                    return CheckResult.Unsuccessful("Command must be used in a guild channel.");
                }

                if (!guildUser.GuildPermissions.Has(GuildPerms.Value))
                {
                    return CheckResult.Unsuccessful($"Bot requires guild permission {GuildPerms.Value}.");
                }
            }

            if (ChannelPerms.HasValue)
            {
                var perms = context.Channel is IGuildChannel guildChannel ? guildUser.GetPermissions(guildChannel) : ChannelPermissions.All(context.Channel);

                if (!perms.Has(ChannelPerms.Value))
                {
                    return CheckResult.Unsuccessful($"Bot requires channel permission {ChannelPerms.Value}.");
                }
            }

            return CheckResult.Successful;
        }
    }
}
