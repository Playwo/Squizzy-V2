using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Insights :eye: ")]
    [Group("GetValue", "Get", "Value")]
    public class InsightsModule : SquizzyModule
    {
        public DbService Db { get; set; }

        [Command("TotalPlayers", "TotalUsers", "PlayerCount", "UserCount", "Players", "Users")]
        [Description("Get the total amount of Squizzy-Players")]
        public async Task SendPlayerCountAsync()
        {
            long playerCount = await Db.CountPlayersAsync();

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithTitle("Total Player Count")
                .WithDescription($"{playerCount}")
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
                .WithDescription($"{questionCount}")
                .Build());
        }

        [Command("TotalQuestions", "QuestionCount", "Questions")]
        [Description("Get the amount of questions in a specific category")]
        [Priority(1)]
        public async Task SendQuestionCountAsync(Category category)
        {
            long questionCount = await Db.CountQuestionsAsnyc(category);

            await ReplyAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Statistic)
                .WithTitle($"Total Amount of {category} Questions")
                .WithDescription($"{questionCount}")
                .Build());
        }
    }
}
