using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Insights :eye: ")]
    public class InsightsModule : SquizzyModule
    {
        public DbService Db { get; set; }

        [Command("MaximumTrophies", "MaxTrophies", "MaxT", "MTrophies", "MT")]
        [Description("Get the maximum amount of trophies you can theoretically reach")]
        public async Task SendMaximumTrophiesAsync([Name("IncludeUpggrades")]bool includeUpgrades = true)
        {
            int questionCount = (int) await Db.CountTotalQuestionsAsync();

            var upgrade = new PerfectTrophiesUpgrade();
            int level = Context.Player.GetUpgradeLevel(upgrade);
            int bonusTrophies = upgrade.CalculateValue(level, 0) / questionCount;


            int maximumTrophies = includeUpgrades
                ? await Db.CountMaximumTrophiesAsync() + bonusTrophies
                : await Db.CountMaximumTrophiesAsync();

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithTitle("Maximum achievable trophy count")
                .WithDescription($"{maximumTrophies} :trophy:")
                .Build());
        }

        [Command("TotalPlayers", "TotalUsers", "PlayerCount", "UserCount", "Players", "Users")]
        [Description("Get the total amount of Squizzy-Players")]
        public async Task SendPlayerCountAsync()
        {
            long playerCount = await Db.CountPlayersAsync();

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithTitle("Total Player Count")
                .WithDescription($"{playerCount} :raising_hand:")
                .Build());
        }

        [Command("TotalQuestions", "QuestionCount", "Questions")]
        [Description("Get the total amount of questions in all categories")]
        public async Task SendTotalQuestionCountAsync()
        {
            long questionCount = await Db.CountTotalQuestionsAsync();

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithTitle($"Total Amount of Questions in all categories")
                .WithDescription($"{questionCount} x :question:")
                .Build());
        }

        [Command("TotalQuestions", "QuestionCount", "Questions")]
        [Description("Get the amount of questions in a specific category")]
        [Priority(1)]
        public async Task SendQuestionCountAsync([Name("Category")]Category category)
        {
            long questionCount = await Db.CountQuestionsAsync(category);

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithTitle($"Total Amount of {category} Questions")
                .WithDescription($"{questionCount} x :question:")
                .Build());
        }
    }
}
