```yaml
---
title: How To Test Integrations with APIs Using WireMock in .NET
source: https://antondevtips.com/blog/how-to-test-integrations-with-apis-using-wiremock-in-dotnet
date_published: 2024-09-24T08:55:21.852Z
date_captured: 2025-08-27T13:17:13.711Z
domain: antondevtips.com
author: Anton Martyniuk
category: testing
technologies: [.NET, ASP.NET Core, WireMock, PostgreSQL, EF Core, Docker, Testcontainers, Refit, MediatR, HttpClientFactory, Npgsql, xUnit, FluentAssertions, Faker, Nuget]
programming_languages: [C#, JSON, SQL]
tags: [integration-testing, api-mocking, dotnet, aspnet-core, wiremock, testcontainers, microservices, testing, http-client, refit]
key_concepts: [integration-testing, api-mocking, test-doubles, dependency-injection, webapplicationfactory, containerization, microservices-architecture, minimal-apis, command-query-pattern, json-path]
code_examples: true
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide on implementing API integration tests in .NET applications using WireMock. It demonstrates how to mock external API dependencies, such as Stock-Service and Carrier-Service, to enable reliable and repeatable tests without requiring live services. The post covers setting up a Shipping Service with ASP.NET Core Minimal APIs, integrating with PostgreSQL via EF Core, and utilizing Refit for HTTP communication. Key topics include configuring WireMock mappings with JSON, leveraging Testcontainers to manage Dockerized WireMock and PostgreSQL instances, and writing C# integration tests to validate various API response scenarios, including success, validation errors, and bad requests.
---
```

# How To Test Integrations with APIs Using WireMock in .NET

![Cover image for the article, showing a code icon and the partial title 'HOW TO INTEGRA... APIS USIN...'](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_integration_tests_wiremock.png&w=3840&q=100)

# How To Test Integrations with APIs Using WireMock in .NET

Sep 24, 2024

[Download source code](/source-code/how-to-test-integrations-with-apis-using-wiremock-in-dotnet)

8 min read

[Sponsor my newsletter to reach 12,000+ readers](/sponsorship)

**Integration testing** is a type of software testing essential for validating the interactions between different components of an application, ensuring they work together as expected. The main goal of integration testing is to identify any issues that may arise when these components interact with each other.

These components are: databases, caches, message queues, APIs, external services, etc.

In this blog post, we will focus on how to write integration tests that integrate with APIs. And how you can use **WireMock** to mock API integrations.

Earlier I had a fantastic post on [integration testing best practices](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises), make sure to [check it out](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises).

## Introduction to API Integration Testing

In integration tests, we are testing one particular application that integrates with other services.

We don't need to run other services, for example, in Docker, to test integration with each other, either with synchronous or asynchronous communication (events). If we decide to do so - these are another kind of tests, named **End-To-End (E2E)** tests. In E2E tests, we test an entire set of services that work together as a whole.

In integration tests, it is sufficient to test if our service is sending a request and receiving a response to an API endpoint with a given schema.

As we don't have other services running during integration testing - we need to somehow simulate these services. And here is where a [WireMock](https://wiremock.org/) library comes into play. WireMock allows mocking the APIs your service depends on.

Before diving deep into WireMock, let's first explore the application we will be testing.

## The Application We Will Be Testing

Today I'll show you how to write integration tests for a **Shipping Service** that is responsible for creating and updating shipments for purchased products.

**ShippingService** implements the following use cases, available through webapi:

**1. Create Shipment:** saves shipment details to the database.

**2. Update Shipment Status:** updates the status of a shipment in the database.

**3. Get Shipment By Number:** returns a shipment from a database by a number.

**ShippingService** has the following integrations:

*   Postgres database, using EF Core
*   Stock-Service, Carrier-Service, using HTTP

Each use case exposes a webapi endpoint implemented by ASP.NET Core Minimal APIs. Each endpoint uses MediatR to publish a corresponding Command or Query to implement the use case.

Let's have a look at implementations of "Create Shipment" use case. It implements the following flow:

*   checks if a Shipment for a given OrderId is already created
*   checks `Stock-Service` whether it has available number of Products
*   creates Shipment in the database
*   sends request to the `Carrier-Service` with shipment details

Let's explore the command handler `Handle` method that implements the given logic:

```csharp
internal sealed class CreateShipmentCommandHandler(
    EfCoreDbContext context,
    IStockApi stockApi,
    ICarrierApi carrierApi,
    ILogger<CreateShipmentCommandHandler> logger)
    : IRequestHandler<CreateShipmentCommand, ErrorOr<ShipmentResponse>>
{
    public async Task<ErrorOr<ShipmentResponse>> Handle(
        CreateShipmentCommand request,
        CancellationToken cancellationToken)
    {
        var shipmentAlreadyExists = await context.Shipments.AnyAsync(x => x.OrderId == request.OrderId, cancellationToken);
        if (shipmentAlreadyExists)
        {
            logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
            return Error.Conflict($"Shipment for order '{request.OrderId}' is already created");
        }

        var products = request.Items.Select(x => new Product(x.Product, x.Quantity)).ToList();
        var stockResponse = await stockApi.CheckStockAsync(new CheckStockRequest(products));

        if (!stockResponse.IsSuccessStatusCode)
        {
            logger.LogInformation("Received error from stock-service: {ErrorMessage}", stockResponse.Error.Content);
            return Error.Validation("ProductsNotAvailableInStock", $"Received error from stock-service: '{stockResponse.Error.Content}'");
        }

        if (!stockResponse.Content.IsSuccess)
        {
            logger.LogInformation("Received error from stock-service: {ErrorMessage}", stockResponse.Content.ErrorMessage);
            return Error.Validation("ProductsNotAvailableInStock", $"Received error from stock-service: '{stockResponse.Content.ErrorMessage}'");
        }

        var shipmentNumber = new Faker().Commerce.Ean8();
        var shipment = request.MapToShipment(shipmentNumber);

        await context.Shipments.AddAsync(shipment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created shipment: {@Shipment}", shipment);

        var carrierRequest = new CreateCarrierShipmentRequest(request.OrderId, request.Address, request.Carrier, request.ReceiverEmail, request.Items);
        await carrierApi.CreateShipmentAsync(carrierRequest);

        var response = shipment.MapToResponse();
        return response;
    }
}
```

For communication with `Stock-Service` and `Carrier-Service` I use `IStockApi` and `ICarrierApi` correspondingly. They are [Refit](https://github.com/reactiveui/refit) API interfaces:

```csharp
public interface IStockApi
{
    [Post("/api/stocks/check")]
    Task<ApiResponse<CheckStockResponse>> CheckStockAsync([Body] CheckStockRequest request);
}

public interface ICarrierApi
{
    [Post("/api/shipments")]
    Task CreateShipmentAsync([Body] CreateCarrierShipmentRequest request);
}
```

I really love using [Refit](https://github.com/reactiveui/refit) library for communication with other services via HTTP protocol. This library provides an interface wrapper (with code generation) that wraps `HttpClient` using `HttpClientFactory`.

Here's how these Refit interfaces are registered in the DI container:

```csharp
public static IServiceCollection AddExternalServices(
    this IServiceCollection services,
    IConfiguration configuration)
{
    var carrierApiBaseUrl = configuration["CarrierApi:BaseUrl"]!;
    var stockApiBaseUrl = configuration["StockApi:BaseUrl"]!;

    services.AddRefitClient<ICarrierApi>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(carrierApiBaseUrl));

    services.AddRefitClient<IStockApi>()
        .ConfigureHttpClient(c => c.BaseAddress = new Uri(stockApiBaseUrl));

    return services;
}
```

Simple, isn't it?

> The provided code that communicates with `Stock-Service` and `Carrier-Service` is simplified and is NOT production ready. Its main purpose is to show how you can communication with other services over HTTP and how to write integration tests for such a communication

## Introduction to WireMock

As I explained earlier, for integration testing, you don't need to run all the services together. You can use WireMock to mock the corresponding services APIs.

WireMock has a flexible API that allows to mock any type of HTTP requests. This library can respond with different status code, has matching filters for requests and other features.

We have 2 options on how to work with WireMock:

*   use `WireMock.Net` Nuget package and create a console/webapi application with HTTP requests mocking setup
*   or we can run WireMock in Docker with all mocking in the JSON files.

We will focus on the 2nd option as it is very easy to set up without writing any extra code.

In this blog post I will showcase how to use WireMock with `TestContainers`. In my [integration testing best practices](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises) I explained why using TestContainers is such a great option.

In short [TestContainers](https://www.nuget.org/packages/Testcontainers) library offers an elegant and easy way to run the integration tests. It allows you to run all your external services as docker containers, as you probably love doing in development.

You need to install the following Nuget packages in your project:

```csharp
<PackageReference Include="Testcontainers" Version="3.9.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="3.9.0" />
<PackageReference Include="WireMock.Net.Testcontainers" Version="1.6.0" />
```

`WireMock.Net.Testcontainers` is a package that allows to run your WireMock as a Docker container.

Let's set up [WebApplicationFactory](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises) for our integration tests. We need to start a `Postgres` and `WireMock` containers:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("test")
            .WithUsername("admin")
            .WithPassword("admin")
            .Build();

    private readonly IContainer _wireMockContainer = new ContainerBuilder()
        .WithImage("wiremock/wiremock:3.7.0")
        .WithPortBinding(8080, true)
        .WithBindMount(GetAbsolutePath("mocks"), "/home/wiremock")
        .Build();

    private DbConnection _dbConnection = null!;
    private string _wireMockUrl = string.Empty;

    public HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        _dbConnection = new NpgsqlConnection(_dbContainer.GetConnectionString());
        
        _wireMockUrl = $"http://127.0.0.1:{_wireMockContainer.GetMappedPublicPort(8080)}";

        HttpClient = CreateClient();

        await _dbConnection.OpenAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        await _dbConnection.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings:Postgres", _dbContainer.GetConnectionString());
        
        Environment.SetEnvironmentVariable("CarrierApi:BaseUrl", _wireMockUrl);
        Environment.SetEnvironmentVariable("StockApi:BaseUrl", _wireMockUrl);
    }
    
    private static string GetAbsolutePath(string relativePath)
    {
        var currentExecutablePath = AppDomain.CurrentDomain.BaseDirectory;
        var resultUri = new Uri(new Uri(currentExecutablePath), relativePath);
        return Path.GetFullPath(resultUri.AbsolutePath);
    }
}
```

The key here is creating URL to the WireMock mock service:

```csharp
_wireMockUrl = $"http://127.0.0.1:{_wireMockContainer.GetMappedPublicPort(8080)}";

Environment.SetEnvironmentVariable("CarrierApi:BaseUrl", _wireMockUrl);
Environment.SetEnvironmentVariable("StockApi:BaseUrl", _wireMockUrl);
```

We can have a single instance of wiremock that will mock both `Carrier` and `Stocker` services APIs.

Let's explore the API of `Carrier-Service`. It has a single "POST" endpoint that creates shipment in the carrier database. The implementation is simplified.

```csharp
public sealed record CreateCarrierShipmentRequest(
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    List<ShipmentItem> Items);

app.MapPost("/api/shipments",
    ([FromBody] CreateCarrierShipmentRequest request,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Received create shipment request: {@Request}", request);
    return Results.Ok();
});
```

And the `Stock-Service` has a "check" endpoint that verifies whether there are available products in the stock:

```csharp
public record CheckStockRequest(List<Product> Products);

public record Product(string Name, int Quantity);

app.MapPost("/api/stocks/check",
    ([FromBody] CheckStockRequest request,
    ILogger<Program> logger) =>
{
    logger.LogInformation("Received check stock request: {@Request}", request);

    if (request.Products.All(x => x.Quantity <= 5))
    {
        return Results.Ok(new CheckStockResponse(true, "Products are available in stock"));
    }

    return Results.Ok(new CheckStockResponse(false, "Products are NOT available in stock"));
});
```

## Mocking APIs with WireMock

All mappings in WireMock are configured in JSON files and passed to WireMock docker image as a docker volume.

When running our integration tests locally on Windows from the IDE, we need to specify an absolute path to the mocking directory. When running our integrations tests on Linux or inside a Docker container, we need to specify a relative path:

```csharp
private readonly IContainer _wireMockContainer = new ContainerBuilder()
    .WithImage("wiremock/wiremock:3.7.0")
    .WithPortBinding(8080, true)
    .WithBindMount("mocks", "/home/wiremock")
    .Build();
```

Next we need to create our mapping directory and files. You need to specify "Copy If Newer" or "Copy Always" option for all the files in the "mocks" folder.

![Screenshot showing the project structure in an IDE, highlighting the 'mocks' folder containing '__files' and 'mappings' for WireMock configurations.](https://antondevtips.com/media/code_screenshots/aspnetcore/integration-tests-wiremock/img_1.png)

In the `mappings` folder you need to specify how you want WireMock to respond to your requests.

Here is how you can mock the `Carrier-Service`:

```json
{
  "request": {
    "method": "POST",
    "url": "/api/shipments",
    "bodyPatterns": [
      {
        "matchesJsonPath": "$[?(@.orderId)]"
      },
      {
        "matchesJsonPath": "$[?(@.address)]"
      },
      {
        "matchesJsonPath": "$[?(@.carrier)]"
      },
      {
        "matchesJsonPath": "$[?(@.receiverEmail)]"
      },
      {
        "matchesJsonPath": "$[?(@.items)]"
      }
    ]
  },
  "response": {
    "status": 200,
    "bodyFileName": "CarrierShipmentResponse.json",
    "headers": {
      "Content-Type": "application/json"
    }
  }
}
```

You need to specify the following blocks:

*   `request` block: method, url, headers (optional), bodyPatterns (optional)
*   `response`: status (HTTP code), headers and body (text, json, or content from file)

In this example, we are checking whether a request has the desired schema. It is essential to check whether our service sends a request with a correct payload.

If a payload is matched - a response is returned. As we're working with JSON, you can either specify a response in the `jsonBody` or `bodyFileName` blocks.

In this example, I am returning a response from the separate JSON file that is located inside a "\_\_files" folder.

The Response is straightforward:

```json
{
  "message": "Shipment created successfully"
}
```

Now let's explore how to mock a more complex use case with `Stock-Service`. As I showed above, we simulate the behaviour of checking products availability in the stock. In case the products count in the request is more than 5 — we return that products are NOT available in the stock.

```csharp
if (request.Products.All(x => x.Quantity <= 5))
{
    return Results.Ok(new CheckStockResponse(true, "Products are available in stock"));
}

return Results.Ok(new CheckStockResponse(false, "Products are NOT available in stock"));
```

In WireMock I've created 3 mocks for this request:

1.  `stock-check-valid.json`

```json
{
  "request": {
    "method": "POST",
    "url": "/api/stocks/check",
    "bodyPatterns": [
      {
        "matchesJsonPath": "$.products[?(@.quantity > 0 && @.quantity <= 5)]"
      }
    ]
  },
  "response": {
    "status": 200,
    "bodyFileName": "StockCheckResponse.json",
    "headers": {
      "Content-Type": "application/json"
    }
  }
}
```

2.  `stock-check-no-products.json`

```json
{
  "request": {
    "method": "POST",
    "url": "/api/stocks/check",
    "bodyPatterns": [
      {
        "matchesJsonPath": "$.products[?(@.quantity > 5)]"
      }
    ]
  },
  "response": {
    "status": 200,
    "jsonBody": {
      "IsSuccess": false,
      "ErrorMessage": "Products are NOT available in stock"
    },
    "headers": {
      "Content-Type": "application/json"
    }
  }
}
```

3.  `stock-check-invalid.json`

```json
{
  "request": {
    "method": "POST",
    "url": "/api/stocks/check",
    "bodyPatterns": [
      {
        "matchesJsonPath": "$.products[?(@.quantity == 0)]"
      }
    ]
  },
  "response": {
    "status": 400,
    "jsonBody": {
      "error": "Quantity cannot be zero."
    },
    "headers": {
      "Content-Type": "application/json"
    }
  }
}
```

In these mock I use `matchesJsonPath` in `bodyPatterns` to write an expression that checks a given condition. Based on the count of products in the request: I return products are found, not found in stock. And I return "400 Bad Request" in case a request contains 0 number of products.

WireMock is very powerful in pattern matching for requests, you can find information about all available matches in their [official docs](https://wiremock.org/docs/request-matching/).

## Writing API Integration Tests

Now that we have set up WireMock and our integration tests, let's write our tests for creating a shipment use case:

```csharp
[Fact]
public async Task CreateShipment_ShouldSucceed_WhenRequestIsValid()
{
    // Arrange
    var address = new Address("Amazing st. 5", "New York", "127675");

    List<ShipmentItem> items = [ new("Samsung Electronics", 1) ];

    var request = new CreateShipmentRequest("12345", address, "Modern Shipping", "test@mail.com", items);

    // Act
    var httpResponse = await factory.HttpClient.PostAsJsonAsync("/api/shipments", request);
    var shipmentResponse = (await httpResponse.Content.ReadFromJsonAsync<ShipmentResponse>(_jsonSerializerOptions))!;

    // Assert
    httpResponse.StatusCode.Should().Be(HttpStatusCode.OK);

    var expectedResponse = new ShipmentResponse(shipmentResponse.Number, "12345", address, "Modern Shipping", "test@mail.com", ShipmentStatus.Created, items);

     shipmentResponse
         .Should()
         .BeEquivalentTo(expectedResponse);
}
```

It's very important to also test the error and validation response in our integration tests.

Let's create a test that checks whether "Products are NOT available in stock" error is correctly handled in our application and a shipment is not created:

```csharp
[Fact]
public async Task CreateShipment_ShouldFail_WhenProductsAreNoAvailableInStock()
{
    // Arrange
    var address = new Address("Amazing st. 5", "New York", "127675");

    List<ShipmentItem> items = [ new("Samsung Electronics", 10) ];

    var request = new CreateShipmentRequest("12345", address, "Modern Shipping", "test@mail.com", items);

    // Act
    var httpResponse = await factory.HttpClient.PostAsJsonAsync("/api/shipments", request);
    var validationResult = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    // Assert
    httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    validationResult?.Errors.Should().HaveCount(1);

    var error = validationResult?.Errors.FirstOrDefault();
    error?.Key.Should().Be("ProductsNotAvailableInStock");
    error?.Value.First().Should().Be("Received error from stock-service: 'Products are NOT available in stock'");
}
```

And here is a test that verified that Bad Request is received when a request with 0 count of products is sent to the `Stock-Service`:

```csharp
[Fact]
public async Task CreateShipment_ShouldFail_WhenZeroProductsSentInRequest()
{
    // Arrange
    var address = new Address("Amazing st. 5", "New York", "127675");

    List<ShipmentItem> items = [ new("Samsung Electronics", 0) ];

    var request = new CreateShipmentRequest("12345", address, "Modern Shipping", "test@mail.com", items);

    // Act
    var httpResponse = await factory.HttpClient.PostAsJsonAsync("/api/shipments", request);
    var validationResult = await httpResponse.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    // Assert
    httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    validationResult?.Errors.Should().HaveCount(1);

    var error = validationResult?.Errors.FirstOrDefault();
    error?.Key.Should().Be("ProductsNotAvailableInStock");
    error?.Value.First().Should().Be("Received error from stock-service: '{\"error\":\"Quantity cannot be zero.\"}'");
}
```

![Screenshot displaying successful integration test results in an IDE, showing multiple tests passing for 'CreateShipmentTests', 'GetShipmentByNumberTests', and 'UpdateShipmentStatusTests'.](https://antondevtips.com/media/code_screenshots/aspnetcore/integration-tests-wiremock/img_2.png)

## Summary

WireMock is a powerful tool for mocking external APIs in .NET applications. It allows you to create reliable and repeatable tests without depending on live services, ensuring that your application can handle various API responses and failures. By using WireMock, you can focus on writing robust integration testing with external APIs, without a need to run these APIs.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-test-integrations-with-apis-using-wiremock-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-test-integrations-with-apis-using-wiremock-in-dotnet&title=How%20To%20Test%20Integrations%20with%20APIs%20Using%20WireMock%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20To%20Test%20Integrations%20with%20APIs%20Using%20WireMock%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-test-integrations-with-apis-using-wiremock-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-test-integrations-with-apis-using-wiremock-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **12,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 12,000+ Subscribers

Join 12,000+ developers already reading

No spam. Unsubscribe any time.