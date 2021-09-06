using System;

namespace Zeebe.Client.Bootstrap.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]    
    public abstract class AbstractJobHandlerAttribute : Attribute 
    { }    
}