using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Squizzy.Services
{
    public abstract class SquizzyService
    {
        public virtual Task InitializeAsync() => Task.CompletedTask;
        public void InjectServices(IServiceProvider provider)
        {
            var members = GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes(typeof(Inject), true).Length > 0)
                .ToArray();

            foreach (var member in members)
            {
                object service = provider.GetService(member.FieldType);

                if (service == null)
                {
                    continue;
                }

                member.SetValue(this, service);
            }
        }
    }
}
