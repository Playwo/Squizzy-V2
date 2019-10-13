using System.Threading.Tasks;
using Qmmands;

namespace Squizzy.Commands
{
    public class RequireMagnets : SoftCheckAttribute
    {
        public int Minimum { get; }
        public int Maximum { get; }

        public override string Description => "Requires your magnet count to be within a certain range";

        public RequireMagnets(int minimum = int.MinValue, int maximum = int.MaxValue)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        public override ValueTask<CheckResult> CheckAsync(SquizzyContext context)
            => context.Player.Magnets < Minimum
                ? CheckResult.Unsuccessful($"You need at least {Minimum} <:magnet:440898600738750465> to do that!")
                : context.Player.Magnets > Maximum
                    ? CheckResult.Unsuccessful($"You can't do that if you have more than {Maximum} <:magnet:440898600738750465>")
                    : (ValueTask<CheckResult>) CheckResult.Successful;
    }
}
