using System;

namespace Squizzy.Services
{
    [Flags]
    public enum RessourceType
    {
        None = 0,
        User = 1,
        Channel = 2,
        Guild = 4,
        Global = 8,
    }
}
