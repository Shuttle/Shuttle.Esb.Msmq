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

            var msmqBuilder = new MsmqBuilder(services);

            builder?.Invoke(msmqBuilder);

            services.TryAddSingleton<IQueueFactory, MsmqQueueFactory>();

            services.AddSingleton<IValidateOptions<MsmqOptions>, MsmqOptionsValidator>();

            foreach (var pair in msmqBuilder.MsmqOptions)
            {
                services.AddOptions<MsmqOptions>(pair.Key).Configure(options =>
                {
                    options.Path = pair.Value.Path;
                    options.Timeout = pair.Value.Timeout;
                    options.UseDeadLetterQueue = pair.Value.UseDeadLetterQueue;
                });
            }

            return services;
        }
    }
}