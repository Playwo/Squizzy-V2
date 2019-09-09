using Discord;

namespace Squizzy.Extensions
{
    public static partial class Extensions
    {
        public static EmbedBuilder WithAppendDescription(this EmbedBuilder builder, string description)
        {
            builder.Description += description;
            return builder;
        }
    }
}
