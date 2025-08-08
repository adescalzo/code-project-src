```yaml
---
title: "The Memento Design Pattern in C#, Practically With Examples [2024] – PostSharp Blog"
source: https://blog.postsharp.net/memento?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=the-memento-design-pattern-in-c
date_published: 2024-07-25T07:00:01.000Z
date_captured: 2025-08-08T13:34:21.377Z
domain: blog.postsharp.net
author: Unknown
category: architecture
technologies: [WPF, .NET, Metalama, Roslyn, Fody, GitHub, MVVM Community Toolkit, ImmutableList]
programming_languages: [C#, XAML]
tags: [memento-pattern, design-patterns, csharp, wpf, undo-redo, state-management, code-generation, aspect-oriented-programming, dotnet, ui-development]
key_concepts: [Memento design pattern, Originator, Memento, Caretaker, undo-redo, state-management, code-generation, aspect-oriented-programming]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a practical guide to implementing the Memento design pattern in C#, focusing on its application in UI features like undo/redo and transactions. It details the pattern's core components—Originator, Memento, and Caretaker—and walks through a manual implementation using a WPF "Fish Tank Manager" example. The author highlights the significant boilerplate code involved and introduces Metalama as a solution for automated code generation to reduce repetitive work. The article also addresses common challenges such as managing object graphs, memory consumption, collection handling with immutable types, and ensuring state isolation during edits, offering practical solutions and best practices.]
---
```

# The Memento Design Pattern in C#, Practically With Examples [2024] – PostSharp Blog

# The Memento Design Pattern in C#, Practically With Examples [2024]

by Metalama Team on 25 Jul 2024

The Memento pattern is one of the foundational design patterns. It helps build UI features like undo/redo or UI transactions. In this article, we will see how to implement the Memento pattern in C#. We use a small WPF-based Fish Tank Manager as a sample application.

## What is the Memento design pattern?

The **Memento design pattern** is a software design pattern that provides the ability to restore an object to its previous state without revealing its internal structure. The aim is to provide standardization in scenarios where the object’s state needs to be saved and restored, such as the GUI undo/redo mechanism we presented earlier, but also for, for example, in-memory transactions.

The Memento pattern consists of three key components:

*   **Originator**: The object whose state needs to be saved and restored.
*   **Memento**: An object that contains a snapshot of the Originator’s state.
*   **Caretaker**: The object that keeps track of multiple mementos, often implementing mechanisms to store and retrieve them.

While using the Memento pattern, two processes occur:

*   **Capture**: When a state snapshot is needed, the Originator creates a Memento object and passes it to the Caretaker for safekeeping.
*   **Restore**: To restore the state, the Caretaker provides the stored Memento back to the Originator, which rewrites its state.

## When to use the Memento pattern?

Here are some situations when you might use the Memento pattern:

1.  **Undo/Redo Functionality**: When you need to implement undo and redo mechanisms, the Memento pattern is ideal. It allows you to save the state of an object at a certain point and revert back to it when needed. For example, in text editors, graphic editors, or games.

2.  **State History or Snapshot Tracking**: If an application requires keeping a history of different states of an object, the Memento pattern can be useful. This is common in version control systems or in any system where you need to track changes over time.

3.  **Transaction Management**: In applications where transactions are used (like database operations), the Memento pattern can help in saving the state before the transaction starts. If the transaction fails, the state can be restored to its original state.

When introduced in an application, the Memento pattern must be exhaustively implemented in the whole object model for the user experience to be consistent. This means significant implementation and maintenance costs.

## Example: Fish in a Fish Tank

Imagine you are implementing an application that allows for tracking fish in your home fish tank.

The application consists of a list of fish, allowing you to add a fish, remove a fish, and edit a fish.

Let’s see how it looks, with its beatiful Undo button:

![Screenshot of the WPF Fish Tank Manager application, showing a list of fish on the left and details of a selected fish (Joseph Nguyen, Lionfish, Date Added 11.07.2024) on the right. An "Undo" button is visible in the top right corner.](assets/images/2024/2024-07-memento/app_final.png)

The app, which is implemented in WPF, initially has no functionality for undoing changes. We will add this as the article progresses. In particular, the `Fish` class, which represents a single fish, looks like this:

```csharp
public sealed class Fish : ObservableRecipient
{
    private string? _name;
    private string? _species;
    private DateTime _dateAdded;

    public string? Name
    {
        get => this._name;
        set => this.SetProperty( ref this._name, value, true );
    }

    public string? Species
    {
        get => this._species;
        set => this.SetProperty( ref this._species, value, true );
    }

    public DateTime DateAdded
    {
        get => this._dateAdded;
        set => this.SetProperty( ref this._dateAdded, value, true );
    }
}
```

As you can see, the class is initially pretty simple and contains only the data fields.

Let’s also look at the data fields of the `MainViewModel` class, which we will change later:

```csharp
private readonly IFishGenerator _fishGenerator;
private bool _isEditing;
private Fish? _currentFish;

public ObservableCollection<Fish> Fishes { get; } = new();
```

The full source code of examples in this article is available on [GitHub](https://github.com/postsharp/TimelessDotNetEngineer/tree/main/src/patterns/memento).

## How to implement the Memento pattern in C#?

The Memento pattern is implemented by defining interfaces for the Memento, Originator, and Caretaker:

*   The `IMementoable` interface is implemented by _originator_ objects that hold state needing to be captured and restored. It allows `IMementoCaretaker` to retrieve memento objects and restore the state of the object.

    ```csharp
    public interface IMementoable
    {
      IMemento SaveTo