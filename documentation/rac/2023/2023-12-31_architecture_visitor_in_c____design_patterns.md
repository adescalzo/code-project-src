```yaml
---
title: Visitor in C# / Design Patterns
source: https://refactoring.guru/design-patterns/visitor/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:59:56.994Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#]
tags: [design-patterns, behavioral-pattern, visitor-pattern, csharp, object-oriented-programming, software-design, code-extensibility]
key_concepts: [visitor-pattern, behavioral-design-pattern, class-hierarchy, double-dispatch, interface, concrete-components, code-extensibility]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the **Visitor** design pattern, a behavioral pattern that enables adding new behaviors to existing class hierarchies without modifying their code. It explains the pattern's structure through a conceptual example, detailing the roles of interfaces like `IComponent` and `IVisitor`, and concrete implementations. The content includes a complete C# code example demonstrating how different visitors can operate on components, showcasing the pattern's flexibility. It also references the related concept of double dispatch and provides links to implementations in other programming languages.
---
```

# Visitor in C# / Design Patterns

# **Visitor** in C#

**Visitor** is a behavioral design pattern that allows adding new behaviors to existing class hierarchy without altering any existing code.

> Read why Visitors can’t be simply replaced with method overloading in our article [Visitor and Double Dispatch](/design-patterns/visitor-double-dispatch).

[Learn more about Visitor](/design-patterns/visitor)

![Visitor Pattern Diagram](/images/patterns/cards/visitor-mini.png?id=854a35a62963bec1d75eab996918989b)
*A diagram illustrating the Visitor pattern, showing a single external entity (visitor) interacting with multiple distinct components in a class hierarchy.*

## Conceptual Example

This example illustrates the structure of the **Visitor** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### **Program.cs:** Conceptual example

```csharp
using System;
using System.Collections.Generic;

namespace RefactoringGuru.DesignPatterns.Visitor.Conceptual
{
    // The Component interface declares an `accept` method that should take the
    // base visitor interface as an argument.
    public interface IComponent
    {
        void Accept(IVisitor visitor);
    }

    // Each Concrete Component must implement the `Accept` method in such a way
    // that it calls the visitor's method corresponding to the component's
    // class.
    public class ConcreteComponentA : IComponent
    {
        // Note that we're calling `VisitConcreteComponentA`, which matches the
        // current class name. This way we let the visitor know the class of the
        // component it works with.
        public void Accept(IVisitor visitor)
        {
            visitor.VisitConcreteComponentA(this);
        }

        // Concrete Components may have special methods that don't exist in
        // their base class or interface. The Visitor is still able to use these
        // methods since it's aware of the component's concrete class.
        public string ExclusiveMethodOfConcreteComponentA()
        {
            return "A";
        }
    }

    public class ConcreteComponentB : IComponent
    {
        // Same here: VisitConcreteComponentB => ConcreteComponentB
        public void Accept(IVisitor visitor)
        {
            visitor.VisitConcreteComponentB(this);
        }

        public string SpecialMethodOfConcreteComponentB()
        {
            return "B";
        }
    }

    // The Visitor Interface declares a set of visiting methods that correspond
    // to component classes. The signature of a visiting method allows the
    // visitor to identify the exact class of the component that it's dealing
    // with.
    public interface IVisitor
    {
        void VisitConcreteComponentA(ConcreteComponentA element);

        void VisitConcreteComponentB(ConcreteComponentB element);
    }

    // Concrete Visitors implement several versions of the same algorithm, which
    // can work with all concrete component classes.
    //
    // You can experience the biggest benefit of the Visitor pattern when using
    // it with a complex object structure, such as a Composite tree. In this
    // case, it might be helpful to store some intermediate state of the
    // algorithm while executing visitor's methods over various objects of the
    // structure.
    class ConcreteVisitor1 : IVisitor
    {
        public void VisitConcreteComponentA(ConcreteComponentA element)
        {
            Console.WriteLine(element.ExclusiveMethodOfConcreteComponentA() + " + ConcreteVisitor1");
        }

        public void VisitConcreteComponentB(ConcreteComponentB element)
        {
            Console.WriteLine(element.SpecialMethodOfConcreteComponentB() + " + ConcreteVisitor1");
        }
    }

    class ConcreteVisitor2 : IVisitor
    {
        public void VisitConcreteComponentA(ConcreteComponentA element)
        {
            Console.WriteLine(element.ExclusiveMethodOfConcreteComponentA() + " + ConcreteVisitor2");
        }

        public void VisitConcreteComponentB(ConcreteComponentB element)
        {
            Console.WriteLine(element.SpecialMethodOfConcreteComponentB() + " + ConcreteVisitor2");
        }
    }

    public class Client
    {
        // The client code can run visitor operations over any set of elements
        // without figuring out their concrete classes. The accept operation
        // directs a call to the appropriate operation in the visitor object.
        public static void ClientCode(List<IComponent> components, IVisitor visitor)
        {
            foreach (var component in components)
            {
                component.Accept(visitor);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<IComponent> components = new List<IComponent>
            {
                new ConcreteComponentA(),
                new ConcreteComponentB()
            };

            Console.WriteLine("The client code works with all visitors via the base Visitor interface:");
            var visitor1 = new ConcreteVisitor1();
            Client.ClientCode(components,visitor1);

            Console.WriteLine();

            Console.WriteLine("It allows the same client code to work with different types of visitors:");
            var visitor2 = new ConcreteVisitor2();
            Client.ClientCode(components, visitor2);
        }
    }
}
```

#### **Output.txt:** Execution result

```
The client code works with all visitors via the base Visitor interface:
A + ConcreteVisitor1
B + ConcreteVisitor1

It allows the same client code to work with different types of visitors:
A + ConcreteVisitor2
B + ConcreteVisitor2
```

#### Read next

[Design Patterns in C++](/design-patterns/cpp)

#### Return

[Template Method in C#](/design-patterns/template-method/csharp/example)

## **Visitor** in Other Languages

[![Visitor in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Visitor in C++")](/design-patterns/visitor/cpp/example) [![Visitor in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Visitor in Go")](/design-patterns/visitor/go/example) [![Visitor in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Visitor in Java")](/design-patterns/visitor/java/example) [![Visitor in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Visitor in PHP")](/design-patterns/visitor/php/example) [![Visitor in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Visitor in Python")](/design-patterns/visitor/python/example) [![Visitor in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Visitor in Ruby")](/design-patterns/visitor/ruby/example) [![Visitor in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Visitor in Rust")](/design-patterns/visitor/rust/example) [![Visitor in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Visitor in Swift")](/design-patterns/visitor/swift/example) [![Visitor in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Visitor in TypeScript")](/design-patterns/visitor/typescript/example)

![Development Environment Illustration](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)
*An abstract illustration depicting various software development elements, including a tablet displaying code, a mobile phone, tools, and charts, symbolizing an integrated development environment or software ecosystem.*

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)