using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using InteractivityAddon;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;
using Squizzy.Commands;
using Squizzy.Services;

namespace Squizzy
{
    public class SquizzySetup
    {
        public IConfigurationRoot Config { get; private set; }
        public DiscordShardedClient Client { get; private set; }
        public CommandService CommandService { get; private set; }
        public IServiceProvider Provider { get; private set; }

        public async Task InitializeAsync()
        {
            var clientConfig = new DiscordSocketConfig()
            {
                AlwaysDownloadUsers = true,
                DefaultRetryMode = RetryMode.Retry502,
                MessageCacheSize = 50,
                LogLevel = LogSeverity.Verbose
            };

            var commandConfig = new CommandServiceConfiguration()
            {
                DefaultRunMode = RunMode.Parallel,
                StringComparison = StringComparison.OrdinalIgnoreCase,
                IgnoresExtraArguments = true
            };

            Client = new DiscordShardedClient(clientConfig);
            CommandService = new CommandService(commandConfig);
            Config = MakeConfig();
            Provider = MakeProvider();

            CommandService.AddModules(Assembly.GetEntryAssembly());
            InitializeTypeParsers();

            await InitializeServicesAsync();
        }

        private async Task InitializeServicesAsync()
        {
            foreach (var type in GetServiceTypes())
            {
                var service = (SquizzyService) Provider.GetService(type);

                service.InjectServices(Provider);
                await service.InitializeAsync();
            }
        }

        private void InitializeTypeParsers()
        {
            CommandService.AddTypeParser(new UserParser());
            CommandService.AddTypeParser(new TextChannelParser());
            CommandService.AddTypeParser(new RoleParser());
            CommandService.AddTypeParser(new MessageParser());
            CommandService.AddTypeParser(new CategoryParser(), true);
            CommandService.AddTypeParser(new StatisticTypeParser(), true);
            CommandService.AddTypeParser(new LeaderboardTypeParser());
            CommandService.AddTypeParser(new PlayerParser());
        }

        private IServiceProvider MakeProvider()
        {
            var services = new ServiceCollection()
                .AddSingleton(Client)
                .AddSingleton(CommandService)
                .AddSingleton(Config)
                .AddSingleton(new InteractivityService(Client, TimeSpan.FromMinutes(2)));

            foreach (var type in GetServiceTypes())
            {
                services.AddSingleton(type);
            }

            return services.BuildServiceProvider();
        }

        private IConfigurationRoot MakeConfig() => new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("config.json")
                .Build();

        private Type[] GetServiceTypes() => Assembly.GetEntryAssembly().GetTypes()
                .Where(x => x.BaseType == typeof(SquizzyService))
                .ToArray();

        public async Task RunAsync()
        {
            await Client.LoginAsync(TokenType.Bot, Config["tokens:discord"]);
            await Client.StartAsync();
            await Client.SetGameAsync("Answering Questions", type: ActivityType.Playing);
            await Task.Delay(-1);
        }
    }
}
