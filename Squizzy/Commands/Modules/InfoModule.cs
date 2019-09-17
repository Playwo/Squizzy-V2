using System;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Information :newspaper:")]
    public class InfoModule : SquizzyModule
    {
        public DbBackEndService DbBackend { get; set; }

        [Command("Invite", "Inv")]
        [Description("Get the bots invite link")]
        public async Task SendInviteAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Invites")
                .WithColor(EmbedColor.Success)
                .AddField("Squizzy Invite Link", "https://discordapp.com/oauth2/authorize?client_id=529241818143916052&scope=bot&permissions=388160")
                .AddField("Officical Scrap Discord", "https://discord.gg/W3d9Jvx")
                .AddField("Official Scrap Wiki", "http://official-scrap-2.wikia.com/wiki/Official_Scrap_2_Wiki")
                .AddField("Suggestions / Bug-Reports", "https://discord.gg/YzEuqpk")
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("Advertisment", "Ad")]
        [Description("Watch an Ad to support my development")]
        [RequireFinishedCooldown]
        public Task SendAdAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Success)
                .WithTitle("Thx for supporting me :)")
                .WithDescription("https://wishes2.com/k9bV \n\n**Advice:**\nIhave not made the ad page!\nWatch out not to download any content from that page! ")
                .Build();

            Context.Player.Cooldown.SetCommandCooldown(Context.Command, TimeSpan.FromHours(12));
            return ReplyAsync(embed: embed);
        }

        [Command("Ping", "Connection", "Status")]
        [Description("Get my latency stats")]
        public async Task SendPingAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithDescription($":hourglass: {Context.Client.Latency}ms\n\n:heartbeat: {DbBackend.Latency}ms")
                .Build();

            await ReplyAsync(embed: embed);
        }
    }
}
