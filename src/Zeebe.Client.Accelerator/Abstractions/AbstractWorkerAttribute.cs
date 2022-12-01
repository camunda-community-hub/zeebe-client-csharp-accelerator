using System;

namespace Zeebe.Client.Accelerator.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]    
    public abstract class AbstractWorkerAttribute : Attribute 
    { }    
}