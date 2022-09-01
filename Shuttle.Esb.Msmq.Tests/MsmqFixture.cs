using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shuttle.Esb.Msmq.Tests
{
    public static class MsmqFixture
    {
        public static IServiceCollection GetServiceCollection()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddMsmq(builder =>
            {
                builder.AddOptions("local", new MsmqOptions
                {
                    Path = ".\\private$"
                });
            });

            return services;
        }
    }
}
