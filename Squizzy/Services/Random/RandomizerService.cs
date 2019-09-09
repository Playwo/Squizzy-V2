using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squizzy.Entities;

namespace Squizzy.Services
{
    public class RandomizerService : SquizzyService
    {
        private Random Generator { get; set; }

        public override Task InitializeAsync()
        {
            Generator = new Random();
            return base.InitializeAsync();
        }

        public Category GetRandomCategory()
            => (Category) Generator.Next(0, 4);

        public int RandomInt(int min, int max)
            => Generator.Next(min, max);

        public T PickRandom<T>(IList<T> list)
        {
            int index = Generator.Next(0, list.Count);
            return list[index];
        }
    }
}