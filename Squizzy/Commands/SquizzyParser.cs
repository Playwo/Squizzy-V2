using System.Threading.Tasks;
using Qmmands;

namespace Squizzy.Commands
{
    public abstract class SquizzyParser<T> : TypeParser<T>
    {
        public abstract ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, SquizzyContext context);

        public override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string value, CommandContext context)
            => ParseAsync(parameter, value, context as SquizzyContext);
    }
}
