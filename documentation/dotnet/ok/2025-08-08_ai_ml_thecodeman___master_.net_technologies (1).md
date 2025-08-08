```yaml
---
title: "TheCodeMan | Master .NET Technologies"
source: https://thecodeman.net/posts/chain-responsibility-pattern?utm_source=newsletter&utm_medium=email&utm_campaign=TheCodeMan%20Newsletter%20-%20Chain%20Responsibility%20Pattern%20%28copy%29
date_published: unknown
date_captured: 2025-08-08T16:18:14.564Z
domain: thecodeman.net
author: Unknown
category: ai_ml
technologies: [.NET 6, JetBrains Rider, Postman]
programming_languages: [C#, JavaScript]
tags: [design-patterns, chain-of-responsibility, dotnet, csharp, software-architecture, behavioral-patterns, code-refactoring, e-commerce, discount-rules]
key_concepts: [chain-of-responsibility-pattern, behavioral-design-pattern, loose-coupling, single-responsibility-principle, design-patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive explanation of the Chain of Responsibility design pattern, a behavioral pattern that enables a series of objects to process a request sequentially. It illustrates the pattern's implementation in .NET 6 through a practical e-commerce example involving various discount rules. The content demonstrates how to refactor a simple if-else logic into a more flexible and maintainable chain of handlers. Detailed C# code examples are provided for the abstract handler and specific discount handlers. The article concludes by outlining the key advantages, such as improved flexibility and loose coupling, as well as potential drawbacks like unhandled requests and performance considerations.
---
```

# TheCodeMan | Master .NET Technologies

# Chain Responsibility Pattern

Nov 25 2024

##### **Many thanks to the sponsors who make it possible for this newsletter to be free for readers.**

##### • [JetBrains Rider](https://www.jetbrains.com/rider/?utm_campaign=rider_free&utm_content=site&utm_medium=cpc&utm_source=thecodeman_newsletter) is **now FREE for non-commercial development**, making it more accessible for hobbyists, students, content creators, and open source contributors.

##### [Download and start today!](https://www.jetbrains.com/rider/?utm_campaign=rider_free&utm_content=site&utm_medium=cpc&utm_source=thecodeman_newsletter)

##### • Tired of outdated API documentation holding your team back? Postman simplifies your life by [automatically syncing documentation with your API updates](https://community.postman.com/t/the-postman-drop-november-edition/71372?utm_source=influencer&utm_medium=Social&utm_campaign=nov24_global_growth_pmdropnl&utm_term=Stefan_Djokic) - no more static docs, no more guesswork!

##### [Read more](https://community.postman.com/t/the-postman-drop-november-edition/71372?utm_source=influencer&utm_medium=Social&utm_campaign=nov24_global_growth_pmdropnl&utm_term=Stefan_Djokic).

### The background

##### The Chain of Responsibility pattern is a behavioral design pattern that allows you to build a chain of objects to handle a request or perform a task.

##### Each object in the chain has the ability to either handle the request or pass it on to the next object in the chain. This pattern promotes loose coupling and flexibility in handling requests.

##### Let's explore how to implement the Chain of Responsibility pattern in .NET 6 using a practical example.

### Example Scenario

##### Let's consider a scenario where we have a series of discount rules for an e-commerce application. Depending on the customer's profile, we want to apply different discount percentages to their orders.

##### The discount rules are as follows:

##### • If the customer is a VIP, apply a 20% discount.

##### • If the customer is a regular customer, apply a 10% discount.

##### • If the customer is a new customer, apply a 5% discount.

##### • If none of the above rules match, apply no discount.

##### Initially, we can handle this logic using a series of If statements:

```csharp
public decimal CalculateDiscount(Customer customer, decimal orderTotal)
{
    if (customer.IsVIP)
    {
        return orderTotal * 0.8m; // 20% discount
    }
    else if (customer.IsRegular)
    {
        return orderTotal * 0.9m; // 10% discount
    }
    else if (customer.IsNew)
    {
        return orderTotal * 0.95m; // 5% discount
    }
    else
    {
        return orderTotal; // no discount
    }
}
```

### Chain of Responsibility

##### While the If statements approach works, it can become unwieldy when the number of rules grows.

##### The Chain of Responsibility pattern provides a more flexible and maintainable solution.

##### Let's refactor the code to use this pattern.

##### **Step #1**: Create an **abstract handler class, DiscountHandler**, that defines a common interface for all discount handlers:

```csharp
public abstract class DiscountHandler
{
    protected DiscountHandler _nextHandler;

    public void SetNextHandler(DiscountHandler nextHandler)
    {
        _nextHandler = nextHandler;
    }

    public abstract decimal CalculateDiscount(Customer customer, decimal orderTotal);
}
```

##### **Step #2**: Implement **concrete discount handlers** by deriving from DiscountHandler. Each handler will handle a specific rule and decide whether to apply a discount or pass the request to the next handler.

##### **VIPDiscountHandler:**

```csharp
public class VIPDiscountHandler : DiscountHandler
{
    public override decimal CalculateDiscount(Customer customer, decimal orderTotal)
    {
        if (customer.IsVIP)
        {
            return orderTotal * 0.8m; // 20% discount
        }

        return _nextHandler?.CalculateDiscount(customer, orderTotal) ?? orderTotal;
    }
}
```

##### **RegularDiscountHandler:**

```csharp
public class RegularDiscountHandler : DiscountHandler
{
    public override decimal CalculateDiscount(Customer customer, decimal orderTotal)
    {
        if (customer.IsRegular)
        {
            return orderTotal * 0.9m; // 10% discount
        }

        return _nextHandler?.CalculateDiscount(customer, orderTotal) ?? orderTotal;
    }
}
```

##### **NewCustomerDiscountHandler:**

```csharp
public class NewCustomerDiscountHandler : DiscountHandler
{
    public override decimal CalculateDiscount(Customer customer, decimal orderTotal)
    {
        if (customer.IsNew)
        {
            return orderTotal * 0.95m; // 5% discount
        }

        return _nextHandler?.CalculateDiscount(customer, orderTotal) ?? orderTotal;
    }
}
```

##### **NoDiscountHandler:**

```csharp
public class NoDiscountHandler : DiscountHandler
{
    public override decimal CalculateDiscount(Customer customer, decimal orderTotal)
    {
        return orderTotal; // no discount
    }
}
```

##### **Step #3:** With the concrete handlers in place, we can create the chain by linking them together:

```csharp
var vipHandler = new VIPDiscountHandler();

vipHandler.SetNextHandler(new RegularDiscountHandler())
          .SetNextHandler(new NewCustomerDiscountHandler())
          .SetNextHandler(new NoDiscountHandler());
```

##### Finally, we can invoke the chain by calling the **CalculateDiscount method** on the first handler in the chain:

```csharp
decimal discountAmount = vipHandler.CalculateDiscount(customer, orderTotal);
```

### Pros and Cons?

##### What are the benefits from this?

##### **1\. Flexibility**

##### The Chain of Responsibility pattern allows you to dynamically modify or extend the chain without affecting other parts of the code. You can add or remove handlers as needed.

##### **2\. Loose coupling**

##### The pattern promotes loose coupling between the sender of a request and its receivers. Each handler only needs to know about its immediate successor, minimizing dependencies.

##### **3\. Single Responsibility Principle**

##### You can decouple classes that invoke operations from classes that perform operations.

#### **Okay,what about Drawbacks?**

##### **1\. Request may go unhandled**

##### If none of the handlers in the chain can handle the request, it may go unhandled, leading to unexpected behavior. It's important to have a default handler or a way to handle such scenarios.

##### **2\. Potential performance impact**

##### If the chain becomes very long, it may result in performance overhead due to the traversal of multiple handlers.

##### Remember, it's essential to strike a balance between the number of handlers and performance considerations when applying this pattern to real-world scenarios.

##### That's all from me for today.

## **dream BIG!**

### **There are 3 ways I can help you:**

#### My Design Patterns Ebooks

[1\. Design Patterns that Deliver](/design-patterns-that-deliver-ebook?utm_source=website)

This isn’t just another design patterns book. Dive into real-world examples and practical solutions to real problems in real applications.[Check out it here.](/design-patterns-that-deliver-ebook?utm_source=website)

[1\. Design Patterns Simplified](/design-patterns-simplified?utm_source=website)

Go-to resource for understanding the core concepts of design patterns without the overwhelming complexity. In this concise and affordable ebook, I've distilled the essence of design patterns into an easy-to-digest format. It is a Beginner level. [Check out it here.](/design-patterns-simplified?utm_source=website)

#### [Join TheCodeMan.net Newsletter](/)

Every Monday morning, I share 1 actionable tip on C#, .NET & Arcitecture topic, that you can use right away.

#### [Sponsorship](/sponsorship)

Promote yourself to 17,150+ subscribers by sponsoring this newsletter.

# Become a better developer every week  
with my newsletter

Every Monday morning, start the week with a cup of coffee and 1 actionable .NET tip that you can use right away.