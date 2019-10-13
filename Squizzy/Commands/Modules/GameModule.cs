using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Games;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Quiz :mortar_board:")]
    public class GameModule : SquizzyModule
    {
        public DbService Db { get; set; }
        public MinigameService Minigame { get; set; }

        [Command("Quiz", "Question", "Questions", "Ask", "Q")]
        [Description("Start a singleplayer quiz game")]
        [RequireFreeRessources(RessourceType.Channel | RessourceType.User)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireMaintenance(false)]
        public Task QuizAsync([Name("Category")]Category category, [Range(1, 50)][Name("Amount")]int amount = 3)
        {
            var player = new MinigamePlayer(Context.Player, Context.User);
            var game = new QuizGame(Context.Channel, player, category, amount);

            return Minigame.StartMinigameAsync(game);
        }

        [Command("CreateLobby", "OpenLobby", "StartMultiplayer", "Multiplayer", "Lobby", "StartMulti", "Multi")]
        [Description("Open a lobby to play multiplayer games")]
        [RequireFreeRessources(RessourceType.Channel | RessourceType.User)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireMaintenance(false)]
        [RequireMultiplayer(true)]
        public Task OpenLobbyAsync()
        {
            var creator = new MinigamePlayer(Context.Player, Context.User);
            return Minigame.OpenLobbyAsync(Context.Channel, creator);
        }

        [Command("SetGame", "SpecifyGame")]
        [Description("Sets the game of your current Multiplayer Lobby")]
        [RequireLobby(true, needsLeader: true)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireMultiplayer(true)]
        public Task ChangeLobbyGameAsync([Name("Game")]MultiGame game)
        {
            var lobby = Minigame.GetLobby(Context.User);
            return lobby.SetGameAsync(game);
        }

        [Command("StartLobby", "Start", "RunLobby", "PlayLobby")]
        [Description("Start the game in your Multiplayer Lobby")]
        [RequireLobby(true, needsLeader: true)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireMultiplayer(true)]
        public Task StartLobbyAsync()
        {
            var lobby = Minigame.GetLobby(Context.User);
            return lobby.StartAsync();
        }

        [Command("JoinLobby", "JoinGame", "JoinMultiplayer", "JoinMulti", "Join")]
        [Description("Join the lobby in the current channel")]
        [RequireLobbyInChannel]
        [RequireLobby(false)]
        [RequireFreeRessources(RessourceType.User)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [RequireMultiplayer(true)]
        public Task JoinLobbyAsync()
        {
            var lobby = Minigame.GetLobby(Context.Channel);
            var player = new MinigamePlayer(Context.Player, Context.User);
            return lobby.AddPlayerAsync(player);
        }

        [Command("CloseLobby", "ExitLobby", "CancelLobby")]
        [Description("Closes your Multiplayer Lobby")]
        [RequireLobby(true, needsLeader: true)]
        [RequireMultiplayer(true)]
        public Task CloseLobbyAsync()
        {
            var lobby = Minigame.GetLobby(Context.User);
            return lobby.CloseAsync("Closed by Lobby Leader", true);
        }
    }
}
