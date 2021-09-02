using System;
using Xunit;
using static Zeebe.Client.Bootstrap.Options.ZeebeClientBootstrapOptions;

namespace Zeebe.Client.Bootstrap.Unit.Tests.Options
{
    public class ClientOptionsTests 
    {
        private readonly long keepAliveInMilliSeconds;

        [Fact]
        public void KeepAliveTimeSpanMatchesKeepAliveInMillisecondsWhenCreated()
        {   
            var actual = Create();

            Assert.Equal(this.keepAliveInMilliSeconds, actual.KeepAlive.Value.TotalMilliseconds);
        }

        public ClientOptionsTests()
        {
            var random = new Random();

            this.keepAliveInMilliSeconds = (long)random.Next(1, int.MaxValue);            
        }

        private ClientOptions Create()
        {
            var options = new ClientOptions
            {
                KeepAliveInMilliSeconds = this.keepAliveInMilliSeconds
            };

            return options;
        }
    }
}