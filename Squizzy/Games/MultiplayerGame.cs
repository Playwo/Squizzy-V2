using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squizzy.Games
{
    public abstract class MultiGame : Game
    {
        public List<MinigamePlayer> Players { get; private set; }

        public abstract int MinimumPlayers { get; }
        public abstract int MaximumPlayers { get; }
        public abstract string Name { get; }

        public MultiGame(int ticks)
            : base(ticks)
        {
        }

        public virtual void SetPlayers(params MinigamePlayer[] players)
        {
            Players = players.ToList();

            foreach (var player in Players)
            {
                player.Parent = this;
                player.Player.MatchesPlayed++;
            }

            if (Players.Count < MinimumPlayers || Players.Count > MaximumPlayers)
            {
                throw new InvalidOperationException("Too few or too many players for that minigame!");
            }
        }
    }
}
