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
        public async Task SendPlayerStatisticsAsync([Name("Player")]SquizzyPlayer player, [Name("Type")]StatisticsType type = StatisticsType.General)
        {
            switch (type)
            {
                case StatisticsType.General:
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithColor(EmbedColor.Statistic)
                        .WithTitle($"General Stats of {player}")
                        .AddField($"**{player.Trophies} :trophy:  &  {player.Magnets} <:magnet:440898600738750465>**",
                                  $"__**Singleplayer**__\n" +
                                  $"Total Answered: {player.TotalAnsweredQuestions}\n" +
                                  $"Total Correct: {player.TotalCorrectQuestions}\n" +
                                  $"Success Rate: {player.SingleplayerSuccessRate}%\n" +
                                  $"Average Answer Time {player.AverageAnswerTime} seconds")
                        .AddField($"{player.Honor} :gem:",
                                  $"Games Played: {player.MatchesPlayed}\n" +
                                  $"Games Won: {player.MatchesWon}" +
                                  $"Success Rate: {player.MultiplayerSuccessRate}%")
                        .Build());
                    break;

                case StatisticsType.Category:
                    long totalAmount = 0;
                    long totalCorrectAmount = 0;
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
                        int correctAmount = player.AnsweredQuestions.Count(x => x.Category == category && x.Correct);
                        int perfectAmount = player.AnsweredQuestions.Count(x => x.Category == category && x.Perfect);
                        rankedDescription.AppendLine($"{category} : {correctAmount} [{Math.Round((double)100*correctAmount / categoryAmount, 1)}%]");
                        perfectDescription.AppendLine($"{category} : {perfectAmount} [{Math.Round((double)100 * perfectAmount / categoryAmount, 1)}%]");
                        
                        totalAmount += categoryAmount;
                        totalCorrectAmount += correctAmount;
                        totalPerfectAmount += perfectAmount;
                    }
                    await ReplyAsync(embed: new EmbedBuilder()
                        .WithColor(EmbedColor.Statistic)
                        .WithTitle($"Category Stats of {player}")
                        .AddField($"Correct Questions {totalCorrectAmount} [{Math.Round((double)100 * totalCorrectAmount / totalAmount,1)}%]",
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
        public Task SendPlayerStatisticsAsync([Name("Type")]StatisticsType type = StatisticsType.General)
            => SendPlayerStatisticsAsync(Context.Player, type);
    }
}
