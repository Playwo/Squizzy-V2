using System;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class InfoModule : SquizzyModule
    {
        public DbBackEndService DbBackend { get; set; }

        [Command("Invite", "Inv")]
        public async Task SendInviteAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Invites")
                .WithColor(Color.Green)
                .AddField("Squizzy Invite Link", "https://discordapp.com/oauth2/authorize?client_id=529241818143916052&scope=bot&permissions=388160")
                .AddField("Officical Scrap Discord", "https://discord.gg/W3d9Jvx")
                .AddField("Official Scrap Wiki", "http://official-scrap-2.wikia.com/wiki/Official_Scrap_2_Wiki")
                .AddField("Suggestions / Bug-Reports", "https://discord.gg/YzEuqpk")
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("Advertisment", "Ad")]
        [RequireFinishedCooldown]
        public Task SendAdAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Thx for supporting me :)")
                .WithDescription("https://wishes2.com/k9bV \n\n**Advice:**\nThe ad page is not made by me! Watch out not to download any content from that page! ")
                .Build();

            Context.Player.Cooldown.SetCommandCooldown(Context.Command, TimeSpan.FromHours(12));
            return ReplyAsync(embed: embed);
        }

        [Command("Ping", "Connection", "Status")]
        public async Task SendPingAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithDescription($":hourglass: {Context.Client.Latency}ms\n\n:heartbeat: {DbBackend.Latency}ms")
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("Help", "HelpMe")]
        public async Task HelpAsync() => await ReplyAsync("Still To Do!");
    }
}
