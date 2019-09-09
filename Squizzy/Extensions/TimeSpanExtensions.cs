using System;

namespace Squizzy.Extensions
{
    public partial class Extensions
    {
        public static string ToTimeString(this TimeSpan timeSpan)
            => timeSpan.Days > 0
                ? $"{timeSpan.Days} days, {timeSpan.Hours} hours"
                : timeSpan.Hours > 0
                    ? $"{timeSpan.Hours} hours, {timeSpan.Minutes} minutes"
                    : timeSpan.Minutes > 0
                        ? $"{timeSpan.Minutes} minutes, {timeSpan.Seconds} seconds"
                        : $"{timeSpan.Seconds} seconds";
    }
}
