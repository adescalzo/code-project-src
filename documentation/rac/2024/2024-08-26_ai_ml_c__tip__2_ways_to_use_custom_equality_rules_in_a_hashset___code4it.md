```yaml
---
title: "C# Tip: 2 ways to use custom equality rules in a HashSet | Code4IT"
source: https://www.code4it.dev/csharptips/hashset-custom-equality/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=understanding-net-stack-traces&_bhlid=838d09afe6fb974d148bee27ea062eb386a6dd32
date_published: 2024-08-27T00:00:00.000Z
date_captured: 2025-08-08T13:40:14.836Z
domain: www.code4it.dev
author: Davide Bellone
category: ai_ml
technologies: [.NET, Spectre.Console, HashSet]
programming_languages: [C#]
tags: [csharp, collections, hashset, equality, iequalitycomparer, iequatable, dotnet, object-equality, gethashcode]
key_concepts: [object-equality, hashset, IEqualityComparer, IEquatable, GetHashCode, Equals, collection-performance]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores two primary methods for defining custom equality rules within a C# `HashSet<T>`: implementing the `IEqualityComparer<T>` interface and implementing the `IEquatable<T>` interface directly on the type. It demonstrates how `HashSet` uses `GetHashCode` and `Equals` for uniqueness checks, illustrating the default behavior and the impact of custom implementations. The post also clarifies the precedence between `IEqualityComparer` and `IEquatable` when both are present, showing that the custom comparer takes priority. Practical C# code examples using a `Pirate` class are provided to illustrate each concept.
---
```

# C# Tip: 2 ways to use custom equality rules in a HashSet | Code4IT

![Featured image for the article "C# Tip: 2 ways to use custom equality rules in a HashSet" with a Code4IT logo.](/csharptips/hashset-custom-equality/featuredImage.png)

# [C# Tip: 2 ways to use custom equality rules in a HashSet](https://www.code4it.dev/csharptips/hashset-custom-equality/)

August 27, 2024 6 min read [CSharp Tips](https://www.code4it.dev/categories/csharp-tips/)

> With HashSet, you can get a list of different items in a performant way. What if you need a custom way to define when two objects are equal?

## Table of Contents

*   [Define a custom IEqualityComparer in a C# HashSet](#define-a-custom-iequalitycomparer-in-a-c-hashset)
*   [Implement the IEquatable interface](#implement-the-iequatable-interface)
*   [What has the precedence: IEquatable or IEqualityComparer?](#what-has-the-precedence-iequatable-or-iequalitycomparer)
*   [Wrapping up](#wrapping-up)

> **Just a second! 🫷**  
> If you are here, it means that you are a software developer. So, you know that storage, networking, and domain management have a cost .  
>   
> If you want to support this blog, please ensure that you have disabled the adblocker for this site. **I configured Google AdSense to show as few ADS as possible** - I don't want to bother you with lots of ads, but I still need to add some to pay for the resources for my site.  
>   
> Thank you for your understanding.  
> \- _Davide_

Sometimes, object instances can be considered equal even though some of their properties are different. Consider a movie translated into different languages: the Italian and French versions are different, but the movie is the same.

If we want to store unique values in a collection, we can use a `HashSet<T>`. But how can we store items in a `HashSet` when we must follow a custom rule to define if two objects are equal?

In this article, we will learn two ways to add custom equality checks when using a `HashSet`.

Let’s start with a dummy class: `Pirate`.

```cs
public class Pirate
{
    public int Id { get; }
    public string Name { get; }

    public Pirate(int id, string username)
    {
        Id = id;
        Name = username;
    }
}
```

I’m going to add some instances of `Pirate` to a `HashSet`. Please, note that there are two pirates whose Id is 4:

```cs
List<Pirate> mugiwara = new List<Pirate>()
{
    new Pirate(1, "Luffy"),
    new Pirate(2, "Zoro"),
    new Pirate(3, "Nami"),
    new Pirate(4, "Sanji"), // This ...
    new Pirate(5, "Chopper"),
    new Pirate(6, "Robin"),
    new Pirate(4, "Duval"), // ... and this
};


HashSet<Pirate> hashSet = new HashSet<Pirate>();


foreach (var pirate in mugiwara)
{
    hashSet.Add(pirate);
}


_output.WriteAsTable(hashSet);
```

(I _really_ hope you’ll get the reference 😂)

Now, what will we print on the console? (ps: `output` is just a wrapper around some functionalities provided by [Spectre.Console](https://spectreconsole.net/), that I used here to print a table)

![Console output showing a table of 7 Pirate objects, including two distinct objects with Id 4 (Sanji and Duval), demonstrating default HashSet behavior without custom equality.](hashset-no-equality.png)

As you can see, we have both Sanji and Duval: even though their Ids are the same, those are two distinct objects.

Also, we haven’t told `HashSet` that the `Id` property must be used as a discriminator.

## Define a custom IEqualityComparer in a C# HashSet

In order to add a custom way to tell the `HashSet` that two objects can be treated as equal, we can define a **custom equality comparer**: it’s nothing but a class that implements the `IEqualityComparer<T>` interface, where `T` is the name of the class we are working on.

```cs
public class PirateComparer : IEqualityComparer<Pirate>
{
    bool IEqualityComparer<Pirate>.Equals(Pirate? x, Pirate? y)
    {
        Console.WriteLine($"Equals: {x.Name} vs {y.Name}");
        return x.Id == y.Id;
    }

    int IEqualityComparer<Pirate>.GetHashCode(Pirate obj)
    {
        Console.WriteLine("GetHashCode " + obj.Name);
        return obj.Id.GetHashCode();
    }
}
```

The first method, `Equals`, compares two instances of a class to tell if they are equal, following the custom rules we write.

The second method, `GetHashCode`, defines a way to build an object’s hash code given its internal status. In this case, I’m saying that the hash code of a Pirate object is just the hash code of its Id property.

To include this custom comparer, you must add a new instance of `PirateComparer` to the `HashSet` declaration:

```cs
HashSet<Pirate> hashSet = new HashSet<Pirate>(new PirateComparer());
```

Let’s rerun the example, and admire the result:

![Console output showing GetHashCode and Equals calls, followed by a table of 6 Pirate objects. Only one Pirate with Id 4 (Sanji) remains, demonstrating the effect of using a custom IEqualityComparer based on Id.](hashset-with-comparer.png)

As you can see, there is only one item whose Id is 4: Sanji.

Let’s focus a bit on the messages printed when executing `Equals` and `GetHashCode`.

```text
GetHashCode Luffy
GetHashCode Zoro
GetHashCode Nami
GetHashCode Sanji
GetHashCode Chopper
GetHashCode Robin
GetHashCode Duval
Equals: Sanji vs Duval
```

Every time we insert an item, we call the `GetHashCode` method to generate an internal ID used by the HashSet to check if that item already exists.

As stated by [Microsoft’s documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-object-gethashcode?wt.mc_id=DT-MVP-5005077),

> Two objects that are equal return hash codes that are equal. However, the reverse is not true: **equal hash codes do not imply object equality**, because different (unequal) objects can have identical hash codes.

This means that if the Hash Code is already used, it’s not guaranteed that the objects are equal. That’s why we need to implement the `Equals` method (hint: **do not just compare the HashCode of the two objects**!).

Is implementing a custom `IEqualityComparer` the best choice?

As always, it depends.

On the one hand, using a custom `IEqualityComparer` has the advantage of allowing you to have different `HashSets` work differently depending on the EqualityComparer passed in input; on the other hand, you are now forced to pass an instance of `IEqualityComparer` everywhere you use a HashSet — and if you forget one, you’ll have a system with inconsistent behavior.

There must be a way to ensure consistency throughout the whole codebase.

## Implement the IEquatable interface

It makes sense to implement the equality checks directly inside the type passed as a generic type to the `HashSet`.

To do that, you need to have that class implement the `IEquatable<T>` interface, where `T` is the class itself.

Let’s rework the Pirate class, letting it implement the `IEquatable<Pirate>` interface.

```cs
public class Pirate : IEquatable<Pirate>
{
    public int Id { get; }
    public string Name { get; }

    public Pirate(int id, string username)
    {
        Id = id;
        Name = username;
    }

    bool IEquatable<Pirate>.Equals(Pirate? other)
    {
        Console.WriteLine($"IEquatable Equals: {this.Name} vs {other.Name}");
        return this.Id == other.Id;
    }

    public override bool Equals(object obj)
    {
        Console.WriteLine($"Override Equals {this.Name} vs {(obj as Pirate).Name}");
        return Equals(obj as Pirate);
    }

    public override int GetHashCode()
    {
        Console.WriteLine($"GetHashCode {this.Id}");
        return (Id).GetHashCode();
    }
}
```

The `IEquatable` interface forces you to implement the `Equals` method. So, now we have two implementations of Equals (the one for `IEquatable` and the one that overrides the default implementation). Which one is correct? Is the `GetHashCode` really used?

Let’s see what happens in the next screenshot:

![Console output showing GetHashCode and IEquatable Equals calls, followed by a table of 6 Pirate objects. Only one Pirate with Id 4 (Sanji) remains, demonstrating the effect of implementing IEquatable on the Pirate class.](result-with-iequatable.png)

As you could’ve imagined, the Equals method called in this case is the one needed to implement the IEquatable interface.

Please note that, as we don’t need to use the custom comparer, the HashSet initialization becomes:

```cs
HashSet<Pirate> hashSet = new HashSet<Pirate>();
```

## What has the precedence: IEquatable or IEqualityComparer?

What happens when we use both `IEquatable` and `IEqualityComparer`?

Let’s quickly demonstrate it.

First of all, keep the previous implementation of the Pirate class, where the equality check is based on the `Id` property:

```cs
public class Pirate : IEquatable<Pirate>
{
    public int Id { get; }
    public string Name { get; }

    public Pirate(int id, string username)
    {
        Id = id;
        Name = username;
    }

    bool IEquatable<Pirate>.Equals(Pirate? other)
    {
        Console.WriteLine($"IEquatable Equals: {this.Name} vs {other.Name}");
        return this.Id == other.Id;
    }

    public override int GetHashCode()
    {
        Console.WriteLine($"GetHashCode {this.Id}");
        return (Id).GetHashCode();
    }
}
```

Now, create a new `IEqualityComparer` where the equality is based on the `Name` property.

```cs
public class PirateComparerByName : IEqualityComparer<Pirate>
{
    bool IEqualityComparer<Pirate>.Equals(Pirate? x, Pirate? y)
    {
        Console.WriteLine($"Equals: {x.Name} vs {y.Name}");
        return x.Name == y.Name;
    }
    int IEqualityComparer<Pirate>.GetHashCode(Pirate obj)
    {
        Console.WriteLine("GetHashCode " + obj.Name);
        return obj.Name.GetHashCode();
    }
}
```

Now we have custom checks on both the Name and the Id.

It’s time to add a new pirate to the list, and initialize the `HashSet` by passing in the constructor an instance of `PirateComparerByName`.

```cs
List<Pirate> mugiwara = new List<Pirate>()
{
    new Pirate(1, "Luffy"),
    new Pirate(2, "Zoro"),
    new Pirate(3, "Nami"),
    new Pirate(4, "Sanji"), // Id = 4
    new Pirate(5, "Chopper"), // Name = Chopper
    new Pirate(6, "Robin"),
    new Pirate(4, "Duval"), // Id = 4
    new Pirate(7, "Chopper") // Name = Chopper
};


HashSet<Pirate> hashSet = new HashSet<Pirate>(new PirateComparerByName());


foreach (var pirate in mugiwara)
{
    hashSet.Add(pirate);
}
```

We now have two pirates with ID = 4 and two other pirates with Name = Chopper.

Can you foresee what will happen?

![Console output showing GetHashCode and Equals calls (from IEqualityComparer), followed by a table of 7 Pirate objects. The IEqualityComparer based on Name takes precedence, resulting in only one "Chopper" entry, but both "Sanji" and "Duval" (with same Id but different names) are present.](hashset-result-iequalitycomparer-iequatable.png)

The checks on the ID are totally ignored: in fact, the final result contains both Sanji and Duval, even if their IDs are the same. **The custom `IEqualityComparer` has the precedence over the `IEquatable` interface**.

_This article first appeared on [Code4IT 🐧](https://www.code4it.dev/)_

## Wrapping up

This started as a short article but turned out to be a more complex topic.

There is actually more to discuss, like performance considerations, code readability, and more. Maybe we’ll tackle those topics in a future article.

I hope you enjoyed this article! Let’s keep in touch on [LinkedIn](https://www.linkedin.com/in/BelloneDavide/) or [Twitter](https://twitter.com/BelloneDavide)! 🤜🤛

Happy coding!

🐧

[CSharp](https://www.code4it.dev/tags/csharp/)

Share it on

[

](https://twitter.com/intent/tweet/?text=C%23%20Tip%3a%202%20ways%20to%20use%20custom%20equality%20rules%20in%20a%20HashSet,%20by%20%40BelloneDavide%0A%0A&url=https%3a%2f%2fwww.code4it.dev%2fcsharptips%2fhashset-custom-equality%2f)[

](https://www.linkedin.com/sharing/share-offsite/?url=https://www.code4it.dev/csharptips/hashset-custom-equality/)