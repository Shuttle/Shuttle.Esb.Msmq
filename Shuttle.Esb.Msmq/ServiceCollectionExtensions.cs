using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMsmq(this IServiceCollection services,
            Action<MsmqBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var configurationBuilder = new MsmqBuilder(services);

            builder?.Invoke(configurationBuilder);

            services.TryAddSingleton<IQueueFactory, MsmqQueueFactory>();

            if (!configurationBuilder.SettingsRetrieved)
            {
                services.TryAddSingleton(serviceProvider => Options.Create(configurationBuilder.Settings));
            }

            return services;
        }
    }
}