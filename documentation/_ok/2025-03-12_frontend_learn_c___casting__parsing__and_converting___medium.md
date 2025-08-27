```yaml
---
title: "Learn C#: Casting, Parsing, and Converting | Medium"
source: https://medium.com/@MJQuinn/learning-c-casting-parsing-and-converting-data-types-481164bfac8f
date_published: 2025-03-12T14:16:27.157Z
date_captured: 2025-08-06T17:50:22.309Z
domain: medium.com
author: Michael Quinn
category: frontend
technologies: [C#, Console Application]
programming_languages: [C#]
tags: [csharp, data-types, type-conversion, casting, parsing, programming-fundamentals, beginner, console-application, data-loss]
key_concepts: [data-casting, data-parsing, data-conversion, implicit-conversion, explicit-conversion, compile-time, runtime, data-loss]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article, part of a "Learning C#" series, explains the fundamental concepts of data type casting, parsing, and converting in C#. It differentiates between implicit and explicit conversions, highlighting when each is applicable. The author details the distinct behaviors of casting (compile-time, potential silent data loss), parsing (strict string-to-numeric conversion), and converting (flexible, runtime, throws errors on data loss). Practical C# code examples demonstrate these concepts within a console application, emphasizing common pitfalls like data loss when converting between different data types.]
---
```

# Learn C#: Casting, Parsing, and Converting | Medium

# Learning C#: Casting, Parsing, and Converting Data Types

## Chapter 1 Section 9

![A banner image with "LEARNING C#" text on a light background, partially overlaid on a blurred code editor displaying C# code.](https://miro.medium.com/v2/resize:fit:700/1*T73s3CA8B7o6oRUBYmOBoA.gif)

Learning C#: Casting, Parsing, & Converting

This next step in the learning journey will keep us building on our previous knowledge as we learn new topics. This article will continue to mix theory and practical examples in a console application as we explore some language specific problems.

> Make sure you check out the [previous articles and all future articles](/@MJQuinn/list/programming-fundamentals-with-c-cf3f29e7f8d2)
> Want to read the articles in order? [Learning C# Table of Contents](/@MJQuinn/learning-c-from-the-beginning-1fb9ca64f6)

**The goals of this section are to introduce you to data type parsing, casting, and converting. We will go over what each do and the differences between them.** By the end of this section you will be able to convert data types when you need to.

# Introduction

As we’ve discussed in other articles, there are three levels of knowledge.
1. What you know you know
2. What you know you don’t know
3. What you don’t know you don’t know

> _Throughout these sections, we will continuously be expanding topics that we know about, while introducing brand new topics. The main goal of these introduction sections is just to get stuff to move from the “never heard of it” zone to the “I’ve read about that once” zone. Understanding comes with time so if you don’t get something at first, that’s okay._

## Topics Covered

*   Operators
*   Variables
*   Data Types
*   Methods
*   Compile Time vs Runtime
*   **_NEW_**: Data Casting
*   **_NEW_**: Data Parsing
*   **_NEW_**: Data Converting
*   **_NEW_**: Implicit vs Explicit Conversion
*   **_NEW_**: Data Loss through Conversion

This section will consist of three different ways to solve a single problem. I have a value in a variable of a certain data type and I want it to be in a variable of a different data type. This problem only happens because C# is statically-typed at compile time. That means that in C#, unlike some other languages, you can never change the data type of a variable after it is declared.

This will come up if you ever need to convert string data into a numeric data type or you need to convert between numeric data types. Some casting happens automatically. That is why I can put int and float variables into a string and not get an error. The conversion is happening automatically, and is called **Implicit Conversion**, but it won’t happen automatically all the time. So this section is going to introduce you to Data Casting, Data Parsing, and Data Converting as methods to solve this problem when you run into it.

# The Theory

So before we can even get to examples we need to talk about how these three work because one of them works very differently. To understand the difference we need to bring up something we talked about at the beginning. C# is a compiled language, meaning that it is first compiled into machine code and then it is run.

> Runtime is the term that refers to all the time that the program is running.

All our code that does stuff, like write messages to the console, or runs calculations is happening during runtime. That means that if our code is slow, it can lower the performance of our application.

On the flip side, some things get run during compile time and only happen once when the code is converted to machine code. This code will not slow down or speed up the performance directly like runtime code will.

And that’s where our first difference comes in. Parsing and Converting both happen during runtime whereas Casting happens during compile time. This isn’t so important that it needs to be something you always consider, but it is important to know of.

One other thing to remember is that these three solutions are examples of **Explicit Conversion**. Unlike implicit conversion, explicit conversion requires you to provide the correct data type that you want to convert to.

## Casting

So consider the following example.

```csharp
namespace OurFirstProject
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            int num = 5;
            MustBeByte(num);
            
            Console.ReadLine();
        }
  
        static void MustBeByte(byte b)
        {
              
        }
    }
}
```

![Screenshot of C# code in an IDE showing an `int` variable `num` being passed to a method `MustBeByte` that expects a `byte`, resulting in a red squiggle error under `num`.](https://miro.medium.com/v2/resize:fit:395/1*RfXR5TSaxrDoOdeLdEQYrw.png)

Our example code

In this example we have a method called `MustBeByte` that takes in a parameter of type `byte` but we are trying to pass in a variable of type `int`. We get the red squiggle and end up getting an error when trying to run the code.

![Screenshot of a C# compiler error message indicating "Cannot convert from 'int' to 'byte'".](https://miro.medium.com/v2/resize:fit:700/1*BlTrIuPeUOr9XtoQSyJnKg.png)

Our produced error from trying to pass an int into a function looking for a byte

We see that the error states that it cannot convert from `int` to `byte` and when it says conversion it means implicit conversion. We are going to us casting to explicitly convert `int` to `byte`.

![Animated GIF demonstrating how to fix the conversion error by explicitly casting `(byte)num` in the C# code.](https://miro.medium.com/v2/resize:fit:700/1*L8_xMG6gaTIBp5cDMbWpNg.gif)

Casting an int to a byte

All we needed to do was to explicitly type the data type we wanted the variable to be casted to. Let’s make this project a little more advanced and print out the byte from the `MustBeByte` method so we can be sure it’s working.

![Screenshot of C# code showing `(byte)num` being passed to `MustBeByte` and the console output displaying `5`, confirming successful casting.](https://miro.medium.com/v2/resize:fit:700/1*nlLUpPhU0uvSlUzZa7c2dA.png)

Showing our updated code and current output.

So this is working well and was pretty easy. But what you may not see is something that sneaks up on a lot of new developers. Remember all the way back when we first talked about binary. We talked about how it takes a certain amount of bits to represent a number. And if we remember back to when we introduced data types we talked about how they all have a specific amount of memory(bits) that they hold, we may start to see a sneaky issue.

Converting a value from a larger data type to a smaller data type can lead to data loss. The data type byte can only hold a maximum of 1 byte which can only represent a maximum of 255. We can actually see this threshold by passing in 255, 256, and 257 and seeing what happens.

![Screenshot of C# code demonstrating casting `int` values (255, 256, 257) to `byte`, with the console output showing `255`, `0`, and `1`, illustrating data loss for values exceeding 255.](https://miro.medium.com/v2/resize:fit:597/1*tQkzbN832FK48HfwGPklbA.png)

We can see the output of testing numbers above 255

We can see that 256 just resets to 0 and every number over that just adds to 0. So our understanding of binary and how all data is held in RAM allows us to get a sneak peak into this possible issue of data loss due to casting.

Casting does work with tons of different data types but sadly you are not able to cast a string value into a numeric value. But you can do that with Conversion and Parsing.

> _Implicit conversion is only possible when going from a smaller data type to a bigger data type. Explicit conversion is required when going from a bigger data type to a smaller data type._

## Parsing and Conversion

These two methods are very similar in how they work so I wanted to put them together but before we can get to the examples we need to go over their big differences.

**Parsing** uses strict interpretation and only works with string types. That means that you would be allowed to parse “125” to an integer but you would not be able to parse “125.00” because that value is technically not an integer. When you are parsing you need to be sure that the data is correct.

**Converting** uses flexible transformation and works with multiple data types. this method also handles improper data better. Converting would be able to handle “125.00.”

So before we get into parsing examples, let’s see how we can solve the casting problem with conversion instead. To use conversion we need to use `Convert.ToByte(value)`.

The full code would look like this.

```csharp
internal class Program
{
    private static void Main(string[] args)
    {
        int num = 255;
        int num2 = 256;
        int num3 = 257;
        MustBeByte(Convert.ToByte(num));
        MustBeByte(Convert.ToByte(num2));
        MustBeByte(Convert.ToByte(num3));
  
        Console.ReadLine();
    }
  
    static void MustBeByte(byte b)
    {
        Console.WriteLine(b);
    }
}
```

Everything looks good and there are no errors so we can run the code and make sure it works. But it doesn’t work and instead produces an error.

![Screenshot of a C# runtime error message indicating "Value was either too large or too small for a Byte" when using `Convert.ToByte` for values exceeding the byte range.](https://miro.medium.com/v2/resize:fit:500/1*bWAOUucYaTNcU0a4ctlJOw.png)

Error message produced by our code changing from casting to conversion

The error is saying that our value is actually too high to convert. So instead of how casting let us lose our data without an error, using convert will produce an error to let us know that we are losing data.

So now that we have demonstrated how those two differ lets reset our example to now instead convert string variables to work with byte. We can convert the variables of type int to type string and change their values to be valid.

```csharp
internal class Program
{
    private static void Main(string[] args)
    {
        string num = "255";
        string num2 = "6";
        string num3 = "25";
        MustBeByte(Convert.ToByte(num));
        MustBeByte(Convert.ToByte(num2));
        MustBeByte(Convert.ToByte(num3));
  
        Console.ReadLine();
    }
  
    static void MustBeByte(byte b)
    {
        Console.WriteLine(b);
    }
}
```

![Screenshot of C# code using `Convert.ToByte` to convert string representations of numbers ("255", "6", "25") to `byte`, with the console output showing the correct converted values.](https://miro.medium.com/v2/resize:fit:505/1*XjrkstgAjY4O0rSoHm9Aig.png)

We can see the proper data conversion from string to byte

Now we can switch that up to parse instead and it should create the same output. When parsing, you instead start with the data type and use `.Parse`. So to parse to a byte you can use `Byte.Parse()`. Remember, unlike Convert, parsing will only take in a string type.

![Screenshot of C# code using `Byte.Parse` to convert string representations of numbers ("255", "6", "25") to `byte`, with the console output showing the correct parsed values.](https://miro.medium.com/v2/resize:fit:472/1*Jj5BVFAT2sWpusO7xc_7sg.png)

We can see the same output with the parsing method

# Recap

*   Implicit conversion happens when we can just drop in a variable without doing anything special.
*   Explicit conversion happens when we need to tell the program exactly what to do.
*   Casting is a quick and easy we to change data types but can also result in data loss without an error.
*   Parsing is okay when you are sure that a string variable will convert perfectly
*   Convert is a more flexible version and lets you change data types. Convert will produce errors on data loss.

# Takeaway

This is a topic where everything has been building towards this understanding. But even with knowing everything and building to this moment you may still be having trouble getting the topic down. This one might take some practice and in C# it will unfortunately be an issue you will run into.

With some time and practice you will be able to handle any data conversion issue that comes your way.

# The Author

If you would like to give a more direct donation, feel free to buy me a coffee [Ko-Fi/Mike](https://ko-fi.com/mikeq)Q. If you have any questions, complaints, or funny jokes, be sure to throw them in the comments.