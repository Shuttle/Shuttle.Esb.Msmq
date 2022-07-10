using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public class MsmqBuilder
    {
        public IServiceCollection Services { get; }
        public MsmqOptions Options { get; set; } = new MsmqOptions();
        
        public MsmqBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }
    }
}