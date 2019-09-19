using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using InteractivityAddon;
using InteractivityAddon.Selection;
using Squizzy.Entities;
using Squizzy.Extensions;
using Squizzy.Services;

namespace Squizzy.Minigame
{
    public class Quizgame : IMinigame
    {
        private DbService _db;
        private InteractivityService _interactivity;
        private EmbedService _embed;
        private RandomizerService _random;

        public IMessageChannel Channel { get; }
        public MinigamePlayer MinigamePlayer { get; }
        public Category QuestionType { get; }
        public int TotalQuestions { get; }
        public bool Cancelled { get; private set; }

        public Quizgame(MinigamePlayer player, IMessageChannel channel, Category type, int questionCount)
        {
            MinigamePlayer = player;
            Channel = channel;
            QuestionType = type;
            TotalQuestions = questionCount;
        }

        public Task InitializeAsync(IServiceProvider provider)
        {
            _db = provider.GetService<DbService>();
            _interactivity = provider.GetService<InteractivityService>();
            _embed = provider.GetService<EmbedService>();
            _random = provider.GetService<RandomizerService>();

            MinigamePlayer.OnInactive += HandleInactivityAsync;
            return Task.CompletedTask;
        }

        public async Task<MinigameResult> RunAsync()
        {
            for (int i = 0; i < TotalQuestions; i++)
            {
                var question = await _db.LoadNextQuestionAsync(MinigamePlayer.Player, QuestionType);
                var lastResult = MinigamePlayer.Player.GetQuestionResult(question.Type, question.Id);

                var selectionEmbed = new EmbedBuilder()
                    .WithThumbnailUrl(question.PictureURL)
                    .WithColor(EmbedColor.Quiz)
                    .WithTitle($"{question.Type} Question #{question.Id}")
                    .WithFooter($"You have {question.Time} seconds to answer. [Question {i + 1}/{TotalQuestions}]");

                if (lastResult.Correct)
                {
                    selectionEmbed.WithDescription($"Last time: {Math.Round(lastResult.Time.TotalSeconds, 3)} Seconds");
                }

                var selection = new MessageSelectionBuilder<string>()
                    .WithSelectionEmbed(selectionEmbed)
                    .WithValues(question.Options.Shuffle(_random.Generator).ToList())
                    .WithAllowCancel(true)
                    .WithTitle(lastResult.Perfect ? $":crown: {question.Text} :crown:" : question.Text)
                    .WithCancelDisplayName("Cancel (Counts as Wrong)")
                    .WithUsers(MinigamePlayer.User)
                    .Build();

                var selectionResult = await _interactivity.SendSelectionAsync(selection, Channel, TimeSpan.FromSeconds(question.Time));

                var newResult = selectionResult.Value == question.CorrectValue
                    ? QuestionResult.FromCorrect(question.Type, question.Id, selectionResult.Elapsed)
                    : QuestionResult.FromIncorrect(question.Type, question.Id);

                MinigamePlayer.Player.ProcessAnsweredQuestion(question, newResult, lastResult, 
                    out int newTrophies, out int oldTrophies, out int magnets);

                var embed = _embed.MakeQuestionResultEmbed(lastResult, newResult, oldTrophies, newTrophies, magnets);
                await Channel.SendMessageAsync(embed: embed);

                if (selectionResult.IsTimeouted)
                {
                    await MinigamePlayer.IncreaseInactivityAsync();
                }
                else
                {
                    MinigamePlayer.ResetInactivity();
                }
                if (Cancelled == true || selectionResult.IsCancelled)
                {
                    break;
                }
            }

            return new MinigameResult();
        }

        public Task CancelAsync()
        {
            Cancelled = true;
            return Task.CompletedTask;
        }

        public async Task HandleInactivityAsync()
        {
            await Channel.SendMessageAsync($"{MinigamePlayer.User.Mention} - The quiz has been cancelled due to inactivity!");
            await CancelAsync();
        }
    }
}
