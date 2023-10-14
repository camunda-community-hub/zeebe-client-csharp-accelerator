using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using Zeebe.Client.Accelerator.Utils;
using System.Text.Json.Serialization;

namespace Zeebe.Client.Accelerator
{
    public class JobHandlerInfoProvider : IJobHandlerInfoProvider
    {
        private static readonly List<Type> GENERIC_HANDLER_TYPES = new List<Type>()
        {
            typeof(IZeebeWorker<>),
            typeof(IAsyncZeebeWorker<>),
            typeof(IZeebeWorker<,>),
            typeof(IAsyncZeebeWorker<,>),
            typeof(IZeebeWorkerWithResult<>),
            typeof(IAsyncZeebeWorkerWithResult<>)
        };
        private readonly Assembly[] assemblies;
        private List<IJobHandlerInfo> jobHandlers;

        public JobHandlerInfoProvider(params Assembly[] assemblies)
        {
            this.assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
        }

        public IEnumerable<IJobHandlerInfo> JobHandlerInfoCollection
        {
            get
            {
                if(jobHandlers != null)
                    return this.jobHandlers;

                this.jobHandlers = GetJobHandlers(this.assemblies).ToList();
                return this.jobHandlers;
            }
        }

        private static IEnumerable<IJobHandlerInfo> GetJobHandlers(Assembly[] assemblies)
        {
            return assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => IsZeebeWorker(t))
                .SelectMany(t => CreateJobHandlerInfo(t));
        }

        private static IEnumerable<IJobHandlerInfo> CreateJobHandlerInfo(Type jobHandlerType)
        {
            return GetJobHandlerMethods(jobHandlerType)
                .Select(m => CreateJobHandlerInfo(jobHandlerType, m));
        }

        private static IEnumerable<MethodInfo> GetJobHandlerMethods(Type jobHandlerType)
        {
            var jobHandlerMethods = jobHandlerType.GetInterfaces()
                .Where(i => IsZeebeWorkerInterface(i))
                .SelectMany(i => i.GetMethods());

            if (jobHandlerMethods.Count() > 1)
            {
                throw new InvalidOperationException(jobHandlerType.Name + " must not have more than one 'HandleJob' method");
            }

            return jobHandlerType.GetMethods()
                .Where(m => IsJobHandlerMethod(m, jobHandlerMethods));
        }

        private static IJobHandlerInfo CreateJobHandlerInfo(Type jobHandlerType, MethodInfo jobHandlerMethod)
        {
            var jobType = jobHandlerMethod.GetParameters()[0].ParameterType;

            return new JobHandlerInfo
            (
                jobHandlerMethod, 
                GetServiceLifetime(jobHandlerMethod),
                GetJobType(jobHandlerType), 
                GetWorkerName(jobHandlerType), 
                GetMaxJobsActive(jobHandlerType), 
                GetHandlerThreads(jobHandlerType),
                GetTimeout(jobHandlerType),
                GetPollInterval(jobHandlerType), 
                GetPollingTimeout(jobHandlerType),
                GetFetchVariables(jobType, jobHandlerType),
                GetAutoComplete(jobHandlerType)
            );
        }

        private static ServiceLifetime GetServiceLifetime(MethodInfo handlerMethod)
        {
            var handler = handlerMethod.ReflectedType;
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

        private static byte? GetHandlerThreads(Type jobType)
        {
            var attr = jobType.GetCustomAttribute<HandlerThreadsAttribute>();
            return attr?.HandlerThreads;
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

        private static string[] GetFetchVariables(Type jobType, Type jobHandlerType)
        {
            var attr = jobHandlerType.GetCustomAttribute<FetchVariablesAttribute>();
            if (attr != null)
                return attr.None ? new string[1] { "" } : StringUtils.ToCamelCase(attr.FetchVariables);

            var fetchVariables = GetFetchVariablesFromJobState(jobType);
            if(fetchVariables != null)
                return fetchVariables;
                
            return null;
        }

        private static bool? GetAutoComplete(Type jobHandlerType)
        {
            var attr = jobHandlerType.GetCustomAttribute<AutoCompleteAttribute>();
            if (attr != null)
            {
                return attr.AutoComplete;
            }
            return null;
        }

        private static string[] GetFetchVariablesFromJobState(Type jobType)
        {
            var jobStateType = GetJobStateType(jobType);
            if(jobStateType == null)
                return null;

            return jobStateType.GetProperties()
                .Where(p => p.CanWrite && (p.GetCustomAttribute<JsonIgnoreAttribute>() == null))
                .Select(p => GetAttributeName(p))
                .ToArray();                
        }

        private static string GetAttributeName(PropertyInfo p)
        {
            var jsonPropertyName = p.GetCustomAttribute<JsonPropertyNameAttribute>();
            if (jsonPropertyName != null && jsonPropertyName.Name != null)
            {
                return jsonPropertyName.Name;
            }
            return StringUtils.ToCamelCase(p.Name);
        }

        private static bool IsZeebeWorker(Type t)
        {
            var interfaces = t.GetInterfaces();
            return
                interfaces.Contains(typeof(IZeebeWorker)) ||
                interfaces.Contains(typeof(IAsyncZeebeWorker))
                || interfaces.Any(i => i.IsGenericType && GENERIC_HANDLER_TYPES.Contains(i.GetGenericTypeDefinition()))
                ;
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
        
        private static bool IsZeebeWorkerInterface(Type i)
        {
            return 
                i.Equals(typeof(IZeebeWorker)) ||
                i.Equals(typeof(IAsyncZeebeWorker)) ||
                i.IsGenericType && GENERIC_HANDLER_TYPES.Any(h => i.GetGenericTypeDefinition().Equals(h));
        }
        private static Type GetJobStateType(Type jobType)
        {
            var zDefinition = typeof(ZeebeJob<>);

            var genericJobType = BaseTypes(jobType)
                .Where(t => t.IsGenericType
                    && (t.GetGenericTypeDefinition().Equals(zDefinition)))
                .SingleOrDefault();

            if (genericJobType == null)
                return null;

            return genericJobType.GetGenericArguments().Single();
        }
        private static IEnumerable<Type> BaseTypes(Type type)
        {
            while (type != null)
            {
                yield return type;
                type = type.BaseType;
            }
        }

    }
}