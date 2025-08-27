```yaml
---
title: "Domain Validation With .NET | Clean Architecture, DDD, .NET 9 | by Michael Maurice | Jul, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/domain-validation-with-net-clean-architecture-ddd-net-9-758e643564fa
date_published: 2025-07-20T17:01:54.432Z
date_captured: 2025-08-12T21:00:58.104Z
domain: medium.com
author: Michael Maurice
category: architecture
technologies: [.NET 9, FluentValidation]
programming_languages: [C#, SQL]
tags: [domain-validation, clean-architecture, domain-driven-design, dotnet, validation, error-handling, design-patterns, best-practices, csharp, .net9]
key_concepts: [Domain Validation, Clean Architecture, Domain-Driven Design, Always-Valid Domain Model, Exception-Based Validation, Result Pattern, Guard Clauses, Aggregate Validation, Input Validation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article delves into the critical role of domain validation in building robust .NET applications, particularly within Clean Architecture and Domain-Driven Design principles, leveraging .NET 9. It distinguishes domain validation from input validation, advocating for the "Always-Valid Domain Model" to ensure domain objects are consistently valid. The piece explores two primary validation strategies: exception-based and the functional Result pattern, detailing their pros and cons. Furthermore, it introduces guard clauses for enforcing invariants and demonstrates how to integrate FluentValidation at the application layer while maintaining domain integrity. The article concludes with best practices for layering validation and highlights relevant .NET 9 enhancements.
---
```

# Domain Validation With .NET | Clean Architecture, DDD, .NET 9 | by Michael Maurice | Jul, 2025 | Medium

# Domain Validation With .NET | Clean Architecture, DDD, .NET 9

![Michael Maurice](https://miro.medium.com/v2/resize:fill:64:64/1*Vydee41-YhCgiyTaA_dPoA.png)

Michael Maurice

Follow

5 min read

·

Jul 20, 2025

113

1

Listen

Share

More

![A diagram titled "DOMAIN VALIDATION" illustrating a layered architecture with "Presentation", "Application", "Infrastructure", and "Domain" layers. Below the "Domain" layer, it shows an "ENTITY" box, which branches to "Guard Clauses" and indicates "Exception-based validation" and "Result pattern validation" as methods for validation.](https://miro.medium.com/v2/resize:fit:521/1*_o8VU2UyTgXQS9urlR53Fg.png)

If you want the full source code, download it from this link: [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

Domain validation represents the cornerstone of building robust, maintainable applications using Clean Architecture and Domain-Driven Design principles in .NET 9. It ensures that business rules and domain invariants are consistently enforced while maintaining clean separation of concerns and preventing invalid states from corrupting your domain model.

# Understanding Domain Validation Fundamentals

Domain validation differs fundamentally from input validation. While input validation ensures data meets basic format requirements at the application boundary, domain validation enforces business rules and invariants that define what makes your domain objects valid. In DDD, domain entities should always be valid entities — there should be no scenario where an entity can exist in an invalid state.

The Always-Valid Domain Model principle states that domain objects should guard themselves from ever becoming invalid. This approach provides several critical benefits:

*   Elimination of Defensive Programming: Once created, you can trust domain objects are in valid states without constant validation checks
*   Centralized Business Logic: All validation rules live within domain objects themselves
*   Reduced Maintenance Burden: You eliminate scattered validation checks throughout your codebase

# Two Primary Validation Approaches

# 1\. Exception-Based Validation

The traditional approach uses exceptions to signal validation failures:

```csharp
public sealed class Email : ValueObject  
{  
    private static readonly Regex EmailRegex = new(  
        @"^\[a-zA-Z0-9.\_%+-\]+@\[a-zA-Z0-9.-\]+\\.\[a-zA-Z\]{2,}$",  
        RegexOptions.Compiled | RegexOptions.IgnoreCase);  
      
    public string Value { get; }  
      
    private Email(string value)  
    {  
        Value = value;  
    }  
      
    public static Email Create(string value)  
    {  
        if (string.IsNullOrWhiteSpace(value))  
            throw new DomainException("Email cannot be empty");  
          
        if (value.Length > 255)  
            throw new DomainException("Email cannot exceed 255 characters");  
          
        if (!EmailRegex.IsMatch(value))  
            throw new DomainException("Invalid email format");  
          
        return new Email(value.ToLowerInvariant());  
    }  
}
```

Advantages:

*   Clear failure indication with immediate termination
*   Familiar to most developers
*   Stack traces aid debugging

Disadvantages:

*   Performance overhead from exception creation
*   Difficulty collecting multiple validation errors
*   Exception handling complexity

# 2\. Result Pattern Validation

The Result pattern provides a functional approach to error handling:

```csharp
public sealed class Result<T>  
{  
    private readonly T? _value;  
    private readonly Error? _error;  
      
    private Result(T value)  
    {  
        _value = value;  
        _error = null;  
        IsSuccess = true;  
    }  
      
    private Result(Error error)  
    {  
        _value = default;  
        _error = error;  
        IsSuccess = false;  
    }  
      
    public bool IsSuccess { get; }  
    public bool IsFailure => !IsSuccess;  
      
    public T Value => IsSuccess   
        ? _value!   
        : throw new InvalidOperationException("Cannot access value of failed result");  
      
    public Error Error => IsFailure   
        ? _error!   
        : throw new InvalidOperationException("Cannot access error of successful result");  
      
    public static Result<T> Success(T value) => new(value);  
    public static Result<T> Failure(Error error) => new(error);  
}
```

Advantages:

*   Explicit Error Handling: Callers must handle success/failure cases explicitly
*   Improved Performance: Avoids exception overhead
*   Better Testing: Easier to test than exception-throwing code
*   Multiple Error Collection: Can aggregate validation errors

Disadvantages:

*   Verbosity: Requires more code compared to exceptions
*   Stack Trace Propagation: Must mark all methods in call chain to return Result objects

# Guard Clauses for Invariant Protection

Guard clauses provide an elegant way to enforce validation rules while maintaining clean, readable code:

```csharp
public static class Guard  
{  
    public static void NotNull<T>(T value,   
        [CallerArgumentExpression(nameof(value))] string? paramName = null)  
    {  
        if (value is null)  
            throw new ArgumentNullException(paramName);  
    }  
      
    public static void NotEmpty(string value,   
        [CallerArgumentExpression(nameof(value))] string? paramName = null)  
    {  
        if (string.IsNullOrWhiteSpace(value))  
            throw new DomainException($"{paramName} cannot be empty");  
    }  
      
    public static void GreaterThan<T>(T value, T minimum,   
        [CallerArgumentExpression(nameof(value))] string? paramName = null)   
        where T : IComparable<T>  
    {  
        if (value.CompareTo(minimum) <= 0)  
            throw new DomainException($"{paramName} must be greater than {minimum}");  
    }  
}
```

Usage in domain entities:

```csharp
public sealed class Product : Entity<ProductId>  
{  
    public string Name { get; private set; }  
    public Money Price { get; private set; }  
    public int StockQuantity { get; private set; }  
      
    public Product(string name, Money price, int stockQuantity)   
        : base(new ProductId(Guid.NewGuid()))  
    {  
        Guard.NotEmpty(name, nameof(name));  
        Guard.NotNull(price, nameof(price));  
        Guard.GreaterThan(stockQuantity, -1, nameof(stockQuantity));  
          
        Name = name;  
        Price = price;  
        StockQuantity = stockQuantity;  
    }  
}
```

# Domain Error Catalogs

Create centralized error catalogs for better maintainability:

```csharp
public static class CustomerErrors  
{  
    public static readonly Error NameRequired = new("Customer.NameRequired", "Customer name is required");  
    public static readonly Error NameTooLong = new("Customer.NameTooLong", "Customer name cannot exceed 100 characters");  
    public static readonly Error EmailRequired = new("Customer.EmailRequired", "Customer email is required");  
    public static readonly Error EmailInvalid = new("Customer.EmailInvalid", "Customer email format is invalid");  
    public static readonly Error NotFound = new("Customer.NotFound", "Customer not found");  
}  
  
public sealed record Error(string Code, string Message);
```

# Aggregate Validation and Invariants

Aggregates serve as consistency boundaries and must enforce invariants across their internal entities:

```csharp
public sealed class Order : AggregateRoot<OrderId>  
{  
    private readonly List<OrderItem> _items = new();  
      
    public CustomerId CustomerId { get; private set; }  
    public Money TotalAmount { get; private set; }  
    public OrderStatus Status { get; private set; }  
      
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();  
      
    public static Result<Order> Create(CustomerId customerId, List<OrderItem> items)  
    {  
        // Business rule: Order must have at least one item  
        if (!items.Any())  
            return Result<Order>.Failure(OrderErrors.EmptyOrder);  
          
        // Business rule: Order cannot exceed maximum value  
        var totalAmount = items.Sum(item => item.Price.Amount * item.Quantity);  
        if (totalAmount > 10000)  
            return Result<Order>.Failure(OrderErrors.ExceedsMaximumValue);  
          
        var order = new Order(customerId, new Money(totalAmount, "USD"));  
        foreach (var item in items)  
        {  
            order._items.Add(item);  
        }  
          
        return Result<Order>.Success(order);  
    }  
}
```

# Integration with FluentValidation in .NET 9

While domain validation should live in the domain layer, FluentValidation complements it at the application layer:

```csharp
public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>  
{  
    public CreateCustomerCommandValidator()  
    {  
        RuleFor(x => x.Name)  
            .NotEmpty()  
            .WithMessage("Customer name is required")  
            .MaximumLength(100)  
            .WithMessage("Customer name cannot exceed 100 characters");  
          
        RuleFor(x => x.Email)  
            .NotEmpty()  
            .WithMessage("Customer email is required")  
            .EmailAddress()  
            .WithMessage("Customer email format is invalid");  
    }  
}
```

Application layer handler combining both approaches:

```csharp
public sealed class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerId>>  
{  
    private readonly ICustomerRepository _customerRepository;  
    private readonly IUnitOfWork _unitOfWork;  
      
    public async Task<Result<CustomerId>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)  
    {  
        // Domain validation through factory method  
        var customerResult = Customer.Create(request.Name, request.Email);  
          
        if (customerResult.IsFailure)  
            return Result<CustomerId>.Failure(customerResult.Error);  
          
        _customerRepository.Add(customerResult.Value);  
        await _unitOfWork.SaveChangesAsync(cancellationToken);  
          
        return Result<CustomerId>.Success(customerResult.Value.Id);  
    }  
}
```

# Best Practices for Domain Validation

# Choose the Right Validation Strategy

Use exceptions when:

*   Validation failure represents a programming error
*   You need immediate termination of invalid operations
*   Single validation failures are expected

Use Result pattern when:

*   Multiple validation errors need collection
*   You want explicit error handling
*   Performance is critical

# Layer Your Validation Properly

Input Validation (Application Layer):

*   Format validation
*   Required field checks
*   Basic data type validation

Business Validation (Domain Layer):

*   Business rule enforcement
*   Invariant protection
*   Cross-entity validation

# Make Validation Explicit

Use clear, descriptive error messages and codes that business stakeholders can understand. Avoid validation duplication between layers — rely on domain objects to maintain their own validity.

# .NET 9 Specific Enhancements

.NET 9 brings several improvements relevant to domain validation:

*   Enhanced Performance: Runtime optimizations benefit validation-heavy scenarios
*   Improved LINQ: New `CountBy` and `AggregateBy` methods simplify validation aggregations
*   Better Error Handling: Enhanced exception handling and result processing
*   Security Improvements: Strengthened validation frameworks and input processing

Domain validation in .NET 9 with Clean Architecture and DDD provides a robust foundation for building maintainable, business-focused applications. By implementing validation at the domain level using appropriate patterns like guard clauses and the Result pattern, while maintaining clear separation of concerns, you create systems that are both technically sound and aligned with business requirements. The key is choosing the right validation strategy for your specific use case and ensuring business rules are consistently enforced across your domain model  
If you want the full source code, download it from this link: [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)