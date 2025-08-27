```yaml
---
title: "Advanced Strategies for Passing Parameters Between Classes in C#: Best Practices and Design Patterns | by Anderson Godoy | Medium"
source: https://medium.com/@anderson.buenogod/advanced-strategies-for-passing-parameters-between-classes-in-c-best-practices-and-design-4c5869bf1f27
date_published: 2024-12-22T10:50:46.835Z
date_captured: 2025-08-08T13:51:14.726Z
domain: medium.com
author: Anderson Godoy
category: architecture
technologies: [.NET]
programming_languages: [C#]
tags: [csharp, object-oriented-programming, design-patterns, dependency-injection, software-design, code-quality, modularity, reusability]
key_concepts: [constructor-injection, method-parameters, dependency-injection, decorator-pattern, chain-of-responsibility, custom-attributes, fluent-interface, delegates]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores advanced strategies for passing parameters between classes in C#, crucial for building modular and maintainable code. It covers fundamental techniques like constructor and method parameter passing, then delves into more sophisticated design patterns. Key patterns discussed include Dependency Injection for managing dependencies, the Decorator Pattern for extending behavior dynamically, and the Chain of Responsibility for conditional processing. Additionally, it introduces concepts like custom attributes for validation and fluent interfaces for readable object configuration, aiming to enhance code flexibility and testability.
---
```

# Advanced Strategies for Passing Parameters Between Classes in C#: Best Practices and Design Patterns | by Anderson Godoy | Medium

![](https://miro.medium.com/v2/resize:fit:700/0*tx0yexW8NWTstGhx)
*Image Description: An abstract, isometric illustration depicting a network of interconnected dark grey cubes with glowing blue elements, resembling a complex system of modules or classes. Blue lines connect these cubes, symbolizing data flow or dependencies. The background features faint diagrams and text, suggesting architectural blueprints or code structures, reinforcing the theme of software design and inter-class communication.*

# Advanced Strategies for Passing Parameters Between Classes in C#: Best Practices and Design Patterns

In C# development, passing parameters between classes is essential for creating modular, reusable, and maintainable code. However, as a project grows, dependencies between classes can quickly become complex. Using the right strategies and design patterns helps simplify this communication, keeping code flexible and testable.

In this article, we’ll explore advanced approaches for passing parameters between classes in C#. We’ll look at basic techniques like constructors and methods, as well as design patterns such as Dependency Injection, Decorator, Chain of Responsibility, and more. Each of these strategies has specific advantages that, when applied well, make code cleaner and more sustainable.

# 1. Passing Parameters via Constructor

Using a constructor is a common practice for passing essential dependencies to a class. This ensures that all required values are configured right at the instance creation.

```csharp
public class DatabaseConfig  
{  
    public string ConnectionString { get; }  
  
    public DatabaseConfig(string connectionString)  
    {  
        ConnectionString = connectionString;  
    }  
}  
  
public class DatabaseService  
{  
    private readonly DatabaseConfig _config;  
  
    public DatabaseService(DatabaseConfig config)  
    {  
        _config = config;  
    }  
  
    public void Connect()  
    {  
        Console.WriteLine($"Connecting to database with: {_config.ConnectionString}");  
    }  
}  
  
// Usage  
var config = new DatabaseConfig("Server=myServer;Database=myDB;");  
var dbService = new DatabaseService(config);  
dbService.Connect();
```

# 2. Passing Parameters via Methods

When a dependency is specific to an operation or is not essential for initialization, passing parameters directly via a method can be a good option.

```csharp
public class ReportGenerator  
{  
    public void GenerateReport(string reportName, User user)  
    {  
        Console.WriteLine($"Generating report '{reportName}' for user: {user.Name}");  
    }  
}  
  
// Usage  
var reportGenerator = new ReportGenerator();  
var user = new User { Name = "Ana" };  
reportGenerator.GenerateReport("Sales Report", user);
```

# 3. Dependency Injection (DI)

Dependency Injection (DI) makes it easier to pass dependencies between classes, promoting flexibility and testability. This technique is ideal for classes that depend on multiple implementations.

```csharp
public interface IEmailService  
{  
    void SendEmail(string message);  
}  
  
public class EmailService : IEmailService  
{  
    public void SendEmail(string message)  
    {  
        Console.WriteLine($"Sending email: {message}");  
    }  
}  
  
public class NotificationService  
{  
    private readonly IEmailService _emailService;  
  
    public NotificationService(IEmailService emailService)  
    {  
        _emailService = emailService;  
    }  
  
    public void Notify(string message)  
    {  
        _emailService.SendEmail(message);  
    }  
}  
  
// DI configuration in .NET  
var serviceCollection = new ServiceCollection();  
serviceCollection.AddTransient<IEmailService, EmailService>();  
serviceCollection.AddTransient<NotificationService>();  
  
var serviceProvider = serviceCollection.BuildServiceProvider();  
  
// Usage  
var notificationService = serviceProvider.GetService<NotificationService>();  
notificationService.Notify("Hello, world!");
```

# 4. Passing Parameters via Properties

For optional parameters or those that may change during the object’s lifetime, using properties is a common practice.

```csharp
public class Logger  
{  
    public string LogFilePath { get; set; }  
  
    public void Log(string message)  
    {  
        Console.WriteLine($"Logging to {LogFilePath}: {message}");  
    }  
}  
  
// Usage  
var logger = new Logger  
{  
    LogFilePath = "logs/app.log"  
};  
logger.Log("Application started.");
```

# 5. Using Delegates for Flexible Behavior

When a class’s behavior depends on a specific logic that may vary, using **delegates** or **funcs** provides high flexibility.

```csharp
public class Calculator  
{  
    public int Calculate(int a, int b, Func<int, int, int> operation)  
    {  
        return operation(a, b);  
    }  
}  
  
// Usage  
var calculator = new Calculator();  
int sum = calculator.Calculate(3, 4, (a, b) => a + b); // Addition  
int product = calculator.Calculate(3, 4, (a, b) => a * b); // Multiplication
```

# 6. Decorator Pattern for Extending Behavior

The **Decorator Pattern** allows adding functionality to a class dynamically without modifying its code.

```csharp
public interface INotifier  
{  
    void Notify(string message);  
}  
  
public class EmailNotifier : INotifier  
{  
    public void Notify(string message)  
    {  
        Console.WriteLine($"Email sent: {message}");  
    }  
}  
  
public class SmsNotifier : INotifier  
{  
    private readonly INotifier _notifier;  
  
    public SmsNotifier(INotifier notifier)  
    {  
        _notifier = notifier;  
    }  
  
    public void Notify(string message)  
    {  
        _notifier.Notify(message);  
        Console.WriteLine($"SMS sent: {message}");  
    }  
}  
  
// Using the Decorator  
INotifier notifier = new SmsNotifier(new EmailNotifier());  
notifier.Notify("Important notification");
```

# 7. Chain of Responsibility for Conditional Processing

With the **Chain of Responsibility**, you can pass a request through a chain of classes until one of them processes it.

```csharp
public abstract class RequestHandler  
{  
    protected RequestHandler? Next { get; set; }  
  
    public void SetNext(RequestHandler next) => Next = next;  
  
    public void Handle(Request request)  
    {  
        // Centralize the main chain logic  
        if (!HandleInternal(request) && Next != null)  
        {  
            Next.Handle(request);  
        }  
    }  
  
    // Abstract method for subclass-specific implementations  
    protected abstract bool HandleInternal(Request request);  
}  
  
public class AuthenticationHandler : RequestHandler  
{  
    protected override bool HandleInternal(Request request)  
    {  
        if (!request.IsAuthenticated)  
        {  
            Console.WriteLine("Authentication failed.");  
            return true; // Stop the chain since the request cannot proceed  
        }  
  
        Console.WriteLine("Authentication successful.");  
        return false; // Continue to the next handler  
    }  
}  
  
public class AuthorizationHandler : RequestHandler  
{  
    protected override bool HandleInternal(Request request)  
    {  
        if (!request.HasPermission("Admin"))  
        {  
            Console.WriteLine("Authorization failed.");  
            return true; // Stop the chain since the user lacks permissions  
        }  
  
        Console.WriteLine("Authorization successful.");  
        return false; // Continue to the next handler  
    }  
}  
  
public class LoggingHandler : RequestHandler  
{  
    protected override bool HandleInternal(Request request)  
    {  
        Console.WriteLine($"Logging request for user: {request.UserId}");  
        return false; // Always pass to the next handler  
    }  
}  
  
// Chain setup  
var authHandler = new AuthenticationHandler();  
var loggingHandler = new LoggingHandler();  
  
authHandler.SetNext(loggingHandler);  
  
// Usage  
authHandler.Handle("authenticated");
```

## Setting Up the Chain

The chain can be set up by creating instances of the handlers and linking them together:

```csharp
public static RequestHandler BuildRequestHandlerChain()  
{  
    var authenticationHandler = new AuthenticationHandler();  
    var authorizationHandler = new AuthorizationHandler();  
    var loggingHandler = new LoggingHandler();  
  
    // Link the handlers together  
    authenticationHandler.Next = authorizationHandler;  
    authorizationHandler.Next = loggingHandler;  
  
    return authenticationHandler;  
}
```

## Usage

To use the chain, create a `Request` object and pass it through the handler chain:

```csharp
public class Request  
{  
    public string UserId { get; set; } = string.Empty;  
    public bool IsAuthenticated { get; set; }  
    public List<string> Permissions { get; set; } = new();  
  
    public bool HasPermission(string permission)  
    {  
        return Permissions.Contains(permission);  
    }  
}  
  
public static void Main(string[] args)  
{  
    // Set up the chain  
    var handlerChain = BuildRequestHandlerChain();  
  
    // Create a request  
    var request = new Request  
    {  
        UserId = "user123",  
        IsAuthenticated = true,  
        Permissions = new List<string> { "User", "Admin" } // This user has "Admin" permissions  
    };  
  
    // Process the request through the handler chain  
    handlerChain.Handle(request);  
  
    Console.WriteLine("Request processing complete.");  
}
```

# Output Example

For the example above, assuming the request is authenticated and has the “Admin” permission, the output would be:

```
Authentication successful.  
Authorization successful.  
Logging request for user: user123  
Request processing complete.
```

If the request was not authenticated, the chain would stop early:

```
Authentication failed.  
Request processing complete.
```

# 8. Custom Attributes for Parameter Validation

Assigning **custom attributes** to class properties is an effective way to enforce validation rules.

```csharp
[AttributeUsage(AttributeTargets.Property)]  
public class RequiredAttribute : Attribute { }  
  
public class User  
{  
    [Required]  
    public string Name { get; set; }  
      
    public int Age { get; set; }  
}  
  
public class Validator  
{  
    public static bool Validate(object obj)  
    {  
        var properties = obj.GetType().GetProperties();  
        foreach (var property in properties)  
        {  
            if (Attribute.IsDefined(property, typeof(RequiredAttribute)) &&  
                property.GetValue(obj) == null)  
            {  
                Console.WriteLine($"The field {property.Name} is required.");  
                return false;  
            }  
        }  
        return true;  
    }  
}  
  
// Usage  
var user = new User();  
bool isValid = Validator.Validate(user); // Output: "The field Name is required."
```

# 9. Fluent Interface for Chained Configuration

A **Fluent Interface** enables configuring objects in a chained, readable manner.

```csharp
public class DatabaseConnection  
{  
    private string _server;  
    private string _database;  
  
    public DatabaseConnection SetServer(string server)  
    {  
        _server = server;  
        return this;  
    }  
  
    public DatabaseConnection SetDatabase(string database)  
    {  
        _database = database;  
        return this;  
    }  
  
    public void Connect()  
    {  
        Console.WriteLine($"Connecting to server {_server} and database {_database}");  
    }  
}  
  
// Fluent Interface usage  
var connection = new DatabaseConnection()  
    .SetServer("localhost")  
    .SetDatabase("MyDatabase");  
  
connection.Connect();
```

# Conclusion

Passing parameters between classes in C# can be implemented in various ways, each suited to different scenarios. Using constructors, methods, Dependency Injection, and design patterns such as Decorator and Chain of Responsibility allows for creating more modular and scalable code.

> Techniques like fluent interfaces, custom attributes, and Value Objects help reduce complexity and promote a more robust and sustainable software design.