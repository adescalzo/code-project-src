```yaml
---
title: "Peter Ritchie's Blog - Identity Obsession and Domain-Driven Design"
source: https://blog.peterritchie.com/posts/identity-obsession-and-domain-driven-design
date_published: unknown
date_captured: 2025-08-08T15:47:32.869Z
domain: blog.peterritchie.com
author: Unknown
category: ai_ml
technologies: [.NET, RDBMS, UUID, GUID, Disqus, Wyam, Trianglify, Please.js, BackgroundCheck.js, highlight.js]
programming_languages: [C#, SQL, JavaScript]
tags: [domain-driven-design, ddd, identity, entities, value-objects, aggregates, repository-pattern, code-smell, software-design, dotnet]
key_concepts: [identity-obsession, domain-driven-design, primitive-obsession, entities, value-objects, aggregates, repository-pattern, ubiquitous-language, code-smells]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces "Identity Obsession," a code smell in Domain-Driven Design (DDD) where domain objects are habitually given global, explicit identifiers, often influenced by relational database implementation details. Drawing parallels to Primitive Obsession, the author argues that not all domain objects require a global identity, and that local or implicit identities are often sufficient, especially within Aggregates. The piece advocates for isolating Identity and Identifiers from Entities, treating them as independent domain concepts managed by services like Repositories rather than the Entities themselves. This approach aims to improve implementation clarity, reduce coupling, and gain deeper insight into the domain.
---
```

# Peter Ritchie's Blog - Identity Obsession and Domain-Driven Design

# Identity Obsession and Domain-Driven Design

Published on Thursday, September 26, 2024

![A complex, colorful mind-map or network diagram with various interconnected circular nodes. Each node contains an icon representing different concepts like global connectivity, data, ideas (lightbulb), communication (email, chat bubbles), technology (laptop, computer screen), and human interaction. The nodes are linked by lines, suggesting relationships and flows within a system or thought process. The overall style is intricate and abstract, resembling a blueprint or a detailed infographic.](../assets/mind-map.png)

Domain-Driven Design builds on established ontological concepts like Identity, Entities, and characteristics. Entities in either context are constructs with _Identity_. Entities are recognizable instances of higher-level classifications. Entities are recognized not just by their characteristics or qualities. Ontologically, an Entity's Identity can be numerical _or_ philosophical. In software, explicit Identity is always implemented _numerically_ (that is, an Identifier that is compared by value.) In either context, Identity can be _implicit_ or _explicit_, too often explicit in software.

Domain-Driven Design categorizes Identity as either _global_ or _local_. I have found that too often, only global Identity is implemented or recognized—which may be due to the explicitness of global Identity. I have also observed the over-use of Identity (explicit and global.) I'm calling this phenomenon "Identity Obsession," I'll explain it by comparing it to what helped inspire it: Primitive Obsession.

There's a code smell called Primitive Obsession[[Refactoring](https://amzn.to/3Y05vTq)], where domain-specific concepts are implemented in object-oriented languages as primitive (or built-in) types instead of custom types. Domain concepts are domain concepts because they are unique to a domain, so it's unlikely they would be on parity with anything in another, independent, domain; especially a technical concept like an `int` or a `string`. Primitive Obsession exists to help recognize that a custom type is necessary embody unique constraints over and above any implementation details of a primitive type.

Like Primitive Obsession, _Identity Obsession_ occurs due to the influence of implementation details. In the case of Identity Obsession, those implementation details are typically _the possibility_ of an RDBMS being involved.

But first, let's align on _Identity_ in Domain-Driven Design. Something has an Identity when you can differentiate it from something beyond its qualities or attributes. The obsession is that _implemented_ Domain Objects are given a globally unique Identifier (as opposed to locally unique) that habitually manifests as an `Id` property on the Domain Object's implementation. It's not uncommon for me to see _modeled_ Domain Objects (i.e., abstract concepts) given a globally unique Identifier attribute—that is, modeled to include a value with unique characteristics (auto-incrementing or universally unique) at a time when implementation details are yet to be considered.

### Identity Obsession

All Entities are modeled with an Identifier and all implemented Entities have a public `Id` property whether they are necessary or not

It's important to remember that one, Domain Objects can be differentiated from one another by more than an assignable property, and two, Domain Objects may have more than one Identity. Arguably, not all Identities are that of the _Domain Object_ (and others are the Identity of a DTO or the database row), but that's another symptom of the obsession. Let's look at a Domain Identity.

> **Note**
> An Entity doesn't need to differentiate itself from another instance of an Entity, it knows who it is

In many domains, an Entity called "Account" exists. Abstractly, it has a Domain-specific Identity used within the domain to identify one physical account from another. The type and value of that Identifier depends on the domain—typically, it's textual and has format/structure rules. Social Security Numbers, for example, have a textual structure of `123-45-6789` ("textual", not "string" to avoid Primitive Obsession).

A Domain-specific Identity is a part of the Domain whether there are technical implementations related to that Domain or not. Upon recognition or observance of that Identity, it becomes part of the Ubiquitous Language and independent of implementation details and constraints. The Identifier of the Domain Identity is sometimes not used as the implemented Identifier. That Identifier value has unique characteristics in technical solutions within the Domain, but that doesn't mean that the Domain Identifier is also the technical solution's unique Identifier. When implementing storage, there are many reasons why I don't want the Domain Identifier to be the _primary key_ and I would choose another value and probably a different primitive type for the implementation, like a UUID/GUID. Similarly, when implementing an API, I may not want to use an SSN within a URI or the primary key from the storage implementation. In this case, there are three _concrete Identifiers_ for the one abstract concept.

Both Entities and Value Objects can have Identity. As Domain-Driven Design practitioners, it's ingrained in us that an Entity has Identity and that often manifests as an `Id` property on an Entity implementation. But it shouldn't be that way. As a _global_ Identity, it should not be managed by the Entity to which it is assigned. The Identity is meaningless within the implementation class of an Entity. Identity is a higher-level concept from the concept it applies to. Something needs to manifest an _Identifier_ and an Entity must be consistent upon creation, so the Entity itself cannot be responsible for creating its own Identifier. The _Identifier_ is in fact an implementation detail. Management of that Identifier is typically the responsibility of a service, often a Repository service since that's when global Identity is required.

An Entity that requires an Identifier upon creation (and to thus be always consistent) in an object oriented language needs that Identifier to be passed in during construction or calculated within the Entity during construction. In the first scenario, instantiation of an Entity is now coupled to something responsible for the generation of Identifiers. In the second scenario the Entity implementation has taken on too much responsibility. At best case it implements an algorithm for generating a globally unique Identifier. At worst case it has to take on responsibility of the Identity semantics, like rules, validation, etc. Let's look at a naive example of an Entity that uses an SSN as an Identifier;

```csharp
public class Client : Person
{
    public Client(string givenName, string familyName, Ssn ssn)
        : base(givenName, familyName)
    {
        Ssn = ssn;
    }
    public Client(string givenName, string familyName, ISsnGenerator ssnGenerator)
        : base(givenName, familyName)
    {
        Ssn = ssnGenerator.Generate();
    }

    public Ssn Ssn { get; }
}
//...
public interface IClientRepository
{
    void Save(Client client);
    Result<Client> FindBySsn(Ssn ssn);
}
```

Constructing a `Client` instance is problematic with either of these constructors. One problem is the life of an SSN. What if an instantiation of a Client doesn't need to be persisted or is destroyed before it gets persisted? SSNs are a limited resource and need to be accounted for. We can't reserve one by generating it and not release it if it never gets used. We _could_ expand on this to include something silly like an `ISsnManager` but now `Client` needs to understand when it does or does not get persisted. This can't accommodate exceptional situations where a `Client` instance is destroyed without warning. This implemented behavior is equivalent of a domain that allows residents of the USA to create and assign their own SSNs. Can you imagine the problems this would create? One may say that this is unique to SSNs and you wouldn't have this problem with `Guid`s. This is a naive point of view due to a perception that it's safe to throw away GUID values due to an implementation detail. There are multiple race conditions between when an new `Client` exists and when they are persisted to the system. Because of that reality, and that Repositories aren't Factories, the only solution is the `Client` class not to have an SSN property:

```csharp
public class Client(string givenName, string familyName)
    : Person(givenName, familyName);
//...
public interface IClientRepository
{
    Result<Ssn> Save(Client client);
    Result<Client> FindBySsn(Ssn ssn);
}
```

With this implementation, SSN generation is deferred until a `Client` is persisted in the system. If the destruction of a `Client` occurs before being persisted, the system generates no SSN, and the release of the SSN is no longer an issue. The implementation of SSN generation and persistence still needs to account for failure, but that can handled _transactionally_ and that responsibility can live where it belongs, outside of the Domain. SSN generation is still a domain concept, but how an allocated value is transactionally assigned is an implementation detail of the chosen infrastructure.

This implement doesn't impose SSN be a primary key, this implementation simply recognizes SSN as a Domain Identifier. Implementation details within the infrastructure are free to use another Identifier for a primary key—if that applies at all in the chosen infrastructure.

> **Note**
> Nested objects implicitly have Identifiers that are field or property names.

## Implicit Local Domain Identifiers

Another aspect of Identity Obsession is the failure to recognize implicit local Identity, which is most common with Aggregates. The idea behind Aggregates is that it is a group of Domain Objects considered a unit with unit-wide consistency requirements. Similar to our Client SSN example, because an Aggregate is a consistency boundary, operations on the Aggregate become part of a _transaction_. Entities composed within an Aggregate are implementation details accessible only through the interface provided by the Aggregate. Those Entities are owned by the Aggregate, meaning the Aggregate controls their lifecycle, including their construction and assignment of an Identifier. The transaction race conditions also arise with Aggregates: global Identifiers are meaningless until the Aggregate is persisted. That is, an Entity with global Identity made available as a property of an Aggregate is problematic due to those race conditions I mentioned earlier. You can't reliably realize global Identity until the Aggregate is persisted, but the Aggregate still needs to differentiate all the Domain Objects it owns. This means owned Domain Objects have _local Identity_.

> **Note**
> Because the Entities within an Aggregate are an implementation detail, Entities within the Aggregate have Identity local to the Aggregate

The Aggregate still needs to differentiate the objects it owns, properties, fields, or indexes are _implicitly_ the Identifiers of Entities assigned to them, _local_ to the Aggregate. Imagine the quintessential e-commerce Domain that has order and order line Domain Objects. In this domain, order lines have no meaning outside of an order. For example:

```csharp
public void CreateDeveloperToolsOrder(IOrderRepository repository)
{
    var order = new Order();
    order.AddItem("Visual Studio 2022", 667.00m);
    order.AddItem("ReSharper", 1, 139.00m);
    order.AddItem("NDepend", 1, 492.00m);
    order.AddItem("LINQPad", 1, 65.00m);
    var firstItem = order.OrderLines.ElementAt(0);
    order.RemoveItem(0);
    var orderNumber = repository.Save(order);
}
```

The resulting `Order` instance comprises three `OrderLine` Entities. `OrderLine` Identity is the index of that instance in the `OrderLines` collection—a local identity. Until the call to `RemoveItem`, the Identity of the `Visual Studio Professional 2022` order line was `0`. Since `Visual Studio Professional 2022` was removed before `Save` was invoked, the local Identifier of `0` was the only Identifier that was ever required of it. Since order item Identity is local to an order, `OrderItem` implements no Identity:

```csharp
public record OrderLine(string ProductName, float Quantity, decimal Price)
{
    public float Quantity { get; private set; } = Quantity;
    public decimal Price { get; private set; } = Price;
    public string ProductName { get; private set; } = ProductName;

    public void ChangePrice(decimal price)
    {
        Price = price;
    }
    public void ChangeQuantity(float quantity)
    {
        Quantity = quantity;
    }
}
```

> **Note**
> Global Identifiers required only for persistence should be an implementation detail of the Repository, not the Entity.

## Recognizing Identity Obsession

### Domain Objects Described With an “ID” Characteristic or Attribute

I have found it rare that Domain Identity manifests as a concept named "ID." For example, an _account_ typically has an Account Number Identifier, not an "ID" in banking. The Account Number _is_ an "ID", but the Domain does not use that term (and should not be part of the Ubiquitous Language.) If you run across descriptions of Domain Objects that use the term "ID" or "Id" you can avoid or remedy Identity Obsession by asking the Domain experts questions to get clarity.

### Entities implemented with an `Id` property

Entity implementations may include an `Id` property if descriptions of the underlying Domain Object include an "ID" characteristic or attribute. If the underlying Domain Object description does not include an "ID" attribute and the Entity implementation has an `Id` property, you've probably discovered Identity Obsession. The Entity implementation should mirror the Domain Object's Identity terminology. If it's unclear, asking questions of the Domain experts will result in clarity.

## Take Aways

> **Take Away**
> Isolate Identity and Identifiers from Entities

Identity Obsession is the compulsion that every Domain Entity is described and implemented with a global Identifier. Global Identifiers are too frequently an implementation detail relating to a relational database. You can improve the implementation and gain deeper insight into the Domain by classifying Identities and treating them as independent Domain concepts. Identity as a separate domain concept enables you to isolate Identity and Identifiers from Entities so that an Entity is standalone and not complicated by the constraints of Identity.

## References

*   [Primitive Obsession](https://wiki.c2.com/?PrimitiveObsession)
*   [Refactoring: Improving the Design of Existing Code](https://amzn.to/3Y05vTq)