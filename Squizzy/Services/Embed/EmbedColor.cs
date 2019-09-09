﻿using Discord;
using sys = System.Drawing;

namespace Squizzy.Services
{
    public static class EmbedColor
    {
        public static Color Success => (Color) sys.Color.Green;
        public static Color Failed => (Color) sys.Color.Red;
        public static Color Statistic => (Color) sys.Color.Blue;
        public static Color Question => (Color) sys.Color.Orange;
        public static Color Error => (Color) sys.Color.DarkRed;
        public static Color Leaderboard => (Color) sys.Color.Gold;
    }
}
