using Microsoft.Extensions.Options;

namespace Shuttle.Esb.Msmq
{
    public class MsmqOptionsValidator : IValidateOptions<MsmqOptions>
    {
        public ValidateOptionsResult Validate(string name, MsmqOptions options)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return ValidateOptionsResult.Fail(Esb.Resources.QueueConfigurationNameException);
            }

            if (string.IsNullOrWhiteSpace(options.Path))
            {
                return ValidateOptionsResult.Fail(string.Format(Esb.Resources.QueueConfigurationItemException, name, nameof(options.Path)));
            }

            return ValidateOptionsResult.Success;
        }
    }
}