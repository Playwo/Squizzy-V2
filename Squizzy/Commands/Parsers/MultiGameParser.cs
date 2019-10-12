using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Games;

namespace Squizzy.Commands
{
    public class MultiGameParser : SquizzyParser<MultiGame>
    {
        private readonly Dictionary<MultiGameType, string[]> LbTypeShortcuts = new Dictionary<MultiGameType, string[]>()
        {
            [MultiGameType.QuizRoyale] = new[] { "QuizRoyale", "Royale", "QR", "QuizR", "QRoyale" },
            [MultiGameType.Duel] = new[] { "Duel", "DuelGame", "1v1", "Due", "Du", "D" }
        };

        public override ValueTask<TypeParserResult<MultiGame>> ParseAsync(Parameter parameter, string value, SquizzyContext context)
        {
            foreach (var gametype in LbTypeShortcuts.Keys)
            {
                bool success = LbTypeShortcuts.TryGetValue(gametype, out string[] shortcuts);

                if (!success)
                {
                    continue;
                }

                if (!shortcuts.Where(x => x.ToLower() == value.ToLower()).Any())
                {
                    continue;
                }

                var game = gametype switch
                {
                    MultiGameType.QuizRoyale => (MultiGame) new QuizRoyaleGame(context.Channel),
                    MultiGameType.Duel => new DuelGame(context.Channel, Category.Random, 5),
                    _ => throw new InvalidOperationException("Game Type not associated with a game"),
                };

                return TypeParserResult<MultiGame>.Successful(game);
            }

            var errorMessage = new StringBuilder()
                .AppendLine("No multiplayer game type found matching your input!")
                .AppendLine("Valid types are:");

            foreach (var lbType in LbTypeShortcuts)
            {
                errorMessage.AppendLine($" => {lbType.Value[0]}");
            }

            return TypeParserResult<MultiGame>.Unsuccessful($"{errorMessage}");
        }
    }
}
