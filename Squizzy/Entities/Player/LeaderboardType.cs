using System;

namespace Squizzy.Entities
{
    public class LeaderboardType
    {
        public Func<SquizzyPlayer, string> Field { get; }
        public string Title { get; }
        public string ValueIdentifier { get; }
        public Leaderboard Type { get; }

        public LeaderboardType(Leaderboard type, Func<SquizzyPlayer, object> field, string title, string valueIdentifier)
        {
            Field = x=> field.Invoke(x).ToString();
            Title = title;
            ValueIdentifier = valueIdentifier;
            Type = type;
        }
    }

    public enum Leaderboard
    {
        Trophies,
        Magnets,
        Honor,
        AnsweredQuestions
    }
}
