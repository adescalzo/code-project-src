```yaml
---
title: Architecture Tests in .NET
source: https://www.nikolatech.net/blogs/architecture-tests-in-dotnet
date_published: 2025-01-16T02:29:31.000Z
date_captured: 2025-08-08T13:20:22.812Z
domain: www.nikolatech.net
author: Unknown
category: architecture
technologies: [.NET, NetArchTest.Rules, XUnit, NuGet, GitHub, CI/CD pipelines]
programming_languages: [C#, bash]
tags: [architecture-tests, dotnet, clean-architecture, software-design, testing, technical-debt, code-quality, netarchtest, xunit, ci-cd]
key_concepts: [architecture-tests, clean-architecture, layered-separation, coupling-restriction, naming-conventions, inheritance-rules, technical-debt, fluent-api]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces architectural tests as an effective method to enforce design rules and prevent technical debt in software systems. It explains how these automated checks verify compliance with architectural principles, similar to unit tests but for structure. The post details how to get started with NetArchTest, a .NET library, showcasing its fluent API for defining rules. Practical examples are provided for enforcing layering, inheritance, and naming conventions within a clean architecture project. Ultimately, the article emphasizes that integrating architectural tests into CI/CD pipelines ensures consistent code quality and adherence to design goals.
---
```

# Architecture Tests in .NET

#### Architecture Tests in .NET

###### 16 Jan 2025

###### 5 min

Software architecture outlines the structure of a system, detailing the organization of its components, their interactions and guiding principles.

It acts as a blueprint with a set of rules to ensure the code you deliver is well-organized, reliable, secure and future-proof.

In large teams with varying levels of experience, adhering to architectural rules can be challenging.

Without a clear strategy, violations can easily be overlooked, leading to technical debt and problems with scalability and maintainability in future.

One effective solution is to introduce architectural tests to help safeguard and enforce these principles.

## Architecture Tests

**Architecture Tests** are automated checks that verify a software system complies with established architectural rules, principles or design patterns.

Think of them as unit tests, but instead of validating business logic or functionality, they ensure compliance with architectural rules and constraints.

With architecture tests you should be able to:

*   Enforce layered separation
*   Restrict coupling
*   Enforce naming conventions
*   Detect architectural violations early and more

You could also add architectural tests to your CI/CD pipelines to stop violations from creeping into the codebase over time.

## Getting Started with Architecture Tests

To get started with architecture tests, you first need to create a new test project. I prefer using **XUnit** for my test projects.

Inside your test project, you need to install the NetArchTest.Rules NuGet package. You can do this via the NuGet Package Manager or by running the following command in the Package Manager Console:

```bash
Install-Package NetArchTest.Rules
```

### NetArchTest

**NetArchTest** is an amazing library for testing the architecture of .NET applications. It supports testing layer boundaries, project dependencies and class relationships.

It offers a fluent API that allows you to create readable rules that can be used in test assertions.

The starting point for any rule is the static **Types** class, where you load a set of types from a path, assembly or namespace:

```csharp
var result = Types.InAssembly(typeof(MyClass).Assembly);
```

After selecting the types, you can filter them using one or more predicates, which can be combined with **And()** or **Or()** conjunctions:

```csharp
var result = Types.InAssembly(typeof(MyClass).Assembly)
    .That()
    .ResideInNamespace("My.Namespace");
```

After filtering the classes, you can apply a set of conditions using the **Should()** or **ShouldNot()** methods:

```csharp
var result = Types.InAssembly(typeof(MyClass).Assembly)
    .That()
    .ResideInNamespace("My.Namespace")
    .Should()
    .BeSealed();
```

Finally, you can retrieve a result from the rule by using GetTypes() to fetch the types that match the rule, or GetResult() to verify if the rule has been satisfied.

```csharp
var result = Types.InAssembly(typeof(MyClass).Assembly)
    .That()
    .ResideInNamespace("My.Namespace")
    .Should()
    .BeSealed()
    .GetResult();
```

The result will also include a list of types that did not meet the conditions.

Additionally, you can extend the library by creating custom rules that implement the ICustomRule interface and group rules into policies using the fluent interface provided by the Policy class.

Feel free to check out the project on GitHub and give it a star: [NetArchTest Github](https://github.com/BenMorris/NetArchTest)

### Project Setup

For this blog post, we'll use a sample project based on clean architecture to explore additional architecture tests you can incorporate into your project.

In Clean Architecture, the direction of dependencies must always point inward. This means that the outer layers can depend on the inner layers, but inner layers should not depend on outer layers.

In sample project we have:

*   Domain
*   Business (Application Layer)
*   Presentation
*   Persistence

![Clean Architecture Diagram](https://coekcx.github.io/BlogImages/images/clean-architecture.png)
*Description: A diagram illustrating Clean Architecture with concentric circles labeled "Presentation", "Business", "Domain", and "Infrastructure". Arrows point inwards from outer layers to inner layers, indicating dependency flow.*

When writing architecture tests, you'll often need assemblies, which is why I prefer to define them in a central location. You can either define them in a separate file or create a base test for them.

```csharp
public class ArchitectureTests
{
    public static readonly Assembly Domain = typeof(Domain.AssemblyReference).Assembly;
    public static readonly Assembly Business = typeof(Business.AssemblyReference).Assembly;
    public static readonly Assembly Persistence = typeof(Persistence.AssemblyReference).Assembly;
    public static readonly Assembly Presentation = typeof(Program).Assembly;

    // ...
}
```

### Layering Rules

In our solution, the Domain project serves as the core and should not depend on any other project. The Business layer orchestrates various use cases and depends on the Domain. Finally, both the Presentation and Persistence layers depend on the Business logic and Domain.

Here is an simple example to ensure that Domain should not have dependencies on Business, Persistence and Presentation layers:

```csharp
[Fact]
public void Domain_ShouldNotHaveDependencyOnBusiness()
{
    var result = Types.InAssembly(Domain)
        .Should()
        .NotHaveDependencyOn(Business.GetName().Name)
        .GetResult();

    result.IsSuccessful.ShouldBeTrue();
}

[Fact]
public void Domain_ShouldNotHaveDependencyOnPersistence()
{
    var result = Types.InAssembly(Domain)
        .Should()
        .NotHaveDependencyOn(Persistence.GetName().Name)
        .GetResult();

    result.IsSuccessful.ShouldBeTrue();
}

[Fact]
public void Domain_ShouldNotHaveDependencyOnPresentation()
{
    var result = Types.InAssembly(Domain)
        .Should()
        .NotHaveDependencyOn(Presentation.GetName().Name)
        .GetResult();

    result.IsSuccessful.ShouldBeTrue();
}
```

You could also verify multiple dependencies in one test if you like that approach more:

```csharp
[Fact]
public void Domain_ShouldNotHaveDependencyOnOtherProjects()
{
    var result = Types.InAssembly(Domain)
        .Should()
        .NotHaveDependencyOnAny(
            Business.GetName().Name,
            Persistence.GetName().Name,
            Presentation.GetName().Name)
        .GetResult();

    result.IsSuccessful.ShouldBeTrue();
}
```

### Inheritence Rules

Another interesting rule you might want to enforce is inheritance.

For instance, ensure that every class within the Domain.Entities namespace inherits from a specific base class, such as Entity:

```csharp
[Fact]
public void Entities_ShouldImplementEntity()
{
    var result = Types.InAssembly(Domain)
        .That()
        .AreClasses()
        .And()
        .ResideInNamespace("Domain.Entities")
        .Should()
        .Inherit(typeof(Entity<>))
        .GetResult();

    result.IsSuccessful.ShouldBeTrue();
}
```

Alternatively, you may want to ensure that classes like requests or commands are sealed to prevent unwanted inheritance.

```csharp
[Fact]
public void Requests_ShouldBeSealed()
{
    var result = Types.InAssembly(Business)
        .That()
        .ImplementInterface(typeof(IRequest<>))
        .Should()
        .BeSealed()
        .GetResult();

    result.IsSuccessful.ShouldBeTrue();
}
```

### Naming Conventions

Naming conventions are widely used and help maintain consistency, but they can be easily overlooked or broken.

Without strict adherence, it becomes difficult to ensure clarity and avoid confusion in large codebases. Here is an simple example how you could enforce naming conventions as well:

```csharp
[Fact]
public void Handlers_ShouldHaveNameEndingWithHandler()
{
    var result = Types.InAssembly(Business)
        .That()
        .ImplementInterface(typeof(IRequestHandler<>))
        .Should()
        .HaveNameEndingWith("Handler")
        .GetResult();

    result.IsSuccessful.ShouldBeTrue();
}
```

## Conclusion

Introducing architectural tests is an effective way to enforce rules and prevent violations, reducing the risk of technical debt.

Whether verifying dependencies, enforcing inheritance rules or maintaining naming conventions, architectural tests are a powerful tool for building and maintaining robust systems.

They not only keep code consistent but also reinforce best practices, ensuring that the architecture remains well aligned with the system's design goals.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/architecture-test-examples)

![Architecture Tests Banner](https://coekcx.github.io/BlogImages/banners/architecture-tests-in-dotnet-banner.png)
*Description: A blue and black banner with the text "Architecture Tests" prominently displayed, and smaller text "Enforce Design Rules in .NET". A stylized "NK" logo is in the top left corner, and a magnifying glass with a checkmark is in the bottom left.*