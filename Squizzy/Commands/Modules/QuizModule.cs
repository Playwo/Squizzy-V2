using System.Threading.Tasks;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Minigame;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Quiz :mortar_board:")]
    public class QuizModule : SquizzyModule
    {
        public DbService Db { get; set; }
        public MinigameService Minigame { get; set; }

        [Command("Quiz", "Question", "Questions", "Ask", "Q")]
        [Description("Start a singleplayer quiz game")]
        [RequireFreeRessources(RessourceType.Channel | RessourceType.User)]
        [RequireMaintenance(false)]
        public Task QuizAsync([Name("Category")]Category category, [Name("Amount")]int amount = 3)
        {
            var player = new MinigamePlayer(Context.Player, Context.User);
            var game = new Quizgame(player, Context.Channel, category, amount);

            return Minigame.StartMinigameAsync(game);
        }
    }
}
