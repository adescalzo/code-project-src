```yaml
---
title: "From Anemic Models to Behavior-Driven Models: A Practical DDD Refactor in C#"
source: https://www.milanjovanovic.tech/blog/from-anemic-models-to-behavior-driven-models-a-practical-ddd-refactor-in-csharp?utm_source=newsletter&utm_medium=email&utm_campaign=tnw141
date_published: 2025-05-10T00:00:00.000Z
date_captured: 2025-08-06T18:18:48.784Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [.NET, Entity Framework Core, OpenSearch, Elasticsearch, ASP.NET Core]
programming_languages: [C#, SQL]
tags: [ddd, refactoring, csharp, dotnet, software-architecture, clean-architecture, object-oriented-programming, legacy-code, design-patterns, domain-modeling]
key_concepts: [Anemic Domain Model, Behavior-Driven Models, Domain-Driven Design (DDD), Aggregate, Factory Method, Encapsulation, "Tell Don't Ask", Dependency Injection]
code_examples: false
difficulty_level: intermediate
summary: |
  This article addresses the common problem of anemic domain models in C# legacy codebases, where business logic is scattered across service classes rather than residing within domain entities. It presents a practical, step-by-step refactoring guide to transform a "god-like" `OrderService` into a more behavior-rich `Order` aggregate. The refactoring demonstrates how to embed business rules, such as stock validation and discount application, directly into the domain model using a static factory method and by guarding internal state. This approach significantly reduces coupling, simplifies application services, and enhances testability and maintainability by making business rules explicit and isolated.
---
```

# From Anemic Models to Behavior-Driven Models: A Practical DDD Refactor in C#

![Cover image for the article, showing "DDD Refactor: Anemic Model -> Rich Behaviors" on a blue and black background.](blog-covers/mnw_141.png)

# From Anemic Models to Behavior-Driven Models: A Practical DDD Refactor in C#

6 min read · May 10, 2025

If you've ever worked with a legacy C# codebase, you know the pain of an anemic domain model. You have probably opened an `OrderService` (_all similarities to production code are merely a coincidence_) and thought _"this file does everything."_ Pricing logic, discount rules, stock checks, database writes — **all jam-packed into one class**. It works — until it doesn't. New features turn into **regression roulette**, and test coverage plummets because the domain is buried under infrastructure.

This is the classic symptom of an anemic domain model, where entities are nothing but data holders, and all logic lives elsewhere. It makes the system harder to reason about, and every change becomes a guessing game. But what if we could push behavior back into the domain, one rule at a time?

In this article, we'll:

1.  **Inspect** a typical anemic implementation.
2.  **Identify** hidden business rules that make it brittle.
3.  **Refactor** toward a behavior-rich aggregate one refactor at a time.
4.  **Highlight** the concrete payoffs so you can justify the change to teammates.

Everything fits in a 6-minute read, but the pattern scales to any legacy system.

## Starting Point: God-like Service Class

Below is an (unfortunately common) `OrderService`. Besides calculating totals it also:

*   applies a **5 % VIP discount**,
*   throws if any product is **out of stock**, and
*   rejects orders that would **exceed the customer's credit limit**.

```csharp
// OrderService.cs
public void PlaceOrder(Guid customerId, IEnumerable<OrderItemDto> items)
{
    var customer = _db.Customers.Find(customerId);
    if (customer is null)
    {
        throw new ArgumentException("Customer not found");
    }

    var order = new Order { CustomerId = customerId };

    foreach (var dto in items)
    {
        var inventory = _inventoryService.GetStock(dto.ProductId);
        if (inventory < dto.Quantity)
        {
            throw new InvalidOperationException("Item out of stock");
        }

        var price = _pricingService.GetPrice(dto.ProductId);
        var lineTotal = price * dto.Quantity;
        if (customer.IsVip)
        {
            lineTotal *= 0.95m; // 5% discount for VIPs
        }

        order.Items.Add(new OrderItem
        {
            ProductId = dto.ProductId,
            Quantity = dto.Quantity,
            UnitPrice = price,
            LineTotal = lineTotal
        });
    }

    order.Total = order.Items.Sum(i => i.LineTotal);

    if (customer.CreditUsed + order.Total > customer.CreditLimit)
    {
        throw new InvalidOperationException("Credit limit exceeded");
    }

    _db.Orders.Add(order);
    _db.SaveChanges();
}
```

### What's Wrong Here?

*   **Scattered rules:** Discount application, stock validation, and credit-limit checks are buried inside the service.
*   **Tight coupling:** `OrderService` must know about pricing, inventory, and EF Core just to place an order.
*   **Painful testing:** Each unit test needs fakes for DB access, pricing, inventory, and VIP vs. non-VIP flows.

**Goal:** Embed these rules **inside the domain** so the application layer only deals with orchestration.

## Guiding Principles Before We Touch Code

1.  **Protect invariants close to the data.** Stock, discounts, and credit checks belong where the data lives — inside the `Order` aggregate.
2.  **Expose intent, hide mechanics.** The application layer should read like a story: _"place order"_, not _"calculate totals, check credit, write to DB"_.
3.  **Refactor in slices.** Each move is safe and compilable; no big-bang rewrites.
4.  **Balance purity with pragmatism.** Move rules only when the payoff (clarity, safety, testability) beats the extra lines of code.

## Step-by-Step Refactor

The goal here isn't to chase purity or academic DDD. It's to incrementally improve cohesion and make room for the domain to express itself.

At every step, we ask: Is this behavior something the domain should own? If yes, we pull it inward.

### Embed Creation & Validation Logic

The first move is to make the aggregate responsible for building itself. A static `Create` method gives us a single entry point where all invariants can fail fast.

While pushing stock validation into `Order` improves testability, it does couple the order flow with inventory availability. In some domains, you'd instead model this as a domain event and validate asynchronously.

```csharp
// Order.cs (Factory Method)
public static Order Create(
    Customer customer,
    IEnumerable<(Guid productId, int quantity)> lines,
    IPricingService pricingService,
    IInventoryService inventoryService)
{
    var order = new Order(customer.Id);

    foreach (var (productId, quantity) in lines)
    {
        if (inventoryService.GetStock(productId) < quantity)
        {
            throw new InvalidOperationException("Item out of stock");
        }

        var unitPrice = pricingService.GetPrice(productId);
        order.AddItem(productId, quantity, unitPrice, customer.IsVip);
    }

    order.EnsureCreditWithinLimit(customer);

    return order;
}
```

**Why?** Creation now **fails fast** if any invariant is broken. The service no longer micromanages stock or discounts.

Notice how we're now following the "Tell, Don't Ask" principle. Rather than the service checking conditions and then manipulating the Order, we're telling the Order to create itself with the necessary validations built in. This is a fundamental shift toward **encapsulation**.

**💡 On Injecting Services into Domain Methods**

Passing services like `IPricingService` or `IInventoryService` into a domain method such as `Order.Create` might seem unconventional at first glance. But it's a deliberate design choice: it keeps the orchestration inside the domain model, where the business logic naturally belongs, instead of bloating the application service with procedural workflows.

This approach maintains the entity's autonomy while still aligning with dependency injection principles — dependencies are passed explicitly, not resolved from within. It's a powerful technique, but one that should be used selectively — only when the operation clearly fits within the domain's responsibility and benefits from direct access to external services.

### Guard the Aggregate's Internal State

```csharp
// Order.cs (excerpt)
private readonly List<OrderItem> _items = new();
public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly(); // C# 12 -> [.._items]

private void AddItem(Guid productId, int quantity, decimal unitPrice, bool isVip)
{
    if (quantity <= 0)
    {
        throw new ArgumentException("Quantity must be positive");
    }

    var finalPrice = isVip ? unitPrice * 0.95m : unitPrice;
    _items.Add(new OrderItem(productId, quantity, finalPrice));

    RecalculateTotal();
}

private void EnsureCreditWithinLimit(Customer customer)
{
    if (customer.CreditUsed + Total > customer.CreditLimit)
    {
        throw new InvalidOperationException("Credit limit exceeded");
    }
}
```

**Why bother?**

*   **Encapsulation**: Consumers can't mutate `_items` directly, ensuring invariants hold.
*   **Self-protection**: The domain model protects its own consistency rather than relying on service-level checks.
*   **True OOP**: Objects now combine data and behavior, as object-oriented programming intended.
*   **Simpler services**: Application services can focus on coordination rather than business rules.

### Shrink the Application Layer to Pure Orchestration

```csharp
public void PlaceOrder(Guid customerId, IEnumerable<OrderLineDto> lines)
{
    var customer = _db.Customers.Find(customerId);
    if (customer is null)
    {
        throw new ArgumentException("Customer not found");
    }
    var input = lines.Select(l => (l.ProductId, l.Quantity));

    var order = Order.Create(customer, input, _pricingService, _inventoryService);

    _db.Orders.Add(order);
    _db.SaveChanges();
}
```

The `PlaceOrder` method drops from **44 lines** to **14**, with **zero business logic**.

## What We Gained

**Before the refactor**

*   Service owned pricing, stock, discount, and credit checks.
*   Unit tests required heavy EF Core and service fakes.
*   Adding a new rule meant touching multiple files.

**After the refactor**

*   Aggregate owns all business rules; service only orchestrates.
*   Pure domain tests — no database container required.
*   Most changes are isolated to the `Order` aggregate.

## Wrapping Up

The real value in refactoring anemic models isn't technical — it's strategic.

By moving business logic closer to the data, you:

*   Reduce the blast radius of changes
*   Make business rules explicit and testable
*   Open the door for tactical patterns like validation, events, and invariants

But you don't need a big rewrite. Start with one rule. Refactor it. Then the next.

That's how legacy systems evolve into maintainable architectures.

If you enjoyed this breakdown and want a hands-on, real-world guide to untangling messy services, check out my course **Domain-Driven Design Refactoring**. It's packed with before-and-after examples like this one.

---