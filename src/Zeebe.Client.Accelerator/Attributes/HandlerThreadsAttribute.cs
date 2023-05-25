using System;
using Zeebe.Client.Accelerator.Abstractions;

namespace Zeebe.Client.Accelerator.Attributes 
{    
    public class HandlerThreadsAttribute : AbstractWorkerAttribute
    {
        public HandlerThreadsAttribute(byte handlerThreads)
        {
            if (handlerThreads < 1)
            {
                throw new ArgumentException($"'{nameof(handlerThreads)}' cannot be smaller then 1.", nameof(handlerThreads));
            }

            this.HandlerThreads = handlerThreads;
        }

        public byte HandlerThreads { get; }
    }
}