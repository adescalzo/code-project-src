```yaml
---
title: Facade in C# / Design Patterns
source: https://refactoring.guru/design-patterns/facade/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T17:57:10.831Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [C#]
tags: [design-patterns, facade-pattern, structural-patterns, csharp, software-design, code-organization, api-design, complexity-management]
key_concepts: [Facade pattern, structural design pattern, subsystem, interface simplification, dependency management, client-subsystem interaction]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the Facade design pattern, a structural pattern that provides a simplified interface to a complex system of classes, libraries, or frameworks. It highlights how Facade reduces overall application complexity and helps centralize dependencies. A conceptual C# example illustrates the pattern's structure, showing how a Facade class delegates client requests to multiple subsystems while managing their lifecycle. The content also notes the pattern's common use in C# applications, especially when interacting with intricate libraries and APIs.]
---
```

# Facade in C# / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Facade](/design-patterns/facade) / [C#](/design-patterns/csharp)

![A stylized red outline of a classical building facade with columns, visually representing the Facade design pattern.](/images/patterns/cards/facade-mini.png?id=71ad6fa98b168c11cb3a1a9517dedf78)

# **Facade** in C#

**Facade** is a structural design pattern that provides a simplified (but limited) interface to a complex system of classes, library or framework.

While Facade decreases the overall complexity of the application, it also helps to move unwanted dependencies to one place.

[Learn more about Facade](/design-patterns/facade)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Facade pattern is commonly used in apps written in C#. It’s especially handy when working with complex libraries and APIs.

**Identification:** Facade can be recognized in a class that has a simple interface, but delegates most of the work to other classes. Usually, facades manage the full life life cycle of objects they use.

## Conceptual Example

This example illustrates the structure of the **Facade** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### [](#example-0--Program-cs)**Program.cs:** Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.Facade.Conceptual
{
    // The Facade class provides a simple interface to the complex logic of one
    // or several subsystems. The Facade delegates the client requests to the
    // appropriate objects within the subsystem. The Facade is also responsible
    // for managing their lifecycle. All of this shields the client from the
    // undesired complexity of the subsystem.
    public class Facade
    {
        protected Subsystem1 _subsystem1;
		
        protected Subsystem2 _subsystem2;

        public Facade(Subsystem1 subsystem1, Subsystem2 subsystem2)
        {
            this._subsystem1 = subsystem1;
            this._subsystem2 = subsystem2;
        }
		
        // The Facade's methods are convenient shortcuts to the sophisticated
        // functionality of the subsystems. However, clients get only to a
        // fraction of a subsystem's capabilities.
        public string Operation()
        {
            string result = "Facade initializes subsystems:\n";
            result += this._subsystem1.operation1();
            result += this._subsystem2.operation1();
            result += "Facade orders subsystems to perform the action:\n";
            result += this._subsystem1.operationN();
            result += this._subsystem2.operationZ();
            return result;
        }
    }
    
    // The Subsystem can accept requests either from the facade or client
    // directly. In any case, to the Subsystem, the Facade is yet another
    // client, and it's not a part of the Subsystem.
    public class Subsystem1
    {
        public string operation1()
        {
            return "Subsystem1: Ready!\n";
        }

        public string operationN()
        {
            return "Subsystem1: Go!\n";
        }
    }
	
    // Some facades can work with multiple subsystems at the same time.
    public class Subsystem2
    {
        public string operation1()
        {
            return "Subsystem2: Get ready!\n";
        }

        public string operationZ()
        {
            return "Subsystem2: Fire!\n";
        }
    }


    class Client
    {
        // The client code works with complex subsystems through a simple
        // interface provided by the Facade. When a facade manages the lifecycle
        // of the subsystem, the client might not even know about the existence
        // of the subsystem. This approach lets you keep the complexity under
        // control.
        public static void ClientCode(Facade facade)
        {
            Console.Write(facade.Operation());
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            // The client code may have some of the subsystem's objects already
            // created. In this case, it might be worthwhile to initialize the
            // Facade with these objects instead of letting the Facade create
            // new instances.
            Subsystem1 subsystem1 = new Subsystem1();
            Subsystem2 subsystem2 = new Subsystem2();
            Facade facade = new Facade(subsystem1, subsystem2);
            Client.ClientCode(facade);
        }
    }
}
```

#### [](#example-0--Output-txt)**Output.txt:** Execution result

```
Facade initializes subsystems:
Subsystem1: Ready!
Subsystem2: Get ready!
Facade orders subsystems to perform the action:
Subsystem1: Go!
Subsystem2: Fire!
```

#### Read next

[Flyweight in C#](/design-patterns/flyweight/csharp/example) 

#### Return

 [Decorator in C#](/design-patterns/decorator/csharp/example)

## **Facade** in Other Languages

[![Icon for C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/facade/cpp/example "Facade in C++") [![Icon for Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/facade/go/example "Facade in Go") [![Icon for Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/facade/java/example "Facade in Java") [![Icon for PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/facade/php/example "Facade in PHP") [![Icon for Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/facade/python/example "Facade in Python") [![Icon for Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/facade/ruby/example "Facade in Ruby") [![Icon for Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/facade/rust/example "Facade in Rust") [![Icon for Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/facade/swift/example "Facade in Swift") [![Icon for TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/facade/typescript/example "Facade in TypeScript")

![An illustration depicting various UI elements, code snippets, and development tools arranged around a central tablet screen, symbolizing software development and promoting an e-book.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)