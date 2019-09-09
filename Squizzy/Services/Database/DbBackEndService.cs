﻿using System;
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
#pragma warning restore

        private MongoClient Client { get; set; }
        private IMongoDatabase Database { get; set; }

        public IMongoCollection<SquizzyPlayer> PlayerCollection { get; private set; }
        public IMongoCollection<Question> ScrapTD_Collection { get; private set; }
        public IMongoCollection<Question> ScrapClicker2_Collection { get; private set; }
        public IMongoCollection<Question> ScrapClicker1_Collection { get; private set; }
        public IMongoCollection<Question> General_Collection { get; private set; }

        //public IMongoCollection<GlobalVal> GlobalsCollection { get; private set; }

        public override Task InitializeAsync()
        {
            Client = new MongoClient(_config["db:connection"]);
            Database = Client.GetDatabase(_config["db:name"]);

            PlayerCollection = Database.GetCollection<SquizzyPlayer>("Players");
            ScrapTD_Collection = Database.GetCollection<Question>("ScrapTD-Questions");
            ScrapClicker2_Collection = Database.GetCollection<Question>("ScrapClicker2-Questions");
            ScrapClicker1_Collection = Database.GetCollection<Question>("ScrapClicker1-Questions");
            General_Collection = Database.GetCollection<Question>("General-Questions");
            //GlobalsCollection = Database.GetCollection<GlobalVal>("Settings");

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
        {
            switch (type)
            {
                case Category.General:
                    return General_Collection;
                case Category.ScrapClicker1:
                    return ScrapClicker1_Collection;
                case Category.ScrapClicker2:
                    return ScrapClicker2_Collection;
                case Category.ScrapTD:
                    return ScrapTD_Collection;
                default:
                    return null;
            }
        }
    }
}
