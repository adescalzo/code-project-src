```yaml
---
title: How To Simplify Assertions in Unit and Integration Tests with Verify in .NET
source: https://antondevtips.com/blog/how-to-simplify-assertions-in-unit-and-integration-tests-with-verify-in-dotnet
date_published: 2024-09-27T08:55:21.852Z
date_captured: 2025-08-06T17:30:00.254Z
domain: antondevtips.com
author: Anton Martyniuk
category: testing
technologies: [Verify, .NET, ASP.NET Core, xUnit, NUnit, MSTest, MediatR, Refit, HttpClient, HttpClientFactory, PostgreSQL, Entity Framework Core, WebApplicationFactory, TestContainers, NSubstitute, FluentAssertions, Faker, ErrorOr, Microsoft.Extensions.Logging]
programming_languages: [C#, SQL]
tags: [unit-testing, integration-testing, snapshot-testing, dotnet, csharp, testing, assertions, test-automation, web-api, microservices]
key_concepts: [snapshot-testing, unit-testing, integration-testing, assertion-simplification, dependency-injection, mediatr-pattern, http-client-factory, repository-pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Verify library, a snapshot-based testing tool for .NET, designed to simplify assertions in unit and integration tests. It explains Verify's advantages, such as improved readability and maintainability, and demonstrates its setup with xUnit. The author provides practical examples, showcasing how Verify can reduce verbose assertions in both integration tests using ASP.NET Core's WebApplicationFactory and TestContainers, and unit tests with NSubstitute. The post also covers managing snapshots, including ignoring dynamic members and customizing snapshot locations, concluding with a recommendation on when to best utilize snapshot testing.]
---
```

# How To Simplify Assertions in Unit and Integration Tests with Verify in .NET

![newsletter](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_tests_verify.png&w=3840&q=100)
*Image: A dark background with purple abstract shapes. On the left, a white icon of angle brackets `< />` above the text "dev tips". On the right, large white text reads "HOW TO SIMPLIFY ASSERTIONS IN UNIT AND INTEGRATION TESTS WITH VERIFY IN .NET". This appears to be the cover image for the article.*

# How To Simplify Assertions in Unit and Integration Tests with Verify in .NET

Sep 27, 2024

[Download source code](/source-code/how-to-simplify-assertions-in-unit-and-integration-tests-with-verify-in-dotnet)

6 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

When writing unit and integration tests, assertions are critical for verifying the expected behavior of your code. However, complex assertions can lead to verbose and hard-to-maintain tests.

Today I will show how you can use a **Verify** library that simplifies assertions by using a snapshot-based testing approach. I will share with you my experience on using a **Verify** library in unit and integration tests.

[](#what-is-verify)

## What is Verify?

[Verify](https://github.com/VerifyTests/Verify) is a library that allows you to perform snapshot-based testing. A snapshot is a saved version of your test result, such as an object or a response, which is then compared against future test runs to ensure the output remains consistent. If the output changes unexpectedly, Verify will fail the test and highlight the difference, allowing you to quickly identify and fix issues.

**What Advantages Does Verify Provide?**

*   Readable Assertions: Unlike traditional assertions, where you compare expected and actual values directly in your code, Verify handles this for you by storing the expected result in a snapshot file. This makes your test easier to read.
*   Maintainability: If the output changes due to an intended change in your code, you simply update the snapshot instead of modifying the test assertion logic.
*   Flexibility: Verify supports a wide range of formats and types, including objects, JSON, XML, HTML, text, and more. It works seamlessly with various test frameworks like xUnit, NUnit, and MSTest.

[](#setting-up-verify)

## Setting Up Verify

To get started with **Verify**, first, install the required NuGet package either for **xUnit** or **NUnit**:

```csharp
dotnet add package Verify.Xunit

dotnet add package Verify.NUnit
```

I prefer using **xUnit**. I explained [on LinkedIn](https://www.linkedin.com/posts/anton-martyniuk-93980994_csharp-dotnet-aspnetcore-activity-7230117338509905922-9JLR) why I prefer using xUnit over NUnit.

Let's look at a simple example of using Verify with xUnit to test a method that returns a complex object.

```csharp
public class OrderServiceTests
{
    [Fact]
    public async Task GetOrderById_ShouldMatchSnapshot()
    {
        // Arrange
        var orderService = new OrderService();
        var orderId = 1;

        // Act
        var result = await orderService.GetOrderById(orderId);

        // Assert
        await Verify(result);
    }
}
```

In this example, `Verify(result)` saves the output of `GetOrderById` method to a snapshot file. On subsequent test runs, Verify will compare the result against the snapshot. If there’s a difference, the test will fail, and Verify will provide a detailed diff of the changes.

Now let's explore how to use **Verify** in real-world application when writing unit and integration tests.

[](#the-application-we-will-be-testing)

## The Application We Will Be Testing

Today I'll show you how to write unit and integration tests for a **Shipping Service** that is responsible for creating and updating shipments for purchased products.

**ShippingService** implements the following use cases, available through webapi:

**1\. Create Shipment:** saves shipment details to the database.

**2\. Update Shipment Status:** updates the status of a shipment in the database.

**3\. Get Shipment By Number:** returns a shipment from a database by a number.

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
    IShipmentRepository repository,
    IStockApi stockApi,
    ICarrierApi carrierApi,
    ILogger<CreateShipmentCommandHandler> logger)
    : IRequestHandler<CreateShipmentCommand, ErrorOr<ShipmentResponse>>
{
    public async Task<ErrorOr<ShipmentResponse>> Handle(
        CreateShipmentCommand request,
        CancellationToken cancellationToken)
    {
        var shipmentAlreadyExists = await repository.ExistsAsync(request.OrderId, cancellationToken);
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

        await repository.AddAsync(shipment, cancellationToken);

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

Now let's explore how to write **tests** with **Verify**.

[](#writing-integration-tests-with-verify)

## Writing Integration Tests with Verify

[Integration testing](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises) is a type of software testing essential for validating the interactions between different components of an application, ensuring they work together as expected. The main goal of integration testing is to identify any issues that may arise when these components interact with each other.

[Integration testing](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises) uses actual implementations of dependencies like databases, message queues, external services, and APIs to validate real interactions.

Let's create an integration test for a "Create Shipment" use case. We will use xUnit, WebApplicationFactory and TestContainers:

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

    shipmentResponse.Should().BeEquivalentTo(new ShipmentResponse(
        shipment.Number,
        shipment.OrderId,
        shipment.Address,
        shipment.Carrier,
        shipment.ReceiverEmail,
        shipment.Status,
        shipment.Items.Select(i => new ShipmentItemResponse(i.Product, i.Quantity)).ToList()
    ));
}
```

As you can see, our assertion is pretty big:

```csharp
shipmentResponse.Should().BeEquivalentTo(new ShipmentResponse(
    shipment.Number,
    shipment.OrderId,
    shipment.Address,
    shipment.Carrier,
    shipment.ReceiverEmail,
    shipment.Status,
    shipment.Items.Select(i => new ShipmentItemResponse(i.Product, i.Quantity)).ToList()
));
```

Now let's rewrite this assertion with Verify:

```csharp
// Replace complex assertions with single line of code
await Verify(shipmentResponse);
```

And that's all you need! Really!

When running tests for the first time, all tests would fail. That's because Verify doesn't have a saved snapshot yet.

![Screenshot_1](https://antondevtips.com/media/code_screenshots/aspnetcore/tests-verify/img_1.png)
*Image: A screenshot of the Visual Studio Test Explorer showing that 8 out of 10 integration tests failed. The tree view shows several `CreateShipmentTests` and `GetShipmentByNumberTests` failing with `VerifyException`.*

After a test fails, a Verify will open the default difference comparer application with 2 snapshots: current and a saved one. You need to decide whether you need to update the saved snapshot, correct the test or fix the code.

As we don't have a saved snapshot yet, we need to save it. Here is an example of snapshot comparisons:

![Screenshot_6](https://antondevtips.com/media/code_screenshots/aspnetcore/tests-verify/img_6.png)
*Image: A screenshot of a diff tool (likely WinMerge) showing two side-by-side panels. The left panel, highlighted in yellow, contains the "received" snapshot with details like OrderId, Address, Carrier, ReceiverEmail, and Items. The right panel, which is empty and gray, represents the "verified" snapshot, indicating that no previous snapshot exists for comparison.*

I use WinMerge, you can use any tool you prefer (even a Git diff comparer).

After saving all the snapshots, we can see that all tests are now successfully completed:

![Screenshot_2](https://antondevtips.com/media/code_screenshots/aspnetcore/tests-verify/img_2.png)
*Image: A screenshot of the Visual Studio Test Explorer showing that all 10 integration tests passed successfully. Green checkmarks are visible next to all test names, indicating their success.*

[](#updating-snapshots-and-ignoring-members)

### Updating Snapshots and Ignoring Members

Let's update our code and generate a `ShipmentResponse.Number` randomly. After running a test it will fail, as `ShipmentResponse.Number` differs in current and saved snapshot:

![Screenshot_3](https://antondevtips.com/media/code_screenshots/aspnetcore/tests-verify/img_3.png)
*Image: A screenshot of a diff tool comparing two snapshots. The left panel shows the "received" snapshot with `Number: 93350877`. The right panel shows the "verified" snapshot with `Number: 14331084`. The `Number` field is highlighted in yellow in both panels, indicating a difference between the two snapshots.*

To fix this issue, you can use `IgnoreMember` method to ignore the `ShipmentResponse.Number` property when comparing snapshots:

```csharp
await Verify(shipmentResponse)
    .IgnoreMember<ShipmentResponse>(x => x.Number);
```

[](#snapshots-location)

### Snapshots Location

All snapshots are saved into the current tests folder and have the following name template:

```
TestClass.TestName.verified.txt

// For example
CreateShipmentTests.CreateShipment_ShouldSucceed_WhenRequestIsValid.verified.txt
```

I recommend changing the default snapshot folder with a separate folder location, so the snapshots won't pollute our solution.

You need to execute this code in either the test's constructor or WebApplicationFactory setup section:

```csharp
Verifier.DerivePathInfo(
    (sourceFile, projectDirectory, type, method) => new PathInfo(
        directory: Path.Combine(projectDirectory, "verify_files"),
        typeName: type.Name,
        methodName: method.Name));
```

![Screenshot_4](https://antondevtips.com/media/code_screenshots/aspnetcore/tests-verify/img_4.png)
*Image: A screenshot of a file explorer showing a folder structure. The `verify_files` directory is highlighted, containing snapshot files like `CreateShipmentTests.CreateShipment_ShouldSucceed_WhenRequestIsValid.verified.txt` and `GetShipmentByNumberTests.GetShipmentByNumber_ShouldReturnShipment_WhenShipmentExists.verified.txt`.*

> You can find the **full source code** of the application and integration tests at the end of the blog post.

[](#writing-unit-tests-with-verify)

## Writing Unit Tests with Verify

**Unit Testing:** typically uses mocks or stubs to simulate dependencies, ensuring the test environment is controlled and isolated.

Let's create our tests' setup with **xUnit** and **NSubstitute** for mocks:

```csharp
public class CreateShipmentTests
{
	private readonly IShipmentRepository _mockRepository;
	private readonly IStockApi _mockStockApi;
	private readonly ICarrierApi _mockCarrierApi;
	private readonly CreateShipmentCommandHandler _handler;

	public CreateShipmentTests()
	{
		_mockRepository = Substitute.For<IShipmentRepository>();
		_mockStockApi = Substitute.For<IStockApi>();
		_mockCarrierApi = Substitute.For<ICarrierApi>();

		var logger = NullLogger<CreateShipmentCommandHandler>.Instance;

		_handler = new CreateShipmentCommandHandler(
			_mockRepository,
			_mockStockApi,
			_mockCarrierApi,
			logger
		);
	}
}
```

Now let's write a unit test with that tests `CreateShipmentCommand`:

```csharp
[Fact]
public async Task Handle_ShouldSucceed_WhenRequestIsValid()
{
    // Arrange
    var address = new Address
    {
        Street = "Amazing st. 5",
        City = "New York",
        Zip = "127675"
    };

    List<ShipmentItem> items = [ new ShipmentItem("Samsung Electronics", 1) ];

    var command = new CreateShipmentCommand("12345", address, "Modern Shipping", "test@mail.com", items);

    _mockRepository.ExistsAsync("12345", Arg.Any<CancellationToken>()).Returns(false);

    var stockApiResponse = new ApiResponse<CheckStockResponse>(
        new HttpResponseMessage(HttpStatusCode.OK),
        new CheckStockResponse(true, string.Empty),
        null!);

    _mockStockApi.CheckStockAsync(Arg.Any<CheckStockRequest>())
        .Returns(stockApiResponse);

    _mockCarrierApi.CreateShipmentAsync(Arg.Any<CreateCarrierShipmentRequest>())
        .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.IsError.Should().BeFalse();
    result.Value.OrderId.Should().Be("12345");

    result.Value.Should().NotBeNull();
		result.Value.Number.Should().Be(shipmentNumber);
		result.Value.Should().BeEquivalentTo(new ShipmentResponse(
			shipment.Number,
			shipment.OrderId,
			shipment.Address,
			shipment.Carrier,
			shipment.ReceiverEmail,
			shipment.Status,
			shipment.Items.Select(i => new ShipmentItemResponse(i.Product, i.Quantity)).ToList()
		));
}
```

As you can see, our assertion is pretty big:

```csharp
result.Value.Should().NotBeNull();
result.Value.Number.Should().Be(shipmentNumber);
result.Value.Should().BeEquivalentTo(new ShipmentResponse(
    shipment.Number,
    shipment.OrderId,
    shipment.Address,
    shipment.Carrier,
    shipment.ReceiverEmail,
    shipment.Status,
    shipment.Items.Select(i => new ShipmentItemResponse(i.Product, i.Quantity)).ToList()
));
```

Now let's rewrite this assertion with Verify:

```csharp
await Verify(result.Value)
    .IgnoreMember<ShipmentResponse>(x => x.Number);
```

And that's it!

![Screenshot_5](https://antondevtips.com/media/code_screenshots/aspnetcore/tests-verify/img_5.png)
*Image: A screenshot of the Visual Studio Test Explorer showing that all 10 unit tests passed successfully. Green checkmarks are visible next to all test names, indicating their success.*

> You can find the **full source code** of the application and integration tests at the end of the blog post.

[](#summary)

## Summary

Verify simplifies assertions in your unit and integration tests by using a snapshot-based approach. It reduces verbosity, improves readability, and makes your tests more maintainable. By adopting Verify, you can ensure that your tests remain robust and easy to manage as your code evolves.

Should you replace all your assertions with Verify? Absolutely no!

Use Verify to simplify your tests' implementation, when it's easier to verify by your eye 2 snapshots rather than spending a lot of time writing complex assertions. I use Verify in my unit and, especially, integration tests for complex and large objects, GraphQL responses, reports, etc.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-simplify-assertions-in-unit-and-integration-tests-with-verify-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-simplify-assertions-in-unit-and-integration-tests-with-verify-in-dotnet&title=How%20To%20Simplify%20Assertions%20in%20Unit%20and%20Integration%20Tests%20with%20Verify%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20To%20Simplify%20Assertions%20in%20Unit%20and%20Integration%20Tests%20with%20Verify%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-simplify-assertions-in-unit-and-integration-tests-with-verify-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-simplify-assertions-in-unit-and-integration-tests-with-verify-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.