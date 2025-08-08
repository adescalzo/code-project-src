```yaml
---
title: "Performance Wars — Null Check — C# | by Tiago Martins | Medium"
source: https://medium.com/@martinstm/performance-wars-null-check-c-affdd096813e
date_published: 2021-10-30T15:15:43.643Z
date_captured: 2025-08-08T12:32:41.236Z
domain: medium.com
author: Tiago Martins
category: performance
technologies: [BenchmarkDotNet, .NET 5.0]
programming_languages: [C#]
tags: [performance, null-check, csharp, dotnet, benchmarking, optimization, string, object, micro-optimization]
key_concepts: [null-comparison, performance-benchmarking, string-null-check, object-null-check, micro-optimization, nanosecond-performance, pragmatic-programming]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article conducts a performance comparison of various null checking methods in C# for both string and custom object types. Utilizing BenchmarkDotNet with .NET 5.0, it analyzes the execution time in nanoseconds for different approaches. The results indicate that while `string.IsNullOrEmpty` is slightly slower due to additional empty string checks, most other null check methods for strings and objects show negligible performance differences. The author concludes that for most applications, the choice of null check method has minimal impact on performance, advocating for simplicity and developer preference over micro-optimizations unless nanosecond-level precision is absolutely critical.]
---
```

# Performance Wars — Null Check — C# | by Tiago Martins | Medium

# Performance Wars — Null Check — C#

[

![Tiago Martins](https://miro.medium.com/v2/resize:fill:64:64/1*4mcEu2ft0SfxzXI-GWA56Q.jpeg)

](/@martinstm?source=post_page---byline--affdd096813e---------------------------------------)

[Tiago Martins](/@martinstm?source=post_page---byline--affdd096813e---------------------------------------)

Follow

2 min read

·

Oct 12, 2021

90

2

Listen

Share

More

Press enter or click to view image in full size

![Banner image for the article titled "Performance Wars C# Null Check" with camouflage-patterned text on a black background.](https://miro.medium.com/v2/resize:fit:700/1*NSGHtgci_rGKXPhW_9VdAw.png)

This post is for people who are really obsessed with performance. Validating a null value can be done in different ways. For this battle, I decided to analyze the type `string` and an object created by me.
All the tests were done with the [BenchmarkDotNet](https://www.nuget.org/packages/BenchmarkDotNet/) NuGet package and using the .NET 5.0 version. You can check the [GitHub repository](https://github.com/martinstm/benchmarks).

# Strings

This test is very trivial. We have a `string` variable with null value. Each method checks if its value is null and if so sets the variable.

## Results

![Bar chart showing performance results for different string null check methods. The Y-axis represents 'Time (ns)' and the X-axis shows various methods. Values range from approximately 1.2 ns to 3.0 ns, with some methods being significantly slower.](https://miro.medium.com/v2/resize:fit:504/1*AnPWU90VTJhVMiXEozJ6JQ.png)

As we can see there are 3 ways which are definitively the worst. The other ones are very similar and no one stands out. For me these results were great, I usually use the `==` operator which is in the average looking at these results.
The `IsNullOrEmpty` method is slower but it also checks if the string is empty. It’s interesting this gap between this method and the `==` operator since it has an `Or` condition and the first one is the null check. You can check this [here](https://github.com/microsoft/referencesource/blob/master/mscorlib/system/string.cs#LC793).

# Objects

In this case, I created a `User` class and define a variable with null value. Each benchmark does the same, checks if that variable is null and create a new instance in that case.

## Results

![Bar chart showing performance results for different object null check methods. The Y-axis represents 'Time (ns)' and the X-axis shows various methods. Values range from approximately 6.0 ns to 7.5 ns, indicating less variation between methods compared to string checks.](https://miro.medium.com/v2/resize:fit:508/1*JVESDLtZCf8HVm6QUjz9XA.png)

Once again there are some approaches that have had similar results in terms of time spent. However, all the cases needed basically the same time to process. There isn’t any substantial gap between them. A curious fact is that the check for objects is slower than for strings.

# Conclusion

This battle was curious. We have to keep in mind that these tests were done just regarding the `null` comparison. For example, in the **comparison between two not null string values, the results will be different.
**It was really interesting to see how close were the results for these tested approaches.
My advice, after this, is to keep it simple and use the operator that you prefer. **We have to be pragmatic**, we are talking about nanoseconds but be careful if every nanosecond counts for your service.