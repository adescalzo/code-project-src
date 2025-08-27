```yaml
---
title: "Result Pattern in C#. Exceptions are traditional way of… | by Deniz Can Yüksel | Medium"
source: https://medium.com/@dnzcnyksl/result-pattern-in-c-b0513099466e
date_published: 2024-11-25T14:11:41.310Z
date_captured: 2025-08-08T13:50:26.680Z
domain: medium.com
author: Deniz Can Yüksel
category: programming
technologies: [.NET Core]
programming_languages: [C#]
tags: [result-pattern, error-handling, exceptions, csharp, design-patterns, clean-code, performance, control-flow]
key_concepts: [Result Pattern, exception handling, control flow, error management, design patterns, explicit error handling]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Result Pattern in C# as a robust alternative to traditional exception handling for managing control flow and errors. It begins by outlining the common drawbacks of using exceptions, such as increased code complexity, performance overhead, and difficulties in debugging. The author then presents a detailed implementation of a generic `Result` class and its typed counterpart `Result<T>`, demonstrating how to encapsulate success, error, and warning states. Through practical C# code examples, the article illustrates how adopting the Result Pattern leads to cleaner, more explicit, and maintainable error management, enhancing code readability and reducing implicit dependencies.
---
```

# Result Pattern in C#. Exceptions are traditional way of… | by Deniz Can Yüksel | Medium

# Result Pattern in C#

![Profile picture of Deniz Can Yüksel](https://miro.medium.com/v2/resize:fill:64:64/1*4K0NPpTOvQ7SGsAfJ0wRlg.jpeg)

Deniz Can Yüksel

Follow

4 min read

·

Oct 2, 2024

253

6

Listen

Share

More

![The .NET Core logo, a purple hexagon with '.NET Core' written in white.](https://miro.medium.com/v2/resize:fit:640/1*1K33MXoFTeWaWpBLkRRiOw.jpeg)

Exceptions are traditional way of control flow.

If there is an error, create an exception and throw it.

Leave the rest to the higher-level function.

This approach works, but there are some drawbacks.

Higher-level functions should know the implementation details of lower-level functions.

Code becomes messy and long when you keep adding exceptions.

Using exceptions is actually expensive and they affect performance.

Even debugging becomes harder.

```csharp
public double GetCustomerDiscount(Guid customerId, double price)  
{  
    try  
    {  
        var customer = GetCustomer(customerId);  
      
        return customer.DiscountRate * price;  
    }  
    catch (CustomerNotFoundException ex)  
    {  
        // do your stuff  
    }  
    catch(Exception ex)  
    {  
        // do your stuff  
    }  
}

public Customer GetCustomer(Guid customerId)  
{  
    try  
    {  
        var customer = context.Customers.FirstOrDefault(c => c.CustomerId == customerId);  
        if (customer is null)  
        {  
            return new CustomerNotFoundException(); // This line would cause a compile-time error as it returns an exception object instead of Customer
        }  
      
        return customer;
    }  
    catch(Exception ex)  
    {  
        logger.LogError(ex.Message);  
        throw;  
    }  
}
```

As you can see higher-level function need to know the implementation details of lower-level function.

```csharp
public double GetCustomerDiscount(Guid customerId, Guid productId)  
{  
    try  
    {  
        var customer = GetCustomer(customerId);  
        var product = GetProduct(productId);  
      
        return customer.DiscountRate * product.Price;  
    }  
    catch (CustomerNotFoundException ex)  
    {  
        // do your stuff  
    }  
    catch (ProductNotFoundException ex)  
    {  
        // do your stuff  
    }  
    catch(Exception ex)  
    {  
        // do your stuff  
    }  
}

public Product GetProduct(Guid productId)  
{  
    try  
    {  
        var product = context.Products.FirstOrDefault(p => p.ProductId == productId);  
        if (product is null)  
        {  
            return new ProductNotFoundException(); // This line would cause a compile-time error as it returns an exception object instead of Product
        }  
      
        return product;  
    }  
    catch(Exception ex)  
    {  
        logger.LogError(ex.Message);  
        throw;  
    }  
}
```

Even with a slight change, you can see how code becomes longer and less maintainable.

Luckily we have an alternative way of control flow. Result Pattern.

```csharp
public abstract record ResultBaseModel(string Message);  
  
public record Error(string Message) : ResultBaseModel(Message);  
  
public record Warning(string Message) : ResultBaseModel(Message);

public class Result  
{  
    public bool IsSuccessful => Errors.Count == 0;  
    public bool IsFailed => !IsSuccessful;  
  
    public List<Error> Errors { get; protected set; } = [];  
    public List<Warning> Warnings { get; protected set; } = [];  
  
    protected Result(List<Error> errors, List<Warning> warnings)  
    {  
        Errors = errors;  
        Warnings = warnings;  
    }  
  
    public static Result Ok()  
    {  
        return new Result([], []);  
    }  
  
    public static Result Fail(params List<Error> errors)  
    {  
        var validErrors = errors.Where(e => e is not null).ToList();  
        if (validErrors.Count == 0)  
        {  
            validErrors.Add(new Error("Fail result is created without any error."));  
        }  
  
        return new Result(validErrors, []);  
    }  
  
    public static implicit operator Result(Error error)  
    {  
        return Fail(error);  
    }  
  
    public static implicit operator Result(List<Error> errors)  
    {  
        return Fail(errors);  
    }  
  
    public void Add(ResultBaseModel model)  
    {  
        if (model is Error)  
        {  
            Errors.Add((model as Error)!);  
  
            return;  
        }  
  
        if (model is Warning)  
        {  
            Warnings.Add((model as Warning)!);  
  
            return;  
        }  
    }  
  
    public void AddRange(IReadOnlyList<ResultBaseModel> models)  
    {  
        foreach (var model in models)  
        {  
            Add(model);  
        }  
    }  
}

public sealed class Result<T> : Result where T : notnull  
{  
    public T? Payload { get; } = default;  
  
    private Result(T payload) : base([], [])  
    {  
        Payload = payload;  
    }  
  
    private Result(List<Error> errors, List<Warning> warnings) : base(errors, warnings)  
    {  
    }  
  
    public static Result<T> Ok(T payload)  
    {  
        return new Result<T>(payload);  
    }  
  
    public static new Result<T> Fail(params List<Error> errors)  
    {  
        var validErrors = errors.Where(e => e is not null).ToList();  
        if (validErrors.Count == 0)  
        {  
            validErrors.Add(new Error("Fail result is created without any error."));  
        }  
  
        return new Result<T>(validErrors, []);  
    }  
  
    public static implicit operator Result<T>(T payload)  
    {  
        return new Result<T>(payload);  
    }  
  
    public static implicit operator Result<T>(Error error)  
    {  
        return Result<T>.Fail(error);  
    }  
  
    public static implicit operator Result<T>(List<Error> errors)  
    {  
        return Result<T>.Fail(errors);  
    }  
}
```

And bonus:

```csharp
public static class ResultExtensions
{
    public static void Merge(this Result result, params Result[] results)  
    {  
        var errors = results.SelectMany(r => r.Errors).ToList();  
        result.AddRange(errors);  
  
        var warnings = results.SelectMany(r => r.Warnings).ToList();  
        result.AddRange(warnings);  
    }  
  
    public static void MergeErrorsAsWarnings(this Result result, params Result[] results)  
    {  
        var errors = results.SelectMany(r => r.Errors).ToList();  
        result.AddRange(errors.Select(x => new Warning(x.Message)).ToList());  
  
        var warnings = results.SelectMany(r => r.Warnings).ToList();  
        result.AddRange(warnings);  
    }  
  
    public static Result<T> ToResult<T>(this Result result) where T : notnull  
    {  
        var resultT = Result<T>.Fail([.. result.Errors]);  
        resultT.AddRange(result.Warnings);  
  
        return resultT;  
    }
}
```

Now we are ready to use Result pattern.

```csharp
public Result<double> GetCustomerDiscount(Guid customerId, Guid productId)  
{  
    var customerResult = GetCustomer(customerId);  
    if (customerResult.IsFailed)  
    {  
        return customerResult.Errors;  
    }  
  
    var productResult = GetProduct(productId);  
    if (productResult.IsFailed)  
    {  
        return productResult.Errors;  
    }  
      
    var discountRate = customerResult.Payload!.DiscountRate;  
    var price = productResult.Payload!.Price;  
  
    return discountRate * price;  
}

public Result<Customer> GetCustomer(Guid customerId)  
{  
    try  
    {  
        var customer = context.Customers.FirstOrDefault(c => c.CustomerId == customerId);  
        if (customer is null)  
        {  
            return new Error($"Customer with id: '{customerId}' is not found.");  
        }  
      
        return customer;  
    }  
    catch(Exception ex)  
    {  
        return new Error($"There is an error while getting customer with id: '{customerId}'. Error message: '{ex.Message}'.");  
    }  
}

public Result<Product> GetProduct(Guid productId)  
{  
    try  
    {  
        var product = context.Products.FirstOrDefault(p => p.ProductId == productId);  
        if (product is null)  
        {  
            return new Error($"Product with id: '{productId}' is not found.");  
        }  
      
        return product;  
    }  
    catch(Exception ex)  
    {  
        return new Error($"There is an error while getting product with id: '{productId}'. Error message: '{ex.Message}'.");  
    }  
}
```

If you have any questions please leave a comment.

You don’t have to copy the code.

The repository is here: [https://github.com/dcyuksel/Result](https://github.com/dcyuksel/Result)