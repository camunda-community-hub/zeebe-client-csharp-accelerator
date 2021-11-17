[![BUILD](https://github.com/arjangeertsema/zeebe-client-csharp-bootstrap/actions/workflows/build.yml/badge.svg)](https://github.com/arjangeertsema/zeebe-client-csharp-bootstrap/actions/workflows/build.yml)
[![ANALYZE](https://github.com/arjangeertsema/zeebe-client-csharp-bootstrap/actions/workflows/analyze.yml/badge.svg)](https://github.com/arjangeertsema/zeebe-client-csharp-bootstrap/actions/workflows/analyze.yml)
[![](https://img.shields.io/nuget/v/zb-client-bootstrap.svg)](https://www.nuget.org/packages/zb-client-bootstrap/) 
[![](https://img.shields.io/nuget/dt/zb-client-bootstrap)](https://www.nuget.org/stats/packages/zb-client-bootstrap?groupby=Version) 
[![](https://img.shields.io/github/license/arjangeertsema/zeebe-client-csharp-bootstrap.svg)](https://www.apache.org/licenses/LICENSE-2.0) 
[![Total alerts](https://img.shields.io/lgtm/alerts/g/arjangeertsema/zeebe-client-csharp-bootstrap.svg?logo=lgtm&logoWidth=18)](https://lgtm.com/projects/g/arjangeertsema/zeebe-client-csharp-bootstrap/alerts/)
[![](https://img.shields.io/badge/Community%20Extension-An%20open%20source%20community%20maintained%20project-FF4700)](https://github.com/camunda-community-hub/community)

# Bootstrap extension for the C# Zeebe client

This project is an extension of the [C# Zeebe client project](https://github.com/camunda-community-hub/zeebe-client-csharp). Zeebe Job handlers are automaticly recognized and bootstrapped via a [.Net HostedService](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice).

## Requirements

* .Net 6.0
* latest [C# Zeebe client release](https://www.nuget.org/packages/zb-client/)
* latest [Zeebe release](https://github.com/zeebe-io/zeebe/releases/)

## How to use

The Zeebe C# client bootstrap extensions is available via nuget (https://www.nuget.org/packages/zb-client-bootstrap/).

See [examples] and [blog post](https://link.medium.com/4a3yax14gjb) for more information.

## Quick start

All classes which implement `IJobHandler<TJob>`, `IJobHandler<TJob, TResponse>`, `IAsyncJobHandler<TJob>` or `IAsyncJobHandler<TJob, TResponse>` are automaticly found, added to the service collection and autowired to Zeebe when you register this bootstrap project with the `IServiceCollection.BootstrapZeebe()` extension method.

The `BootstrapZeebe` method has two parameters:

1. `ZeebeBootstrapOptions` via [configuration, action delegate or both](https://docs.microsoft.com/en-us/dotnet/core/extensions/options-library-authors).
1. An array with assemblies which will be scanned for job handlers.

```csharp
ConfigureServices((hostContext, services) => {
    services.BootstrapZeebe(
        hostContext.Configuration.GetSection("ZeebeBootstrap"),
        this.GetType().Assembly
    );
})
```

### Job

The job is an implementation of `AbstractJob`. By default the simple name of the job is mapped to BPMN task job type. Job types must be unique. The default job configuration can be overwritten with `AbstractJobAttribute` implementations, see [attributes] for more information.

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

There is also a generic version `AbstractJob<TState>` which will automaticly deserialize job variables into a typed object. Each property is automaticly added to the `FetchVariables` collection when the `FetchVariablesAttribute` is not used.

```csharp
public class SimpleJob : AbstractJob<SimpleJobState>
{
    public SimpleJob(IJob job, SimpleJobState state) 
        : base(job, state)
    {  }
}

public class SimpleJobState
{
    public bool Test { get; set; }
}
```



### Job handler

The job handler is an implementation of `IJobHandler<TJob>`, `IJobHandler<TJob, TResponse>`, `IAsyncJobHandler<TJob>` or `IAsyncJobHandler<TJob, TResponse>`. Job handlers are automaticly added to the DI container, therefore you can use dependency injection inside the job handlers.  The default job handler configuration can be overwritten with `AbstractJobHandlerAttribute` implementations, see [attributes] for more information.


```csharp
public class SimpleJobHandler : IAsyncJobHandler<SimpleJob>
{
    public async Task HandleJob(SimpleJob job, CancellationToken cancellationToken)
    {  
        //TODO: make the handling idempotent.
        await Usecase.ExecuteAsync(cancellationToken);
    }
}
```

A handled job has three outcomes:

1. The job has been handled without exceptions: this will automaticly result in a `JobCompletedCommand` beeing send to the broker. The `TResponse` is automaticly serialized and added to the `JobCompletedCommand`.
1. An exception has been thrown while handling the job and the exception implements `AbstractJobException`: this wil automaticly result in a `ThrowErrorCommand` beeing send to the broker;
1. Any other exception will automaticly result in a `FailCommand` beeing send to the broker;

The `JobCompletedCommand` accepts variables which are added to process instance. For this use case the job handler can be use with a second generic parameter `IJobHandler<TJob, TResponse>`. The response is automaticly serialized.

```csharp
public class SimpleJobHandler : IAsyncJobHandler<SimpleJob, SimpleResponse>
{
    public async Task<SimpleResponse> HandleJob(SimpleJob job, CancellationToken cancellationToken)
    {
        //TODO: make the handling idempotent.
        var result = await Usecase.ExecuteAsync(cancellationToken);
        return new SimpleResponse(result);
    }
}
```


## Extensions

1. `IPublishMessageCommandStep3`, `ICreateProcessInstanceCommandStep3` and `ISetVariablesCommandStep1` are extended with the `State(object state)` method which uses the registered `IZeebeVariablesSerializer` service to automaticly serialize state and pass the result to the `Variables(string variables)` method.

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
