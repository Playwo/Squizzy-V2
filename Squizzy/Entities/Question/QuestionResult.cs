using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Squizzy.Entities
{
    public class QuestionResult
    {
        [BsonElement]
        public int QuestionId { get; private set; }

        [BsonElement]
        public Category Category { get; private set; }

        //[BsonElement]
        public bool Correct { get; private set; }

        //[BsonElement]
        public bool Perfect { get; private set; }

        [BsonElement]
        public TimeSpan Time { get; private set; }

        public QuestionResult(Category category, int questionId, bool correct, TimeSpan time)
        {
            QuestionId = questionId;
            Category = category;
            Time = time;
            Correct = correct;
        }



        public bool HasToReplace(QuestionResult result)
            => Correct
                ? result.Correct
                    ? Time < result.Time //New Result Faster than Old Result
                    : true //New Result True, Old Result Wrong
                : result.Correct;  // New Result Wrong => Save if old one was right => Trophy loss

        public static QuestionResult FromIncorrect(Category category, int questionId)
            => new QuestionResult(category, questionId, false, TimeSpan.Zero);

        public static QuestionResult FromCorrect(Category category, int questionId, TimeSpan time)
            => new QuestionResult(category, questionId, true, time);

        public int CalculateTrophies(Question question, SquizzyPlayer player)
        {
            var pTimeUpgrade = new PerfectTimeUpgrade();
            int pTimeLevel = player.GetUpgradeLevel(pTimeUpgrade);
            double pTime = pTimeUpgrade.CalculateValue(pTimeLevel);

            Perfect = false;

            if (!Correct)
            {
                return 0;
            }
            if (Time.TotalSeconds < pTime)
            {
                Perfect = true;
                var pTrophiesUpgrade = new PerfectTrophiesUpgrade();
                int pTrohpiesLevel = player.GetUpgradeLevel(pTrophiesUpgrade);
                return pTrophiesUpgrade.CalculateValue(pTrohpiesLevel, question.Reward);
            }

            double factor = -Math.Log(0.3333) / (question.Time - pTime);

            int trophies = (int) Math.Floor(
                question.Reward * Math.Pow(Math.E, -factor * (Time.TotalSeconds - pTime)));

            return trophies;
        }
    }
}
