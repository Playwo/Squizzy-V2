using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Commands;
using Squizzy.Services;

namespace Squizzy.Extensions
{
    public static partial class Extensions
    {
        public static async Task<bool> RunHardChecksAsync(this Command command, SquizzyContext context)
        {
            var checks = command.Checks
                .Where(x => x is HardCheckAttribute)
                .Select(x => x as HardCheckAttribute).ToList();

            var checkResults = await Task.WhenAll(checks.Select(x => x.CheckAsync(context).AsTask()));

            return !checkResults.Any(x => !x.IsSuccessful);
        }

        public static async Task<bool> RunSoftChecksAsync(this Command command, SquizzyContext context)
        {
            var checks = command.Checks
                .Where(x => x is SoftCheckAttribute)
                .Select(x => x as SoftCheckAttribute).ToList();

            var checkResults = await Task.WhenAll(checks.Select(x => x.CheckAsync(context).AsTask()));

            return checkResults.Any(x => !x.IsSuccessful);
        }

        public static string GetHelp(this Command command)
        {
            var builder = new StringBuilder()
                .Append($"{command.Name}");

            foreach (var parameter in command.Parameters)
            {
                builder.Append($" [{parameter.Name}]");
            }

            return builder.ToString();
        }

        public static Embed GetHelpEmbed(this Command command)
        {
            var builder = new EmbedBuilder()
                .WithColor(EmbedColor.Help)
                .WithTitle($"{command.Name} Overview")
                .AddField("Usage", GetHelp(command))
                .AddField("Aliases", $"{string.Join(", ", command.Aliases)}")
                .AddField("Description", command.Description ?? "None");

            if (!string.IsNullOrWhiteSpace(command.Remarks))
            {
                builder.AddField("Remarks", command.Remarks);
            }


            if (command.Checks.Count > 0)
            {
                var checkBuilder = new StringBuilder();

                foreach (var check in command.Checks)
                {
                    checkBuilder.AppendLine($"- {(check as SquizzyCheck).Description}");
                }
                builder.AddField("Checks", checkBuilder);
            }

            return builder.Build();
        }
    }
}
