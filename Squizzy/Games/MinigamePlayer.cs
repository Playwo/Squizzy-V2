using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Discord.WebSocket;
using Squizzy.Entities;

namespace Squizzy.Games
{
    public class MinigamePlayer : IEquatable<MinigamePlayer>
    {
        private const int InactivityThreshold = 2;

        public Game Parent { get; set; }
        public SquizzyPlayer Player { get; }
        public SocketUser User { get; }
        public int InactivityRow { get; private set; }
        public ulong Id => Player.Id;

        public MinigamePlayer(SquizzyPlayer player, SocketUser user)
        {
            Player = player;
            User = user;

            if (Player.Id != User.Id)
            {
                throw new InvalidOperationException("Player and User must have the same Id!");
            }
        }

        public void ResetInactivity()
            => InactivityRow = 0;

        public Task IncreaseInactivityAsync()
        {
            InactivityRow++;

            return InactivityRow > InactivityThreshold
                ? Parent.OnPlayerInactiveAsync(this)
                : Task.CompletedTask;
        }

        public bool Equals([AllowNull] MinigamePlayer other) => other?.Id == Id;
    }
}
