```yaml
---
title: "🧩 Why Most .NET Developers Misuse Records — and What It’s Costing Them | by Yaseer Arafat | Jul, 2025 | Medium"
source: https://blog.yaseerarafat.com/why-most-net-developers-misuse-records-and-what-its-costing-them-afb17f9d05ae
date_published: 2025-08-02T04:57:22.136Z
date_captured: 2025-08-12T21:01:44.442Z
domain: blog.yaseerarafat.com
author: Yaseer Arafat
category: general
technologies: [.NET, C# 9, C# 10, Entity Framework Core, System.Text.Json, Newtonsoft.Json, RabbitMQ, Azure Service Bus, Blazor, SignalR]
programming_languages: [C#]
tags: [csharp, dotnet, records, immutability, value-objects, performance, ef-core, data-transfer-objects, best-practices, programming-patterns]
key_concepts: [record types, immutability, value-based-equality, reference-equality, change-tracking, value-objects, dto, performance-optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  This article critically analyzes the common misuses of C# record types by .NET developers, leading to bugs, broken equality, and performance issues. It details five common pitfalls, including using records with setters, as EF Core entities, or abusing `with` expressions in performance-critical paths. The author clarifies appropriate use cases for records, such as value objects, configuration models, DTOs, and event contracts, emphasizing their strength in immutability and value-based comparison. The post also provides benchmarks comparing records and classes, highlighting memory overhead and performance implications, and offers a rule of thumb for when to choose each type.
---
```

# 🧩 Why Most .NET Developers Misuse Records — and What It’s Costing Them | by Yaseer Arafat | Jul, 2025 | Medium

Member-only story

# 🧩 **Why Most .NET Developers Misuse Records — and What It’s Costing Them**

[

![Yaseer Arafat](https://miro.medium.com/v2/resize:fill:64:64/1*nYb2C46Z-oiTSkQ1tJweMw.jpeg)

](/?source=post_page---byline--afb17f9d05ae---------------------------------------)

[Yaseer Arafat](/?source=post_page---byline--afb17f9d05ae---------------------------------------)

Follow

7 min read

·

Jul 8, 2025

56

3

Listen

Share

More

C# 9 introduced `record` types with much fanfare—touted as a more concise, immutable-friendly alternative to classes. Since then, developers have enthusiastically adopted them across projects.

![](https://miro.medium.com/v2/resize:fit:700/1*qk9XftKnz46Dtdc2ZCzTwQ.png)

*Image: A graphic titled "Why Most .NET Developers Misuse Records — And What It's Costing Them". It shows a crossed-out C# logo in an orange square connected by a dashed line to a green square labeled "record" with a checkmark, implying a misuse or misunderstanding of C# records.*

But let’s be honest:
**Most .NET developers are using records wrong.**
And they’re silently paying the price — **in bugs, broken equality logic, failed EF Core queries, and performance bottlenecks**.

This isn’t just another “records vs classes” post. This is a battle-tested breakdown of where records _break things_, where they _make sense_, and when to avoid them altogether.

# 🔍 What Are Records — Really?

At a glance, records offer:

*   🔹 _Value-based equality_ (`==` compares values, not references)
*   🔹 _Immutability-by-default_ via positional constructors
*   🔹 _Concise syntax_ for models, DTOs, and value objects
*   🔹 _Support for with-expressions_ to clone with modification

But the danger? **They look like classes** — and that’s where the misuse begins.

# 🚫 Misusing Records: 5 Common Patterns That Hurt Your Code

## 1\. Using Records with Setters — and Thinking It’s Still Immutable

```csharp
public record User {  
    public string Name { get; set; }  
    public int Age { get; set; }  
}
```

This is syntactically a record, but **semantically a mutable class**. You’ve lost the entire benefit of record immutability.

```csharp
var user = new User { Name = "John", Age = 30 };  
user.Age = 35; // Mutation — no value object behavior here
```

✅ Use **positional syntax** for true immutability:

```csharp
public record User(string Name, int Age);
```

![](https://miro.medium.com/v2/resize:fit:700/1*y4CXhSo4hw0_2ndLjr0q6w.png)

*Image: A comparison showing "Mutable vs Immutable Record Syntax". On the left, a `public record MutablePerson` with `get; set;` properties is marked with two warning signs. On the right, a `public record ImmutablePerson(string firstName, string lastName)` using positional syntax is marked with a green checkmark.*

## 2\. Using Records as EF Core Entities

This one’s deadly.

EF Core **depends on reference equality** for change tracking. Records override `Equals` and `GetHashCode` by default, which breaks identity tracking.

```csharp
public record Product(int Id, string Name); // ❌ Don't do this in EF
```

✔️ Use plain classes for entities, not records:

```csharp
public class Product {  
    public int Id { get; set; }  
    public string Name { get; set; }  
}
```

![](https://miro.medium.com/v2/resize:fit:700/1*blTFFA26lyS6AwVjPgZxBg.png)

*Image: A diagram titled "Why EF Core + Record = Trouble". On the left, two "Record" boxes with "Id: 1, Name: Alice" are shown, with a red X indicating that records override equality, confusing identity tracking. On the right, two "Class" boxes with "Id: 1, Name: Alice" are shown, with a green checkmark indicating that EF expects identity tracking based on reference equality.*

## 3\. Abusing the `with` Expression in Performance-Critical Paths

```csharp
var updatedUser = user with { Age = 40 };
```

This looks elegant. But the `with` expression creates a **shallow copy**, which is wasteful in tight loops, serialization layers, or memory-constrained code.

✅ Clone only when needed. In perf-sensitive code, **prefer explicit constructors** or **mutable structs**.

![](https://miro.medium.com/v2/resize:fit:700/1*imTc1uZIFEn7R8gFN0LUJg.png)

*Image: A bar chart titled "Clone Cost in Hot Loops". It compares "with-expression" (red bars) and "constructor" (blue bars) for memory (bytes) and time (µs). The "with-expression" shows significantly higher memory and time (8.35s) compared to "constructor" (3.07s), indicating the performance overhead of `with` expressions.*

## 4\. Assuming Records Are Always Faster Than Classes

Records carry more overhead than you think:

*   Custom `Equals`
*   `GetHashCode`
*   Copy constructors
*   `ToString` overrides

For large object graphs or tight loops, this can hurt.

![](https://miro.medium.com/v2/resize:fit:700/1*8MdaagAw3uHoxm5ONQYlgQ.png)

*Image: A bar chart titled "Record vs Class in Collections". It shows "Record" taking 1.28 seconds (28% more) and "Class" taking 1.00 seconds in a 100,000-object loop, illustrating that records can be slower due to overhead.*

## 5\. Using `record class` When You Meant `record struct`

C# 10 added `record struct`, which allows value semantics + immutability — perfect for high-performance or interop scenarios.

| Type          | Heap vs Stack | Equality   | Use Case                       |
| :------------ | :------------ | :--------- | :----------------------------- |
| `record class` | Heap          | Value-based | DTOs, configs, API models      |
| `record struct` | Stack         | Value-based | Perf-sensitive value types     |

![](https://miro.medium.com/v2/resize:fit:700/1*rGgMWPeDdw9VAtQaa1YVVQ.png)

*Image: A table visually representing "record class vs record struct Usage Chart". It categorizes `record class` as Heap-allocated, Value-based equality, suitable for DTOs/configs/API models, and `record struct` as Stack-allocated, Value-based equality, suitable for perf-sensitive value types.*

# ✅ When You Should Use Records

**Records shine when immutability, value-based comparison, and intention-driven modeling are essential. Here’s where they truly belong:**

🔹 **Value Objects in Domain-Driven Design (DDD)**
When modeling concepts that have _no identity_ of their own (like `Money`, `Address`, or `Email`), records are perfect. You want two instances with the same values to be treated as equal — and that’s exactly what records provide.

```csharp
public record Money(decimal Amount, string Currency);
```

Why this works:

*   You care about the values, not _which_ instance it is.
*   Easy equality and comparisons.
*   Immutability ensures consistency across logic boundaries.

🔹 **Configuration Models and Options Patterns**
Immutable configuration types passed into services or bound from settings benefit from records.

```csharp
public record JwtSettings(string Issuer, string Secret, int ExpiryMinutes);
```

Why it works:

*   Clearly communicates immutability.
*   Avoids accidental mutation during runtime.
*   Simplifies testing and validation.

🔹 **Data Transfer Objects (DTOs)**
When returning structured, read-only data from an API, records can reduce boilerplate while ensuring safety.

```csharp
public record UserDto(string Name, string Email);
```

Why it works:

*   Ideal for serializing/deserializing fixed-shape data.
*   `with` expression makes transformation easy (e.g., masking data).
*   Easier comparisons for unit tests.

_⚠️ Caveat: Only if immutability is desired. For client-bound data that changes, consider using a class._

🔹 **Event and Message Contracts**
In event-driven systems, especially in CQRS or messaging platforms like RabbitMQ or Azure Service Bus, records serve well for modeling events.

```csharp
public record UserRegistered(Guid UserId, string Email, DateTime Timestamp);
```

Why it works:

*   Guarantees the message stays unchanged once published.
*   Supports pattern matching on event types.
*   Built-in equality simplifies deduplication or idempotency logic.

🔹 **Discriminated Unions (with inheritance)**
When you need polymorphic behavior, like a set of related event types or command results, records with inheritance and pattern matching are powerful.

```csharp
public abstract record CommandResult;  
public record Success(string Message) : CommandResult;  
public record Failure(string Error) : CommandResult;
```

Why this works:

*   Pattern matching on types is cleaner and type-safe.
*   No need for manual `switch-case` with enums and states.
*   Helps model workflows or state transitions expressively.

# ❌ When You Shouldn’t

While records offer elegance, they’re **not a silver bullet**. Misusing them — especially in areas that rely on identity, state tracking, or controlled mutability — can lead to fragile code, hidden bugs, and performance bottlenecks.

🔻 **EF Core Entities or Tracked Domain Models**
EF Core relies on _reference equality_ to track changes. Records override `Equals` and `GetHashCode`, which confuses EF's tracking mechanism — causing incorrect updates, missed changes, or even silent data corruption.

```csharp
// ❌ Avoid  
public record Product(int Id, string Name);
```

✔️ Use `class` instead, with standard identity handling and navigation properties.

🔻 **Models Requiring Partial or Conditional Mutation**
If you need to mutate individual properties over time (e.g., in multi-step form workflows or stateful business logic), using records can introduce unnecessary friction.

```csharp
// A user draft form that evolves over 4 pages?   
// ⛔ Not a good record candidate.
```

In such cases, go with `class` and encapsulate mutation via methods or validation layers.

🔻 **Complex Serialization Scenarios (e.g., Polymorphic JSON)**
When working with tools like `System.Text.Json` or `Newtonsoft.Json`, record types can require custom converters, especially for:

*   Inheritance
*   Backward compatibility
*   Flattened hierarchies

This adds more effort than it’s worth unless you control both ends of the pipeline.

🔻 **Long-Lived Mutable State (e.g., Blazor, SignalR)**
Records don’t play well in frameworks that rely on state mutation and change detection (e.g., Blazor’s UI rendering or SignalR’s Hub state). Here, reference equality matters, and immutable updates may cause re-renders or state confusion.

🔻 **Entities with Identity and Lifecycle (e.g., Versioning, Soft Deletes)**
If your model includes `Id`, `UpdatedAt`, `IsDeleted`, or similar lifecycle markers, you’re managing identity — not just values. Records break this mental model.

```csharp
// ❌ This record hides identity behind equality logic.  
public record Invoice(int Id, decimal Amount, bool IsDeleted);
```

📌 **Rule of Thumb:**

> If your type represents an identity or changes over time — use a `class`.If your type represents a value with no concept of identity — use a `record`.

# 🧪 Benchmark Snapshot

🚀 _Record vs Class memory usage in collection-heavy workloads_

![](https://miro.medium.com/v2/resize:fit:700/1*R1lTToB17RbH0yM8tkABNQ.png)

*Image: A bar chart titled "Benchmark Graph — Memory Overhead". It compares "Record types" and "Class types" in terms of memory usage (bytes) for 100,000 objects. Record types show significantly higher memory usage (around 900 bytes) compared to Class types (around 700 bytes), indicating a 28% increase in memory for records in a loop with cloning logic.*

_Summary: record types used 28% more memory in a 100,000-object loop with cloning logic._

# 📎 Full Code Reference

👉 [**View the complete example Gist**](https://gist.github.com/emonarafat/52a511ab1284b50f163440524364fffe)

Includes:

*   Correct and incorrect record usage
*   EF Core-safe patterns
*   Value object examples

# ✅ Why This Post Exists

When records landed in C# 9, I started replacing all my DTOs and domain models with them — thrilled by the cleaner syntax. But things started breaking. EF Core tracked nothing. `Equals` calls behaved strangely. Even JSON serializers started throwing.

That pain taught me that **not all elegant syntax leads to maintainable code**.
And I want you to avoid the trap I fell into.

# ✅ Stay Connected. Build Better.

🚀 Cut the noise. Write better systems. Build for scale.
🧠 You’re reading real-world insights from a senior engineer shipping secure, scalable, cloud-native systems since 2009.

📩 Want more?
Subscribe for sharp, actionable takes on modern .NET, microservices, and architecture patterns.

🔗 Let’s connect:
💼 [LinkedIn](https://linkedin.com/in/yaseerarafat) — Tech insights, career reflections, and dev debates
🛠️ [GitHub](https://github.com/emonarafat) — Production-ready patterns & plugin-based architecture tools
🤝 [Upwork](https://www.upwork.com/freelancers/~019243c0d9b337e319?mp_source=share) — Need a ghost architect? Let’s build something real.

# ⚡ Liked This Post? Fuel the Next One.

Let’s skip the applause and go straight to impact.

If this post helped you…

*   🛠️ Fix a frustrating bug
*   🔥 Rethink your architecture
*   💡 Trigger a rare “Aha!” dev moment

Then fuel the work behind it.

## 👉 [Buy Me 3 Coffees](https://coff.ee/yaseer_arafat) ☕☕☕

Because:

*   ☕ 1 coffee = appreciation
*   ☕☕ 2 coffees = respect
*   ☕☕☕ 3 coffees = legacy of better builds

## 💜 Want to Go Deeper?

Support monthly — and power every late-night draft, refactor, and diagram drop.
**No decaf. No fluff. Just devs supporting devs.**

**👉** [**Buy Me a Coffee**](https://coff.ee/yaseer_arafat)

# 🔗 Want More? Explore Related .NET Articles

📚 These posts expand on performance, architecture, and best practices:

*   ⚠️ [**Task.Result Will Wreck Your App — and Your Interview**](https://medium.com/@yaseer.arafat/task-result-will-wreck-your-app-and-your-interview-4076197fcca1)
    _Deadlocks, UI freezes, and async disasters — all from one innocent-looking line._
*   🔄 [**Implementing JSON Converters with System.Text.Json**](https://medium.com/@yaseer.arafat/implementing-json-converters-with-system-text-json-precision-performance-and-maintainability-e1ec66be2dba)
    _Custom serialization, clean converters, and avoiding pitfalls with_ `_System.Text.Json_`_._
*   🧠 [**Async Void is a Trap — and You Might Be Walking Right Into It**](https://medium.com/@yaseer.arafat/️-async-void-is-a-trap-and-you-might-be-walking-right-into-it-3636332fe102)
    _Why_ `_async void_` _breaks your app’s error handling — and what to use instead._
*   🧱 [**From Monolith to Modular: Architecting for Extensibility in .NET 10**](https://medium.com/@yaseer.arafat/from-monolith-to-modular-architecting-for-extensibility-in-net-10-473c2a375970)
    _Build plugin-ready systems with AppHost/AppBuilder in .NET 10._
*   🚀 [**EF Core Tracks Everything — And 5 Other Lies Slowing Down Your App**](https://medium.com/@yaseer.arafat/ef-core-tracks-everything-and-5-other-lies-slowing-down-your-app-db6b85dcf647)
    _Debunking ORM myths that kill your app’s scalability._