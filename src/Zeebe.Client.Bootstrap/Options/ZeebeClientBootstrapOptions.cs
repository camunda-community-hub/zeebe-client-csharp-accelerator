using System;
using Zeebe.Client.Api.Builder;

namespace Zeebe.Client.Bootstrap.Options
{
    public class ZeebeClientBootstrapOptions 
    {
        public virtual ClientOptions Client { get; set; }
        public virtual WorkerOptions Worker { get; set; }

        public class ClientOptions 
        {
            public virtual string GatewayAddress { get; set; }
            public virtual TransportEncryptionOptions TransportEncryption { get; set; }        
            public virtual long? KeepAliveInMilliSeconds { get; set; }
            public virtual TimeSpan? KeepAlive { get { return KeepAliveInMilliSeconds.HasValue ? TimeSpan.FromMilliseconds(KeepAliveInMilliSeconds.Value) : null; } }
            public virtual Func<int, TimeSpan> RetrySleepDurationProvider { get; set; }

            public class TransportEncryptionOptions 
            {
                public virtual string RootCertificatePath { get; set; }
                public virtual string AccessToken { get; set; }
                public virtual IAccessTokenSupplier AccessTokenSupplier { get; set; }
            }
        }

        public class WorkerOptions
        {
            public virtual int MaxJobsActive { get; set; }
            public virtual long TimeoutInMilliseconds { get; set; }
            public virtual TimeSpan Timeout { get { return TimeSpan.FromMilliseconds(TimeoutInMilliseconds); } }
            public virtual long PollIntervalInMilliseconds { get; set; }
            public virtual TimeSpan PollInterval { get { return TimeSpan.FromMilliseconds(PollIntervalInMilliseconds); } }
            public virtual long PollingTimeoutInMilliseconds { get; set; }
            public virtual TimeSpan PollingTimeout { get { return TimeSpan.FromMilliseconds(PollingTimeoutInMilliseconds); } }
            public virtual long RetryTimeoutInMilliseconds { get; set; }
            public virtual TimeSpan RetryTimeout { get { return TimeSpan.FromMilliseconds(RetryTimeoutInMilliseconds); } }
            public virtual string Name { get; set; }
        }
    }
}
