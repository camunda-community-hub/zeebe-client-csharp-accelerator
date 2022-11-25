using System;

namespace Zeebe.Client.Accelerator.Abstractions
{
    public abstract class BpmnErrorException : Exception
    {
        public BpmnErrorException(string code, string message)
            : base(message)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException($"'{nameof(code)}' cannot be null or whitespace.", nameof(code));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException($"'{nameof(message)}' cannot be null or whitespace.", nameof(message));
            }

            this.Code = code;
        }

        public string Code { get; }
    }
}