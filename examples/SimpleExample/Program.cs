using Microsoft.Extensions.Hosting;
using Zeebe.Client.Bootstrap.Extensions;

namespace SimpleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args)
                .Build()
                    .Run();
        }

        static IHostBuilder CreateHostBuilder(string[] args) =>
            Host
                .CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) => {
                        services.BootstrapZeebe(
                            hostContext.Configuration.GetSection("ZeebeBootstrap"),
                            typeof(Program).Assembly
                        );
                    });
    }
}
