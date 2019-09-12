using System;
using System.Collections.Generic;
using System.Linq;
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

        public Category GetRandomCategory(bool includeRandom = false)
        {
            var categories = ((Category[]) Enum.GetValues(typeof(Category))).ToList();
            
            if (!includeRandom)
            {
                categories.Remove(Category.Random);
            }

            return categories[Generator.Next(0, categories.Count - 1)];
        }
           

        public int RandomInt(int min, int max)
            => Generator.Next(min, max);

        public T PickRandom<T>(IList<T> list)
        {
            int index = Generator.Next(0, list.Count);
            return list[index];
        }
    }
}