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

        //[BsonRequired]
        public bool Correct { get; private set; }

        //[BsonRequired]
        public bool Perfect { get; private set; }

        [BsonRequired]
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

        public int CalculateTrophies(Question question)
        {
            double perfectTime = 2.0d;
            Perfect = false;

            if (!Correct)
            {
                return 0;
            }
            if (Time.TotalSeconds < perfectTime)
            {
                Perfect = true;
                return question.Reward;
            }

            double factor = -Math.Log(0.3333) / (question.Time - perfectTime);

            int trophies = (int) Math.Floor(
                question.Reward * Math.Pow(Math.E, -factor * (Time.TotalSeconds - perfectTime)));

            return trophies;
        }
    }
}
