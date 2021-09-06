using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Bootstrap.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Bootstrap
{
    public class JobHandlerInfoProvider : IJobHandlerInfoProvider
    {
        private static readonly List<Type> HANDLER_TYPES = new List<Type>()
        {
            typeof(IJobHandler<>),
            typeof(IJobHandler<,>),
            typeof(IAsyncJobHandler<>),
            typeof(IAsyncJobHandler<,>)
        };
        private readonly IAssemblyProvider assemblyProvider;
        private List<IJobHandlerInfo> jobHandlers;

        public JobHandlerInfoProvider(IAssemblyProvider assemblyProvider)
        {
            this.assemblyProvider = assemblyProvider ?? throw new ArgumentNullException(nameof(assemblyProvider));
        }

        public IEnumerable<IJobHandlerInfo> JobHandlerInfoCollection
        {
            get
            {
                if(jobHandlers != null)
                    return this.jobHandlers;

                this.jobHandlers = GetJobHandlers(assemblyProvider).ToList();
                return this.jobHandlers;
            }
        }

        private static IEnumerable<IJobHandlerInfo> GetJobHandlers(IAssemblyProvider assemblyProvider)
        {
            return assemblyProvider
                .Assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => ImplementsJobHandlerInterface(t))
                .SelectMany(t => CreateJobHandlerInfo(t));
        }

        private static IEnumerable<IJobHandlerInfo> CreateJobHandlerInfo(Type jobHandlerType)
        {
            return GetJobHandlerMethods(jobHandlerType)
                .Select(m => CreateJobHandlerInfo(m));
        }

        private static IEnumerable<MethodInfo> GetJobHandlerMethods(Type jobHandlerType)
        {
            var jobHandlerMethods = jobHandlerType.GetInterfaces()
                .Where(i => IsJobHandlerInterface(i))
                .SelectMany(i => i.GetMethods());

            return jobHandlerType.GetMethods()
                .Where(m => IsJobHandlerMethod(m, jobHandlerMethods));
        }

        private static IJobHandlerInfo CreateJobHandlerInfo(MethodInfo jobHandlerMethod)
        {
            var jobType = jobHandlerMethod.GetParameters()[0].ParameterType;

            return new JobHandlerInfo
            (
                jobHandlerMethod, 
                GetServiceLifetime(jobHandlerMethod),
                GetJobType(jobType), 
                GetWorkerName(jobType), 
                GetMaxJobsActive(jobType), 
                GetTimeout(jobType),
                GetPollInterval(jobType), 
                GetPollingTimeout(jobType),
                GetFetchVariables(jobType)
            );
        }

        private static ServiceLifetime GetServiceLifetime(MethodInfo handlerMethod)
        {
            var handler = handlerMethod.DeclaringType;
            var attr = handler.GetCustomAttribute<ServiceLifetimeAttribute>();
            if(attr == null)
                return ServiceLifetime.Transient;

            return attr.ServiceLifetime;
        }

        private static string GetJobType(Type jobType)
        {
            var attr = jobType.GetCustomAttribute<JobTypeAttribute>();
            var name = attr?.JobType;
            if(!String.IsNullOrEmpty(name))
                return name;

            return jobType.Name;
        }

        private static string GetWorkerName(Type jobType)
        {
            var attr = jobType.GetCustomAttribute<WorkerNameAttribute>();
            var name = attr?.WorkerName;
            if(!String.IsNullOrEmpty(name))
                return name;

            return jobType.Assembly.GetName().Name;
        }

        private static int? GetMaxJobsActive(Type jobType)
        {
            var attr = jobType.GetCustomAttribute<MaxJobsActiveAttribute>();
            return attr?.MaxJobsActive;
        }

        private static TimeSpan? GetTimeout(Type jobType)
        {
            var attr = jobType.GetCustomAttribute<TimeoutAttribute>();
            return attr?.Timeout;
        }

        private static TimeSpan? GetPollingTimeout(Type jobType)
        {
            var attr = jobType.GetCustomAttribute<PollingTimeoutAttribute>();
            return attr?.PollingTimeout;
        }

        private static TimeSpan? GetPollInterval(Type jobType)
        {
            var attr = jobType.GetCustomAttribute<PollIntervalAttribute>();
            return attr?.PollInterval;
        }

        private static string[] GetFetchVariables(Type jobType)
        {
            var attr = jobType.GetCustomAttribute<FetchVariablesAttribute>();
            return attr?.FetchVariables;
        }

        private static bool ImplementsJobHandlerInterface(Type type)
        {
            return HANDLER_TYPES.Any(h => ImplementsGenericType(type, h));
        }

        private static bool IsJobHandlerMethod(MethodInfo method, IEnumerable<MethodInfo> jobHandlerMethods)
        {
            var methodParameters = method.GetParameters()
                .Select(p => p.ParameterType)
                .ToList();

            return jobHandlerMethods.Any(h => 
                h.Name.Equals(method.Name) &&
                h.GetParameters().Select(p => p.ParameterType).SequenceEqual(methodParameters) &&
                h.ReturnParameter.ParameterType.Equals(method.ReturnParameter.ParameterType)
            );
        }

        private static bool ImplementsGenericType(Type type, Type genericType)
        {
            return
                type.IsClass && 
                type.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType);
        }
        
        private static bool IsJobHandlerInterface(Type i)
        {
            return i.IsGenericType && 
                HANDLER_TYPES.Any(h => i.GetGenericTypeDefinition().Equals(h));
        }
    }
}