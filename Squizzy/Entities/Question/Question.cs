using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using MongoDB.Bson.Serialization.Attributes;

namespace Squizzy.Entities
{
    public class Question
    {
        [BsonId]
        public int Id { get; private set; }

        [BsonRequired]
        public string Text { get; private set; }

        [BsonRequired]
        public Category Type { get; set; }

        [BsonRequired]
        public List<string> Options { get; private set; }

        [BsonRequired]
        public int CorrectOption { get; private set; }

        [BsonRequired]
        public int Reward { get; private set; }

        [BsonRequired]
        public int Time { get; private set; }

        [BsonRequired]
        public string PictureURL { get; private set; }

        [BsonIgnore]
        public bool HasPicture => !string.IsNullOrWhiteSpace(PictureURL);

        [BsonIgnore]
        public string CorrectValue => Options[CorrectOption];

        public Question(int id, string text, List<string> options, int correctOption, int reward, int time, string pictureURL)
        {
            Id = id;
            Text = text;
            Options = options;
            CorrectOption = correctOption;
            Reward = reward;
            Time = time;
            PictureURL = pictureURL;
        }

        private string GetOptionsMessage()
        {
            var builder = new StringBuilder();
            for (int i = 0; i < Options.Count; i++)
            {
                builder.AppendLine($"{i + 1}) {Options[i]}");
            }
            return builder.ToString();
        }

        public Embed ToEmbed(SocketUser user, int index = 1, int total = 1)
            => new EmbedBuilder()
                .WithColor(Color.Blue)
                .WithAuthor(user)
                .WithTitle($"{Type} Question#{Id}")
                .AddField(Text, GetOptionsMessage())
                .WithFooter($"You have {Time} Seconds to answer! [Question {index}/{total}]")
                .Build();
    }
}
