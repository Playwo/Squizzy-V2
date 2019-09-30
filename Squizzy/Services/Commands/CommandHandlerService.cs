using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Qmmands;
using Squizzy.Commands;

namespace Squizzy.Services
{
    public class CommandHandlerService : SquizzyService
    {
#pragma warning disable
        [Inject] private readonly DiscordShardedClient _client;
        [Inject] private readonly DbService _db;
        [Inject] private readonly CommandService _commands;
        [Inject] private readonly IConfigurationRoot _config;
        [Inject] private readonly LoggerService _logger;
        [Inject] private readonly IServiceProvider _provider;
        [Inject] private readonly EmbedService _embed;
        [Inject] private readonly BlockingService _ressourceAdministration;
        [Inject] private readonly MaintenanceService _maintenance;
#pragma warning restore

        public override Task InitializeAsync()
        {
            _client.MessageReceived += HandleMessageAsync;
            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.CommandExecutionFailed += CommandExecutionFailedAsync;
            return base.InitializeAsync();
        }

        private Task HandleMessageAsync(SocketMessage msg)
        {
            _ = Task.Run(async () =>
              {
                  try
                  {
                      if (msg.Author.Id == _client.CurrentUser.Id || msg.Author.IsBot)
                      {
                          return;
                      }

                      if (msg is SocketUserMessage message)
                      {

                          if (!CommandUtilities.HasPrefix(msg.Content, _config["prefix"], out string output))
                          {
                              return;
                          }

                          if (_maintenance.IsMaintenanceEnabled() && !RequireHelper.IsHelper(message.Author, _provider))
                          {
                              await message.Channel.SendMessageAsync(embed: _embed.GetDisabledDueToMaintenanceEmbed());
                              return;
                          }

                          var context = await _db.LoadContextAsync(message);
                          var result = await _commands.ExecuteAsync(output, context);

                          if (!(result is FailedResult failedResult))
                          {
                              return;
                          }
                          if (result is CommandNotFoundResult)
                          {
                              return;
                          }
                          var response = _embed.GetFailedResultEmbed(failedResult);
                          await msg.Channel.SendMessageAsync(embed: response);
                          _ressourceAdministration.UnblockCommandId(context.CommandId);
                      }
                  }
                  catch(Exception ex)
                  {
                      await _logger.ReportErrorAsync(msg, ex);
                  }
              });
            return Task.CompletedTask;
        }

        private async Task CommandExecutedAsync(CommandExecutedEventArgs args)
        {
            var ctx = args.Context as SquizzyContext;
            if (!_maintenance.IsMaintenanceEnabled() && !ctx.Command.Attributes.Any(x => x.GetType() != typeof(NoSaveAttribute))) 
            {
                await _db.SavePlayerAsync(ctx.Player);
            }

            var message = new LogMessage(LogSeverity.Info, "CommandHandler", $"Executed {ctx.Command.Name} in {ctx.Channel.Name} for {ctx.User.Username}");
            await _logger.LogAsync(message);
            _ressourceAdministration.UnblockCommandId(ctx.CommandId);
        }

        private async Task CommandExecutionFailedAsync(CommandExecutionFailedEventArgs args)
        {
            var ctx = args.Context as SquizzyContext;

            if (args.Result.Exception != null)
            {
                await _logger.ReportErrorAsync(ctx.Message, args.Result.Exception);
            }

            var message = new LogMessage(LogSeverity.Warning, "CommandHandler", $"Command {ctx.Command.Name} Execution failed in {ctx.Channel.Name} for {ctx.User.Username}", args.Result.Exception);
            await _logger.LogAsync(message);

            _ressourceAdministration.UnblockCommandId(ctx.CommandId);
        }
    }
}