using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Zeebe.Client.Accelerator
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
                .Where(t => ImplementsJobHandlerInterface(t))
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
                .Where(i => IsJobHandlerInterface(i))
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
                GetTimeout(jobHandlerType),
                GetPollInterval(jobHandlerType), 
                GetPollingTimeout(jobHandlerType),
                GetFetchVariables(jobType, jobHandlerType)
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

        private static string[] GetFetchVariables(Type jobType, Type jobHandlerType)
        {
            var attr = jobHandlerType.GetCustomAttribute<FetchVariablesAttribute>();
            if (attr != null)
                return attr.None ? new string[1] { "" } : attr.FetchVariables;
            attr = jobType.GetCustomAttribute<FetchVariablesAttribute>();
            if (attr != null)
                return attr.None ? new string[1] { "" } : attr.FetchVariables;

            var fetchVariables = GetFetchVariablesFromJobState(jobType);
            if(fetchVariables != null)
                return fetchVariables;
                
            return null;
        }

        private static string[] GetFetchVariablesFromJobState(Type jobType)
        {
            var jobStateType = GetJobStateType(jobType);
            if(jobStateType == null)
                return null;

            return jobStateType.GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => ToCamelCase(p.Name))
                .ToArray();                
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
        private static Type GetJobStateType(Type jobType)
        {
            var definition = typeof(AbstractJob<>);
            var zDefinition = typeof(ZeebeJob<>);

            var genericJobType = BaseTypes(jobType)
                .Where(t => t.IsGenericType
                    && (t.GetGenericTypeDefinition().Equals(zDefinition)))
                .SingleOrDefault();

            if(genericJobType == null)
            {
                genericJobType = BaseTypes(jobType)
                .Where(t => t.IsAbstract
                    && t.IsGenericType
                    && (t.GetGenericTypeDefinition().Equals(definition)))
                .SingleOrDefault();
            }
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

        private static string ToCamelCase(string str)
        {
            var words = str.Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);

            var leadWord = Regex.Replace(words[0], @"([A-Z])([A-Z]+|[a-z0-9]+)($|[A-Z]\w*)",
                m =>
                {
                    return m.Groups[1].Value.ToLower() + m.Groups[2].Value.ToLower() + m.Groups[3].Value;
                });

            var tailWords = words.Skip(1)
                .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                .ToArray();

            return $"{leadWord}{string.Join(string.Empty, tailWords)}";
        }
    }
}