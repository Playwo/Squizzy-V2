using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using InteractivityAddon;
using Squizzy.Services;
using Squizzy.Extensions;
using InteractivityAddon.Selection;
using Squizzy.Entities;
using System.Text;

namespace Squizzy.Games
{
    public sealed class QuizRoyaleGame : MultiGame
    {
        private DbService _db;
        private InteractivityService _interactivity;
        private EmbedService _embed;
        private RandomizerService _random;

        public IMessageChannel Channel { get; }

        public override int MinimumPlayers => 3;
        public override int MaximumPlayers => 16;
        public override string Name => "Quiz Royale";

        public QuizRoyaleGame(IMessageChannel channel)
            : base(-1)
        {
            Channel = channel;
        }

        public override void SetPlayers(params MinigamePlayer[] players)
        {
            Ticks = players.Length - 1;
            base.SetPlayers(players);
        }

        public override Task InitializeAsync(IServiceProvider provider)
        {
            _db = provider.GetService<DbService>();
            _interactivity = provider.GetService<InteractivityService>();
            _embed = provider.GetService<EmbedService>();
            _random = provider.GetService<RandomizerService>();

            return Task.CompletedTask;
        }

        protected override async Task TickAsync(int tick)
        {
            var playerSpeeds = new Dictionary<MinigamePlayer, TimeSpan>();

            foreach (var player in Players)
            {
                await Channel.SendMessageAsync($"The next player is {player.User}.\nGet ready to answer!");
                await Task.Delay(1000);

                var question = await _db.LoadRandomQuestionAsync(Category.Random);
                var selectionEmbed = question.ToEmbedBuilder(player.User, 1, 1);

                var selection = new MessageSelectionBuilder<string>()
                    .WithSelectionEmbed(selectionEmbed)
                    .WithValues(question.Options.Shuffle(_random.Generator).ToList())
                    .WithTitle(question.Text)
                    .WithUsers(player.User)
                    .Build();

                var selectionResult = await _interactivity.SendSelectionAsync(selection, Channel, TimeSpan.FromSeconds(question.Time));

                var newResult = selectionResult.Value == question.CorrectValue
                    ? QuestionResult.FromCorrect(question.Type, question.Id, selectionResult.Elapsed)
                    : QuestionResult.FromIncorrect(question.Type, question.Id);

                playerSpeeds.Add(player, newResult.Correct
                    ? newResult.Time
                    : TimeSpan.FromSeconds(question.Time));
            }

            var orderedPlayers = playerSpeeds.OrderBy(x => x.Value.TotalMilliseconds).ToList();

            var description = new StringBuilder();

            for (int i = 0; i < orderedPlayers.Count; i++)
            {
                description.AppendLine($"#{i + 1} - {orderedPlayers[i].Key.User} [{Math.Round(orderedPlayers[i].Value.TotalSeconds, 2)} seconds]");
            }

            Players.Remove(orderedPlayers.Last().Key);

            await Task.Delay(1000);
            await Channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Multiplayer)
                .WithTitle($"The Round has ended! [Round {tick + 1}/{Ticks}]")
                .WithDescription($"**Results:**\n{description}")
                .Build());

            await Task.Delay(1000);
            await Channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Multiplayer)
                .WithTitle($"{orderedPlayers.Last().Key.User} has been kicked!")
                .WithDescription($"He/She needed {Math.Round(orderedPlayers.Last().Value.TotalSeconds, 2)} seconds to answer, which makes up the last place\n" +
                $"{(Players.Count > 1 ? "Everyone else get ready, the next round starts now :D" : "That was the last round, thanks for playing :D")}")
                .Build());

            if (Players.Count == 1)
            {
                var winner = Players.First();

                winner.Player.Honor += Ticks;
                winner.Player.MatchesWon++;

                await Channel.SendMessageAsync(embed: _embed.MakeMultiplayerGameWinEmbed(winner.User, "Quiz Royale", Ticks));
            }
        }

        public override Task CancelAsync()
        {
            Exit();
            return Task.CompletedTask;
        }

        public override Task SaveDataAsync()
            => _db.SavePlayersAsync(Players.Select(x => x.Player));
    }
}
