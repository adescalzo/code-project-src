```yaml
---
title: Prototype in C# / Design Patterns
source: https://refactoring.guru/design-patterns/prototype/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T17:58:04.644Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#]
tags: [design-patterns, creational-patterns, prototype, object-cloning, csharp, object-oriented, shallow-copy, deep-copy]
key_concepts: [Prototype design pattern, object cloning, shallow copy, deep copy, ICloneable interface, MemberwiseClone method]
code_examples: false
difficulty_level: intermediate
summary: |
  The Prototype design pattern is a creational pattern that enables the cloning of objects, even complex ones, without coupling to their specific classes. It relies on a common interface that allows objects to produce full copies of themselves, leveraging access to private fields within the same class. The article demonstrates the pattern's implementation in C#, showcasing both shallow and deep copying techniques using the `MemberwiseClone()` method and custom logic. This pattern is particularly useful for creating new instances from existing objects efficiently.
---
```

# Prototype in C# / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Prototype](/design-patterns/prototype) / [C#](/design-patterns/csharp)

![Illustration of the Prototype pattern showing a grey robot transforming into a red robot, symbolizing object cloning.](/images/patterns/cards/prototype-mini.png?id=bc3046bb39ff36574c08d49839fd1c8e)

# **Prototype** in C#

**Prototype** is a creational design pattern that allows cloning objects, even complex ones, without coupling to their specific classes.

All prototype classes should have a common interface that makes it possible to copy objects even if their concrete classes are unknown. Prototype objects can produce full copies since objects of the same class can access each other’s private fields.

[Learn more about Prototype](/design-patterns/prototype)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Prototype pattern is available in C# out of the box with a `ICloneable` interface.

**Identification:** The prototype can be easily recognized by a `clone` or `copy` methods, etc.

## Conceptual Example

This example illustrates the structure of the **Prototype** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### [](#example-0--Program-cs)**Program.cs:** Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.Prototype.Conceptual
{
    public class Person
    {
        public int Age;
        public DateTime BirthDate;
        public string Name;
        public IdInfo IdInfo;

        public Person ShallowCopy()
        {
            return (Person) this.MemberwiseClone();
        }

        public Person DeepCopy()
        {
            Person clone = (Person) this.MemberwiseClone();
            clone.IdInfo = new IdInfo(IdInfo.IdNumber);
            clone.Name = String.Copy(Name);
            return clone;
        }
    }

    public class IdInfo
    {
        public int IdNumber;

        public IdInfo(int idNumber)
        {
            this.IdNumber = idNumber;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Person p1 = new Person();
            p1.Age = 42;
            p1.BirthDate = Convert.ToDateTime("1977-01-01");
            p1.Name = "Jack Daniels";
            p1.IdInfo = new IdInfo(666);

            // Perform a shallow copy of p1 and assign it to p2.
            Person p2 = p1.ShallowCopy();
            // Make a deep copy of p1 and assign it to p3.
            Person p3 = p1.DeepCopy();

            // Display values of p1, p2 and p3.
            Console.WriteLine("Original values of p1, p2, p3:");
            Console.WriteLine("   p1 instance values: ");
            DisplayValues(p1);
            Console.WriteLine("   p2 instance values:");
            DisplayValues(p2);
            Console.WriteLine("   p3 instance values:");
            DisplayValues(p3);

            // Change the value of p1 properties and display the values of p1,
            // p2 and p3.
            p1.Age = 32;
            p1.BirthDate = Convert.ToDateTime("1900-01-01");
            p1.Name = "Frank";
            p1.IdInfo.IdNumber = 7878;
            Console.WriteLine("\nValues of p1, p2 and p3 after changes to p1:");
            Console.WriteLine("   p1 instance values: ");
            DisplayValues(p1);
            Console.WriteLine("   p2 instance values (reference values have changed):");
            DisplayValues(p2);
            Console.WriteLine("   p3 instance values (everything was kept the same):");
            DisplayValues(p3);
        }

        public static void DisplayValues(Person p)
        {
            Console.WriteLine("      Name: {0:s}, Age: {1:d}, BirthDate: {2:MM/dd/yy}",
                p.Name, p.Age, p.BirthDate);
            Console.WriteLine("      ID#: {0:d}", p.IdInfo.IdNumber);
        }
    }
}
```

#### [](#example-0--Output-txt)**Output.txt:** Execution result

```
Original values of p1, p2, p3:
   p1 instance values: 
      Name: Jack Daniels, Age: 42, BirthDate: 01/01/77
      ID#: 666
   p2 instance values:
      Name: Jack Daniels, Age: 42, BirthDate: 01/01/77
      ID#: 666
   p3 instance values:
      Name: Jack Daniels, Age: 42, BirthDate: 01/01/77
      ID#: 666

Values of p1, p2 and p3 after changes to p1:
   p1 instance values: 
      Name: Frank, Age: 32, BirthDate: 01/01/00
      ID#: 7878
   p2 instance values (reference values have changed):
      Name: Jack Daniels, Age: 42, BirthDate: 01/01/77
      ID#: 7878
   p3 instance values (everything was kept the same):
      Name: Jack Daniels, Age: 42, BirthDate: 01/01/77
      ID#: 666
```

#### Read next

[Singleton in C#](/design-patterns/singleton/csharp/example) 

#### Return

 [Factory Method in C#](/design-patterns/factory-method/csharp/example)

## **Prototype** in Other Languages

[![Prototype in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/prototype/cpp/example "Prototype in C++") [![Prototype in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/prototype/go/example "Prototype in Go") [![Prototype in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/prototype/java/example "Prototype in Java") [![Prototype in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/prototype/php/example "Prototype in PHP") [![Prototype in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/prototype/python/example "Prototype in Python") [![Prototype in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/prototype/ruby/example "Prototype in Ruby") [![Prototype in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/prototype/rust/example "Prototype in Rust") [![Prototype in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/prototype/swift/example "Prototype in Swift") [![Prototype in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/prototype/typescript/example "Prototype in TypeScript")

[![Banner promoting an e-book or archive of design pattern examples, featuring a central tablet surrounded by UI elements, code snippets, and development tools.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)