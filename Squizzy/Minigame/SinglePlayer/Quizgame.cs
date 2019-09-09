using System;
using System.Threading.Tasks;
using Discord;
using InteractivityAddon;
using InteractivityAddon.Selection;
using Microsoft.Extensions.DependencyInjection;
using Squizzy.Entities;
using Squizzy.Services;

namespace Squizzy.Minigame
{
    public class Quizgame : IMinigame
    {
        private DbService _db;
        private InteractivityService _interactivity;
        private EmbedService _embed;

        public IMessageChannel Channel { get; }
        public MinigamePlayer Player { get; }
        public Category QuestionType { get; }
        public int TotalQuestions { get; }
        public bool Cancelled { get; private set; }

        public Quizgame(MinigamePlayer player, IMessageChannel channel, Category type, int questionCount)
        {
            Player = player;
            Channel = channel;
            QuestionType = type;
            TotalQuestions = questionCount;
        }

        public Task InitializeAsync(IServiceProvider provider)
        {
            _db = provider.GetService<DbService>();
            _interactivity = provider.GetService<InteractivityService>();
            _embed = provider.GetService<EmbedService>();

            Player.OnInactive += HandleInactivityAsync;
            return Task.CompletedTask;
        }

        public async Task<MinigameResult> RunAsync()
        {
            for (int i = 0; i < TotalQuestions; i++)
            {
                var question = await _db.LoadNextQuestionAsync(Player.Player, QuestionType);

                var selection = new MessageSelectionBuilder<string>()
                    .WithSelectionEmbed(new EmbedBuilder()
                    .WithThumbnailUrl(question.PictureURL)
                    .WithColor(Color.Blue)
                    .WithTitle($"{question.Type} Question #{question.Id}")
                    .WithFooter($"You have {question.Time} seconds to answer. [Question {i + 1}/{TotalQuestions}]"))
                    .WithValues(question.Options)
                    .WithAllowCancel(allowCancel: true)
                    .WithTitle(question.Text)
                    .WithCancelDisplayName("Cancel (Counts as Wrong)")
                    .WithUsers(Player.User)
                    .Build();

                var result = await _interactivity.SendSelectionAsync(selection, Channel, TimeSpan.FromSeconds(question.Time));

                var newResult = result.Value == question.CorrectValue
                    ? QuestionResult.FromCorrect(question, result.Elapsed)
                    : QuestionResult.FromIncorrect(question);

                Player.Player.ProcessAnsweredQuestion(newResult, out var oldResult);
                var embed = _embed.MakeQuestionResultEmbed(question, oldResult, newResult);
                await Channel.SendMessageAsync(embed: embed);

                if (result.IsTimeouted)
                {
                    await Player.IncreaseInactivityAsync();
                }
                else
                {
                    Player.ResetInactivity();
                }
                if (Cancelled == true || result.IsCancelled)
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
            await Channel.SendMessageAsync($"{Player.User.Mention} - The quiz has been cancelled due to inactivity!");
            await CancelAsync();
        }
    }
}
