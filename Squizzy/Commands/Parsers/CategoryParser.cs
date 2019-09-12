using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Extensions;
using Squizzy.Entities;
using Squizzy.Services;
using System.Text;

namespace Squizzy.Commands
{
    public class CategoryParser : SquizzyParser<Category>
    {
        private readonly Dictionary<Category, string[]> CategoryShortcuts = new Dictionary<Category, string[]>()
        {
            [Category.General] = new[] { "General", "Gen", "Ge", "G" },
            [Category.ScrapClicker1] = new[] { "ScrapClicker1", "Scrap1", "Sc1", "Sc", "S1", "C1" },
            [Category.ScrapClicker2] = new[] { "ScrapClicker2", "Scrap2", "Sc2", "S2", "C2" },
            [Category.ScrapTD] = new[] { "ScrapTowerDefense", "ScrapTD", "STD", "STowerDefense", "TD" },
            [Category.Random] = new[] { "Random", "Rand", "Ran", "Ra", "R" }
        };

        public override ValueTask<TypeParserResult<Category>> ParseAsync(Parameter parameter, string value, SquizzyContext context)
        {
            bool success = Enum.TryParse(value, out Category result);

            if (success)
            {
                return TypeParserResult<Category>.Successful(result);
            }

            foreach (Category category in Enum.GetValues(typeof(Category)))
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

                if (category != Category.Random)
                {
                    return TypeParserResult<Category>.Successful(category);
                }

                var _random = context.ServiceProvider.GetService<RandomizerService>();

                return TypeParserResult<Category>.Successful(_random.GetRandomCategory());
            }

            var errorMessage = new StringBuilder()
                .AppendLine("No category found matching your input!")
                .AppendLine("Valid categories are:");
            foreach (Category category in Enum.GetValues(typeof(Category)))
            {
                errorMessage.AppendLine($" => {category}");
            }

            return TypeParserResult<Category>.Unsuccessful("No category found matching your input!");
        }
    }
}
