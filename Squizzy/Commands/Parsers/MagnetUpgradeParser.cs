using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Extensions;
using Squizzy.Entities;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class MagnetUpgradeParser : SquizzyParser<MagnetUpgrade>
    {
        public override ValueTask<TypeParserResult<MagnetUpgrade>> ParseAsync(Parameter parameter, string value, SquizzyContext context)
        {
            var _magnet = context.ServiceProvider.GetService<UpgradeService>();

            var upgrades = _magnet.GetAllMagnetUpgrades();

            foreach (var upgrade in upgrades)
            {
                if (!upgrade.NameShortcuts.Where(x => x.ToLower() == value.ToLower()).Any())
                {
                    continue;
                }

                return TypeParserResult<MagnetUpgrade>.Successful(upgrade);
            }

            var errorMessage = new StringBuilder()
                .AppendLine("No magnet upgrade found matching your input!")
                .AppendLine("Valid types are:");

            foreach (var upgrade in upgrades)
            {
                errorMessage.AppendLine($" => {upgrade.Name}");
            }

            return TypeParserResult<MagnetUpgrade>.Unsuccessful($"{errorMessage}");
        }
    }
}
