using System.Threading.Tasks;
using Squizzy.Extensions;
using Qmmands;
using Squizzy.Services;

namespace Squizzy.Commands
{
    public class RequireLobby : SquizzyCheck
    {
        public override string Description => $"Requires you to be {(NeedsLeader ? "leader" : "member")} of a multiplayer lobby";
        public bool NeedsLeader { get; }
        public bool NeedsLobby { get; }
        public bool NeedsOpened { get; }

        public RequireLobby(bool needsLobby, bool needsOpened = true, bool needsLeader = false)
        {
            NeedsOpened = needsOpened;
            NeedsLobby = needsLobby;
            NeedsLeader = needsLeader;
        }

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
        {
            var minigame = context.ServiceProvider.GetService<MinigameService>();
            var lobby = minigame.GetLobby(context.User);

            //I know this is trash lol

            if (lobby == null)
            {
                if (NeedsLobby)
                {
                    return CheckResult.Unsuccessful("You need to be member of a lobby to do that");
                }
            }
            else
            {
                if (!NeedsLobby)
                {
                    return CheckResult.Unsuccessful("You can't do that while already being in another lobby");
                }
                if (NeedsOpened)
                {
                    if (!lobby.Opened)
                    {
                        return CheckResult.Unsuccessful("You can't do that after the lobby has been closed");
                    }
                }
                else
                {
                    if (lobby.Opened)
                    {
                        return CheckResult.Unsuccessful("You can't do that while the lobby is still open");
                    }
                }
                if (NeedsLeader && lobby.Creator.Id != context.User.Id)
                {
                    return CheckResult.Unsuccessful("You need to be leader of a lobby to do that");
                }
            }

            return CheckResult.Successful;
        }
    }
}
