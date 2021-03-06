﻿using System;
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
                      if (msg.Author == _client.CurrentUser || msg.Author.IsBot)
                      {
                          return;
                      }
                      if (msg.Channel is SocketDMChannel)
                      {
                          await msg.Channel.SendMessageAsync("Sorry, but I do not support DM commands!\nIf you want to play visit the official scrap server: https://discord.gg/W3d9Jvx");
                      }

                      if (msg is SocketUserMessage message && msg.Channel is SocketTextChannel channel)
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

                          var check = new RequireBotPermissionAttribute(ChannelPermission.SendMessages);
                          var checkResult = await check.CheckAsync(context);

                          if (!checkResult.IsSuccessful)
                          {
                              return;
                          }

                          try
                          {
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
                          }
                          catch (Exception ex)
                          {
                              await _logger.ReportErrorAsync(msg, ex);
                          }
                          finally
                          {
                              _ressourceAdministration.UnblockCommandId(context.CommandId);
                          }
                      }
                  }
                  catch (Exception ex)
                  {
                      await _logger.ReportErrorAsync(msg, ex);
                  }
              });
            return Task.CompletedTask;
        }

        private async Task CommandExecutedAsync(CommandExecutedEventArgs args)
        {
            var ctx = args.Context as SquizzyContext;

            try
            {
                if (!_maintenance.IsMaintenanceEnabled() && ctx.Command.Attributes.Any(x => x.GetType() == typeof(SaveAttribute)))
                {
                    await _db.SavePlayerAsync(ctx.Player);
                }

                var message = new LogMessage(LogSeverity.Info, "CommandHandler", $"Executed {ctx.Command.Name} in {ctx.Channel.Name} for {ctx.User.Username}");
                await _logger.LogAsync(message);
            }
            catch (Exception ex)
            {
                await _logger.ReportErrorAsync(ctx.Message, ex);
                var message = new LogMessage(LogSeverity.Warning, "CommandHandler", $"Command {ctx.Command.Name} Error in CommandExecutedAsync! Channel: {ctx.Channel.Name} User: {ctx.User.Username}", ex);
                await _logger.LogAsync(message);
            }
            finally
            {
                _ressourceAdministration.UnblockCommandId(ctx.CommandId);
            }
        }

        private async Task CommandExecutionFailedAsync(CommandExecutionFailedEventArgs args)
        {
            var ctx = args.Context as SquizzyContext;

            try
            {
                if (args.Result.Exception != null)
                {
                    await _logger.ReportErrorAsync(ctx.Message, args.Result.Exception);
                }

                var message = new LogMessage(LogSeverity.Warning, "CommandHandler", $"Command {ctx.Command.Name} Execution failed in {ctx.Channel.Name} for {ctx.User.Username}", args.Result.Exception);
                await _logger.LogAsync(message);
            }
            finally
            {
                _ressourceAdministration.UnblockCommandId(ctx.CommandId);
            }
        }
    }
}