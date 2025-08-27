```yaml
---
title: State in C# / Design Patterns
source: https://refactoring.guru/design-patterns/state/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:59:53.409Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [C#]
tags: [design-pattern, behavioral-pattern, state-pattern, csharp, object-oriented-programming, oop, software-design, state-machine]
key_concepts: [state-pattern, behavioral-design-pattern, state-machine, object-oriented-design, delegation, context-class, state-class, concrete-state]
code_examples: false
difficulty_level: intermediate
summary: |
  [The State design pattern is a behavioral pattern that enables an object to change its behavior when its internal state changes, making it appear as if the object has changed its class. This pattern extracts state-specific behaviors into separate state classes, and the original object delegates requests to an instance of these state classes. It is commonly used in C# to refactor complex switch-based state machines into a more organized, object-oriented structure. The provided conceptual example illustrates the pattern's structure, including the Context, abstract State, and concrete State implementations, demonstrating how state transitions are managed programmatically.]
---
```

# State in C# / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [State](/design-patterns/state) / [C#](/design-patterns/csharp)

![Conceptual diagram illustrating the State design pattern, showing a context object delegating behavior to different state objects in a cyclical flow.](/images/patterns/cards/state-mini.png?id=f4018837e0641d1dade756b6678fd4ee)

# **State** in C#

**State** is a behavioral design pattern that allows an object to change the behavior when its internal state changes.

The pattern extracts state-related behaviors into separate state classes and forces the original object to delegate the work to an instance of these classes, instead of acting on its own.

[Learn more about State](/design-patterns/state)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The State pattern is commonly used in C# to convert massive `switch`\-base state machines into objects.

**Identification:** State pattern can be recognized by methods that change their behavior depending on the objects’ state, controlled externally.

## Conceptual Example

This example illustrates the structure of the **State** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### [](#example-0--Program-cs)**Program.cs:** Conceptual example

using System;

namespace RefactoringGuru.DesignPatterns.State.Conceptual
{
    // The Context defines the interface of interest to clients. It also
    // maintains a reference to an instance of a State subclass, which
    // represents the current state of the Context.
    class Context
    {
        // A reference to the current state of the Context.
        private State \_state = null;

        public Context(State state)
        {
            this.TransitionTo(state);
        }

        // The Context allows changing the State object at runtime.
        public void TransitionTo(State state)
        {
            Console.WriteLine($"Context: Transition to {state.GetType().Name}.");
            this.\_state = state;
            this.\_state.SetContext(this);
        }

        // The Context delegates part of its behavior to the current State
        // object.
        public void Request1()
        {
            this.\_state.Handle1();
        }

        public void Request2()
        {
            this.\_state.Handle2();
        }
    }
    
    // The base State class declares methods that all Concrete State should
    // implement and also provides a backreference to the Context object,
    // associated with the State. This backreference can be used by States to
    // transition the Context to another State.
    abstract class State
    {
        protected Context \_context;

        public void SetContext(Context context)
        {
            this.\_context = context;
        }

        public abstract void Handle1();

        public abstract void Handle2();
    }

    // Concrete States implement various behaviors, associated with a state of
    // the Context.
    class ConcreteStateA : State
    {
        public override void Handle1()
        {
            Console.WriteLine("ConcreteStateA handles request1.");
            Console.WriteLine("ConcreteStateA wants to change the state of the context.");
            this.\_context.TransitionTo(new ConcreteStateB());
        }

        public override void Handle2()
        {
            Console.WriteLine("ConcreteStateA handles request2.");
        }
    }

    class ConcreteStateB : State
    {
        public override void Handle1()
        {
            Console.Write("ConcreteStateB handles request1.");
        }

        public override void Handle2()
        {
            Console.WriteLine("ConcreteStateB handles request2.");
            Console.WriteLine("ConcreteStateB wants to change the state of the context.");
            this.\_context.TransitionTo(new ConcreteStateA());
        }
    }

    class Program
    {
        static void Main(string\[\] args)
        {
            // The client code.
            var context = new Context(new ConcreteStateA());
            context.Request1();
            context.Request2();
        }
    }
}

#### [](#example-0--Output-txt)**Output.txt:** Execution result

Context: Transition to ConcreteStateA.
ConcreteStateA handles request1.
ConcreteStateA wants to change the state of the context.
Context: Transition to ConcreteStateB.
ConcreteStateB handles request2.
ConcreteStateB wants to change the state of the context.
Context: Transition to ConcreteStateA.

#### Read next

[Strategy in C#](/design-patterns/strategy/csharp/example) 

#### Return

 [Observer in C#](/design-patterns/observer/csharp/example)

## **State** in Other Languages

![Icon for C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858) [![State in C++](/design-patterns/state/cpp/example "State in C++")](/design-patterns/state/cpp/example) ![Icon for Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca) [![State in Go](/design-patterns/state/go/example "State in Go")](/design-patterns/state/go/example) ![Icon for Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e) [![State in Java](/design-patterns/state/java/example "State in Java")](/design-patterns/state/java/example) ![Icon for PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618) [![State in PHP](/design-patterns/state/php/example "State in PHP")](/design-patterns/state/php/example) ![Icon for Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f) [![State in Python](/design-patterns/state/python/example "State in Python")](/design-patterns/state/python/example) ![Icon for Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb) [![State in Ruby](/design-patterns/state/ruby/example "State in Ruby")](/design-patterns/state/ruby/example) ![Icon for Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87) [![State in Rust](/design-patterns/state/rust/example "State in Rust")](/design-patterns/state/rust/example) ![Icon for Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d) [![State in Swift](/design-patterns/state/swift/example "State in Swift")](/design-patterns/state/swift/example) ![Icon for TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7) [![State in TypeScript](/design-patterns/state/typescript/example "State in TypeScript")](/design-patterns/state/typescript/example)

![Banner image promoting an eBook, showing various development tools and screens, representing code examples in an IDE.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)