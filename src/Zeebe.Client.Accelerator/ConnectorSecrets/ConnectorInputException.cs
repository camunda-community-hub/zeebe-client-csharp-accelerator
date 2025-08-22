using System;

namespace Zeebe.Client.Accelerator.ConnectorSecrets;

public class ConnectorInputException : Exception
{
    public ConnectorInputException(string message) : base(message) { }
    public ConnectorInputException(string message, Exception innerException) : base(message, innerException) { }
}