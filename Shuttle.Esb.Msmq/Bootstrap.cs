using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public class Bootstrap : IComponentRegistryBootstrap
    {
        public void Register(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, "registry");

            if (!registry.IsRegistered<IMsmqConfiguration>())
            {
                registry.Register<IMsmqConfiguration, MsmqConfiguration>();
            }
        }
    }
}