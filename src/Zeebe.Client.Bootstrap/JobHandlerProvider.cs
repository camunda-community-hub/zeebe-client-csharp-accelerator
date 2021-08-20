using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Zeebe.Client.Api.Worker;
using Zeebe.Client.Bootstrap.Abstractions;
using Zeebe.Client.Bootstrap.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Zeebe.Client.Bootstrap
{
    public class JobHandlerProvider : IJobHandlerProvider
    {
        private static Type JOB_HANDLER_TYPE = typeof(IJobHandler<>);        
        private static string JOB_HANDLER_METHOD_NAME = nameof(IJobHandler<AbstractJob>.HandleJob);
        public static Type JOB_HANDLER_METHOD_RETURN_TYPE = typeof(void);
        private static Type ASYNC_JOB_HANDLER_TYPE = typeof(IAsyncJobHandler<>);
        private static string ASYNC_JOB_HANDLER_METHOD_NAME = nameof(IAsyncJobHandler<AbstractJob>.HandleJob);
        public static Type ASYNC_JOB_HANDLER_METHOD_RETURN_TYPE = typeof(Task);
        private readonly IAssemblyProvider assemblyProvider;
        private List<IJobHandlerReference> references;

        public JobHandlerProvider(IAssemblyProvider assemblyProvider)
        {
            this.assemblyProvider = assemblyProvider ?? throw new ArgumentNullException(nameof(assemblyProvider));
        }

        public IEnumerable<IJobHandlerReference> JobHandlers
        {
            get
            {
                if(references != null)
                    return this.references;

                this.references = GetReferences(assemblyProvider);
                return this.references;
            }
        }

        private List<IJobHandlerReference> GetReferences(IAssemblyProvider assemblyProvider)
        {            
            return assemblyProvider
                .Assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => ImplementsGenericType(t, JOB_HANDLER_TYPE) || ImplementsGenericType(t, ASYNC_JOB_HANDLER_TYPE))
                .SelectMany(t => CreateReferences(t))
                .ToList();
        }

        private static IEnumerable<IJobHandlerReference> CreateReferences(Type t)
        {
            return GetJobHandlerMethods(t)
                .Select(m => CreateReference(m));
        }

        private static IJobHandlerReference CreateReference(MethodInfo m)
        {
            var jobType = m.GetParameters()[1].ParameterType;

            return new JobHandlerReference(
                m, 
                GetServiceLifetime(m),
                GetJobType(jobType), 
                GetWorkerName(jobType), 
                GetMaxJobsActive(jobType), 
                GetTimeout(jobType),
                GetPollInterval(jobType), 
                GetPollingTimeout(jobType)
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

        private static IEnumerable<MethodInfo> GetJobHandlerMethods(Type t)
        {
            var jobTypes = t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == JOB_HANDLER_TYPE)
                .Select(i => i.GenericTypeArguments.First());

            var asyncJobTypes = t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == ASYNC_JOB_HANDLER_TYPE)
                .Select(i => i.GenericTypeArguments.First());

            return t.GetMethods()
                .Where(m => 
                    IsJobHandlerMethod(m, JOB_HANDLER_METHOD_NAME, JOB_HANDLER_METHOD_RETURN_TYPE, jobTypes) || 
                    IsJobHandlerMethod(m, ASYNC_JOB_HANDLER_METHOD_NAME, ASYNC_JOB_HANDLER_METHOD_RETURN_TYPE, asyncJobTypes));
        }
        private static bool IsJobHandlerMethod(MethodInfo m, string methodName, Type returnType, IEnumerable<Type> jobTypes)
        {
            var parameters = m.GetParameters().ToArray();

            return m.Name.Equals(methodName) &&
                m.ReturnType.Equals(returnType) &&
                parameters.Length == 3 &&
                parameters[0].ParameterType.Equals(typeof(IJobClient)) &&
                jobTypes.Any(jt =>  jt.Equals(parameters[1].ParameterType));
        }

        public bool ImplementsGenericType(Type type, Type genericType)
        {
            return
                type.IsClass && 
                type.GetInterfaces()
                    .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericType);
        }
    }
}