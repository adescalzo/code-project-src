```yaml
---
title: "Coding Grounds: Discriminated Union Part Two - The C# side of things"
source: https://toreaurstad.blogspot.com/2024/05/discriminated-union-part-two-c-side-of.html?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=choosing-the-best-immutable-dictionary
date_published: unknown
date_captured: 2025-08-21T16:55:29.535Z
domain: toreaurstad.blogspot.com
author: Tore Aurstad
category: ai_ml
technologies: [F#, C#, .NET, .NET 9, .NET 10, C# 13, OneOf]
programming_languages: [C#, F#, Bash]
tags: [discriminated-unions, csharp, fsharp, functional-programming, object-oriented-programming, pattern-matching, records, language-features, dotnet, type-testing]
key_concepts: [Discriminated Unions, Pattern Matching, Type Testing, Object Inheritance, Functional Programming, Object-Oriented Programming, Records, Enum Classes]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article, the second part of a series, explores the implementation of discriminated unions (DUs) in C#, contrasting it with F#'s native support. It demonstrates how to achieve DU-like functionality in C# using abstract records and pattern matching with type testing. The author highlights the current limitations in C# compared to F#, particularly the reliance on object inheritance. The article also discusses future C# language proposals, such as "Enum classes," which aim to introduce built-in support for DUs in upcoming .NET versions.]
---
```

# Coding Grounds: Discriminated Union Part Two - The C# side of things

### Discriminated Union Part Two - The C# side of things

In this article , _discriminated unions_ will be further looked into, continuing from the last article. It visited these topics using F#. The previous article showing the previous article focused on F# and discriminated unions is available here:  
  
[https://toreaurstad.blogspot.com/2024/05/discriminated-unions-part-one-f-side-of.html](https://toreaurstad.blogspot.com/2024/05/discriminated-unions-part-one-f-side-of.html "https://toreaurstad.blogspot.com/2024/05/discriminated-unions-part-one-f-side-of.html")  
  
In this article, C# will be used. As last noted, _discriminated unions_ are a set of types that are allowed to be used. In F#, these types dont have to be in an inheritance chain, they can really be a mix of different types. In C# however, one has to use a base type for the union itself and declare this as abstract, i.e. a _placeholder_ for our discriminated union, called DU from now in this article. C# is a mix of object oriented and functional programming language. It does not support discriminated unions as built-in constructs, such as F#. We must use object inheritance still, but _pattern matching_ in C# with _type testing_. Lets first look at the POCOs that are included in this example, we must use a base class for our union. In F# we had this:
```bash
type Shape =
    | Rectangle of width : float * length : float
    | Circle of radius : float
    | Prism of width : float * depth:float * height : float
    | Cube of width : float
```
In C# we use an abstract _record_, since they possess immutability after construction has been made and are therefore a good match for functional programming (FP). Also, records offer a compact syntax which lets itself nice to FP. (we COULD use an abstract class too, but records are now available and lends themsevles better to FP since they are immutable after construction is finished). We could define this abstract record baseclass which will function as a Discriminated Union (DU) like:
```csharp
public abstract record Shape;
```
However, keeping note of which types are allowed into the DU is easier if we nest our types. I have also included the methods on the Shape objects as static methods that uses _pattern matching with type testing_ to define the _bounds_ of our DU.
```csharp
public abstract record Shape
{

	public record Rectangle(float Width, float Length) : Shape;
	public record Circle(float Radius) : Shape;
	public record Prism(float Width, float Depth, float Length) : Shape;
	public record Cube(float Width) : Shape;
	public record Torus(float LargeRadius, float SmallRadius) : Shape; //we will discriminate this shape, not include it in our supported calculations

	public static double CalcArea(Shape shape) => shape switch
	{
		Rectangle rect => rect.Width * rect.Length,
		Circle circ => Math.PI * Math.Pow(circ.Radius, 2),
		Prism prism => 2.0*(prism.Width*prism.Depth) + 2.0*(prism.Width+prism.Depth)*prism.Length,
		Cube cube => 6 * Math.Pow(cube.Width, 2),
		_ => throw new NotSupportedException($"Area calculation for this Shape: ${shape.GetType()}")
	};

	public static double CalcVolume(Shape shape) => shape switch
	{
		Prism prism => prism.Width * prism.Depth * prism.Length,
		Cube cube => Math.Pow(cube.Width, 3),
		_ => throw new NotSupportedException($"Volume calculation for this Shape: ${shape.GetType()}")
	};

};
```
Sample code of using this source code is shown below:
```csharp
void Main()
{
	var torus = new Shape.Torus(LargeRadius: 7, SmallRadius: 3);
	//var torusArea = Shape.CalcArea(torus);

	var rect = new Shape.Rectangle(Width: 1.3f, Length: 10.0f);
	var circle = new Shape.Circle(Radius: 2.0f);
	var prism = new Shape.Prism(Width: 15, Depth: 5, Length: 7);
	var cube = new Shape.Cube(Width: 2.0f);

	var rectArea = Shape.CalcArea(rect);
	var circleArea = Shape.CalcArea(circle);
	var prismArea = Shape.CalcArea(prism);
	var cubeArea = Shape.CalcArea(cube);

	//var circleVolume = Shape.CalcVolume(circle);
	var prismVolume = Shape.CalcVolume(prism);
	var cubeVolume = Shape.CalcVolume(cube);
	//var rectVolume = Shape.CalcVolume(rect);

	Console.WriteLine("\nAREA CALCULATIONS:");
	Console.WriteLine($"Circle area: {circleArea:F2}");
	Console.WriteLine($"Prism area: {prismArea:F2}");
	Console.WriteLine($"Cube area: {cubeArea:F2}");
	Console.WriteLine($"Rectangle area: {rectArea:F2}");

	Console.WriteLine("\nVOLUME CALCULATIONS:");
	//Console.WriteLine( "Circle volume: %A", circleVolume);
	Console.WriteLine($"Prism volume: {prismVolume:F2}");
	Console.WriteLine($"Cube volume: {cubeVolume:F2}");
	//Console.WriteLine( "Rectangle volume: %A", rectVolume);
}
```
I have commented out some lines here, they will throw an _UnsupportedException_ if one uncomments them running the code. The torus forexample lacks support for area and volume calculation by intent, it is not supported (yet). The calculations of the volume of a circle and a rectangle is not possible, since they are 2D geometric figures and not 3D, i.e. do not posess a volume. Output from running the program is shown below:
```bash
AREA CALCULATIONS:
Circle area: 12,57
Prism area: 430,00
Cube area: 24,00
Rectangle area: 13,00

VOLUME CALCULATIONS:
Prism volume: 525,00
Cube volume: 8,00
```

#### Conclusions F# vs C#

True support for DU is only available in F#, but we can get close to it using C#, inheritance, pattern matching with type checking. F# got much better support for it for now, but C# probably will catch up in a few years and also finally get support for it as a built-in construct. The syntax for DU in F# an C# is fairly similar, using records and pattern switching with type checking makes the code in C# not longer than in F#, but F# got direct support for DU, in C# we have to add additional code to support something that is a built-in functionality of F#. Listed on the page _What's new in C# 13_, DU has not made their way into the list, .NET 9 Preview SDK will be available probably in November this year (2024).  
  
[https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13 "https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13")  
  
There are different approaches to writing DU in C# for now. Some go for the _OneOf_ operator of functional programming, not presented further in this article. Probably discriminated unions will make their way in .NET 10 in November 2027, so there will still be a lot of waiting around for getting this feature into C#. For now, being aware what the buzz about DU is all about, my two articles on it hopefully made it a bit clearer. One disadvantage of this is that it’s not consistent like in F#. We have to manually manage which types we want to support in each method. However, this is done using inheritance in C#. At the same time, we need to adjust the inheritance hierarchy so that all types inherit from such a discriminated union (DU). If a type needs to be part of MULTIPLE different DUs, we face limitations in C# since we can only inherit from a specific type in the hierarchy. This is likely why many C# developers are requesting DU functionality. As of now, Microsoft’s language team seems to be leaning toward something called ENUM CLASSES. It appears that this feature will be included in .NET 10, which means it won’t be available until 2027  

#### Further viewing/reading of the topic

There are proposals for better support of DU in C# is taking its form now in concrete propals. A proposal for _Enum classes_ are available here, it could be the design choice C# language team lands on:  
  
[https://github.com/dotnet/csharplang/blob/main/proposals/discriminated-unions.md](https://github.com/dotnet/csharplang/blob/main/proposals/discriminated-unions.md "https://github.com/dotnet/csharplang/blob/main/proposals/discriminated-unions.md")  
  
Lead Designer Mads Torgersen comments around DU in C# in this video at 21:00 :  
  
[https://learn.microsoft.com/en-us/shows/ask-the-expert/ask-the-expert-whats-new-in-c-100](https://learn.microsoft.com/en-us/shows/ask-the-expert/ask-the-expert-whats-new-in-c-100 "https://learn.microsoft.com/en-us/shows/ask-the-expert/ask-the-expert-whats-new-in-c-100")