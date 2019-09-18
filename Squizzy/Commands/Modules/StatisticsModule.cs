using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Statistics :bookmark_tabs:")]
    public class StatisticsModule : SquizzyModule
    {
        public DbService Db { get; set; }

        [Command("Statistics", "Stats", "Stat")]
        [Description("Get quiz statistics about someone else")]
        [Priority(1)]
        public async Task SendPlayerStatisticsAsync(SquizzyPlayer player, StatisticsType type = StatisticsType.General)
        {
            switch (type)
            {
                case StatisticsType.General:
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithColor(EmbedColor.Statistic)
                        .WithTitle($"General Stats of {player}")
                        .AddField($"**{player.Trophies} :trophy:  &  {player.Magnets} <:magnet:440898600738750465>**",
                                  $"__**Questions**__\n" +
                                  $"Total Answered: {player.TotalAnsweredQuestions}\n" +
                                  $"Total Correct: {player.TotalCorrectQuestions}\n" +
                                  $"Success Rate: {player.SuccessRate}%\n" +
                                  $"Average Answer Time {player.AverageAnswerTime} seconds")
                        .AddField($"{player.Honor} :gem:",
                                  $"Soon...")
                        .Build());
                    break;

                case StatisticsType.Category:
                    long totalAmount = 0;
                    long totalRankedAmount = 0;
                    long totalPerfectAmount = 0;
                    var rankedDescription = new StringBuilder();
                    var perfectDescription = new StringBuilder();
                    foreach (Category category in Enum.GetValues(typeof(Category)))
                    {
                        if (category == Category.Random)
                        {
                            continue;
                        }

                        long categoryAmount = await Db.CountQuestionsAsync(category);
                        int rankedAmount = player.AnsweredQuestions.Count(x => x.Category == category);
                        int perfectAmount = player.AnsweredQuestions.Count(x => x.Category == category && x.Perfect);
                        rankedDescription.AppendLine($"{category} : {rankedAmount} [{Math.Round((double)100*rankedAmount / categoryAmount, 1)}%]");
                        perfectDescription.AppendLine($"{category} : {perfectAmount} [{Math.Round((double)100 * perfectAmount / categoryAmount, 1)}%]");
                        
                        totalAmount += categoryAmount;
                        totalRankedAmount += rankedAmount;
                        totalPerfectAmount += perfectAmount;
                    }
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithColor(EmbedColor.Statistic)
                        .WithTitle($"Category Stats of {player}")
                        .AddField($"Ranked Questions {totalRankedAmount} [{Math.Round((double)100 * totalRankedAmount / totalAmount,1)}%]",
                                  $"{rankedDescription}")
                        .AddField($"Perfect Questions {totalPerfectAmount} [{Math.Round((double)100 * totalPerfectAmount / totalAmount, 1)}%]",
                                  $"{perfectDescription}")
                        .Build());
                    break;

                default:
                    throw new InvalidOperationException("Invalid StatisticsType!");
            }
        }

        [Command("Statistics", "Stats", "Stat")]
        [Description("Get your quiz statistics")]
        public Task SendPlayerStatisticsAsync(StatisticsType type = StatisticsType.General)
            => SendPlayerStatisticsAsync(Context.Player, type);
    }
}
