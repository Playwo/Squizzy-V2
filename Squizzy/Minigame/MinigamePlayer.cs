using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Squizzy.Entities;

namespace Squizzy.Minigame
{
    public class MinigamePlayer
    {
        public SquizzyPlayer Player { get; }
        public SocketUser User { get; }
        public int InactivityRow { get; set; }
        public int InactivityThreshold { get; set; } = 3;
        public event Func<Task> OnInactive;

        public MinigamePlayer(SquizzyPlayer player, SocketUser user)
        {
            Player = player;
            User = user;
        }

        public void ResetInactivity()
            => InactivityRow = 0;

        public Task IncreaseInactivityAsync()
        {
            InactivityRow++;

            return InactivityRow > InactivityThreshold
                ? OnInactive?.Invoke()
                : Task.CompletedTask;
        }
    }
}
