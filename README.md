[![BUILD](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/actions/workflows/build.yml/badge.svg)](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/actions/workflows/build.yml)
[![ANALYZE](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/actions/workflows/analyze.yml/badge.svg)](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/actions/workflows/analyze.yml)
[![](https://img.shields.io/nuget/v/zb-client-accelerator.svg)](https://www.nuget.org/packages/zb-client-accelerator/) 
[![](https://img.shields.io/nuget/dt/zb-client-accelerator)](https://www.nuget.org/stats/packages/zb-client-accelerator?groupby=Version) 
[![](https://img.shields.io/github/license/VonDerBeck/zeebe-client-csharp-accelerator.svg)](https://www.apache.org/licenses/LICENSE-2.0) 
![Compatible with: Camunda Platform 8](https://img.shields.io/badge/Compatible%20with-Camunda%20Platform%208-0072Ce)
[![](https://img.shields.io/badge/Lifecycle-Incubating-blue)](https://github.com/Camunda-Community-Hub/community/blob/main/extension-lifecycle.md#incubating-)

# Bootstrap Accelerator for the C# Zeebe client

This project is an extension of the [C# Zeebe client project](https://github.com/camunda-community-hub/zeebe-client-csharp). Zeebe Job handlers are automaticly recognized and bootstrapped via a [.Net HostedService](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice).

Read the [Zeebe documentation](https://docs.camunda.io/docs/components/zeebe/zeebe-overview/) for more information about the Zeebe project.

The basic idea for this came from https://github.com/camunda-community-hub/zeebe-client-csharp-bootstrap.
We loved the idea, but had in some parts our own preferences for defaults and behaviour. So this is our version of a good Bootstrap
Extension for the C# Zeebe Client. Credits for the base work still belong to https://github.com/arjangeertsema.

## Requirements

* net standard 2.0 or higher, which means
    * .net core 2.1 or higher
    * or .net framework 4.7.1 or higher
* latest [C# Zeebe client release](https://www.nuget.org/packages/zb-client/)
* latest [Zeebe release](https://github.com/zeebe-io/zeebe/releases/)

## How to use

The Zeebe C# client bootstrap extension is available via nuget (https://www.nuget.org/packages/zb-client-accelerator/).

## Quick start

All classes which implement `IJobHandler<ZeebeJob>`, `IJobHandler<ZeebeJob, TResponse>`, `IAsyncJobHandler<ZeebeJob>` or `IAsyncJobHandler<ZeebeJob, TResponse>` are automatically found, added to the service collection and autowired to Zeebe when you register this bootstrap project with the `IServiceCollection.BootstrapZeebe()` extension method.

More magic is provided by `using global::Zeebe.Client.Accelerator.Extensions;` which provides you with further extensions for `IHost`, `IZeebeClient` etc. in
order to deploy processes or create one time message receivers.

### Bootstrap Zeebe

The `BootstrapZeebe` method has two parameters:

1. `ZeebeBootstrapOptions` via [configuration, action delegate or both](https://docs.microsoft.com/en-us/dotnet/core/extensions/options-library-authors).
1. An array with assemblies which will be scanned for job handlers.

```csharp
ConfigureServices((hostContext, services) => {
    services.BootstrapZeebe(
        hostContext.Configuration.GetSection("ZeebeConfiguration"),
        this.GetType().Assembly
    );
})
```

Example Web Application:

```csharp
// Start building my WebApplication
var builder = WebApplication.CreateBuilder(args);

// Bootstrap Zeebe Integration
builder.Services.BootstrapZeebe(
    builder.Configuration.GetSection("ZeebeConfiguration"),
    typeof(Program).Assembly);
```

The configuration will e.g. look as follows: 

```json
{
  "ZeebeConfiguration": {
    "Client": {
      "GatewayAddress": "127.0.0.1:26500"
    },
    "Worker": {
      "MaxJobsActive": 5,
      "TimeoutInMilliseconds": 500,
      "PollIntervalInMilliseconds": 50,
      "PollingTimeoutInMilliseconds": 1000,
      "RetryTimeoutInMilliseconds": 1000
    }
  },
  ...
}
```

If we want to deploy some processes right before the final startup of our application (provided as extension for `IHost` and `IServiceProvider`) we create a deployment as follows:

```csharp
var app = builder.Build();
...
// Deploy all process resources
app.CreateZeebeDeployment()
    .UsingDirectory("Resources")
    .AddResource("insurance_application.bpmn")
    .AddResource("document_request.bpmn")
    .AddResource("risk_check.dmn")
    .Deploy();

// Now run the application
app.Run();
```

### Job Handler

The job handler is an implementation of `IJobHandler<ZeebeJob>`, `IJobHandler<ZeebeJob, TResponse>`, `IAsyncJobHandler<ZeebeJob>` or `IAsyncJobHandler<ZeebeJob, TResponse>`. Job handlers are automaticly added to the DI container, therefore you can use dependency injection inside the job handlers.  The default job handler configuration can be overwritten with `AbstractJobHandlerAttribute` implementations, see [attributes] for more information.

```csharp
[JobType("doSomeWork")]
public class SimpleJobHandler : IAsyncJobHandler<ZeebeJob>
{
    private readonly MyApiService _myApiService;

    public SimpleJobHandler(MyApiService myApiService)
    {
        _myApiService = myApiService;
    }

    public async Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
    {  
        // execute business service etc.
        await _myApiService.DoSomethingAsync(cancellationToken);
    }
}
```

Of course you are able to access process variables and return a result. E.g.:

```csharp
[JobType("doAwesomeWork")]
public class SimpleJobHandler : IAsyncJobHandler<ZeebeJob<SimpleJobPayload>, SimpleResponse>
{
    private readonly MyApiService _myApiService;

    public SimpleJobHandler(MyApiService myApiService)
    {
        _myApiService = myApiService;
    }

    public async Task<SimpleResponse> HandleJob(ZeebeJob<SimpleJobPayload> job, CancellationToken cancellationToken)
    {  
        // get variables
        var variables = job.getVariables();

        // execute business service etc.
        var result = await _myApiService.DoSomethingAsync(variables.CustomerNo, cancellationToken);
        return new SimpleResponse(result);
    }

    class SimpleJobPayload
    {
        public string CustomerNo { get; set; }
    }
}
```

And there are more options, including the option to access custom headers configured in the process model:

```csharp
[JobType("doComplexWork")]
public class SimpleJobHandler : IAsyncJobHandler<ZeebeJob>
{
    private readonly MyApiService _myApiService;

    public SimpleJobHandler(MyApiService myApiService)
    {
        _myApiService = myApiService;
    }

    public async Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
    {  
        // get all variables
        ProcessVariables variables = job.getVariables<ProcessVariables>();
        // get custom headers
        MyCustomHeaders headers = job.getCustomHeaders<MyCustomHeaders>();

        // execute business service etc.
        await _myApiService.DoSomethingComplex(variables.Application, headers.SomeConfiguration, cancellationToken);
        ...
    }

    class ProcessVariables
    {
        public string? BusinessKey { get; set; }

        public NewApplication Application { get; set; }

        public string? ApplicantName { get; set; }

        ...
    }

    class MyCustomHeaders
    {
        public string SomeConfiguration { get; set; }
    }
}
```

If you like to explicitely restrict the variables fetched from Zeebe, you have the following additional option:

```csharp
[JobType("doComplexWork")]
[FetchVariables("businessKey", "applicantName")]
public class SimpleJobHandler : IAsyncJobHandler<ZeebeJob>
{
   ...
}
```

A handled job has three outcomes:

1. The job has been handled without exceptions: this will automaticly result in a `JobCompletedCommand` beeing send to the broker. The optional `TResponse` is automaticly serialized and added to the `JobCompletedCommand`.
1. A `BpmnErrorException` has been thrown while handling the job: this will automaticly result in a `ThrowErrorCommand` beeing send to the broker triggering Error Boundary Events in the process.
1. Any other unexpected exception will automatically result in a `FailCommand` beeing send to the broker including message details and reducing the number of retries;

### Dynamic message receiver

See [Example for synchronous responses from processes](https://github.com/camunda-community-hub/camunda-8-examples/tree/main/synchronous-response-springboot) for a description of the scenario.

You can create a dynamic job handlers for receiving a message once as follows:

```csharp
try
{
    string jsonContent = _zeebeClient.ReceiveMessage("received_" + number, TimeSpan.FromSeconds(5), "someNewVariable1", "someVariable2");
    ...
} catch (MessageTimeoutException)
{
    // nothing received
    ...
}
```

Simply waiting without receiving any variables:

```csharp
bool messageReceived = _zeebeClient.ReceiveMessage("received_" + number, TimeSpan.FromSeconds(3));
```

The one time job handler will be destroyed after `ReceiveMessage` returns.

## Hints

1. By default the job handlers are added to de DI container with a `Transient` service lifetime. This can be overriden by adding the `ServiceLifetimeAttribute` to the job handler, see [attributes] for more information.
1. By default the `ZeebeVariablesSerializer` is registered as the implementation for `IZeebeVariablesSerializer` which uses `System.Text.Json.JsonSerializer`. Serialization / Deserialization uses CamelCase as naming policy. 

## How to build

Run `dotnet build Zeebe.Client.Bootstrap.sln`

## How to test

Run `dotnet test Zeebe.Client.Bootstrap.sln`

[examples]:  https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/tree/main/examples
[attributes]: https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/tree/main/src/Zeebe.Client.Bootstrap/Attributes