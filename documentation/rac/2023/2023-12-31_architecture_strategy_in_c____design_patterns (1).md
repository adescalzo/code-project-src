```yaml
---
title: Strategy in C# / Design Patterns
source: https://refactoring.guru/design-patterns/strategy/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T14:00:46.811Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#]
tags: [design-patterns, strategy-pattern, behavioral-patterns, csharp, software-design, object-oriented-programming, algorithm, polymorphism, code-structure, extensibility]
key_concepts: [strategy-pattern, behavioral-design-pattern, context-object, strategy-interface, concrete-strategies, algorithm-encapsulation, interchangeable-algorithms, runtime-behavior-modification]
code_examples: false
difficulty_level: intermediate
summary: |
  The Strategy design pattern is a behavioral pattern that encapsulates a set of interchangeable behaviors into separate objects. It allows an object, known as the context, to delegate the execution of a specific behavior to one of these strategy objects. This pattern enables the context's behavior to be changed at runtime by replacing the currently linked strategy object. It is commonly used in frameworks to provide flexibility, allowing users to alter a class's behavior without modifying its core structure. The article provides a conceptual C# example demonstrating the pattern's structure with sorting algorithms.
---
```

# Strategy in C# / Design Patterns

[Design Patterns](/design-patterns) / [Strategy](/design-patterns/strategy) / [C#](/design-patterns/csharp)

![An abstract diagram illustrating the Strategy design pattern, showing a context object (left, with dashed line indicating a changeable part) delegating an action to an interchangeable strategy object (right, with a hand icon), which can be one of several concrete strategies (e.g., sword and shield icons representing different algorithms).](/images/patterns/cards/strategy-mini.png?id=d38abee4fb6f2aed909d262bdadca936)

# Strategy in C#

**Strategy** is a behavioral design pattern that turns a set of behaviors into objects and makes them interchangeable inside original context object.

The original object, called context, holds a reference to a strategy object. The context delegates executing the behavior to the linked strategy object. In order to change the way the context performs its work, other objects may replace the currently linked strategy object with another one.

[Learn more about Strategy](/design-patterns/strategy)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Strategy pattern is very common in C# code. It’s often used in various frameworks to provide users a way to change the behavior of a class without extending it.

**Identification:** Strategy pattern can be recognized by a method that lets a nested object do the actual work, as well as a setter that allows replacing that object with a different one.

## Conceptual Example

This example illustrates the structure of the **Strategy** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### Program.cs: Conceptual example

```csharp
using System;
using System.Collections.Generic;

namespace RefactoringGuru.DesignPatterns.Strategy.Conceptual
{
    // The Context defines the interface of interest to clients.
    class Context
    {
        // The Context maintains a reference to one of the Strategy objects. The
        // Context does not know the concrete class of a strategy. It should
        // work with all strategies via the Strategy interface.
        private IStrategy _strategy;

        public Context()
        { }

        // Usually, the Context accepts a strategy through the constructor, but
        // also provides a setter to change it at runtime.
        public Context(IStrategy strategy)
        {
            this._strategy = strategy;
        }

        // Usually, the Context allows replacing a Strategy object at runtime.
        public void SetStrategy(IStrategy strategy)
        {
            this._strategy = strategy;
        }

        // The Context delegates some work to the Strategy object instead of
        // implementing multiple versions of the algorithm on its own.
        public void DoSomeBusinessLogic()
        {
            Console.WriteLine("Context: Sorting data using the strategy (not sure how it'll do it)");
            var result = this._strategy.DoAlgorithm(new List<string> { "a", "b", "c", "d", "e" });

            string resultStr = string.Empty;
            foreach (var element in result as List<string>)
            {
                resultStr += element + ",";
            }

            Console.WriteLine(resultStr);
        }
    }

    // The Strategy interface declares operations common to all supported
    // versions of some algorithm.
    //
    // The Context uses this interface to call the algorithm defined by Concrete
    // Strategies.
    public interface IStrategy
    {
        object DoAlgorithm(object data);
    }

    // Concrete Strategies implement the algorithm while following the base
    // Strategy interface. The interface makes them interchangeable in the
    // Context.
    class ConcreteStrategyA : IStrategy
    {
        public object DoAlgorithm(object data)
        {
            var list = data as List<string>;
            list.Sort();

            return list;
        }
    }

    class ConcreteStrategyB : IStrategy
    {
        public object DoAlgorithm(object data)
        {
            var list = data as List<string>;
            list.Sort();
            list.Reverse();

            return list;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The client code picks a concrete strategy and passes it to the
            // context. The client should be aware of the differences between
            // strategies in order to make the right choice.
            var context = new Context();

            Console.WriteLine("Client: Strategy is set to normal sorting.");
            context.SetStrategy(new ConcreteStrategyA());
            context.DoSomeBusinessLogic();
            
            Console.WriteLine();
            
            Console.WriteLine("Client: Strategy is set to reverse sorting.");
            context.SetStrategy(new ConcreteStrategyB());
            context.DoSomeBusinessLogic();
        }
    }
}
```

#### Output.txt: Execution result

```
Client: Strategy is set to normal sorting.
Context: Sorting data using the strategy (not sure how it'll do it)
a,b,c,d,e

Client: Strategy is set to reverse sorting.
Context: Sorting data using the strategy (not sure how it'll do it)
e,d,c,b,a
```

#### Read next

[Template Method in C#](/design-patterns/template-method/csharp/example) 

#### Return

 [State in C#](/design-patterns/state/csharp/example)

## Strategy in Other Languages

[![Strategy in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/strategy/cpp/example "Strategy in C++") [![Strategy in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/strategy/go/example "Strategy in Go") [![Strategy in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/strategy/java/example "Strategy in Java") [![Strategy in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/strategy/php/example "Strategy in PHP") [![Strategy in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/strategy/python/example "Strategy in Python") [![Strategy in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/strategy/ruby/example "Strategy in Ruby") [![Strategy in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/strategy/rust/example "Strategy in Rust") [![Strategy in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/strategy/swift/example "Strategy in Swift") [![Strategy in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/strategy/typescript/example "Strategy in TypeScript")

![A colorful illustration depicting various software development tools and interfaces, including a tablet displaying code, a mobile phone, a desktop monitor with charts, and various coding symbols, representing a comprehensive development environment or resource.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)