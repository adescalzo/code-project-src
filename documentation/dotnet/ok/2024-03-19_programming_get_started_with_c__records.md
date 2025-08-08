```yaml
---
title: Get Started with C# Records
source: https://antondevtips.com/blog/getting-started-with-csharp-records
date_published: 2024-03-19T12:03:18.228Z
date_captured: 2025-08-06T17:14:59.149Z
domain: antondevtips.com
author: Anton Martyniuk
category: programming
technologies: [.NET, C# 9.0, C# 10, C# 11, Entity Framework, WPF, Blazor]
programming_languages: [C#]
tags: [csharp, records, immutability, value-objects, dotnet, language-features, data-structures, object-oriented-programming, programming-guide]
key_concepts: [immutability, value-based-equality, positional-records, init-only-properties, required-keyword, record-structs, data-transfer-objects, domain-driven-design]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive introduction to C# records, a new type introduced in C# 9.0 designed for immutability and value-based equality. It covers both classic and concise (positional) forms of record declaration, including the use of `init` only properties and the C# 11 `required` keyword. The post also explains how records handle immutability with the `with` expression, their unique equality comparison behavior, and support for inheritance. Additionally, it introduces record structs from C# 10 and highlights the useful default `ToString()` implementation. Finally, the article offers practical guidance on when to use records for scenarios like DTOs and value objects, and when to avoid them, such as with Entity Framework or UI data binding.
---
```

# Get Started with C# Records

![Cover image for an article titled 'C# RECORDS GET STARTED' with a 'dev tips' logo, set against a dark background with purple abstract shapes.](https://antondevtips.com/media/covers/csharp/cover_csharp_records.png)

# Getting Started with C# Records

Mar 19, 2024

4 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

## Introduction to C# Records

C# 9.0 introduced a new type of class in called **record**.
Record is a reference type that offers immutability and equality comparison out of the box.

There are 2 forms of defining a record: a classic and concise. Let's have a look on the classic form first:

```csharp
public record Product
{
    public int Id { get; init; }

    public string Name { get; init; }
    
    public decimal Price { get; init; }
}
    
// Create an instance of Product's record
var product = new Product
{
    Id = 1,
    Name = "Phone",
    Price = 500.00
};
```

This type of declaration is similar to regular C# classes, but instead of a **class** keyword - a new **record** keyword is being used. The best practise when creating a record is declaring all of its properties as **init** only. This way properties can't be changed, ensuring the immutability of the record's data. Although you can define properties as a regular **get-set** if needed.

A new keyword called **required** was introduced in C# 11, which is a great addition to records in my opinion:

```csharp
public record Product
{
    public required int Id { get; set; }

    public required string Name { get; set; }
    
    public required decimal Price { get; set; }
}
```

This ensures that all properties marked with **required** keyword, should be assigned when creating an object, otherwise a compilation error is raised:

```csharp
// This code doesn't compile as Price property is not assigned with a value
var product = new Product
{
    Id = 1,
    Name = "Phone"
};
```

## Concise (positional) form of C# Records

C# also provides a shortened form of record declaration:

```csharp
public record Product(int Id, string Name, decimal Price);

var product = new Product(1, "PC", 1000.00m);
```

A single line of code! We assign all the properties using a record's constructor which is called a primary constructor. Under the hood this code is translated to a classic form of a record declaration with all properties being **init** only. This form of record is also called a **positional record** as all properties under the hood are created in the exact order as their position in the primary constructor. Positional form of declaration looks really elegant and concise when a record doesn't have a lot of fields.

## Records immutability

Records in C# are immutable by their nature, you can't change properties of record object. Instead you should use the **when** keyword to create a new instance of record with updated properties:

```csharp
var product = new Product(1, "PC", 1000.00m);
// This code doesn't compile: can't change a record property
product.Price = 1200.00m;
    
// Instead create a new product with updated price 
var product2 = product with
{
    Price = 1200.00m
};
```

## Records equality comparison

A default equality comparison behaviour for different C# type is as follows:

*   **struct**: two objects are equal if they have the same type and store the same values.
*   **class**: two objects are equal if their references in memory are equal.
*   **record**: two objects are equal if they have the same type and store the same values.

Records are reference types but have a value object behaviour when it comes to equality comparison.

```csharp
var product1 = new Product(1, "PC", 1000.00m);
var product2 = new Product(2, "Laptop", 1500.00m);
var product3 = new Product(1, "PC", 1000.00m);

Console.WriteLine(product1 == product2); // prints "false"
Console.WriteLine(product1 == product3); // prints "true" as both records have the same values
```

## Records inheritance

As any other class - records support inheritance. Let's have a look on the following example:

```csharp
public record Product(int Id, string Name, decimal Price);

public record SpecialProduct(int Id, string Name, decimal Price, decimal Discount)
    : Product(Id, Name, Price);
    
var product = new Product(1, "PC", 1000.00m);
var specialProduct = new SpecialProduct(1, "PC", 1000.00m, 100.00m);
```

Primary constructors are utilized here to pass properties to a parent record. When using a classic form of declaration, records look the same as classes:

```csharp
public record SpecialProduct : Product
{
    public SpecialProduct(int id, string name, decimal price, decimal discount)
        : base(id, name, price)
    {
        Discount = discount;
    }
    
    public decimal Discount { get; init; }
}
```

## Record structs

In C# 10 a **records structs** were introduced:

```csharp
public readonly record struct Point(double X, double Y);

public readonly record struct Point
{
    public double X { get; init; }
    
    public double Y { get; init; }
}

public record struct Point
{
    public double X { get; set; }
    
    public double Y { get; set; }
}
```

**Records structs** can be positional structs with primary constructor, have a classic form of declaration and even be declared as **readonly record struct**. **Records structs** have the same behaviour as regular records but are a **value objects**. **NOTE:** A classic record could be also declared using **record class** phrase but this is usually shortened to just a **record**.

## Record's ToString Method

Records out of the box provide a useful implementation of the **ToString** method:

```csharp
var product1 = new Product(1, "PC", 1000.00m);

Console.WriteLine(product1); // Product { Id = 1, Name = PC, Price = 1000,00 }
```

ToString method prints all of values of the record properties which is extremely useful for logging and debugging.

## When To Use Records

Records in C# are a powerful feature and are preferred to be used in the following scenarios:

*   **Data Transfer Objects (DTOs)**: Records are an excellent choice for DTOs in API development due to their immutable nature. They ensure that data objects can't be accidentally modified in any layer of an application.
*   **REPR (Request-Endpoint-Response) pattern**: Records are an excellent choice for request and response models that should not be modified after creation.
*   **Value objects in DDD**: In Domain-Driven Design, records can be used for creation of value objects where immutability and value-based equality are essential.
*   **Read-Only Data**: Records is a preferred choice in any scenarios that require immutable (read-only) data.

While records are a great and useful feature, there are scenarios where they shouldn't be used:

*   **Entity Framework Entities**: Entity Framework relies heavily on change tracking for database operations and utilizes a reference equality comparison. That's why records with their value-based equality comparison are a bad choice here.
*   **UI Data Binding**: For applications with UI data binding that requires frequent updates (like in WPF or Blazor), the immutability of records can be limiting. Mutable POCO (Plain Old CLR Objects) are typically more suitable in these cases.
*   **Performance-Critical Sections**: If you're working in a performance-critical part of your application, using records where object mutations are frequent - can lead to performance overhead due to a big number of created new instances of records on every modification.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-csharp-records&title=Getting%20Started%20with%20C%23%20Records)[X](https://twitter.com/intent/tweet?text=Getting%20Started%20with%20C%23%20Records&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-csharp-records)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-csharp-records)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.