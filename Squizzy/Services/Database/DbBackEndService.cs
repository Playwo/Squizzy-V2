using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using Squizzy.Entities;

namespace Squizzy.Services
{
    public class DbBackEndService : SquizzyService
    {
#pragma warning disable
        [Inject] private readonly IConfigurationRoot _config;
        [Inject] private readonly RandomizerService _random;
#pragma warning restore

        private MongoClient Client { get; set; }
        private IMongoDatabase Database { get; set; }

        public IMongoCollection<SquizzyPlayer> PlayerCollection { get; private set; }
        public IMongoCollection<Question> ScrapTD_Collection { get; private set; }
        public IMongoCollection<Question> ScrapClicker2_Collection { get; private set; }
        public IMongoCollection<Question> ScrapClicker1_Collection { get; private set; }
        public IMongoCollection<Question> General_Collection { get; private set; }

        public override Task InitializeAsync()
        {
            Client = new MongoClient(_config["db:connection"]);
            Database = Client.GetDatabase(_config["db:name"]);

            PlayerCollection = Database.GetCollection<SquizzyPlayer>("Players");
            ScrapTD_Collection = Database.GetCollection<Question>("ScrapTD-Questions");
            ScrapClicker2_Collection = Database.GetCollection<Question>("ScrapClicker2-Questions");
            ScrapClicker1_Collection = Database.GetCollection<Question>("ScrapClicker1-Questions");
            General_Collection = Database.GetCollection<Question>("General-Questions");

            return base.InitializeAsync();
        }

        public int Latency
        {
            get {
                var start = DateTime.Now;
                Database.RunCommand((Command<BsonDocument>) "{ping:1}");
                var latency = DateTime.Now - start;
                return (int) latency.TotalMilliseconds;
            }
        }

        public IMongoCollection<Question> GetCategoryCollection(Category type) 
            => type switch
        {
            Category.General => General_Collection,
            Category.ScrapClicker1 => ScrapClicker1_Collection,
            Category.ScrapClicker2 => ScrapClicker2_Collection,
            Category.ScrapTD => ScrapTD_Collection,
            Category.Random => GetCategoryCollection(_random.GetRandomCategory()),
            _ => null,
        };
    }
}
