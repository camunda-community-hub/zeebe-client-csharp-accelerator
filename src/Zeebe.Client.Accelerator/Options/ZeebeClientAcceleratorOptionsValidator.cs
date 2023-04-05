using System;
using System.Collections.Generic;
using System.Linq;
using static Zeebe.Client.Accelerator.Options.ZeebeClientAcceleratorOptions;

namespace Zeebe.Client.Accelerator.Options
{
    public class ZeebeClientAcceleratorOptionsValidator
    {
        public void Validate(ZeebeClientAcceleratorOptions options)
        {
            var exceptions = ValidateClientOptions(options?.Client).Concat(ValidateWorkerOptions(options?.Worker));

            if(exceptions.Count() == 0)
                return;

            throw new AggregateException("ZeebeClientBootstrapOptions is not valid", exceptions);
        }

        private static IEnumerable<Exception> ValidateClientOptions(ClientOptions options)
        {
            if(options == null)
            {
                yield return new ArgumentNullException(nameof(ZeebeClientAcceleratorOptions.Client));
                yield break;
            }
            if (options.Cloud != null)
            {
                if (String.IsNullOrWhiteSpace(options.Cloud.ClientId))
                    yield return new ArgumentException($"'{nameof(options.Cloud)}{nameof(options.Cloud.ClientId)}' must not be empty.", $"{nameof(ZeebeClientAcceleratorOptions.Client)}.{nameof(options.Cloud)}.{nameof(options.Cloud.ClientId)}");
                if (String.IsNullOrWhiteSpace(options.Cloud.ClientSecret))
                    yield return new ArgumentException($"'{nameof(options.Cloud)}.{nameof(options.Cloud.ClientSecret)}' must not be empty.", $"{nameof(ZeebeClientAcceleratorOptions.Client)}.{nameof(options.Cloud)}.{nameof(options.Cloud.ClientSecret)}");
            }
        }

        private static IEnumerable<Exception> ValidateWorkerOptions(WorkerOptions options)
        {
            if(options == null)
            {
                yield return new ArgumentNullException(nameof(ZeebeClientAcceleratorOptions.Worker));
                yield break;
            }

            if (options.MaxJobsActive < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientAcceleratorOptions.Worker)}.{nameof(options.MaxJobsActive)}");
            if (options.Timeout.TotalMilliseconds < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientAcceleratorOptions.Worker)}.{nameof(options.Timeout)}");
            if (options.PollInterval.TotalMilliseconds < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientAcceleratorOptions.Worker)}.{nameof(options.PollInterval)}");
            if (options.PollingTimeout.TotalMilliseconds < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientAcceleratorOptions.Worker)}.{nameof(options.PollingTimeout)}");
            if (options.RetryTimeout.TotalMilliseconds < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientAcceleratorOptions.Worker)}.{nameof(options.RetryTimeout)}");
            if (String.IsNullOrWhiteSpace(options.Name) && options.Name != null)
                yield return new ArgumentException($"'{nameof(options.Name)}' cannot be empty or whitespace.", $"{nameof(ZeebeClientAcceleratorOptions.Worker)}.{nameof(options.Name)}");
        }
    }
}