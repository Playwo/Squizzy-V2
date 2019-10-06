using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using InteractivityAddon;
using InteractivityAddon.Selection;
using Squizzy.Entities;
using Squizzy.Extensions;
using Squizzy.Services;

namespace Squizzy.Games
{
    public sealed class QuizGame : Game
    {
        private DbService _db;
        private InteractivityService _interactivity;
        private EmbedService _embed;
        private RandomizerService _random;

        public IMessageChannel Channel { get; }
        public MinigamePlayer Player { get; }
        public Category QuestionType { get; }

        public QuizGame(IMessageChannel channel, MinigamePlayer player, Category type, int questionCount)
            :base(questionCount)
        {
            player.Parent = this;

            Player = player;
            Channel = channel;
            QuestionType = type;
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
            var question = await _db.LoadNextQuestionAsync(Player.Player, QuestionType);
            var lastResult = Player.Player.GetQuestionResult(question.Type, question.Id);

            var selectionEmbed = question.ToEmbedBuilder(Player.User, tick, Ticks);

            if (lastResult.Correct)
            {
                selectionEmbed.WithDescription($"Last time: {Math.Round(lastResult.Time.TotalSeconds, 3)} Seconds");
            }
            else
            {
                selectionEmbed.WithDescription($"Last result: Wrong");
            }

            var selection = new MessageSelectionBuilder<string>()
                .WithSelectionEmbed(selectionEmbed)
                .WithValues(question.Options.Shuffle(_random.Generator).ToList())
                .WithAllowCancel(true)
                .WithTitle(lastResult.Perfect ? $":crown: {question.Text} :crown:" : question.Text)
                .WithCancelDisplayName("Cancel (Counts as Wrong)")
                .WithUsers(Player.User)
                .Build();

            var selectionResult = await _interactivity.SendSelectionAsync(selection, Channel, TimeSpan.FromSeconds(question.Time));

            var newResult = selectionResult.Value == question.CorrectValue
                ? QuestionResult.FromCorrect(question.Type, question.Id, selectionResult.Elapsed)
                : QuestionResult.FromIncorrect(question.Type, question.Id);

            Player.Player.ProcessAnsweredQuestion(question, newResult, lastResult,
                out int newTrophies, out int oldTrophies, out int magnets);

            var embed = _embed.MakeQuestionResultEmbed(lastResult, newResult, oldTrophies, newTrophies, magnets);
            await Channel.SendMessageAsync(embed: embed);

            if (selectionResult.IsCancelled)
            {
                Exit();
                return;
            }
            if (selectionResult.IsTimeouted)
            {
                await Player.IncreaseInactivityAsync();
            }
            else
            {
                Player.ResetInactivity();
            }
        }

        public override Task OnPlayerInactiveAsync(MinigamePlayer inactivePlayer)
        {
            Exit();
            return Channel.SendMessageAsync($"{Player.User.Mention} - The quiz has been cancelled due to inactivity!");
        }

        public override Task CancelAsync()
        {
            Exit();
            return Task.CompletedTask;
        }
    }
}
