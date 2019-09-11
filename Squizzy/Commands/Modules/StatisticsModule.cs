using System;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class StatisticsModule : SquizzyModule
    {
        [Command("Statistics", "Stats", "Stat")]
        [Priority(1)]
        public Task SendPlayerStatisticsAsync(SquizzyPlayer player, StatisticsType type = StatisticsType.General)
            => type switch
            {
                StatisticsType.General => ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(EmbedColor.Statistic)
                    .WithTitle($"General Stats of {player}")
                    .AddField($"**{player.Trophies} :trophy:  &  {player.Magnets} <:magnet:440898600738750465>**",
                              $"__**Questions**__\n" +
                              $"Total Answered: {player.TotalAnsweredQuestions}\n" +
                              $"Total Correct: {player.TotalCorrectQuestions}\n" +
                              $"Success Rate: {player.SuccessRate}\n" +
                              $"Average Answer Time {player.AverageAnswerTime} seconds")
                    .AddField($"{player.Honor} :gem:",
                              $"Soon...")
                    .Build()),
                StatisticsType.Category => ReplyAsync(embed: new EmbedBuilder()
                    .WithColor(EmbedColor.Statistic)
                    .WithTitle($"Category Stats of {player}")
                    .Build()),
                _ => throw new InvalidOperationException("Invalid StatisticsType!"),
            };

        [Command("Statistics", "Stats", "Stat")]
        public Task SendPlayerStatisticsAsync(StatisticsType type = StatisticsType.General)
            => SendPlayerStatisticsAsync(Context.Player, type);
    }
}
