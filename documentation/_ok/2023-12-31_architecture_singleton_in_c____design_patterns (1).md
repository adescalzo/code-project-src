```yaml
---
title: Singleton in C# / Design Patterns
source: https://refactoring.guru/design-patterns/singleton/csharp/example#example-1
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T17:57:20.331Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#]
tags: [design-patterns, singleton, creational-patterns, csharp, multithreading, concurrency, software-design, object-oriented-programming]
key_concepts: [singleton-pattern, creational-design-pattern, thread-safety, lazy-initialization, double-checked-locking, antipattern, static-methods, global-state]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains the Singleton design pattern in C#, a creational pattern ensuring only one instance of a class exists and provides a global access point to it. It discusses the pattern's pros and cons, noting its similarity to global variables and potential issues with modularity and unit testing. The article presents two implementations: a basic, "naïve" version and a "thread-safe" version using double-checked locking to prevent multiple instances in concurrent environments. Code examples in C# are provided for both implementations, demonstrating their behavior and addressing concurrency challenges.
---
```

# Singleton in C# / Design Patterns

![Singleton](/images/patterns/cards/singleton-mini.png?id=914e1565dfdf15f240e766163bd303ec)

# **Singleton** in C#

**Singleton** is a creational design pattern, which ensures that only one object of its kind exists and provides a single point of access to it for any other code.

Singleton has almost the same pros and cons as global variables. Although they’re super-handy, they break the modularity of your code.

You can’t just use a class that depends on a Singleton in some other context, without carrying over the Singleton to the other context. Most of the time, this limitation comes up during the creation of unit tests.

[Learn more about Singleton](/design-patterns/singleton)

**Complexity:**
**Popularity:**
**Usage examples:** A lot of developers consider the Singleton pattern an antipattern. That’s why its usage is on the decline in C# code.

**Identification:** Singleton can be recognized by a static creation method, which returns the same cached object.

## Naïve Singleton

It’s pretty easy to implement a sloppy Singleton. You just need to hide the constructor and implement a static creation method.

The same class behaves incorrectly in a multithreaded environment. Multiple threads can call the creation method simultaneously and get several instances of Singleton class.

#### Program.cs: Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.Singleton.Conceptual.NonThreadSafe
{
    // The Singleton class defines the `GetInstance` method that serves as an
    // alternative to constructor and lets clients access the same instance of
    // this class over and over.

    // EN : The Singleton should always be a 'sealed' class to prevent class
    // inheritance through external classes and also through nested classes.
    public sealed class Singleton
    {
        // The Singleton's constructor should always be private to prevent
        // direct construction calls with the `new` operator.
        private Singleton() { }

        // The Singleton's instance is stored in a static field. There there are
        // multiple ways to initialize this field, all of them have various pros
        // and cons. In this example we'll show the simplest of these ways,
        // which, however, doesn't work really well in multithreaded program.
        private static Singleton _instance;

        // This is the static method that controls the access to the singleton
        // instance. On the first run, it creates a singleton object and places
        // it into the static field. On subsequent runs, it returns the client
        // existing object stored in the static field.
        public static Singleton GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Singleton();
            }
            return _instance;
        }

        // Finally, any singleton should define some business logic, which can
        // be executed on its instance.
        public void someBusinessLogic()
        {
            // ...
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The client code.
            Singleton s1 = Singleton.GetInstance();
            Singleton s2 = Singleton.GetInstance();

            if (s1 == s2)
            {
                Console.WriteLine("Singleton works, both variables contain the same instance.");
            }
            else
            {
                Console.WriteLine("Singleton failed, variables contain different instances.");
            }
        }
    }
}
```

#### Output.txt: Execution result

```text
Singleton works, both variables contain the same instance.
```

## Thread-safe Singleton

To fix the problem, you have to synchronize threads during the first creation of the Singleton object.

#### Program.cs: Conceptual example

```csharp
using System;
using System.Threading;

namespace Singleton
{
    // This Singleton implementation is called "double check lock". It is safe
    // in multithreaded environment and provides lazy initialization for the
    // Singleton object.
    class Singleton
    {
        private Singleton() { }

        private static Singleton _instance;

        // We now have a lock object that will be used to synchronize threads
        // during first access to the Singleton.
        private static readonly object _lock = new object();

        public static Singleton GetInstance(string value)
        {
            // This conditional is needed to prevent threads stumbling over the
            // lock once the instance is ready.
            if (_instance == null)
            {
                // Now, imagine that the program has just been launched. Since
                // there's no Singleton instance yet, multiple threads can
                // simultaneously pass the previous conditional and reach this
                // point almost at the same time. The first of them will acquire
                // lock and will proceed further, while the rest will wait here.
                lock (_lock)
                {
                    // The first thread to acquire the lock, reaches this
                    // conditional, goes inside and creates the Singleton
                    // instance. Once it leaves the lock block, a thread that
                    // might have been waiting for the lock release may then
                    // enter this section. But since the Singleton field is
                    // already initialized, the thread won't create a new
                    // object.
                    if (_instance == null)
                    {
                        _instance = new Singleton();
                        _instance.Value = value;
                    }
                }
            }
            return _instance;
        }

        // We'll use this property to prove that our Singleton really works.
        public string Value { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The client code.
            
            Console.WriteLine(
                "{0}\n{1}\n\n{2}\n",
                "If you see the same value, then singleton was reused (yay!)",
                "If you see different values, then 2 singletons were created (booo!!)",
                "RESULT:"
            );
            
            Thread process1 = new Thread(() =>
            {
                TestSingleton("FOO");
            });
            Thread process2 = new Thread(() =>
            {
                TestSingleton("BAR");
            });
            
            process1.Start();
            process2.Start();
            
            process1.Join();
            process2.Join();
        }
        
        public static void TestSingleton(string value)
        {
            Singleton singleton = Singleton.GetInstance(value);
            Console.WriteLine(singleton.Value);
        } 
    }
}
```

#### Output.txt: Execution result

```text
FOO
FOO
```

## Want more?

There are even more special flavors of the Singleton pattern in C#. Take a look at this article to find out more:

[C# in Depth: Implementing Singleton](https://refactoring.guru/csharp-singleton)

#### Read next

[Adapter in C#](/design-patterns/adapter/csharp/example)

#### Return

[Prototype in C#](/design-patterns/prototype/csharp/example)

## **Singleton** in Other Languages

![Singleton in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Singleton in C++") ![Singleton in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Singleton in Go") ![Singleton in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Singleton in Java") ![Singleton in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Singleton in PHP") ![Singleton in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Singleton in Python") ![Singleton in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Singleton in Ruby") ![Singleton in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Singleton in Rust") ![Singleton in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Singleton in Swift") ![Singleton in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Singleton in TypeScript")

![Examples IDE](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

### Image Analysis

*   **Image: `singleton-mini.png`**
    *   **Description:** A small icon depicting a three-tiered podium, with the number "1" prominently displayed on all three steps, symbolizing the concept of a single, unique instance.
*   **Image: `1.png` (from external analysis)**
    *   **Description:** A small icon depicting a three-tiered podium, with the number "1" prominently displayed on all three steps, symbolizing the concept of a single, unique instance.
*   **Image: `2.png` (from external analysis)**
    *   **Description:** An abstract illustration representing software development or a complex system. It features a central desktop monitor displaying various UI elements and code snippets, flanked by a tablet and a mobile phone. Surrounding these devices are various tools and abstract data representations, suggesting a comprehensive development environment or interconnected digital services.