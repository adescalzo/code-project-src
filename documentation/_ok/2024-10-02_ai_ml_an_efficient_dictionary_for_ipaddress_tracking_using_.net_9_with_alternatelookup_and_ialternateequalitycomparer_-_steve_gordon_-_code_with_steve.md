```yaml
---
title: An Efficient Dictionary for IPAddress Tracking using .NET 9 with AlternateLookup and IAlternateEqualityComparer - Steve Gordon - Code with Steve
source: https://www.stevejgordon.co.uk/efficient-dictionary-for-ipaddress-tracking-using-net-9-with-alternatelookup-and-ialternateequalitycomparer?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=10-lessons-i-learned-from-using-aspire-in-production&_bhlid=0b9dfdf6658c1f19635c9b0a2c60e26398534aa7
date_published: 2024-10-02T08:58:07.000Z
date_captured: 2025-08-08T15:46:02.183Z
domain: www.stevejgordon.co.uk
author: Steve Gordon
category: ai_ml
technologies: [.NET 9, Dictionary, IPAddress, ReadOnlyMemory, ReadOnlySpan, IAlternateEqualityComparer, HashCode]
programming_languages: [C#]
tags: [.net, .net-9, collections, dictionary, performance, low-allocation, memory-management, csharp, span, equality-comparer]
key_concepts: [AlternateLookup, IAlternateEqualityComparer, ReadOnlySpan, ReadOnlyMemory, stack-allocation, heap-allocation, performance-optimization, custom-equality-comparer]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces and demonstrates the new `AlternateLookup` feature in .NET 9, along with the `IAlternateEqualityComparer` interface, for optimizing dictionary lookups. It showcases how to efficiently track IP address information by using `ReadOnlySpan<byte>` as an alternate lookup key, avoiding unnecessary memory allocations. The post provides a custom `ReadOnlyMemoryComparer` implementation, detailing how to handle `Equals`, `GetHashCode`, and `Create` methods for low-allocation scenarios. The author explains the benefits of using these features for performance and memory efficiency, particularly when dealing with `Span<T>` types.
---
```

# An Efficient Dictionary for IPAddress Tracking using .NET 9 with AlternateLookup and IAlternateEqualityComparer - Steve Gordon - Code with Steve

![A header image for a blog post with a teal to green gradient background. White text reads ".NET" at the top, followed by a horizontal line. Below that, "AN EFFICIENT DICTIONARY FOR IPADDRESS TRACKING USING .NET 9 WITH ALTERNATELOOKUP". Another horizontal line is below this text, and at the very bottom, "www.stevejgordon.co.uk | @stevejgordon". The corners of the image have small white triangular accents.](https://www.stevejgordon.co.uk/wp-content/uploads/2024/10/An-Efficient-Dictionary-for-IPAddress-Tracking-using-.NET-9-with-AlternateLookup-and-IAlternateEqualityComparer-750x410.png)

# An Efficient Dictionary for IPAddress Tracking using .NET 9 with AlternateLookup and IAlternateEqualityComparer

[2nd October 2024](https://www.stevejgordon.co.uk/efficient-dictionary-for-ipaddress-tracking-using-net-9-with-alternatelookup-and-ialternateequalitycomparer) [Steve Gordon](https://www.stevejgordon.co.uk/author/stevejgordon) [.NET 9](https://www.stevejgordon.co.uk/category/net-9)

In this post, I will introduce and demonstrate enhancements to collections in .NET 9 and C# 13 for low-allocation code paths. Specifically, I will demonstrate using a custom `IAlternateEqualityComparer` and the `AlternateLookup` on a `Dictionary` used for efficiently tracking IP address information, keyed on the bytes of IP addresses.

My use case, the basis for the techniques shown here, was tracking the number of requests from non-GitHub IP addresses when handling GitHub webhooks. I wanted to use a dictionary to store a small amount of state, keyed by the IP address. One of my goals was to be as performant and memory-efficient as possible; another was to learn about this new feature of collections.

In this post, I’ll simplify this scenario as much as possible so we can focus on the new C# 13 `AlternateLookup` feature of the Dictionary. I plan to return to this topic with a deeper, more advanced post describing my IP address tracking implementation in greater detail.

Whenever the application handles a request from an IP address which is not a known GitHub IP, we should either add that IP to the dictionary (if it does not exist) or retrieve the state already held in the dictionary for that IP.

We’ll begin by defining a dictionary to hold our data:

```csharp
var dictionary = new Dictionary<ReadOnlyMemory<byte>, string>(ReadOnlyMemoryComparer.Default);
```

In this pared-down example, the value type for this dictionary is a string. It’s a small class in the real scenario, but that detail is unimportant for now. The key is defined as `ReadOnlyMemory<byte>`, which simplifies some of the later code. In practice, this will always be an array of bytes when a new entry is added to the dictionary. A custom comparer has been passed in, and we’ll come to that soon.

When we need to check a remote `IPAddress` for a connection, we’ll first check if we have an existing entry for that IP. During this check, I wanted to avoid any unnecessary allocations. Therefore, ideally, we’ll use a `ReadOnlySpan<byte>` to hold the bytes of the IP we want to look up/add. This allows us to avoid intermediate allocations when preparing that data.

```csharp
var ipAddress = IPAddress.Parse("172.100.50.50");
var size = ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? 4 : 16;
Span<byte> key = stackalloc byte[size];
if (!ipAddress.TryWriteBytes(key, out var bytesWritten))
{
    // Should never happen
}
key = key[..bytesWritten];
```

In the above example, we can stack allocate a small byte buffer to hold the bytes representing the IP address we have received. By defining this local variable as a `Span<byte>` we can use `stackalloc` without the unsafe keyword. Using the `TryWriteBytes` method on the `IPAddress` we can populate that buffer with the bytes of the IP address. While we expect the buffer to be correctly sized, we slice the Span using the range operator to the correct length based on the bytes written into the buffer.

At his point, pre .NET 9, we’d be stuck. We have a `Span<byte>` that we want to look up, but our dictionary uses `ReadOnlyMemory<byte>` for the key. Fortunately, .NET 9 brings a new feature to some of the generic collection types, allowing us to use an alternate type for the key lookup. To use this feature, we’ll first need to define an `IAlternateEqualityComparer<TAlternate,T>` to assist in the comparison.

I’ll provide the complete code for such a comparer and then discuss the details.

```csharp
internal sealed class ReadOnlyMemoryComparer : IEqualityComparer<ReadOnlyMemory<byte>>,
    IAlternateEqualityComparer<ReadOnlySpan<byte>, ReadOnlyMemory<byte>>
{
    public static IEqualityComparer<ReadOnlyMemory<byte>> Default { get; } = new ReadOnlyMemoryComparer();
    public ReadOnlyMemory<byte> Create(ReadOnlySpan<byte> alternate) =>
        alternate.ToArray();
    public bool Equals(ReadOnlySpan<byte> alternate, ReadOnlyMemory<byte> other) =>
        alternate.SequenceEqual(other.Span);
    public bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) =>
        x.Span.SequenceEqual(y.Span);
    public int GetHashCode(ReadOnlySpan<byte> alternate)
    {
        HashCode hc = default;
        hc.AddBytes(alternate);
        return hc.ToHashCode();
    }
    public int GetHashCode(ReadOnlyMemory<byte> obj) => GetHashCode(obj.Span);
}
```

Our class implements both `IEqualityComparer<ReadOnlyMemory<byte>>` and `IAlternateEqualityComparer<ReadOnlySpan<byte>, ReadOnlyMemory<byte>>`. Crucially, we can now pass a Span/ReadOnlySpan as a generic argument when the constraints use the “allows ref struct” keyword combination in the `where` clause.

We must provide two implementations for `Equals` and `GetHashCode`.

`IEqualityComparer.Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)` is pretty easy as we can access the `Span` property on both `ReadOnlyMemory<byte>` instances and then use the optimised `SequenceEqual` method to check if the bytes are the same. `IAlternateEqualityComparer.Equals(ReadOnlySpan<byte> alternate, ReadOnlyMemory<byte> other)` is equally straightforward and pretty self-explanatory.

`IAlternateEqualityComparer.GetHashCode(ReadOnlySpan<byte> alternate)` uses the `HashCode` type, adding the bytes and returning the hashcode. `IEqualityComparer.GetHashCode(ReadOnlyMemory<byte> obj)` calls into this method since we can easily access a Span from the `ReadOnlyMemory<byte>`.

The final method defined on `IAlternateEqualityComparer` that we must implement is the `Create` method. This accepts our alternate type `ReadOnlySpan<byte>` and must return the key type `ReadOnlyMemory<byte>`. This will be used when adding an item to the dictionary to handle the conversion. In the above implementation, we call `ToArray` on the span to create an array. Since this can be implicitly converted to `ReadOnlyMemory<byte>`, this compiles without issue. This is incurring a small heap allocation, but we can’t avoid it as we need an instance to use as the key.

The implementation above also caches an instance of this comparer, which is accessed via the `Default` property, so we only need to allocate one instance. It’s that default instance that we passed into the constructor for the dictionary.

We’re now ready to leverage the new features of .NET 9 to employ our new Span-based comparer.

```csharp
var alternateLookup = dictionary.GetAlternateLookup<ReadOnlySpan<byte>>();
```

We can now call `GetAlternateLookup<TAlternate>` on the dictionary to access methods to operate on the dictionary based on a `ReadOnlySpan<byte>`.

Remember that stack-allocated buffer we filled earlier? We can now use it when adding an entry to the dictionary.

```csharp
alternateLookup[buffer] = "Hello, world!";
```

This will trigger the `IAlternateEqualityComparer.Create` method which converts that `Span<byte>` into an array, making it safe to store.

Of course, we can perform other standard dictionary operations, such as checking whether an IP address is already present.

```csharp
var exists = alternateLookup.ContainsKey(buffer); //true
```

By using `AlternateLookup`, we can now look up items in the dictionary with sliced data from existing memory or stack allocated memory for small items. In this particular example, we could have achieved a similar result by using `IPAddress` directly as the key; however, holding onto those objects which have a slightly larger size was something I wanted to see if I could avoid. I expect the comparisons also to be faster when using the `Span<byte>`; however, reliably benchmarking that proved complicated because of how the `IPAddress` class is implemented internally.

## Conclusion

This has been a basic introduction to the `AlternateLookup` feature added to .NET 9. Keep this in mind if you’re working with collections and need to avoid intermediate allocations for the key lookup value. The canonical example would be for a string-keyed dictionary, where we can now avoid allocating strings, by parsing a `Span<char>` to perform a check for a matching entry. `StringComparer` already implements the new interface, requiring little effort to leverage.

As I’ve shown, crafting a custom compare is not too complicated and works well in scenarios where we can use a `Span<T>` to parse or produce a key value without heap allocating when we want to perform lookups on a collection.

Steve Gordon is a Pluralsight author, 7x Microsoft MVP, and a .NET engineer at [Elastic](https://www.elastic.co) where he maintains the .NET APM agent and related libraries. Steve is passionate about community and all things .NET related, having worked with ASP.NET for over 21 years. Steve enjoys sharing his knowledge through his blog, in videos and by presenting talks at user groups and conferences. Steve is excited to participate in the active .NET community and founded .NET South East, a .NET Meetup group based in Brighton. He enjoys contributing to and maintaining OSS projects. You can find Steve on most social media platforms as [@stevejgordon](https://twitter.com/stevejgordon)

## Post navigation

[Receiving GitHub Webhooks When Using the ASP.NET Core Developer Certificate](https://www.stevejgordon.co.uk/receiving-github-webhooks-when-using-the-aspnetcore-developer-certificate)

[Implementing ASP.NET Core Automatic Span (Activity) Linking for Internal Redirects with Middleware on .NET 9](https://www.stevejgordon.co.uk/implementing-aspnetcore-span-linking-for-redirects-with-middleware)