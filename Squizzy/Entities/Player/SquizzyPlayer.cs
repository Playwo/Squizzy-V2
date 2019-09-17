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
        public string Name { get; set; }

        [BsonIgnore]
        public int TotalWrongQuestions => TotalAnsweredQuestions - TotalCorrectQuestions;

        [BsonIgnore]
        public int TotalRankedQuestions => AnsweredQuestions.Count;

        [BsonIgnore]
        public double AverageAnswerTime => Math.Round(AnsweredQuestions.Sum(x => x.Time.TotalSeconds) / AnsweredQuestions.Count, 3);

        [BsonIgnore]
        public double SuccessRate => TotalAnsweredQuestions > 0
            ? Math.Round(100d * (TotalCorrectQuestions / (double) TotalAnsweredQuestions), 2)
            : 0;

        public SquizzyPlayer(ulong id)
        {
            Id = id;
        }

        public override string ToString() => Name;

        public bool HasAnsweredQuestion(Question question)
            => AnsweredQuestions.Any(x => x.QuestionId == question.Id);

        public void ProcessAnsweredQuestion(Question question, QuestionResult newResult, out QuestionResult oldResult,
                                            out int newTrophies, out int oldTrophies, out int magnets)
        {
            TotalAnsweredQuestions++;
            if (newResult.Correct)
            {
                TotalCorrectQuestions++;
            }

            oldResult = AnsweredQuestions.Find(x => x.QuestionId == newResult.QuestionId) ?? QuestionResult.FromIncorrect(question);

            oldTrophies = oldResult.CalculateTrophies(question);
            newTrophies = newResult.CalculateTrophies(question);

            magnets = (int)Math.Ceiling(newTrophies / 5.0);
            if (magnets == 0)
            {
                magnets = -3;
            }
            if (Magnets - magnets < 0)
            {
                magnets = -Magnets;
            }

            Magnets += magnets;

            if (newResult.IsBetterThan(oldResult))
            {
                ReplaceResult(oldResult, newResult);
                Trophies += newTrophies - oldTrophies;
            }
        }

        private void ReplaceResult(QuestionResult oldResult, QuestionResult newResult)
        {
            AnsweredQuestions.Remove(oldResult);
            AnsweredQuestions.Add(newResult);
        }
    }
}
