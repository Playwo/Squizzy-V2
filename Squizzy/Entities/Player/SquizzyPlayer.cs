using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;

namespace Squizzy.Entities
{
    public class SquizzyPlayer
    {
        [BsonId]
        public ulong Id { get; private set; }

        [BsonRequired]
        public int Trophies { get; set; } = 0;

        [BsonRequired]
        public int Magnets { get; set; } = 0;

        [BsonRequired]
        public int Honor { get; set; } = 0;

        [BsonRequired]
        public List<QuestionResult> AnsweredQuestions { get; set; } = new List<QuestionResult>();

        [BsonRequired]
        public int TotalAnsweredQuestions { get; set; } = 0;

        [BsonRequired]
        public int TotalCorrectQuestions { get; set; } = 0;

        [BsonRequired]
        public bool HasMultiplayer { get; set; } = false;

        [BsonRequired]
        public CooldownManager Cooldown { get; set; } = new CooldownManager();

        [BsonIgnore]
        public int TotalWrongQuestions => TotalAnsweredQuestions - TotalCorrectQuestions;

        [BsonIgnore]
        public int TotalRankedQuestions => AnsweredQuestions.Count;

        [BsonIgnore]
        public double SuccessRate => Math.Round(100d * (TotalCorrectQuestions / (double) TotalAnsweredQuestions), 2);

        public SquizzyPlayer(ulong id)
        {
            Id = id;
        }

        public bool HasAnsweredQuestion(Question question)
            => AnsweredQuestions.Any(x => x.QuestionId == question.Id);

        public void ProcessAnsweredQuestion(QuestionResult newResult, out QuestionResult oldResult)
        {
            TotalAnsweredQuestions++;
            if (newResult.Correct)
            {
                TotalCorrectQuestions++;
            }

            oldResult = AnsweredQuestions.Find(x => x.QuestionId == newResult.QuestionId);

            if (QuestionResult.ShouldReplace(oldResult, newResult))
            {
                ReplaceResult(oldResult, newResult);
            }
        }

        private void ReplaceResult(QuestionResult oldResult, QuestionResult newResult)
        {
            AnsweredQuestions.Remove(oldResult);
            AnsweredQuestions.Add(newResult);
        }
    }
}
