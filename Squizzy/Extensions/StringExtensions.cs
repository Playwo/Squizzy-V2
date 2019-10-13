using System;

namespace Squizzy.Extensions
{
    public partial class Extensions
    {
        public static bool EqualsIgnoreCase(this string str, string otherString) 
            => str is null 
                ? false 
                : str.Equals(otherString, StringComparison.OrdinalIgnoreCase);

        public static string Truncate(this string value, int maxLength)
            => string.IsNullOrEmpty(value)
                ? value
                : value.Length <= maxLength
                    ? value
                    : value.Substring(0, maxLength);
    }
}
