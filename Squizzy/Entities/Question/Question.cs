﻿using System.Collections.Generic;
using Discord;
using Discord.WebSocket;
using MongoDB.Bson.Serialization.Attributes;
using Squizzy.Services;

namespace Squizzy.Entities
{
    public class Question
    {
        [BsonId]
        public int Id { get; private set; }

        [BsonElement]
        public string Text { get; private set; }

        [BsonElement]
        public Category Type { get; set; }

        [BsonElement]
        public List<string> Options { get; private set; }

        [BsonElement]
        public int CorrectOption { get; private set; }

        [BsonElement]
        public int Reward { get; private set; }

        [BsonElement]
        public int Time { get; private set; }

        [BsonElement]
        public string PictureURL { get; private set; }

        [BsonIgnore]
        public bool HasPicture => !string.IsNullOrWhiteSpace(PictureURL);

        [BsonIgnore]
        public string CorrectValue => Options[CorrectOption];

        public Question(int id, string text, List<string> options, int correctOption, int reward, int timeInSeconds, string pictureURL)
        {
            Id = id;
            Text = text;
            Options = options;
            CorrectOption = correctOption;
            Reward = reward;
            Time = timeInSeconds;
            PictureURL = pictureURL;
        }

        public EmbedBuilder ToEmbedBuilder(SocketUser user, int index, int total)
            => new EmbedBuilder()
                    .WithAuthor(user)
                    .WithThumbnailUrl(PictureURL)
                    .WithColor(EmbedColor.Quiz)
                    .WithTitle($"{Type} Question #{Id}")
                    .WithFooter($"You have {Time} seconds to answer. [Question {index}/{total}]");
    }
}
