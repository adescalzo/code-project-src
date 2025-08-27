```yaml
---
title: Mediator in C# / Design Patterns
source: https://refactoring.guru/design-patterns/mediator/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T15:09:48.129Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [C#]
tags: [design-patterns, behavioral-patterns, mediator-pattern, csharp, software-design, loose-coupling, object-oriented-programming, gui, mvc]
key_concepts: [mediator-pattern, loose-coupling, component-communication, design-principles, mvc-pattern, indirect-communication]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Mediator is a behavioral design pattern that reduces direct coupling between software components. It achieves this by centralizing communication through a dedicated mediator object, allowing components to interact indirectly. This pattern enhances the modifiability, extensibility, and reusability of individual components by removing their direct dependencies on numerous other classes. The article provides a conceptual C# example, illustrating the roles of the `IMediator` interface, `ConcreteMediator`, `BaseComponent`, and concrete components in coordinating actions. A common application of the Mediator pattern is in facilitating communication within GUI applications, with the Controller part of the MVC pattern being a related concept.]
---
```

# Mediator in C# / Design Patterns

/ [Design Patterns](/design-patterns) / [Mediator](/design-patterns/mediator) / [C#](/design-patterns/csharp)

![Diagram illustrating the Mediator pattern: a central red rectangle (the Mediator) with bidirectional arrows connecting it to four surrounding white rectangles (the Components), showing all communication flows through the center.](/images/patterns/cards/mediator-mini.png?id=a7e43ee8e17e4474737b1fcb3201d7ba)

# **Mediator** in C#

**Mediator** is a behavioral design pattern that reduces coupling between components of a program by making them communicate indirectly, through a special mediator object.

The Mediator makes it easy to modify, extend and reuse individual components because they’re no longer dependent on the dozens of other classes.

[Learn more about Mediator](/design-patterns/mediator)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The most popular usage of the Mediator pattern in C# code is facilitating communications between GUI components of an app. The synonym of the Mediator is the Controller part of MVC pattern.

## Conceptual Example

This example illustrates the structure of the **Mediator** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### Program.cs: Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.Mediator.Conceptual
{
    // The Mediator interface declares a method used by components to notify the
    // mediator about various events. The Mediator may react to these events and
    // pass the execution to other components.
    public interface IMediator
    {
        void Notify(object sender, string ev);
    }

    // Concrete Mediators implement cooperative behavior by coordinating several
    // components.
    class ConcreteMediator : IMediator
    {
        private Component1 _component1;

        private Component2 _component2;

        public ConcreteMediator(Component1 component1, Component2 component2)
        {
            this._component1 = component1;
            this._component1.SetMediator(this);
            this._component2 = component2;
            this._component2.SetMediator(this);
        } 

        public void Notify(object sender, string ev)
        {
            if (ev == "A")
            {
                Console.WriteLine("Mediator reacts on A and triggers following operations:");
                this._component2.DoC();
            }
            if (ev == "D")
            {
                Console.WriteLine("Mediator reacts on D and triggers following operations:");
                this._component1.DoB();
                this._component2.DoC();
            }
        }
    }

    // The Base Component provides the basic functionality of storing a
    // mediator's instance inside component objects.
    class BaseComponent
    {
        protected IMediator _mediator;

        public BaseComponent(IMediator mediator = null)
        {
            this._mediator = mediator;
        }

        public void SetMediator(IMediator mediator)
        {
            this._mediator = mediator;
        }
    }

    // Concrete Components implement various functionality. They don't depend on
    // other components. They also don't depend on any concrete mediator
    // classes.
    class Component1 : BaseComponent
    {
        public void DoA()
        {
            Console.WriteLine("Component 1 does A.");

            this._mediator.Notify(this, "A");
        }

        public void DoB()
        {
            Console.WriteLine("Component 1 does B.");

            this._mediator.Notify(this, "B");
        }
    }

    class Component2 : BaseComponent
    {
        public void DoC()
        {
            Console.WriteLine("Component 2 does C.");

            this._mediator.Notify(this, "C");
        }

        public void DoD()
        {
            Console.WriteLine("Component 2 does D.");

            this._mediator.Notify(this, "D");
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            // The client code.
            Component1 component1 = new Component1();
            Component2 component2 = new Component2();
            new ConcreteMediator(component1, component2);

            Console.WriteLine("Client triggers operation A.");
            component1.DoA();

            Console.WriteLine();

            Console.WriteLine("Client triggers operation D.");
            component2.DoD();
        }
    }
}
```

#### Output.txt: Execution result

```
Client triggers operation A.
Component 1 does A.
Mediator reacts on A and triggers following operations:
Component 2 does C.

Client triggers operation D.
Component 2 does D.
Mediator reacts on D and triggers following operations:
Component 1 does B.
Component 2 does C.
```

#### Read next

[Memento in C#](/design-patterns/memento/csharp/example) 

#### Return

 [Iterator in C#](/design-patterns/iterator/csharp/example)

## **Mediator** in Other Languages

[![Mediator in C++ icon](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/mediator/cpp/example "Mediator in C++") [![Mediator in Go icon](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/mediator/go/example "Mediator in Go") [![Mediator in Java icon](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/mediator/java/example "Mediator in Java") [![Mediator in PHP icon](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/mediator/php/example "Mediator in PHP") [![Mediator in Python icon](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/mediator/python/example "Mediator in Python") [![Mediator in Ruby icon](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/mediator/ruby/example "Mediator in Ruby") [![Mediator in Rust icon](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/mediator/rust/example "Mediator in Rust") [![Mediator in Swift icon](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/mediator/swift/example "Mediator in Swift") [![Mediator in TypeScript icon](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/mediator/typescript/example "Mediator in TypeScript")

![Banner promoting an eBook, showing a desktop environment with various UI elements, code snippets, charts, and development tools, suggesting an integrated development experience.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)