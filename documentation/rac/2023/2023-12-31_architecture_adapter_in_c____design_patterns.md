```yaml
---
title: Adapter in C# / Design Patterns
source: https://refactoring.guru/design-patterns/adapter/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T18:02:39.006Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [IDE]
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [adapter-pattern, design-patterns, structural-pattern, csharp, object-oriented-programming, software-design, legacy-code, interface-adaptation]
key_concepts: [adapter-pattern, structural-design-pattern, incompatible-interfaces, object-collaboration, wrapper, legacy-code-integration, interface-adaptation]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Adapter design pattern is a structural pattern that enables incompatible objects to work together. It acts as a wrapper, transforming calls from one object's interface into a format recognizable by another. This pattern is particularly useful for integrating new code with existing legacy systems, allowing modern classes to interact with older ones without modifying their original interfaces. The article provides a clear conceptual C# example demonstrating how an Adapter class implements a target interface and wraps an adaptee object to achieve compatibility.]
---
```

# Adapter in C# / Design Patterns

/ [Design Patterns](/design-patterns) / [Adapter](/design-patterns/adapter) / [C#](/design-patterns/csharp)

![A conceptual diagram illustrating the Adapter design pattern, showing three distinct components that fit together, symbolizing how the adapter connects two incompatible parts. The left part is grey, the middle and right parts are red, suggesting a transformation or connection.](/images/patterns/cards/adapter-mini.png)

# **Adapter** in C#

**Adapter** is a structural design pattern, which allows incompatible objects to collaborate.

The Adapter acts as a wrapper between two objects. It catches calls for one object and transforms them to format and interface recognizable by the second object.

[Learn more about Adapter](/design-patterns/adapter)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Adapter pattern is pretty common in C# code. It’s very often used in systems based on some legacy code. In such cases, Adapters make legacy code work with modern classes.

**Identification:** Adapter is recognizable by a constructor which takes an instance of a different abstract/interface type. When the adapter receives a call to any of its methods, it translates parameters to the appropriate format and then directs the call to one or several methods of the wrapped object.

## Conceptual Example

This example illustrates the structure of the **Adapter** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

### Program.cs: Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.Adapter.Conceptual
{
    // The Target defines the domain-specific interface used by the client code.
    public interface ITarget
    {
        string GetRequest();
    }

    // The Adaptee contains some useful behavior, but its interface is
    // incompatible with the existing client code. The Adaptee needs some
    // adaptation before the client code can use it.
    class Adaptee
    {
        public string GetSpecificRequest()
        {
            return "Specific request.";
        }
    }

    // The Adapter makes the Adaptee's interface compatible with the Target's
    // interface.
    class Adapter : ITarget
    {
        private readonly Adaptee _adaptee;

        public Adapter(Adaptee adaptee)
        {
            this._adaptee = adaptee;
        }

        public string GetRequest()
        {
            return $"This is '{this._adaptee.GetSpecificRequest()}'";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Adaptee adaptee = new Adaptee();
            ITarget target = new Adapter(adaptee);

            Console.WriteLine("Adaptee interface is incompatible with the client.");
            Console.WriteLine("But with adapter client can call it's method.");

            Console.WriteLine(target.GetRequest());
        }
    }
}
```

### Output.txt: Execution result

```text
Adaptee interface is incompatible with the client.
But with adapter client can call it's method.
This is 'Specific request.'
```

#### Read next

[Bridge in C#](/design-patterns/bridge/csharp/example)

#### Return

[Singleton in C#](/design-patterns/singleton/csharp/example)

## **Adapter** in Other Languages

[![Icon representing the C++ programming language.](/images/patterns/icons/cpp.svg)](/design-patterns/adapter/cpp/example "Adapter in C++") [![Icon representing the Go programming language.](/images/patterns/icons/go.svg)](/design-patterns/adapter/go/example "Adapter in Go") [![Icon representing the Java programming language.](/images/patterns/icons/java.svg)](/design-patterns/adapter/java/example "Adapter in Java") [![Icon representing the PHP programming language.](/images/patterns/icons/php.svg)](/design-patterns/adapter/php/example "Adapter in PHP") [![Icon representing the Python programming language.](/images/patterns/icons/python.svg)](/design-patterns/adapter/python/example "Adapter in Python") [![Icon representing the Ruby programming language.](/images/patterns/icons/ruby.svg)](/design-patterns/adapter/ruby/example "Adapter in Ruby") [![Icon representing the Rust programming language.](/images/patterns/icons/rust.svg)](/design-patterns/adapter/rust/example "Adapter in Rust") [![Icon representing the Swift programming language.](/images/patterns/icons/swift.svg)](/design-patterns/adapter/swift/example "Adapter in Swift") [![Icon representing the TypeScript programming language.](/images/patterns/icons/typescript.svg)](/design-patterns/adapter/typescript/example "Adapter in TypeScript")

[![A banner image depicting various elements of a modern development environment, including a tablet displaying code, a smartphone, tools like a hammer and wrench, and abstract data visualizations, suggesting a comprehensive set of coding examples or an IDE integration.](/images/patterns/banners/examples-ide.png)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)