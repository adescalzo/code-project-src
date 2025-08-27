```yaml
---
title: Getting Started with Primary Constructors in .NET 8 and C# 12
source: https://antondevtips.com/blog/getting-started-with-primary-constructors-in-net-8-and-csharp-12
date_published: 2024-04-19T11:00:09.948Z
date_captured: 2025-08-06T17:19:49.564Z
domain: antondevtips.com
author: Anton Martyniuk
category: programming
technologies: [.NET 8]
programming_languages: [C#]
tags: [csharp, dotnet, primary-constructors, language-features, classes, records, structs, dependency-injection, inheritance, constructor]
key_concepts: [primary-constructors, records, classes, structs, dependency-injection, immutability, mutability, inheritance]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces primary constructors, a concise syntax feature new to C# 12 and .NET 8, applicable to classes, structs, and records. It details the behavioral differences between primary constructors in classes/structs versus records, particularly concerning parameter storage and property generation. The post demonstrates practical uses for primary constructors, including initializing properties and fields, referencing parameters within class members, and facilitating dependency injection. It also covers the mutable nature of class primary constructor parameters and explains how to handle inheritance with this new language feature.
---
```

# Getting Started with Primary Constructors in .NET 8 and C# 12

![Cover image for "Getting Started with Primary Constructors in .NET 8 and C# 12" showing a code icon and "dev tips" text.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fcsharp%2Fcover_csharp_primary_constructors.png&w=3840&q=100)

# Getting Started with Primary Constructors in .NET 8 and C# 12

Apr 19, 2024

6 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

## Introduction

In .NET 8, C# 12 introduced a concise syntax for constructors - **primary constructors**. These constructors are available for **classes** and **structs** and they look like primary constructors for **records**. But they have different behaviour.

This blog post will explain all the aspects of primary constructors, their nuances and difference when comparing to records.

## How To Declare Primary Constructor in C#

C# provides the following syntax for declaring a primary constructor for classes, structs and records:

```csharp
public class ProductClass(int Id, string Name, string Description, decimal Price);

public struct ProductStruct(int Id, string Name, string Description, decimal Price);

public record ProductRecord(int Id, string Name, string Description, decimal Price);
```

In **records** primary constructor parameters are stored as init only properties, while in **classes** - parameters don't become properties.

This means that when creating a new product - its properties won't be available, as an opposite to a record.

```csharp
var productClass = new ProductClass(1, "PC", "Some amazing PC", 1000.00m);

// Error: productClass.Name and other properties are not available here
Console.WriteLine(productClass.Name);

var productRecord = new ProductRecord(1, "PC", "Some amazing PC", 1000.00m);

// productRecord.Name is available here
Console.WriteLine(productRecord.Name);
```

Moreover **primary constructor** parameters aren't members of a class:

```csharp
public class ProductClass(int Id, string Name, string Description, decimal Price)
{
    public override string ToString()
    {
        // Error: Name property is not a member of a class
        return this.Name;
    }
}
```

Class **primary constructors** serve a different purpose. They can be used:

*   to initialize a class property or a field
*   to reference constructor parameters in a class members
*   for dependency injection

## How To Initialize Class Properties Using Primary Constructors

First let's have a look on how to declare a `Product` class with a classic constructor:

```csharp
public class Product
{
    public int Id { get; }
    public string Name { get; }
    public string Description { get; }
    public decimal Price { get; }

    public Product(int id, string name, string description, decimal price)
    {
        Id = id;
        Name = name;
        Description = description;
        Price = price;
    }
}
```

**Primary constructors** offer a much more concise way to assign class properties:

```csharp
public class Product(int id, string name, string description, decimal price)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public decimal Price { get; } = price;
}

var product = new Product(1, "PC", "Some amazing PC", 1000.00m);
Console.WriteLine(product.Name);
```

Here we are utilizing **primary constructor** parameters to assign properties of the `Product` class. Did you notice that parameters now have a **camelCase** naming? It is a recommended naming style for **primary constructors** for `classes` and `structs`.

It differs from **PascalCase** naming for records, because in classes **primary constructor** parameters are not properties as they are in records. That's why C# team has chosen a different naming style to visually distinguish these constructor parameters.

## How To Initialize Class Fields Using Primary Constructors

**Primary constructor** parameters can be assigned to a class fields too:

```csharp
public class Product(int id, string name, string description, decimal price)
{
    private readonly int _id = id;
    
    public string Name { get; } = name;
    public string Description { get; } = description;
    public decimal Price { get; } = price;
}
```

## How To Use Primary Constructor Parameters Inside Class Members

**Primary constructor** parameters can be freely used inside any of a class members. For example, we can use these parameters inside a `ToString` method:

```csharp
public class Product(int id, string name, string description, decimal price)
{
    public override string ToString()
    {
        return $"Id: {id}; Name: {name}; Description: {description}; Price: {price:00.00} $";
    }
}
```

If **primary constructor** parameters are assigned to properties, fields or are used inside methods: the compiler creates a storage for them in the giving type. Otherwise, the **primary constructor** parameters aren't stored in the object.

## How To Use Primary Constructors For Dependency Injection

Let's explore an example of a `ProductsService` class that has few dependencies injected via constructor:

```csharp
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IProductPriceCalculator _priceCalculator;
        
    public ProductService(
        IProductRepository productRepository,
        IProductPriceCalculator priceCalculator)
    {
        _productRepository = productRepository;
        _priceCalculator = priceCalculator;
    }
        
    public Product GetById(int id)
    {
        return _productRepository.GetById(id);
    }

    public Product Create(Product product)
    {
        product.Price = _priceCalculator.Calculate(product);

        product = _productRepository.Create(product);

        return product;
    }
}
```

With a help of a **primary constructor** you can make your `ProductService` more concise:

```csharp
public class ProductService(
    IProductRepository productRepository,
    IProductPriceCalculator priceCalculator) : IProductService
{
    public Product GetById(int id)
    {
        return productRepository.GetById(id);
    }

    public Product Create(Product product)
    {
        product.Price = priceCalculator.Calculate(product);

        product = productRepository.Create(product);

        return product;
    }
}
```

**Important tip:** all class dependencies that are used in primary constructors are **freely mutable** and **aren't** **readonly** or init only as in the 1st example. Make sure to consider this behavior when deciding whether to use **primary constructors** for **Dependency Injection** or not.

There are debates in the dev community: some like to use primary constructors for dependencies, some does not. This is a kind of tradeoff - on one hand you have more concise code, but less safer, on the other hand - you have safety, but more code. Select what works for you the best.

## Mutable State of Primary Constructor Parameters

As mentioned above - primary constructor parameters are freely mutable.

For example, a `MutableProduct` class that updates the primary constructor parameter in the `ApplyDiscount` method:

```csharp
public class MutableProduct(int id, string name, string description, decimal price)
{
    public int Id { get; } = id;
    public string Name { get; } = name;
    public string Description { get; } = description;
    public decimal Price => price;

    public void ApplyDiscount(decimal discountPercentage)
    {
        price -= price * (discountPercentage / 100);
    }
}
```

When accessing a `Price` property, it's value will be updated after calling the `ApplyDiscount` method.

```csharp
var mutableProduct = new MutableProduct(1, "PC", "Some amazing PC", 1000.00m);

// Outputs: 1000,00
Console.WriteLine(mutableProduct.Price);

mutableProduct.ApplyDiscount(20);

// Outputs: 800,00
Console.WriteLine(mutableProduct.Price);
```

## How To Initialize Base Class When Using Primary Constructors

Imagine you want to sell digital products, like an online PDF book. Such a digital product will have the same properties as a `Product` class with additional `FileSize` (in kilobytes) and `DownloadLink` properties.

We can define a `DigitalProduct` class that inherits from a `Product` class. When declaring a child class you have 2 options to call constructor of the base class.

The 1st option is to use a parent's **primary constructor** and call it directly at a definition level:

```csharp
public class DigitalProduct(
    int id, string name, string description, decimal price,
    double fileSize, string downloadLink)
    : Product(id, name, description, price)
{
    public double FileSize { get; } = fileSize;
    public string DownloadLink { get; } = downloadLink;
}
```

The 2nd option is to use a regular constructor and call a `base()` method to initialize the parent object:

```csharp
public class DigitalProduct : Product
{
    public DigitalProduct(
        int id, string name, string description, decimal price,
        double fileSize, string downloadLink)
        : base(id, name, description, price)
    {
        FileSize = fileSize;
        DownloadLink = downloadLink;
    }

    public double FileSize { get; }
    public string DownloadLink { get; }
}
```

There is one caveat when using derived classes with primary constructors. Let's add a `ToString` method to the `DigitalProduct` class:

```csharp
public class DigitalProduct(
    int id, string name, string description, decimal price,
    double fileSize, string downloadLink)
    : Product(id, name, description, price)
{
    public double FileSize { get; } = fileSize;
    public string DownloadLink { get; } = downloadLink;
    
    public override string ToString()
    {
        return $"Id: {id}; Name: {name}; Description: {description}; Price: {price:00.00} $";
    }
}
```

In this example we are using primary constructor parameters inside a `ToString` method. The problem with this approach is that two copies of parameters are being created: one in the base class and another in the derived class. So when accessing data inside a `DigitalProduct` and `Product` classes - data may be out of sync.

The better option is to always use the Properties in the `ToString` method instead of constructor parameters:

```csharp
public class DigitalProduct(
    int id, string name, string description, decimal price,
    double fileSize, string downloadLink)
    : Product(id, name, description, price)
{
    public double FileSize { get; } = fileSize;
    public string DownloadLink { get; } = downloadLink;
    
    public override string ToString()
    {
        return $"Id: {Id}; Name: {Name}; Description: {Description}; Price: {Price:00.00} $";
    }
}
```

## Summary

**Primary constructors** introduced in C# 12, offer a much more concise way to assign class properties. While the primary constructors in classes and records may look the same, but they serve a different purpose.

**It is important to remember the following nuances:**

*   in **records** primary constructor parameters are stored as init only properties, while in **classes** - parameters don't become properties
*   **class** primary constructor parameters are not readonly or init only, they are freely mutable
*   **class** primary constructor parameters are stored in the object if they are assigned to properties, fields or used inside class methods

**Class primary constructors can be used to:**

*   to initialize a class property or a field
*   to reference constructor parameters in class members
*   for dependency injection

Hope you find this newsletter useful. See you next time.