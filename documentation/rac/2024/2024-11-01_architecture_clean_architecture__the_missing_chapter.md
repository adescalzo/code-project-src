```yaml
---
title: "Clean Architecture: The Missing Chapter"
source: https://www.milanjovanovic.tech/blog/clean-architecture-the-missing-chapter?utm_source=LinkedIn&utm_medium=social&utm_campaign=18.08.2025
date_published: 2024-11-02T00:00:00.000Z
date_captured: 2025-08-20T17:46:14.171Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: architecture
technologies: [.NET, ASP.NET Core, Telerik, xUnit, SQL]
programming_languages: [C#, SQL]
tags: [clean-architecture, software-architecture, dotnet, code-organization, design-patterns, dependency-management, modular-monolith, vertical-slice-architecture, architecture-testing, web-api]
key_concepts: [Clean Architecture, Layered Architecture, Package by Feature, Package by Component, Vertical Slice Architecture, Modular Monolith, Dependency Injection, Architecture Testing]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article clarifies that Clean Architecture focuses on dependency management and business capabilities, not just folder structure. It critiques traditional layer-based organization for scattering features and creating hidden dependencies. The author advocates for "package by feature" (vertical slice architecture) or "package by component" (modular monolith) approaches, illustrating them with practical C# code examples. The piece emphasizes using `internal` access modifiers for encapsulation and suggests architecture testing to enforce design rules. Ultimately, it promotes a pragmatic approach to architecture that prioritizes shipping features efficiently.]
---
```

# Clean Architecture: The Missing Chapter

![Blog cover image for 'Clean Architecture: The Missing Chapter' featuring the title and 'MJ Tech' logo on a blue and black abstract background.](/blog-covers/mnw_114.png?imwidth=3840)

# Clean Architecture: The Missing Chapter

8 min read · November 02, 2024

Remember when APIs were an afterthought? We're [**not going back**](https://shortclick.link/djx5wv). 74% of devs surveyed in the 2024 State of the API Report are API-first, up from 66% in 2023. What else? We're getting faster. 63% of devs can produce an API within a week, up from 47% in 2023. Collaboration on APIs is still a challenge. APIs are increasingly revenue generators. Read the [**rest of the report**](https://shortclick.link/djx5wv) for fresh insights.

New Report: [**State of Designer-Developer Collaboration 2024**](https://www.telerik.com/design-system/designer-developer-collaboration-survey-2024/?utm_medium=cpm&utm_source=milanjovanovich&utm_campaign=dt_survey_design_story_dev_collab_report). Whether you're on the development or design end of creating web applications, it's easy to lose track of the world beyond your daily work. This brand-new report will give you some interesting insights and in-depth analysis on the relationship between designers and developers, including excellent ideas for making it more efficient and satisfying. [**Check it out**](https://www.telerik.com/design-system/designer-developer-collaboration-survey-2024/?utm_medium=cpm&utm_source=milanjovanovich&utm_campaign=dt_survey_design_story_dev_collab_report).

[

Sponsor this newsletter

](/sponsor-the-newsletter)

I see the same mistake happen over and over again.

Developers discover Clean Architecture, get excited about its principles, and then... they turn the famous Clean Architecture diagram into a project structure.

But here's the thing: **Clean Architecture is not about folders**. It's about dependencies.

Simon Brown wrote a "missing chapter" for Uncle Bob's Clean Architecture book that addresses exactly this issue. Yet somehow, this crucial message got lost along the way.

Today, I'll show you what Uncle Bob's Clean Architecture diagram really means and how you should actually organize your code. We'll look at practical examples that you can use in your projects right now.

Let's clear up this common misconception once and for all.

## [The Problem With Traditional Layering](#the-problem-with-traditional-layering)

Almost every .NET developer has built a solution that looks like this:

*   `MyApp.Web` for controllers and views
*   `MyApp.Business` for services and business logic
*   `MyApp.Data` for repositories and data access

It's the default approach. It's what we see in tutorials. It's what we teach juniors.

And it's completely wrong.

### [Why Layer-Based Organization Fails](#why-layer-based-organization-fails)

When you organize code by technical layers, you scatter related components across multiple projects. A single feature, like managing policies, ends up spread across your entire codebase:

*   Policies controller in the Web layer
*   Policy service in the Business layer
*   Policy repository in the Data layer

Here's what you'll see when looking at the folder structure:

```text
📁 MyApp.Web
|__ 📁 Controllers
    |__ #️⃣ PoliciesController.cs
📁 MyApp.Business
|__ 📁 Services
    |__ #️⃣ PolicyService.cs
📁 MyApp.Data
|__ 📁 Repositories
    |__ #️⃣ PolicyRepository.cs
```

Here's a visual representation of the layer-based architecture:

![Diagram illustrating feature scattering in a traditional layer-based architecture, showing MyApp.Web, MyApp.Business, and MyApp.Data projects with dependencies flowing downwards.](/blogs/mnw_114/layered_architecture.png?imwidth=640)

This fragmentation creates several problems:

1.  **Violates Common Closure Principle** - Classes that change together should stay together. When your "Policies" feature changes, you're touching three different projects.
    
2.  **Hidden dependencies** - Public interfaces everywhere make it possible to bypass layers. Nothing stops a controller from directly accessing a repository.
    
3.  **No business intent** - Opening your solution tells you nothing about what the application does. It only shows technical implementation details.
    
4.  **Harder maintenance** - Making changes requires jumping between multiple projects.
    

The worst part? This approach doesn't even achieve what it promises. Despite the separate projects, you often end up with a "big ball of mud" because public access modifiers allow any class to reference any other class.

### [The Real Intent of Layers](#the-real-intent-of-layers)

Clean Architecture's circles were never meant to represent projects or folders. They represent different levels of policy, with dependencies pointing inward toward business rules.

You can achieve this without splitting your code into artificial technical layers.

Let me show you a better way.

## [Better Approaches to Code Organization](#better-approaches-to-code-organization)

Instead of splitting your code by technical layers, you have two better options: **package by feature** or **package by component**.

Let's look at both.

### [Package by Feature](#package-by-feature)

Organizing by feature is a solid option. Each feature gets its own namespace and contains everything needed to implement that feature.

```text
📁 MyApp.Policies
|__ 📁 RenewPolicy
    |__ #️⃣ RenewPolicyCommand.cs
    |__ #️⃣ RenewPolicyHandler.cs
    |__ #️⃣ PolicyValidator.cs
    |__ #️⃣ PolicyRepository.cs
|__ 📁 ViewPolicyHistory
    |__ #️⃣ PolicyHistoryQuery.cs
    |__ #️⃣ PolicyHistoryHandler.cs
    |__ #️⃣ PolicyHistoryViewModel.cs
```

Here's a diagram representing this structure:

![Diagram illustrating vertical slice architecture for 'package by feature' organization, where features like 'RenewPolicy' and 'ViewPolicyHistory' contain all related components within their own namespaces.](/blogs/mnw_114/feature_folder_architecture.png?imwidth=640)

This approach:

*   Makes features explicit
*   Keeps related code together
*   Simplifies navigation
*   Makes it easier to maintain and modify features

If you want to learn more, check out my article about [**vertical slice architecture**](vertical-slice-architecture).

### [Package by Component](#package-by-component)

A component is a cohesive group of related functionality with a well-defined interface. Component-based organization is more coarse-grained than feature folders. Think of it as a mini application that handles one specific business capability.

This is very similar to how I define modules in a [**modular monolith**](what-is-a-modular-monolith).

Here's what a component-based organization looks like:

```text
📁 MyApp.Web
|__ 📁 Controllers
    |__ #️⃣ PoliciesController.cs
📁 MyApp.Policies
|__ #️⃣ PoliciesComponent.cs     // Public interface
|__ #️⃣ PolicyService.cs         // Implementation detail
|__ #️⃣ PolicyRepository.cs      // Implementation detail
```

The key difference? Only `PoliciesComponent` is public. Everything else is internal to the component.

![Diagram illustrating component-based architecture, showing MyApp.Web depending on MyApp.Policies component, with internal implementation details hidden.](/blogs/mnw_114/component_architecture.png?imwidth=640)

This means:

*   No bypassing layers
*   Clear dependencies
*   Real encapsulation
*   Business intent visible in the structure

### [Which One Should You Choose?](#which-one-should-you-choose)

Choose **Package by Feature** when:

*   You have many small, independent features
*   Your features don't share much code
*   You want maximum flexibility

Choose **Package by Component** when:

*   You have clear business capabilities
*   You want strong encapsulation
*   You might split into microservices later

Both approaches achieve what Clean Architecture really wants: proper dependency management and business focus.

Here's a side-by-side comparison of these architectural approaches:

![Comparison diagram showing layered, vertical slice, and component architectural approaches side-by-side, highlighting how different components are organized and their visibility.](/blogs/mnw_114/architecture_comparison.png?imwidth=3840)

Greyed-out types are internal to the defining assembly.

In the Missing Chapter of Clean Architecture, Simon Brown argues strongly for package by component. The key insight is that components are the natural way to slice a system. They represent complete business capabilities, not just technical features.

My recommendation? Start with package by component. Within the component, organize around features.

## [Practical Examples](#practical-examples)

Let's transform a typical layered application into a clean, component-based structure. We'll use an insurance policy system as an example.

### [The Traditional Way](#the-traditional-way)

Here's how most developers structure their solution:

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

### [The Clean Way](#the-clean-way)

Here's the same functionality organized as a proper component:

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

1.  **Single public interface** - Only `IPoliciesComponent` is public. Everything else is internal.
    
2.  **Protected dependencies** - No way to bypass the component and access the repository directly.
    
3.  **Clear dependencies** - All dependencies flow inward through the component.
    
4.  **Proper encapsulation** - Implementation details are truly hidden.
    

This is how you would register the services with dependency injection:

```csharp
services.AddScoped<IPoliciesComponent, PoliciesComponent>();
services.AddScoped<IRenewPolicyHandler, RenewPolicyHandler>();
services.AddScoped<IPolicyRepository, SqlPolicyRepository>();
```

This structure enforces Clean Architecture principles through compiler-checked boundaries, not just conventions.

The compiler won't let you bypass the component's public interface. That's much stronger than hoping developers follow the rules.

## [Best Practices and Limitations](#best-practices-and-limitations)

Let's discuss something that is often overlooked: the practical limitations of enforcing Clean Architecture in .NET.

### [The Limits of Encapsulation](#the-limits-of-encapsulation)

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

### [The Trade-offs](#the-trade-offs)

Some teams split their code into separate assemblies for stronger encapsulation:

```plaintext
MyCompany.Policies.Core.dll
MyCompany.Policies.Infrastructure.dll
MyCompany.Policies.Api.dll
```

This comes with trade-offs:

1.  **More complex build process** - Multiple projects need to be compiled and referenced.
2.  **Harder navigation** - Jumping between assemblies in the IDE is slower.
3.  **Deployment complexity** - More DLLs to manage and deploy.

### [A Pragmatic Approach](#a-pragmatic-approach)

Here's what I recommend:

1.  **Use a single assembly**
    
    *   Keep related code together
    *   Use `internal` for implementation details
    *   Make only the component interfaces public
    *   Add `sealed` to prevent inheritance when possible
2.  **Enforce through architecture testing**
    
    *   Add architecture tests to verify dependencies
    *   Automatically check for architectural violations
    *   Fail the build if someone bypasses the rules

```csharp
[Fact]
public void Controllers_Should_Only_Depend_On_Component_Interfaces()
{
    var allTypes = Types.InAssembly(Assembly.GetExecutingAssembly());

    TestResult? result = allTypes
        .That()
        .ResideInNamespace("MyApp.Controllers")
        .Should()
        .OnlyHaveDependenciesOn(
            allTypes
                .That()
                .HaveNameEndingWith("Component")
                .Or()
                .HaveNameStartingWith("IPolicy")
                .GetTypes()
                .Select(t => t.FullName!)
                .ToArray())
        .GetResult();

    result.IsSuccessful.Should().BeTrue();
}
```

Want to learn more about enforcing architecture through testing? Check out my article on [**architecture testing**](enforcing-software-architecture-with-architecture-tests).

Remember: Clean Architecture is about managing dependencies, not about achieving perfect encapsulation. Use the tools the language gives you, but don't over-complicate things chasing an impossible ideal.

## [Conclusion](#conclusion)

Clean Architecture isn't about projects, folders, or perfect encapsulation.

It's about:

*   Organizing code around business capabilities
*   Managing dependencies effectively
*   Keeping related code together
*   Making boundaries explicit

Start with a single project. Use components. Make interfaces public and implementations internal. Add architecture tests if you need more control.

And remember: **pragmatism beats purism**. Your architecture should help you ship features faster, not slow you down with artificial constraints.

Want to learn more? Check out my [**Pragmatic Clean Architecture**](/pragmatic-clean-architecture) course, where I'll show you how to build maintainable applications with proper boundaries, clear dependencies, and business-focused components.

That's all for today. Stay awesome, and I'll see you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,200+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

.formkit-form\[data-uid="134c4e25db"\] \*{box-sizing:border-box;}.formkit-form\[data-uid="134c4e25db"\]{-webkit-font-smoothing:antialiased;-moz-osx-font-smoothing:grayscale;}.formkit-form\[data-uid="134c4e25db"\] legend{border:none;font-size:inherit;margin-bottom:10px;padding:0;position:relative;display:table;}.formkit-form\[data-uid="134c4e25db"\] fieldset{border:0;padding:0.01em 0 0 0;margin:0;min-width:0;}.formkit-form\[data-uid="134c4e25db"\] body:not(:-moz-handler-blocked) fieldset{display:table-cell;}.formkit-form\[data-uid="134c4e25db"\] h1,.formkit-form\[data-uid="134c4e25db"\] h2,.formkit-form\[data-uid="134c4e25db"\] h3,.formkit-form\[data-uid="134c4e25db"\] h4,.formkit-form\[data-uid="134c4e25db"\] h5,.formkit-form\[data-uid="134c4e25db"\] h6{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] h2{font-size:1.5em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] h3{font-size:1.17em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] p{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]){text-align:left;}.formkit-form\[data-uid="134c4e25db"\] p:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] hr:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]){color:inherit;font-style:initial;}.formkit-form\[data-uid="134c4e25db"\] .ordered-list,.formkit-form\[data-uid="134c4e25db"\] .unordered-list{list-style-position:outside !important;padding-left:1em;}.formkit-form\[data-uid="134c4e25db"\] .list-item{padding-left:0;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="modal"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="slide in"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:none;}.formkit-sticky-bar .formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:block;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input,.formkit-form\[data-uid="134c4e25db"\] .formkit-select,.formkit-form\[data-uid="134c4e25db"\] .formkit-checkboxes{width:100%;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit{border:0;border-radius:5px;color:#ffffff;cursor:pointer;display:inline-block;text-align:center;font-size:15px;font-weight:500;cursor:pointer;margin-bottom:15px;overflow:hidden;padding:0;position:relative;vertical-align:middle;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus{outline:none;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus > span{background-color:rgba(0,0,0,0.1);}.formkit-form\[data-uid="134c4e25db"\] .formkit-button > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit > span{display:block;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;padding:12px 24px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input{background:#ffffff;font-size:15px;padding:12px;border:1px solid #e3e3e3;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;line-height:1.4;margin:0;-webkit-transition:border-color ease-out 300ms;transition:border-color ease-out 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:focus{outline:none;border-color:#1677be;-webkit-transition:border-color ease 300ms;transition:border-color ease 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit