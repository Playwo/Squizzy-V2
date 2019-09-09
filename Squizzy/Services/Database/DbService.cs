using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using MongoDB.Driver;
using Squizzy.Commands;
using Squizzy.Entities;

namespace Squizzy.Services
{
    public class DbService : SquizzyService
    {
#pragma warning disable
        [Inject] private readonly IServiceProvider _provider;
        [Inject] private readonly DbBackEndService _dbBackEnd;
        [Inject] private readonly RandomizerService _random;
        [Inject] private readonly DiscordShardedClient _client;
#pragma warning restore

        #region Load
        public async Task<SquizzyPlayer> LoadPlayerAsync(ulong id)
            => await (await _dbBackEnd.PlayerCollection.FindAsync(x => x.Id == id)).FirstOrDefaultAsync() ?? new SquizzyPlayer(id);

        public async Task<Question> LoadQuestionAsync(Category type, int id)
        {
            var question = await (await _dbBackEnd.GetCategoryCollection(type).FindAsync(x => x.Id == id)).FirstOrDefaultAsync();

            if (question != null)
            {
                question.Type = type;
            }

            return question;
        }

        public async Task<Question> LoadNextQuestionAsync(SquizzyPlayer player, Category type)
        {
            if (type == Category.Random)
            {
                type = _random.GetRandomCategory();
            }

            var cursor = await _dbBackEnd.GetCategoryCollection(type).FindAsync(x => true);
            var remainingQuestions = await cursor.ToListAsync();
            remainingQuestions = remainingQuestions.OrderByDescending(y => player.AnsweredQuestions.Find(z => z.QuestionId == y.Id)?.Time ?? TimeSpan.MaxValue).ToList();
            return remainingQuestions[_random.RandomInt(0, 5)]; //Return one of the 6 slowest questions
        }

        public async Task<SquizzyContext> LoadContextAsync(SocketUserMessage message)
        {
            var guildUser = message.Author as SocketGuildUser;

            var shard = guildUser != null
                ? _client.GetShardFor(guildUser.Guild)
                : _client.GetShard(0);

            var player = await LoadPlayerAsync(message.Author.Id);

            return new SquizzyContext(shard, message, guildUser?.Guild, player, _provider);
        }

        #region Leaderboard
        public List<SquizzyPlayer> LoadLeaderboard(Leaderboard type, int amount)
        {
            switch (type)
            {
                case Leaderboard.Trophies:
                    return LoadTrohiesLb(amount);
                case Leaderboard.Magnets:
                    return LoadMagnetsLb(amount);
                case Leaderboard.Honor:
                    return LoadHonorLb(amount);
                case Leaderboard.AnsweredQuestions:
                    return LoadTotalAnsweredQuestionsLb(amount);
                case Leaderboard.SuccessRate:
                    return LoadSuccessRateLb(amount);
                default:
                    return new List<SquizzyPlayer>();
            }
        }

        private List<SquizzyPlayer> LoadSuccessRateLb(int amount)
            => _dbBackEnd.PlayerCollection.Find(x => true)
                .SortByDescending(x => x.SuccessRate)
                .Limit(amount)
                .ToList();

        private List<SquizzyPlayer> LoadTrohiesLb(int amount)
            => _dbBackEnd.PlayerCollection.Find(x => true)
                .SortByDescending(x => x.Trophies)
                .Limit(amount)
                .ToList();

        private List<SquizzyPlayer> LoadTotalAnsweredQuestionsLb(int amount)
            => _dbBackEnd.PlayerCollection.Find(x => true)
                .SortByDescending(x => x.TotalAnsweredQuestions)
                .Limit(amount)
                .ToList();

        private List<SquizzyPlayer> LoadMagnetsLb(int amount)
            => _dbBackEnd.PlayerCollection.Find(x => true)
                .SortByDescending(x => x.Magnets)
                .Limit(amount)
                .ToList();

        private List<SquizzyPlayer> LoadHonorLb(int amount)
            => _dbBackEnd.PlayerCollection.Find(x => true)
                .SortByDescending(x => x.Honor)
                .Limit(amount)
                .ToList();
        #endregion
        #endregion
        #region Save
        public async Task SavePlayerAsync(SquizzyPlayer player)
        {
            var options = new UpdateOptions() { IsUpsert = true }; //Create If non existing
            await _dbBackEnd.PlayerCollection.ReplaceOneAsync(x => x.Id == player.Id, player, options); //Replace Document
        }
        #endregion
    }
}
