```yaml
---
title: "3 DDD patterns you'll regret ignoring in 2 years - andresdescalzo@gmail.com - Gmail"
source: https://mail.google.com/mail/u/0/#inbox/FMfcgzQcpdlBgFWvvTqnpGbLhWJNMqzn
date_published: unknown
date_captured: 2025-09-05T12:40:32.101Z
domain: mail.google.com
author: Unknown
category: ai_ml
technologies: [.NET, Entity Framework, Moq, NSubstitute]
programming_languages: [C#]
tags: [ddd, design-patterns, software-architecture, refactoring, unit-testing, domain-model, specification-pattern, domain-events, dotnet, clean-code]
key_concepts: [Domain Driven Design, Rich Domain Model, Specification Pattern, Domain Events, Service Layer, Unit Testing, Decoupling, Business Logic]
code_examples: false
difficulty_level: intermediate
summary: |
  This article addresses the common pitfall of centralizing all business logic within service classes, leading to messy code, difficult unit testing, and tangled side effects. It proposes three Domain Driven Design (DDD) patterns—Rich Domain Model, Specification Pattern, and Domain Events—as solutions. The Rich Domain Model advocates for placing business logic directly within domain entities for easier testing and reuse. The Specification Pattern helps encapsulate and test complex query criteria independently of the database context. Finally, Domain Events promote decoupling side effects from core domain logic, simplifying service classes and improving testability. The author illustrates these concepts with practical C# code examples, emphasizing their benefits for .NET projects.
---
```

# 3 DDD patterns you'll regret ignoring in 2 years - andresdescalzo@gmail.com - Gmail

## 3 DDD patterns you'll regret ignoring in 2 years

### Kristijan Kralj

One of the most common mistakes I see when joining existing projects:

Developers put all business logic inside service classes.

But here is the thing:

Service classes should orchestrate, not decide.

Ok, that’s a fancy sentence. But it doesn’t help.

So, let me clear that up a bit.

Putting all business logic, and everything else, inside Handler/Service classes may look fine at first. Because you are moving fast and finishing features one after another.

But after a while, the problems appear:

*   Service classes become bigger and messier.
*   There is no way to write simple unit tests for the logic. Instead, you need to write integration tests, which are slow, or write unit tests that abuse mocking libraries harder than a junior dev abuses `Console.WriteLine` as their main logging strategy.
*   Side effects, such as sending emails, background jobs, HTTP, or messaging concerns, blend with the domain logic. This adds another level of complexity.

To combat those problems, I use 3 DDD (Domain Driven Design) patterns.

You are probably thinking now:

_“Oh, great, so you tackle complexity by adding even more complexity with DDD? Thanks, but no thanks. I don’t need Aggregate Roots, Bounded Context, or any other of those fancy patterns that half of devs don’t understand, and the other half use, but don’t know why they use it.”_

And immediately after that, move this email into the trash folder.

Well, not quite.

Yes, DDD is a complex topic.

And that complexity is better saved for large projects.

However, there are 3 DDD patterns that can help you untangle the mess you are starting to get drawn into.

They are:

1.  **Rich domain model** - BENEFIT: push business logic to domain classes so you can test it without mocking.
2.  **Specification pattern** - BENEFIT: easily reuse and unit test filtering logic that contains business rules.
3.  **Domain events** - BENEFIT: decouple main processing from side effects (sending emails, background jobs), so the handler classes have fewer dependencies and are simpler.

Let me walk you through code examples, so it’s easier to understand their benefits.

**Side note:** The code snippets and the lesson below come from my upcoming **“Zero to Architect Playbook: The .NET developer’s toolbox for making confident software architecture decisions”** course.

The course will give you everything you need to confidently make software architecture decisions in real-world .NET projects - using reusable checklists, architecture blueprints, cheatsheets, and decision-making tools you can apply to your current or next project.

If that sounds interesting, [join the waitlist here](https://click.convertkit-mail2.com/75u5p3ne80h8h6oxwp8fzhwl85666tnhox6ov/rehmdw3kh9umq6vnl2b2/aHR0cHM6Ly9scC5kZXZzZWNyZXRzLm5ldC9zdWNjZXNz).

## Rich domain model

In the simplest terms, a rich domain model involves placing business logic that operates on data into classes that hold that data.

For example, if you have a service class that updates the invoice status:

![C# code demonstrating a service method that handles invoice status updates, mixing business logic, data access, and side effects like email sending.](https://i.imgur.com/vHq4X2x.png)
```csharp
public async Task<Result> Execute(long invoiceId, string statusString, string userId)
{
    // Validate input status string
    if (!Enum.TryParse<InvoiceStatus>(statusString, true, out var newStatus))
    {
        return Result.Error($"Invalid invoice status: {statusString}");
    }

    // Check if the invoice exists and belongs to the user
    var invoice = await _dbContext.Set<Invoice>()
        .FirstOrDefaultAsync(x => x.Id == invoiceId && x.AppUserId == userId);
    if (invoice == null)
    {
        return Result.NotFound("Invoice not found");
    }

    // Allow staying in the same status (no-op)
    if (invoice.Status == newStatus)
    {
        return Result.Success();
    }

    // Validate status transition
    if (IsValidStatusTransition(invoice.Status, newStatus))
    {
        return Result.Error($"Cannot transition from {invoice.Status} to {newStatus}.");
    }

    // Update the invoice status
    invoice.Status = newStatus;
    await _dbContext.SaveChangesAsync();

    // Send email if the new status is sent
    if (newStatus == InvoiceStatus.Sent)
    {
        await _emailSender.SendInvoiceEmail(invoice.Id);
    }

    return Result.Success();
}
```

You’ll notice that:

*   Business rules (the criteria for updating invoice status) are tangled between validation logic, DB querying, and email sending
*   To write tests, you need to figure out how to mock DBContext and use Moq/NSubstitute for IEmailSender

But when you push that status change logic to the Invoice class:

![C# code illustrating a Rich Domain Model where the UpdateStatus business logic is encapsulated within the Invoice domain class.](https://i.imgur.com/eBw017l.png)
```csharp
public Result UpdateStatus(InvoiceStatus newStatus)
{
    // Allow staying in the same status (no-op)
    if (Status == newStatus)
    {
        return Result.Success();
    }

    // Validate status transition
    if (IsValidStatusTransition(Status, newStatus))
    {
        return Result.Error($"Cannot transition from {Status} to {newStatus}.");
    }

    // Update the invoice status
    Status = newStatus;
    return Result.Success();
}
```

The unit tests are stupidly simple to write:

![C# unit test for the Invoice.UpdateStatus method, demonstrating simplified testing due to the Rich Domain Model.](https://i.imgur.com/y3y303D.png)
```csharp
[Fact]
public void Status_change_is_changed_to_new_one_for_valid_transition()
{
    var invoice = new Invoice
    {
        Status = InvoiceStatus.Draft
    };

    var result = invoice.UpdateStatus(InvoiceStatus.Sent);

    result.IsSuccess.ShouldBeTrue();
    invoice.Status.ShouldBe(InvoiceStatus.Sent);
}
```

No need to mock anything.

And if you ever need to reuse that logic, you can easily call it with _invoice.UpdateStatus_.

**Rule of thumb:**

It’s okay to start with business logic in service classes. In fact, that’s the situation I run into when joining the latest project. But once the rules got more complicated, I started moving the logic into domain classes.

## Specification pattern

The specification pattern is a way to put complex query logic in a separate class, so that you can easily reuse it and test it.

Here’s a code snippet to filter invoices by search term:

![C# code snippet showing complex invoice filtering logic directly integrated with a database context, illustrating the problem the Specification Pattern solves.](https://i.imgur.com/eQ4m1kY.png)
```csharp
var query = _dbContext.Set<Invoice>()
    .Where(x => x.AppUserId = userId);

if (!string.IsNullOrWhiteSpace(search))
{
    var searchLower = search.ToLower();
    query = query.Where(x =>
        x.InvoiceNumber.ToString().Contains(searchLower) ||
        x.Customer.Name.Contains(searchLower) ||
        x.Customer.Email.Contains(searchLower) ||
        x.Status.ToString().Contains(searchLower) ||
        x.Items.Any(i => i.Product.Name.Contains(searchLower)));
}

var totalCount = await query.CountAsync();
```

Again, the criteria for searching for invoices come from a business perspective. But it’s implemented in a way that’s intertwined with the DB query logic.

However, with the specification pattern, you put that business decision in a class that doesn’t depend on a DB context:

![C# code defining an InvoiceSearchSpecification class, demonstrating the Specification Pattern for encapsulating complex query criteria.](https://i.imgur.com/7s2j402.png)
```csharp
public class InvoiceSearchSpecification: SpecificationPattern<Invoice>
{
    public InvoiceSearchSpecification(string search)
    {
        var searchLower = search.ToLower();
        Criteria = invoice =>
            invoice.InvoiceNumber.ToString().Contains(searchLower) ||
            invoice.Customer.Name.Contains(searchLower) ||
            invoice.Customer.Email.Contains(searchLower) ||
            invoice.Status.ToString().ToLower().Contains(searchLower) ||
            invoice.Items.Any(item => item.Product.Name.ToLower().Contains(searchLower));
    }
}
```

And now, unit test focuses on testing the business requirements:

![C# unit test for the InvoiceSearchSpecification, verifying its filtering logic independently of a database.](https://i.imgur.com/jW0j9x1.png)
```csharp
[Fact]
public void InvoiceSearchSpecification_FiltersCorrectly()
{
    var specification = new InvoiceSearchSpecification("test");
    var invoices = new List<Invoice>
    {
        new Invoice { InvoiceNumber = "INV-001", Customer = new Customer { Name = "Test Customer" } },
        new Invoice { InvoiceNumber = "INV-002", Customer = new Customer { Name = "Another Customer" } }
    }.AsQueryable();

    var filteredInvoices = invoices.Where(specification.Criteria).ToList();

    filteredInvoices.ShouldContain(i => i.InvoiceNumber == "INV-001");
    filteredInvoices.ShouldNotContain(i => i.InvoiceNumber == "INV-002");
}
```

## Domain events

Domain events are a way to express that something important has happened in the business domain.

For example, sending emails when some state changes.

Instead of mixing side effects (like sending an email) directly into your service methods, you raise an event and let other parts of the system react to it.

If you take a look again at the first code snippet of this email, the method ends with sending an email when the invoice status changes to Sent:

![C# code demonstrating a service method that handles invoice status updates, mixing business logic, data access, and side effects like email sending.](https://i.imgur.com/vHq4X2x.png)
```csharp
public async Task<Result> Execute(long invoiceId, string statusString, string userId)
{
    // Validate input status string
    if (!Enum.TryParse<InvoiceStatus>(statusString, true, out var newStatus))
    {
        return Result.Error($"Invalid invoice status: {statusString}");
    }

    // Check if the invoice exists and belongs to the user
    var invoice = await _dbContext.Set<Invoice>()
        .FirstOrDefaultAsync(x => x.Id == invoiceId && x.AppUserId == userId);
    if (invoice == null)
    {
        return Result.NotFound("Invoice not found");
    }

    // Allow staying in the same status (no-op)
    if (invoice.Status == newStatus)
    {
        return Result.Success();
    }

    // Validate status transition
    if (IsValidStatusTransition(invoice.Status, newStatus))
    {
        return Result.Error($"Cannot transition from {invoice.Status} to {newStatus}.");
    }

    // Update the invoice status
    invoice.Status = newStatus;
    await _dbContext.SaveChangesAsync();

    // Send email if the new status is sent
    if (newStatus == InvoiceStatus.Sent)
    {
        await _emailSender.SendInvoiceEmail(invoice.Id);
    }

    return Result.Success();
}
```

Some drawbacks of this:

*   The service class has an additional dependency on IEmailSender
*   If you want to add another side effect (background job, sync to CRM, audit log…), the service method will continue to grow

But when you use domain events, you flip it around:

*   The Invoice class raises an InvoiceStatusSent event

![C# code showing the Invoice domain class raising an InvoiceStatusSent domain event, decoupling the event from its handlers.](https://i.imgur.com/x5D64h3.png)
```csharp
public class Invoice
{
    // ... other properties and methods

    public void UpdateStatus(InvoiceStatus newStatus)
    {
        // ... status update logic ...
        Status = newStatus;

        if (newStatus == InvoiceStatus.Sent)
        {
            AddDomainEvent(new InvoiceStatusSentEvent(Id));
        }
    }

    // Method to add domain events (e.g., to be dispatched later)
    private void AddDomainEvent(IDomainEvent eventItem)
    {
        // ... logic to store event ...
    }
}
```

*   Event handlers listen for that event and perform side effects (send email, publish message, etc.)

![C# code illustrating an event handler for InvoiceStatusSentEvent, demonstrating how side effects like sending emails are handled separately from the domain logic.](https://i.imgur.com/h5E42lA.png)
```csharp
public class InvoiceStatusSentEventHandler : IDomainEventHandler<InvoiceStatusSentEvent>
{
    private readonly IEmailSender _emailSender;

    public InvoiceStatusSentEventHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Handle(InvoiceStatusSentEvent domainEvent)
    {
        await _emailSender.SendInvoiceEmail(domainEvent.InvoiceId);
    }
}
```

*   Your domain logic stays focused on what happened, not what to do about it

This separation means that:

*   New side effects are easier to add
*   Service classes don’t get additional dependencies
*   Unit tests for the domain just assert “event was raised”
*   Unit tests for handlers just assert “when event is handled, email is sent”

To recap, use these 3 DDD patterns:

1.  **Rich domain model** - put rules where data lives, so it’s easier to reuse and test it.
2.  **Specification pattern** - keep complex query criteria.
3.  **Domain events** - separate side effects from the main processing logic.

This will keep your services less bloated, testing less painful, and making new changes easier.

Enjoy your weekend.

Kristijan