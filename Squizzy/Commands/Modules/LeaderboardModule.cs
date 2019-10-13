using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using InteractivityAddon;
using InteractivityAddon.Pagination;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Extensions;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Leaderboards :first_place:")]
    public class LeaderboardModule : SquizzyModule
    {
        public DbService Db { get; set; }
        public InteractivityService Interactivity { get; set; }

        [Command("Leaderboard", "Lb", "Leader", "Leaders", "Top")]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [Description("Get the player leaderboard for different statistic values")]
        public async Task GetLeaderboardAsync([Name("Type")]LeaderboardType type, [Name("Pages"), Range(1, 10)]int pagecount = 5)
        {
            var leaders = Db.LoadLeaderboard(type.Type, pagecount * 10);

            int maximum = leaders.Count < pagecount * 10
                ? leaders.Count - 1
                : (pagecount * 10) - 1;

            var pages = new List<PageBuilder>();

            for (int i = 0; i <= maximum; i += 10)
            {
                int amount = i + 10 >= maximum
                    ? maximum - i + 1
                    : 10;

                var players = leaders.GetRange(i, amount);
                var page = await GetLeaderboardPageAsync(players, type);
                pages.Add(page);
            }

            var paginator = new PaginatorBuilder()
                .WithUsers(Context.User)
                .WithPages(pages.ToArray())
                .WithPaginatorFooter(PaginatorFooter.PageNumber | PaginatorFooter.Users)
                .Build();

            await Interactivity.SendPaginatorAsync(paginator, Context.Channel, timeout: TimeSpan.FromMinutes(3));
        }

        private async Task<PageBuilder> GetLeaderboardPageAsync(List<SquizzyPlayer> players, LeaderboardType type)
        {
            var sBuilder = new StringBuilder();

            for (int i = 0; i < players.Count; i++)
            {
                var user = await Context.Client.GetOrDownloadUserAsync(players[i].Id);
                sBuilder.Append($"#{i + 1} - {user} - [{type.Field.Invoke(players[i])} {type.ValueIdentifier}]\n");
            }

            return new PageBuilder()
                .WithColor(EmbedColor.Leaderboard)
                .WithTitle($"{type.Title} Leaderboard")
                .WithDescription(sBuilder.ToString());
        }
    }
}
