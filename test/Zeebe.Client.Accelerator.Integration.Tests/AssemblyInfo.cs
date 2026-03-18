using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace Zeebe.Client.Accelerator.Integration.Tests
{
    [CollectionDefinition("Sequential", DisableParallelization = true)]
    public class NoParallelizationCollection { }

    internal class AssemblyInfo
    {
    }
}
