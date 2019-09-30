using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Squizzy.Extensions;
using Squizzy.Services;
using System.Linq;
using System.Timers;

namespace Squizzy.Games
{
    public class GameLobby : IDisposable
    {
        private const int LobbyTimeout = 180000; //3 mins

        private readonly MinigameService _minigame;
        private readonly BlockingService _blocking;
        private readonly RandomizerService _random;

        private List<MinigamePlayer> MinigamePlayers { get; set; }
        private IUserMessage Message { get; set; }
        private Timer TimeoutTimer { get; }

        public ulong Id { get; }
        public bool Opened { get; private set; }
        public IMessageChannel Channel { get; }
        public MinigamePlayer Creator { get; private set; }
        public MultiGame Game { get; private set; }
        public IReadOnlyCollection<MinigamePlayer> Players => MinigamePlayers.ToImmutableArray();
        public bool CanStart => Game == null
            ? false
            : MinigamePlayers.Count <= Game.MaximumPlayers && MinigamePlayers.Count >= Game.MinimumPlayers;

        public GameLobby(IMessageChannel channel, IServiceProvider provider, MinigamePlayer creator)
        {
            Id = (ulong) DateTime.UtcNow.Ticks;
            MinigamePlayers = new List<MinigamePlayer>();
            TimeoutTimer = new Timer(LobbyTimeout)
            {
                AutoReset = false,
                Enabled = false
            };
            TimeoutTimer.Elapsed += async (x, y) =>
            {
                await CloseAsync("Timed out", true);
            };
            Channel = channel;
            Creator = creator;

            _minigame = provider.GetService<MinigameService>();
            _blocking = provider.GetService<BlockingService>();
            _random = provider.GetService<RandomizerService>();
        }

        public Task OpenAsync()
        {
            Opened = true;
            _blocking.BlockChannel(Id, Channel.Id);
            TimeoutTimer.Start();
            return AddPlayerAsync(Creator);
        }

        public async Task<bool> AddPlayerAsync(MinigamePlayer player)
        {
            if (MinigamePlayers.Exists(x => x.Id == player.Id))
            {
                return false;
            }

            _blocking.BlockUser(Id, player.User.Id);
            MinigamePlayers.Add(player);
            await UpdateAsync(false);
            return true;
        }

        public async Task<bool> RemovePlayerAsync(MinigamePlayer player)
        {
            bool removedPlayer = MinigamePlayers.RemoveAll(x => x.Id == player.Id) > 0;

            if (removedPlayer)
            {
                _blocking.UnblockUser(Id, player.User.Id);
                await UpdateAsync(false);
            }

            return removedPlayer;
        }

        public Task SetGameAsync(MultiGame game)
        {
            Game = game;
            return UpdateAsync(true);
        }

        private async Task UpdateAsync(bool resend)
        {
            TimeoutTimer.Stop();
            TimeoutTimer.Start();

            var description = new StringBuilder();

            foreach (var player in MinigamePlayers)
            {
                description.AppendLine($" - {player.User}");
            }

            var embed = new EmbedBuilder()
                .WithColor(EmbedColor.Multiplayer)
                .WithTitle($"{Creator.User}'s Game Lobby")
                .AddField("Game", Game?.Name ?? "None")
                .AddField("Players", description)
                .Build();

            if (Message == null || resend == true)
            {
                try
                {
                    try
                    {
                        if (Message != null)
                        {
                            await Message.DeleteAsync();
                        }
                    }
                    catch { }


                    Message = await Channel.SendMessageAsync(embed: embed);
                }
                catch
                {
                    await CloseAsync("**Error**\nCould not send the Lobby message!", true);
                }
            }
            else
            {
                try
                {
                    await Message.ModifyAsync(x => x.Embed = embed);
                }
                catch
                {
                    Message = null;
                    await UpdateAsync(true);
                }
            }
        }

        public async Task CloseAsync(string reason, bool dispose)
        {
            try
            {
                Opened = false;

                var embed = new EmbedBuilder()
                    .WithTitle("Lobby closed")
                    .WithColor(EmbedColor.Multiplayer)
                    .WithDescription(reason)
                    .Build();

                await Message.ModifyAsync(x => x.Embed = embed);
            }
            catch
            {
            }
            finally
            {
                _blocking.UnblockCommandId(Id);

                if(dispose)
                {
                    Dispose();
                }
            }
        }

        public async Task StartAsync()
        {
            if (CanStart)
            {
                Players.Shuffle(_random.Generator);
                await CloseAsync("The game has been started", false);
                Game.SetPlayers(Players.ToArray());
                await _minigame.StartMinigameAsync(Game);
                Dispose();
            }
            else
            {
                var embed = new EmbedBuilder()
                    .WithColor(EmbedColor.Failed)
                    .WithTitle("Could not start the game")
                    .WithDescription(Game == null
                        ? "No game selected"
                        : Players.Count > Game.MaximumPlayers
                            ? "Too many players"
                            : "Too few players")
                    .Build();

                await Channel.SendMessageAsync(embed: embed);
            }
        }

        public void Dispose()
        {
            MinigamePlayers.Clear();
            MinigamePlayers = null;
            TimeoutTimer.Dispose();
            Game = null;

            _minigame.DeleteLobby(this);
        }
    }
}
