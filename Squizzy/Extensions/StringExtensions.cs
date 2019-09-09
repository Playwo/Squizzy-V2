using System;

namespace Squizzy.Extensions
{
    public partial class Extensions
    {
        public static bool EqualsIgnoreCase(this string str, string otherString) => str is null ? false : str.Equals(otherString, StringComparison.OrdinalIgnoreCase);
    }
}
