using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Entities;

namespace Squizzy.Commands
{
    public class LeaderboardTypeParser : SquizzyParser<LeaderboardType>
    {
        private readonly Dictionary<LeaderboardType, string[]> LbTypeShortcuts = new Dictionary<LeaderboardType, string[]>()
        {
            [new LeaderboardType(Leaderboard.Trophies ,x => x.Trophies, "Trophy", ":trophy:")] = new[] { "Trophies", ":trophy:", "Trophie", "Trophy", "Troph" ,"Tro", "Tr", "T" },
            [new LeaderboardType(Leaderboard.Honor, x => x.Honor, "Honor", ":gem:")] = new[] { "Honor", "Hono", "Hon", "Ho", "H" },
            [new LeaderboardType(Leaderboard.Magnets, x => x.Magnets, "Magnet", "<:magnet:440898600738750465>")] = new[] { "Magnets", "<:magnet:440898600738750465>",  "Magnet", "Magne", "Mags", "Mag", "Ma", "M" },
            [new LeaderboardType(Leaderboard.AnsweredQuestions, x => x.AnsweredQuestions.Count, "Answered Questions", "x :question:")] = new[] { "AnsweredQuestions", "Answereds", "Answered", "Questions", "Q" , "AQ", "AQS", "QS"}
        };

        public override ValueTask<TypeParserResult<LeaderboardType>> ParseAsync(Parameter parameter, string value, SquizzyContext context)
        {
            foreach (var category in LbTypeShortcuts.Keys)
            {
                bool success = LbTypeShortcuts.TryGetValue(category, out string[] shortcuts);

                if (!success)
                {
                    continue;
                }

                if (!shortcuts.Where(x => x.ToLower() == value.ToLower()).Any())
                {
                    continue;
                }

                return TypeParserResult<LeaderboardType>.Successful(category);
            }

            var errorMessage = new StringBuilder()
            .AppendLine("No leaderboard type found matching your input!")
            .AppendLine("Valid types are:");
            foreach (var lbType in LbTypeShortcuts)
            {
                errorMessage.AppendLine($" => {lbType.Value[0]}");
            }

            return TypeParserResult<LeaderboardType>.Unsuccessful($"{errorMessage}");
        }
    }
}
