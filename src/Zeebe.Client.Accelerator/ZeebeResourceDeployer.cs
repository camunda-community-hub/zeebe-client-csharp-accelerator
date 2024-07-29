using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Zeebe.Client.Accelerator
{
    public class ZeebeResourceDeployer
    {
        readonly IServiceProvider ServiceProvider;

        public ZeebeResourceDeployer(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public ZeebeResourceDeployerWithDirectory UsingDirectory(string directory)
        {
            return new ZeebeResourceDeployerWithDirectory(ServiceProvider, directory);
        }

        public class ZeebeResourceDeployerWithDirectory : ZeebeResourceDeployer
        {
            readonly ILogger<ZeebeResourceDeployer> Logger;
            private string Directory;
            private List<string> Resources = new List<string>();

            public ZeebeResourceDeployerWithDirectory(IServiceProvider serviceProvider, string directory)
                : base(serviceProvider)
            {
                Directory = directory;
                Logger = serviceProvider.GetRequiredService<ILogger<ZeebeResourceDeployer>>();
            }

            public ZeebeResourceDeployerWithDirectory AddResource(string resource)
            {
                Resources.Add(resource);
                return this;
            }

            public async void Deploy()
                => await DeployAsync();

            public async void Deploy(Action callback)
                => await DeployAsync().ContinueWith(t => callback());

            public async void Deploy(Func<Task> asyncCallback)
                => await DeployAsync().ContinueWith(async t => await asyncCallback());

            public async Task DeployAsync()
            {
                if (Resources.Count == 0)
                {
                    // TODO scan directory and add all found bpmn / dmn resources
                    throw new ArgumentException("Illegal call to Deploy() - please add resources first.");
                }

                var serviceScope = ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
                var zeebeClient = serviceScope.ServiceProvider.GetService<IZeebeClient>();
                var cmd = zeebeClient.NewDeployCommand()
                    .AddResourceFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Directory, Resources.First()));
                Resources.RemoveAt(0);
                Resources.ForEach(resource => cmd
                    .AddResourceFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Directory, resource)));
                var deployResponse = await cmd.Send().ConfigureAwait(false);
                deployResponse.Processes.ToList().ForEach(p => Logger.LogInformation("Deployed process {key}", p.BpmnProcessId));
            }
        }
    }
}
