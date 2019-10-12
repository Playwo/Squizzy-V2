using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using InteractivityAddon;
using InteractivityAddon.Selection;
using Squizzy.Entities;
using Squizzy.Extensions;
using Squizzy.Services;

namespace Squizzy.Games
{
    public class DuelGame : MultiGame
    {
        private DbService _db;
        private RandomizerService _random;
        private InteractivityService _interactivity;
        private EmbedService _embed;

        public IMessageChannel Channel { get; }
        private Category Category { get; }

        public override int MinimumPlayers => 2;
        public override int MaximumPlayers => 2;
        public override string Name => "Duel";

        private Dictionary<MinigamePlayer, int> WinCounter { get; set; }


        public DuelGame(IMessageChannel channel, Category category, int rounds)
            : base(rounds)
        {
            Channel = channel;
            Category = category;
            WinCounter = new Dictionary<MinigamePlayer, int>();
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

                var question = await _db.LoadRandomQuestionAsync(Category);
                var selectionEmbed = question.ToEmbedBuilder(player.User, 1, 1);


                var selection = new MessageSelectionBuilder<string>()
                    .WithSelectionEmbed(selectionEmbed)
                    .WithValues(question.Options.Shuffle(_random.Generator).ToList())
                    .WithTitle(question.Text)
                    .WithUsers(player.User)
                    .Build();

                var selectionResult = await _interactivity.SendSelectionAsync(selection, Channel, TimeSpan.FromSeconds(question.Time));

                if (selectionResult.IsTimeouted)
                {
                    await player.IncreaseInactivityAsync();
                }

                var newResult = selectionResult.Value == question.CorrectValue
                    ? QuestionResult.FromCorrect(question.Type, question.Id, selectionResult.Elapsed)
                    : QuestionResult.FromIncorrect(question.Type, question.Id);

                playerSpeeds.Add(player, newResult.Correct
                    ? newResult.Time
                    : TimeSpan.FromSeconds(question.Time));
            }

            var thisRoundRanking = playerSpeeds.OrderBy(x => x.Value.TotalMilliseconds).ToList();

            WinCounter.AddOrUpdate(thisRoundRanking.First().Key, player => 1, (player, wins) => wins + 1);

            var totalRanking = Players.OrderByDescending(x => WinCounter.Where(x => x.Key.Id == x.Key.Id).FirstOrDefault().Value);

            var thisRoundRankingDescription = new StringBuilder();
            var totalRankingDescription = new StringBuilder();

            int i = 0;

            foreach (var player in thisRoundRanking)
            {
                i++;
                thisRoundRankingDescription.AppendLine($"#{i} {player.Key.User}: {Math.Round(player.Value.TotalSeconds, 2)} seconds");
            }

            i = 0;

            foreach (var player in totalRanking)
            {
                int wins = WinCounter.Where(x => x.Key.Id == player.Id).FirstOrDefault().Value;
                i++;
                totalRankingDescription.AppendLine($"#{i} {player.User}: {wins} rounds won");
            }


            await Task.Delay(1000);
            await Channel.SendMessageAsync(embed: new EmbedBuilder()
                .WithColor(EmbedColor.Multiplayer)
                .WithTitle($"The Round has ended! [Round {tick}/{Ticks}]")
                .WithDescription($"**Results**\nThis round:\n{thisRoundRankingDescription}\nTotal:\n{totalRankingDescription}")
                .Build());

            await Task.Delay(1000);



            if (tick == Ticks)
            {
                var winner = WinCounter.OrderByDescending(x => x.Value).First().Key;

                winner.Player.Honor += 2;
                winner.Player.MatchesWon++;

                await Channel.SendMessageAsync(embed: _embed.MakeMultiplayerGameWinEmbed(winner.User, "Duel", 2));
            }
        }

        public override Task OnPlayerInactiveAsync(MinigamePlayer inactivePlayer) => CancelAsync();

        public override Task CancelAsync()
        {
            Exit();
            return Channel.SendMessageAsync("The game has been cancelled!");
        }

        public override Task SaveDataAsync()
            => _db.SavePlayersAsync(Players.Select(x => x.Player));
    }
}
