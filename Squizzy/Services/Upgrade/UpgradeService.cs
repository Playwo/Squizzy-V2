using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Squizzy.Entities;

namespace Squizzy.Services
{
    public class UpgradeService : SquizzyService
    {
        public IEnumerable<IUpgrade> GetAllUpgrades()
        {
            var upgradeTypes = Assembly.GetEntryAssembly().GetTypes()
                .Where(x => x.GetInterfaces().Any(z => z == typeof(IUpgrade)) && !x.IsAbstract && !x.IsInterface)
                .ToList();

            foreach (var upgradeType in upgradeTypes)
            {
                yield return Activator.CreateInstance(upgradeType) as IUpgrade;
            }
        }

        public IEnumerable<MagnetUpgrade> GetAllMagnetUpgrades()
        {
            var magnetUpgradeTypes = Assembly.GetEntryAssembly().GetTypes()
                .Where(x => x.BaseType == typeof(MagnetUpgrade) && !x.IsAbstract && !x.IsInterface)
                .ToList();

            foreach(var magnetUpgradeType in magnetUpgradeTypes)
            {
                yield return Activator.CreateInstance(magnetUpgradeType) as MagnetUpgrade;
            }
        }
    }
}
