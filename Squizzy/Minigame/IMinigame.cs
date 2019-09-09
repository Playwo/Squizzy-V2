using System;
using System.Threading.Tasks;

namespace Squizzy.Minigame
{
    public interface IMinigame
    {
        Task InitializeAsync(IServiceProvider provider);
        Task<MinigameResult> RunAsync();
        Task CancelAsync();
    }
}
