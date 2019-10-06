using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
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
        public async Task<SquizzyPlayer> LoadPlayerAsync(SocketUser user)
        {
            var cursor = await _dbBackEnd.PlayerCollection.FindAsync(x => x.Id == user.Id);
            var player = (await cursor.FirstOrDefaultAsync()) ?? new SquizzyPlayer(user.Id);
            player.Name = user.ToString();
            return player;
        }
        public async Task<Question> LoadQuestionAsync(Category type, int id)
        {
            var question = await (await _dbBackEnd.GetCategoryCollection(type).FindAsync(x => x.Id == id)).FirstOrDefaultAsync();

            if (question != null)
            {
                question.Type = type;
            }

            return question;
        }

        public async Task<List<Question>> LoadAllQuestionsAsync()
        {
            var tempQuestionStore = new List<Question>();

            tempQuestionStore.AddRange(await LoadQuestionsAsync(Category.General));
            tempQuestionStore.AddRange(await LoadQuestionsAsync(Category.ScrapClicker1));
            tempQuestionStore.AddRange(await LoadQuestionsAsync(Category.ScrapClicker2));
            tempQuestionStore.AddRange(await LoadQuestionsAsync(Category.ScrapTD));

            return tempQuestionStore;
        }

        public Task<Question> LoadRandomQuestionAsync(Category type) 
            => _dbBackEnd.GetCategoryCollection(type).AsQueryable().Sample(1).FirstOrDefaultAsync();

        public async Task<Question> LoadNextQuestionAsync(SquizzyPlayer player, Category type)
        {
            var remainingQuestions = await LoadQuestionsAsync(type);
            remainingQuestions = remainingQuestions
                .OrderByDescending(y => player.AnsweredQuestions.Find(z => z.Correct && z.QuestionId == y.Id && z.Category == y.Type)?.Time ?? TimeSpan.MaxValue).ToList();
            return remainingQuestions.ElementAt(_random.RandomInt(0, 10)); //Return one of the 11 slowest questions
        }

        public async Task<SquizzyContext> LoadContextAsync(SocketUserMessage message)
        {
            var guildUser = message.Author as SocketGuildUser;

            var shard = guildUser != null
                ? _client.GetShardFor(guildUser.Guild)
                : _client.GetShard(0);

            var player = await LoadPlayerAsync(message.Author);

            return new SquizzyContext(shard, message, guildUser?.Guild, player, _provider);
        }

        public async Task<IEnumerable<Question>> LoadQuestionsAsync(Category category) 
            => (await _dbBackEnd.GetCategoryCollection(category).FindAsync(x => true)).ToEnumerable();

        #region Leaderboard
        public List<SquizzyPlayer> LoadLeaderboard(Leaderboard type, int amount) 
            => type switch
        {
            Leaderboard.Trophies => LoadTrohiesLb(amount),
            Leaderboard.Magnets => LoadMagnetsLb(amount),
            Leaderboard.Honor => LoadHonorLb(amount),
            Leaderboard.AnsweredQuestions => LoadTotalAnsweredQuestionsLb(amount),
            _ => new List<SquizzyPlayer>(),
        };

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

        public Task SavePlayersAsync(params SquizzyPlayer[] players)
            => SavePlayersAsync(players.AsEnumerable());

        public async Task SavePlayersAsync(IEnumerable<SquizzyPlayer> players)
        {
            foreach (var player in players)
            {
                await SavePlayerAsync(player);
            }
        }
        #endregion
        #region Count
        public async Task<int> CountMaximumTrophiesAsync()
            => (await LoadAllQuestionsAsync()).Sum(x => x.Reward);

        public Task<long> CountPlayersAsync()
            => _dbBackEnd.PlayerCollection.CountDocumentsAsync(x => true);

        public Task<long> CountQuestionsAsync(Category category)
            => _dbBackEnd.GetCategoryCollection(category).CountDocumentsAsync(x => true);

        public async Task<long> CountTotalQuestionsAsync()
        {
            long totalCount = 0;

            foreach(Category category in Enum.GetValues(typeof(Category)))
            {
                if (category == Category.Random)
                {
                    continue;
                }

                totalCount += await CountQuestionsAsync(category);
            }

            return totalCount;
        }

    #endregion

    public async Task RecalculateTrophiesAsync()
        {
            var tempQuestionStore = new List<Question>();

            tempQuestionStore.AddRange(await LoadQuestionsAsync(Category.General));
            tempQuestionStore.AddRange(await LoadQuestionsAsync(Category.ScrapClicker1));
            tempQuestionStore.AddRange(await LoadQuestionsAsync(Category.ScrapClicker2));
            tempQuestionStore.AddRange(await LoadQuestionsAsync(Category.ScrapTD));

            foreach(var player in (await _dbBackEnd.PlayerCollection.FindAsync(x => true)).ToEnumerable())
            {
                int trophies = 0;

                foreach(var questionResult in player.AnsweredQuestions)
                {
                    var question = tempQuestionStore.Find(x => x.Type == questionResult.Category && x.Id == questionResult.QuestionId);
                    trophies += questionResult.CalculateTrophies(question);
                }

                player.Trophies = trophies;
                await SavePlayerAsync(player);
            }
        }
    }
}
