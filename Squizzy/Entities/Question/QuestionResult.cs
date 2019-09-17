using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Squizzy.Entities
{
    public class QuestionResult
    {
        [BsonRequired]
        public int QuestionId { get; private set; }

        [BsonRequired]
        public Category Category { get; private set; }

        [BsonRequired]
        public bool Correct { get; private set; }

        [BsonRequired]
        public bool Perfect { get; private set; }

        [BsonRequired]
        public TimeSpan Time { get; private set; }

        public QuestionResult(Question question, bool correct, TimeSpan time)
        {
            QuestionId = question.Id;
            Category = question.Type;
            Time = time;
            Correct = correct;
        }

        public bool IsBetterThan(QuestionResult result)
            => result.Correct
                ? Correct
                    ? result.Time < Time //New Result Faster than Old Result
                    : true //New Result True, Old Result Wrong
                : false; // New Result Wrong

        public static QuestionResult FromIncorrect(Question question)
            => new QuestionResult(question, false, TimeSpan.Zero);

        public static QuestionResult FromCorrect(Question question, TimeSpan time)
            => new QuestionResult(question, true, time);                

        public int CalculateTrophies(Question question)
        {
            if (!Correct)
            {
                Perfect = false;
                return 0;
            }

            if (Time.TotalSeconds < 1.5)
            {
                Perfect = true;
                return question.Reward;
            }

            double timeRange = question.Time - Time.TotalSeconds;
            Perfect = false;

            return (int) Math.Round(question.Reward / timeRange * Time.TotalSeconds);
        }
    }
}
