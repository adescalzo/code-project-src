```yaml
---
title: "TheCodeMan | Master .NET Technologies"
source: https://thecodeman.net/posts/dotnet10-extension?utm_source=Newsletter&utm_medium=email&utm_campaign=TheCodeMan%20Newsletter%20-%20%20.NET%2010%20extension%20-%20New%20Keyword%20v2
date_published: unknown
date_captured: 2025-08-08T14:17:06.814Z
domain: thecodeman.net
author: Unknown
category: ai_ml
technologies: [.NET 10, C# 14, ASP.NET Core, HttpContext]
programming_languages: [C#]
tags: [dotnet, csharp, extension-methods, language-features, code-organization, clean-architecture, multi-tenancy, saas, programming-patterns]
key_concepts: [extension-blocks, extension-properties, private-backing-fields, static-extension-blocks, code-organization, domain-modeling, lazy-caching, httpcontext-extension]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces the new `extension` keyword in .NET 10 (C# 14 Preview 3), highlighting its capabilities beyond traditional extension methods. It explains how extension blocks allow for extension properties, private backing fields for caching, and cohesive grouping of logic. The author demonstrates real-world use cases, such as managing HttpContext in multi-tenant SaaS applications and extending third-party DTOs without modifying the original class. This new feature aims to improve code organization, promote cleaner architecture, and enhance maintainability.
---
```

# TheCodeMan | Master .NET Technologies

### .NET 10 Extension

Apr 15 2025

### New "extension" keyword in .NET 10

With .NET 10 extension feature now you can define **extension block**s for any type. It is shaping up to be one of the most exciting updates in recent years - and one of the most underrated gems is the new extension keyword introduced in C# 14 (Preview 3).

If you thought extension methods were powerful before, this takes things to a whole new level.

In this article, I’ll break down:

*   What the new extension feature does
*   Why it matters
*   Real-world use cases you couldn’t solve cleanly before
*   And how it changes the way we organize logic around existing types

### Wait, what’s wrong with Extension Methods?

With the new syntax, you can define extension blocks for any type.

These blocks allow:

*   Extension **properties**
*   Private **backing fields** inside the extension scope
*   Grouped logic in a clean and local context
*   Even **static extension blocks** for related helpers

Syntax:

```csharp
public static class MyExtensions
{
    extension(TargetType instance)
    {
        public ReturnType PropertyOrMethod => ...;
    }

    extension static
    {
        public static ReturnType StaticHelper() => ...;
    }
}
```

### Real-World Use Case: Multi-Tenant SaaS and HttpContext

Let’s say you’re building a multi-tenant SaaS product. You need to:

*   Extract the TenantId from claims
*   Parse and validate the UserId
*   Check if the user is a tenant admin
*   Read custom headers
*   Expose defaults and validation logic

The old way? You’d create scattered static methods.

The new way? One structured block with everything grouped and cached.

```csharp
public static class MultiTenantHttpContextExtensions
{
    extension(HttpContext ctx)
    {
        private string? _tenantId;
        private Guid? _userId;

        public string TenantId =>
            _tenantId ??=
                ctx.User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value
                ?? throw new UnauthorizedAccessException("Tenant ID missing");

        public Guid UserId =>
            _userId ??=
                Guid.TryParse(
                    ctx.User.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value, out var id)
                ? id
                : throw new UnauthorizedAccessException("User ID invalid");

        public bool IsTenantAdmin =>
            ctx.User.IsInRole("Admin") || ctx.Request.Headers["X-Tenant-Admin"] == "true";

        public string? GetHeader(string name) =>
            ctx.Request.Headers.TryGetValue(name, out var value)
                ? value.ToString()
                : null;
    }

    extension static
    {
        public static string DefaultTenantId => "public";

        public static bool IsValidTenantId(string? id) =>
            !string.IsNullOrWhiteSpace(id) && id.All(char.IsLetterOrDigit);
    }
}
```

This approach brings:

*   Readable, organized logic
*   Lazy caching via private fields (_tenantId,_ userId)
*   Cohesive domain logic scoped to the right context

### Another Example: Extending External Models

Let’s say you’re working with a third-party DTO:

```csharp
public class OrderDto
{
    public List<OrderItemDto> Items { get; set; }
    public string Status { get; set; }
}
```

With new extensions, you can wrap domain logic around it without modifying the class:

```csharp
public static class OrderExtensions
{
    extension(OrderDto order)
    {
        public decimal TotalAmount =>
            order.Items.Sum(i => i.Quantity * i.PricePerUnit);

        public bool IsComplete =>
            order.Status == "Completed";

        public int TotalItems =>
            order.Items.Sum(i => i.Quantity);
    }
}
```

Usage becomes clean and expressive:

```csharp
if (order.TotalAmount > 1000 && order.IsComplete)
{
    // Do something
}
```

### Why not just use methods from .NET 10?

![Comparing with Extension Methods](/images/blog/posts/dotnet10-extension/comparing-with-extension-methods.png)
_Description: A comparison table showing the differences between "Traditional Extension Methods" and the "New extension Keyword" in C# 14. The table highlights that while both support methods, the new extension keyword additionally supports properties, private fields (for caching), logical grouping (block-based), and static helpers in the same file, which traditional extension methods do not._

This isn't just a cosmetic change - it's a paradigm shift in how we organize logic.

### My Gotchas & Notes

*   This is currently available in **.NET 10 Preview 3** with C# 14
*   Still being refined, so expect updates in syntax and tooling support
*   All your existing extension methods continue to work - this is purely additive

### My Advice

Use this **when you’re extending a type with multiple behaviors** that feel like they belong together:

*   DTO transformations
*   HTTP context and claims logic
*   Computed values on external models
*   Domain-specific enhancements (e.g., IsActive, TotalAmount, NeedsSync, etc.)

It keeps your logic close, clean, and cache-friendly - and once you start using it, you won’t want to go back.

### Wrapping Up

The new extension keyword isn’t flashy at first glance. But it solves a real pain point for developers who care about clean architecture, rich domain modeling, and maintainable code.

It gives us the power we didn’t even know we were missing in C# - and it’s just the beginning of a new era of expressiveness.

Let me know if you'd like to see how I plan to use this in:

*   CQRS helpers
*   ViewModel builders
*   ASP.NET Core service pipelines

Because I already note what I can refactor for some of my older projects.

That's all from me today.

P.S. Follow me on [YouTube](https://www.youtube.com/@thecodeman_).