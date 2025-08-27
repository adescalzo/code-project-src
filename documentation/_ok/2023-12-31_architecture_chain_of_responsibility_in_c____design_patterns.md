```yaml
---
title: Chain of Responsibility in C# / Design Patterns
source: https://refactoring.guru/design-patterns/chain-of-responsibility/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T15:09:53.416Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript, .NET, IDE]
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, behavioral-patterns, chain-of-responsibility, csharp, object-oriented-programming, software-architecture, handler, request-processing, example]
key_concepts: [chain-of-responsibility-pattern, handler-interface, request-handling, object-chaining, behavioral-patterns, decoupling, dynamic-composition]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Chain of Responsibility, a behavioral design pattern that enables passing requests sequentially along a chain of potential handlers. It explains how the pattern decouples the sender from the receiver by allowing multiple objects to attempt to handle a request until one succeeds. The content provides a detailed conceptual example implemented in C#, showcasing the `IHandler` interface, `AbstractHandler` base class, and concrete handlers. It also highlights the pattern's usage in scenarios like filters and event chains. The article includes code examples and their execution output, demonstrating the pattern's practical application.]
---
```

# Chain of Responsibility in C# / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Chain of Responsibility](/design-patterns/chain-of-responsibility) / [C#](/design-patterns/csharp)

![Chain of Responsibility](/images/patterns/cards/chain-of-responsibility-mini.png?id=36d85eba8d14986f053123de17aac7a7 "Icon representing the Chain of Responsibility design pattern")

# **Chain of Responsibility** in C#

**Chain of Responsibility** is a behavioral design pattern that allows passing a request along a chain of potential handlers until one of them handles the request.

The pattern allows multiple objects to handle the request without coupling the sender class to the concrete classes of the receivers. The chain can be composed dynamically at runtime with any handler that follows a standard handler interface.

[Learn more about Chain of Responsibility](/design-patterns/chain-of-responsibility)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Usage examples:** The Chain of Responsibility is pretty common in C#. It’s mostly relevant when your code operates with chains of objects, such as filters, event chains, etc.

**Identification:** The pattern is recognizable by behavioral methods of one group of objects that indirectly call the same methods in other objects, while all the objects follow the common interface.

## Conceptual Example

This example illustrates the structure of the **Chain of Responsibility** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way are the elements of the pattern related?

#### Program.cs: Conceptual example

```csharp
using System;
using System.Collections.Generic;

namespace RefactoringGuru.DesignPatterns.ChainOfResponsibility.Conceptual
{
    // The Handler interface declares a method for building the chain of
    // handlers. It also declares a method for executing a request.
    public interface IHandler
    {
        IHandler SetNext(IHandler handler);
		
        object Handle(object request);
    }

    // The default chaining behavior can be implemented inside a base handler
    // class.
    abstract class AbstractHandler : IHandler
    {
        private IHandler _nextHandler;

        public IHandler SetNext(IHandler handler)
        {
            this._nextHandler = handler;
            
            // Returning a handler from here will let us link handlers in a
            // convenient way like this:
            // monkey.SetNext(squirrel).SetNext(dog);
            return handler;
        }
		
        public virtual object Handle(object request)
        {
            if (this._nextHandler != null)
            {
                return this._nextHandler.Handle(request);
            }
            else
            {
                return null;
            }
        }
    }

    class MonkeyHandler : AbstractHandler
    {
        public override object Handle(object request)
        {
            if ((request as string) == "Banana")
            {
                return $"Monkey: I'll eat the {request.ToString()}.\\n";
            }
            else
            {
                return base.Handle(request);
            }
        }
    }

    class SquirrelHandler : AbstractHandler
    {
        public override object Handle(object request)
        {
            if (request.ToString() == "Nut")
            {
                return $"Squirrel: I'll eat the {request.ToString()}.\\n";
            }
            else
            {
                return base.Handle(request);
            }
        }
    }

    class DogHandler : AbstractHandler
    {
        public override object Handle(object request)
        {
            if (request.ToString() == "MeatBall")
            {
                return $"Dog: I'll eat the {request.ToString()}.\\n";
            }
            else
            {
                return base.Handle(request);
            }
        }
    }

    class Client
    {
        // The client code is usually suited to work with a single handler. In
        // most cases, it is not even aware that the handler is part of a chain.
        public static void ClientCode(AbstractHandler handler)
        {
            foreach (var food in new List<string> { "Nut", "Banana", "Cup of coffee" })
            {
                Console.WriteLine($"Client: Who wants a {food}?");

                var result = handler.Handle(food);

                if (result != null)
                {
                    Console.Write($"   {result}");
                }
                else
                {
                    Console.WriteLine($"   {food} was left untouched.");
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The other part of the client code constructs the actual chain.
            var monkey = new MonkeyHandler();
            var squirrel = new SquirrelHandler();
            var dog = new DogHandler();

            monkey.SetNext(squirrel).SetNext(dog);

            // The client should be able to send a request to any handler, not
            // just the first one in the chain.
            Console.WriteLine("Chain: Monkey > Squirrel > Dog\\n");
            Client.ClientCode(monkey);
            Console.WriteLine();

            Console.WriteLine("Subchain: Squirrel > Dog\\n");
            Client.ClientCode(squirrel);
        }
    }
}
```

#### Output.txt: Execution result

```
Chain: Monkey > Squirrel > Dog

Client: Who wants a Nut?
   Squirrel: I'll eat the Nut.
Client: Who wants a Banana?
   Monkey: I'll eat the Banana.
Client: Who wants a Cup of coffee?
   Cup of coffee was left untouched.

Subchain: Squirrel > Dog

Client: Who wants a Nut?
   Squirrel: I'll eat the Nut.
Client: Who wants a Banana?
   Banana was left untouched.
Client: Who wants a Cup of coffee?
   Cup of coffee was left untouched.
```

#### Read next

[Command in C#](/design-patterns/command/csharp/example) 

#### Return

 [Proxy in C#](/design-patterns/proxy/csharp/example)

## **Chain of Responsibility** in Other Languages

[![Chain of Responsibility in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Icon for C++")](/design-patterns/chain-of-responsibility/cpp/example "Chain of Responsibility in C++") [![Chain of Responsibility in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Icon for Go")](/design-patterns/chain-of-responsibility/go/example "Chain of Responsibility in Go") [![Chain of Responsibility in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Icon for Java")](/design-patterns/chain-of-responsibility/java/example "Chain of Responsibility in Java") [![Chain of Responsibility in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Icon for PHP")](/design-patterns/chain-of-responsibility/php/example "Chain of Responsibility in PHP") [![Chain of Responsibility in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Icon for Python")](/design-patterns/chain-of-responsibility/python/example "Chain of Responsibility in Python") [![Chain of Responsibility in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Icon for Ruby")](/design-patterns/chain-of-responsibility/ruby/example "Chain of Responsibility in Ruby") [![Chain of Responsibility in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Icon for Rust")](/design-patterns/chain-of-responsibility/rust/example "Chain of Responsibility in Rust") [![Chain of Responsibility in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Icon for Swift")](/design-patterns/chain-of-responsibility/swift/example "Chain of Responsibility in Swift") [![Chain of Responsibility in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Icon for TypeScript")](/design-patterns/chain-of-responsibility/typescript/example "Chain of Responsibility in TypeScript")

![Banner promoting an eBook with examples for an IDE](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc "Banner promoting an eBook with examples for an IDE")

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

---
**Image Analysis:**

*   **Image 1 (from content):** `![Chain of Responsibility](/images/patterns/cards/chain-of-responsibility-mini.png?id=36d85eba8d14986f053123de17aac7a7)`
    *   **Description:** A small, stylized icon representing the Chain of Responsibility design pattern, typically used as a card or thumbnail image.

*   **Image 2 (from content):** `![](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)`
    *   **Description:** A promotional banner image featuring a stylized depiction of an Integrated Development Environment (IDE) with code snippets, suggesting the availability of practical code examples and an accompanying eBook.

*   **Image 3 (provided separately):** `image.png`
    *   **Description:** An abstract illustration depicting various elements of software development and data processing. It includes visual metaphors for code (e.g., `< />` tags), data visualization (charts), mobile application interfaces, and tools like a hammer and wrench, suggesting a comprehensive ecosystem for building and managing software.