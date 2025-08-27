```yaml
---
title: "Understanding Scope In C#. Scope refers to visibility and lifetime… | by kacar | Medium"
source: https://medium.com/@kacar7/understanding-scope-in-c-69312a56ddb2
date_published: 2025-02-11T18:02:07.292Z
date_captured: 2025-08-06T17:51:15.883Z
domain: medium.com
author: kacar
category: programming
technologies: [.NET]
programming_languages: [C#]
tags: [csharp, scope, variables, programming-fundamentals, object-oriented-programming, namespaces, lifetime, visibility]
key_concepts: [block-scope, method-scope, class-scope, static-scope, namespace-scope, variable-lifetime, variable-visibility]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a foundational understanding of scope in C#, explaining how it dictates the visibility and lifetime of variables, methods, and objects. Using a relatable banking system analogy, it illustrates five distinct types of scope: block, method, class (instance variables), static, and namespace. Each concept is clarified with practical C# code examples, demonstrating where specific program elements can be accessed and how long they persist within an application. The content is designed to help beginners grasp these core programming principles.
---
```

# Understanding Scope In C#. Scope refers to visibility and lifetime… | by kacar | Medium

# Understanding Scope In C#

Scope refers to visibility and lifetime of variables, methods, or objects in a program. When we want to access or modify a variable, we have to understand the concept of scope. There are several types of scopes.

![C# Scope Title Image](https://miro.medium.com/v2/resize:fit:472/1*gbowrab5efA4Lv6eNAVtmw.png)

Let’s use a real-life banking system to explain scope. Imagine you are developing a bank application where customers can deposit and withdraw money.

## Block Scope:

A variable declared inside a `{}` block is accessible only within that block.

**Example:** A customer tries to withdraw money, and we check if they have enough balance.

```csharp
class BankAccount
{
    public void Withdraw(decimal amount, decimal balance)
    {
        if (amount <= balance)
        {
            decimal newBalance = balance - amount; // newBalance only exists inside this block
            Console.WriteLine($"Withdrawal successful! New balance: {newBalance}");
        }

        // Console.WriteLine(newBalance); // ERROR: newBalance is not accessible here
    }
}
```

Checking if you have enough money inside an ATM. The check only happens inside that moment; it doesn’t exist elsewhere.

## Method Scope:

Variables declared inside a method can only be accessed within that method.

**Example:** A method calculates interest on a savings account.

```csharp
class BankAccount
{
    public void CalculateInterest()
    {
        decimal principal = 1000; // Only exists inside this method
        decimal rate = 0.05m;
        decimal interest = principal * rate;

        Console.WriteLine($"Interest earned: {interest}");
    }
}

// principal and rate cannot be accessed outside CalculateInterest()
```

We don’t see calculations outside of that specific process.

## Class Scope (Instance Variables):

Variables declared inside a class but outside methods are accessible throughout the class.

**Example:** A bank account has an `accountNumber` and a `balance` that should be accessible across all methods in the class.

```csharp
class BankAccount
{
    private string accountNumber;
    private decimal balance;

    public BankAccount(string accNumber, decimal initialBalance)
    {
        accountNumber = accNumber; // Accessible across the entire class
        balance = initialBalance;
    }

    public void ShowBalance()
    {
        Console.WriteLine($"Account {accountNumber} has balance: {balance}");
    }
}
```

Your bank account number and balance are known by the bank but not by everyone else.

## Static Scope:

A static variable or method belongs to the class itself, not instances.

**Example:** The bank keeps track of how many accounts have been created.

```csharp
class Bank
{
    private static int totalAccounts = 0; // Shared across all instances

    public Bank()
    {
        totalAccounts++;
    }

    public static void ShowTotalAccounts()
    {
        Console.WriteLine($"Total accounts created: {totalAccounts}");
    }
}
```

The bank keeps track of how many accounts it has.

## Namespace Scope:

Anything declared at the namespace level is accessible across the entire namespace.

**Example:** Define banking-related classes inside a `BankingSystem` namespace.

```csharp
namespace BankingSystem
{
    class BankAccount { }
    class Customer { }
    class Transaction { }
}
```

All bank-related operations are grouped under a single **banking department**.

👏 If you found my article useful, could you please clap the story to help spread the article?