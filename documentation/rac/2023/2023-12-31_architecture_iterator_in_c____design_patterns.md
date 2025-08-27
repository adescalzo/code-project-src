```yaml
---
title: Iterator in C# / Design Patterns
source: https://refactoring.guru/design-patterns/iterator/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T15:09:48.812Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, iterator-pattern, behavioral-patterns, csharp, collections, data-structures, traversal, software-design]
key_concepts: [iterator-design-pattern, behavioral-design-pattern, sequential-traversal, ienumerator-interface, ienumerable-interface, abstraction, decoupling, collection-traversal]
code_examples: false
difficulty_level: intermediate
summary: |
  The article introduces the Iterator behavioral design pattern, explaining its purpose of allowing sequential traversal through complex data structures without exposing their internal details. It highlights how the pattern enables clients to iterate over different collections uniformly using a single iterator interface. A detailed conceptual example in C# is provided, illustrating the roles of `Iterator` and `IteratorAggregate` classes, including concrete implementations for alphabetical and reverse order traversal. The example demonstrates how to implement custom iterators and aggregates using C#'s `IEnumerator` and `IEnumerable` interfaces, showcasing both straight and reverse iteration.
---
```

# Iterator in C# / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Iterator](/design-patterns/iterator) / [C#](/design-patterns/csharp)

![Iterator](/images/patterns/cards/iterator-mini.png?id=76c28bb48f997b36965983dd2b41f02e)
Image Description: A red outline icon depicting a person with a backpack and walking stick, traversing a series of linked data blocks, symbolizing the sequential traversal of a collection.

# **Iterator** in C#

**Iterator** is a behavioral design pattern that allows sequential traversal through a complex data structure without exposing its internal details.

Thanks to the Iterator, clients can go over elements of different collections in a similar fashion using a single iterator interface.

[Learn more about Iterator](/design-patterns/iterator)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The pattern is very common in C# code. Many frameworks and libraries use it to provide a standard way for traversing their collections.

**Identification:** Iterator is easy to recognize by the navigation methods (such as `next`, `previous` and others). Client code that uses iterators might not have direct access to the collection being traversed.

## Conceptual Example

This example illustrates the structure of the **Iterator** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### [](#example-0--Program-cs)**Program.cs:** Conceptual example

using System;
using System.Collections;
using System.Collections.Generic;

namespace RefactoringGuru.DesignPatterns.Iterator.Conceptual
{
    abstract class Iterator : IEnumerator
    {
        object IEnumerator.Current => Current();

        // Returns the key of the current element
        public abstract int Key();
		
        // Returns the current element
        public abstract object Current();
		
        // Move forward to next element
        public abstract bool MoveNext();
		
        // Rewinds the Iterator to the first element
        public abstract void Reset();
    }

    abstract class IteratorAggregate : IEnumerable
    {
        // Returns an Iterator or another IteratorAggregate for the implementing
        // object.
        public abstract IEnumerator GetEnumerator();
    }

    // Concrete Iterators implement various traversal algorithms. These classes
    // store the current traversal position at all times.
    class AlphabeticalOrderIterator : Iterator
    {
        private WordsCollection _collection;
		
        // Stores the current traversal position. An iterator may have a lot of
        // other fields for storing iteration state, especially when it is
        // supposed to work with a particular kind of collection.
        private int _position = -1;
		
        private bool _reverse = false;

        public AlphabeticalOrderIterator(WordsCollection collection, bool reverse = false)
        {
            this._collection = collection;
            this._reverse = reverse;

            if (reverse)
            {
                this._position = collection.getItems().Count;
            }
        }
		
        public override object Current()
        {
            return this._collection.getItems()[_position];
        }

        public override int Key()
        {
            return this._position;
        }
		
        public override bool MoveNext()
        {
            int updatedPosition = this._position + (this._reverse ? -1 : 1);

            if (updatedPosition >= 0 && updatedPosition < this._collection.getItems().Count)
            {
                this._position = updatedPosition;
                return true;
            }
            else
            {
                return false;
            }
        }
		
        public override void Reset()
        {
            this._position = this._reverse ? this._collection.getItems().Count - 1 : 0;
        }
    }

    // Concrete Collections provide one or several methods for retrieving fresh
    // iterator instances, compatible with the collection class.
    class WordsCollection : IteratorAggregate
    {
        List<string> _collection = new List<string>();
		
        bool _direction = false;
        
        public void ReverseDirection()
        {
            _direction = !_direction;
        }
		
        public List<string> getItems()
        {
            return _collection;
        }
		
        public void AddItem(string item)
        {
            this._collection.Add(item);
        }
		
        public override IEnumerator GetEnumerator()
        {
            return new AlphabeticalOrderIterator(this, _direction);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The client code may or may not know about the Concrete Iterator
            // or Collection classes, depending on the level of indirection you
            // want to keep in your program.
            var collection = new WordsCollection();
            collection.AddItem("First");
            collection.AddItem("Second");
            collection.AddItem("Third");

            Console.WriteLine("Straight traversal:");

            foreach (var element in collection)
            {
                Console.WriteLine(element);
            }

            Console.WriteLine("\nReverse traversal:");

            collection.ReverseDirection();

            foreach (var element in collection)
            {
                Console.WriteLine(element);
            }
        }
    }
}

#### [](#example-0--Output-txt)**Output.txt:** Execution result

Straight traversal:
First
Second
Third

Reverse traversal:
Third
Second
First

#### Read next

[Mediator in C#](/design-patterns/mediator/csharp/example) 

#### Return

 [Command in C#](/design-patterns/command/csharp/example)

## **Iterator** in Other Languages

[![Iterator in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/iterator/cpp/example "Iterator in C++") [![Iterator in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/iterator/go/example "Iterator in Go") [![Iterator in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/iterator/java/example "Iterator in Java") [![Iterator in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/iterator/php/example "Iterator in PHP") [![Iterator in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/iterator/python/example "Iterator in Python") [![Iterator in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/iterator/ruby/example "Iterator in Ruby") [![Iterator in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/iterator/rust/example "Iterator in Rust") [![Iterator in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/iterator/swift/example "Iterator in Swift") [![Iterator in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/iterator/typescript/example "Iterator in TypeScript")

[![](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)
Image Description: A colorful illustration showing various digital devices (desktop monitor, tablet, smartphone) displaying code snippets and graphical user interfaces, surrounded by development tools like a hammer, wrench, and pen, representing a software development environment or technical content.

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)