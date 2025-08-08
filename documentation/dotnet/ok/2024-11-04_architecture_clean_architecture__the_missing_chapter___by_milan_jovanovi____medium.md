```yaml
---
title: "Clean Architecture: The Missing Chapter | by Milan Jovanović | Medium"
source: https://medium.com/@MilanJovanovicTech/clean-architecture-the-missing-chapter-558f41802356
date_published: 2024-11-04T18:48:14.573Z
date_captured: 2025-08-08T15:53:14.138Z
domain: medium.com
author: Milan Jovanović
category: architecture
technologies: [.NET, ASP.NET Core, SQL, xUnit, FluentAssertions]
programming_languages: [C#, SQL]
tags: [clean-architecture, software-architecture, code-organization, dependency-management, dotnet, design-patterns, modular-monolith, vertical-slice-architecture, encapsulation, architecture-testing]
key_concepts: [Clean Architecture, dependency management, package by feature, package by component, vertical slice architecture, modular monolith, encapsulation, architecture testing]
code_examples: true
difficulty_level: intermediate
summary: |
  This article clarifies that Clean Architecture is fundamentally about managing dependencies and organizing code around business capabilities, not merely folder structures. It critiques the common mistake of traditional layer-based organization, which leads to scattered features and weak encapsulation. The author introduces superior alternatives like "package by feature" and "package by component," demonstrating how to enforce architectural boundaries using C# `internal` keywords and architecture tests. The piece advocates for a pragmatic approach, emphasizing that a well-designed architecture should accelerate feature delivery rather than hinder it with rigid constraints.
---
```

# Clean Architecture: The Missing Chapter | by Milan Jovanović | Medium

![A banner image with the text "CLEAN ARCHITECTURE THE MISSING CHAPTER" on a blue and black background, with geometric shapes.](https://miro.medium.com/v2/resize:fit:700/0*Xt8ZIuH_mMyHousZ.png)

# Clean Architecture: The Missing Chapter

[

![Milan Jovanović](https://miro.medium.com/v2/da:true/resize:fill:64:64/0*qDCZBXG8qaslZbQs)

](/@MilanJovanovicTech?source=post_page---byline--558f41802356---------------------------------------)

[Milan Jovanović](/@MilanJovanovicTech?source=post_page---byline--558f41802356---------------------------------------)

Follow

8 min read

·

Nov 2, 2024

249

7

Listen

Share

More

I see the same mistake happen over and over again.

Developers discover Clean Architecture, get excited about its principles, and then… they turn the famous Clean Architecture diagram into a project structure.

But here’s the thing: **Clean Architecture is not about folders**. It’s about dependencies.

Simon Brown wrote a “missing chapter” for Uncle Bob’s Clean Architecture book that addresses exactly this issue. Yet somehow, this crucial message got lost along the way.

Today, I’ll show you what Uncle Bob’s Clean Architecture diagram really means and how you should actually organize your code. We’ll look at practical examples that you can use in your projects right now.

Let’s clear up this common misconception once and for all.

# The Problem With Traditional Layering

Almost every .NET developer has built a solution that looks like this:

*   `MyApp.Web` for controllers and views
*   `MyApp.Business` for services and business logic
*   `MyApp.Data` for repositories and data access

It’s the default approach. It’s what we see in tutorials. It’s what we teach juniors.

And it’s completely wrong.

# Why Layer-Based Organization Fails

When you organize code by technical layers, you scatter related components across multiple projects. A single feature, like managing policies, ends up spread across your entire codebase:

*   Policies controller in the Web layer
*   Policy service in the Business layer
*   Policy repository in the Data layer

Here’s what you’ll see when looking at the folder structure:

📁 MyApp.Web
|\_\_ 📁 Controllers
    |\_\_ #️⃣ PoliciesController.cs
📁 MyApp.Business
|\_\_ 📁 Services
    |\_\_ #️⃣ PolicyService.cs
📁 MyApp.Data
|\_\_ 📁 Repositories
    |\_\_ #️⃣ PolicyRepository.cs

Here’s a visual representation of the layer-based architecture:

![A diagram illustrating traditional layer-based architecture with `MyApp.Web`, `MyApp.Business`, and `MyApp.Data` projects, showing dependencies flowing downwards.](https://miro.medium.com/v2/resize:fit:490/1*kaTNGjE1kt_e_abSOVkHuQ.png)

This fragmentation creates several problems:

1.  **Violates Common Closure Principle** — Classes that change together should stay together. When your “Policies” feature changes, you’re touching three different projects.
2.  **Hidden dependencies** — Public interfaces everywhere make it possible to bypass layers. Nothing stops a controller from directly accessing a repository.
3.  **No business intent** — Opening your solution tells you nothing about what the application does. It only shows technical implementation details.
4.  **Harder maintenance** — Making changes requires jumping between multiple projects.

The worst part? This approach doesn’t even achieve what it promises. Despite the separate projects, you often end up with a “big ball of mud” because public access modifiers allow any class to reference any other class.

# The Real Intent of Layers

Clean Architecture’s circles were never meant to represent projects or folders. They represent different levels of policy, with dependencies pointing inward toward business rules.

You can achieve this without splitting your code into artificial technical layers.

Let me show you a better way.

# Better Approaches to Code Organization

Instead of splitting your code by technical layers, you have two better options: **package by feature** or **package by component**.

Let’s look at both.

# Package by Feature

Organizing by feature is a solid option. Each feature gets its own namespace and contains everything needed to implement that feature.

📁 MyApp.Policies
|\_\_ 📁 RenewPolicy
    |\_\_ #️⃣ RenewPolicyCommand.cs
    |\_\_ #️⃣ RenewPolicyHandler.cs
    |\_\_ #️⃣ PolicyValidator.cs
    |\_\_ #️⃣ PolicyRepository.cs
|\_\_ 📁 ViewPolicyHistory
    |\_\_ #️⃣ PolicyHistoryQuery.cs
    |\_\_ #️⃣ PolicyHistoryHandler.cs
    |\_\_ #️⃣ PolicyHistoryViewModel.cs

Here’s a diagram representing this structure:

![A diagram illustrating a "package by feature" architecture, where `MyApp.Policies` contains feature-specific sub-folders like `RenewPolicy` and `ViewPolicyHistory`, with internal components.](https://miro.medium.com/v2/resize:fit:490/1*FXuiQqLWAJgpHnPgCuzeJw.png)

This approach:

*   Makes features explicit
*   Keeps related code together
*   Simplifies navigation
*   Makes it easier to maintain and modify features

If you want to learn more, check out my article about [**vertical slice architecture**](https://www.milanjovanovic.tech/blog/vertical-slice-architecture).

# Package by Component

A component is a cohesive group of related functionality with a well-defined interface. Component-based organization is more coarse-grained than feature folders. Think of it as a mini application that handles one specific business capability.

This is very similar to how I define modules in a [**modular monolith**](https://www.milanjovanovic.tech/blog/what-is-a-modular-monolith).

Here’s what a component-based organization looks like:

📁 MyApp.Web
|\_\_ 📁 Controllers
    |\_\_ #️⃣ PoliciesController.cs
📁 MyApp.Policies
|\_\_ #️⃣ PoliciesComponent.cs     // Public interface
|\_\_ #️⃣ PolicyService.cs         // Implementation detail
|\_\_ #️⃣ PolicyRepository.cs      // Implementation detail

The key difference? Only `PoliciesComponent` is public. Everything else is internal to the component.

![A diagram illustrating a "package by component" architecture, showing `MyApp.Web` depending on a public `IPoliciesComponent` within `MyApp.Policies`, with internal implementation details like `PolicyService` and `PolicyRepository`.](https://miro.medium.com/v2/resize:fit:491/1*zuwAo-QqkARFELWp1EDTwg.png)

This means:

*   No bypassing layers
*   Clear dependencies
*   Real encapsulation
*   Business intent visible in the structure

# Which One Should You Choose?

Choose **Package by Feature** when:

*   You have many small, independent features
*   Your features don’t share much code
*   You want maximum flexibility

Choose **Package by Component** when:

*   You have clear business capabilities
*   You want strong encapsulation
*   You might split into microservices later

Both approaches achieve what Clean Architecture really wants: proper dependency management and business focus.

Here’s a side-by-side comparison of these architectural approaches:

![A side-by-side comparison diagram of three architectural approaches: traditional layering, package by feature, and package by component, highlighting public and internal components.](https://miro.medium.com/v2/resize:fit:700/1*PyTrdyob7ARoi0E4xYKchg.png)

Greyed-out types are internal to the defining assembly.

In the Missing Chapter of Clean Architecture, Simon Brown argues strongly for package by component. The key insight is that components are the natural way to slice a system. They represent complete business capabilities, not just technical features.

My recommendation? Start with package by component. Within the component, organize around features.

# Practical Examples

Let’s transform a typical layered application into a clean, component-based structure. We’ll use an insurance policy system as an example.

# The Traditional Way

Here’s how most developers structure their solution:

```csharp
// MyApp.Data  
public interface IPolicyRepository  
{  
    Task<Policy> GetByIdAsync(string policyNumber);  
    Task SaveAsync(Policy policy);  
}  
  
// MyApp.Business  
public class PolicyService : IPolicyService  
{  
    private readonly IPolicyRepository _repository;  
  
    public PolicyService(IPolicyRepository repository)  
    {  
        _repository = repository;  
    }  
  
    public async Task RenewPolicyAsync(string policyNumber)  
    {  
        var policy = await _repository.GetByIdAsync(policyNumber);  
        // Business logic here  
        await _repository.SaveAsync(policy);  
    }  
}  
  
// MyApp.Web  
public class PoliciesController : ControllerBase  
{  
    private readonly IPolicyService _policyService;  
  
    public PoliciesController(IPolicyService policyService)  
    {  
        _policyService = policyService;  
    }  
  
    [HttpPost("renew/{policyNumber}")]  
    public async Task<IActionResult> RenewPolicy(string policyNumber)  
    {  
        await _policyService.RenewPolicyAsync(policyNumber);  
        return Ok();  
    }  
}
```

The problem? Everything is public. Any class can bypass the service and go straight to the repository.

# The Clean Way

Here’s the same functionality organized as a proper component:

```csharp
// The only public contract  
public interface IPoliciesComponent  
{  
    Task RenewPolicyAsync(string policyNumber);  
}  
  
// Everything below is internal to the component  
internal class PoliciesComponent : IPoliciesComponent  
{  
    private readonly IRenewPolicyHandler _renewPolicyHandler;  
  
    // Public constructor for DI  
    public PoliciesComponent(IRenewPolicyHandler renewPolicyHandler)  
    {  
        _renewPolicyHandler = renewPolicyHandler;  
    }  
  
    public async Task RenewPolicyAsync(string policyNumber)  
    {  
        await _renewPolicyHandler.HandleAsync(policyNumber);  
    }  
}  
  
internal interface IRenewPolicyHandler  
{  
    Task HandleAsync(string policyNumber);  
}  
  
internal class RenewPolicyHandler : IRenewPolicyHandler  
{  
    private readonly IPolicyRepository _repository;  
  
    internal RenewPolicyHandler(IPolicyRepository repository)  
    {  
        _repository = repository;  
    }  
  
    public async Task HandleAsync(string policyNumber)  
    {  
        var policy = await _repository.GetByIdAsync(policyNumber);  
        // Business logic for policy renewal here  
        await _repository.SaveAsync(policy);  
    }  
}  
  
internal interface IPolicyRepository  
{  
    Task<Policy> GetByIdAsync(string policyNumber);  
    Task SaveAsync(Policy policy);  
}
```

The key improvements are:

1.  **Single public interface** — Only `IPoliciesComponent` is public. Everything else is internal.
2.  **Protected dependencies** — No way to bypass the component and access the repository directly.
3.  **Clear dependencies** — All dependencies flow inward through the component.
4.  **Proper encapsulation** — Implementation details are truly hidden.

This is how you would register the services with dependency injection:

```csharp
services.AddScoped<IPoliciesComponent, PoliciesComponent>();  
services.AddScoped<IRenewPolicyHandler, RenewPolicyHandler>();  
services.AddScoped<IPolicyRepository, SqlPolicyRepository>();
```

This structure enforces Clean Architecture principles through compiler-checked boundaries, not just conventions.

The compiler won’t let you bypass the component’s public interface. That’s much stronger than hoping developers follow the rules.

# Best Practices and Limitations

Let’s discuss something that is often overlooked: the practical limitations of enforcing Clean Architecture in .NET.

## The Limits of Encapsulation

The `internal` keyword in .NET provides protection within a single assembly. Here's what that means in practice:

```csharp
// In a single project:  
public interface IPoliciesComponent { } // Public contract  
internal class PoliciesComponent : IPoliciesComponent { }  
internal class PolicyRepository { }  
  
// Someone could still do this:  
public class BadPoliciesComponent : IPoliciesComponent  
{  
    public BadPoliciesComponent()  
    {  
        // Nothing stops them from creating a bad implementation  
    }  
}
```

While `internal` helps, it doesn't prevent all architectural violations.

## The Trade-offs

Some teams split their code into separate assemblies for stronger encapsulation:

MyCompany.Policies.Core.dll
MyCompany.Policies.Infrastructure.dll
MyCompany.Policies.Api.dll

This comes with trade-offs:

1.  **More complex build process** — Multiple projects need to be compiled and referenced.
2.  **Harder navigation** — Jumping between assemblies in the IDE is slower.
3.  **Deployment complexity** — More DLLs to manage and deploy.

## A Pragmatic Approach

Here’s what I recommend:

1.  **Use a single assembly**

*   Keep related code together
*   Use `internal` for implementation details
*   Make only the component interfaces public
*   Add `sealed` to prevent inheritance when possible

**2\. Enforce through architecture testing**

*   Add architecture tests to verify dependencies
*   Automatically check for architectural violations
*   Fail the build if someone bypasses the rules

```csharp
[Fact]  
public void Controllers_Should_Only_Depend_On_Component_Interfaces()  
{  
    var result = Types.InAssembly(Assembly.GetExecutingAssembly())  
        .That()  
        .ResideInNamespace("MyApp.Controllers")  
        .Should()  
        .OnlyDependOn(type =>  
            type.Name.EndsWith("Component") ||  
            type.Name.StartsWith("IPolicy"))  
        .GetResult();  
  
    result.IsSuccessful.Should().BeTrue();  
}
```

Want to learn more about enforcing architecture through testing? Check out my article on [**architecture testing**](https://www.milanjovanovic.tech/blog/enforcing-software-architecture-with-architecture-tests).

Remember: Clean Architecture is about managing dependencies, not about achieving perfect encapsulation. Use the tools the language gives you, but don’t over-complicate things chasing an impossible ideal.

# Conclusion

Clean Architecture isn’t about projects, folders, or perfect encapsulation.

It’s about:

*   Organizing code around business capabilities
*   Managing dependencies effectively
*   Keeping related code together
*   Making boundaries explicit

Start with a single project. Use components. Make interfaces public and implementations internal. Add architecture tests if you need more control.

And remember: **pragmatism beats purism**. Your architecture should help you ship features faster, not slow you down with artificial constraints.

Want to learn more? Check out my [**Pragmatic Clean Architecture**](https://www.milanjovanovic.tech/pragmatic-clean-architecture) course, where I’ll show you how to build maintainable applications with proper boundaries, clear dependencies, and business-focused components.

That’s all for today. Stay awesome, and I’ll see you next week.

_Originally published at_ [_https://www.milanjovanovic.tech_](https://www.milanjovanovic.tech/blog/clean-architecture-the-missing-chapter) _on November 2, 2024._

**P.S. Whenever you’re ready, there are 3 ways I can help you:**

1.  [**Pragmatic Clean Architecture:**](https://www.milanjovanovic.tech/pragmatic-clean-architecture) Join 3,150+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [**Modular Monolith Architecture:**](https://www.milanjovanovic.tech/modular-monolith-architecture) Join 1,050+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [**Patreon Community:**](https://www.patreon.com/milanjovanovic) Join a community of 1,050+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.