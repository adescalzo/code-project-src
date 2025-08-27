```yaml
---
title: Efficient Object Mapping with Mapperly in .NET
source: https://okyrylchuk.dev/blog/efficient-object-mapping-with-mapperly-in-dotnet/
date_published: 2024-12-13T16:54:47.000Z
date_captured: 2025-08-12T11:28:50.405Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET, Mapperly, Riok.Mapperly, NuGet, System.Text.Json, RandomAccess]
programming_languages: [C#]
tags: [object-mapping, source-generators, .net, csharp, performance, compile-time, code-generation, library, developer-tools, d-t-o]
key_concepts: [object mapping, source generators, compile-time code generation, runtime reflection, type safety, deep cloning, DTO]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces Mapperly, a .NET source generator designed to streamline object mapping by generating efficient, type-safe code at compile time. It addresses the performance and potential runtime errors associated with traditional reflection-based mapping libraries. The post demonstrates how to get started with Mapperly, showcases the generated code, and explores advanced features like custom property mapping, deep cloning, and property ignoring. It highlights Mapperly's key advantages, including compile-time code generation, enhanced type safety, and zero runtime overhead, making it a strong alternative for performance-critical .NET applications.
---
```

# Efficient Object Mapping with Mapperly in .NET

# Efficient Object Mapping with Mapperly in .NET

In the world of .NET development, object mapping has long been a necessary but often cumbersome task. Developers traditionally rely on runtime reflection-based libraries, which have performance overhead and potential runtime errors.

I prefer to write object mappers manually. It’s more readable and more efficient. Can we level up the manual mapping and automate it? 

Meet **Mapperly**, a game-changing source generator transforming how we approach object mapping in .NET applications.

## **What is Mapperly?**

[**Mapperly**](https://github.com/riok/mapperly)is a .NET source generator for generating object mappings. It leverages source generation to create efficient, type-safe mapping code at compile time.

A source generator in C# is a powerful compile-time code generation feature introduced in .NET 5 that allows you to generate additional source code during compilation.

## **Getting Started with Mapperly**

To start, you need to install **Riok.Mapperly** NuGet package.

**Package Manager**:

```
Install-Package Riok.Mapperly
```

**CLI:**

```
dotnet add package Riok.Mapperly
```

Let’s create a simple class User and corresponding DTO.

```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

The mapper declaration:

```csharp
[Mapper]
public partial class UserMapper
{
    public partial UserDto ToDto(User car);
}
```

You only need to add the **Mapper** attribute to your mapper, which must be partial with partial methods.

The mapper usage:

```csharp
[Fact]
public void UserMapperTest()
{
    User user = new()
    {
        Id = 1,
        Name = "Oleg",
        Email = "me@okyrylchuk.dev"
    };
    UserMapper userMapper = new();

    UserDto userDto = userMapper.ToDto(user);

    Assert.Equal(user.Id, userDto.Id);
    Assert.Equal(user.Name, userDto.Name);
    Assert.Equal(user.Email, userDto.Email);
}
```

## **Generated Code**

As you can see, creating a mapper with **Mapperly** is super easy.

When you compile the code, **Mapperly** will generate a mapper for you. You can find it in the **Dependencies/Analyzers/Riok.Mapperly/Riok.Mapperly.MapperGenerator** in your project. 

![Screenshot showing the MapperlySample project structure in Visual Studio, with Riok.Mapperly listed under Dependencies/Analyzers/Riok.Mapperly/Riok.Mapperly.MapperGenerator, indicating the location of generated code.](https://okyrylchuk.dev/wp-content/uploads/2024/12/generated.png.webp)

The generated code looks like the following:

```csharp
public partial class UserMapper
{
    [global::System.CodeDom.Compiler.GeneratedCode("Riok.Mapperly", "4.1.1.0")]
    public partial global::UserDto? ToDto(global::User? car)
    {
        if (car == null)
            return default;
        var target = new global::UserDto();
        target.Id = car.Id;
        target.Name = car.Name;
        target.Email = car.Email;
        return target;
    }
}
```

## **Advanced Mapping Scenarios**

**Mapperly** supports various mapping scenarios:

#### Custom Property Mapping

If you have custom properties to map, you can use the **MapProperty** attribute. For example, if we add the **Contact** class with the **Phone** property to **User** class and we want to map it to the flattened **Phone** property in the **UserDto**: 

```csharp
public partial class UserMapper
{
    [MapProperty(nameof(User.Contact.Phone), nameof(UserDto.Phone))]
    public partial UserDto ToDto(User car);
}
```

#### Deep Cloning

By default, **Mapperly** doesn’t do a deep copy for performance reasons. You have to enable it: 

```csharp
[Mapper(UseDeepCloning = true)]
public partial class UserMapper
{
    public partial UserDto ToDto(User car);
}
```

#### Ignore Properties

You can ignore properties with **MapperIgnore** attribute: 

```csharp
public class User
{
    [MapperIgnore]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
```

**Mapperly** is very flexible for advanced scenarios. To learn more about **Mapperly** features, visit the [documentation page](https://mapperly.riok.app/docs/category/usage-and-configuration/). 

## **Mapperly Key Advantages**

1.  **Compile-Time Code Generation** Unlike reflection-based mappers, Mapperly generates mapping code during compilation. This approach eliminates runtime overhead and provides instant performance benefits without additional configuration.
2.  **Type Safety** Mapperly catches mapping errors at compile-time, preventing potential runtime exceptions. During the build process, you get immediate feedback about mismatched or incorrectly mapped properties.
3.  **Zero Runtime Overhead** The generated mapping code is as performant as manually written mapping logic. There’s no runtime penalty for using Mapperly, making it an excellent choice for performance-critical applications.

## **Conclusion**

**Mapperly** is an excellent source generator for creating mappers for your objects.

**Mapperly** represents a significant leap forward in .NET object mapping. It prioritizes compile-time safety and performance to address many longstanding challenges in object transformation.

However, it’s not a silver bullet. **Mapperly** is less advanced in configuration than some reflection-based mappers.

Always consider trade-offs and perform benchmarks for your use cases. In many cases, manual object mapping is enough.