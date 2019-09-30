using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using InteractivityAddon;
using Squizzy.Games;

namespace Squizzy.Services
{
    public class MinigameService : SquizzyService
    {
#pragma warning disable
        [Inject] private readonly IServiceProvider _provider;
        [Inject] private readonly InteractivityService _interactivity;
        [Inject] private readonly BlockingService _blocking;
#pragma warning restore

        private ConcurrentDictionary<ulong, GameLobby> Lobbies { get; set; }
        private object LobbyLock { get; set; }

        public override Task InitializeAsync()
        {
            Lobbies = new ConcurrentDictionary<ulong, GameLobby>();
            LobbyLock = new object();
            return base.InitializeAsync();
        }

        public async Task StartMinigameAsync(Games.Game minigame)
        {
            await minigame.InitializeAsync(_provider);
            await minigame.ExecuteAsync();
        }

        public async Task OpenLobbyAsync(IMessageChannel channel, MinigamePlayer creator)
        {
            var lobby = new GameLobby(channel, _provider, creator);

            Lobbies.TryAdd(channel.Id, lobby);
            await lobby.OpenAsync();
        }

        public void DeleteLobby(GameLobby lobby)
        {
            lock(LobbyLock)
            {
                Lobbies.TryRemove(lobby.Channel.Id, out _);
            }
        }

        public GameLobby GetLobby(SocketUser user)
        {
            lock(LobbyLock)
            {
                return Lobbies.Values.Where(x => x.Players.Any(z => z.Id == user.Id)).FirstOrDefault();
            }
        }

        public GameLobby GetLobby(IMessageChannel channel)
        {
            lock(LobbyLock)
            {
                Lobbies.TryGetValue(channel.Id, out var lobby);
                return lobby;
            }
        }
    }
}
