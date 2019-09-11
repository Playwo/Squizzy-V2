using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Entities;

namespace Squizzy.Commands
{
    public class StatisticTypeParser : SquizzyParser<StatisticsType>
    {
        private readonly Dictionary<StatisticsType, string[]> CategoryShortcuts = new Dictionary<StatisticsType, string[]>()
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

            foreach (StatisticsType category in Enum.GetValues(typeof(StatisticsType)))
            {
                success = CategoryShortcuts.TryGetValue(category, out string[] shortcuts);

                if (!success)
                {
                    continue;
                }

                if (!shortcuts.Where(x => x.ToLower() == value.ToLower()).Any())
                {
                    continue;
                }

                return TypeParserResult<StatisticsType>.Successful(category);
            }

            return TypeParserResult<StatisticsType>.Unsuccessful("No category found matching your input!");
        }
    }
}
