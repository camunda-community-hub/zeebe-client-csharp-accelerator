[![Build Status](https://github.com/arjangeertsema/zeebe-client-csharp-bootstrap/actions/workflows/ci.yml/badge.svg)](https://github.com/arjangeertsema/zeebe-client-csharp-bootstrap/actions/workflows/ci.yml)
[![](https://img.shields.io/nuget/v/zb-client-bootstrap.svg)](https://www.nuget.org/packages/zb-client-bootstrap/) 
[![](https://img.shields.io/nuget/dt/zb-client-bootstrap)](https://www.nuget.org/stats/packages/zb-client-bootstrap?groupby=Version) 
[![](https://img.shields.io/github/license/arjangeertsema/zeebe-client-csharp-bootstrap.svg)](https://www.apache.org/licenses/LICENSE-2.0) 
[![Total alerts](https://img.shields.io/lgtm/alerts/g/arjangeertsema/zeebe-client-csharp-bootstrap.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/zeebe-io/zb-csharp-client-bootstrap/alerts/)
[![](https://img.shields.io/badge/Community%20Extension-An%20open%20source%20community%20maintained%20project-FF4700)](https://github.com/camunda-community-hub/community)

# Boostrap extension for the C# Zeebe client

This project is an extension of the [C# Zeebe client project](https://github.com/camunda-community-hub/zeebe-client-csharp). Zeebe Job handlers are automaticly recognized and boostrapped via a [.Net HostedService](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice).

## Requirements

* .Net 5.0
* latest [C# Zeebe client release](https://www.nuget.org/packages/zb-client/)
* latest [Zeebe release](https://github.com/zeebe-io/zeebe/releases/)

## How to use

The Zeebe C# client boostrap extensions is available via nuget (https://www.nuget.org/packages/zb-client-bootstrap/).

See [examples] for more information.

## Quick start

All classes which implement `IJobHandler<TJob>`, `IJobHandler<TJob, TResponse>`, `IAsyncJobHandler<TJob>` or `IAsyncJobHandler<TJob, TResponse>` are automaticly found and boostrapped when you register this boostrap project with the `IServiceCollection.BoostrapZeebe()` method.

The `BoostrapZeebe` method has two parameters:

1. `ZeebeBootstrapOptions` via [Configuration, Action delegate or both](https://docs.microsoft.com/en-us/dotnet/core/extensions/options-library-authors).
1. An array with assembly filters, only assemblies which start with one of the filters will be scanned for job handlers.

```csharp
ConfigureServices((hostContext, services) => {
    services.BootstrapZeebe(
        hostContext.Configuration.GetSection("ZeebeBootstrap"),
        "SimpleAsyncExample"
    );
})
```

### Job

The job is an implementation of `AbstractJob`. A job can be configured via optional [attributes](). By default the simple name of the job is mapped to BPMN task job type. Job types must be unique.

```csharp
public class SimpleJob : AbstractJob
{
    public SimpleJob(IJob job) 
        : base(job)
    { 
        //Variable mapping logic can be added here.
    }
}
```

### Job handler

The job handler is an implementation of `IJobHandler<TJob>`, `IJobHandler<TJob, TResponse>`, `IAsyncJobHandler<TJob>` or `IAsyncJobHandler<TJob, TResponse>`. A jobhandler can be configured via optional [attributes]. Job handlers are automaticly added to the DI container, therefore you can use dependency injection inside the job handlers. 


```csharp
public class SimpleJobHandler : IAsyncJobHandler<SimpleJob>
{
    public async Task HandleJob(SimpleJob job, CancellationToken cancellationToken)
    {  
        await Usecase.ExecuteAsync();
    }
}
```

A handled job has three outcomes:

1. The job has been handled without exceptions: this will automaticly result in a `JobCompletedCommand` beeing send to the broker. The `TResponse` is automaticly serialized and added to the `JobCompletedCommand`.
1. An exception has been thrown while handling the job and the exception implements `AbstractJobException`: this wil automaticly result in a `ThrowErrorCommand` beeing send to the broker;
1. Any other exception will automaticly result in a `FailCommand` beeing send to the broker;

## Conventions

This project uses the following conventions:

1. By default the simple name of the `AbstractJob` implementation is used to match the `Type` which is specified in the BPMN model. This can be overriden by adding the `JobTypeAttribute` to the `AbstractJob` implementation, see [attributes] for more information.
1. By default the assembly name which contains the job handler is used as the `Worker name`. This can be overriden by adding the `WorkerNameAttribute` to the `AbstractJob` implementation, see [attributes] for more information.
1. By default the job handlers are added to de DI container with a `Transient` service lifetime. This can be overriden by adding the `ServiceLifetimeAttribute` to the job handler, see [attributes] for more information.
1. By default the `ZeebeVariablesSerializer` is registered as the implementation for `IZeebeVariablesSerializer` which uses `System.Text.Json.JsonSerializer`. You can override this registration by registering your service after the `BootstrapZeebe` method or you can register `System.Text.Json.JsonSerializerOptions` to configure the `System.Text.Json.JsonSerializer`. 

## How to build

Run `dotnet build Zeebe.Client.Bootstrap.sln`

## How to test

Run `dotnet test Zeebe.Client.Bootstrap.sln`

[examples]:  https://github.com/arjangeertsema/zeebe-client-csharp-bootstrap/tree/main/examples
[attributes]: https://github.com/arjangeertsema/zeebe-client-csharp-bootstrap/tree/main/src/Zeebe.Client.Bootstrap/Attributes