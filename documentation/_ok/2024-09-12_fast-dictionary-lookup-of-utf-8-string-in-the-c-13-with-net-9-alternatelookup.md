```yaml
---
title: "Fast Dictionary Lookup of UTF-8 String in the C# 13 with .NET 9 AlternateLookup | by Yoshifumi Kawai | Medium"
source: https://neuecc.medium.com/fast-dictionary-lookup-of-utf-8-string-in-the-c-13-with-net-9-alternatelookup-43798aef022d
date_published: 2024-09-12T03:14:30.235Z
date_captured: 2025-08-08T13:47:16.029Z
domain: neuecc.medium.com
author: Yoshifumi Kawai
category: ai_ml
technologies: [.NET 9, .NET 8, C# 13, Dictionary, ConcurrentDictionary, HashSet, FrozenDictionary, FrozenSet, MessagePack for C#, System.IO.Hashing, NuGet, System.Text.Json, XxHash3, xxHash, SIMD]
programming_languages: [C#]
tags: [.net, csharp, performance, dictionary, data-structures, memory-management, allocation-free, utf8, span, net9]
key_concepts: [AlternateLookup, ReadOnlySpan, IAlternateEqualityComparer, allows-ref-struct, memory-optimization, hash-functions, SIMD, custom-equality-comparer]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the new `GetAlternateLookup` method in .NET 9, which enables dictionary-like collections to perform lookups using alternate key types, such as `ReadOnlySpan<char>` for `string` keys. This feature significantly reduces memory allocations by eliminating the need for `ToString()` conversions during dictionary operations. The author demonstrates its usage and explains the necessity of implementing the `IAlternateEqualityComparer` interface. A practical example of a custom `Utf8StringEqualityComparer` for `ReadOnlySpan<byte>` keys, utilizing the `XxHash3` hashing algorithm, is provided. The article emphasizes the benefits for performance-critical applications like serializers, allowing for highly optimized, allocation-free key lookups.
---
```

# Fast Dictionary Lookup of UTF-8 String in the C# 13 with .NET 9 AlternateLookup | by Yoshifumi Kawai | Medium

# Fast Dictionary Lookup of UTF-8 String in the C# 13 with .NET 9 AlternateLookup

In .NET 9, a new method `GetAlternateLookup<TKey, TValue, TAlternate>()` has been added to dictionary-like classes: `Dictionary`, `ConcurrentDictionary`, `HashSet`, `FrozenDictionary`, and `FrozenSet`. Until now, Dictionary operations could only be performed via TKey. This was natural, but it became problematic with string keys, as we want to operate with both `string` and `ReadOnlySpan<char>`. Previously, when only `ReadOnlySpan<char>` was available, conversion to string using ToString was mandatory, it allocates new memory even if we just wanted to reference a Dictionary value!

This issue has been resolved with the introduction of `GetAlternateLookup` in .NET 9, which allows dictionaries to have alternate search keys.

```csharp
var dict = new Dictionary<string, int>  
{  
    { "foo", 10 },  
    { "bar", 20 },  
    { "baz", 30 }  
};  
  
var lookup = dict.GetAlternateLookup<ReadOnlySpan<char>>();  
  
var keys = "foo, bar, baz";  
  
// .NET 9 SpanSplitEnumerator  
foreach (Range range in keys.AsSpan().Split(','))  
{  
    ReadOnlySpan<char> key = keys.AsSpan(range).Trim();  
  
    // Get/Add/Remove from string key dictionary using ReadOnlySpan<char>  
    int value = lookup[key];  
    Console.WriteLine(value);  
}
```

By the way, the usual string Split allocates an array and individual split strings. However, in .NET 8, [MemoryExtensions.Split](https://learn.microsoft.com/en-us/dotnet/api/system.memoryextensions.split) was added, allowing a fixed number of splits on `ReadOnlySpan<char>`. In .NET 9, a new Split that returns SpanSplitEnumerator has been added. This allows cutting out `ReadOnlySpan<char>` from the original string without any additional allocations.

To reference keys with the extracted `ReadOnlySpan<char>`, `GetAlternateLookup` becomes necessary.

One use case is serializers, which frequently require key-value lookups. In [MessagePack for C#](https://github.com/MessagePack-CSharp/MessagePack-CSharp) that I’m developing, we adopt multiple strategies for fast, allocation-free deserialization. One is [AutomataDictionary](https://github.com/MessagePack-CSharp/MessagePack-CSharp/blob/bcedbce3fd98cb294210d6b4a22bdc4c75ccd916/src/MessagePack/Internal/AutomataDictionary.cs), which treats UTF8 strings as 8-byte [automata](https://en.wikipedia.org/wiki/Automata_theory). This part is further inlined and embedded in IL Emit and Source Generator to eliminate dictionary lookups. Another is the [AsymmetricKeyHashTable](https://github.com/MessagePack-CSharp/MessagePack-CSharp/blob/5793c81/src/MessagePack/Internal/AsymmetricKeyHashTable.cs) mechanism, which allows searching with two keys representing the same target, internally creating a dictionary searchable by both `byte[]` and `ArraySegment<byte>`.

```csharp
// From MessagePack for C#  
internal interface IAsymmetricEqualityComparer<TKey1, TKey2>  
{  
    int GetHashCode(TKey1 key1);  
    int GetHashCode(TKey2 key2);  
    bool Equals(TKey1 x, TKey1 y);  
    bool Equals(TKey1 x, TKey2 y); // Comparison between TKey1 and TKey2  
}
```

In other words, until now, scenarios requiring dictionaries with alternate search keys necessitated creating custom dictionaries, and for performance, even basic data structures had to be custom-made. However, from .NET 9, this is finally achievable with standard tools.

What’s needed for AlternateLookup is `IAlternateEqualityComparer<in TAlternate, T>`, defined as follows: (The definition is similar to `IAsymmetricEqualityComparer`, so I might have anticipated the future by 10 years)

```csharp
public interface IAlternateEqualityComparer<in TAlternate, T>  
    where TAlternate : allows ref struct  
    where T : allows ref struct  
{  
    bool Equals(TAlternate alternate, T other);  
    int GetHashCode(TAlternate alternate);  
    T Create(TAlternate alternate);  
}
```

The language feature [allows ref struct](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/ref-struct) added in C# 13 allows ref structs, such as `Span<T>`, to be used as generic type arguments.

Basically, this needs to be implemented along with `IEqualityComparer<T>`. In fact, `Dictionary.GetAlternateLookup` throws a runtime exception (not a compile-time check!) if the Dictionary's `IEqualityComparer` doesn't implement `IAlternateEqualityComparer`. Also, it's a bit odd that an EqualityComparer has a `Create` method, but this is necessary for Add operations.

Currently, the standard only provides `IAlternateEqualityComparer` for `string`. The EqualityComparer typically used for strings implements `IAlternateEqualityComparer` and can be operated with `ReadOnlySpan<char>`, but nothing else is provided.

However, what’s realistically needed in modern times is UTF8, `ReadOnlySpan<byte>`. I mentioned using it for serializer lookups, but the input of modern serializers is UTF8. There's no place for `ReadOnlySpan<char>`. So, let's prepare an `IAlternateEqualityComparer` like this!

```csharp
public sealed class Utf8StringEqualityComparer : IEqualityComparer<byte[]>, IAlternateEqualityComparer<ReadOnlySpan<byte>, byte[]>  
{  
    public static IEqualityComparer<byte[]> Default { get; } = new Utf8StringEqualityComparer();  
  
    // IEqualityComparer  
  
    public bool Equals(byte[]? x, byte[]? y)  
    {  
        if (x == null && y == null) return true;  
        if (x == null || y == null) return false;  
  
        return x.AsSpan().SequenceEqual(y);  
    }  
  
    public int GetHashCode([DisallowNull] byte[] obj)  
    {  
        return GetHashCode(obj.AsSpan());  
    }  
  
    // IAlternateEqualityComparer  
  
    public byte[] Create(ReadOnlySpan<byte> alternate)  
    {  
        return alternate.ToArray();  
    }  
  
    public bool Equals(ReadOnlySpan<byte> alternate, byte[] other)  
    {  
        return other.AsSpan().SequenceEqual(alternate);  
    }  
  
    public int GetHashCode(ReadOnlySpan<byte> alternate)  
    {  
        // System.IO.Hashing package, cast to int is safe for hashing  
        return unchecked((int)XxHash3.HashToUInt64(alternate));  
    }  
}
```

By default, `byte[]` is compared by reference, but we want to compare by data match, so we use `ReadOnlySpan<T>.SequenceEqual`. This achieves fast comparison utilizing SIMD, especially when T is one of several primitives. For hash code calculation, it's best to use [XxHash3](https://learn.microsoft.com/en-us/dotnet/api/system.io.hashing.xxhash3), the .NET implementation of XXH3, the latest version of the fast [xxHash](https://github.com/Cyan4973/xxHash) algorithm series. This requires importing `System.IO.Hashing` from NuGet. The return value is ulong as it's calculated in 64 bits, but when a 32-bit value is needed, the xxHash author states that simply dropping bits is fine, so we can just cast to int.

Here’s an example of how to use it:

```csharp
// Create a dictionary with Utf8StringEqualityComparer  
  
var dict = new Dictionary<byte[], bool>(Utf8StringEqualityComparer.Default)  
{  
    { "foo"u8.ToArray(), true },  
    { "bar"u8.ToArray(), false },  
    { "baz"u8.ToArray(), false }  
};  
  
var lookup = dict.GetAlternateLookup<ReadOnlySpan<byte>>();  
  
// Assume we have this input  
  
ReadOnlySpan<byte> json = """      
{  
    "foo": 0,  
    "bar": 0,  
    "baz": 0  
}  
"""u8;  
  
// System.Text.Json  
var reader = new Utf8JsonReader(json);  
  
while (reader.Read())  
{  
    if (reader.TokenType == JsonTokenType.PropertyName)  
    {  
        // Can search with the extracted Key  
        ReadOnlySpan<byte> key = reader.ValueSpan;  
        var flag = lookup[key];  
          
        Console.WriteLine(flag);  
    }  
}
```

One thing to note is that it’s better to avoid creating AlternateKey with `string` and `ReadOnlySpan<byte>`. This would always require encoding, resulting in the worst of both worlds (even if using [Rune](https://learn.microsoft.com/en-us/dotnet/api/system.text.rune) for allocation-less processing, it's no match for `byte[]` keys that can be compared with just binary comparison). If you absolutely need both searches, it's better to prepare two dictionaries.

Anyway, this is a long-awaited feature for me! I’ve created dictionaries in various variations many times, unable to use generics for Span support and having to hard-code them. I’m very excited that it’s now available for general use. While `allows ref struct` has some complexities in generic definitions (maybe automatic assignment would have been fine?), it's an important advancement as a language.

Let's start using .NET 9 and C# 13. It's still in preview, but the official release should be in November.