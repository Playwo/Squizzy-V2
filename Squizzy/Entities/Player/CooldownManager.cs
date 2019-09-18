using System;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Qmmands;

namespace Squizzy.Entities
{
    public class CooldownManager
    {
        [JsonRequired]
        public ConcurrentDictionary<string, DateTime> CommandUsage { get; }

        public CooldownManager()
        {
            CommandUsage = new ConcurrentDictionary<string, DateTime>();
        }

        public TimeSpan GetRemainingCommandCooldown(Command command)
            => CommandUsage.TryGetValue(command.Name, out var cooldownEndTime) == false
                ? TimeSpan.Zero
                : cooldownEndTime <= DateTime.UtcNow
                    ? TimeSpan.Zero
                    : cooldownEndTime - DateTime.UtcNow;

        public void SetCommandCooldown(Command command, TimeSpan cooldown)
            => CommandUsage.AddOrUpdate(command.Name, DateTime.UtcNow + cooldown, (key, value) => value = DateTime.UtcNow + cooldown);

    }
}
