# Implement Mediator Pattern with MediatR in C#

```markdown
**Source:** https://medium.com/codenx/implement-mediator-pattern-with-mediatr-in-c-8a271d7b9901
**Date Captured:** 2025-07-28T17:34:52.924Z
**Domain:** medium.com
**Author:** Chaitanya (Chey) Penmetsa
**Category:** programming
```

## Summary

This article explains how to implement the Mediator pattern in C# using the MediatR library. It highlights MediatR's benefits, such as decoupling and enabling pipeline behaviors, and clarifies its role within CQRS. The guide provides a practical, step-by-step example of building a Customer API, demonstrating how to define requests and handlers, integrate them into a controller, and configure MediatR for dependency injection.

---

In this blog, we’ll explore the implementation of the mediator pattern in C#. To grasp the concept of the mediator pattern and its use cases, I recommend reading my previous blog:

[Mediator Pattern Explained with C#](https://medium.com/codenx/mediator-pattern-explained-with-c-f0562cbce880)

As discussed in the previous blog, incorporating a mediator into an application can lead to increased complexity over time. For smaller applications, employing numerous design patterns may introduce unnecessary intricacies. However, as the application expands, accumulating business logic and adhering to principles like KISS (Keep It Simple, Stupid) and DRY (Don’t Repeat Yourself) may involve making direct calls between services or managers. While dependency injection can address tight coupling, it may still result in classes becoming cumbersome with various dependencies.

This is where the Mediator Pattern proves beneficial. In the context of business logic, each component communicates with the mediator, and the mediator takes on the responsibility of facilitating communication between different components. Another scenario where the mediator pattern finds utility is in .NET controllers. Instead of injecting all managers into controllers, you can simply inject a mediator, allowing it to handle communication with other managers.

![Diagram showing a Controller communicating with a Mediator, which then communicates with Manager/Service 1 and Manager/Service 2.](https://miro.medium.com/v2/resize:fit:700/1*vQ3s-vuaz8_E7Gv53NMgEA.png)

## Misconception

A lot of times people hear mediator pattern used with CQRS and assume that CQRS needs mediator pattern. That is not true; you can implement CQRS without the mediator pattern as well. Just using the mediator pattern in CQRS reduces the coupling between commands/queries and handlers. We will see CQRS and Event sourcing examples in a future blog.

## MediatR

MediatR is a .NET implementation of the Mediator pattern that offers support for both synchronous and asynchronous requests/responses, commands, queries, notifications, and events. It employs intelligent dispatch using C# generic variance. Simplifying the adoption of the Command Query Responsibility Segregation (CQRS) pattern, MediatR provides a straightforward approach to managing command and query handlers. Functioning as a mediator, MediatR efficiently directs commands and queries to their designated handlers.

### Key Features of MediatR

*   **Decoupling:** MediatR facilitates the separation of the request sender (command or query) from its recipient (handler), contributing to more maintainable and modular code.
*   **Pipeline Behaviors:** It accommodates the incorporation of pipeline behaviors, allowing for the easy addition of cross-cutting concerns like validation, logging, and authentication.
*   **Automatic Handler Discovery:** MediatR possesses the capability to automatically identify and register handlers, reducing the need for explicit configuration.

## Example

Let’s explore the functionality of MediatR in C# by constructing a sample Customer API within an e-commerce application. In this context, we will refer to commands and queries as requests, and the corresponding classes responsible for handling them will be termed handlers.

## Step 1: Create projects and install required package

For this, we will create 2 projects: one called `MediatRAPI` which will serve the Customer API, and another class library called `MediatRHandlers` where we configure requests and request handlers. Also, install the MediatR NuGet package using the command below.

```bash
dotnet add package MediatR
```

## Step 2: Create Requests

Let us create two requests as shown below for creating a Customer and retrieving the Customer by Customer Id.

```csharp
public class Customer  
{  
    public string FirstName { get; set; }  
  
    public string LastName { get; set; }  
  
    public string EmailAddress { get; set; }  
  
    public string Address { get; set; }  
}
```

```csharp
using MediatR;  
using MediatRHandlers.Entities;  
  
namespace MediatRHandlers.Requests  
{  
    public class CreateCustomerRequest : IRequest<int>  
    {  
        public Customer Customer { get; set; }  
    }  
}
```

```csharp
using MediatR;  
using MediatRHandlers.Entities;  
  
namespace MediatRHandlers.Requests  
{  
    public class GetCustomerRequest : IRequest<Customer?>  
    {  
        public int CustomerId { get; set; }  
    }  
}
```

## Step 3: Create Handlers

For each of the above requests, create handlers as shown below.

```csharp
using MediatR;  
using MediatRHandlers.Repositories;  
using MediatRHandlers.Requests;  
  
namespace MediatRHandlers.RequestHandlers  
{  
    public class CreateCustomerHandler : IRequestHandler<CreateCustomerRequest, int>  
    {  
        //Inject Validators   
        private readonly ICustomerRepository _customerRepository;  
  
        public CreateCustomerHandler(ICustomerRepository customerRepository)  
        {  
            _customerRepository = customerRepository;  
        }  
  
        public async Task<int> Handle(CreateCustomerRequest request,   
            CancellationToken cancellationToken)  
        {  
            // First validate the request  
            return await _customerRepository.CreateCustomer(request.Customer);  
        }  
    }  
}
```

```csharp
using MediatR;  
using MediatRHandlers.Entities;  
using MediatRHandlers.Repositories;  
using MediatRHandlers.Requests;  
  
namespace MediatRHandlers.RequestHandlers  
{  
    public class GetCustomerHandler : IRequestHandler<GetCustomerRequest, Customer?>  
    {  
        private readonly ICustomerRepository _customerRepository;  
  
        public GetCustomerHandler(ICustomerRepository customerRepository)  
        {  
            _customerRepository = customerRepository;  
        }  
  
        public async Task<Customer?> Handle(GetCustomerRequest request, CancellationToken cancellationToken)  
        {  
            return await _customerRepository.GetCustomer(request.CustomerId);  
        }  
    }  
}
```

## Step 4: Create Controller

Create a Customer Controller as shown below. If you notice, we are not injecting all the handlers; instead, we are injecting only the mediator.

```csharp
using MediatR;  
using MediatRHandlers.Entities;  
using MediatRHandlers.Requests;  
using Microsoft.AspNetCore.Mvc;  
  
namespace MediatRAPI.Controllers  
{  
    [ApiController]  
    [Route("[controller]")]  
    public class CustomerController : ControllerBase  
    {  
        private readonly IMediator _mediator;  
  
        public CustomerController(IMediator mediator)  
        {  
            _mediator = mediator;  
        }  
  
        [HttpGet("customerId")]  
        public async Task<Customer?> GetCustomerAsync(int customerId)  
        {  
            var customerDetails = await _mediator.Send(new GetCustomerRequest() { CustomerId = customerId});  
  
            return customerDetails;  
        }  
  
        [HttpPost]  
        public async Task<int> CreateCustomerAsync(Customer customer)  
        {  
            var customerId = await _mediator.Send(new CreateCustomerRequest() { Customer = customer});  
            return customerId;  
        }  
    }  
}
```

## Step 5: Wire up the registrations

Register the MediatR registrations in the program or startup file as shown below.

```csharp
using Microsoft.Extensions.DependencyInjection;  
  
namespace MediatRHandlers  
{  
    public static class MediatRDependencyHandler  
    {  
        public static IServiceCollection RegisterRequestHandlers(this IServiceCollection services)  
        {  
            return services  
                .AddMediatR(cf => cf.RegisterServicesFromAssembly(typeof(MediatRDependencyHandler).Assembly));  
        }  
    }  
}
```

```csharp
using MediatRHandlers;  
using MediatRHandlers.Repositories;  
  
var builder = WebApplication.CreateBuilder(args);  
  
// Add services to the container.  
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();  
builder.Services.RegisterRequestHandlers();  
  
builder.Services.AddControllers();  
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle  
builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen();  
  
var app = builder.Build();  
  
// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())  
{  
    app.UseSwagger();  
    app.UseSwaggerUI();  
}  
  
app.UseAuthorization();  
  
app.MapControllers();  
  
app.Run();
```

## Step 6: Run the API

Once you run the API project, you will see the Customer API Swagger, where you can test creating a customer and getting a customer as shown below.

![Swagger UI showing available Customer API endpoints: GET /customer/customerId and POST /customer.](https://miro.medium.com/v2/resize:fit:700/1*7EP90j3aoY4Lw_jp8LC5DQ.png)

![Swagger UI showing details for the POST /customer endpoint, including a curl command example for creating a customer.](https://miro.medium.com/v2/resize:fit:700/1*LfrWhTz3MzE0iUzsSAAaNA.png)

![Swagger UI showing details for the GET /customer/{customerId} endpoint, including a curl command example and a sample JSON response for a customer.](https://miro.medium.com/v2/resize:fit:700/1*2I-gE8c1TuhwTLon04jxMA.png)

It is crucial to thoroughly evaluate the pros and cons of the Mediator pattern before incorporating it into your project. Although it can serve as a robust solution for orchestrating communication between objects in your system, it may not be the optimal choice for every scenario.

Thanks for taking the time to read the article. If you found it helpful and would like to show support, please consider:

1.  Clap for the story and bookmark for future reference.
2.  Follow me on [Chaitanya (Chey) Penmetsa](/@chaitupmk) for more content.
3.  Stay connected on [LinkedIn](https://www.linkedin.com/in/chaitanyapenmetsa/).

Wishing you a happy learning journey 📈, and I look forward to sharing new articles with you soon.