```yaml
---
title: The Best Way To Validate Objects in .NET in 2024
source: https://antondevtips.com/blog/the-best-way-to-validate-objects-in-dotnet-in-2024
date_published: 2024-09-06T08:55:21.852Z
date_captured: 2025-08-06T17:28:07.070Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, DataAnnotations, FluentValidation]
programming_languages: [C#]
tags: [validation, dotnet, data-annotations, fluentvalidation, object-validation, software-design, best-practices, code-quality, data-validation]
key_concepts: [object-validation, data-annotations, fluent-validation, separation-of-concerns, single-responsibility-principle, conditional-validation, custom-validators, asynchronous-validation, nested-object-validation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores object validation techniques in .NET, comparing the traditional DataAnnotations with the more flexible FluentValidation library. It highlights the drawbacks of DataAnnotations, such as tight coupling and limited flexibility, while advocating for FluentValidation's advantages in separation of concerns, reusability, and expressiveness. The post provides comprehensive C# code examples demonstrating FluentValidation's capabilities, including validating nested objects, performing asynchronous checks, implementing conditional rules, and creating custom validators. Ultimately, it positions FluentValidation as the superior approach for robust object validation in modern .NET applications.
---
```

# The Best Way To Validate Objects in .NET in 2024

![A dark blue banner with abstract purple shapes. On the left, a white square icon with a code tag (</>) and the text "dev tips". On the right, large white text reads "THE BEST WAY TO VALIDATE OBJECTS IN .NET IN 2024".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_aspnet_best_validation.png&w=3840&q=100)

# The Best Way To Validate Objects in .NET in 2024

Sep 6, 2024

5 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In today's blog post you will learn how to validate objects in .NET using various techniques and libraries. We'll explore what is the best way to validate objects in .NET in 2024.

Object validation is a crucial aspect of any .NET application. Ensuring that your data is valid before processing, can prevent numerous issues, from simple bugs to critical security vulnerabilities.

There are 2 popular approaches to validating objects in .NET:

*   DataAnnotations
*   FluentValidation

I will show why I consider `FluentValidation` the best approach to validate object in .NET in 2024.

## Validation with DataAnnotations

**DataAnnotations** have been part of .NET for years and remain a straightforward way to enforce validation rules on your models by using attributes. Here's a simple example:

```csharp
public class User
{
    [Required(ErrorMessage = "Username is required.")]
    [StringLength(100, ErrorMessage = "Username must be less than 100 characters.")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address.")]
    public string Email { get; set; }

    [Range(18, 99, ErrorMessage = "Age must be between 18 and 99.")]
    public int Age { get; set; }
}
```

While **DataAnnotations** might seem like a concise way to define validation and have entity and validation in one place, this approach has its drawbacks:

*   **Tight Coupling:** Data annotations tightly couple the entity classes with the validation logic, making it challenging to switch to a different validation framework without modifying the entity classes.
*   **Limited Flexibility:** Data annotations provide limited flexibility compared to FluentValidation configurations. Complex validation scenarios may be difficult or impossible to express using data annotations alone.
*   **Code Clutter:** Embedding validation logic within entity classes can clutter the codebase, especially as the application grows larger. This violates the Single Responsibility Principle and makes the code harder to maintain.
*   **Limited Reusability:** While data annotations can be reused across multiple entities, they are not flexible when it comes to validation scenarios where validation depends on other properties of an object.

As your validation needs to grow more complex, DataAnnotations can become cumbersome and less maintainable.

## Validation with FluentValidation Library

**FluentValidation** offers a more flexible and powerful approach for validation. With FluentValidation, you define your validation logic in a separate class, keeping your models clean and your validation logic expressive:

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(user => user.Username)
            .NotEmpty()
            .Length(1, 100);

        RuleFor(user => user.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(user => user.Age)
            .InclusiveBetween(18, 99);
    }
}
```

`FluentValidation` has built-in default error messages, but you can customize them if you need to:

```csharp
public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(user => user.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(1, 100).WithMessage("Username must be less than 100 characters.");

        RuleFor(user => user.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(user => user.Age)
            .InclusiveBetween(18, 99).WithMessage("Age must be between 18 and 99.");
    }
}
```

With FluentValidation you can inject a dependency into a Validator constructor and, for example, use localization provider to get a localized error message.

Here's how to call a validator:

```csharp
var validator = new UserValidator();
var result = validator.Validate(user);

if (!result.IsValid)
{
    foreach (var failure in result.Errors)
    {
        Console.WriteLine($"Property {failure.PropertyName} failed validation. Error: {failure.ErrorMessage}");
    }
}
```

You can also use asynchronous method to validate an object:

```csharp
var validator = new UserValidator();
var result = await validator.ValidateAsync(user);
```

`FluentValidation` offers the following advantages:

*   **Flexibility:** FluentValidation provides more flexibility compared to data annotations. It allows for more complex configurations and validation rules that might not be achievable with data annotations alone.
*   **Separation of Concerns:** FluentValidation promotes better separation of concerns by keeping validation logic separate from domain classes. This improves code readability and maintainability, as it separates validation concerns from domain logic.
*   **Explicitness:** FluentValidation configurations are explicit and self-documenting. Developers can easily understand the validation rules by reading the FluentValidation code, which helps in understanding the validation logic without inspecting the domain classes directly.
*   **Reuse and Composition:** FluentValidation configurations can be reused across multiple entities or projects. Validation classes can be composed, allowing for easier management and maintenance of complex validation logic.

That's why I consider `FluentValidation` the best approach to validate object in .NET. Let's explore some more complex examples on how to validate objects with `FluentValidation`.

## Validating Nested Objects with FluentValidation

Let's explore an example on how to validate nested objects:

```csharp
public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public Manufacturer Manufacturer { get; set; }
}

public class Manufacturer
{
    public string Name { get; set; }
    public string Country { get; set; }
}
```
```csharp
using FluentValidation;

public class ManufacturerValidator : AbstractValidator<Manufacturer>
{
    public ManufacturerValidator()
    {
        RuleFor(manufacturer => manufacturer.Name)
            .NotEmpty();

        RuleFor(manufacturer => manufacturer.Country)
            .NotEmpty();
    }
}

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty();

        RuleFor(product => product.Price)
            .GreaterThan(0);

        RuleFor(product => product.Manufacturer)
            .NotNull()
            .SetValidator(new ManufacturerValidator());
    }
}
```

As you can see in the example above, you can call the `SetValidator` method to add `ManufacturerValidator` to a `Product` model.

Here's how to call a validator:

```csharp
var validator = new ProductValidator();
var result = validator.Validate(product);

if (!result.IsValid)
{
    foreach (var failure in result.Errors)
    {
        Console.WriteLine($"Property {failure.PropertyName} failed validation. Error: {failure.ErrorMessage}");
    }
}
```

## Asynchronous Validation with Fluent Validation

To perform asynchronous validation with `FluentValidation`, you can use the `MustAsync` method for custom asynchronous rules. Here's an example of how to validate a `Product` model using async validation:

```csharp
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty()
            .MustAsync(BeUniqueName).WithMessage("Product name must be unique.");

        RuleFor(product => product.Price)
            .GreaterThan(0)

        RuleFor(product => product.Manufacturer)
            .NotNull()
            .SetValidator(new ManufacturerValidator());

        RuleFor(product => product.Manufacturer.Name)
            .MustAsync(ManufacturerExists).WithMessage("Manufacturer does not exist.");
    }

    private async Task<bool> BeUniqueName(string name, CancellationToken cancellationToken)
    {
        // Simulate an async check to a database or external service
        await Task.Delay(100, cancellationToken); // Simulated delay
        // Check if the name is unique (this would be a database or service call in a real application)
        return name != "ExistingProductName";
    }

    private async Task<bool> ManufacturerExists(string manufacturerName, CancellationToken cancellationToken)
    {
        // Simulate an async check to a database or external service
        await Task.Delay(100, cancellationToken); // Simulated delay
        // Check if the manufacturer exists (this would be a database or service call in a real application)
        return manufacturerName != "NonExistentManufacturer";
    }
}
```

Generally, I don't recommend checking for database uniqueness in the validators, but some people do it in validators. I find these business checks to fit in the other places. But you should be aware of this feature.

When using asynchronous validation in a validator class - you must call the `ValidateAsync` method, otherwise an exception will be thrown:

```csharp
var validator = new ProductValidator();
var result = await validator.ValidateAsync(product);
```

## Conditional Validation with Fluent Validation

`FluentValidation` provides a way to apply conditional validation rules using the `When` and `Unless` methods. These methods allow you to specify conditions under which certain validation rules should be applied or ignored.

The `When` method applies a rule only if the specified condition is true, for example:

```csharp
public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty();

        // Validate the Price when IsDiscounted is false
        RuleFor(product => product.Price)
            .GreaterThan(0)
            .When(product => !product.IsDiscounted);

        // Validate the DiscountPercentage when IsDiscounted is true
        RuleFor(product => product.DiscountPercentage)
            .InclusiveBetween(0, 100)
            .When(product => product.IsDiscounted);
    }
}
```

The `Unless` method works in the opposite way to When. It applies a rule unless the specified condition is true. For example:

```csharp
// Validate Price unless IsDiscounted is true
RuleFor(product => product.Price)
    .GreaterThan(0)
    .Unless(product => product.IsDiscounted);
```

## Dependent Property Validation with FluentValidation

In FluentValidation you can easily create a validation rule that checks whether an `EndDate` is greater than `StartDate`:

```csharp
RuleFor(x => x.EndDate)
    .GreaterThan(x => x.StartDate)
    .WithMessage("EndDate must be after StartDate.");
```

This example and the ones with product discounts showcase validation rules that depend on the other fields. I will feel sorry for those who will try to implement custom `DataAnnotation` attributes that rely on other properties of an object.

## Creating Custom Validators in FluentValidation

In `FluentValidation` you can easily create custom validators. Let's create a custom `BeAlphanumeric` validator for a `Product` model:

```csharp
using FluentValidation;

public static class CustomValidators
{
    public static IRuleBuilderOptions<T, string> BeAlphanumeric<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(value => value.All(char.IsLetterOrDigit))
              .WithMessage("'{PropertyName}' must contain only alphanumeric characters.");
    }
}

public class ProductValidator : AbstractValidator<Product>
{
    public ProductValidator()
    {
        RuleFor(product => product.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .BeAlphanumeric();
            
        // Other validation rules...
    }
}
```

By creating an extension method to `IRuleBuilder` you can use your custom validators as if it was a standard validation method like `NotEmpty`.

Hope you find this newsletter useful. See you next time.