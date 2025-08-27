```yaml
---
title: "Polymorphism in C#. Polymorphism is a fancy term that often… | by Orkhan Mustafayev | Medium"
source: https://medium.com/@orkhanmustafayev/polymorphism-in-c-24591de32b23
date_published: 2025-03-10T20:45:17.629Z
date_captured: 2025-08-06T17:51:09.190Z
domain: medium.com
author: Orkhan Mustafayev
category: programming
technologies: []
programming_languages: [C#]
tags: [csharp, oop, polymorphism, method-overloading, method-overriding, inheritance, abstraction, object-oriented-programming, programming-concepts]
key_concepts: [polymorphism, method-overloading, method-overriding, virtual-keyword, override-keyword, abstract-keyword, method-hiding, object-oriented-programming]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive introduction to polymorphism, a fundamental concept in object-oriented programming (OOP) in C#. It explains polymorphism as the ability for objects of different classes to be treated as objects of a common type, while retaining their specific behaviors. The two main types, compile-time polymorphism (method overloading) and runtime polymorphism (method overriding), are detailed with clear C# code examples. The article also covers the use of `virtual`, `override`, `abstract`, and `new` keywords, highlighting their roles in achieving different forms of polymorphism. It concludes by emphasizing the benefits of polymorphism for writing extensible, maintainable, and reusable code.]
---
```

# Polymorphism in C#. Polymorphism is a fancy term that often… | by Orkhan Mustafayev | Medium

# Polymorphism in C#

A background image featuring lines of JavaScript code, with the title 'Polymorphism in C#' overlaid in white text.

Polymorphism is a fancy term that often pops up when you start exploring object-oriented programming (OOP) concepts in C#. Although it might seem intimidating at first, the core idea is straightforward: **polymorphism** allows objects of different classes to be treated as though they are objects of the same class, all while still behaving according to the specific implementation in their own class.

In other words, it’s about having multiple “forms” (poly-morph) for the same behavior or function call. This article will walk you through the basics of polymorphism, explain why it’s useful, and provide some short code examples to illustrate how it works in C#.

## What is Polymorphism?

Polymorphism is one of the four fundamental principles of object-oriented programming (alongside **encapsulation**, **inheritance**, and **abstraction**). The concept means “many forms,” and in the context of C#, it gives you the ability to use a single interface (like a method or property) in many different ways depending on the actual object instance.

In C#, there are generally two kinds of polymorphism:

1.  **Compile-time polymorphism (Method Overloading)**
2.  **Runtime polymorphism (Method Overriding)**

Let’s discuss each one in detail.

## 1\. Compile-Time Polymorphism (Method Overloading)

**Compile-time polymorphism** (also called **static polymorphism**) occurs when multiple methods share the same name, but differ at least one in the following:

*   The number of parameters
*   The type of parameters
*   The order of parameters

The decision about which method to call is made at **compile time** based on the arguments passed in the method call.

## Example: Method Overloading

```csharp
public class Calculator  
{  
    // Method 1: Adds two integers  
    public int Add(int x, int y)  
    {  
        return x + y;  
    }  
  
    // Method 2: Adds three integers  
    public int Add(int x, int y, int z)  
    {  
        return x + y + z;  
    }  
  
    // Method 3: Adds two doubles  
    public double Add(double x, double y)  
    {  
        return x + y;  
    }  
}

public class Program  
{  
    public static void Main()  
    {  
        Calculator calc = new Calculator();  
  
        // The compiler decides which 'Add' to call based on argument types and count  
        Console.WriteLine(calc.Add(3, 4));        // Calls Add(int x, int y)  
        Console.WriteLine(calc.Add(3, 4, 5));     // Calls Add(int x, int y, int z)  
        Console.WriteLine(calc.Add(3.5, 4.2));    // Calls Add(double x, double y)  
    }  
}
```

**Key Takeaways:**

*   All methods have the same name, but they differ by their signatures (parameters).
*   The C# compiler chooses which `Add` method to call based on the arguments you pass.
*   This decision is made **before** your program runs (at compile time).

## 2\. Runtime Polymorphism (Method Overriding)

**Runtime polymorphism** (also called **dynamic polymorphism**) is achieved through **method overriding**. It enables you to define a method in a subclass (derived class) that has the **same signature** as a method in the parent class (base class), but with a different implementation.

The word “runtime” refers to the fact that the actual method to call is determined **while the program is running**, based on the **actual object** in use.

## Using `virtual` and `override`

*   In the **base class**, you mark the method you want to override with the `virtual` keyword.
*   In the **derived class**, you override that method using the `override` keyword.

## Example: Method Overriding

```csharp
public class Animal  
{  
    // Mark the method as virtual so it can be overridden in derived classes  
    public virtual void Speak()  
    {  
        Console.WriteLine("The animal makes a sound.");  
    }  
}

public class Dog : Animal  
{  
    // Override the virtual method from the base class  
    public override void Speak()  
    {  
        Console.WriteLine("The dog barks.");  
    }  
}

public class Cat : Animal  
{  
    // Override the virtual method from the base class  
    public override void Speak()  
    {  
        Console.WriteLine("The cat meows.");  
    }  
}

public class Program  
{  
    public static void Main()  
    {  
        // Notice how we have a reference to Animal, but an object of Dog  
        Animal myDog = new Dog();  
        Animal myCat = new Cat();  
          
        // Runtime Polymorphism kicks in  
        myDog.Speak(); // Calls Dog's implementation => "The dog barks."  
        myCat.Speak(); // Calls Cat's implementation => "The cat meows."  
    }  
}
```

**Key Takeaways:**

*   The actual method that gets called depends on the **object’s type** (`Dog`, `Cat`) rather than the **reference type** (`Animal`).
*   This determination occurs at **runtime**.
*   Helps in writing extensible, flexible code, where different subclasses can provide their own specific behaviors.

## The `abstract` Keyword

Sometimes, you have a situation where the base class method really doesn’t have a default or meaningful implementation — it only makes sense for derived classes to define their own. In these cases, you can mark the base class method as `**abstract**`. An abstract method has **no body** and forces derived classes to provide an implementation.

```csharp
public abstract class Shape  
{  
    // No implementation here, derived classes must override it  
    public abstract double GetArea();  
}

public class Circle : Shape  
{  
    private double radius;  
  
    public Circle(double radius)  
    {  
        this.radius = radius;  
    }  
  
    // Must override the abstract method  
    public override double GetArea()  
    {  
        return Math.PI * radius * radius;  
    }  
}

public class Rectangle : Shape  
{  
    private double width;  
    private double height;  
  
    public Rectangle(double width, double height)  
    {  
        this.width = width;  
        this.height = height;  
    }  
  
    // Must override the abstract method  
    public override double GetArea()  
    {  
        return width * height;  
    }  
}

public class Program  
{  
    public static void Main()  
    {  
        Shape circle = new Circle(5);  
        Shape rectangle = new Rectangle(4, 6);  
  
        Console.WriteLine("Circle area: " + circle.GetArea());  
        Console.WriteLine("Rectangle area: " + rectangle.GetArea());  
    }  
}
```

**Why use** `**abstract**` **classes?**  
They provide a template or blueprint for derived classes. If you know every shape must implement `GetArea()`, then marking it as `abstract` in the base class enforces that contract.

## The `new` Keyword (Method Hiding)

In some cases, you might have a method in the derived class with the same signature as a method in the base class, but you **don’t** want to override the base class version (maybe for backward compatibility or other reasons). You can use the `**new**` keyword to _hide_ the base class method. However, this is typically less common than method overriding.

```csharp
public class BaseClass  
{  
    public void ShowMessage()  
    {  
        Console.WriteLine("Message from BaseClass");  
    }  
}

public class DerivedClass : BaseClass  
{  
    public new void ShowMessage()  
    {  
        Console.WriteLine("Message from DerivedClass");  
    }  
}

public class Program  
{  
    public static void Main()  
    {  
        DerivedClass derived = new DerivedClass();  
  
        // Calls the DerivedClass version  
        derived.ShowMessage(); // "Message from DerivedClass"  
  
        // Calls the BaseClass version via a base-class reference  
        BaseClass baseRef = derived;  
        baseRef.ShowMessage(); // "Message from BaseClass"  
    }  
}
```

Because the method in `DerivedClass` is hiding rather than overriding the base class method, a `BaseClass` reference still invokes the original `ShowMessage`.

## Why Do We Need Polymorphism?

1.  **Extensibility and Flexibility**: Polymorphism lets you write code that works for classes you might not even have created yet. For instance, your code can work with a list of `Animal` objects, and each `Animal` type (dog, cat, bird, etc.) can provide its own specialized behavior.
2.  **Cleaner, More Maintainable Code**: By centralizing a concept in a base class and customizing it in derived classes, you keep code organized. You don’t have to write big `if-else` or `switch` statements to figure out the object’s type and perform different actions.
3.  **Reusability**: You can define methods once in the parent class and reuse them across many child classes, adding or changing implementations only where needed.

## Tips for Using Polymorphism Effectively

1.  **Prefer Method Overriding to Method Hiding**: If you want different derived classes to have different behaviors, you usually want to override a virtual or abstract method. Hiding is more niche and can lead to confusion.
2.  **Use Abstract Classes When Appropriate**: If the base class does not have a meaningful default behavior for a method, mark it `abstract`. This enforces a contract so that all subclasses implement the method.
3.  **Avoid Excessive Method Overloading**: Overloading is useful, but too many overloaded methods with slightly different parameters can make code confusing. Keep them clear and consistent.
4.  **Leverage Interfaces**: Although not directly related to polymorphism via inheritance, interfaces allow you to provide different implementations for the same contract (set of methods) across multiple classes. This is another powerful way to achieve polymorphism in C#.

## Wrapping Up

Polymorphism is a cornerstone of object-oriented programming because it **encourages flexible and maintainable code**. In C#, you’ll often see it through **method overloading** (compile-time polymorphism) and **method overriding** (runtime polymorphism). With this knowledge in hand, you can start designing classes that share a common interface but have unique, customized behaviors — making your C# code more elegant, extensible, and easier to manage in the long run.