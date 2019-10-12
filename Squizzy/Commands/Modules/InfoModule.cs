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

        [Command("Invite", "Inv", "InviteLink", "Links", "Link", "Info", "Information")]
        [Description("Get the bots invite link")]
        public async Task SendInviteAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Invites")
                .WithColor(EmbedColor.Success)
                .AddField("Squizzy Invite Link", "https://discordapp.com/oauth2/authorize?client_id=529241818143916052&scope=bot&permissions=388160")
                .AddField("Officical Scrap Discord", "https://discord.gg/W3d9Jvx")
                .AddField("Official Scrap Wiki", "https://official-scrap-2.fandom.com/wiki/")
                .AddField("Suggestions / Bug-Reports", "https://discord.gg/YzEuqpk")
                .AddField("Planned / Upcoming updates", "https://trello.com/b/HOuKowBF/squizzy")
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("Support", "Donate")]
        [Description("Support my development")]
        public Task SendSupportPossibilitesAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Success)
                .WithTitle("Consider Supporting me?")
                .WithDescription("You can support me for free by watching an ad:\n" +
                                 "https://wishes2.com/k9bV [Only 1 per day or I dont get anything, Please dont download anything on the ad page!]\n\n" +
                                 "If you want to make me more productive help me out by buying me a coffee :)\n" +
                                 "https://www.buymeacoffee.com/q7Iu6yQ8S \n\n" +
                                 "This bot will always be free but I'm really grateful for everyone who supports me :gem:")
                .Build();

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
