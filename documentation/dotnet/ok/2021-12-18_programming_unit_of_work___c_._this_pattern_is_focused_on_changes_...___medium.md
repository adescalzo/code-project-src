```yaml
---
title: "Unit Of Work — C#. This pattern is focused on changes ... | Medium"
source: https://medium.com/@martinstm/unit-of-work-net-core-652f9b6cf894
date_published: 2021-12-18T16:23:02.247Z
date_captured: 2025-08-08T12:32:33.334Z
domain: medium.com
author: Tiago Martins
category: programming
technologies: [.NET, SQL Database (General)]
programming_languages: [C#, SQL]
tags: [unit-of-work, design-patterns, csharp, data-access, transaction-management, repository-pattern, software-architecture, dotnet, object-lifecycle]
key_concepts: [Unit of Work pattern, Repository pattern, transaction management, object lifecycle, dependency injection, concurrency, data persistence, architectural patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an overview of the Unit of Work design pattern, defining it as a context that tracks changes to objects within a transaction. It illustrates its implementation in C# with core methods like `BeginTransaction`, `SaveChanges`, `Commit`, and `Rollback`, which mirror typical database operations. The author provides practical code examples demonstrating its usage for managing multiple data modifications atomically and highlights its synergy with the Repository pattern. The piece emphasizes that `SaveChanges` keeps changes in memory until `Commit` persists them to the underlying data storage, making the pattern adaptable to various storage systems.
---
```

# Unit Of Work — C#. This pattern is focused on changes ... | Medium

# Unit Of Work — C#

![C# Unit of Work Logo](https://miro.medium.com/v2/resize:fit:700/1*qEmLubkC3Agiaap5wnBu9g.png)
*Image: A purple hexagonal C# logo with the text "Unit Of Work" below it, indicating the topic of the article.*

This pattern is focused on changes in objects. As Martin Fowler said, this unit keeps a list of changed objects in a transaction context. Also manages the write operation and deal with the concurrency problems. We can look at this as a context, session, or object which follows all the changes on data models during a transaction.

# Architecture Overview

![UML Diagram of IUnitOfWork and UnitOfWork](https://miro.medium.com/v2/resize:fit:487/1*plkCvq5SxjVF-UrZKvGD2g.png)
*Image: A UML class diagram showing an `«interface» IUnitOfWork` at the top, with an arrow pointing down to a `UnitOfWork` class. The `UnitOfWork` class lists methods: `+ BeginTransaction(): void`, `+ SaveChanges(): void`, `+ Commit(): bool`, `+ Rollback(): void`, and `+ Repository<TEntity>(): IRepository<TEntity>`. Dashed arrows point from below `UnitOfWork` upwards, indicating dependencies or usage by other components.*

So we have the interface, the implementation, and two services with the UnitOfWork dependency.

# Show me some code…

The following code block shows a simple interface to accomplish the main purpose of this pattern.

```csharp
public interface IUnitOfWork   
{  
    void BeginTransaction();  
    void SaveChanges();  
    bool Commit();  
    void Rollback();  
}
```

As you can imagine, these names were inspired by the operations of any database.  
The `BeginTransaction` should initiate the transaction. If you are using a SQL database, you could define here the isolation level, for example.  
At this moment we can start to create and update some objects and then call the `SaveChanges` method. The goal of this method is to keep these changes available in memory during a transaction. When we finish all the changes in the open transaction we can `Commit` if everything is according to the data storage rules else we invoke `Rollback` to discard all these changes in case of any error.

# A simple example of usage

In the following example, we have a simple `Update` operation. The business logic behind is just to ensure that all the changes made in a `Person` are recorded in a history table.

```csharp
public void Update(Person person)  
{  
    UnitOfWork.BeginTransaction();    try  
    {  
        PersonService.Update(person);  
        PersonHistoryService.Create(person.Id, "Person updated!");  
    }  
    catch (Exception e)  
    {  
        UnitOfWork.Rollback();  
        _logger.LogError(e.Message);  
    }    UnitOfWork.Commit();  
}
```

Steps:

1.  Open a transaction
2.  Update the `Person` and create a new entry in the history table
3.  Each service calls `SaveChanges`
4.  If catches an exception discard changes using `Rollback` and log a message
5.  If everything is ok, `Commit` the transaction and write these changes in your storage system

This is a basic case that shows how to use the `UnitOfWork` service. It’s important to understand that when we call `SaveChanges` we just keep these objects in memory during the transaction scope. Those changes are persisted in data storage just when we commit the transaction.  
This was a case where we did multiple changes, and we must guarantee that all the changes are stored at the same time. However, we could have cases where we don’t open a transaction and we just need to call `SaveChanges` in the end.

# An even simpler example of usage

In the next code block, we have the method from service to create a history entry. After the repository call, we just need to save these changes.

```csharp
public void Create(int personId, string message)  
{  
    PersonHistory history = new PersonHistory(personId, message);  
    NoteRepository.Insert(history);  
    UnitOfWork.SaveChanges();  
}
```

As we can see this method calls the `SaveChanges` and if we want to create history without a transaction, that is enough to store the data.

# Repository Pattern Importance

Having the previous example in mind, we know that we must have the dependency injection of all the used repositories (`PersonRepository` and `NoteRepository` ) configured in the constructor of the class. It would be great if the Unit of Work knows how to give us an instance of a specific repository to keep the code simpler.

```csharp
UnitOfWork.Repository<Note>().Insert(note);
```

As we can see it’s easy to use and doesn’t need so many dependencies in our constructor. We just need to have the `IUnitOfWork` dependency.  
You can see how to implement the Repository Pattern [here](/p/78d0646b6045).  
To get this behavior we should change the interface:

```csharp
public interface IUnitOfWork   
{  
    void BeginTransaction();  
    void SaveChanges();  
    bool Commit();  
    void Rollback();  
      
    IRepository<TEntity> Repository<TEntity>();  
}
```

As we can see it’s really important to have the Repository Pattern implemented. This new method just works for cases where we have this pattern defined.  
The Unit of Work keeps a dictionary where the key is the name of the `TEntity` type and the value it’s a dynamic object which will be an instance of the repository of the `TEntity` . If the dictionary already has a value for the type, it returns it. If not, it will create a new instance and stores it. We can see the algorithm in the following code block:

```csharp
private Dictionary<string, dynamic> _repositories;

public IRepository<TEntity> Repository<TEntity>()  
{  
    if (_repositories == null)  
        _repositories = new Dictionary<string, dynamic>();    

    var type = typeof(TEntity).Name;    

    if (_repositories.ContainsKey(type))  
        return (IRepository<TEntity>)_repositories[type];    

    var repositoryType = typeof(Repository<>);  
    _repositories.Add(type, Activator.CreateInstance(  
        repositoryType.MakeGenericType(typeof(TEntity)), this)  
    );  
    return _repositories[type];  
}
```

# Conclusion

This was an overview of the Unit Of Work pattern and some examples of usage and architecture design. I assume that we are using a storage system to persist the data. It could be any type of storage, you just need to change the way how to implement the `IUnitOfWork` interface.