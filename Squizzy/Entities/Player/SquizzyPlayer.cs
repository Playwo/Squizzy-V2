﻿using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Options;

namespace Squizzy.Entities
{
    public class SquizzyPlayer
    {
        [BsonId]
        public ulong Id { get; private set; }

        [BsonElement]
        [BsonDefaultValue(0)]
        public int Trophies { get; set; } = 0;

        [BsonElement]
        [BsonDefaultValue(0)]
        public int Magnets { get; set; } = 0;

        [BsonElement]
        [BsonDefaultValue(0)]
        public int Honor { get; set; } = 0;

        [BsonElement]  //Default value provided through DbBackEndService#RegisterClassMaps
        public List<QuestionResult> AnsweredQuestions { get; set; } = new List<QuestionResult>();

        [BsonElement]
        [BsonDefaultValue(0)]
        public int TotalAnsweredQuestions { get; set; } = 0;

        [BsonElement]
        [BsonDefaultValue(0)]
        public int TotalCorrectQuestions { get; set; } = 0;

        [BsonElement]
        [BsonDefaultValue(false)]
        public bool HasMultiplayer { get; set; } = false;

        [BsonDictionaryOptions(DictionaryRepresentation.ArrayOfArrays)]
        [BsonElement] //Default value provided through DbBackEndService#RegisterClassMaps
        public Dictionary<int, int> Upgrades { get; private set; } = new Dictionary<int, int>();

        [BsonElement] //Default value provided through DbBackEndService#RegisterClassMaps
        public CooldownManager Cooldown { get; private set; } = new CooldownManager();

        [BsonElement]
        [BsonDefaultValue(0)]
        public int MatchesWon { get; set; } = 0;

        [BsonElement]
        [BsonDefaultValue(0)]
        public int MatchesPlayed { get; set; }


        [BsonIgnore]
        public string Name { get; set; }

        [BsonIgnore]
        public int TotalWrongQuestions => TotalAnsweredQuestions - TotalCorrectQuestions;

        [BsonIgnore]
        public int TotalRankedQuestions => AnsweredQuestions.Count;

        [BsonIgnore]
        public double AverageAnswerTime => AnsweredQuestions.Count > 0
            ? Math.Round(AnsweredQuestions.Sum(x => x.Time.TotalSeconds) / AnsweredQuestions.Count, 3)
            : 0;

        [BsonIgnore]
        public double SingleplayerSuccessRate => TotalAnsweredQuestions > 0
            ? Math.Round(100d * (TotalCorrectQuestions / (double) TotalAnsweredQuestions), 2)
            : 0;

        [BsonIgnore]
        public double MultiplayerSuccessRate => MatchesPlayed > 0
            ? Math.Round(100d * (MatchesWon / (double) MatchesPlayed), 2)
            : 0;

        public SquizzyPlayer(ulong id)
        {
            Id = id;
        }

        public override string ToString() => Name;

        public bool HasAnsweredQuestion(Question question)
            => AnsweredQuestions.Any(x => x.QuestionId == question.Id);

        public int GetUpgradeLevel<TUpgrade>() where TUpgrade : IUpgrade, new() //Returns the level or 0
            => Upgrades.Where(x => x.Key == new TUpgrade().Id).FirstOrDefault().Value;

        public int GetUpgradeLevel(IUpgrade upgrade)
            => Upgrades.Where(x => x.Key == upgrade.Id).FirstOrDefault().Value;

        public void SetUpgradeLevel(IUpgrade upgrade, int level)
        {
            if (!Upgrades.TryAdd(upgrade.Id, level))
            {
                Upgrades[upgrade.Id] = level;
            }
        }

        public void ProcessAnsweredQuestion(Question question, QuestionResult newResult, QuestionResult oldResult,
                                            out int newTrophies, out int oldTrophies, out int magnets)
        {
            TotalAnsweredQuestions++;
            if (newResult.Correct)
            {
                TotalCorrectQuestions++;
            }

            oldTrophies = oldResult.CalculateTrophies(question, this);
            newTrophies = newResult.CalculateTrophies(question, this);

            var upgrade = new MagnetGainUpgrade();
            int level = GetUpgradeLevel(upgrade);
            magnets = upgrade.CalculateValue(level);

            if (!newResult.Correct)
            {
                magnets = -magnets;
            }

            if (Magnets + magnets < 0)
            {
                magnets = -Magnets;
            }

            Magnets += magnets;

            if (newResult.HasToReplace(oldResult))
            {
                ReplaceResult(newResult);
                Trophies += newTrophies - oldTrophies;
            }
        }

        public QuestionResult GetQuestionResult(Category category, int questionId)
            => AnsweredQuestions.Where(x => x.Category == category && x.QuestionId == questionId).FirstOrDefault() ?? QuestionResult.FromIncorrect(category, questionId);

        private void ReplaceResult(QuestionResult newResult)
        {
            AnsweredQuestions.RemoveAll(x => x.QuestionId == newResult.QuestionId && x.Category == newResult.Category);
            AnsweredQuestions.Add(newResult);
        }
    }
}
