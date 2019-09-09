using System;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Entities;

namespace Squizzy.Commands
{
    public class StatisticsModule : SquizzyModule
    {
        [Command("Statistics", "Stats")]
        public Task SendPlayerStatisticsAsync(StatisticsType type)
        {
            switch (type)
            {
                case StatisticsType.General:
                    var generalEmbed = new EmbedBuilder()
                        .Build();
                    return ReplyAsync(embed: generalEmbed);
                case StatisticsType.Category:
                    var categoryEmbed = new EmbedBuilder()
                        .Build();
                    return ReplyAsync(embed: categoryEmbed);
                default:
                    throw new InvalidOperationException("Invalid StatisticsType!");
            }
        }
    }
}
