# Intermediate API Application Boilerplate

Many of our implementation efforts involve integrations between systems.  Often times these integration can be handled by making a point-to-point (PtP) API calls from or web application to the upstream system.  This would be a common occurrence for any upstream systems that support REST or GraphQL APIs that can be called directly via frontend Javascript or from backend code.

Occasionally, we run into scenarios where the upstream API is not accessible via a direct PtP integration from the frontend.  These "backend-only" APIs typically also involve some sort of network-transport IP whitelisting that further complicates matters as most of our web applications are hosted in the cloud with potentially variable IP addresses.

The solution we came up with is to create an "Intermediate" API application that acts as a simple middleware between the web application and the upstream system. This API application essentially "wraps" the upstream AP. It can also handle request / response model simplification and translation and can have a static public IP address assigned to it to avoid issues with IP whitelisting.

These intermediate API applications can be easily deployed and hosted as stateless API applications in Azure for a very minimal cost (roughly $75 USD per month) and offer excellent performance and scalability.

The diagram below outlines the high level architecture of these Intermediate API applications:

![Architecture Diagram](https://github.com/rstrube/dotnet-inter-api-boilerplate/blob/main/doc/img/intermediate-api-app-architecture.png)

## Features

This boilerplate code is designed to be as un-opinionated as possible, but it does provide some basic features, best practices and patterns to follow:

1. Provides simple, clean initialization logic (based on the new .NET 6 `WebApplicationBuilder`) which can easily be extended and enhanced.
2. Provides basic CORS configuration.
3. Comes with [Serilog](https://serilog.net/) pre-configured which provides an excellent foundation for all logging.
4. Leverages strongly typed configuration `IOptions<ABC>` objects that are available via dependency injection.
5. Demonstrates how to differentiate between upstream data models and simplified client data models (for the web application) and how to extrapolate and map between the two.
6. Demonstrates how to compartmentalize the upstream client code.
7. Demonstrates best practices for calling upstream API using .NETs `IHttpClient` and supporting extension methods.
8. Demonstrates how to create a "mock" client for an upstream API.  This "mock" client will not actually make a call to the upstream API, but it will return data in the same structure.  This can be useful for when access to an API is intermittent and/or the upstream API is still in active development.
9. Uses a free, public API [Bored API](https://www.boredapi.com/) as a demonstration of how to wrap an upstream API.

## Initialization

The code base leverages the new .NET 6 `WebApplicationBuilder` (as opposed to using the older `IWebHostBuilder` or `IHostBuilder`).  You can examine the code in [Program.cs](https://github.com/rstrube/dotnet-inter-api-boilerplate/blob/main/src/InterApiBoilerplate/Program.cs) to better understand the initialization logic.  You'll most likely want to extend the services that are registered for dependency injection purposes, and/or register additional configuration objects.

## Configuration

The code also establishes the best practice of using strongly-typed configuration objects that can be injected using dependency injection.  In the boilerplate code base we have strongly typed configuration objects for both CORS configuration [CorsConfig](https://github.com/rstrube/dotnet-inter-api-boilerplate/blob/main/src/InterApiBoilerplate/Configuration/CorsConfig.cs) and for the upstream API we are wrapping [BoreClientConfig](https://github.com/rstrube/dotnet-inter-api-boilerplate/blob/main/src/InterApiBoilerplate/Configuration/BoredClientConfig.cs) (for [Bored API](https://www.boredapi.com/)).  These classes directly tie to configuration that is locate in `appSettings.json` to strongly typed `IOptions<ABC>` objects that can be passed in via dependency injection into constructors.

### AllowedHosts Configuration

This is a built in .NET configuration option that defines the hostname(s) that the application can run on.  For example if you had a value of `myapiapplication.azure.com` then the API application will only function and respond to requests if it is deployed to that domain.  A value of `*` indicates the API application can run on any hostname.

```
 "AllowedHosts": "*",
```

### CORS Configuration

The built in CORS configuration provides some core functionality that allows you to specify an "allowlist" of clients that can make calls to the API application.

```
"Cors" : {
    "AllowAnyOrigin": false,
    "AllowedOrigins": [
      "http://*.example.com",
      "https://*.example.com"
    ]
  },
```
* `AllowAnyOrigin`: If `true` API application will accept request from *any* client, if `false` the API application will only accept incomings requests from clients with specific hostnames.
* `AllowedOrigins`: An array of clients hostnames that the API application will accept requests from.  Supports `*` as a wildcard.  So `https://*.example.com` would allow HTTPS requests from both `www.example.com` and `example.com`.

This configuration directly ties to the class [CorsConfig.cs] which can be passed into constructors via dependency injection as `IOptions<CorsConfig>`.

### Bored API Configuration

This defines configuration for the upstream API ([Bored API]((https://www.boredapi.com/))).  It also serves as an excellent example for how to define configuration for upstream APIs.

```
"BoredClient": {
    "PooledConnectionLifetime" : 15,
    "UseMock": false,
    "BaseAddress": "https://www.boredapi.com/",
    "ActivityPath": "api/activity"
  },
```

* `PooledConnectionLifetime`: The length of time in minutes that connections will be pooled by a given `IHttpClient` instance.  Pooling connections improves overall performance, but can lead to complications if DNS entries change/go stale.  Setting this to a reasonable time (like 15 minutes) will ensure DNS changes will be propagated and taken into account within 15 minutes.  Please see [this article](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines) for more information.
* `UseMock`: If set to `true` the API application will use the `MockBoredClient` as opposed to `BoredClient`.  The "mock" client will not actually make upstream API calls, but does return data matching the same expected format.
* `BaseAddress`: The base address (protocol & hostname) for the upstream API.
* `ActivityPath` The path to the bored API endpoint for retrieving a list of activity suggestions.
  
## Note on Microsoft.AspNet.WebApi.Client and Newtonsoft.Json

Microsoft provided a the `Microsoft.AspNet.WebApi.Client` library which included a variety of extension methods on the `HttpContent` class (and used `Newtonsoft.Json` behind the scenes).  `Newtonsoft.Json` was for many years the defacto and best JSON serializer/deserializer. Although it's still possible to use the same combination, it's largely been superseded by `System.Net.Http.Json` (for extension methods on `HttpContent`) and `System.Text.Json` for JSON serialization/deserialization.  This combination is now recommended for .NET 5+ applications, what the boilerplate currently uses.

## Response Data

This API application also demonstrates how to separate out the the upstream data models, from the data models you want to expose to the client application (that is calling your API application).  In the boilerplate [BoredActivity.cs](https://github.com/rstrube/dotnet-inter-api-boilerplate/blob/main/src/InterApiBoilerplate/Upstream/Models/BoredActivity.cs) represents the upstream model, and [Activity.cs](https://github.com/rstrube/dotnet-inter-api-boilerplate/blob/main/src/InterApiBoilerplate/Models/Activity.cs) represents the simplified data model that's exposed to the client application.  You'll also notice that the `Activity` class extrapolates some data (specifically the accessibility score) into a string value `Easy` `Intermediate` or `Hard`.  This demonstrates how to take an upstream data model and enhance / extrapolate the data for your client application.

## Building and Running

1. Navigate to `src/InterApiBoilerplate`
2. Run `dotnet run`
3. Application is accessible at `https://localhost:5000` (by default)

You can then use [Postman](https://www.postman.com/) or your web browser (the calls are only GETs for this API application) to test out the various calls.

### Suggest an Activity for one Person

[https://localhost:5000/api/activity]

Example response:
```
{
  "id":"4150284",
  "activityDescription":"Donate to your local food bank",
  "activityType":"charity",
  "numberOfParticipants":1,
  "price":0.5,
  "uri":"",
  "accessibilityScore":0.8,
  "accessibilityRating":"Hard"
}
```

### Suggest an Activity for N number of People

`https://localhost:5000/api/activity/{n}` e.g. [https://localhost:5000/api/activity/4]

Example response:
```
{
  "id":"5262759",
  "activityDescription":"Go see a movie in theaters with a few friends",
  "activityType":"social",
  "numberOfParticipants":4,
  "price":0.2,
  "uri":"",
  "accessibilityScore":0.3,
  "accessibilityRating":"Easy"
}
```