using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Entities;

namespace Squizzy.Commands
{
    public class StatisticTypeParser : SquizzyParser<StatisticsType>
    {
        private readonly Dictionary<StatisticsType, string[]> StatisticsTypeShortcuts = new Dictionary<StatisticsType, string[]>()
        {
            [StatisticsType.General] = new[] { "General", "Gen", "Ge", "G" },
            [StatisticsType.Category] = new[] { "Category", "Categ", "Cate", "Cat", "C", "Ca", "CG" },
        };

        public override ValueTask<TypeParserResult<StatisticsType>> ParseAsync(Parameter parameter, string value, SquizzyContext context)
        {
            bool success = Enum.TryParse(value, out StatisticsType result);

            if (success)
            {
                return TypeParserResult<StatisticsType>.Successful(result);
            }

            foreach (var statisticsType in StatisticsTypeShortcuts.Keys)
            {
                success = StatisticsTypeShortcuts.TryGetValue(statisticsType, out string[] shortcuts);

                if (!success)
                {
                    continue;
                }

                if (!shortcuts.Where(x => x.ToLower() == value.ToLower()).Any())
                {
                    continue;
                }

                return TypeParserResult<StatisticsType>.Successful(statisticsType);
            }

            var errorMessage = new StringBuilder()
                .AppendLine("No leaderboard type found matching your input!")
                .AppendLine("Valid types are:");

            foreach (var statisticsTypeShortcut in StatisticsTypeShortcuts)
            {
                errorMessage.AppendLine($" => {statisticsTypeShortcut.Value[0]}");
            }

            return TypeParserResult<StatisticsType>.Unsuccessful($"{errorMessage}");
        }
    }
}
