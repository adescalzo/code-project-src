```yaml
---
title: Composite in C# / Design Patterns
source: https://refactoring.guru/design-patterns/composite/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T17:58:07.936Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, composite-pattern, csharp, structural-patterns, object-oriented-programming, tree-structure, recursion]
key_concepts: [composite-design-pattern, structural-patterns, object-composition, tree-structures, recursion, component, leaf, client]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Composite design pattern is a structural pattern that enables the composition of objects into tree structures, allowing clients to treat individual objects and compositions uniformly. It is particularly useful for building hierarchies where operations can be applied recursively across the entire structure, summing up results. The article provides a conceptual example in C#, illustrating the roles of the abstract Component, concrete Leaf, and Composite classes. This pattern simplifies client code by abstracting the complexities of managing individual versus composite objects within a hierarchy.]
---
```

# Composite in C# / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Composite](/design-patterns/composite) / [C#](/design-patterns/csharp)

![Diagram illustrating the Composite design pattern, showing a tree structure with root, branch, and leaf nodes, where red nodes represent composite objects and white nodes represent leaf objects.](/images/patterns/cards/composite-mini.png?id=a369d98d18b417f255d04568fd0131b8)

# **Composite** in C#

**Composite** is a structural design pattern that lets you compose objects into tree structures and then work with these structures as if they were individual objects.

Composite became a pretty popular solution for the most problems that require building a tree structure. Composite’s great feature is the ability to run methods recursively over the whole tree structure and sum up the results.

[Learn more about Composite](/design-patterns/composite)

**Complexity:** Medium

**Popularity:** High

**Usage examples:** The Composite pattern is pretty common in C# code. It’s often used to represent hierarchies of user interface components or the code that works with graphs.

**Identification:** If you have an object tree, and each object of a tree is a part of the same class hierarchy, this is most likely a composite. If methods of these classes delegate the work to child objects of the tree and do it via the base class/interface of the hierarchy, this is definitely a composite.

## Conceptual Example

This example illustrates the structure of the **Composite** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### **Program.cs:** Conceptual example

```csharp
using System;
using System.Collections.Generic;

namespace RefactoringGuru.DesignPatterns.Composite.Conceptual
{
    // The base Component class declares common operations for both simple and
    // complex objects of a composition.
    abstract class Component
    {
        public Component() { }

        // The base Component may implement some default behavior or leave it to
        // concrete classes (by declaring the method containing the behavior as
        // "abstract").
        public abstract string Operation();

        // In some cases, it would be beneficial to define the child-management
        // operations right in the base Component class. This way, you won't
        // need to expose any concrete component classes to the client code,
        // even during the object tree assembly. The downside is that these
        // methods will be empty for the leaf-level components.
        public virtual void Add(Component component)
        {
            throw new NotImplementedException();
        }

        public virtual void Remove(Component component)
        {
            throw new NotImplementedException();
        }

        // You can provide a method that lets the client code figure out whether
        // a component can bear children.
        public virtual bool IsComposite()
        {
            return true;
        }
    }

    // The Leaf class represents the end objects of a composition. A leaf can't
    // have any children.
    //
    // Usually, it's the Leaf objects that do the actual work, whereas Composite
    // objects only delegate to their sub-components.
    class Leaf : Component
    {
        public override string Operation()
        {
            return "Leaf";
        }

        public override bool IsComposite()
        {
            return false;
        }
    }

    // The Composite class represents the complex components that may have
    // children. Usually, the Composite objects delegate the actual work to
    // their children and then "sum-up" the result.
    class Composite : Component
    {
        protected List<Component> _children = new List<Component>();
        
        public override void Add(Component component)
        {
            this._children.Add(component);
        }

        public override void Remove(Component component)
        {
            this._children.Remove(component);
        }

        // The Composite executes its primary logic in a particular way. It
        // traverses recursively through all its children, collecting and
        // summing their results. Since the composite's children pass these
        // calls to their children and so forth, the whole object tree is
        // traversed as a result.
        public override string Operation()
        {
            int i = 0;
            string result = "Branch(";

            foreach (Component component in this._children)
            {
                result += component.Operation();
                if (i != this._children.Count - 1)
                {
                    result += "+";
                }
                i++;
            }
            
            return result + ")";
        }
    }

    class Client
    {
        // The client code works with all of the components via the base
        // interface.
        public void ClientCode(Component leaf)
        {
            Console.WriteLine($"RESULT: {leaf.Operation()}\n");
        }

        // Thanks to the fact that the child-management operations are declared
        // in the base Component class, the client code can work with any
        // component, simple or complex, without depending on their concrete
        // classes.
        public void ClientCode2(Component component1, Component component2)
        {
            if (component1.IsComposite())
            {
                component1.Add(component2);
            }
            
            Console.WriteLine($"RESULT: {component1.Operation()}");
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            Client client = new Client();

            // This way the client code can support the simple leaf
            // components...
            Leaf leaf = new Leaf();
            Console.WriteLine("Client: I get a simple component:");
            client.ClientCode(leaf);

            // ...as well as the complex composites.
            Composite tree = new Composite();
            Composite branch1 = new Composite();
            branch1.Add(new Leaf());
            branch1.Add(new Leaf());
            Composite branch2 = new Composite();
            branch2.Add(new Leaf());
            tree.Add(branch1);
            tree.Add(branch2);
            Console.WriteLine("Client: Now I've got a composite tree:");
            client.ClientCode(tree);

            Console.Write("Client: I don't need to check the components classes even when managing the tree:\n");
            client.ClientCode2(tree, leaf);
        }
    }
}
```

#### **Output.txt:** Execution result

```
Client: I get a simple component:
RESULT: Leaf

Client: Now I've got a composite tree:
RESULT: Branch(Branch(Leaf+Leaf)+Branch(Leaf))

Client: I don't need to check the components classes even when managing the tree:
RESULT: Branch(Branch(Leaf+Leaf)+Branch(Leaf)+Leaf)
```

#### Read next

[Decorator in C#](/design-patterns/decorator/csharp/example) 

#### Return

 [Bridge in C#](/design-patterns/bridge/csharp/example)

## **Composite** in Other Languages

![Icon for C++ language](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858) [Composite in C++](/design-patterns/composite/cpp/example "Composite in C++") 
![Icon for Go language](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca) [Composite in Go](/design-patterns/composite/go/example "Composite in Go") 
![Icon for Java language](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e) [Composite in Java](/design-patterns/composite/java/example "Composite in Java") 
![Icon for PHP language](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618) [Composite in PHP](/design-patterns/composite/php/example "Composite in PHP") 
![Icon for Python language](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f) [Composite in Python](/design-patterns/composite/python/example "Composite in Python") 
![Icon for Ruby language](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb) [Composite in Ruby](/design-patterns/composite/ruby/example "Composite in Ruby") 
![Icon for Rust language](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87) [Composite in Rust](/design-patterns/composite/rust/example "Composite in Rust") 
![Icon for Swift language](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d) [Composite in Swift](/design-patterns/composite/swift/example "Composite in Swift") 
![Icon for TypeScript language](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7) [Composite in TypeScript](/design-patterns/composite/typescript/example "Composite in TypeScript")

![Banner advertising an e-book titled 'Dive Into Design Patterns', showing various development tools and code snippets on screens, implying practical examples.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)