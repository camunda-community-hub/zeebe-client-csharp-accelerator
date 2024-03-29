﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zeebe.Client.Accelerator.Abstractions;
using Zeebe.Client.Accelerator.Utils;
using Zeebe.Client.Api.Responses;
using Zeebe.Client.Api.Worker;

namespace Zeebe.Client.Accelerator.Extensions
{
    public static class ZeebeClientExtensions
    {
        /// <summary>
        /// Creates a dynamic job worker for retrieving an expected Zeebe message in a defined timespan. Blocks
        /// until the message has been received or the timeout has been reached. The job worker will be closed
        /// afterwards.
        /// </summary>
        /// <param name="jobType">job type</param>
        /// <param name="timeSpan">timeout duration</param>
        /// <returns>true if message has been received, false otherwise</returns>
        public static bool ReceiveMessage(this IZeebeClient zeebeClient, string jobType, TimeSpan timeSpan)
        {
            try
            {
                ReceiveMessage(zeebeClient, jobType, timeSpan, "");
                return true;
            } catch (MessageTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a dynamic job worker for retrieving an expected Zeebe message in a defined timespan. Blocks
        /// until the message has been received or the timeout has been reached. The job worker will be closed
        /// afterwards.
        /// </summary>
        /// <param name="jobType">job type</param>
        /// <param name="timeSpan">timeout duration</param>
        /// <param name="fetchVariables">variables to fetch</param>
        /// <returns>fetched variables as JSON string</returns>
        /// <exception cref="MessageTimeoutException">in case the timout has been reached without receiving the message</exception>
        public static string ReceiveMessage(this IZeebeClient zeebeClient, string jobType, TimeSpan timeSpan, params string[] fetchVariables)
        {
            using (var messageHandle = new BlockingCollection<string>())
            {
                var messageReceiver = zeebeClient.NewWorker().JobType(jobType)
                    .Handler((jobClient, job) => new ZeebeMessageReceiver(messageHandle).ReceiveMessageAsync(jobClient, job))
                    .Timeout(timeSpan)
                    .MaxJobsActive(1)
                    .FetchVariables(fetchVariables)
                    .HandlerThreads(1)
                    .AutoCompletion()
                    .Name(jobType)
                    .Open();

                var matched = messageHandle.TryTake(out string variables, timeSpan);

                messageReceiver.Dispose();

                if (!matched)
                {
                    throw new MessageTimeoutException("No message received for jobType " + jobType + " in " + timeSpan);
                }
                else
                {
                    return variables;
                }
            }

        }

        /// <summary>
        /// Creates a dynamic job worker for retrieving an expected Zeebe message in a defined timespan. Blocks
        /// until the message has been received or the timeout has been reached. The job worker will be closed
        /// afterwards.
        /// </summary>
        /// <param name="jobType">job type</param>
        /// <param name="timeSpan">timeout duration</param>
        /// <typeparam name="T">return type - all attributes of this type will automatically be treated as variables to fetch (CamelCase)</typeparam>
        /// <returns>fetched variables deserialized to the given type</returns>
        /// <exception cref="MessageTimeoutException">in case the timout has been reached without receiving the message</exception>
        public static T ReceiveMessage<T>(this IZeebeClient zeebeClient, string jobType, TimeSpan timeSpan)
        {
            var fetchVariables = typeof(T).GetProperties()
                .Where(p => p.CanWrite)
                .Select(p => StringUtils.ToCamelCase(p.Name))
                .ToArray();
            var result = ReceiveMessage(zeebeClient,jobType, timeSpan, fetchVariables);
            return new ZeebeVariablesDeserializer().Deserialize<T>(result);
        }


        private class ZeebeMessageReceiver
        {

            private BlockingCollection<string> _Signal;

            public ZeebeMessageReceiver(BlockingCollection<string> signal)
            {
                _Signal = signal;
            }

            public async Task ReceiveMessageAsync(IJobClient jobClient, IJob job)
            {
                var variables = job.Variables;
                await jobClient.NewCompleteJobCommand(job).Send();
                _Signal.Add(variables);
            }
        }
    }

    public class MessageTimeoutException : Exception
    {
        public MessageTimeoutException(string message) : base(message) { }
    };

}
