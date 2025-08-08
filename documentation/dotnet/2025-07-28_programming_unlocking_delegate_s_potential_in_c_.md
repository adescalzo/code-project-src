# Unlocking delegate's potential in C#

```markdown
**Source:** https://blog.elmah.io/unlocking-delegates-potential-in-c/
**Date Captured:** 2025-07-28T16:26:16.148Z
**Domain:** blog.elmah.io
**Author:** Unknown
**Category:** programming
```

---

# Unlocking delegate's potential in C#

[](https://www.facebook.com/sharer.php?u=https://blog.elmah.io/unlocking-delegates-potential-in-c/ "Share to Facebook")[](https://x.com/share?text=Unlocking%20delegate's%20potential%20in%20C%23&url=https://blog.elmah.io/unlocking-delegates-potential-in-c/ "Share to X")[](https://www.linkedin.com/sharing/share-offsite/?url=https://blog.elmah.io/unlocking-delegates-potential-in-c/ "Share to LinkedIn")[](mailto:?subject=Unlocking%20delegate's%20potential%20in%20C%23&body=https://blog.elmah.io/unlocking-delegates-potential-in-c/ "Share to Email")

Written by [Ali Hamza Ansari](/author/ali/), February 11, 2025

## Contents

Show contents

*   [What is Delegates in C#?](#what-is-delegates-in-c)
*   [Example 1: A Printing methods’ Delegate](#example-1-a-printing-methods%E2%80%99-delegate)
*   [Example 2: Menu with conditional method execution](#example-2-menu-with-conditional-method-execution)
*   [Example 3: Delegates with Lambda expression](#example-3-delegates-with-lambda-expression)
*   [Advantages of delegates in C#](#advantages-of-delegates-in-c)
*   [Common Built-in Delegates in C#](#common-built-in-delegates-in-c)
*   [Action](#action)
*   [Func](#func) 
*   [Predicate](#predicate) 
*   [Built-in Delegates Used in LINQ](#built-in-delegates-used-in-linq)
*   [Conclusion](#conclusion)

Working on event-driven applications can be challenging, where you need to write a large number of methods. Remembering those methods and deciding their usage for every requirement is tedious, especially when methods have different signatures. C# brings delegates to cope with this problem. Delegating is a powerful feature introduced solely to ease developers in event-driven programming. Understanding delegates is difficult sometimes. So, I brought a comprehensive post to let you know the delegate's definition, analogy, and magic.

[![Unlocking delegate's potential in C#](https://blog.elmah.io/content/images/2025/02/unlocking-delegates-potential-in-csharp-o-83d5276a7b816653.png)](https://blog.elmah.io/content/images/2025/02/unlocking-delegates-potential-in-csharp-o-83d5276a7b816653.png)

## What is Delegates in C#?

Delegates are function pointers that dynamically encapsulate and invoke one or multiple methods at runtime. They act as a decoupling layer between methods and the caller in a type-safe manner.  All the methods passed as a parameter to a delegate must have the same signature(return type and parameters list). Delegate store references to methods and does not know the implementations of the methods. Its job is to execute the appropriate method upon calling. 

To understand better, consider the analogy of a waiter in a restaurant.  The waiters are the middleman between you (the customer) and the chef(s) in the kitchen. 

*   **The Delegate (Waiter)**: The waiter knows references to all the chefs and what dishes they cook. The waiter doesn’t cook the food themselves. Instead, they take your order and decide on their own which chef should handle it based on the type of dish you ordered.
*   **The Method (Chef)**: The Chef is the executor or performer here. Each chef in the kitchen prepares certain types of dishes, just like a method that performs a specific task.
*   **The Encapsulation**: As a customer, you don’t need to know which chef cooks what dish and don't need to go to the chef yourself to order. The waiter (delegate) abstracts that detail and ensures the correct chef is called.

Let's understand deeper with the help of code 

## Example 1: A Printing methods’ Delegate

Create delegate:

```csharp
public delegate void PrintMessage(string message);
```

Create a method matching delegate signature (return type and parameters)

```csharp
void PrintToConsole(string message)
{
    Console.WriteLine($"{message} printed to console");
}
```

Create a delegate instance and assign the PrintToConsole method to it:

```csharp
PrintMessage printMessageDel = new PrintMessage(PrintToConsole);
printMessageDel("the message");
```

**Output**

[![Output](https://blog.elmah.io/content/images/2024/12/output.png)](https://blog.elmah.io/content/images/2024/12/output.png)

Now, assign another method to this delegate.

The method 

```csharp
void PrintToFile(string message)
{
    Console.WriteLine($"{message} printed to file");
}
```

Add it to the delegate.

```csharp
printMessageDel += PrintToFile;
```

Calling the delegate is the same as above.

```csharp
printMessageDel("the message");
```

**Output**

[![Output](https://blog.elmah.io/content/images/2024/12/output2-1.png)](https://blog.elmah.io/content/images/2024/12/output2-1.png)

It calls the methods in the order they were added to the delegate.

Hence, we abstracted the method calls with the delegate. Without delegates, we had to do

```csharp
PrintToConsole("the message");
PrintToFile("the message");
```

A delegate that references more than one method is called a Multicast delegate.

## Example 2: Menu with conditional method execution

This example will be simple but will meet real-world applications of delegating. We are asking for user input and invoking methods based on the input. Lets employee delegate here

Create a delegate

```csharp
public delegate void PrintMessage(string name);
```

Methods are

```csharp
void AddContact(string name)
{
    Console.WriteLine($"Added contact: {name}");
}

void ViewContact(string name)
{
    Console.WriteLine($"Viewing contact: {name}");
}

void DeleteContact(string name)
{
    Console.WriteLine($"Deleted contact: {name}");
}
```

Assigning methods to the delegate

```csharp
Dictionary<string, PrintMessage> menu = new Dictionary<string, PrintMessage>
{
    { "1", AddContact },
    { "2", ViewContact },
    { "3", DeleteContact }
};
```

Main program

```csharp
while (true)
{
   Console.WriteLine("\nMenu:");
   Console.WriteLine("1. Add Contact");
   Console.WriteLine("2. View Contact");
   Console.WriteLine("3. Delete Contact");
   Console.WriteLine("4. Exit");
   Console.Write("Enter your choice: ");
   string choice = Console.ReadLine();


   if (menu.ContainsKey(choice))
   {
       Console.Write("Enter the contact name: ");
       string name = Console.ReadLine();
       menu[choice](name);  // Pass the input string (contact name) to the method


       if (choice == "4") break;
   }
   else
   {
       Console.WriteLine("Invalid choice. Please try again.");
   }
}
```

**Output**

[![Output](https://blog.elmah.io/content/images/2024/12/output3-1.png)](https://blog.elmah.io/content/images/2024/12/output3-1.png)

[![Output](https://blog.elmah.io/content/images/2024/12/output4-1.png)](https://blog.elmah.io/content/images/2024/12/output4-1.png)

## Example 3: Delegates with Lambda expression

You don't always need to create methods separately before assigning them to a delegate. With the modern lambda expression of C#, you can simply add a lambda expression directly to the delegate.

```csharp
PrintMessage printMessageDel = (name) =>
{
   Console.WriteLine($"Greetings, {name}!");
};


printMessageDel("Wyne");
printMessageDel("Doug");
```

**Output**

[![Output](https://blog.elmah.io/content/images/2024/12/output5-1.png)](https://blog.elmah.io/content/images/2024/12/output5-1.png)

## Advantages of delegates in C#

One of the delegate's most significant additions in coding is decoupling between the caller and the receivers (methods). Calling code only needs to know the method signature while its implementation and names are abstracted. Delegates help in event-driven programming, where one object can notify other objects of changes (like button clicks, data changes, etc.) following the observer pattern. The same signature methods are assigned to a single delegate, avoiding compile-time type safety errors that may arise due to incompatible method calls. You can design callback patterns where one method is passed to another as a parameter and executes in a specific flow. Multicasting allows the invocation of multiple methods on a single action or call. 

## Common Built-in Delegates in C#

C# provides several delegates in the System namespace. Some of the most common are

*   Action 
*   Func
*   Predicate

### Action

Represents a method that does not return a value (void).

```csharp
Action<string> greet = name => Console.WriteLine($"Hello, {name}!");
greet("Perry");
// Outputs Hello, Perry!
```

### Func 

 Represents a method that returns a value.

```csharp
Func<int, int, int> add = (a, b) => a + b;
Console.WriteLine(add(5, 3)); 
// Output: 8
```

### Predicate 

Represents a method that takes one parameter of type T and returns a boolean (`bool`).

```csharp
Predicate<int> isEven = num => num % 2 == 0;
Console.WriteLine(isEven(4));
// Output: True
```

## Built-in Delegates Used in LINQ

If you are using LINQ, you are already using its delegate. Methods like `Select`,  `Where`, and `OrderBy` used `Func` delegate.

```csharp
List<int> numbers = new List<int> { 1, 2, 3, 4 };


// Select - Project each element
var squares = numbers.Select(num => num * num);
Console.WriteLine(string.Join(", ", squares));
```

**Output**

[![Output](https://blog.elmah.io/content/images/2024/12/output6-1.png)](https://blog.elmah.io/content/images/2024/12/output6-1.png)

Now you understand the magic behind LINQ usefulness and conciseness is delegate. When we pass a lambda function or lambda expression in LINQ, we actually assign it to a delegate.

## Conclusion

C# delegates are a powerful tool for method decoupling in a type-safe manner. We saw the basics and real-world usage of delegates in the blog post. Delegates can be handy in dealing with a large number of method calls where you want to remember only some of the methods and abstract those details. Multicasting and callback mechanisms add further to the usability of delegates.

[

Previous post

The NuGet packages we use to build elmah.io revisited

](/the-nuget-packages-we-use-to-build-elmah-io-revisited/)

[

Next post

Leveraging Tuples in C#: Real-World Use Cases

](/leveraging-tuples-in-c-real-world-use-cases/)

### [elmah.io](https://elmah.io): Error logging and Uptime Monitoring for your web apps

This blog post is brought to you by elmah.io. elmah.io is error logging, uptime monitoring, deployment tracking, and service heartbeats for your .NET and JavaScript applications. Stop relying on your users to notify you when something is wrong or dig through hundreds of megabytes of log files spread across servers. With elmah.io, we store all of your log messages, notify you through popular channels like email, Slack, and Microsoft Teams, and help you fix errors fast.

[![elmah.io app banner](https://blog.elmah.io/assets/img/elmahio-app-banner.webp?v=58649d91c2)](https://elmah.io)

[See how we can help you monitor your website for crashes Monitor your website](https://app.elmah.io/signup/)

[Copyright](https://blog.elmah.io/copyright/)

•

[Guest posts](https://blog.elmah.io/guest-posts/)