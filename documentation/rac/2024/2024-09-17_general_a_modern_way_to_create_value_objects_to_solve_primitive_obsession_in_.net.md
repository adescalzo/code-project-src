```yaml
---
title: A Modern Way to Create Value Objects to Solve Primitive Obsession in .NET
source: https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net
date_published: 2024-09-17T11:00:20.576Z
date_captured: 2025-08-06T17:28:35.131Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, ValueOf, Entity Framework Core]
programming_languages: [C#]
tags: [value-objects, primitive-obsession, domain-driven-design, dotnet, csharp, records, record-structs, entity-framework-core, software-architecture, clean-code]
key_concepts: [Primitive Obsession, Value Objects, Domain-Driven Design, Immutability, Value Equality, Self-validation, Data Encapsulation, ORM Mapping]
code_examples: false
difficulty_level: intermediate
summary: |
  This article addresses the "Primitive Obsession" anti-pattern, explaining how using basic data types for complex concepts leads to unclear, error-prone, and hard-to-maintain code. It introduces Value Objects as a solution, highlighting their characteristics like immutability, value-based equality, and self-validation. The post demonstrates implementing Value Objects in .NET using the `ValueOf` library, C# records, and `readonly record structs`, providing practical code examples. It also covers how to correctly map these Value Objects with Entity Framework Core and integrate them with request/response DTO models, promoting cleaner and more robust domain models.
---
```

# A Modern Way to Create Value Objects to Solve Primitive Obsession in .NET

![A banner image with a dark background and abstract blue shapes. On the left, a white icon with `</>` inside and the text "dev tips" below it. On the right, large white text reads "A MODERN WAY CREATE VALUE OBJECTS TO SO PRIMITIVE OBSE IN .NET".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Farchitecture%2Fcover_architecture_value_objects.png&w=3840&q=100)

# A Modern Way to Create Value Objects to Solve Primitive Obsession in .NET

Sep 17, 2024

[Download source code](/source-code/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net)

8 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

**Primitive obsession** is a tendency to use basic data types to represent more complex concepts. It is a common anti-pattern that can lead to unclear code and harder-to-maintain systems.

In this blog post, I will explain to you why primitive obsession can lead to problems in your applications and how to address this issue using Value Objects in .NET.

## Why Is Primitive Obsession a Problem?

**Primitive obsession** occurs when basic data types (such as int, string, or DateTime) are overused to represent complex concepts in your domain. This practice can lead to unclear code, bugs, and difficulty in maintaining and extending your system.

**Why Is Primitive Obsession a Problem?**

*   **Lack of Expressiveness:** primitive types like int or string do not clarify the meaning of the data they represent. For instance, a string could be an email address, a username, or an URL, but there's no way to tell just by looking at the type.
*   **Increased Risk of Errors:** when you use the same primitive type for different concepts, it's easy to pass incorrect data. For example, using a string for both a username and an email address could lead to mistakenly passing an email where a username is expected.
*   **Scattered Validation Logic:** validation for primitive types often ends up scattered across the codebase. Every time you need to check that a string is a valid email address, you need to write the validation logic, which can lead to duplication and inconsistencies.
*   **Difficulty in Evolving the Code:** as your application grows, requirements might change. If you've used primitive types everywhere, it becomes difficult to make these changes in a consistent and non-breaking way.

## Examples of Primitive Obsession

Let's explore an example where a `User` entity has `Email` property. When creating a user, you might implement a validation when creating a `User` object.

You may argue why you need such a validation as you have input validation for your webapi requests. But the truth is that validation for input requests won't save you from a bug in the code where you pass or map wrong parameters in other components of your application. Such validation is a popular way in Domain Driven Design, where you create a final safety guard when creating your objects.

Let's explore an example (with simplified email validation, non-production ready):

```csharp
public class User
{
    public string Email { get; set; }

    public User(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            throw new ArgumentException("Invalid email address", nameof(email));
        }
        
        Email = email;
    }
}
```

**Why this is a problem?**

*   The Email property is just a string, so it can hold any kind of string, not just valid email addresses.
*   Validation logic is embedded in the constructor, making it hard to reuse elsewhere where you have emails.
*   If another part of the codebase needs to handle emails, the validation logic might need to be repeated.

Let's explore another example:

```csharp
public class User
{
    public string Email { get; }
    public string Username { get; }

    public User(string email, string username)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required", nameof(email));
        }
        
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username is required", nameof(username));
        }

        Email = email;
        Username = username;
    }
}
```

Let's try to create a user:

```csharp
var user = new User("anton@test.com", "anton");
var user = new User("anton", "anton@test.com");
```

This code compiles and executes successfully. Did you notice a problem?

As we use `string` type for both email and username - you can pass parameters in the wrong order. And if you don't have a validation when constructing objects, like I showed you above, you will end up with wrong data in the database. This can lead to serious problems.

The solution to this problem is to use **Value Objects**, which encapsulate related data and behavior into a single, meaningful unit.

## What Are Value Objects?

**Value Objects** represent a value in your domain that has no identity but is defined by its attributes. For example, an `Address` might be a Value Object, as it is defined by its properties (Street, City, Zip).

Value Objects are a key concept in Domain-Driven Design (DDD).

**Key characteristics of Value Objects:**

*   Immutability: once created, a Value Object cannot be changed. Any modification results in a new instance.
*   Equality: value Objects are compared based on their properties, not by reference.
*   Self-validation: they ensure that their state is always valid by enforcing constraints on their properties.

**Benefits of Using Value Objects:**

*   **Expressiveness:** code becomes more readable and self-explanatory.
*   **Encapsulation:** business rules are encapsulated within the Value Object, ensuring consistency in every place they are used.
*   **Reducing Bugs:** by limiting the scope of primitive types, you reduce the risk of passing incorrect data.
*   **Reusability:** value Objects can be reused across different parts of the application, promoting DRY (Don't Repeat Yourself) principle.

Now let's explore what options do we have for creating Value Objects in .NET.

## An Example Application

Today I'll show you how to implement **Value Objects** for a **Shipping Application** that is responsible for creating and updating customers, orders and shipments for ordered products.

This application has the following entities:

*   Customers
*   Orders, OrderItems
*   Shipments, ShipmentItems

I am using Domain Driven Design practices for my entities. Let's explore a `Customer` and `Shipment` entities that use primitive types for all the properties:

```csharp
public class Customer
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

    private readonly List<Order> _orders = [];

    private Customer() { }

    public static Customer Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber
        };
    }

    public void AddOrder(Order order)
    {
        _orders.Add(order);
    }
}
```
```csharp
public class Shipment
{
    private readonly List<ShipmentItem> _items = [];

    public Guid Id { get; private set; }

    public string Number { get; private set; }

    public Guid OrderId { get; private set; }

    public string Address { get; private set; }

    public string Carrier { get; private set; }

    public string ReceiverEmail { get; private set; }

    public ShipmentStatus Status { get; private set; }

    public IReadOnlyList<ShipmentItem> Items => _items.AsReadOnly();

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private Shipment()
    {
    }

    public static Shipment Create(
        string number,
        Guid orderId,
        string address,
        string carrier,
        string receiverEmail,
        List<ShipmentItem> items)
    {
        var shipment = new Shipment
        {
            Id = Guid.NewGuid(),
            Number = number,
            OrderId = orderId,
            Address = address,
            Carrier = carrier,
            ReceiverEmail = receiverEmail,
            Status = ShipmentStatus.Created,
            CreatedAt = DateTime.UtcNow
        };

        shipment.AddItems(items);

        return shipment;
    }

    public void AddItems(List<ShipmentItem> items)
    {
        _items.AddRange(items);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddItem(ShipmentItem item)
    {
        _items.Add(item);
        UpdatedAt = DateTime.UtcNow;
    }
}
```

These entities are obsessed with primitive types: shipment number, address, email addresses, phone number, etc. Let's explore how to replace these primitive types with Value Objects.

## Creating Value Objects in .NET

I can list the following most popular options for creating value objects:

*   using [ValueOf](https://github.com/mcintyre321/ValueOf) library
*   using C# Records
*   using C# Record Structs

Let's explore each option more in-depth.

### Creating Value Objects with ValueOf

The [ValueOf](https://github.com/mcintyre321/ValueOf) is one of the popular libraries for creation of value objects by providing a base class that handles much of the boilerplate code.

First, you need to install the package:

```bash
dotnet add package ValueOf
```

Here's how you can use the `ValueOf` library to create the value objects:

```csharp
using ValueOf;

public class EmailAddress : ValueOf<string, EmailAddress>
{
    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Value) || !Value.Contains("@"))
        {
            throw new ArgumentException("Invalid email address.");
        }
    }
}

public class OrderNumber : ValueOf<string, OrderNumber>
{
    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Value))
        {
            throw new ArgumentException("Order number cannot be empty.");
        }
    }
}
```

You need to inherit your Value Object (EmailAddress and OrderNumber) class from a base `ValueOf` class and provide 2 generic types:

*   an underline primitive type
*   a Value Object type itself

`ValueOf` support C# tuples, if you need to represent your value object as multiple properties, for example, Address:

```csharp
using ValueOf;

public class Address : ValueOf<(string Street, string City, string Zip), Address>
{
    protected override void Validate()
    {
        if (string.IsNullOrWhiteSpace(Value.Street))
        {
            throw new ArgumentException("Street cannot be empty.");
        }
        
        if (string.IsNullOrWhiteSpace(Value.City))
        {
            throw new ArgumentException("City cannot be empty.");
        }
        
    if (string.IsNullOrWhiteSpace(Value.Zip))
        {
            throw new ArgumentException("Zip code cannot be empty.");
        }
    }
}
```

Here is how you can create these value objects:

```csharp
var email = EmailAddress.From("anton@test.com");
var orderNumber = OrderNumber.From("ORD-12345");
var address = Address.From(("123 Main St", "Springfield", "12345"));
```

You can use a `Value` property to retrieve a value hidden inside value objects:

```csharp
string emailValue = email.Value;
string orderNumberValue = orderNumber.Value;
(string street, string city, string zip) = address.Value;
```

### Creating Value Objects with Records

Some developer implements Value Objects using `ValueOf` library, even more create their own implementations. What if I tell you that C# **records** already have all you need for value objects.

Records are a really modern-way to create Value Object in .NET.

Records are immutable reference types and their support equality comparison out of the box. They are compared based on their properties, not by reference. Records also have a ready "ToString" method out of the box, that outputs all the properties in a readable way.

Here's how you can define the same value objects using records:

```csharp
public record EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
        {
            throw new ArgumentException("Invalid email address.", nameof(value));
        }

        Value = value;
    }
}
```

If you don't need validation inside `EmailAddress` and other value objects, you can simplify this to:

```csharp
public record EmailAddress(string Value);
public record OrderNumber(string Value);
public record Address(string Street, string City, string Zip);
```

Single line of code, magnificent.

### Creating Value Objects with Record Structs

Records are a wonderful choice for value objects, but they are reference types. If you care about memory allocations, you can use `readonly record structs` for **Value Objects**. They behave the same as `records` but they are value types and not allocated on the heap.

This is my personal choice for creating Value Objects.

Here is how you can define the Value Objects with `record structs`:

```csharp
public readonly record struct EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
        {
            throw new ArgumentException("Invalid email address.", nameof(value));
        }

        Value = value;
    }
}
```

Or in a more concise form:

```csharp
public readonly record struct EmailAddress(string Value);
public readonly record struct OrderNumber(string Value);
public readonly record struct Address(string Street, string City, string Zip);
```

Here is how the `Customer` entity will look like with Value Objects:

```csharp
public class Customer
{
	public Guid Id { get; private set; }
	public FirstName FirstName { get; private set; }
	public LastName LastName { get; private set; }
	public EmailAddress Email { get; private set; }
	public PhoneNumber PhoneNumber { get; private set; }
	public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

	private readonly List<Order> _orders = [];

	private Customer() { }

	public static Customer Create(
		FirstName firstName,
		LastName lastName,
		EmailAddress email,
		PhoneNumber phoneNumber)
	{
		return new Customer
		{
			Id = Guid.NewGuid(),
			FirstName = firstName,
			LastName = lastName,
			Email = email,
			PhoneNumber = phoneNumber
		};
	}

	public void AddOrder(Order order)
	{
		_orders.Add(order);
	}
}
```

## Mapping Value Objects in EF Core

After introducing Value Objects in your entity models, you need to modify your EF Core Mapping. You can't longer use the usual mapping like this:

```csharp
builder.Property(x => x.FirstName).IsRequired();
builder.Property(x => x.LastName).IsRequired();
builder.Property(x => x.Email).IsRequired();
builder.Property(x => x.PhoneNumber).IsRequired();
```

You need to use conversion to tell EF Core how to map Value Object to the database, and how to map database values to ValueObjects:

```csharp
builder.Property(x => x.Email)
    .HasConversion(
        email => email.Value,
        value => new EmailAddress(value)
    )
    .IsRequired();

builder.Property(x => x.PhoneNumber)
    .HasConversion(
        phoneNumber => phoneNumber.Value,
        value => new PhoneNumber(value)
    )
    .IsRequired();
```

It requires a bit more code, but Value Objects give you a lot of advantages.

## Value Objects and Request/Response/DTO models

Value Object is your domain-specific models, the outside world should not know about them. And moreover, your public request/response/DTO models should be as simple as possible.

It is a good practice to have plain primitives types in your request/response/DTO models and map them to domain Value Object and vice versa.

For example, I am doing the mapping in my "Create Customer" use case:

```csharp
var customer = Customer.Create(
    firstName: new FirstName(request.FirstName),
    lastName: new LastName(request.LastName),
    email: new EmailAddress(request.Email),
    phoneNumber: new PhoneNumber(request.PhoneNumber)
);

await customerRepository.AddAsync(customer, cancellationToken);
await unitOfWork.SaveChangesAsync(cancellationToken);
```

And the reverse mapping to `CustomerResponse` from Value Objects:

```csharp
internal static class MappingExtensions
{
    public static CustomerResponse MapToResponse(this Customer customer)
    {
        return new CustomerResponse(
            CustomerId: customer.Id,
            FirstName: customer.FirstName.Value,
            LastName: customer.LastName.Value,
            Email: customer.Email.Value,
            PhoneNumber: customer.PhoneNumber.Value
        );
    }
}
```

## Summary

If you have ever experienced bugs in your code where wrong values reached the database (or any other source) - consider using Value Objects in your projects. C# records and readonly record structs provide you an elegant, easy and fast way to implement Value Objects without boilerplate code.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net&title=A%20Modern%20Way%20to%20Create%20Value%20Objects%20to%20Solve%20Primitive%20Obsession%20in%20.NET)[X](https://twitter.com/intent/tweet?text=A%20Modern%20Way%20to%20Create%20Value%20Objects%20to%20Solve%20Primitive%20Obsession%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-modern-way-to-create-value-objects%2Fblog%2Fa-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.