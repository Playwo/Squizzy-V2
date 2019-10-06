using System;
using System.Threading.Tasks;

namespace Squizzy.Games
{
    public abstract class Game
    {
        private TaskCompletionSource<object> ExitSource { get; }
        public int Ticks { get; set; }

        public Game(int ticks)
        {
            Ticks = ticks;
            ExitSource = new TaskCompletionSource<object>();
        }

        protected abstract Task TickAsync(int tick);
        public abstract Task InitializeAsync(IServiceProvider provider);
        public virtual Task OnPlayerInactiveAsync(MinigamePlayer inactivePlayer) => Task.CompletedTask;
        public abstract Task CancelAsync();
        public virtual Task SaveDataAsync() => Task.CompletedTask;

        public async Task ExecuteAsync()
        {
            for (int tick = 1; tick <= Ticks; tick++)
            {
                var tickTask = TickAsync(tick);
                var exitTask = ExitSource.Task;

                var firstTask = await Task.WhenAny(tickTask, exitTask);

                if (firstTask == exitTask)
                {
                    return;
                }
            }

            await SaveDataAsync();
        }

        public void Exit() => ExitSource.TrySetResult(null);
    }
}
