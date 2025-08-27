```yaml
---
title: Template Method in C# / Design Patterns
source: https://refactoring.guru/design-patterns/template-method/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:59:54.786Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [C#]
tags: [design-patterns, behavioral-patterns, template-method, csharp, object-oriented-programming, inheritance, software-design, algorithm, abstraction]
key_concepts: [Template Method pattern, abstract class, concrete class, algorithm skeleton, primitive operations, hooks, inheritance, polymorphism]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Template Method is a behavioral design pattern that defines the skeleton of an algorithm in a base class, allowing subclasses to override specific steps without altering the overall structure. It promotes code reuse and ensures that the core algorithm remains consistent while providing flexibility for variations. The pattern utilizes abstract methods for mandatory steps and virtual "hook" methods for optional extensions. This C# example demonstrates its implementation using abstract and concrete classes, showcasing how client code can interact with different subclasses through the base class interface. It highlights the roles of abstract and concrete classes in defining and implementing the algorithm's steps.]
---
```

# Template Method in C# / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Template Method](/design-patterns/template-method) / [C#](/design-patterns/csharp)

![Template Method - A small icon representing the Template Method design pattern.](/images/patterns/cards/template-method-mini.png?id=9f200248d88026d8e79d0f3dae411ab4)

# **Template Method** in C#

**Template Method** is a behavioral design pattern that allows you to define a skeleton of an algorithm in a base class and let subclasses override the steps without changing the overall algorithm’s structure.

[Learn more about Template Method](/design-patterns/template-method)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Template Method pattern is quite common in C# frameworks. Developers often use it to provide framework users with a simple means of extending standard functionality using inheritance.

**Identification:** Template Method can be recognized if you see a method in base class that calls a bunch of other methods that are either abstract or empty.

## Conceptual Example

This example illustrates the structure of the **Template Method** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### [](#example-0--Program-cs)**Program.cs:** Conceptual example

using System;

namespace RefactoringGuru.DesignPatterns.TemplateMethod.Conceptual
{
    // The Abstract Class defines a template method that contains a skeleton of
    // some algorithm, composed of calls to (usually) abstract primitive
    // operations.
    //
    // Concrete subclasses should implement these operations, but leave the
    // template method itself intact.
    abstract class AbstractClass
    {
        // The template method defines the skeleton of an algorithm.
        public void TemplateMethod()
        {
            this.BaseOperation1();
            this.RequiredOperations1();
            this.BaseOperation2();
            this.Hook1();
            this.RequiredOperation2();
            this.BaseOperation3();
            this.Hook2();
        }

        // These operations already have implementations.
        protected void BaseOperation1()
        {
            Console.WriteLine("AbstractClass says: I am doing the bulk of the work");
        }

        protected void BaseOperation2()
        {
            Console.WriteLine("AbstractClass says: But I let subclasses override some operations");
        }

        protected void BaseOperation3()
        {
            Console.WriteLine("AbstractClass says: But I am doing the bulk of the work anyway");
        }
        
        // These operations have to be implemented in subclasses.
        protected abstract void RequiredOperations1();

        protected abstract void RequiredOperation2();
        
        // These are "hooks." Subclasses may override them, but it's not
        // mandatory since the hooks already have default (but empty)
        // implementation. Hooks provide additional extension points in some
        // crucial places of the algorithm.
        protected virtual void Hook1() { }

        protected virtual void Hook2() { }
    }

    // Concrete classes have to implement all abstract operations of the base
    // class. They can also override some operations with a default
    // implementation.
    class ConcreteClass1 : AbstractClass
    {
        protected override void RequiredOperations1()
        {
            Console.WriteLine("ConcreteClass1 says: Implemented Operation1");
        }

        protected override void RequiredOperation2()
        {
            Console.WriteLine("ConcreteClass1 says: Implemented Operation2");
        }
    }

    // Usually, concrete classes override only a fraction of base class'
    // operations.
    class ConcreteClass2 : AbstractClass
    {
        protected override void RequiredOperations1()
        {
            Console.WriteLine("ConcreteClass2 says: Implemented Operation1");
        }

        protected override void RequiredOperation2()
        {
            Console.WriteLine("ConcreteClass2 says: Implemented Operation2");
        }

        protected override void Hook1()
        {
            Console.WriteLine("ConcreteClass2 says: Overridden Hook1");
        }
    }

    class Client
    {
        // The client code calls the template method to execute the algorithm.
        // Client code does not have to know the concrete class of an object it
        // works with, as long as it works with objects through the interface of
        // their base class.
        public static void ClientCode(AbstractClass abstractClass)
        {
            // ...
            abstractClass.TemplateMethod();
            // ...
        }
    }

    class Program
    {
        static void Main(string\[\] args)
        {
            Console.WriteLine("Same client code can work with different subclasses:");

            Client.ClientCode(new ConcreteClass1());

            Console.Write("\\n");
            
            Console.WriteLine("Same client code can work with different subclasses:");
            Client.ClientCode(new ConcreteClass2());
        }
    }
}

#### [](#example-0--Output-txt)**Output.txt:** Execution result

Same client code can work with different subclasses:
AbstractClass says: I am doing the bulk of the work
ConcreteClass1 says: Implemented Operation1
AbstractClass says: But I let subclasses override some operations
ConcreteClass1 says: Implemented Operation2
AbstractClass says: But I am doing the bulk of the work anyway

Same client code can work with different subclasses:
AbstractClass says: I am doing the bulk of the work
ConcreteClass2 says: Implemented Operation1
AbstractClass says: But I let subclasses override some operations
ConcreteClass2 says: Overridden Hook1
ConcreteClass2 says: Implemented Operation2
AbstractClass says: But I am doing the bulk of the work anyway

#### Read next

[Visitor in C#](/design-patterns/visitor/csharp/example) 

#### Return

 [Strategy in C#](/design-patterns/strategy/csharp/example)

## **Template Method** in Other Languages

[![Template Method in C++ - Icon representing C++ language.](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/template-method/cpp/example "Template Method in C++") [![Template Method in Go - Icon representing Go language.](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/template-method/go/example "Template Method in Go") [![Template Method in Java - Icon representing Java language.](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/template-method/java/example "Template Method in Java") [![Template Method in PHP - Icon representing PHP language.](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/template-method/php/example "Template Method in PHP") [![Template Method in Python - Icon representing Python language.](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/template-method/python/example "Template Method in Python") [![Template Method in Ruby - Icon representing Ruby language.](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/template-method/ruby/example "Template Method in Ruby") [![Template Method in Rust - Icon representing Rust language.](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/template-method/rust/example "Template Method in Rust") [![Template Method in Swift - Icon representing Swift language.](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/template-method/swift/example "Template Method in Swift") [![Template Method in TypeScript - Icon representing TypeScript language.](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/template-method/typescript/example "Template Method in TypeScript")

[![](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)
![Banner image showing an Integrated Development Environment (IDE) with code, suggesting practical examples and an eBook.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

![Image showing various development elements like a monitor displaying code and charts, a mobile phone with app interfaces, tools like a wrench and a pen, and abstract data flow lines, all on a dark background. It represents software development and design.](image_placeholder_for_analysis)