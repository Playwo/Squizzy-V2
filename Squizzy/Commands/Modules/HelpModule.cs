using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Squizzy.Extensions;
using Squizzy.Services;

namespace Squizzy.Commands
{
    [Name("Help :question:")]
    public class HelpModule : SquizzyModule
    {
        public CommandService Command { get; set; }

        [Command("CommandHelp", "CommandInfo", "CmdInfo", "Help", "CmdHelp","CInfo" ,"CHelp", "HelpMe", "HowDoesThisWork")]
        [Description("Get help about a certain command")]
        [Priority(1)]
        public Task SendCommandHelpAsync([Name("Command")]Command command) 
            => ReplyAsync(embed: command.GetHelpEmbed());

        [Command("Help", "HelpMe", "HowDoesThisWork")]
        [Description("Get the entire help page")]
        public async Task SendHelpAsync()
        {
            var helpBuilder = new EmbedBuilder();

            foreach(var module in Command.GetAllModules())
            {
                if (!await module.RunHardChecksAsync(Context))
                {
                    continue;
                }

                var runnableCommands = new List<Command>();

                foreach(var command in module.Commands)
                {
                    if (!await command.RunHardChecksAsync(Context))
                    {
                        continue;
                    }

                    runnableCommands.Add(command);
                }

                if (runnableCommands.Count == 0)
                {
                    continue;
                }

                var cmdHelpBuilder = new StringBuilder();
                foreach(var command in runnableCommands)
                {
                    cmdHelpBuilder.AppendLine(command.GetHelp());
                }

                helpBuilder.AddField($"{module.Name}",
                                     $"{cmdHelpBuilder}");
            }

            var helpEmbed = helpBuilder
                .WithTitle("Help Page")
                .WithColor(EmbedColor.Help)
                .Build();

            await ReplyAsync(embed: helpEmbed);
        }

    }
}
