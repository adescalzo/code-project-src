```yaml
---
title: Parameterized Tests with xUnit in .NET
source: https://www.nikolatech.net/blogs/parameterized-tests-with-xunit-dotnet
date_published: 2025-04-16T17:57:58.950Z
date_captured: 2025-08-08T14:15:58.450Z
domain: www.nikolatech.net
author: Unknown
category: testing
technologies: [xUnit, .NET, NUnit, MSTest]
programming_languages: [C#]
tags: [unit-testing, parameterized-tests, xunit, dotnet, test-automation, software-testing]
key_concepts: [parameterized-tests, inline-data, member-data, class-data, theory-tests, unit-testing]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces parameterized tests as a method to reduce repetition and improve clarity in unit tests, particularly within the .NET ecosystem. It explains how parameterized tests allow the same test logic to run with multiple input data sets, making test code cleaner and easier to maintain. The post focuses on xUnit, demonstrating three primary attributes for supplying test data: InlineData for simple, direct inputs; MemberData for data from properties, fields, or methods; and ClassData for external, structured, or dynamically generated data, often combined with TheoryData for strong typing. By leveraging these xUnit features, developers can create more scalable and maintainable test suites.
---
```

# Parameterized Tests with xUnit in .NET

# Parameterized Tests with xUnit in .NET

![Banner: A blue and white graphic with the title "Parameterized Tests with xUnit in .NET". The xUnit logo is in the bottom left corner, and the NikolaTech logo is in the top left.](https://coekcx.github.io/BlogImages/banners/parameterized-tests-with-xunit-dotnet-banner.png)

#### Parameterized Tests with xUnit in .NET
###### 16 Apr 2025
###### 5 min

**Unit tests** are great; they help us catch bugs early and make refactoring much safer. A well-tested project is not only more reliable but also easier to maintain.

One common challenge with unit tests is **redundant test data**. Repeating similar test cases with slight variations can quickly become tedious, especially when that data needs to change.

That’s where parameterized tests come in.

Instead of duplicating test logic for every data variation, parameterized tests allow us to supply multiple inputs from external sources.

## Parameterized Tests

**Parameterized tests** allow you to run the same test logic multiple times with different sets of input data.

They help keep your test code clean, expressive, and easy to maintain. It's supported by every relevant testing framework:

*   **xUnit**
*   **NUnit**
*   **MSTest**

In this post, I’ll focus on **xUnit** to demonstrate how parameterized tests can simplify repetitive test cases.

If you're new to unit testing, be sure to check out this guide: [Unit Testing with xUnit](https://www.nikolatech.net/blogs/unit-testing-using-xunit-dotnet)

Here’s a simple validator class that checks whether a product name is valid:

```csharp
public static class ProductValidator
{
    public static bool IsValid(string productName)
    {
        return !string.IsNullOrWhiteSpace(productName);
    }
}
```

The method returns false if the name is null, an empty string, or whitespace. Here’s how you could write tests for each invalid case using **`[Fact]`**:

```csharp
public class ProductValidatorTests
{
    [Fact]
    public void IsValid_ShouldReturnFalse_WhenNameIsNull()
    {
        var result = ProductValidator.IsValid(null);
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenNameIsEmpty()
    {
        var result = ProductValidator.IsValid(string.Empty);
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenNameIsWhitespace()
    {
        var result = ProductValidator.IsValid("   ");
        result.ShouldBeFalse();
    }
}
```

This works, but it’s repetitive and clutters your tests. With parameterized tests, the goal is to replace this block of code with something like this:

```csharp
public class ProductValidatorTests
{
    [Theory] 
    public void IsValid_ShouldReturnFalse_WhenNameIsInvalid(string productName)
    {
        var result = ProductValidator.IsValid(productName);
        result.ShouldBeFalse();
    }
}
```

**`[Theory]`** is an attribute that allows you to write parameterized tests.

To make this work, the last piece is supplying input values, and xUnit offers you multiple ways to do that:

*   **InlineData Attribute**
*   **MemberData Attribute**
*   **ClassData Attribute**

## InlineData Attribute

**`[InlineData]`** is the simplest way to supply input values directly to a **`[Theory]`** test in xUnit.

It allows you to define one or more sets of parameters, right above your test method, and xUnit will run the test once for each set.

```csharp
public class ProductValidatorTests
{   
    [Theory]
    [InlineData(null)]
    [InlineData("")]       
    [InlineData("   ")]  
    public void IsValid_ShouldReturnFalse_WhenNameIsInvalid(string input)
    {
        var result = ProductValidator.IsValid(input);
        result.ShouldBeFalse();
    }
}
```

InlineData works best when you have just a few test cases and the inputs are simple, like in our example.

## MemberData Attribute

**`[MemberData]`** is an attribute that allows you to pass test data to a **`[Theory]`** test using a property, field, or method with type of **`IEnumerable<object[]>`**.

Here's an example of a test using the MemberData attribute:

```csharp
public class ProductValidatorTests
{
    public static IEnumerable<object[]> InvalidProductNames =>
        new List<object[]>
        {
            new object[] { null },
            new object[] { "" },
            new object[] { "   " }
        };

    [Theory]
    [MemberData(nameof(InvalidProductNames))]
    public void IsValid_ShouldReturnFalse_ForInvalidProductNames(string productName)
    {
        var result = ProductValidator.IsValid(productName);
        result.ShouldBeFalse();
    }
}
```

This is an interesting alternative when you want to reuse the data inside the same class or if the data is generated programmatically.

## ClassData Attribute

Saving the best for last, the **`[ClassData]`** attribute is your best bet when you want full control over your test data.

It allows you to move your test data into a separate class, where you can structure it clearly and generate it dynamically. Here's an example of a test using ClassData:

```csharp
public class InvalidProductNamesData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        yield return new object[] { null };
        yield return new object[] { "" };
        yield return new object[] { "   " };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

```csharp
public class ProductValidatorTests
{
    [Theory]
    [ClassData(typeof(InvalidProductNamesData))]
    public void IsValid_ShouldReturnFalse_WhenNameIsInvalid(string input)
    {
        var result = ProductValidator.IsValid(input);
        result.ShouldBeFalse();
    }
}
```

To get the most out of `[ClassData]`, I personally love combining it with **TheoryData** to take advantage of strongly typed test data:

```csharp
public class InvalidProductNamesTheoryData : TheoryData<string>
{
    public InvalidProductNamesTheoryData()
    {
        Add(null);
        Add("");
        Add("   ");
    }
}
```

```csharp
public class ProductValidatorTests
{
    [Theory]
    [ClassData(typeof(InvalidProductNamesTheoryData))]
    public void IsValid_ShouldReturnFalse_WhenNameIsInvalid(string input)
    {
        var result = ProductValidator.IsValid(input);
        result.ShouldBeFalse();
    }
}
```

TheoryData is a helper class provided by xUnit that makes it easier to provide strongly typed test data for `[Theory]` tests.

## Conclusion

Parameterized tests are a powerful way to reduce repetition and improve clarity in your unit tests.

**xUnit** provides several ways to feed data into your **`[Theory]`** tests, each suited for different use cases:

Use **InlineData** for quick, simple inputs.

Use **MemberData** when your data comes from a static method or property.

Use **ClassData**, optionally combined with **TheoryData**, when you need fully structured or dynamically generated test data.

By leveraging these features, you can keep your test suite clean, scalable, and easy to maintain.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/unit-test-examples)

I hope you enjoyed it, subscribe and get a notification when a new blog is up!

# Subscribe

###### Stay tuned for valuable insights every Thursday morning.

##### Share This Article: