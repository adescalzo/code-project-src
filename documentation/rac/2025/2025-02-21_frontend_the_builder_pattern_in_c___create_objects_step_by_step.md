```yaml
---
title: "The Builder Pattern in C#: Create Objects Step by Step"
source: https://okyrylchuk.dev/blog/the-builder-pattern-in-csharp-create-objects-step-by-step/
date_published: 2025-02-21T15:50:00.000Z
date_captured: 2025-08-11T16:15:13.226Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: frontend
technologies: [.NET, Cognibase, SQL Server]
programming_languages: [C#]
tags: [builder-pattern, design-patterns, creational-patterns, csharp, dotnet, object-creation, fluent-interface, immutability, software-design, code-quality]
key_concepts: [Builder pattern, creational-design-patterns, object-construction, fluent-interface, immutable-objects, code-readability, code-maintainability, encapsulation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to implementing and utilizing the Builder pattern in C#. It addresses the common problem of constructing complex objects with numerous optional parameters, which can lead to unwieldy constructors and less readable code. The Builder pattern offers a structured solution by allowing object creation step by step, enhancing readability through method chaining and improving maintainability by encapsulating construction logic. The post illustrates the pattern with a practical `DbConfiguration` example, demonstrating how to achieve immutable objects and a fluent interface.
---
```

# The Builder Pattern in C#: Create Objects Step by Step

# The Builder Pattern in C#: Create Objects Step by Step

The **Builder pattern** is one of the most useful creational design patterns in object-oriented programming. It helps solve a common problem: constructing complex objects step by step while keeping the construction process flexible and clean. In this post, we’ll explore how to implement and use the Builder pattern in C#.

## **The Problem**

I bet you saw a lot of such code in the .NET Projects. It’s a big class with many properties, getters, and setters. Some of them are required, and others are optional. The instance of the object is created with an object initializer.

```csharp
var configuration = new DbConfiguration
{
    Server = "localhost",
    Database = "MyDatabase",
    UserId = "sa",
    Password = "password",
    Port = 1433,
    UseSSL = false,
    ConnectionTimeout = 30,
    IntegratedSecurity = false
};

public class DbConfiguration
{
    public required string Server { get; set; }
    public required string Database { get; set; }
    public required string UserId { get; set; }
    public required string Password { get; set; }
    public int? Port { get; set; }
    public bool? UseSSL { get; set; }
    public int? ConnectionTimeout { get; set; }
    public bool? IntegratedSecurity { get; set; }
    public int? MinPoolSize { get; set; }
    public int? MaxPoolSize { get; set; }
    public bool? Encrypt { get; set; }
    public bool? TrustServerCertificate { get; set; }

    public string GetConnectionString()
    {
        StringBuilder sb =
            new StringBuilder($"Server={Server};Database={Database};User Id={UserId};Password={Password};");
        if (Port.HasValue)
            sb.Append($"Port={Port.Value};");
        if (UseSSL.HasValue)
            sb.Append($"SSL Mode={(UseSSL.Value ? "Require" : "Disable")};");
        if (ConnectionTimeout.HasValue)
            sb.Append($"Timeout={ConnectionTimeout.Value};");
        if (IntegratedSecurity.HasValue && IntegratedSecurity.Value)
            sb.Append("Integrated Security=True;");
        if (MinPoolSize.HasValue)
            sb.Append($"Min Pool Size={MinPoolSize.Value};");
        if (MaxPoolSize.HasValue)
            sb.Append($"Max Pool Size={MaxPoolSize.Value};");
        if (Encrypt.HasValue)
            sb.Append($"Encrypt={Encrypt.Value};");
        if (TrustServerCertificate.HasValue)
            sb.Append($"TrustServerCertificate={TrustServerCertificate.Value};");

        return sb.ToString();
    }
}
```

To make this code better, we can introduce the constructor.

```csharp
[SetsRequiredMembers]
public DbConfiguration(
    string server, 
    string database, 
    string userId, 
    string password, 
    int? port = null, 
    bool? useSSL = null, 
    int? connectionTimeout = null,
    bool? integratedSecurity = null, 
    int? minPoolSize = null, 
    int? maxPoolSize = null, 
    bool? encrypt = null, 
    bool? trustServerCertificate = null)
{
    Server = server;      
    Database = database;
    UserId = userId;
    Password = password;
    Port = port;
    UseSSL = useSSL;
    ConnectionTimeout = connectionTimeout;
    IntegratedSecurity = integratedSecurity;
    MinPoolSize = minPoolSize;
    MaxPoolSize = maxPoolSize;
    Encrypt = encrypt;
    TrustServerCertificate = trustServerCertificate;
}
```

It’s not a super solution, but it makes object creation shorter and easier to read.

```csharp
var configuration = new DbConfiguration(
    server: "localhost",
    database: "MyDatabase",
    userId: "sa",
    password: "password",
    port: 1433,
    useSSL: false,
    connectionTimeout: 30,
    integratedSecurity: false);
```

## **Why Use the Builder Pattern?**

In C#, constructors can become challenging to manage when dealing with objects with many optional parameters. Using a traditional constructor with numerous parameters can lead to unreadable and error-prone code. The **Builder pattern** addresses this issue by providing a way to create objects in a structured and controlled manner.

**Key Benefits of the Builder pattern**:

*   **Improved Readability:** Allows chaining of methods to build objects in a readable way.
*   **Better Maintainability:** Helps manage complex object creation logic.
*   **Encapsulation:** Encapsulates the object construction logic inside a dedicated class.

## **Implementing the Builder Pattern**

Using the **Builder pattern**, we can construct the **DbConfiguration** object more flexibly and readably.

Let’s forbid the creation of the **DbConfiguration** instance with a constructor or object initializer outside the class and add the internal **Builder** class:

```csharp
public class DbConfiguration
{
    public string Server { get; private set; }
    public string Database { get; private set; }
    public string UserId { get; private set; }
    public string Password { get; private set; }
    public int? Port { get; private set; }
    public bool? UseSSL { get; private set; }
    public int? ConnectionTimeout { get; private set; }
    public bool? IntegratedSecurity { get; private set; }
    public int? MinPoolSize { get; private set; }
    public int? MaxPoolSize { get; private set; }
    public bool? Encrypt { get; private set; }
    public bool? TrustServerCertificate { get; private set; }

    // Block the default constructor
    private DbConfiguration() { }

    public class Builder
    {
        private readonly DbConfiguration _dbConfiguration;

        public Builder(string server, 
            string database, 
            string userId, 
            string password)
        {
            _dbConfiguration = new DbConfiguration
            {
                Server = server,
                Database = database,
                UserId = userId,
                Password = password
            };
        }

        public Builder WithServer(string server)
        {
            _dbConfiguration.Server = server;
            return this;
        }

        public Builder WithDatabase(string database)
        {
            _dbConfiguration.Database = database;
            return this;
        }

        public Builder WithUserId(string userId)
        {
            _dbConfiguration.UserId = userId;
            return this;
        }

        public Builder WithPassword(string password)
        {
            _dbConfiguration.Password = password;
            return this;
        }

        public Builder WithPort(int port)
        {
            _dbConfiguration.Port = port;
            return this;
        }

        public Builder WithUseSSL(bool useSSL)
        {
            _dbConfiguration.UseSSL = useSSL;
            return this;
        }

        public Builder WithConnectionTimeout(int connectionTimeout)
        {
            _dbConfiguration.ConnectionTimeout = connectionTimeout;
            return this;
        }

        public Builder WithIntegratedSecurity(bool integratedSecurity)
        {
            _dbConfiguration.IntegratedSecurity = integratedSecurity;
            return this;
        }

        public Builder WithMinPoolSize(int minPoolSize)
        {
            _dbConfiguration.MinPoolSize = minPoolSize;
            return this;
        }

        public Builder WithMaxPoolSize(int maxPoolSize)
        {
            _dbConfiguration.MaxPoolSize = maxPoolSize;
            return this;
        }

        public Builder WithEncrypt(bool encrypt)
        {
            _dbConfiguration.Encrypt = encrypt;
            return this;
        }

        public Builder WithTrustServerCertificate(bool trustServerCertificate)
        {
            _dbConfiguration.TrustServerCertificate = trustServerCertificate;
            return this;
        }

        public DbConfiguration Build()
        {
            return _dbConfiguration;
        }
    }
}
```

So, the **DbConfiguration** instance creation will look like:

```csharp
var configuration = new DbConfiguration.Builder(
    server: "localhost",
    database: "MyDatabase",
    userId: "sa",
    password: "password")
    .WithPort(1433)
    .WithUseSSL(false)
    .WithConnectionTimeout(30)
    .WithIntegratedSecurity(false)
    .Build();
```

## **When to use Builder Pattern**

**Benefits of the Builder Pattern**

1.  **Clear Construction Process**: The builder clearly states what properties are required (in the constructor) and what properties are optional (as separate methods).
2.  **Immutable Objects**: Notice how the Configuration’s properties are all **private set**. Once built, you cannot modify it.
3.  **Fluent Interface**: The chaining method makes the code more readable and discoverable through IDE intellisense.
4.  **Default Values**: The builder can set sensible defaults for optional properties that aren’t specified.
5.  **Validation**: The Build() method can ensure the object is in a valid state before returning it.

**The Builder pattern is handy when**:

*   Objects have lots of properties, especially optional ones.
*   You need immutable objects.
*   You want to enforce construction rules.
*   The object construction process is complex.
*   You want to create different representations of the same object.

## **Conclusion**

The **Builder pattern** provides a structured way to construct objects with required and optional properties while keeping the code clean and readable. It simplifies object instantiation, making it more flexible and maintainable. Applying this pattern improves configuration management and reduces the complexity of handling multiple optional parameters.