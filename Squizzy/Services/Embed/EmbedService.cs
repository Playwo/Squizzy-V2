using System;
using System.Text;
using Discord;
using Discord.WebSocket;
using Qmmands;
using Squizzy.Entities;
using Squizzy.Extensions;

namespace Squizzy.Services
{
    public class EmbedService : SquizzyService
    {
        public Embed MakeQuestionResultEmbed(QuestionResult oldResult, QuestionResult newResult,
                                             int oldTrophies,          int newTrophies, int magnets)
        {
            var builder = new EmbedBuilder
            {
                Title = newResult.Correct
                    ? "Correct! :D"
                    : "Wrong! :(",
                Description = (newResult.Correct
                    ? newResult.Perfect
                        ? $"You answered in {Math.Round(newResult.Time.TotalSeconds, 3)} seconds.\n" +
                          $"That means this question is now perfect. :crown:\n"
                        : $"You answered in {Math.Round(newResult.Time.TotalSeconds, 3)} seconds.\n"
                    : "You provided the wrong answer or cancelled! :x:\n")  +
                    (magnets > 0
                        ? $"=> You received {magnets} <:magnet:440898600738750465>\n"
                        : $"=> You lost {-magnets} <:magnet:440898600738750465>\n"),
                Color = newResult.Correct
                    ? newTrophies > oldTrophies
                        ? newResult.Perfect && !oldResult.Perfect
                            ? Color.Gold
                            : Color.Green
                        : Color.Orange
                    : Color.Red
            };

            if (oldResult == null || oldTrophies == 0)
            {
                return newResult.Correct
                    ? builder
                        .WithAppendDescription($"=> You won {newTrophies} :trophy:")
                        .Build()
                    : builder
                        .WithAppendDescription("=> You get no :trophy:")
                        .Build();
            }
            else
            {
                if (oldResult.Correct && !newResult.Correct)
                {
                    return builder
                        .WithAppendDescription($"=> You lose {oldTrophies} :trophy:")
                        .Build();
                }
                if (oldResult.Time <= newResult.Time)
                {
                    return builder
                        .WithAppendDescription("The answer is correct but you have answered this question slower than before!\n" +
                        "=> You get no :trophy: but you don't lose any either")
                        .Build();
                }
                if (oldResult.Time > newResult.Time && oldTrophies == newTrophies)
                {
                    return newResult.Perfect
                        ? builder
                            .WithAppendDescription($"Your answer is {(oldResult.Time - newResult.Time).Milliseconds} ms faster than your last best time!\n" +
                            $"This question is already perfect so you can't get any more :trophy:" +
                            "=> You get no :trophy: but you don't lose any either")
                            .Build()
                        : builder
                            .WithAppendDescription($"Your answer is {(oldResult.Time - newResult.Time).Milliseconds} ms faster than your last best time!\n" +
                            $"To earn more :trophy: you need to be even faster\n" +
                            "=> You get no :trophy: but you don't lose any either")
                            .Build();
                }
                if (oldResult.Time > newResult.Time && oldTrophies < newTrophies)
                {
                    return builder
                        .WithAppendDescription($"Your answer is {(oldResult.Time - newResult.Time).Milliseconds} ms faster than your last best time!\n" +
                        $"=> You won {newTrophies - oldTrophies} :trophy:")
                        .Build();
                }

                throw new Exception("Invalid QuestionResult State!");
            }
        }

        public Embed GetFailedResultEmbed(FailedResult result)
        {
            var description = new StringBuilder();

            switch (result)
            {
                case ChecksFailedResult checksResult:
                    description.AppendLine("__The following check(s) failed:__");
                    foreach (var check in checksResult.FailedChecks)
                    {
                        description.AppendLine($"- `{check.Result.Reason}`");
                    }
                    break;
                case ParameterChecksFailedResult checksResult:
                    description.AppendLine("__The following check(s) failed:__");
                    foreach (var check in checksResult.FailedChecks)
                    {
                        description.AppendLine($"- `{check.Result.Reason}`");
                    }
                    break;
                case ArgumentParseFailedResult argResult:
                    description.AppendLine("__**The following parameters are missing:**__");
                    description.AppendLine($"`{argResult.Reason}`");
                    break;
                case OverloadsFailedResult overloadsResult:
                    description.AppendLine("__**The syntax was wrong**__");
                    description.AppendLine($"`{overloadsResult.Reason}`");
                    break;
                case TypeParseFailedResult parseResult:
                    description.AppendLine("__**The following parameter couldn't be parsed**__");
                    description.AppendLine($"`[{parseResult.Parameter.Name}] - {parseResult.Reason}`");
                        break;
            }

            return new EmbedBuilder()
             .WithColor(EmbedColor.Failed)
             .WithTitle("**Something went wrong!**")
             .WithDescription(description.ToString())
             .Build();
        }

        public Embed GetDisabledDueToMaintenanceEmbed()
            => new EmbedBuilder()
                    .WithColor(EmbedColor.Failed)
                    .WithTitle("**Something went wrong!**")
                    .WithDescription("I am currently in maintenance mode!\n" +
                                     "Go get a coffee, I'll be back in a moment :)")
                    .Build();

        public Embed GetExceptionEmbed(SocketMessage msg, Exception ex)
            => new EmbedBuilder()
                    .WithColor(EmbedColor.Error)
                    .WithTitle("ERROR, FIX ME NOW!")
                    .WithDescription($"Stacktrace:\n{ex.Message}\n{ex.StackTrace}".Substring(0, 2000))
                    .AddField("Additional Infos:", $"Message: {msg.Content}\nUser: {msg.Author}\nChannel: {msg.Channel}")
                    .Build();

        public Embed GetSorryEmbed()
            => new EmbedBuilder()
                    .WithColor(EmbedColor.Error)
                    .WithTitle("An error occured!")
                    .WithDescription("Please don't repeat whatever you did.\n" +
                                     "The error has already been reported and will most likely be fixed soon :)")
                    .Build();
    }
}
