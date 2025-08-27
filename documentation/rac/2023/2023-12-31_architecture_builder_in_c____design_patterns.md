```yaml
---
title: Builder in C# / Design Patterns
source: https://refactoring.guru/design-patterns/builder/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T18:00:23.170Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, creational-patterns, builder-pattern, csharp, object-oriented-programming, software-design, code-example, pattern-implementation]
key_concepts: [builder-pattern, creational-design-patterns, director, concrete-builder, product, interface, object-construction, method-chaining]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an in-depth explanation of the Builder design pattern, a creational pattern used for constructing complex objects step by step. It emphasizes the pattern's flexibility, allowing the creation of diverse products without requiring a common interface. The content includes a comprehensive conceptual example implemented in C#, detailing the roles of the Builder interface, Concrete Builder, Product, and the optional Director class. It also offers guidance on identifying the Builder pattern and highlights its utility for objects with extensive configuration options.
---
```

# Builder in C# / Design Patterns

# **Builder** in C#

![Builder Pattern Icon](images/patterns/cards/builder-mini.png "An icon representing the Builder pattern, showing a construction hard hat next to a house being assembled from three distinct parts, symbolizing step-by-step construction.")

**Builder** is a creational design pattern, which allows constructing complex objects step by step.

Unlike other creational patterns, Builder doesn’t require products to have a common interface. That makes it possible to produce different products using the same construction process.

The Builder pattern is a well-known pattern in C# world. It’s especially useful when you need to create an object with lots of possible configuration options.

**Identification:** The Builder pattern can be recognized in a class, which has a single creation method and several methods to configure the resulting object. Builder methods often support chaining (for example, `someBuilder.setValueA(1).setValueB(2).create()`).

## Conceptual Example

This example illustrates the structure of the **Builder** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

### **Program.cs:** Conceptual example

```csharp
using System;
using System.Collections.Generic;

namespace RefactoringGuru.DesignPatterns.Builder.Conceptual
{
    // The Builder interface specifies methods for creating the different parts
    // of the Product objects.
    public interface IBuilder
    {
        void BuildPartA();
		
        void BuildPartB();
		
        void BuildPartC();
    }
    
    // The Concrete Builder classes follow the Builder interface and provide
    // specific implementations of the building steps. Your program may have
    // several variations of Builders, implemented differently.
    public class ConcreteBuilder : IBuilder
    {
        private Product _product = new Product();
        
        // A fresh builder instance should contain a blank product object, which
        // is used in further assembly.
        public ConcreteBuilder()
        {
            this.Reset();
        }
        
        public void Reset()
        {
            this._product = new Product();
        }
		
        // All production steps work with the same product instance.
        public void BuildPartA()
        {
            this._product.Add("PartA1");
        }
		
        public void BuildPartB()
        {
            this._product.Add("PartB1");
        }
		
        public void BuildPartC()
        {
            this._product.Add("PartC1");
        }
		
        // Concrete Builders are supposed to provide their own methods for
        // retrieving results. That's because various types of builders may
        // create entirely different products that don't follow the same
        // interface. Therefore, such methods cannot be declared in the base
        // Builder interface (at least in a statically typed programming
        // language).
        //
        // Usually, after returning the end result to the client, a builder
        // instance is expected to be ready to start producing another product.
        // That's why it's a usual practice to call the reset method at the end
        // of the `GetProduct` method body. However, this behavior is not
        // mandatory, and you can make your builders wait for an explicit reset
        // call from the client code before disposing of the previous result.
        public Product GetProduct()
        {
            Product result = this._product;

            this.Reset();

            return result;
        }
    }
    
    // It makes sense to use the Builder pattern only when your products are
    // quite complex and require extensive configuration.
    //
    // Unlike in other creational patterns, different concrete builders can
    // produce unrelated products. In other words, results of various builders
    // may not always follow the same interface.
    public class Product
    {
        private List<object> _parts = new List<object>();
		
        public void Add(string part)
        {
            this._parts.Add(part);
        }
		
        public string ListParts()
        {
            string str = string.Empty;

            for (int i = 0; i < this._parts.Count; i++)
            {
                str += this._parts[i] + ", ";
            }

            str = str.Remove(str.Length - 2); // removing last ",c"

            return "Product parts: " + str + "\n";
        }
    }
    
    // The Director is only responsible for executing the building steps in a
    // particular sequence. It is helpful when producing products according to a
    // specific order or configuration. Strictly speaking, the Director class is
    // optional, since the client can control builders directly.
    public class Director
    {
        private IBuilder _builder;
        
        public IBuilder Builder
        {
            set { _builder = value; } 
        }
        
        // The Director can construct several product variations using the same
        // building steps.
        public void BuildMinimalViableProduct()
        {
            this._builder.BuildPartA();
        }
		
        public void BuildFullFeaturedProduct()
        {
            this._builder.BuildPartA();
            this._builder.BuildPartB();
            this._builder.BuildPartC();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // The client code creates a builder object, passes it to the
            // director and then initiates the construction process. The end
            // result is retrieved from the builder object.
            var director = new Director();
            var builder = new ConcreteBuilder();
            director.Builder = builder;
            
            Console.WriteLine("Standard basic product:");
            director.BuildMinimalViableProduct();
            Console.WriteLine(builder.GetProduct().ListParts());

            Console.WriteLine("Standard full featured product:");
            director.BuildFullFeaturedProduct();
            Console.WriteLine(builder.GetProduct().ListParts());

            // Remember, the Builder pattern can be used without a Director
            // class.
            Console.WriteLine("Custom product:");
            builder.BuildPartA();
            builder.BuildPartC();
            Console.Write(builder.GetProduct().ListParts());
        }
    }
}
```

### **Output.txt:** Execution result

```
Standard basic product:
Product parts: PartA1

Standard full featured product:
Product parts: PartA1, PartB1, PartC1

Custom product:
Product parts: PartA1, PartC1
```

### **Builder** in Other Languages

This pattern can also be implemented in other programming languages:

*   [![Builder in C++ Icon](/images/patterns/icons/cpp.svg "Icon representing C++") Builder in C++
*   [![Builder in Go Icon](/images/patterns/icons/go.svg "Icon representing Go") Builder in Go
*   [![Builder in Java Icon](/images/patterns/icons/java.svg "Icon representing Java") Builder in Java
*   [![Builder in PHP Icon](/images/patterns/icons/php.svg "Icon representing PHP") Builder in PHP
*   [![Builder in Python Icon](/images/patterns/icons/python.svg "Icon representing Python") Builder in Python
*   [![Builder in Ruby Icon](/images/patterns/icons/ruby.svg "Icon representing Ruby") Builder in Ruby
*   [![Builder in Rust Icon](/images/patterns/icons/rust.svg "Icon representing Rust") Builder in Rust
*   [![Builder in Swift Icon](/images/patterns/icons/swift.svg "Icon representing Swift") Builder in Swift
*   [![Builder in TypeScript Icon](/images/patterns/icons/typescript.svg "Icon representing TypeScript") Builder in TypeScript

![Examples in IDE](images/patterns/banners/examples-ide.png "An abstract illustration depicting various digital interfaces and tools, including a tablet displaying code, a smartphone, charts, and development tools, suggesting a comprehensive development environment.")