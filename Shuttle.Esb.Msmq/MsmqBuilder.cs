using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.Msmq
{
    public class MsmqBuilder
    {
        public IServiceCollection Services { get; }
        public MsmqSettings Settings { get; set; } = new MsmqSettings();
        public bool SettingsRetrieved { get; private set;  } = false;
        
        public MsmqBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public MsmqBuilder GetSettings(string key = null)
        {
            SettingsRetrieved = true;

            Services.AddOptions<MsmqSettings>().Configure<IConfiguration>((options, configuration) =>
            {
                var settings = configuration.GetRequiredSection(key ?? MsmqSettings.SectionName).Get<MsmqSettings>();

                options.LocalQueueTimeout = settings.LocalQueueTimeout;
                options.RemoteQueueTimeout = settings.RemoteQueueTimeout;
            });

            return this;
        }
    }
}