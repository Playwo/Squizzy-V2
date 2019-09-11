using System;
using System.Threading.Tasks;
using Qmmands;
using Squizzy.Entities;

namespace Squizzy.Commands
{
    public class RequireAccount : SquizzyParameterCheck
    {
        public override ValueTask<CheckResult> CheckAsync(object argument, SquizzyContext context)
        {
            if (!(argument is SquizzyPlayer player))
            {
                throw new InvalidOperationException("RequireAccount Check can only be used on SquizzyPlayer Parameters");
            }

            return player.TotalAnsweredQuestions == 0
                ? CheckResult.Unsuccessful($"{player.Name} has never used Squizzy before!")
                : CheckResult.Successful;
        }
    }
}
