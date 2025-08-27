```yaml
---
title: How To Write Better and Cleaner Code in .NET
source: https://antondevtips.com/blog/how-to-write-better-and-cleaner-code-in-dotnet
date_published: 2024-12-03T08:55:21.852Z
date_captured: 2025-08-06T17:34:43.288Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, C# 13, .NET 9, Visual Studio, Visual Studio Code, Git, Entity Framework Core, Microsoft.Extensions.Logging, ASP.NET Core]
programming_languages: [C#, SQL]
tags: [clean-code, code-quality, dotnet, csharp, best-practices, refactoring, software-design, readability, maintainability, programming-tips]
key_concepts: [naming-conventions, code-formatting, guard-clauses, early-return, single-responsibility-principle, boy-scout-rule, magic-numbers, defensive-programming]
code_examples: false
difficulty_level: intermediate
summary: |
  This article presents 12 practical tips for writing cleaner and more maintainable code in .NET. It emphasizes the importance of clear and meaningful naming conventions, along with the removal of redundant comments, to enhance code readability. Key advice includes consistent code formatting, reducing nesting, and utilizing early returns to simplify control flow. The post also covers avoiding magic numbers and strings, controlling method parameters, applying the Single Responsibility Principle, and correctly using braces and empty collections to prevent common errors and improve code robustness.
---
```

# How To Write Better and Cleaner Code in .NET

![Cover image for the article "How To Write Better and Cleaner Code in .NET", featuring a white code icon (</>) and "dev tips" text on a dark background with purple abstract shapes.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fcsharp%2Fcover_csharp_clean_code.png&w=3840&q=100)

# How To Write Better and Cleaner Code in .NET

Dec 3, 2024

8 min read

### Newsletter Sponsors

Dive into key concepts, explore real-world applications, and master the latest features through hands-on exercises in Visual Studio and Visual Studio Code with [Mark Price's latest edition C#13 and .NET 9](https://amzn.to/4ghgOx7)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

"Any fool can write code that a computer can understand. Good programmers write code that humans can understand." — Martin Fowler.

I have seen a lot of experienced developers writing code that is hard to read and not enjoyable to work with. I am sure you have worked with such code.

I am passionate about writing code that is easy to read and maintain.

In this blog post, I want to share with you some simple and easy tips — I use in everyday life to write a much better code.

## Tip 1: Naming

When naming your variables, methods, and classes, ensure you give them clear and meaningful names. Good naming conventions enhance code readability and help others (and your future self) understand the intent of your code without needing additional context.

Let's explore a few examples of bad naming that lead to confusion for those who are reading the code:

```csharp
public class AccountService
{
    private string _str;
    private DateTime _dateTime;
    private decimal _iNumber;

    public bool HasNoTime()
    {
        return _dateTime < DateTime.Now;
    }

    public bool RemoveAmount(decimal amount)
    {
        _iNumber -= amount;
        return _iNumber >= 0;
    }
}
```

Do you really understand the purpose of this class, its methods and private fields? This is a class that operates with a BankAccount, checks if an account is expired and performs withdrawal.

If you write such code, you will understand its intent, but what if someone else reads it? Or you will read this code a few weeks later?

Let's rewrite this code to make it more obvious:

```csharp
public class BankAccount
{
    private string _ownerName;
    private DateTime _expirationDate;
    private decimal _balance;

    public bool IsExpired()
    {
        return _expirationDate < DateTime.Now;
    }

    public bool Wihdraw(decimal amount)
    {
        if (_balance >= amount)
        {
            _balance -= amount;
            return true;
        }

        return false;
    }
}
```

Now it's clear that we're working with BankAccount, its owner, expiration date and balance.

**Best Practices:**

*   **Classes:** Use nouns that describe the object's purpose (e.g., UserRepository, EmailService).
*   **Methods:** Use verbs or verb phrases (e.g., GetUserById, SendEmail).
*   **Variables:** Choose descriptive names that reflect their role (e.g., totalPrice, isActive).

## Tip 2: Remove Redundant Comments in The Code

Often poor naming leads to comments in the code explaining the intent. This is a bad practice.

Such comments clutter your code, make it less readable and can become outdated. Your code is your source of truth. Comments should explain the **WHY**, not the **WHAT**.

Comments in the code should not duplicate what the code can already tell you. So instead of this:

```csharp
// Calculate the price for the product
var price = product.Price - Math.Max(product.Price * product.Discount / 100, MaxDiscount);
```

It's better to replace comment with a method that clearly indicates the code intent:

```csharp
var price = GetDiscountedPrice(product);

private decimal GetDiscountedPrice(Product product)
{
    var discountValue = Math.Max(product.Price * product.Discount / 100, MaxDiscount);
    return product.Price - discountValue;
}
```

**Best Practices:**

*   Remove comments that describe what the code does.
*   Remove historical comments or commented code - GIT remembers everything
*   Use comments to explain complex logic or reasons behind decisions.
*   Use comments to write code summary for your public contracts (classes, methods, models)
*   Ensure comments stay updated with code changes.

## Tip 3: Format Code with Indentations and Whitespaces

Proper code formatting enhances readability. Consistent indentation and spacing make it easier to follow the code's structure.

This code really reads hardly:

```csharp
public class Calculator{
public int Add(int a,int b){return a+b;}
}
```

Well formated code is way more readable:

```csharp
public class Calculator
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}
```

**Best practices:**

*   Use consistent indentation (e.g., tabs or 4 spaces).
*   Place braces on new lines consistently.
*   Add spaces around operators and after commas.
*   Use empty lines to separate logical blocks of code.
*   Use empty lines to separate public from private fields in your classes.

## Tip 4: Reduce Nesting

Deeply nested code is hard to read and maintain:

```csharp
if (user is not null)
{
    if (user.IsActive)
    {
        if (user.HasPermission)
        {
            foreach (var preference in user.Preferences)
            {
                // Perform action
            }
        }
    }
}
```

The recommended practice is try to use not more than 2 levels of nesting. Reducing nesting improves code readability:

```csharp
if (user is null || !user.IsActive || !user.HasPermission)
{
    return;
}

foreach (var preference in user.Preferences)
{
    // Perform action
}
```

**Best practices:**

*   Use guard clauses to handle edge cases upfront.
*   Avoid nesting by returning early.
*   Consider refactoring complex nested logic into separate methods.

## Tip 5: Return Early

When conditions aren't met - return early from the method and prevent unnecessary code execution. As we have seen in the previous tip, returning early from the method reduces nesting, and as a result, it improves code readability.

Instead of this code:

```csharp
if (user is not null)
{
    if (user.IsActive)
    {
        if (user.HasPermission)
        {
            foreach (var preference in user.Preferences)
            {
                // Perform action
            }
        }
    }
}
```

You can use return early principle:

```csharp
if (user is null)
{
    return;
}

if (!user.IsActive)
{
    return;
}

if (!user.HasPermission)
{
    return;
}

foreach (var preference in user.Preferences)
{
    // Perform action
}
```

This allows reading code from top to bottom line-by-line without a need to scroll up and down to see the full context.

You can also merge some if statements if they belong together:

```csharp
if (user is null || !user.IsActive || !user.HasPermission)
{
    return;
}
```

You can improve the readability of this code further by extracting a complex if statement to a separate private method:

```csharp
if (!IsUserAllowedToUpdatePreferences(user))
{
    return;
}

foreach (var preference in user.Preferences)
{
    // Perform action
}

private bool IsUserAllowedToUpdatePreferences(User user)
{
    return user is not null && user.IsActive && user.HasPermission;
}
```

**Best practices:**

*   Check for invalid conditions at the beginning of methods.
*   Use return, continue, or break statements to exit early.
*   Improves code flow and readability.

## Tip 6: Get Rid of Else Keyword

Else keyword in most cases reduces code readability. Let'e explore this example of code:

```csharp
var shipmentAlreadyExists = await dbContext.Shipments
    .AnyAsync(x => x.OrderId == request.OrderId, cancellationToken);

if (shipmentAlreadyExists)
{
    logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
}
else
{
    var shipment = request.MapToShipment(shipmentNumber);
    await dbContext.AddAsync(shipment, cancellationToken);
    await dbContext.SaveChangesAsync(cancellationToken);
}

return shipmentAlreadyExists ? Results.Conflict(...) : Results.Ok(response);
```

Often when reading code in the else statement, you need to scroll up to see the corresponding if. This is a small example, but imagine a much bigger code base.

After you add an early return, an else block becomes unnecessary:

```csharp
var shipmentAlreadyExists = await dbContext.Shipments
    .AnyAsync(x => x.OrderId == request.OrderId, cancellationToken);

if (shipmentAlreadyExists)
{
    logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
    return Results.Conflict(...);
}

var shipment = request.MapToShipment(shipmentNumber);
await dbContext.AddAsync(shipment, cancellationToken);
await dbContext.SaveChangesAsync(cancellationToken);

return Results.Ok(response);
```

This code reads more easily, as you read it line-by-line, without a need to scroll up and down.

**Best practices:**

*   Simplify code by eliminating unnecessary else blocks.
*   Improves the linear flow of the code.

## Tip 7: Avoid Double Negatives

When writing if statements - avoid using double negatives as they are really confusing.

For example:

```csharp
if (!user.IsNotActive)
{
    // User is active
}
```

This is really confusing.

And this reads much better:

```csharp
if (user.IsActive)
{
    // User is active
}

if (!user.IsActive)
{
    // User is NOT active
}
```

Name all your **boolean** variables, properties and methods from the positive side, answering **what is happening** or **what happened** instead of what **didn't happen**.

For example, bad naming:

*   user.IsNotActive
*   user.HasNoDept
*   creditCard.IsNotExpired

This is a better naming:

*   user.IsActive
*   user.HasDept
*   creditCard.IsExpired

## Tip 8: Avoid Magic Numbers and Strings

Magic numbers and strings are hard-coded values with no explanation. Replace them with named constants or enums.

```csharp
var discountValue = Math.Max(product.Discount, 100);

if (order.Status == 3)
{
    // Process order
}

if (user.MembershipLevel == "Silver")
{
    // Process user order
}
```
```csharp
const MaxDiscount = 100;
const SilverMembershipLevel = "Silver";

public enum OrderStatus
{
    Pending = 1,
    Processing = 2,
    Completed = 3
}
```

With constants and enums, code becomes more readable and more maintainable:

```csharp
var discountValue = Math.Max(product.Discount, MaxDiscount);

if (order.Status is OrderStatus.Completed)
{
    // Process order
}

if (user.MembershipLevel == SilverMembershipLevel)
{
    // Process user order
}
```

Whenever you need to increase the maximum amount of discount, rename the membership level or add new order statuses — you can do it in a single place.

## Tip 9: Control Number of Method Parameters

Methods with many parameters are hard to read and use. Limit the number of parameters, ideally, to three or fewer.

```csharp
public void CreateUser(string firstName, string lastName, int age, string email, string phoneNumber)
{
    // Create user
}
```

It's much better to group related method parameters into a separate class:

```csharp
public class CreateUserRequest
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
}

public void CreateUser(CreateUserRequest request)
{
    // Create user
}
```

It will be much easier to add new properties to the class without changing the method's signature.

## Tip 10: Applying the Single Responsibility Principle

A class or method should have one, and only one, reason to change. This principle simplifies maintenance and enhances code clarity.

Instead of one monstrous class and method:

```csharp
public class ReportService
{
    public void GenerateReport()
    {
        // Fetch data
        // Analyze data
        // Generate report
        // Send report via email
    }
}
```

Break it down to multiple methods or classes:

```csharp
public class DataFetcher {  }
public class DataAnalyzer {  }
public class ReportGenerator {  }
public class EmailService {  }
```

Let's explore an example where a method is doing multiple things:

```csharp
public Task Handle(User user)
{
    await SaveUserAndSendEmailAsync(user);
}

private Task SaveUserAndSendEmailAsync(User user)
{
}

private Task ValidateAndUpateUserAsync(User user)
{
}
```

For example, here `SaveUserAndSendEmailAsync` is doing 2 things: saving user in the database and sending email afterward.

I also like breaking down such methods into separate single responsible methods:

```csharp
public Task<Result> Handle(User user)
{
    if (!IsValid(user))
    {
        return Result.BadRequest(...);
    }
    
    await SaveAsync(user);
    
    await SendEmailAsync(user);
}

private Task SaveAsync(User user)
{
}

private Task SendEmailAsync(User user)
{
}

private Task<bool> IsValid(User user)
{
}
```

This way, code is much more maintainable and more readable.

## Tip 11: Correctly Use Braces

Always use braces `{ }` with conditional statements, even if they're one-liners. This prevents errors when adding new lines and makes your code more readable and predictable.

```csharp
if (isValid)
    ProcessData();
    
foreach (var preference in user.Preferences)
    AddToReport(preference);
SaveReport();
```

This code hardly reads, right?

Add braces and newlines, and it will be much easier to read it:

```csharp
if (isValid)
{
    ProcessData();
}

foreach (var preference in user.Preferences)
{
    AddToReport(preference);
}
    
SaveReport();
```

**Best practices:**

*   Consistent use of braces improves readability.
*   Reduces the risk of bugs from unintended code execution.

## Tip 12: Do Not Return Null for Collections

Returning null collections can lead to `NullReferenceException` or to too many if checks.

```csharp
public List<Item>? GetOrderItems()
{
    if (noItemsFound)
    {
        return null;
    }

    return items;
}

public void ProcessOrder(...)
{
    var items = GetOrderItems();
    
    // NullReferenceException is thrown here
    var count = items.Count;
    
    foreach (var item in items)
    {
        // Process items
    }
}

public void ProcessOrder(...)
{
    var items = GetOrderItems();
    
    if (items is null)
    {
        return;
    }
    
    var count = items.Count;
    
    foreach (var item in items)
    {
        // Process items
    }
}
```

Instead, return an empty collection.

```csharp
public List<Item> GetOrderItems()
{
    if (noItemsFound)
    {
        return new List<Item>();
    }

    return items;
}

public void ProcessOrder(...)
{
    var items = GetOrderItems();
    
    // No need for any additional checks
    var count = items.Count;
    
    foreach (var item in items)
    {
        // Process items
    }
}
```

By returning an empty collection, you make your code safer and more readable.

In C# 12 you can simplify returning of an empty collection to collection expressions by using `[ ]` syntax:

```csharp
public List<Item> GetOrderItems()
{
    if (noItemsFound)
    {
        return [];
    }

    return items;
}
```

## Summary

These are small tips that can make your code much better. Remember that consistency across your solution is very important. You should use the same naming conventions, code formatting across the whole solution for consistency.

Whenever you write some code: add new features, update existing or fix bugs — take a minute and improve the code you touch. This is called a **Boy Scout Rule** — leave your code better than you found it. By applying this rule, you will improve code quality and perform code refactoring without a significant effort and time.

If you want to further improve your code, I recommend adding [static code analysis](https://antondevtips.com/blog/best-practices-for-increasing-code-quality-in-dotnet-projects).

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-better-and-cleaner-code-in-dotnet&title=How%20To%20Write%20Better%20and%20Cleaner%20Code%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20To%20Write%20Better%20and%20Cleaner%20Code%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-better-and-cleaner-code-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-better-and-cleaner-code-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.