# C# 13 Params Collections - Reflection IT

```markdown
**Source:** https://www.reflectionit.nl/blog/2024/csharp-13-params-collections?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=adventures-serializing-absolutely-everything-in-c
**Date Captured:** 2025-07-28T16:27:07.764Z
**Domain:** www.reflectionit.nl
**Author:** Unknown
**Category:** ai_ml
```

---

# C# 13 Params Collections

By [Fons Sonnemans](/blog/authors/fons-sonnemans), posted on 16-Apr-2024 , Modified on 17-Apr-2024  
11809 Views

With the version 17.10.0 Preview 3.0 of [Visual Studio Preview](https://visualstudio.microsoft.com/vs/preview/) you can test some new C# 13 features. In this blog I will explain the **params Collection** feature as documented in this [proposal](https://github.com/dotnet/csharplang/blob/main/proposals/params-collections.md). To use this feature you have to set the **LangVersion** in your csproj file to **preview**.

[?](#)

1

2

3

4

5

6

7

8

9

`<``Project` `Sdk``=``"Microsoft.NET.Sdk"``>`

    `<``PropertyGroup``>`

        `<``OutputType``>Exe</``OutputType``>`

        `<``TargetFramework``>net8.0</``TargetFramework``>`

        `<``ImplicitUsings``>enable</``ImplicitUsings``>`

        `<``Nullable``>enable</``Nullable``>`

        `<``LangVersion``>preview</``LangVersion``>`

    `</``PropertyGroup``>`

`</``Project``>`

In C#, the \`params\` keyword allows a method to accept a variable number of arguments, providing flexibility in how many parameters you pass without needing to define multiple method overloads. This can make your code more concise and easier to maintain. For instance, it's particularly useful when the exact number of inputs is not known in advance or can vary. Moreover, it simplifies the calling code, as you can pass an array or a comma-separated list of arguments that the method will interpret as an array.

From C# 1.0 till 12.0 params parameter must be an array type. However, it might be beneficial for a developer to be able to have the same convenience when calling APIs that take other collection types. For example, an ImmutableArray<T>, ReadOnlySpan<T>, or plain IEnumerable. Especially in cases where compiler is able to avoid an implicit array allocation for the purpose of creating the collection (ImmutableArray<T>, ReadOnlySpan<T>, etc). This saves Heap memory allocation which improves the performance. The Garbage collector doesn't have to free this memory.

### C# 1.0 params array

Lets start with an example of an old-school params array parameter in a Sum method. 

[?](#)

1

2

3

4

5

6

7

8

9

10

11

12

13

14

`internal` `class` `Program {`

    `static` `void` `Main(``string``[] args) {`

        `Console.WriteLine(Sum(1, 2, 3, args.Length));`

    `}`

    `private` `static` `Sum(``params` `int``[] values) {`

        `int` `sum = 0;`

        `foreach` `(var item` `in` `values) {`

            `sum += item;`

        `}`

        `return` `sum;`

    `}`

`}`

When you decompile this code using a tool like [ILSpy](https://github.com/icsharpcode/ILSpy) or the [SharpLab.io](https://sharplab.io/#v2:EYLgZgpghgLgrgJwgZwLQAUoKgW2QYQHsAbYiAYxgEtCA7ZAGgBMQBqAHwAEAmARgFgAUEKq0YEBLSjEABDzm8A7DIDeQoTM0KAbHIAsMgLJRRACk68ADAG0AujKwBzZAEpVGrZ4sBOUwGU4HFNeBhluUIBmUKdkADoAGQhaRxgACxcXAG4PTQBfdUFPAAcEKgA3WAgdGVEYGQCgoqxcZBqxOxkK4jgUNzVCzy1amWRAmQBeGUtsgcGZMEIkKHJUmVMKhBrxHDbO6R7Xd1m5rVGd1kmqbZmTmXzjr2Uzm617+6A=) website you see that an array of int\[4\] is created in the Main() method (line 5). This is heap allocation which is something you can/should avoid.

[?](#)

1

2

3

4

5

6

7

8

9

10

11

12

13

14

15

16

17

18

19

20

21

22

23

`internal` `class` `Program`

`{`

    `private` `static` `void` `Main(``string``[] args)`

    `{`

        `int``[] array =` `new` `int``[4];`

        `RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)``/*OpCode not supported: LdMemberToken*/``);`

        `array[3] = args.Length;`

        `Console.WriteLine(Sum(array));`

    `}`

    `private` `static` `int` `Sum(``params` `int``[] values)`

    `{`

        `int` `num = 0;`

        `int` `num2 = 0;`

        `while` `(num2 < values.Length)`

        `{`

            `int` `num3 = values[num2];`

            `num += num3;`

            `num2++;`

        `}`

        `return` `num;`

    `}`

`}`

### C# 13 params Collections

In the next example the Sum() method is using a **params ReadOnlySpan<int>** parameter in line 7. Nothing else is changed. 

[?](#)

1

2

3

4

5

6

7

8

9

10

11

12

13

14

`internal` `class` `Program {`

    `static` `void` `Main(``string``[] args) {`

        `Console.WriteLine(Sum(1, 2, 3, args.Length));`

    `}`

    `private` `static` `int` `Sum(``params` `ReadOnlySpan<``int``> values) {`

        `int` `sum = 0;`

        `foreach` `(var item` `in` `values) {`

            `sum += item;`

        `}`

        `return` `sum;`

    `}`

`}`

When you [decompile](https://sharplab.io/#v2:EYLgZgpghgLgrgJwgZwLQAUoKgW2QYQHsAbYiAYxgEtCA7ZAGgBMQBqAHwAEAmARgFgAUFVowICWlGIACHrN4B2aQG8hQ6RvkA2WQBZpAWSgiAFJ14AGANoBdaVgDmyAJQr1mj+YCcJgMpwcE14GaW4QgGYQx2QAOgAZCFoHGAALZ2cAbncNAF81QQ8ABwQqADdYCG1pERhpf0DCrFxkaQAlaCYAeVpiAE9fRtoAHhqAPmly4jgUV1UCj00a6WQA6QBeaQss+YXpMEIkKHIU6RNyhGqxHGraCalplzcd3c0V69YNqivtl+k8588SjeP00/xyQA==) this code you see a  <>y\_\_InlineArray4<int> value is used in the Main() method (line 6). This is a struct which is created by the compiler. It uses the [Inline Arrays](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/inline-arrays) feature of C# 12. Structs are allocated on the Stack so this code doesn't allocate any heap memory.

[?](#)

1

2

3

4

5

6

7

8

9

10

11

12

13

14

15

16

17

18

19

20

21

22

23

24

25

26

27

28

29

30

31

32

33

34

35

36

37

`internal` `class` `Program`

`{`

    `[NullableContext(1)]`

    `private` `static` `void` `Main(``string``[] args)`

    `{`

        `<>y__InlineArray4<``int``> buffer =` `default``(<>y__InlineArray4<``int``>);`

        `<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray4<``int``>,` `int``>(``ref` `buffer, 0) = 1;`

        `<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray4<``int``>,` `int``>(``ref` `buffer, 1) = 2;`

        `<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray4<``int``>,` `int``>(``ref` `buffer, 2) = 3;`

        `<PrivateImplementationDetails>.InlineArrayElementRef<<>y__InlineArray4<``int``>,` `int``>(``ref` `buffer, 3) = args.Length;`

        `Console.WriteLine(Sum(<PrivateImplementationDetails>.InlineArrayAsReadOnlySpan<<>y__InlineArray4<``int``>,` `int``>(``ref` `buffer, 4)));`

    `}`

    `private` `static` `int` `Sum([ParamCollection] ReadOnlySpan<``int``> values)`

    `{`

        `int` `num = 0;`

        `ReadOnlySpan<``int``> readOnlySpan = values;`

        `int` `num2 = 0;`

        `while` `(num2 < readOnlySpan.Length)`

        `{`

            `int` `num3 = readOnlySpan[num2];`

            `num += num3;`

            `num2++;`

        `}`

        `return` `num;`

    `}`

`}`

`}`

`[StructLayout(LayoutKind.Auto)]`

`[InlineArray(4)]`

`internal` `struct` `<>y__InlineArray4<T>`

`{`

    `[CompilerGenerated]`

    `private` `T _element0;`

`}`

### Benchmark

To compare the performance between the two I have created this Benchmark using [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet). It compares the Sum of 5 decimals using old-school **params arrays** and **params ReadOnlyCollection<decimal>**.

[?](#)

1

2

3

4

5

6

7

8

9

10

11

12

13

14

15

16

17

18

19

20

21

22

23

24

25

26

27

28

29

30

31

32

33

34

`using` `BenchmarkDotNet.Attributes;`

`using` `BenchmarkDotNet.Running;`

`BenchmarkRunner.Run<BM>();`

`[MemoryDiagnoser(``false``)]`

`[HideColumns(``"RatioSD"``,` `"Alloc Ratio"``)]`

`//[ShortRunJob]`

`public` `class` `BM {`

    `private` `decimal` `_value = 500m;`

    `[Benchmark]`

    `public` `decimal` `CallSumArray() => SumArray(1m, 100m, 200m, 300m, 400m, _value);`

    `[Benchmark(Baseline =` `true``)]`

    `public` `decimal` `CallSumSpan() => SumSpan(1m, 100m, 200m, 300m, 400m, _value);`

    `private` `static` `decimal` `SumArray(``params` `decimal``[] values) {`

        `decimal` `sum = 0;`

        `foreach` `(var item` `in` `values) {`

            `sum += item;`

        `}`

        `return` `sum;`

    `}`

    `private` `static` `decimal` `SumSpan(``params` `ReadOnlySpan<``decimal``> values) {`

        `decimal` `sum = 0;`

        `foreach` `(var item` `in` `values) {`

            `sum += item;`

        `}`

        `return` `sum;`

    `}` 

`}`

The CallSumSpan method is 28% faster and doesn't allocate any heap memory. The CallSumArray method allocated 120 bytes.

![Benchmark summary](https://www.reflectionit.nl/Images/Blog/CSharp/Params/ParamsBM.png)

### Don't want to wait for C# 13

If you don't want to wait for C# 13 you can already use a solution with simular results in C# 12. You can use [Collection Expression](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/collection-expressions) with a normal ReadOnlySpan<T> parameter. A collection expression contains a sequence of elements between **\[** and **\]** brackets, see line 4. 

[?](#)

1

2

3

4

5

6

7

8

9

10

11

12

13

14

`internal` `class` `Program {`

    `static` `void` `Main(``string``[] args) {`

        `Console.WriteLine(Sum([1, 2, 3, args.Length]));`

    `}`

    `private` `static` `int` `Sum(ReadOnlySpan<``int``> values) {`

        `int` `sum = 0;`

        `foreach` `(var item` `in` `values) {`

            `sum += item;`

        `}`

        `return` `sum;`

    `}`

`}`

When you [decompile](https://sharplab.io/#v2:EYLgZgpghgLgrgJwgZwLQAUoKgW2QYQHsAbYiAYxgEtCA7ZAGgBMQBqAHwAEAmARgFgAUEKq0YEBLSjEABDzm8A7DIDeQoTM0KAbHIAsMgLJRRACk68ADAG0AujKwBzZAEpVGrZ4sBOUwGU4HFNrXgYZbjCAZjCnZAA6ABkIWkcYAAtbFxcAbg9NAF91QU8ABwQqADdYCB0ZURgZAKCAJWgmAHlaYgBPPxKoWgAeeoA+GSriOBQ3NWLPLXqZZECZAF4ZS1y5+ZkwQiQocjSZUyqEOvEcOtpx6SnXd22drWWr1nWqS63nmUKnr2Ur2+Wj+fyAA===) this code you see the same code you saw when you used the **params ReadOnlyCollection<decimal>**.

### Closure

In this blog post I showed you the new 'params Collection' feature in C# 13, available in Visual Studio Preview 17.10.0 Preview 3.0. It explains how to enable the feature by setting 'LangVersion' to preview in the project file and delves into the benefits of using 'params' with collection types other than arrays, like 'ReadOnlySpan<T>'. This enhancement aims to improve performance by reducing heap memory allocations, thus easing the workload on the garbage collector. The post includes an example of the traditional 'params array' in a \`Sum\` method to illustrate the concept.

Hopefully Microsoft will add in .NET 9 (and later) more overloads with 'params ReadOnlySpan<T>' to the methods which are using 'params arrays'. For example the [String.Split](https://learn.microsoft.com/en-us/dotnet/api/system.string.split?view=net-8.0#system-string-split\(system-char\(\)\)) method.

### Tags

[CSharp](/blog/tags/csharp)

All postings/content on this blog are provided "AS IS" with no warranties, and confer no rights. All entries in this blog are my opinion and don't necessarily reflect the opinion of my employer or sponsors. The content on this site is licensed under a Creative Commons Attribution By license.

## About **Fons Sonnemans**

![Photo Fons Sonnemans](https://www.reflectionit.nl//images/upload/fons.jpg)

Algemeen directeur, trainer, speaker, coach, developer

[](https://twitter.com/#!/fonssonnemans)[](https://nl.linkedin.com/in/fonssonnemans)[](mailto:fons.sonnemans@reflectionit.nl)[](https://github.com/sonnemaf)

[![Microsoft MVP](/img/MVP_Logo.png "Microsoft MVP")](https://mvp.microsoft.com/en-us/PublicProfile/5000175?fullName=Fons%20Sonnemans)

## **Trainingen** overzicht

[Visual C#](/training/csharp)

*   [Upgrade to .NET 9 and C# 13](/training/csharp/upgrade-to-latest-dotnet-and-csharp)
*   [OOP with C#](/training/csharp/oop-with-csharp)
*   [Fast Track Advanced C#](/training/csharp/fast-track-advanced-csharp)
*   [Advanced C#](/training/csharp/advanced-csharp)
*   [Design Patterns in C#](/training/csharp/design-patterns-csharp)
*   [High Performance C# & .NET](/training/csharp/performance-csharp)
*   [Avoiding C# Traps and Pitfalls](/training/csharp/avoiding-csharp-traps-and-pitfalls)
*   [C# PubQuiz](/training/csharp/csharp-pubquiz)
*   [Async Programming in C# 5.0](/training/csharp/async-csharp)
*   [C# 13 and .NET 9](/training/csharp/csharp-13-and-dotnet9)
*   [C# 12 and .NET 8](/training/csharp/csharp-12-and-dotnet8)
*   [C# 10.0 and C# 11.0](/training/csharp/csharp-10-11)
*   [C# 9.0 and C# 10.0](/training/csharp/csharp-9-10)
*   [C# 8.0 and C# 9.0](/training/csharp/csharp-8-9)
*   [C# 7](/training/csharp/csharp7)
*   [Unit Testing in C#](/training/csharp/unittesting-csharp)
*   [.NET and C# Linux apps](/training/csharp/linux-dotnet-csharp)
*   [C# 3.0 and LINQ](/training/csharp/csharp-3-upgrade-and-linq)
*   [Voorbeeld maatwerktraining](/training/csharp/modern-csharp-features)

[Xaml](/training/xaml)

[Data](/training/data)

[ASP.NET & Blazor](/training/asp-net)

[Visual Studio](/training/visual-studio)

[Other](/training/other)

## Popular Tags

[CSharp (96)](/blog/tags/csharp)

[XAML (86)](/blog/tags/xaml)

[Visual Studio (52)](/blog/tags/visual-studio)

[Apps (50)](/blog/tags/apps)

[Blend (44)](/blog/tags/blend)

[Framework (31)](/blog/tags/framework)

[UWP (31)](/blog/tags/uwp)

[Windows 8 (25)](/blog/tags/windows-8)

[Windows 10 (25)](/blog/tags/windows-10)

[Silverlight (23)](/blog/tags/silverlight)

[Windows Phone (23)](/blog/tags/windows-phone)

[Speaker (18)](/blog/tags/speaker)

[Web (16)](/blog/tags/web)

[ASP.NET (16)](/blog/tags/asp-net)

[Behaviors (11)](/blog/tags/behaviors)

## Rss

[Subscribe](/blog/rss)

## Leave a **comment**

 

Name 

Comment

## Blog **comments**

0 responses