```yaml
---
title: What’s new in System.Text.Json in .NET 9
source: https://okyrylchuk.dev/blog/whats-new-in-system-text-json-in-dotnet-9/
date_published: 2024-11-01T18:08:49.000Z
date_captured: 2025-08-12T11:27:14.880Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET 9, System.Text.Json, ASP.NET Core, .NET]
programming_languages: [C#, JSON]
tags: [dotnet, json, serialization, deserialization, system-text-json, .net-9, json-schema, json-processing, csharp, web-api]
key_concepts: [JSON serialization, JSON deserialization, nullable reference type support, enum member customization, JSON indentation control, JSON Schema export, streaming JSON documents, JsonObject property manipulation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an overview of the new features introduced in System.Text.Json for .NET 9, enhancing its capabilities for JSON serialization and deserialization. Key updates include the ability to customize enum member names and enforce nullable reference type annotations, improving data integrity. Developers gain more control over JSON output with customizable indentation options and the introduction of a JSON Schema Exporter. Additionally, the article highlights support for streaming multiple JSON documents, validation for required constructor parameters, and new APIs for manipulating JsonObject properties and performing deep equality comparisons on JsonElement.
---
```

# What’s new in System.Text.Json in .NET 9

# What’s new in System.Text.Json in .NET 9

.NET 9 with updates for **System.Text.Json** is coming soon. It’s time to overview highly requested features like nullable reference type support, customizing enum member names, out-of-order metadata deserialization, customizing serialization indentation, and many more.

## Customizing Enum Member Names

A new **JsonStringEnumMemberName** attribute allows customizing enum member names.

```csharp
var json = JsonSerializer.Serialize(MyState.Ready | MyState.InProgress);

Console.WriteLine(json);
// "Ready, In Progress"

[Flags, JsonConverter(typeof(JsonStringEnumConverter))]
enum MyState
{
    Ready = 1,
    [JsonStringEnumMemberName("In Progress")]
    InProgress = 2,
}
```

## Respecting Nullable Notation

A new **RespectNullableAnnotations** option in the JsonSerializerOptions enables respect for non-nullable reference types in serialization and deserialization.

```csharp
JsonSerializerOptions options = new()
{
    RespectNullableAnnotations = true
};

JsonSerializer.Serialize(new Dto(null!), options);
// Throws System.Text.Json.JsonException

JsonSerializer.Deserialize<Dto>("""{ "Value" : null }""", options);
// Throws System.Text.Json.JsonException

record Dto(string Value);
```

However, the feature has some limitations; **System.Text.Json** does not support nullability enforcement on:

*   Any properties, fields, or constructor parameters that are generic.
*   Collection element types when we cannot distinguish between **List<string>** and **List<string?>** types.
*   Top-level types that are passed when making the first **JsonSerializer.(De)**serialize call.

You can enable respecting nullable notation in the project settings.

```xml
<ItemGroup>
	<RuntimeHostConfigurationOption Include="System.Text.Json.JsonSerializerOptions.RespectNullableAnnotations" Value="true" />
</ItemGroup>
```

## Customizing Identation

A new **IndentCharacter** and **IndentSize** options in the JsonSerializerOptions allow the indent character and size to be customizable.

```csharp
var person = new
{
    Name = "Oleg",
    Surname = "Kyrylchuk"
};

var options = new JsonSerializerOptions
{
    WriteIndented = true,
    IndentCharacter = '\t',
    IndentSize = 2,
};

string json = JsonSerializer.Serialize(person, options);

Console.WriteLine(json);
//{
//                "Name": "Oleg",
//                "Surname": "Kyrylchuk"
//}
```

## JSON Schema Exporter

A new **JsonSchemaExporter** type allows exporting JSON schema that represents a .NET type.

```csharp
JsonNode schema = JsonSchemaExporter.GetJsonSchemaAsNode(
        JsonSerializerOptions.Default,
        typeof(Person));

Console.WriteLine(schema);

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Title { get; set; }
}
```

JSON schema:

```json
{
    "type": [
        "object",
        "null"
    ],
    "properties": {
        "Name": {
            "type": [
                "string",
                "null"
            ]
        },
        "Age": {
            "type": "integer"
        },
        "Title": {
            "type": [
                "string",
                "null"
            ]
        }
    }
}
```

## Streaming Multiple JSON Documents

A new **AllowMultipleValues** option in the JsonSerializerOptions allows reading multiple, whitespace-separated JSON documents from a single buffer or stream.

The option also allows reading JSON documents with trailing data that is invalid JSON.

```csharp
JsonReaderOptions options = new()
{
    AllowMultipleValues = true
};

Utf8JsonReader reader = new(
    """
            {"name": "oleg"} [1,2] {}
        """u8,
    options);

while (reader.Read())
{
    Console.WriteLine(reader.TokenType);
}

// Output:
// StartObject
// PropertyName
// String
// EndObject
// StartArray
// Number
// Number
// EndArray
// StartObject
// EndObject
```

## Respecting Required Constructor Parameters

A new **RespectRequiredConstructorParameters** option in the JsonSerializerOptions enables validation if required constructor parameters are missing.

```csharp
JsonSerializerOptions options = new()
{
    RespectRequiredConstructorParameters = true
};

// Throws exception JsonException
JsonSerializer.Deserialize<RecordDto>("{}", options);

// Throws exception JsonException
JsonSerializer.Deserialize<ClassDto>("{}", options);

record RecordDto(string Value);

class ClassDto(string value)
{
    public string Value => value;
}
```

## JsonSerializerOptions.Web

A **JsonSerializerOptions** has a new singleton instance of JsonSerializerOptions – **Web.**

It has the serializer options that ASP.NET Core uses for web applications.

```csharp
var me = new Person("Oleg", "Kyrylchuk");

string webJson = JsonSerializer.Serialize(
    me,
    JsonSerializerOptions.Web // Defaults to camelCase naming policy.
    );

Console.WriteLine(webJson);
// {"firstName":"Oleg","lastName":"Kyrylchuk"}

record Person(string FirstName, string LastName);
```

## Ordering JsonObject Properties

**JsonObject** implements **IList<KeyValuePair<string, JsonNode?>>**.

It means new ordered-dictionary-like APIs that enable explicit property order manipulation. Among the new methods are:

*   IndexOf
*   Insert
*   SetAt
*   RemoveAt
*   GetAt.

```csharp
JsonObject jObj = new()
{
    ["Name"] = "John",
    ["Age"] = 30
};

Console.WriteLine(jObj is IList<KeyValuePair<string, JsonNode?>>); // True

int agePosition = jObj.IndexOf("Age");
jObj.Insert(agePosition, "TestProperty", "Foo");
// {"Name":"John","TestProperty":"Foo","Age":30}

jObj.SetAt(2, "Surname", "Doe");
// {"Name":"John","TestProperty":"Foo","Surname":"Doe"}

jObj.RemoveAt(1);
// {"Name":"John","Surname":"Doe"}

KeyValuePair<string, JsonNode?> firstElement = jObj.GetAt(0);
// {"Name":"John"}
```

## JsonElement DeepEquals

In .NET 8, the **JsonNode** type got the **DeepEquals** method. In .NET 9, a similar method gets the **JsonElement** type.

```csharp
var json = "{\"name\":\"oleg\"}";

JsonElement left = JsonDocument.Parse(json).RootElement;
JsonElement right = JsonDocument.Parse(json).RootElement;

JsonElement.DeepEquals(left, right); // True
```

## Out-of-Order Metadata Reads

Certain features of **System.Text.Json** are polymorphism or **ReferenceHandler.Preserve** requires emitting metadata properties.

However, metadata properties must be defined at the start of the JSON object while deserializing. If they are not, the exception is thrown.

That was the issue with deserializing JSON payloads that do not originate from **System.Text.Json**.

```csharp
JsonSerializerOptions options = new()
{
    ReferenceHandler = ReferenceHandler.Preserve
};
Base value = new Derived("Name");
JsonSerializer.Serialize(value, options);
// {"$id":"1","$type":"derived","Name":"Name"}

var metadataAtTheEndJson = """{"Name":"Name","$type":"derived"}""";
JsonSerializer.Deserialize<Base>(invalidJson, options);
// System.Text.Json.JsonException: The metadata property is
// either not supported by the type or is not the first property
// in the deserialized JSON object

[JsonDerivedType(typeof(Derived), "derived")]
    record Base;
    record Derived(string Name) : Base;
```

A new **AllowOutOfOrderMetadataProperties** fixes the issue and disables this restriction.

```csharp
JsonSerializerOptions options = new()
{
    AllowOutOfOrderMetadataProperties = true
};

var metadataAtTheEndJson = """{"Name":"Name","$type":"derived"}""";
JsonSerializer.Deserialize<Base>(metadataAtTheEndJson, options); // Success
```