## Summary
The article introduces `ReadOnlySet<T>`, a new type in .NET 9 Preview 6, designed to provide a truly read-only view of a `HashSet<T>`. It addresses the limitation of `IReadOnlySet<T>`, which can be unsafely cast back to a mutable `HashSet<T>`, allowing unintended modifications. The new type also offers a more efficient alternative to workarounds like `ImmutableHashSet<T>` or `FrozenSet`, which create memory-intensive copies and are truly immutable rather than just read-only facades.

---

```markdown
**Source**: https://steven-giesel.com/blogPost/f368c7d3-488e-4bea-92b4-abf176353fa3?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=programmatically-monitoring-and-reacting-to-logs-in-net-aspire
**Date Captured:** 2025-07-28T19:32:16.633Z
**Domain:** steven-giesel.com
**Author:** Unknown
**Category:** frontend
```

---

# ReadOnlySet<T> in .NET 9

June 26, 2024 | 2 minute read | Tags: [C#](/searchByTag/C%23), [.NET](/searchByTag/.NET), [.NET 9](/searchByTag/.NET%209)

## Table of Contents
1. [IReadOnlySet isn't enough](#1-ireadonlyset-isnt-enough)
2. [Wrong workaround](#2-wrong-workaround)

The next preview (preview 6) will bring a new type `ReadOnlySet<T>`. This is a read-only set that is similar to `ReadOnlyCollection<T>`. Let's see how it works and why it was introduced.

So what does that new type try to solve? Well, there are two issues with the "current" approach, current as in .NET 8.

## 1. IReadOnlySet isn't enough

For sure, we already have the [`IReadOnlySet<T>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.ireadonlyset-1?view=net-8.0) interface. So why isn't that enough? Let's have a look at `List<T>` and `IReadOnlyList<T>`, which suffers somewhat from the same issue.

```csharp
var list = new List<int> { 1, 2, 3 };
IReadOnlyList<int> readOnlyList = list;

// You can easily cast it back to a List<T> and modify it
var list2 = (List<int>)readOnlyList;
list2.Add(4);
```

Of course, consumers of your API shouldn't do that, but you can't prevent it. The same applies to `IReadOnlySet<T>`. So that is why we have an `AsReadOnly` method on `List`:

```csharp
var list = new List<int> { 1, 2, 3 };
var readOnlyList = list.AsReadOnly();

// You can't cast it back to a List<T>
var list2 = (List<int>)readOnlyList; // Exception
```

That is exactly what `ReadOnlySet<T>` will do for `IReadOnlySet<T>`:

```csharp
var set = new HashSet<int> { 1, 2, 3 };
var readonlySet = new ReadOnlySet<int>(set);
```

## 2. Wrong workaround

If users tried to roll out their own way of creating a read-only set, they would have to do something like `ImmutableHashSet<T>` or `FrozenSet`.

```csharp
var set = new HashSet<int> { 1, 2, 3 };
var readOnly = set.ToFrozenSet(); // Or ToImmutableHashSet()
```

While technically this is working, it has two major problems:

1.  Immutable collections like `ImmutableHashSet<T>` or `FrozenSet` need to copy the whole collection into their own memory to guarantee immutability. This can be a waste of memory and CPU cycles.
2.  It's not really readonly. Readonly and immutable are two different concepts. Readonly means you can't modify it, but you can still modify the original collection, which then would be reflected in the "readonly" collection. With immutable collections, this wouldn't be reflected. I even have a whole blog post about it: ["ReadOnlyCollection is not an immutable collection"](https://steven-giesel.com/blogPost/c20eb758-a611-4f98-9ddf-b9e2b83fcac9/readonlycollection-is-not-an-immutable-collection).

And that is why we have `ReadOnlySet<T>` now. If you want to dive deeper, check out the API Proposal: [https://github.com/dotnet/runtime/issues/100113](https://github.com/dotnet/runtime/issues/100113)
```