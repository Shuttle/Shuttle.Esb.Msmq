using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMsmq(this IServiceCollection services,
            Action<MsmqBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var msmqBuilder = new MsmqBuilder(services);

            builder?.Invoke(msmqBuilder);

            services.TryAddSingleton<IQueueFactory, MsmqQueueFactory>();

            services.AddOptions<MsmqOptions>().Configure(options =>
            {
                options.LocalQueueTimeout = msmqBuilder.Options.LocalQueueTimeout;
                options.RemoteQueueTimeout = msmqBuilder.Options.RemoteQueueTimeout;
            });

            return services;
        }
    }
}