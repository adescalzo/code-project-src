```yaml
---
title: Decorator in C# / Design Patterns
source: https://refactoring.guru/design-patterns/decorator/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T17:58:07.853Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [C#]
tags: [design-pattern, structural-pattern, csharp, object-oriented-programming, software-design, wrapper, composition, behavioral-extension]
key_concepts: [decorator-pattern, structural-pattern, object-composition, wrapper-objects, dynamic-behavior-extension, delegation, interface-design]
code_examples: false
difficulty_level: intermediate
summary: |
  The Decorator pattern is a structural design pattern that enables adding new behaviors to objects dynamically. It achieves this by placing objects inside special wrapper objects, known as decorators, which implement the same interface as the wrapped components. This allows for stacking multiple layers of functionality, where each decorator adds its own behavior before or after delegating to the wrapped object. The article provides a clear conceptual example in C#, illustrating the roles of components, concrete components, and various concrete decorators. This pattern is particularly useful for extending functionality without modifying existing class structures, promoting flexibility and adherence to the Open/Closed Principle.
---
```

# Decorator in C# / Design Patterns

![A small icon representing the Decorator pattern, showing a paint roller and a Russian nesting doll (Matryoshka doll), symbolizing wrapping and adding layers.](/images/patterns/cards/decorator-mini.png?id=d30458908e315af195cb183bc52dbef9)

# **Decorator** in C#

**Decorator** is a structural pattern that allows adding new behaviors to objects dynamically by placing them inside special wrapper objects, called _decorators_.

Using decorators you can wrap objects countless number of times since both target objects and decorators follow the same interface. The resulting object will get a stacking behavior of all wrappers.

[Learn more about Decorator](/design-patterns/decorator)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Usage examples:** The Decorator is pretty standard in C# code, especially in code related to streams.

**Identification:** Decorator can be recognized by creation methods or constructors that accept objects of the same class or interface as a current class.

## Conceptual Example

This example illustrates the structure of the **Decorator** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### Program.cs: Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.Composite.Conceptual
{
    // The base Component interface defines operations that can be altered by
    // decorators.
    public abstract class Component
    {
        public abstract string Operation();
    }

    // Concrete Components provide default implementations of the operations.
    // There might be several variations of these classes.
    class ConcreteComponent : Component
    {
        public override string Operation()
        {
            return "ConcreteComponent";
        }
    }

    // The base Decorator class follows the same interface as the other
    // components. The primary purpose of this class is to define the wrapping
    // interface for all concrete decorators. The default implementation of the
    // wrapping code might include a field for storing a wrapped component and
    // the means to initialize it.
    abstract class Decorator : Component
    {
        protected Component _component;

        public Decorator(Component component)
        {
            this._component = component;
        }

        public void SetComponent(Component component)
        {
            this._component = component;
        }

        // The Decorator delegates all work to the wrapped component.
        public override string Operation()
        {
            if (this._component != null)
            {
                return this._component.Operation();
            }
            else
            {
                return string.Empty;
            }
        }
    }

    // Concrete Decorators call the wrapped object and alter its result in some
    // way.
    class ConcreteDecoratorA : Decorator
    {
        public ConcreteDecoratorA(Component comp) : base(comp)
        {
        }

        // Decorators may call parent implementation of the operation, instead
        // of calling the wrapped object directly. This approach simplifies
        // extension of decorator classes.
        public override string Operation()
        {
            return $"ConcreteDecoratorA({base.Operation()})";
        }
    }

    // Decorators can execute their behavior either before or after the call to
    // a wrapped object.
    class ConcreteDecoratorB : Decorator
    {
        public ConcreteDecoratorB(Component comp) : base(comp)
        {
        }

        public override string Operation()
        {
            return $"ConcreteDecoratorB({base.Operation()})";
        }
    }
    
    public class Client
    {
        // The client code works with all objects using the Component interface.
        // This way it can stay independent of the concrete classes of
        // components it works with.
        public void ClientCode(Component component)
        {
            Console.WriteLine("RESULT: " + component.Operation());
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();

            var simple = new ConcreteComponent();
            Console.WriteLine("Client: I get a simple component:");
            client.ClientCode(simple);
            Console.WriteLine();

            // ...as well as decorated ones.
            //
            // Note how decorators can wrap not only simple components but the
            // other decorators as well.
            ConcreteDecoratorA decorator1 = new ConcreteDecoratorA(simple);
            ConcreteDecoratorB decorator2 = new ConcreteDecoratorB(decorator1);
            Console.WriteLine("Client: Now I've got a decorated component:");
            client.ClientCode(decorator2);
        }
    }
}
```

#### Output.txt: Execution result

```
Client: I get a simple component:
RESULT: ConcreteComponent

Client: Now I've got a decorated component:
RESULT: ConcreteDecoratorB(ConcreteDecoratorA(ConcreteComponent))
```

#### Read next

[Facade in C#](/design-patterns/facade/csharp/example) 

#### Return

 [Composite in C#](/design-patterns/composite/csharp/example)

## **Decorator** in Other Languages

[![Decorator in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/decorator/cpp/example "Decorator in C++") [![Decorator in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/decorator/go/example "Decorator in Go") [![Decorator in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/decorator/java/example "Decorator in Java") [![Decorator in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/decorator/php/example "Decorator in PHP") [![Decorator in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/decorator/python/example "Decorator in Python") [![Decorator in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/decorator/ruby/example "Decorator in Ruby") [![Decorator in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/decorator/rust/example "Decorator in Rust") [![Decorator in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/decorator/swift/example "Decorator in Swift") [![Decorator in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/decorator/typescript/example "Decorator in TypeScript")

![An illustration depicting various development tools and screens, including a tablet, smartphone, and desktop, with code snippets and UI elements, suggesting a comprehensive set of examples or an IDE.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)