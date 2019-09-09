using System.Threading.Tasks;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Minigame;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class QuizModule : SquizzyModule
    {
        public DbService Db { get; set; }
        public MinigameService Minigame { get; set; }

        [Command("Quiz", "Question", "Questions", "Ask", "Q")]
        public Task QuizAsync(Category category, int amount = 3)
        {
            var player = new MinigamePlayer(Context.Player, Context.User);
            var game = new Quizgame(player, Context.Channel, category, amount);

            return Minigame.StartMinigameAsync(game);
        }
    }
}
