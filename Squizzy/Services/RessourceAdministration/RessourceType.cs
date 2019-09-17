using System;

namespace Squizzy.Services
{
    [Flags]
    public enum RessourceType
    {
        User = 1,
        Channel = 2,
        Guild = 4,
    }
}
