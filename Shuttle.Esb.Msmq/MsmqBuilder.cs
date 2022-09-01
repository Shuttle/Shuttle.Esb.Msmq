using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public class MsmqBuilder
    {
        internal readonly Dictionary<string, MsmqOptions> MsmqOptions = new Dictionary<string, MsmqOptions>();
        public IServiceCollection Services { get; }
        
        public MsmqBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public MsmqBuilder AddOptions(string name, MsmqOptions amazonSqsOptions)
        {
            Guard.AgainstNullOrEmptyString(name, nameof(name));
            Guard.AgainstNull(amazonSqsOptions, nameof(amazonSqsOptions));

            MsmqOptions.Remove(name);

            MsmqOptions.Add(name, amazonSqsOptions);

            return this;
        }
    }
}