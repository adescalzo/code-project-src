```yaml
---
title: Intro to Serialization with Source Generation in System.Text.Json
source: https://okyrylchuk.dev/blog/intro-to-serialization-with-source-generation-in-system-text-json/
date_published: 2024-02-23T21:12:45.000Z
date_captured: 2025-08-06T18:28:59.298Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [System.Text.Json, .NET 5, C# 9, BenchmarkDotNet, .NET, Native AOT]
programming_languages: [C#]
tags: [serialization, json, source-generation, performance, dotnet, csharp, benchmarking, reflection, aot, code-generation]
key_concepts: [source-generation, serialization, reflection, performance-optimization, compile-time-code-generation, metadata-generation, application-trimming, native-aot]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article introduces System.Text.Json source generation as a powerful alternative to reflection for serialization and deserialization in .NET, starting from .NET 5 and C# 9. It details how to implement source generation using `JsonSerializerContext` and attributes like `JsonSerializable` and `JsonSourceGenerationOptions`. The author explains two primary generation modes, Metadata and Serialization, and provides benchmark results using BenchmarkDotNet, demonstrating significant performance improvements and reduced memory allocations compared to reflection. The post concludes by highlighting the benefits of source generation for application trimming and enabling Native AOT compilation.]
---
```

# Intro to Serialization with Source Generation in System.Text.Json 

**System.Text.Json** serializer uses reflections for serialization and deserialization. Reflection is slow.

.NET 5, alongside C# 9, introduces source generators. It allows the generation of code during compilation. Source generation is particularly useful for scenarios where code needs to be generated based on metadata, configuration, or other input at compile time.

You can create your source generators. However, Microsoft provides many built-in source generators. One of them can help boost serialization performance.

## Source Generation

It’s easy to start using source generation for serialization with the **System.Text.Json** serializer.

You have to create your source generation context to pass it to the serializer.

```csharp
public record Person(
    string FirstName,
    string LastName,
    int Age);
    
[JsonSerializable(typeof(Person))]
public partial class MySourceGenerationContext: JsonSerializerContext
{ }
```

We created **MySourceGenerationContext** derived from **JsonSerializerContext** for the record **Person**.

The context must be partial because the source generator will augment it with the needed code. It’ll look like this.

```csharp
[JsonSerializable(typeof(Person))]
public partial class MySourceGenerationContext
        : JsonSerializerContext
{
    public static MyJsonContext Default { get; }

    public JsonTypeInfo<Person> Person { get; }

    public MySourceGenerationContext(
        JsonSerializerOptions options) : base(options)
    { }

    public override JsonTypeInfo GetTypeInfo(Type type)
        => ...;
}
```

Why do we need this context?

The serializer needs metadata to access the object’s properties. You can also configure how to serialize the object, for instance, using attributes or serializer options.

The serializer computes metadata at run time using reflection. Once metadata is generated, it is cached for reuse. But this “warm-up” phase takes time and allocation.

The source generator computes metadata at compile time. It can also generate highly optimized serialization logic for some serialization features specified ahead-of-time. By default, the source generator generates type-metadata initialization logic and serialization logic, but we can configure it to generate only one of the outputs.

Now is the time to see how to use the context with a serializer.

```csharp
var person = new Person("John", "Doe", 30);

// Reflection
JsonSerializer.Serialize(person);

// Source generated
JsonSerializer.Serialize(person, MySourceGenerationContext.Default.Person);
```

You need to pass the context as a parameter to the Serialize/Deserialize method.

Another way is to set the context in the **JsonSerializerOptions** with other options.

```csharp
var options = new JsonSerializerOptions
{
    TypeInfoResolver = MySourceGenerationContext.Default,
    WriteIndented = true
};

var json = JsonSerializer.Serialize(person, options);
//{
//  "FirstName": "John",
//  "LastName": "Doe",
//  "Age": 30
//}
```

You can also set serializer options for source generation in your context and provide it with more types.

```csharp
public record Address(
    string Street,
    string City,
    string State,
    string Zip);


[JsonSerializable(typeof(Person))]
[JsonSerializable(typeof(Address))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class MySourceGenerationContext: JsonSerializerContext
{
}
```

## **Metadata Mode**

As mentioned before, the Source Generator, by default, generates type-metadata initialization logic and serialization logic.

We can configure the Source Generator to use only one mode based on the de(serialization) scenarios.

```csharp
[JsonSerializable(typeof(Person))]
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata)]
public partial class MySourceGenerationContext: JsonSerializerContext
{
}
```

In this mode, the source generator collects metadata during compilation and generates source code files as an integral part of the application.

It improves de(serialization) performance and reduces startup overhead.

## **Serialization Mode**

The serializer has many options that customize the output, such as naming policies. The serializer has to obtain metadata about these options at run time.

```csharp
[JsonSerializable(typeof(Person))]
[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization)]
public partial class MySourceGenerationContext: JsonSerializerContext
{
}
```

In the **Serialization** mode, the source generator can optimize the serialization logic for chosen options, which speeds up the serialization.

The limitation is that optimized code doesn’t support all serialization options. If the feature is not supported, it’ll use the default serialization logic. Also, Serialization mode currently is not supported for deserialization.

## **Benchmarks**

I use [BenchmarkDotNet](https://okyrylchuk.dev/blog/boost-your-apps-performance-with-dotnet-benchmarking/) for benchmarks. In the first benchmark, I serialized just one instance of a Person record.

```csharp
[Benchmark]
public void SerializeReflection()
{
    JsonSerializer.Serialize(_jsonWriter, _person);

    _memoryStream.SetLength(0);
    _jsonWriter.Reset();
}

[Benchmark]
public void SerializeGenerated()
{
    JsonSerializer.Serialize(_jsonWriter, _person, MySourceGenerationContext.Default.Person);

    _memoryStream.SetLength(0);
    _jsonWriter.Reset();
}
```

I use Utf8JsonWriter to write JSON into memory to eliminate allocations of creating strings when the Serialize method returns the output.

![Benchmark results showing SerializeGenerated is faster than SerializeReflection for a single object, both with zero memory allocation.](https://ci3.googleusercontent.com/meips/ADKq_Nbj_1iwDmb9kFT1XpaQVt6hrHP8elognm8wzbsgqSNpMtND5lvsc4XgUEtXMq87o0W68fC5pLKVp0nxjW4ikg3qM1OqcPxgimWC9PYhY4R4Ya2H8ezUvXQKa6hbDEfOPKv8Bw0qEuXMk0cEK9T6r3wZjhpNJOQnFrfl3dE-Y1H2AHL9Cjxq6Xfjiw=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1708639970790-benchmarks1.png)

The source generator is faster. Both benchmarks have zero memory allocation.

This is a simple benchmark. Let’s try to serialize many objects.

```csharp
[Params(10, 100, 1000)]
public int Count;

[Benchmark]
public void SerializeReflection()
{
    JsonSerializer.Serialize(_jsonWriter, _persons);

    _memoryStream.SetLength(0);
    _jsonWriter.Reset();
}

[Benchmark]
public void SerializeGenerated()
{
    JsonSerializer.Serialize(_jsonWriter, _persons, MySourceGenerationContext.Default.PersonArray);

    _memoryStream.SetLength(0);
    _jsonWriter.Reset();
}
```

I ran a benchmark for three scenarios, where arrays of Persons have 10, 100, and 1000 items, respectively.

![Benchmark results comparing reflection-based and source-generated serialization for arrays of 10, 100, and 1000 Person objects, showing significant performance gains and reduced memory allocation for source generation.](https://ci3.googleusercontent.com/meips/ADKq_Nau5XBgudhFyh3cYDGCHCNumQ6nvxzWNzwsAgY8R2OXKWa1BmXxb6ZD-lPzBD4lZ5Hku1bsmu2Es2rYYa2Wcmc1zl34OgpFy1X0uUWwcwYNQEpv9wxQTAbRkceW8ep9pi_N5VjJD5IbDicbWpLi5a1GuN3Jjb0Sy9gSDvObC6-QCcURP61tQwIVYA=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1708640403229-benchmarks2.png)

Not bad, huh? The source generator way is much faster. While the serializer based on reflection has memory allocations, the generated serializer still has no memory allocation.

## Summary

The source generator is an excellent feature in .NET. By using it with System.Text.Json, we can significantly improve (de)serialization performance and the warm-up phase at application startup.

It’s great when you use application trimming. The source generator reduces the size of the application.

If you create a native AOT application, the source generator is the only option, as certain reflection APIs cannot be used in those applications.

This post is only an introduction to using source generation in the System.Text.Json serializer.

I constantly post on social media about new features in .NET, including Source Generation in System.Text.Json. The latest is the disabling of [reflection mode](https://www.linkedin.com/posts/okyrylchuk_dotnet-activity-7165406911125786624-qoMn/) by default and the [WitAddedModifier extension method](https://www.linkedin.com/posts/okyrylchuk_a-new-witaddedmodifier-extension-method-of-activity-7154218452163919872-DoSu/).