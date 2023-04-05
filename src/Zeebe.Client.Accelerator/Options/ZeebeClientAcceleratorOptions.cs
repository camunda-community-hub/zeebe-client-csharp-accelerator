using System;
using System.ComponentModel;
using Zeebe.Client.Api.Builder;

namespace Zeebe.Client.Accelerator.Options
{
    public class ZeebeClientAcceleratorOptions 
    {
        public virtual ClientOptions Client { get; set; } = new ClientOptions();
        public virtual WorkerOptions Worker { get; set; }

        public class ClientOptions 
        {
            private string _gatewayAddress;
            public virtual string GatewayAddress { 
                get { return GetEnvironmentVariable("ZEEBE_ADDRESS", _gatewayAddress); }
                set { _gatewayAddress = value; } 
            }
            public virtual TransportEncryptionOptions TransportEncryption { get; set; }
            public virtual CloudOptions Cloud { get; set; }
            public virtual long? KeepAliveInMilliSeconds { get; set; }
            public virtual TimeSpan? KeepAlive { get { return KeepAliveInMilliSeconds.HasValue ? TimeSpan.FromMilliseconds(KeepAliveInMilliSeconds.Value) : (TimeSpan?) null; } }
            public virtual Func<int, TimeSpan> RetrySleepDurationProvider { get; set; }

            public class TransportEncryptionOptions 
            {
                public virtual string RootCertificatePath { get; set; }
                public virtual string AccessToken { get; set; }
                public virtual IAccessTokenSupplier AccessTokenSupplier { get; set; }
            }

            public class CloudOptions
            {
                private string _clientId;
                public virtual string ClientId {
                    get { return GetEnvironmentVariable("ZEEBE_CLIENT_ID", _clientId); } 
                    set { _clientId = value; } 
                }
                private string _clientSecret;
                public virtual string ClientSecret {
                    get { return GetEnvironmentVariable("ZEEBE_CLIENT_SECRET", _clientSecret);  }
                    set { _clientSecret = value; }
                }
                private string _authorizationServerUrl = "https://login.cloud.camunda.io/oauth/token";
                public virtual string AuthorizationServerUrl {
                    get { return GetEnvironmentVariable("ZEEBE_AUTHORIZATION_SERVER_URL", _authorizationServerUrl); } 
                    set { _authorizationServerUrl = value; } 
                }
                private string _tokenAudience = "zeebe.camunda.io";
                public virtual string TokenAudience {
                    get { return GetEnvironmentVariable("ZEEBE_TOKEN_AUDIENCE", _tokenAudience); }
                    set { _tokenAudience = value; } 
                }
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

        public static string GetEnvironmentVariable(string name, string defaultValue)
            => Environment.GetEnvironmentVariable(name) is string v && v.Length > 0 ? v : defaultValue;
    }
}
