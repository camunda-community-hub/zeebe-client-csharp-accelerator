using System;
using System.Collections.Generic;
using System.Linq;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Options
{
    public class ZeebeClientBootstrapOptionsValidator
    {
        public void Validate(ZeebeClientBootstrapOptions options)
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
                yield return new ArgumentNullException(nameof(ZeebeClientBootstrapOptions.Client));
                yield break;
            }
        }

        private static IEnumerable<Exception> ValidateWorkerOptions(WorkerOptions options)
        {
            if(options == null)
            {
                yield return new ArgumentNullException(nameof(ZeebeClientBootstrapOptions.Worker));
                yield break;
            }

            if (options.MaxJobsActive < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientBootstrapOptions.Worker)}.{nameof(options.MaxJobsActive)}");
            if (options.Timeout.TotalMilliseconds < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientBootstrapOptions.Worker)}.{nameof(options.Timeout)}");
            if (options.PollInterval.TotalMilliseconds < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientBootstrapOptions.Worker)}.{nameof(options.PollInterval)}");
            if (options.PollingTimeout.TotalMilliseconds < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientBootstrapOptions.Worker)}.{nameof(options.PollingTimeout)}");
            if (options.RetryTimeout.TotalMilliseconds < 1)
                yield return new ArgumentOutOfRangeException($"{nameof(ZeebeClientBootstrapOptions.Worker)}.{nameof(options.RetryTimeout)}");
            if (String.IsNullOrWhiteSpace(options.Name) && options.Name != null)
                yield return new ArgumentException($"'{nameof(options.Name)}' cannot be empty or whitespace.", $"{nameof(ZeebeClientBootstrapOptions.Worker)}.{nameof(options.Name)}");
        }
    }
}