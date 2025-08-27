```yaml
---
title: Flyweight in C# / Design Patterns
source: https://refactoring.guru/design-patterns/flyweight/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T15:56:06.820Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Newtonsoft.Json, .NET]
programming_languages: [C#]
tags: [design-pattern, flyweight, csharp, memory-optimization, structural-pattern, object-sharing, software-design, performance]
key_concepts: [flyweight-pattern, structural-design-pattern, memory-management, object-pooling, intrinsic-state, extrinsic-state, flyweight-factory]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Flyweight design pattern, a structural pattern focused on minimizing memory consumption by enabling programs to support a vast quantity of objects through state sharing. It explains how the pattern differentiates between intrinsic (shared) and extrinsic (unique) object states, often managed by a Flyweight Factory. A detailed C# code example illustrates the pattern's implementation, demonstrating how the factory reuses existing flyweight instances to reduce new object creation. The content emphasizes that the Flyweight pattern is most effective in scenarios where a large number of objects share significant portions of their state, thereby optimizing RAM usage.]
---
```

# Flyweight in C# / Design Patterns

![Conceptual illustration for the Flyweight design pattern, showing a seesaw with a smaller grey block on one side and a larger red block on the other, symbolizing the balance and optimization of memory by sharing object state.](/images/patterns/cards/flyweight-mini.png?id=422ca8d2f90614dce810a8812c626698)

# **Flyweight** in C#

**Flyweight** is a structural design pattern that allows programs to support vast quantities of objects by keeping their memory consumption low.

The pattern achieves it by sharing parts of object state between multiple objects. In other words, the Flyweight saves RAM by caching the same data used by different objects.

[Learn more about Flyweight](/design-patterns/flyweight)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Flyweight pattern has a single purpose: minimizing memory intake. If your program doesn’t struggle with a shortage of RAM, then you might just ignore this pattern for a while.

**Identification:** Flyweight can be recognized by a creation method that returns cached objects instead of creating new.

## Conceptual Example

This example illustrates the structure of the **Flyweight** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

### Program.cs: Conceptual example

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
// Use Json.NET library, you can download it from NuGet Package Manager
using Newtonsoft.Json;

namespace RefactoringGuru.DesignPatterns.Flyweight.Conceptual
{
    // The Flyweight stores a common portion of the state (also called intrinsic
    // state) that belongs to multiple real business entities. The Flyweight
    // accepts the rest of the state (extrinsic state, unique for each entity)
    // via its method parameters.
    public class Flyweight
    {
        private Car _sharedState;

        public Flyweight(Car car)
        {
            this._sharedState = car;
        }

        public void Operation(Car uniqueState)
        {
            string s = JsonConvert.SerializeObject(this._sharedState);
            string u = JsonConvert.SerializeObject(uniqueState);
            Console.WriteLine($"Flyweight: Displaying shared {s} and unique {u} state.");
        }
    }

    // The Flyweight Factory creates and manages the Flyweight objects. It
    // ensures that flyweights are shared correctly. When the client requests a
    // flyweight, the factory either returns an existing instance or creates a
    // new one, if it doesn't exist yet.
    public class FlyweightFactory
    {
        private List<Tuple<Flyweight, string>> flyweights = new List<Tuple<Flyweight, string>>();

        public FlyweightFactory(params Car[] args)
        {
            foreach (var elem in args)
            {
                flyweights.Add(new Tuple<Flyweight, string>(new Flyweight(elem), this.getKey(elem)));
            }
        }

        // Returns a Flyweight's string hash for a given state.
        public string getKey(Car key)
        {
            List<string> elements = new List<string>();

            elements.Add(key.Model);
            elements.Add(key.Color);
            elements.Add(key.Company);

            if (key.Owner != null && key.Number != null)
            {
                elements.Add(key.Number);
                elements.Add(key.Owner);
            }

            elements.Sort();

            return string.Join("_", elements);
        }

        // Returns an existing Flyweight with a given state or creates a new
        // one.
        public Flyweight GetFlyweight(Car sharedState)
        {
            string key = this.getKey(sharedState);

            if (flyweights.Where(t => t.Item2 == key).Count() == 0)
            {
                Console.WriteLine("FlyweightFactory: Can't find a flyweight, creating new one.");
                this.flyweights.Add(new Tuple<Flyweight, string>(new Flyweight(sharedState), key));
            }
            else
            {
                Console.WriteLine("FlyweightFactory: Reusing existing flyweight.");
            }
            return this.flyweights.Where(t => t.Item2 == key).FirstOrDefault().Item1;
        }

        public void listFlyweights()
        {
            var count = flyweights.Count;
            Console.WriteLine($"\nFlyweightFactory: I have {count} flyweights:");
            foreach (var flyweight in flyweights)
            {
                Console.WriteLine(flyweight.Item2);
            }
        }
    }

    public class Car
    {
        public string Owner { get; set; }

        public string Number { get; set; }

        public string Company { get; set; }

        public string Model { get; set; }

        public string Color { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The client code usually creates a bunch of pre-populated
            // flyweights in the initialization stage of the application.
            var factory = new FlyweightFactory(
                new Car { Company = "Chevrolet", Model = "Camaro2018", Color = "pink" },
                new Car { Company = "Mercedes Benz", Model = "C300", Color = "black" },
                new Car { Company = "Mercedes Benz", Model = "C500", Color = "red" },
                new Car { Company = "BMW", Model = "M5", Color = "red" },
                new Car { Company = "BMW", Model = "X6", Color = "white" }
            );
            factory.listFlyweights();

            addCarToPoliceDatabase(factory, new Car {
                Number = "CL234IR",
                Owner = "James Doe",
                Company = "BMW",
                Model = "M5",
                Color = "red"
            });

            addCarToPoliceDatabase(factory, new Car {
                Number = "CL234IR",
                Owner = "James Doe",
                Company = "BMW",
                Model = "X1",
                Color = "red"
            });

            factory.listFlyweights();
        }

        public static void addCarToPoliceDatabase(FlyweightFactory factory, Car car)
        {
            Console.WriteLine("\nClient: Adding a car to database.");

            var flyweight = factory.GetFlyweight(new Car {
                Color = car.Color,
                Model = car.Model,
                Company = car.Company
            });

            // The client code either stores or calculates extrinsic state and
            // passes it to the flyweight's methods.
            flyweight.Operation(car);
        }
    }
}
```

### Output.txt: Execution result

```text
FlyweightFactory: I have 5 flyweights:
Camaro2018_Chevrolet_pink
black_C300_Mercedes Benz
C500_Mercedes Benz_red
BMW_M5_red
BMW_white_X6

Client: Adding a car to database.
FlyweightFactory: Reusing existing flyweight.
Flyweight: Displaying shared {"Owner":null,"Number":null,"Company":"BMW","Model":"M5","Color":"red"} and unique {"Owner":"James Doe","Number":"CL234IR","Company":"BMW","Model":"M5","Color":"red"} state.

Client: Adding a car to database.
FlyweightFactory: Can't find a flyweight, creating new one.
Flyweight: Displaying shared {"Owner":null,"Number":null,"Company":"BMW","Model":"X1","Color":"red"} and unique {"Owner":"James Doe","Number":"CL234IR","Company":"BMW","Model":"X1","Color":"red"} state.

FlyweightFactory: I have 6 flyweights:
Camaro2018_Chevrolet_pink
black_C300_Mercedes Benz
C500_Mercedes Benz_red
BMW_M5_red
BMW_white_X6
BMW_red_X1
```

#### Read next

[Proxy in C#](/design-patterns/proxy/csharp/example) 

#### Return

 [Facade in C#](/design-patterns/facade/csharp/example)

## **Flyweight** in Other Languages

![Flyweight in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858) ![Flyweight in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca) ![Flyweight in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e) ![Flyweight in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618) ![Flyweight in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f) ![Flyweight in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb) ![Flyweight in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87) ![Flyweight in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d) ![Flyweight in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)

![Image of various development tools and screens, representing an IDE or a development environment.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)