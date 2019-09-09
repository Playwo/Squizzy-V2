using System;
using System.Threading.Tasks;
using InteractivityAddon;
using Squizzy.Minigame;

namespace Squizzy.Services
{
    public class MinigameService : SquizzyService
    {
#pragma warning disable
        [Inject] private readonly IServiceProvider _provider;
        [Inject] private readonly InteractivityService _interactivity;
#pragma warning restore

        public async Task<MinigameResult> StartMinigameAsync(IMinigame minigame)
        {
            await minigame.InitializeAsync(_provider);
            return await minigame.RunAsync();
        }
    }
}
