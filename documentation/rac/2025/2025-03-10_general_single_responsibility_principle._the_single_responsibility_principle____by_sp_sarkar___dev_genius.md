```yaml
---
title: "Single Responsibility Principle. The Single Responsibility Principle… | by SP Sarkar | Dev Genius"
source: https://blog.devgenius.io/single-responsibility-principle-4501c74eb69f
date_published: 2025-03-10T17:19:19.064Z
date_captured: 2025-08-06T17:50:51.618Z
domain: blog.devgenius.io
author: SP Sarkar
category: general
technologies: [BCrypt, ADO.NET, SQL Server, SMTP]
programming_languages: [C#, SQL]
tags: [solid-principles, srp, object-oriented-design, software-design, refactoring, clean-code, maintainability, testability, csharp, design-patterns]
key_concepts: [single-responsibility-principle, solid-principles, object-oriented-design, code-maintainability, code-testability, dependency-injection, repository-pattern, loose-coupling]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article thoroughly explains the Single Responsibility Principle (SRP), a fundamental SOLID principle, asserting that a class should have only one reason to change. It uses a relatable restaurant analogy to illustrate the concept of specialized roles, then applies it to software design. The author demonstrates a common violation of SRP with a `UserService` class handling multiple concerns like user registration, password hashing, database operations, and email notifications. The content then provides a detailed refactoring process, breaking down the monolithic `UserService` into distinct, focused classes such as `UserValidator`, `PasswordManager`, `UserRepository`, and `EmailService`. This refactoring highlights the significant benefits of SRP, including improved maintainability, testability, reusability, and reduced coupling, ultimately leading to cleaner, more scalable, and robust applications.]
---
```

# Single Responsibility Principle. The Single Responsibility Principle… | by SP Sarkar | Dev Genius

# Single Responsibility Principle

![](https://miro.medium.com/v2/resize:fit:700/1*soVerCzGciutFFvyWXcOdg.png)

A visually appealing illustration of the Single Responsibility Principle (SRP) in software development. Courtesy DALL.E

# Introduction

The Single Responsibility Principle (SRP) is one of the five SOLID principles of object-oriented design. It states that a class should have only one reason to change, meaning it should have a single, well-defined responsibility. To understand this principle better, let’s look at a real-life analogy.

# The Restaurant Analogy

Imagine a restaurant where a single person is responsible for everything — welcoming guests, taking orders, cooking food, serving customers, and washing dishes. While it might work in a small setup, this approach quickly becomes inefficient as the workload increases. Mistakes are more likely to happen, customers will wait longer, and the overall experience will suffer.

To improve efficiency, restaurants assign specific roles to employees:

*   **Host:** Greets and seats customers.
*   **Waiter:** Takes orders and serves food.
*   **Chef:** Cooks the meals.
*   **Dishwasher:** Cleans the dishes.

Each person has a clearly defined responsibility, ensuring smooth operations. This same principle applies to software design.

# Definition of Single Responsibility Principle?

The Single Responsibility Principle is the “S” in the SOLID principles of object-oriented design. It states that:

> _“A class should have only one reason to change” or “A class should have only one responsibility.”_

This means each class in your application should focus on doing one thing well, rather than trying to handle multiple responsibilities. Understanding the Single Responsibility Principle in Real Life

# Why is SRP Important?

1.  **Maintainability**: When a class does only one thing, it’s easier to understand and modify.
2.  **Testability**: Classes with single responsibilities are easier to test.
3.  **Reusability**: Focused classes can be reused in different contexts.
4.  **Reduced coupling**: Changes in one part of your system are less likely to affect other parts.
5.  **Cleaner code**: Results in more organized, readable code.

# Without Single Responsibility Principle

![](https://miro.medium.com/v2/resize:fit:700/1*vBySRSDwEncRMffCkAtn0Q.png)

An illustration of a User Management System without the Single Responsibility Principle (SRP). Courtesy DALL.E

In software development, a class that tries to handle multiple responsibilities becomes difficult to maintain and modify. Consider a poorly designed `UserService` class:

```csharp
public class UserService  
{  
    // Responsibility 1: User data management  
    public void RegisterUser(string username, string email, string password)  
    {  
        // Validate user input  
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))  
            throw new ArgumentException("User details cannot be empty");  
          
        // Hash password  
        var hashedPassword = BCrypt.HashPassword(password);  
          
        // Save to database  
        using (var connection = new SqlConnection("connection_string"))  
        {  
            connection.Open();  
            var command = new SqlCommand(  
                "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)",   
                connection);  
            command.Parameters.AddWithValue("@Username", username);  
            command.Parameters.AddWithValue("@Email", email);  
            command.Parameters.AddWithValue("@Password", hashedPassword);  
            command.ExecuteNonQuery();  
        }  
          
        // Send welcome email  
        var smtpClient = new SmtpClient("smtp.example.com")  
        {  
            Port = 587,  
            Credentials = new NetworkCredential("username", "password"),  
            EnableSsl = true,  
        };  
          
        smtpClient.Send(  
            "noreply@example.com",  
            email,  
            "Welcome to Our Application",  
            $"Hi {username}, thank you for registering with our application!"  
        );  
    }  
}
```

Here the`UserService` class violates the **Single Responsibility Principle (SRP)** because it handles multiple responsibilities in a single class:

1.  **User Registration** — Creating a new user.
2.  **Password Hashing** — Encrypting the user’s password.
3.  **Database Operations** — Storing user data in the database.
4.  **Email Notification** — Sending a welcome email.

# Refactoring to Follow SRP

![](https://miro.medium.com/v2/resize:fit:700/1*YyV8yDfJSh2YawP0258bIA.png)

A visually appealing illustration of the Single Responsibility Principle (SRP) using a User Management analogy. Courtesy DALL.E

# Validate User input into separate class

```csharp
public class UserValidator  
{  
    public void ValidateUserRegistration(string username, string email, string password)  
    {  
        if (string.IsNullOrEmpty(username))  
            throw new ArgumentException("Username cannot be empty");  
              
        if (string.IsNullOrEmpty(email))  
            throw new ArgumentException("Email cannot be empty");  
              
        if (string.IsNullOrEmpty(password))  
            throw new ArgumentException("Password cannot be empty");  
              
        // Add more complex validation as needed  
    }  
}
```

Here `UserValidator` class adheres to the **Single Responsibility Principle (SRP)** because it has only one responsibility: **validating user registration data**. It ensures that the username, email, and password are not empty before proceeding further.

**Why This Follows SRP**

*   It does **not** handle database operations.
*   It does **not** send emails.
*   It does **not** hash passwords.
*   It **only** validates user input.

# Password management into separate class

```csharp
public class PasswordManager  
{  
    public string HashPassword(string password)  
    {  
        return BCrypt.HashPassword(password);  
    }  
      
    public bool VerifyPassword(string password, string hashedPassword)  
    {  
        return BCrypt.Verify(password, hashedPassword);  
    }  
}
```

The `PasswordManager` class follows the **Single Responsibility Principle (SRP)** because it focuses solely on **password management**—hashing and verifying passwords.

**Why This Follows SRP**

*   The class is only responsible for password security.
*   Can be used across different services without duplicating password-handling logic.
*   Can be unit tested separately without affecting other functionalities.

# User Database Management into Separate class

```csharp
public class UserRepository  
{  
    private readonly string _connectionString;  
      
    public UserRepository(string connectionString)  
    {  
        _connectionString = connectionString;  
    }  
      
    public void SaveUser(string username, string email, string hashedPassword)  
    {  
        using (var connection = new SqlConnection(_connectionString))  
        {  
            connection.Open();  
            var command = new SqlCommand(  
                "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)",   
                connection);  
            command.Parameters.AddWithValue("@Username", username);  
            command.Parameters.AddWithValue("@Email", email);  
            command.Parameters.AddWithValue("@Password", hashedPassword);  
            command.ExecuteNonQuery();  
        }  
    }  
}
```

The `UserRepository` class follows the **Single Responsibility Principle (SRP)** because it is solely responsible for **database operations related to users**.

**Why This Follows SRP**

*   The class only handles **data persistence** and nothing else.
*   If the database structure or query changes, modifications are localized to this class.
*   `UserService` no longer deals with database interactions.

# Sending Email Service into separate class

```csharp
public class EmailService  
{  
    private readonly SmtpClient _smtpClient;  
      
    public EmailService(string smtpServer, int port, string username, string password)  
    {  
        _smtpClient = new SmtpClient(smtpServer)  
        {  
            Port = port,  
            Credentials = new NetworkCredential(username, password),  
            EnableSsl = true,  
        };  
    }  
      
    public void SendWelcomeEmail(string email, string username)  
    {  
        _smtpClient.Send(  
            "noreply@example.com",  
            email,  
            "Welcome to Our Application",  
            $"Hi {username}, thank you for registering with our application!"  
        );  
    }  
}
```

The `EmailService` class follows the **Single Responsibility Principle (SRP)** because it is solely responsible for **sending emails**.

**Why This Follows SRP**

*   The class only handles email notifications.
*   If email logic changes, it won’t affect other parts of the system.
*   `UserService` no longer deals with SMTP configurations or email sending.

# Refactored User Service class

```csharp
public class UserService  
{  
    private readonly UserValidator _validator;  
    private readonly PasswordManager _passwordManager;  
    private readonly UserRepository _userRepository;  
    private readonly EmailService _emailService;  
      
    public UserService(        UserValidator validator,  
        PasswordManager passwordManager,  
        UserRepository userRepository,  
        EmailService emailService)  
    {  
        _validator = validator;  
        _passwordManager = passwordManager;  
        _userRepository = userRepository;  
        _emailService = emailService;  
    }  
      
    public void RegisterUser(string username, string email, string password)  
    {  
        // Each class handles its own responsibility  
        _validator.ValidateUserRegistration(username, email, password);  
        var hashedPassword = _passwordManager.HashPassword(password);  
        _userRepository.SaveUser(username, email, hashedPassword);  
        _emailService.SendWelcomeEmail(email, username);  
    }  
}
```

The refactored `UserService` class now follows the **Single Responsibility Principle (SRP)** by delegating specific tasks to separate classes.

**Why This Follows SRP**

*   **Encapsulated Responsibilities** — Each class (`UserValidator`, `PasswordManager`, `UserRepository`, `EmailService`) is responsible for a **single concern**.
*   **Better Maintainability** — Changes to validation, password hashing, database operations, or email sending won’t impact `UserService`.
*   **Improved Testability** — Each component can be **unit tested independently**.
*   **Easier Dependency Injection** — The service depends on abstractions, making it more flexible.

# Conclusion

The **Single Responsibility Principle (SRP)** is a fundamental design principle that promotes **clean, maintainable, and scalable** software. By ensuring that each class has a **single reason to change**, we improve **code readability, testability, and maintainability**.

By applying SRP: ✅ We reduce **code complexity** and avoid tightly coupled dependencies. ✅ We make it **easier to modify and extend** our application without breaking unrelated functionality. ✅ We enable **better unit testing** by isolating individual responsibilities.

Just like a well-structured restaurant where each employee has a **specific role**, a well-designed software system ensures that each class has a **clear, focused responsibility**. Following SRP leads to more **robust, scalable, and flexible** applications, making future modifications **simpler and less error-prone**.

By embracing SRP in your projects, you take a **major step toward writing high-quality, maintainable code**.

![](https://miro.medium.com/v2/resize:fit:700/1*156c702896504a74953372c0d51f2271.jpeg)
A visually appealing illustration of a restaurant with various employees, each performing a specific role (Host, Waiter, Chef, Dishwasher), representing the Single Responsibility Principle.

![](https://miro.medium.com/v2/resize:fit:700/1*vBySRSDwEncRMffCkAtn0Q.png)
An illustration of a User Management System without the Single Responsibility Principle (SRP), depicting a single entity overwhelmed by multiple responsibilities.

![](https://miro.medium.com/v2/resize:fit:700/1*YyV8yDfJSh2YawP0258bIA.png)
A visually appealing illustration of the Single Responsibility Principle (SRP) applied to a User Management system, showing responsibilities broken down into distinct, specialized components.