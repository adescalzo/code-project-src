```yaml
---
title: "Decorator Pattern: Add Behavior Without Breaking Code"
source: https://okyrylchuk.dev/blog/decorator-pattern-add-behavior-without-breaking-code/?utm_source=emailoctopus&utm_medium=email&utm_campaign=Decorator
date_published: 2025-01-24T15:50:00.000Z
date_captured: 2025-08-08T13:18:45.965Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: ai_ml
technologies: [.NET, Scrutor]
programming_languages: [C#]
tags: [design-patterns, decorator-pattern, dotnet, dependency-injection, software-architecture, logging, cross-cutting-concerns, scrutor]
key_concepts: [Decorator Pattern, Gang of Four (GoF), Open/Closed Principle, Dependency Injection, Cross-cutting concerns]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Decorator Pattern, a structural design pattern from the Gang of Four (GoF), which enables dynamic addition of new behaviors to objects without altering their core structure. It highlights the pattern's adherence to the Open/Closed Principle, making it ideal for implementing cross-cutting concerns like logging or caching in .NET applications. The author provides a practical C# example of a `LoggingBookDecorator` and demonstrates two methods for registering decorators with .NET's Dependency Injection container: manual registration and simplified registration using the Scrutor library. The post concludes by emphasizing the pattern's role in fostering modular and maintainable software.
---
```

# Decorator Pattern: Add Behavior Without Breaking Code

# Decorator Pattern: Add Behavior Without Breaking Code

## Understanding Decorator Pattern

The **Decorator Pattern** is a structural design pattern used to dynamically add new behaviors or responsibilities to an object at runtime without altering its structure or modifying its existing code. This pattern is part of the **Gang of Four (GoF)** design patterns and promotes **the open/closed principle**, which states that classes should be open for extension but closed for modification.

In .NET applications, you can add **cross-cutting concerns** like logging, caching, or performance monitoring to your services.

## Decorator Implementation

First, let’s define an example of an interface for the Book Service.

```csharp
public interface IBookService
{
    Task<Book> GetBookAsync(int bookId);
}

public record Book(int Id);
```

As the next step, let’s create an implementation.

```csharp
public class BookService: IBookService
{
    public async Task<Book> GetBookAsync(int bookId)
    {
        return await Task.FromResult(new Book(bookId));
    }
}
```

We want to add operations logging to our service without modifying its logic. Let’s create a **LoggingBookDecorator** for that.

```csharp
public class LoggingBookDecorator : IBookService
{
    private readonly IBookService _bookService;
    private readonly ILogger<LoggingBookDecorator> _logger;

    public LoggingBookDecorator(
        IBookService bookService,
        ILogger<LoggingBookDecorator> logger)
    {
        _bookService = bookService;
        _logger = logger;
    }

    public async Task<Book> GetBookAsync(int bookId)
    {
        _logger.LogInformation($"Getting book with ID: {bookId}");

        try
        {
            var result = await _bookService.GetBookAsync(bookId);
            _logger.LogInformation($"Successfully retrieved book {bookId}");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving book {bookId}");
            throw;
        }
    }
}
```

## Register Decorator

To register a decorator in .NET’s Dependency Injection (DI) container, you must explicitly resolve the dependencies and create the decorator.

First, register the original **BookService** with the DI container.

Next, register the **LoggingBookDecorator**. Instead of directly registering it as a type, you use a factory method to resolve the dependencies and wrap the **BookService** with the **LoggingBookDecorator**.

```csharp
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<IBookService>(provider =>
{
    var innerService = provider.GetRequiredService<BookService>();
    var logger = provider.GetRequiredService<ILogger<LoggingBookDecorator>>();

    return new LoggingBookDecorator(innerService, logger);
});
```

## Scrutor

[**Scrutor**](https://github.com/khellang/Scrutor) is a popular library that makes decorator registration more convenient.

First, install the **Scrutor** package:

```bash
dotnet add package Scrutor
```

Then, you can simplify the decorator registration in the DI:

```csharp
builder.Services.AddScoped<IBookService, BookService>()
    .Decorate<IBookService, LoggingBookDecorator>();
```

You can chain multiple decorators. The order of registration determines the order of execution:

```csharp
builder.Services.AddScoped<IBookService, BookService>()
    .Decorate<IBookService, LoggingBookDecorator>()
    .Decorate<IBookService, CachingBookDecorator>();
```

## Conclusion

The decorator pattern, combined with .NET’s built-in DI container, provides a powerful way to enhance services dynamically. Whether you use libraries like Scrutor or manual registration, the approach is straightforward and effective.

Mastering this technique will add another tool to your .NET developer arsenal, enabling you to write more modular and maintainable applications.

---

**Image Analysis:**