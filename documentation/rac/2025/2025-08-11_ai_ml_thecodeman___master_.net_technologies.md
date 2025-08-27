```yaml
---
title: "TheCodeMan | Master .NET Technologies"
source: https://thecodeman.net/posts/saga-orchestration-pattern?utm_source=newsletter&utm_medium=email&utm_campaign=TheCodeMan%20Newsletter%20-%20%20Start%20with%20Saga%20Orchestration%20Pattern%20v2
date_published: unknown
date_captured: 2025-08-11T11:05:59.306Z
domain: thecodeman.net
author: Unknown
category: ai_ml
technologies: [MassTransit, Automatonymous, Entity Framework, SQL Server, RabbitMQ, .NET 8, ASP.NET Core, JetBrains Rider]
programming_languages: [C#, SQL, JavaScript]
tags: [saga-pattern, orchestration, microservices, distributed-transactions, .net, messaging, state-machine, event-driven, enterprise-integration]
key_concepts: [Saga Pattern, Orchestration Pattern, Distributed Transactions, Microservices Architecture, State Machine, Command-Driven Communication, Event-Driven Architecture, Compensation Transactions]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a detailed explanation of the Saga Orchestration pattern, focusing on its application in managing complex, distributed transactions across multiple microservices. It highlights the challenges of ensuring data consistency in independent services and presents the Saga pattern as a solution through sequential, compensable transactions. The content includes a practical .NET implementation using MassTransit Automatonymous for state machine management, Entity Framework for persistence with SQL Server, and RabbitMQ for message brokering. Code examples illustrate the Saga State Machine, commands, events, and command handlers, demonstrating a robust approach to distributed workflow management.
---
```

# TheCodeMan | Master .NET Technologies

## Saga Orchestration Pattern

January 13 2025

##### **Many thanks to the sponsors who make it possible for this newsletter to be free for readers.**

##### • Download JetBrains Rider IDE for [FREE NOW here](https://www.jetbrains.com/rider/?utm_campaign=rider_free&utm_content=site&utm_medium=cpc&utm_source=thecodeman_newsletter).

### The Challenge

##### In today's digital age, the travel booking process is becoming increasingly complex due to the variety of services travelers require. This domain problem includes the integration of different services such as hotel reservation, airline ticket reservation and transportation organization (eg taxi) from the airport to the accommodation. All these services must be coordinated in an efficient manner to enable a smooth user experience.

##### The main challenge in this domain **is managing transactions across multiple microservices that are not directly connected**. Each microservice (hotel, plane, car/taxi) operates independently, which means that there is no simple way to ensure consistency of transactions across all services.

##### For example, if a traveler wants to cancel a trip, it is necessary to coordinate the cancellation of all reservations (hotel, plane, taxi), which can be complex and prone to errors.

### The Solution?

##### To solve this problem, the SAGA pattern is used.

##### SAGA is a sequential process that manages transactions between microservices by breaking each operation into multiple smaller transactions that can be independently executed and compensated.

##### Each of these transactions is part of a larger "saga" that ensures system-wide consistency.

##### Implementation in the context of travel booking:

##### In our case, the SAGA template could be applied as follows:

##### **1\. Initial booking**: The traveler starts the travel booking process. The first transaction may be a hotel room reservation.

##### **2\. Sequential transactions**: After a successful hotel reservation, the flight reservation follows, and then the organization of taxi transportation.

##### **3\. Compensation transactions**: If there is a problem at any stage (eg the hotel is full), SAGA initiates compensation transactions. This means that previously made reservations (such as an airline ticket) will be canceled or modified, in order to maintain the integrity of the entire reservation. (not part of this newsletter issue)

![A flowchart illustrating the Saga Orchestration pattern, showing a central "TripBooking" service coordinating sequential steps (hotel booking, flight booking, car rental) with individual microservices and receiving confirmation events from each.](images/blog/posts/saga-orchestration-pattern/saga-orchestration.png)

##### This represents the Saga orchestration process.

##### Orchestration is a method where one service acts as the central coordinator for communication among microservices. In this setup, a single service, known as the orchestrator, manages and directs the interactions between other services.

##### This approach relies on command-driven communication, where commands specify the desired actions. The sender issues a command to trigger a specific event, and the recipient executes it without needing to know the origin of the command.

##### Let's take a look on the implementation.

### .NET Implementation

##### I will use MassTransit Automatonymous (there are many libraries for working with sagas, even lightweight ones), which is a state machine library for building distributed systems.

##### It provides a way to define the behavior of a system as a state machine, and it handles the coordination between services using the SAGA pattern.

#### Persistence:

##### I use EF DbContext with SQL Server in the background (any other provider can be used).

```csharp
public class BookingDbContext(DbContextOptions<BookingDbContext> options) : DbContext(options)
{
    public DbSet<Traveler> Travelers { get; set; }

    public DbSet<BookingSagaData> BookingSagaData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookingSagaData>().HasKey(s => s.CorrelationId);
    }
}
```

##### Where Traveler is an entity repesenting a person who are booking the trip:

```csharp
public class Traveler
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public DateTime BookedOn { get; set; }
}
```

#### Saga State Machine

##### A Saga State Machine in the context of MassTransit and .NET is used to manage the state and coordination of a long-running process that involves multiple services.

##### A Saga ensures that the process completes successfully or compensates for any failures by managing the state transitions and interactions between the various services involved.

##### Key Concepts of Saga State Machine

##### **1\. State Machine**: A state machine manages the states and transitions of a saga. It defines the different states a saga can be in and the events that cause transitions between these states.

##### **2\. Saga State**: The state of a saga is persisted to ensure that the saga can recover from failures and continue processing from where it left off.

##### **3\. Events**: Events trigger transitions between states. They are typically messages that indicate something significant has happened in the system.

##### **4\. Commands**: Commands are actions that the saga instructs other services to perform as part of the process.

#### State Machine:

```csharp
public class BookingSaga : MassTransitStateMachine<BookingSagaData>
{
    public State HotelBooking { get; set; }
    public State FlightBooking { get; set; }
    public State CarRenting { get; set; }
    public State BookingCompleting { get; set; }

    public Event<HotelBooked> HotelBooked { get; set; }
    public Event<FlightBooked> FlightBooked { get; set; }
    public Event<CarRented> CarRented { get; set; }
    public Event<BookingCompleted> BookingCompleted { get; set; }

    public BookingSaga()
    {
        InstanceState(x => x.CurrentState);

        Event(() => HotelBooked, e => e.CorrelateById(m => m.Message.TravelerId));
        Event(() => FlightBooked, e => e.CorrelateById(m => m.Message.TravelerId));
        Event(() => CarRented, e => e.CorrelateById(m => m.Message.TravelerId));

        Initially
        (
            When(HotelBooked)
            .Then(context =>
                {
                    context.Saga.TravelerId = context.Message.TravelerId;
                    context.Saga.HotelName = context.Message.HotelName;
                    context.Saga.FlightCode = context.Message.FlightCode;
                    context.Saga.CarPlateNumber = context.Message.CarPlateNumber;
                })
            .TransitionTo(FlightBooking)
            .Publish(context => new BookFlight(context.Message.TravelerId, context.Message.Email, context.Message.FlightCode, context.Message.CarPlateNumber))
        );

        During(FlightBooking,
            When(FlightBooked)
            .Then(context => context.Saga.FlightBooked = true)
            .TransitionTo(CarRenting)
            .Publish(context => new RentCar(context.Message.TravelerId, context.Message.Email, context.Message.CarPlateNumber))
        );

        During(CarRenting,
            When(CarRented)
            .Then(context =>
                {
                    context.Saga.CarRented = true;
                    context.Saga.BookingFinished = true;
                })
            .TransitionTo(BookingCompleting)
            .Publish(context => new BookingCompleted
                {
                    TravelerId = context.Message.TravelerId,
                    Email = context.Message.Email
                })
            .Finalize());
    }
}
```

#### Commands and Events:

##### **Commands** are messages that represent instructions or actions that need to be performed. They are typically sent by one service to another, directing the receiving service to perform a specific task. Commands are imperative, meaning they tell the recipient exactly what to do.

```csharp
public record BookHotel(string Email, string HotelName, string FlightCode, string CarPlateNumber);

public record BookFlight(Guid TravelerId, string Email, string FlightCode, string CarPlateNumber);

public record RentCar(Guid TravelerId, string Email, string CarPlateNumber);
```

##### **Events** are messages that represent something that has happened within the system. They are declarative, meaning they simply announce that a certain condition or state change has occurred. Events are used to signal other parts of the system that something significant has happened.

```csharp
public class HotelBooked
{
    public Guid TravelerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string HotelName { get; set; } = string.Empty;
    public string FlightCode { get; set; } = string.Empty;
    public string CarPlateNumber { get; set; } = string.Empty;
}

public class FlightBooked
{
    public Guid TravelerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FlightCode { get; set; } = string.Empty;
    public string CarPlateNumber { get; set; } = string.Empty;
}

public class CarRented
{
    public Guid TravelerId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string CarPlateNumber { get; set; } = string.Empty;
}

public class BookingCompleted
{
    public Guid TravelerId { get; set; }
    public string Email { get; set; } = string.Empty;
}
```

#### Commands Handlers:

##### Command Handlers are components that are responsible for handling commands. They contain the logic required to perform the action specified by the command. When a command is sent to a service, the Command Handler processes it, performs the necessary operations, and often triggers subsequent actions or events.

##### There are 4 command handlers:

##### **1\. BookHotelHandler**

##### **2\. BookFlightHandler**

##### **3\. RentCarHandler**

##### **4\. BookingCompletedHandler**

##### Here I will show you handler for hotel booking:

```csharp
public class BookHotelHandler(BookingDbContext _dbContext) : IConsumer<BookHotel>
{
    public async Task Consume(ConsumeContext<BookHotel> context)
    {
        Console.WriteLine($"Booking hotel {context.Message.HotelName} for traveler {context.Message.Email}");

        var traveler = new Traveler
        {
            Id = Guid.NewGuid(),
            Email = context.Message.Email,
            BookedOn = DateTime.Now
        };

        _dbContext.Travelers.Add(traveler);

        await _dbContext.SaveChangesAsync();

        await context.Publish(new HotelBooked
        {
            TravelerId = traveler.Id,
            HotelName = context.Message.HotelName,
            FlightCode = context.Message.FlightCode,
            CarPlateNumber = context.Message.CarPlateNumber
        });
    }
}
```

#### MassTransit and RabbitMq Configurations

##### This configuration sets up MassTransit with RabbitMQ as the transport mechanism and uses Entity Framework for saga state persistence.

##### It registers all consumers from the specified assembly, configures the saga state machine with MSSQL as the database, and sets up RabbitMQ with specific credentials.

##### Endpoint names are formatted in kebab-case for consistency, and an in-memory outbox is used to ensure message consistency and reliability.

##### The overall setup ensures a robust and consistent messaging and saga orchestration infrastructure, enabling reliable handling of complex workflows and state management in a distributed system.

```csharp
public class BookHotelHandler(BookingDbContext _dbContext) : IConsumer<BookHotel>
{
    public async Task Consume(ConsumeContext<BookHotel> context)
    {
        Console.WriteLine($"Booking hotel {context.Message.HotelName} for traveler {context.Message.Email}");

        var traveler = new Traveler
        {
            Id = Guid.NewGuid(),
            Email = context.Message.Email,
            BookedOn = DateTime.Now
        };

        _dbContext.Travelers.Add(traveler);

        await _dbContext.SaveChangesAsync();

        await context.Publish(new HotelBooked
        {
            TravelerId = traveler.Id,
            HotelName = context.Message.HotelName,
            FlightCode = context.Message.FlightCode,
            CarPlateNumber = context.Message.CarPlateNumber
        });
    }
}
```

### Triggering Saga

##### The first step in the transaction queue is to create a hotel reservation. In order to do this, it is necessary to publish the BookHotel command on the MassTransit bus, which will trigger the Handler and the Saga State machine.

```csharp
[HttpPost("bookTrip")]
public IActionResult BookTrip(BookingDetails bookingDetails)
{
    _bus.Publish(new BookHotel(bookingDetails.Email, bookingDetails.HotelName, bookingDetails.FlightCode, bookingDetails.CarPlateNumber));

    return Accepted();
}
```

### Wrapping up

##### Here, on a simple and realistic example, we have shown the application of the Saga pattern for resolving transactions with microservices.

##### The Saga State Machine (orchestrator) is implemented, which manages transactions with the help of the RabbitMq queue.

##### All state changes are remembered in the database, which enables these transactions to be executed for a long time.

##### In the case of distributed transactions with microservice architecture, some of the actions may cause an error.

##### Let's say the hotel and flight are booked, but the service that rents the car threw an error, in that case it is necessary to rollback the previous 2 actions.

##### For this problem, compensatory transactions are implemented that will cancel the booked hotel and flight.

##### That's one of the things you should look at next.

##### The complete code written in .NET 8 can be found in the [following repository](https://github.com/StefanTheCode/SagaPatternDemo/tree/main).

##### That's all from me today.

### **There are 3 ways I can help you:**

#### My Design Patterns Ebooks

1. [Design Patterns that Deliver](/design-patterns-that-deliver-ebook?utm_source=website)

This isn’t just another design patterns book. Dive into real-world examples and practical solutions to real problems in real applications.[Check out it here.](/design-patterns-that-deliver-ebook?utm_source=website)

1. [Design Patterns Simplified](/design-patterns-simplified?utm_source=website)

Go-to resource for understanding the core concepts of design patterns without the overwhelming complexity. In this concise and affordable ebook, I've distilled the essence of design patterns into an easy-to-digest format. It is a Beginner level. [Check out it here.](/design-patterns-simplified?utm_source=website)

#### [Join TheCodeMan.net Newsletter](/)

Every Monday morning, I share 1 actionable tip on C#, .NET & Arcitecture topic, that you can use right away.

#### [Sponsorship](/sponsorship)

Promote yourself to 17,150+ subscribers by sponsoring this newsletter.

### Become a better developer every week
with my newsletter

Every Monday morning, start the week with a cup of coffee and 1 actionable .NET tip that you can use right away.