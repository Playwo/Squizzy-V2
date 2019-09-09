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
        public TimeSpan Time { get; private set; }

        public QuestionResult(Question question, bool correct, TimeSpan time)
        {
            QuestionId = question.Id;
            Category = question.Type;
            Time = time;
            Correct = correct;
        }

        public static QuestionResult FromIncorrect(Question question)
            => new QuestionResult(question, false, TimeSpan.Zero);

        public static QuestionResult FromCorrect(Question question, TimeSpan time)
            => new QuestionResult(question, true, time);

        public static bool ShouldReplace(QuestionResult oldResult, QuestionResult newResult)
            => oldResult != null
                ? newResult.Correct && oldResult.Correct
                    ? newResult.Time < oldResult.Time
                    : true
                : true;
    }
}
