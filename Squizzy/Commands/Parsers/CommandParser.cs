using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Extensions;

namespace Squizzy.Commands
{
    public class CommandParser : SquizzyParser<Command>
    {
        public override ValueTask<TypeParserResult<Command>> ParseAsync(Parameter parameter, string value, SquizzyContext context)
        {
            var _commands = context.ServiceProvider.GetService<CommandService>();

            var command = _commands.FindCommands(value).FirstOrDefault()?.Command;

            return command == null
                ? TypeParserResult<Command>.Unsuccessful("Could not find a command matching your input!")
                : TypeParserResult<Command>.Successful(command);
        } 
    }
}
