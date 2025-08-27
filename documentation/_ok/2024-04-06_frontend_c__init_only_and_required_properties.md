```yaml
---
title: C# Init Only and Required Properties
source: https://antondevtips.com/blog/csharp-init-only-and-required-properties
date_published: 2024-04-06T11:00:52.570Z
date_captured: 2025-08-06T17:16:14.331Z
domain: antondevtips.com
author: Anton Martyniuk
category: frontend
technologies: [C# 9.0, C# 11, .NET 8, System.Text.Json]
programming_languages: [C#]
tags: [csharp, language-features, immutability, records, properties, dotnet, object-initialization, compile-time]
key_concepts: [init-only-properties, required-properties, immutability, records, positional-records, primary-constructor, compile-time-checks, object-initialization]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains C# 9.0's `init` only properties and C# 11's `required` keyword, highlighting their roles in enforcing object immutability and ensuring property assignment during object creation. It demonstrates how `init` properties allow assignment only at initialization, while `required` properties mandate assignment at compile time. The article also covers positional records, a concise way to declare records with `init` properties, and discusses important notes regarding `required` properties' runtime behavior and interaction with constructors. This guide helps developers understand and effectively utilize these modern C# features for robust data modeling.
---
```

# C# Init Only and Required Properties

![Cover image for the article "C# Init Only and Required Properties", featuring a dev tips logo and abstract purple shapes on a dark background.](https://antondevtips.com/media/covers/csharp/cover_csharp_init_required.png)

# C# Init Only and Required Properties

Apr 6, 2024

3 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

## Init only properties

C# 9.0 introduced a new type of a property accessor called **init**.
The **init** only property can be assigned only during object creation and can't be changed further. This enforces immutability.

Let's see how to define **init** only properties:

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
    Price = 500.00m
};
```

The best practise when creating a record is declaring all of its properties as **init** only. This ensures that properties can't be changed, ensuring the immutability of the record's data. Although you can define **init** properties in a **class** too.

**init** only properties have the same behaviour as **set** properties. You can use a backing field for the property if needed:

```csharp
public record Product
{
    private readonly decimal _price;

    public required decimal Price
    {
        get => _price;
        init => _price = value;
    }
    
    // Other properties...
}
```

## Required properties

A new keyword called **required** was introduced in C# 11, which is a great addition to **init** only properties:

```csharp
public record Product
{
    public required int Id { get; init; }

    public required string Name { get; init; }
    
    public required decimal Price { get; init; }
}
```

This ensures that all properties marked with **required** keyword, should be assigned when creating an object, otherwise a compilation error is raised:

```csharp
// This code doesn't compile as Price property is not assigned
var product = new Product
{
    Id = 1,
    Name = "Phone"
};
```

**init** and **required** keywords make a great pair together. **required** ensures that **init** only properties are assigned during objection creation, as these properties can't be changed further.

## Positional Records

C# also provides a shortened form of record declaration:

```csharp
public record Product(int Id, string Name, decimal Price);

var product = new Product(1, "PC", 1000.00m);
```

A single line of code! We assign all the properties using a record's constructor which is called a **primary** constructor. Under the hood this code is translated to a classic form of a record declaration with all properties being **init** only. This form of record is also called a **positional record** as all properties under the hood are created in the exact order as their position in the primary constructor. Positional form of declaration looks really elegant and concise when a record doesn't have a lot of fields.

When creating a record's object using a primary constructor - you are forced to assign all the properties. Though the **required** keyword is not used here.

## Init Only and Required Properties Important Notes

### 1. Required keyword only works at compile time

When an object is created at the runtime, **required** keyword has no effect, it doesn't throw exceptions even if the field is not assigned with a value. **required** works only at compile time. Although there is one exception: objects serialized and deserialized using `System.Text.Json` in **NET 8** can throw exceptions when **required** properties are not assigned with a value.

### 2. Nullable properties can be marked as required

It is perfectly fine to mark nullable properties as required. It forces you to assign it a real value or null when creating an object. Let's add a nullable `Description` property to the `Product` record:

```csharp
public record Product
{
    public required int Id { get; init; }

    public required string Name { get; init; }
    
    public required string? Description { get; init; }
    
    public required decimal Price { get; init; }
}
    
var product = new Product
{
    Id = 1,
    Name = "PC",
    Price = 1000.00m,
    Description = "Some amazing PC" // Assign some value
};

var product2 = new Product
{
    Id = 1,
    Name = "PC",
    Price = 1000.00m,
    Description = null // Explicitly assign null here
};
```

You should really consider using **required** keyword for all types of properties whenever appropriate.

### 3. Using constructor with required properties

Let's see the following code where we create a class with required properties and a constructor:

```csharp
public class User
{
    public User(string name, int age)
    {
        Name = name;
        Age = age;
    }
    
    public required string Name { get; init; }
    
    public required int Age { get; init; }
}
    
var user = new User("Anton", 30); // This doesn't compile
```

This code doesn't compile as compiler tells that required properties are not assigned, even if they are assigned from the constructor. To fix the code you need to add property assignments along with the constructor:

```csharp
var user = new User("Anton", 30)
{
    Name = "Anton",
    Age = 30
}; // Now this compiles
```

This one is a real caveat and you should consider this strange behaviour when modelling classes.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcsharp-init-only-and-required-properties&title=C%23%20Init%20Only%20and%20Required%20Properties)[X](https://twitter.com/intent/tweet?text=C%23%20Init%20Only%20and%20Required%20Properties&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcsharp-init-only-and-required-properties)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcsharp-init-only-and-required-properties)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.