```markdown

# Fixing C# type pattern-matching | MALTSEV.SPACE

**Source:** https://maltsev.space/blog/007-fixing-csharp-type-pattern-matching?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2096
**Date Captured:** 2025-07-28T15:59:15.880Z
**Domain:** maltsev.space
**Author:** by Aleksey Maltsev
**Category:** ai_ml
```

---

![Fixing C# type pattern-matching hero image placeholder](data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACAAAAAXCAYAAABqBU3hAAAMEElEQVR4AQCBAH7/ABAAAP8PAAD/DQAA/wkAAP8FAAD/AQAA/wAAAP8AAAD/AAAA/wAAAP8AAAT/AAEJ/wAGD/8ACxX/ABAb/wAUIP8AGCT/Ahom/wMaJf8DFyL/ARMc/wAMFP8ABQr/AAAB/wAAAP8AAAD/AAAA/wAAAP8AAAD/AQAA/wUAAP8HAAD/AIEAfv8AEgAA/xAAAP8OAAD/CwAA/wcAAP8DAAD/AAAA/wAAAP8AAAD/AAAD/wABCP8ABQ3/AAkT/wAOGf8AFB//ABgl/wMcKf8GHir/Bx4q/wYbJv8EFyD/ABAY/wAIDv8AAQX/AAAA/wAAAP8AAAD/AAAA/wAAAP8EAAD/BwAA/woAAP8AgQB+/wAUAwD/EwIA/xECAP8NAQD/CgAA/wYAAP8CAAD/AAAB/wABBP8ABAn/AAcO/wALE/8AEBr/ABUg/wIaJ/8GHyz/CSMw/wwlMv8NJTH/DCIu/wodJ/8GFx//Ag8V/wAHDP8AAQT/AAAA/wAAAP8AAAD/AwAA/wgAAP8MAAD/DgIA/wCBAH7/ABcHAP8VBwD/EwYA/xAFAP8NBQH/CQQC/wUFBP8CBQf/AAcL/wAKEP8ADRX/ABIb/wEXIv8FHSn/CSMw/w0oNv8RLDr/Ey47/xQuO/8UKzf/ESYw/w0fKP8IFx7/BA8U/wEIDP8AAwX/AQEB/wQAAP8IAgD/DQQA/xAGAv8SCAP/AIEAfv8AGAoE/xcKBP8VCgT/EgkE/w8JBf8LCQf/CAkK/wUKDf8DDRH/AhAW/wMUHP8EGSP/Bx8q/wslMv8PKzn/FDA//xc1Q/8aN0X/GzZE/xozQP8XLjn/Eycw/w4eJv8KFhz/Bg8T/wUKDP8GBwj/CAYF/wwIBf8QCgb/FAwI/xYNCf8AgQB+/wAYDQf/FwwH/xUMB/8TCwj/DwsJ/wwLC/8JDA3/Bw4R/wUQFv8FFBz/Bhki/wgeKf8LJDH/Dys5/xQxQP8YN0b/HDtL/x89Tf8gPUz/HzpH/xw0QP8XLTf/EiQt/w0bI/8KFBn/CA4S/wgLDf8KCgr/DgsK/xINC/8VDwz/FxAN/wCBAH7/ABcNCP8WDQj/FAwJ/xEMCf8ODAv/CwwN/wgND/8GDxP/BRIY/wUWHv8GGyX/CCEt/wwoNf8QLj3/FTVF/xo7S/8eP1D/IUFR/yJBUP8gPUz/HTdF/xgwO/8TJzH/Dh4m/woWHP8IEBX/CA0P/woMDf8NDAz/EQ4M/xQQDv8WEQ7/AIEAfv8AFAwI/xMMCP8QCwj/DgsJ/woKCv8HCwz/BQwP/wMOE/8CERn/AhUf/wQbJv8GIS7/Cig2/w8vP/8UNkb/GTtN/x1AUf8fQlP/IEFR/x89Tf8bN0b/Fi88/xEmMf8LHSb/BxUc/wUPFP8FDA//BwoM/woLC/8ODAz/EQ4N/xMPDf8AgQB+/wAQCgf/DgkH/wwJB/8JCAf/BggJ/wMICv8ACQ3/AAsS/wAOF/8AEx3/ABgl/wIfLf8GJjX/Cy0+/xAzRf8VOUv/GT1Q/xs/Uf8cPlD/GjtL/xc1RP8SLTr/DCMv/wcaJP8DEhr/AAwS/wAIDP8CBwn/BQcI/wgJCf8LCgr/DQsK/wCBAH7/AAwHBf8KBwX/CAYF/wUFBf8CBQb/AAUI/wAGC/8ACA//AAsU/wAPG/8AFSL/ABsq/wEiMv8GKTv/CzBC/xA1SP8UOUz/FjtO/xY6TP8VN0f/ETBA/wwoNv8HHyv/ARYg/wAOFv8ACA7/AAQJ/wADBv8AAwX/AwQF/wYGBv8IBwb/AIEAfv8ACQYE/wgFBP8FBAT/AgME/wADBf8AAgb/AAMJ/wAFDf8ACBL/AAwY/wARH/8AGCf/AB4v/wElN/8GLD7/CzFE/w41SP8QN0r/ETZI/w8yQ/8MLDz/ByQy/wIbJ/8AEh3/AAoT/wAEC/8AAAX/AAAC/wAAAf8AAQL/AgIC/wMDA/8AgQB+/wAIBQT/BwUE/wQEBP8BAwT/AAIE/wABBf8AAgj/AAML/wAGEP8AChb/AA8d/wAVJf8AGyz/ACI0/wIoO/8GLUH/CjFF/wwzRv8NMkT/Cy5A/wgoOP8DIC//ABgl/wAPGv8ACBD/AAIJ/wAABP8AAAH/AAAA/wAAAP8AAAH/AQEB/wCBAH7/AAkHBf8IBgX/BQUF/wEEBf8AAgX/AAIG/wACCP8ABAz/AAYQ/wAKFv8ADhz/ABQk/wAaK/8AIDL/ACY5/wQrP/8IL0L/CjBD/wovQv8JLD3/BiY2/wIfLf8AFiP/AA4Z/wAHEP8AAgj/AAAD/wAAAf8AAAD/AAAA/wABAf8CAgL/AIEAfv8ADAkI/wsIB/8IBwf/BAYH/wAFB/8ABAj/AAQK/wAFDf8ACBH/AAsX/wAPHf8AFST/ABor/wAgMv8AJjn/BCo+/wcuQf8JL0L/Ci5B/wkrPP8GJjb/Ah8t/wAXI/8ADxr/AAgR/wADCv8AAAX/AAAC/wAAAv8AAQL/AwMD/wUEBP8AgQB+/wAQDAr/DgsK/wwKCv8ICQn/BAgJ/wAHCv8ABwz/AAgP/wAKE/8ADRn/ABEf/wAWJf8AHCz/ACEz/wInOf8GKz7/CS5B/wsvQv8LL0D/Ciw8/wgnNv8FIC7/ARkk/wARG/8ACxP/AAYM/wADB/8AAgX/AQME/wUFBf8IBgb/CQcG/wCBAH7/ABMPDP8SDgz/Dw0M/wwLC/8ICgv/BAkM/wAKDv8ACxH/AA0V/wAQGv8AFCD/ABgn/wAdLf8BIzP/BCg5/wgsPv8LL0H/DTBC/w0vQP8NLTz/Cyg2/wghLv8EGib/ARMd/wANFf8ACQ7/AAYK/wIGB/8FBgf/CQgH/wwKCP8OCwn/AIEAfv8AFhAN/xUPDP8SDgz/Dg0M/wsMDP8HCw3/AwsP/wAMEv8ADhb/ABEb/wAVIf8AGif/AR4t/wMjM/8GKDn/Ciw9/wwvQP8OMEH/Dy9A/w8tPP8NKDb/CiIu/wcbJv8EFR3/Ag8V/wILD/8DCAv/BQcI/wkICP8NCgj/EAsJ/xIMCv8AgQB+/wAXDwv/FQ4L/xMNCv8PDAr/DAsL/wgLDP8FCw7/AgwS/wAPFv8AEhv/ABUg/wAaJv8CHiz/BSMy/wcnN/8KKzv/DS4+/w8vP/8QLj3/Dyw6/w0nNP8LIS3/CBsk/wUUHP8EDxT/AwoO/wUICv8HBwf/CwgH/w4JB/8RCwj/EwwI/wCBAH7/ABUMB/8UCwf/EgoH/w8KB/8LCQj/CAkJ/wQJDP8CCw//AA0T/wAQGf8AFB7/ABgk/wIdKv8EIS//ByU0/wopOP8MKzv/Diw7/w8rOv8OKTb/DSUw/wofKf8HGCH/BRIZ/wMMEf8DCAv/BAYG/wYFBP8KBQP/DQcD/xAIBP8SCQT/AIEAfv8AEgcC/xEHAv8PBgL/DAYC/wkFA/8GBQX/AwYI/wAIDP8AChD/AA0V/wARG/8AFSD/ARom/wMeLP8FIjD/CCU0/wooNv8MKDf/DCg1/wwlMv8KISz/CBsk/wUVHP8DDhT/AQkM/wEEBv8CAgL/BAEA/wcBAP8KAgD/DQMA/w8EAP8AgQB+/wAPAwD/DgIA/wsCAP8JAQD/BgEA/wMBAf8AAgT/AAQH/wAHDP8AChH/AA4X/wASHf8AFiL/ABon/wMeLP8FITD/ByMy/wkkMv8JIzD/CSEt/wccJ/8FFx//AhAX/wAKD/8ABAf/AAAB/wAAAP8AAAD/AwAA/wcAAP8JAAD/CwAA/wCBAH7/AAsAAP8KAAD/CAAA/wYAAP8DAAD/AAAA/wAAAP8AAQT/AAQI/wAHDv8ACxP/AA8Z/wATH/8AFyT/ABso/wMeLP8FIC7/BiEu/wcgLP8GHSj/BBkj/wITG/8ADBP/AAYL/wAAA/8AAAD/AAAA/wAAAP8AAAD/AwAA/wYAAP8HAAD/AYEAfv8ACgAA/wgAAP8HAAD/BAAA/wEAAP8AAAD/AAAA/wAAAv8AAgb/AAUM/wAJEf8ADRf/ABId/wAWIv8AGSb/ARwq/wMeLP8EHyz/BR4q/wQbJv8DFiD/ABEZ/wAKEf8ABAj/AAAA/wAAAP8AAAD/AAAA/wAAAP8BAAD/BAAA/wUAAP/aWm4Z9Ry2GgAAAABJRU5ErkJggg==)  ![Fixing C# type pattern-matching hero image](https://maltsev.space/_app/immutable/assets/hero.CQQ3JrKF.jpeg)

2024-02-18 • by Aleksey Maltsev • 733 views

# Fixing C# type pattern-matching

Craving Kotlin's secure pattern matching on sealed classes for your C# code? Discover how the Visitor Pattern can satisfy your longing!

[← Back to all posts](/blog)

Hello there! Are you looking for ways to make your code more robust, maintainable, and less prone to runtime errors? Well, buckle up because today we’re going to explore how to substitute type pattern matching in C# with the Visitors pattern. Yes, I know it sounds a bit strange, but stick with me, and you’ll see how this can help you write better code.

## [#](#task-example-validation-of-property-values)Task Example: Validation of Property Values

So let’s start with an example. Imagine we have a marker interface for some property value - `IValue`. It has two implementations - `StringValue` and `NumericValue`, holding `string` and `long` values respectively.

```
public interface IValue { }

public record StringValue(string? Value) : IValue;

public record NumericValue(long Value) : IValue;
```

As an example, let’s implement validation of those values. We write a static helper function `IsValid`, which accepts `IValue` and returns a boolean value: `true` if the value is valid, `false` otherwise. We do it in a straightforward way - just make a `switch` expression with branches for `StringValue` and `NumericValue` types. But for our `switch` to be exhaustive, we’re forced to make a default branch with throwing `UnreachableException`.

```
public static class ValidationHelper
{
    public static bool IsValid(IValue value)
    {
        return value switch
        {
            StringValue stringValue => !string.IsNullOrWhiteSpace(stringValue.Value),
            NumericValue numericValue => numericValue.Value >= 0,
            _ => throw new UnreachableException()
        };
    }
}
```

So a simple console application that spins the gears of our code will look like that:

```
while (true)
{
    Console.Write("Write a property value: ");
    var input = Console.ReadLine();
    var value = Parse(input);
    Console.WriteLine($"Value '{input}' is valid: " + ValidationHelper.IsValid(value));    
}

static IValue Parse(string? value)
{
    if (long.TryParse(value, out var num)) return new NumericValue(num);
    return new StringValue(value);
}
```

Let’s test it:

```
Write a property value: foo
Value 'foo' is valid: True
Write a property value: 1 
Value '1' is valid: True
Write a property value: -42
Value '-42' is valid: False
```

## [#](#the-issue)The Issue

At first glance everything looks fine! But what if we have dozens of such type pattern matching across the project, and some other developer introduces a new type for a value, for example, `DateTimeValue`?

In that case, he or she needs to find all usages of pattern matching for `IValue` and add a branch for the new type. And also write tests to check that we won’t have an `UnreachableException` thrown at runtime:

```
public record DateTimeValue(DateTimeOffset Value) : IValue;
```

As our case is very simple, still, let’s imaging that our imaginary developer implemented only parsing of the new type, but forgot to handle it in our helper function.

```
static IValue Parse(string? value)
{
    if (long.TryParse(value, out var num)) return new NumericValue(num);
    // Parsing date, no other changes!
    if (DateTimeOffset.TryParse(value, out var dateTime)) return new DateTimeValue(dateTime);
    return new StringValue(value);
}
```

So, you may already guess what will happen if we pass `2024-02-18T19:38:37Z` to our CLI input.

```
Write a property value: 2024-02-18T19:38:37Z
Unhandled exception. System.Diagnostics.UnreachableException: The program executed an instruction that was thought to be unreachable.                                
   at TypePatternMatchingOnVisitors.ValidationHelper.IsValid(IValue value) in C:UsersAxelUprojectslearnTypePatternMatchingOnVisitorsValidationHelper.cs:line 13
   at Program.<Main>$(String[] args) in C:UsersAxelUprojectslearnTypePatternMatchingOnVisitorsProgram.cs:line 8                                                

Process finished with exit code -532,462,766.
```

We found a bug! (how surprisingly, ha-ha)

Imaging that this will happen in production during the midnight while you’re on-call. **Not so funny now, huh?**

## [#](#solution-visitor-pattern)Solution: Visitor Pattern

I’m sure that this bug can be found with tests or during code-review. But can we have a compilation error, indicating what places to fix? Like in Kotlin or Java with sealed interfaces and classes that allow creating an exhaustive `when` expression without a default branch and receiving compilation errors when a new type is not handled.

Unfortunately, in C# we don’t have language support for that yet. But surprisingly, an old-fashioned OOP pattern called [Visitor](https://refactoring.guru/design-patterns/visitor) can help us achieve that. We can add a generic `Accept<T>` method for `IValue`, which accepts `IValueVisitor<T>` and returns a value of type `T`.

```
public interface IValue
{
    T Accept<T>(IValueVisitor<T> visitor);
}
```

Interface `IValueVisitor<T>` has methods `Accept`, with overloads, each accepting an implementation of `IValue` interface as a parameter and returning a value of generic type `T`.

```
public interface IValueVisitor<out T>
{
    T Visit(StringValue stringValue);
    T Visit(NumericValue numericValue);
}
```

In all `IValue` implementations, we just call `visitor.Visit(this)` and return the value from this invocation:

```
public record StringValue(string? Value): IValue
{
    public T Accept<T>(IValueVisitor<T> visitor) => visitor.Visit(this);
}

// ... same for other IValue implementations
```

We can rewrite a helper validation function to a class `ValueValidationVisitor`, that for each `Visit` overload performs the same check as it was done for the static function described above, generic type parameter in that case will be `bool`. Here’s how it looks like:

```
public class ValueValidationVisitor: IValueVisitor<bool>
{
    public bool Visit(StringValue stringValue) => !string.IsNullOrWhiteSpace(stringValue.Value);

    public bool Visit(NumericValue numericValue) => numericValue.Value >= 0;
}
```

When a developer adds a new class implementing `IValue`, for example, `DateTimeValue`, we need to implement an `Accept` method, which should invoke the visitor’s `Visit` method:

```
public record DateTimeValue(DateTimeOffset Value) : IValue
{
    // Compilation error - we don't implement Visit for this value type yet!
    public T Accept<T>(IValueVisitor<T> visitor) => visitor.Visit(this);
}
```

But there’s no such overload at `IValueVisitor<T>` that accepts `DateTimeValue` value, so we’ve got to add it into `IValueVisitor<T>` and implement it all over visitor’s implementations:

```
public interface IValueVisitor<out T>
{
    // ... other Visit overloads

    T Visit(DateTimeValue dateTimeValue);
}

public class ValueValidationVisitor: IValueVisitor<bool>
{
    // singleton for visitor cause it's stateless and safe to share between IValue instances
    public static readonly ValueValidationVisitor Instance = new();

    // ... other Visit overloads

    public bool Visit(DateTimeValue dateTimeValue) => dateTimeValue.Value <= DateTimeOffset.UtcNow;
}
```

So after all tons of code we’ve written, we can now change the console app and use `ValueValidationVisitor` instead of `ValidationHelper`. There’s also a singleton instance of `ValueValidationVisitor` that we can use in client code, so let’s do it.

```
while (true)
{
    Console.Write("Write a property value: ");
    var input = Console.ReadLine();
    var value = Parse(input);
    //Console.WriteLine($"Value '{input}' is valid: " + ValidationHelper.IsValid(value));    
    Console.WriteLine($"Value '{input}' is valid: " + value.Accept(ValueValidationVisitor.Instance));    
}
```

Voilà, now not only the bug is fixed, but also the chance of missed type handling is reduced, so in overall we strengthen our type-safety guarantees.

## [#](#final-thoughts)Final Thoughts

However Visitor pattern is a lot more verbose than simple switch statement or expression, we are now almost absolutely sure that the developer doesn’t miss to handle its new type. And as a reviewer, one will see all places that were changed in git diff without the need to double-check in the code of the project.

So there you have it! By using Visitor pattern, you can make your C# code more maintainable and less prone to runtime errors, especially when adding new types or modifying existing ones. It may not be as elegant or concise, but it can save you a lot of headaches in the long run. If you want to see code - check it in [this](https://github.com/AxelUser/TypePatternMatchingOnVisitors) repository.

And remember, a little bit of extra verbosity is worth the peace of mind!

[← Back to all posts](/blog)

Enjoyed this post? Share it with the universe!

[Twitter](https://twitter.com/intent/tweet?text=Fixing%20C%23%20type%20pattern-matching&url=https%3A%2F%2Fmaltsev.space%2Fblog%2F007-fixing-csharp-type-pattern-matching%3Futm_source%3Dbonobopress%26utm_medium%3Dnewsletter%26utm_campaign%3D2096) [LinkedIn](https://www.linkedin.com/shareArticle?mini=true&title=Fixing%20C%23%20type%20pattern-matching&url=https%3A%2F%2Fmaltsev.space%2Fblog%2F007-fixing-csharp-type-pattern-matching%3Futm_source%3Dbonobopress%26utm_medium%3Dnewsletter%26utm_campaign%3D2096) [Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fmaltsev.space%2Fblog%2F007-fixing-csharp-type-pattern-matching%3Futm_source%3Dbonobopress%26utm_medium%3Dnewsletter%26utm_campaign%3D2096)

## Want to read more?

 [![](https://maltsev.space/_app/immutable/assets/hero.CvtWwOfE.jpeg)

2025-07-15

### SIMD Within a Register: How I Doubled Hash Table Lookup Performance

It started with a simple thought: four bytes in a hash table bucket look just like an integer. Luckily, this one idea led to a deep dive into bit-twiddling and a 2x performance boost.

BitwiseC#](/blog/012-simd-within-a-register-how-i-doubled-hash-table-lookup-performance) [![](https://maltsev.space/_app/immutable/assets/hero.xHvcAvrs.jpeg)

2025-07-07

### Practical Bitwise Tricks in Everyday Code

An opinionated and short collection of bitwise tricks that make sense at an average 1x engineer's code.

Bitwise](/blog/011-practical-bitwise-tricks-in-everyday-code)