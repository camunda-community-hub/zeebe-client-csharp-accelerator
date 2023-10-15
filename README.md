[![BUILD](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/actions/workflows/build.yml/badge.svg)](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/actions/workflows/build.yml)
[![ANALYZE](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/actions/workflows/analyze.yml/badge.svg)](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/actions/workflows/analyze.yml)
[![](https://img.shields.io/nuget/v/zb-client-accelerator.svg)](https://www.nuget.org/packages/zb-client-accelerator/) 
[![](https://img.shields.io/nuget/dt/zb-client-accelerator)](https://www.nuget.org/stats/packages/zb-client-accelerator?groupby=Version) 
[![](https://img.shields.io/github/license/VonDerBeck/zeebe-client-csharp-accelerator.svg)](https://www.apache.org/licenses/LICENSE-2.0) 
[![](https://img.shields.io/badge/Community%20Extension-An%20open%20source%20community%20maintained%20project-FF4700)](https://github.com/camunda-community-hub/community)
![Compatible with: Camunda Platform 8](https://img.shields.io/badge/Compatible%20with-Camunda%20Platform%208-0072Ce)
[![](https://img.shields.io/badge/Lifecycle-Incubating-blue)](https://github.com/Camunda-Community-Hub/community/blob/main/extension-lifecycle.md#incubating-)

# Bootstrap Accelerator for the C# Zeebe client

This project is an extension of the [C# Zeebe client project](https://github.com/camunda-community-hub/zeebe-client-csharp). Zeebe Workers are automatically recognized and bootstrapped via a [.Net HostedService](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/multi-container-microservice-net-applications/background-tasks-with-ihostedservice).

Read the [Zeebe documentation](https://docs.camunda.io/docs/components/zeebe/zeebe-overview/) for more information about the Zeebe project.

The basic idea and implementation for this came from https://github.com/camunda-community-hub/zeebe-client-csharp-bootstrap.
We loved the idea, but had in some parts our own preferences for defaults, behaviour and separation of concerns. So this is our version of a good Bootstrap
Extension for the C# Zeebe Client. Credits for the base work still belong to https://github.com/arjangeertsema.

## Requirements

Since version 2.1.0:

* [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) / [.NET 7](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
* [Zeebe C# client 2.2.0 release](https://www.nuget.org/packages/zb-client/)
* [Zeebe 8.x release](https://github.com/zeebe-io/zeebe/releases/)

For older .NET versions please use the 1.x.x release of this extension based on Zeebe C# client 1.3.0 release.

## How to use

The Zeebe C# client bootstrap extension is available via nuget (https://www.nuget.org/packages/zb-client-accelerator/).

**Recommendation:** a complete sample project using this extension can be found in [examples].

## Quick start

All classes which implement `IZeebeWorker`, `IAsyncZeebeWorker`, `IZeebeWorkerWithResult` or `IAsyncZeebeWorkerWithResult` are automatically added to the service collection and autowired to Zeebe when you register this bootstrap project with the `IServiceCollection.BootstrapZeebe()` extension method.

More power is provided by `using global::Zeebe.Client.Accelerator.Extensions;` which provides you with further extensions for `IHost`, `IZeebeClient` etc. in
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
      "HandlerThreads": 3,
      "TimeoutInMilliseconds": 500,
      "PollIntervalInMilliseconds": 50,
      "PollingTimeoutInMilliseconds": 1000,
      "RetryTimeoutInMilliseconds": 1000
    }
  },
}
```
The `GatewayAddress` attribute can be set as well via standard environment variable `ZEEBE_ADDRESS` (since 1.0.2).

### Configuring Camunda Platform 8 SaaS Connection
*Since 1.0.2*

Connections to the Camunda SaaS can be easily configured. Upon creating a new Zeebe API Client in the Cloud Console select the "Env Vars" section for your credentials and memorize all `ZEEBE_*` environment variables. You will get something like the following:

```
export ZEEBE_ADDRESS='a1b2c3dd-12ab-3c4d-ab1b-ab1c23abcc12.bru-2.zeebe.camunda.io:443'
export ZEEBE_CLIENT_ID='ABcDE~a0bCD1eFGH1aEF5G.6HI_abCd0'
export ZEEBE_CLIENT_SECRET='ABCDeFgHi1J0KLMnO0PQrOstUVWXyZAbCdeFGh2IjkLmnO-pqrstUVw0xyzab.cd'
export ZEEBE_AUTHORIZATION_SERVER_URL='https://login.cloud.camunda.io/oauth/token'
export ZEEBE_TOKEN_AUDIENCE='zeebe.camunda.io'
```
You now have 2 options. You can either set exactly these `ZEEBE_*` environment variables and you are done. 
Of course you can alternatively manage these settings in the `appsettings.json` file:

```json
{
  "ZeebeConfiguration": {
    "Client": {
      "GatewayAddress": "a1b2c3dd-12ab-3c4d-ab1b-ab1c23abcc12.bru-2.zeebe.camunda.io:443",
      "Cloud": {
        "ClientId": "ABcDE~a0bCD1eFGH1aEF5G.6HI_abCd0",
        "ClientSecret": "ABCDeFgHi1J0KLMnO0PQrOstUVWXyZAbCdeFGh2IjkLmnO-pqrstUVw0xyzab.cd",
        "AuthorizationServerUrl": "https://login.cloud.camunda.io/oauth/token",
        "TokenAudience": "zeebe.camunda.io"
      }
    }
```
Further rules:
- Environment variables have precedence over `appsettings.json`. 
- `AutorizationServerUrl` and `TokenAudience` have the shown values as default values. Thus they are optional settings.

#### Troubleshouting

If you get DNS errors from the gRPC layer (e.g. "DNS resolution failed for service"), you might need to set the following environment variable:

```
export GRPC_DNS_RESOLVER=native
```

Further documentation is available under [gRPC environment variables](https://chromium.googlesource.com/external/github.com/grpc/grpc/+/HEAD/doc/environment_variables.md).

### Other Transport layer options

The implementation is based on the [Zeebe C# Client](https://github.com/camunda-community-hub/zeebe-client-csharp) and therefore has some more options available:

```
{
  "ZeebeConfiguration": {
    "Client": {
      "GatewayAddress": "my-zeebe-gateway:26500",
      "KeepAliveInMilliSeconds": ...
      "TransportEncryption": {
        "RootCertificatePath": "...",
        "AccessToken": "..."
      }
```
Transport encryption settings can as well be provided using environment variables `ZEEBE_ROOT_CERTIFICATE_PATH`, `ZEEBE_ACCESS_TOKEN`.

### Deploy Processes

If we want to deploy some processes right before the final startup of our application we create a deployment using the extension for `IHost` or `IServiceProvider` as follows:

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

### Zeebe Workers

A Zeebe Worker is an implementation of `IZeebeWorker`, `IAsyncZeebeWorker`, `IZeebeWorkerWithResult` or `IAsyncZeebeWorkerWithResult`. Zeebe Workers are automatically added to the DI container, therefore you can use dependency injection inside.  The default worker configuration can be overwritten with `AbstractWorkerAttribute` implementations, see [attributes] for more information.

```csharp
[JobType("doSomeWork")]
public class SomeWorker : IAsyncZeebeWorker
{
    private readonly MyApiService _myApiService;

    public SimpleJobHandler(MyApiService myApiService)
    {
        _myApiService = myApiService;
    }

    /// <summary>
    /// Handles the job "doSomeWork".
    /// </summary>
    /// <param name="job">the Zeebe job</param>
    /// <param name="cancellationToken">cancellation token</param>
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
public class AwesomeWorker : IAsyncZeebeWorker<SimpleJobPayload, SimpleResponse>
{
    ...

    public async Task<SimpleResponse> HandleJob(ZeebeJob<SimpleJobPayload> job, CancellationToken cancellationToken)
    {  
        // get variables as declared (SimpleJobPayload)
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
The above code will fetch exactly the variables defined as attributes in `SimpleJobPaylad` from the process.

And there are more options, including the option to access custom headers configured in the process model:

```csharp
[JobType("doComplexWork")]
public class ComplexWorker : IAsyncZeebeWorker
{
    ...

    public async Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
    {  
        // get all variables (and deserialize to a given type)
        ProcessVariables variables = job.getVariables<ProcessVariables>();
        // get custom headers (and deserialize to a given type)
        MyCustomHeaders headers = job.getCustomHeaders<MyCustomHeaders>();

        // execute business service etc.
        await _myApiService.DoSomethingComplex(variables.Customer, headers.SomeConfiguration, cancellationToken);
        ...
    }

    class ProcessVariables
    {
        public string? BusinessKey { get; set; }

        public CustomerData Customer { get; set; }

        public string? AccountName { get; set; }

        ...
    }

    class MyCustomHeaders
    {
        public string SomeConfiguration { get; set; }
    }
}
```

The following table gives you an overview of the available options:

| **Interface**                            | **Description**                                                    | **Fetched Variables**                                                                       |
|------------------------------------------|--------------------------------------------------------------------|---------------------------------------------------------------------------------------------|
| `IAsyncZeebeWorker`                      | Asynchronous worker without specific input and no response         | Default is to fetch all process variables.  Use `FetchVariables` attribute for restictions. |
| `IAsyncZeebeWorker<TInput>`              | Asynchronous worker with specific input and no response            | Fetches exactly the variables defined as attributes in `TInput`.                            |
| `IAsyncZeebeWorker<TInput, TResponse>`   | Asynchronous worker with specific input and specific response      | Fetches exactly the variables defined as attributes in `TInput`.                            |
| `IAsyncZeebeWorkerWithResult<TResponse>` | Asynchronous worker without specific input but a specific response | Default is to fetch all process variables. Use `FetchVariables` attribute for restrictions. |
| `IZeebeWorker`                           | Synchronous worker without specific input and no response          | Default is to fetch all process variables.  Use `FetchVariables` attribute for restictions. |
| `IZeebeWorker<TInput>`                   | Synchronous worker with specific input and no response             | Fetches exactly the variables defined as attributes in `TInput`.                            |
| `IZeebeWorker<TInput, TResponse>`        | Synchronous worker with specific input and specific response       | Fetches exactly the variables defined as attributes in `TInput`.                            |
| `IZeebeWorkerWithResult<TResponse>`      | Synchronous worker without specific input but a specific response  | Default is to fetch all process variables. Use `FetchVariables` attribute for restrictions. |

If you like to explicitely restrict the variables fetched from Zeebe, you have the following additional option:

```csharp
[JobType("doComplexWork")]
[FetchVariables("businessKey", "applicantName")]
public class SimpleWorker : IAsyncZeebeWorker
{
   ...
}
```

In case you do not want to fetch any variables at all from Zeebe, use `[FetchVariables(none: true)]`:

```csharp
[JobType("doSimpleWork")]
[FetchVariables(none: true)]
class SimpleWorker : IZeebeWorker
{
   ...
}
```

A handled job has three outcomes:

1. The job has been handled without exceptions: this will automaticly result in a `JobCompletedCommand` beeing send to the broker. The optional `TResponse` is automaticly serialized and added to the `JobCompletedCommand`.
1. A `BpmnErrorException` has been thrown while handling the job: this will automaticly result in a `ThrowErrorCommand` beeing send to the broker triggering Error Boundary Events in the process.
1. Any other unexpected exception will automatically result in a `FailCommand` beeing send to the broker including message details and reducing the number of retries;

### Custom attribute naming
*Since 1.1.0*

This extension uses CamelCase as default naming policy. In order to customize serialization and deserialization the standard `JsonPropertyName`and `JsonIgnore` attributes are fully supported:

```csharp
public class MyJobVariables
{
    [JsonPropertyName("MY_AmountName")]
    public long Amount { get; set; }

    [JsonIgnore]
    public string ToBeIgnored { get; set; }
}
```

### Manual job completion
*Since 2.1.0*

For use cases where autocompletion is not to be used, the `[AutoComplete(false)]` attribute is at your disposal:

```csharp
[AutoComplete(false)]
public class ManualJobHandler : IAsyncZeebeWorker
{
    public async Task HandleJob(ZeebeJob job, CancellationToken cancellationToken)
    {
        // do something ...

        // complete job manually
        await job.GetClient().NewCompleteJobCommand(job.Key).Send(token: cancellationToken);
    }
}
```

Please be aware, that uncatched exceptions still lead to sending fail commands (or error commands in case of `BpmnErrorException`).
It's the responsability of the worker implementation to catch and handle all exceptions if a different behaviour is intended.

### Dynamic message receiver

See [Example for synchronous responses from processes](https://github.com/camunda-community-hub/camunda-8-examples/tree/main/synchronous-response-springboot) for a description of the scenario.

You can create a one time job handler for receiving a message for a dynamic job type `"received_" + number` as follows:

```csharp
try
{
    string jsonContent = _zeebeClient.ReceiveMessage("received_" + number, TimeSpan.FromSeconds(5), "someVariable1", "someVariable2");
    ...
} catch (MessageTimeoutException)
{
    // nothing received
    ...
}
```
Of course it is possible to use a typed response, which will automatically fetch and deserialize all variables defined as attributes in the given type:

```csharp
MyVariables typedContent = _zeebeClient.ReceiveMessage<MyVariables>("received_" + number, TimeSpan.FromSeconds(3));
```


Simply waiting without receiving any variables:

```csharp
bool messageReceived = _zeebeClient.ReceiveMessage("received_" + number, TimeSpan.FromSeconds(3));
```

The one time job handler will be destroyed after `ReceiveMessage` returns.

## Hints

1. By default the workers are added to de DI container with a `Transient` service lifetime. This can be overriden by adding the `ServiceLifetimeAttribute` to the worker, see [attributes] for more information.
1. By default the `ZeebeVariablesSerializer` is registered as the implementation for `IZeebeVariablesSerializer` which uses `System.Text.Json.JsonSerializer`. Serialization / Deserialization always uses CamelCase as naming policy! `JsonPropertyName` and `JsonIgnore` attributes are supported, so that you still have the option to customize your attribute naming.
1. The default job type of a worker is the class name of the worker. This can be overriden by adding the `JobTypeAttribute` to the worker, e.g. `[JobType("myJobName")]`.

## How to build

Run `dotnet build Zeebe.Client.Accelerator.sln`

## How to test

Run `dotnet test Zeebe.Client.Accelerator.sln`

[examples]:  https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/tree/main/examples
[attributes]: https://github.com/VonDerBeck/zeebe-client-csharp-accelerator/tree/main/src/Zeebe.Client.Accelerator/Attributes
