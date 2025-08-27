```yaml
---
title: Song recommendations with C# Reactive Extensions
source: https://blog.ploeh.dk/2025/07/21/song-recommendations-with-c-reactive-extensions/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2100
date_published: 2025-07-21T03:00:00.000Z
date_captured: 2025-08-12T11:18:24.609Z
domain: blog.ploeh.dk
author: Mark Seemann
category: frontend
technologies: [ReactiveX, Reactive Extensions for .NET, .NET, NServiceBus, Git, System.Reactive]
programming_languages: [C#]
tags: [reactive-extensions, csharp, functional-programming, pipes-and-filters, architecture, observables, monads, asynchronous, software-design, data-streams]
key_concepts: [Reactive Extensions, pipes-and-filters architecture, functional programming, monads, Kleisli arrows, observable streams, asynchronous programming, message-based systems]
code_examples: false
difficulty_level: intermediate
summary: |
  This article demonstrates implementing a song recommendation algorithm using C# Reactive Extensions (Rx) as a lightweight pipes-and-filters architecture. It explains how to model data streams using `IObservable<T>` and compose them using monadic bind (`SelectMany` or LINQ query syntax). The author contrasts Rx's simplicity with more elaborate distributed message-based systems, highlighting trade-offs in capabilities like persistence and distribution. The example showcases handling impure operations and pure transformations within observable streams, concluding with a discussion on composition.
---
```

# Song recommendations with C# Reactive Extensions

# Song recommendations with C# Reactive Extensions by Mark Seemann

_Observables as small Recawr Sandwiches._

This article is part of a series titled [Alternative ways to design with functional programming](/2025/04/07/alternative-ways-to-design-with-functional-programming). In the [previous article in the series](/2025/07/14/song-recommendations-with-pipes-and-filters), you read some general reflections on a pipes-and-filters architecture. This article gives an example in C#.

The code shown here is from the _rx_ branch of the example code Git repository. As the name implies, it uses [ReactiveX](https://reactivex.io/), also known as [Reactive Extensions for .NET](https://github.com/dotnet/reactive).

To be honest, when refactoring an algorithm that's originally based on sequences of data, there's nothing particularly _reactive_ going on here. You may, therefore, argue that it's a poor fit for this kind of architecture. Be that as it may, keep in mind that the example code [in reality runs for about ten minutes](https://x.com/Tyrrrz/status/1493369905869213700), so moving towards an architecture that supports progress reporting or cancellation may not be entirely inappropriate.

The advantage of using Reactive Extensions for this particular example is that, compared to full message-bus-based frameworks, it offers a lightweight [developer experience](/2024/05/13/gratification), which enables us to focus on the essentials of the architecture.

### Handle own scrobbles

We'll start with the part of the process that finds the user's own top scrobbles. Please consult with [Oleksii Holub](https://tyrrrz.me/)'s [original article](https://tyrrrz.me/blog/pure-impure-segregation-principle#interleaved-impurities), or my article on [characterizing the implementation](/2025/04/10/characterising-song-recommendations), if you need a refresher.

When using Reactive Extensions, should we model this part as [IObservable<T>](https://learn.microsoft.com/dotnet/api/system.iobservable-1) or [IObserver<T>](https://learn.microsoft.com/dotnet/api/system.iobserver-1)?

Once you recall that `IObservable<T>`, [being a monad](/2025/03/03/reactive-monad), is eminently composable, the choice is clear. The `IObserver<T>` interface, on the other hand, gives rise to a [contravariant functor](/2021/09/02/contravariant-functors), but since that abstraction has weaker language support, we should go with the [monad](/2022/03/28/monads).

We can start by declaring the type and its initializer:

```csharp
public sealed class HandleOwnScrobblesObservable : IObservable<Scrobble>
{
    private readonly string userName;
    private readonly SongService _songService;
 
    public HandleOwnScrobblesObservable(string userName, SongService songService)
    {
        this.userName = userName;
        _songService = songService;
    }
 
    // Implementation goes here...
```

Given a `userName` we want to produce a (finite) stream of scrobbles, so we declare that the class implements `IObservable<Scrobble>`. An instance of this class is also going to need the `songService`, so that its implementation can query the out-of-process system that holds the data.

What's that, you say? Why does `_songService` have a leading underscore, while the `userName` field does not? Because Oleksii Golub used that naming convention for that service, but [I don't feel obliged to stay consistent](/2021/05/17/against-consistency) with that.

Given that we already have working code, the implementation is relatively straightforward.

```csharp
public IDisposable Subscribe(IObserver<Scrobble> observer)
{
    return Observable.Create<Scrobble>(Produce).Subscribe(observer);
}
 
private async Task Produce(IObserver<Scrobble> obs)
{
    // Impure
    var scrobbles = await _songService.GetTopScrobblesAsync(userName);
 
    // Pure
    var scrobblesSnapshot = scrobbles
        .OrderByDescending(s => s.ScrobbleCount)
        .Take(100);
 
    // Impure
    foreach (var scrobble in scrobblesSnapshot)
        obs.OnNext(scrobble);
    obs.OnCompleted();
}
```

If you are unused to Reactive Extensions, the hardest part may be figuring out how to implement `Subscribe` without getting bogged down with having to write too much of the implementation. Rx is, after all, a reusable library, so it should come with some building blocks for that, and it does.

It seems that the simplest way to implement `Subscribe` is to delegate to `Observable.Create`, which takes an expression as input. You can write the implementation inline as a lambda expression, but here I've used a `private` helper method to slightly decouple the implementation from the library requirements.

The first impure step is the same as we've already seen in the 'reference implementation', and the pure step should be familiar too. In the final impure step, the `Produce` method publishes the scrobbles to any subscribers that may be listening.

This is the step where you'll need to extrapolate if you want to implement this kind of architecture on another framework than Reactive Extensions. If you're using a distributed message-based framework, you may have a message bus on which you publish messages, instead of `obs`. So `obs.OnNext` may, instead, be `bus.Publish`, or something to that effect. You may also need to package each scrobble in an explicit message object, and add correlation identifier and such.

In many message-based frameworks ([NServiceBus](https://particular.net/nservicebus), for example), you're expected to implement some kind of message handler where messages arrive at a `Handle` method, typically on a stateless, long-lived object. This enables you to set up robust, distributed systems, but also comes with some overhead that requires you to coordinate or correlate messages.

In this code example, `userName` is just a class field, and once the object is done producing messages, it signals so with `obs.OnCompleted()`, after which the stream has ended.

The Rx implementation is simpler than some message-based systems I just outlined. That's the reason I chose it for this article. It doesn't mean that it's better, because that simplicity comes at the expense of missing capabilities. This system has no persistence, and I while I'm no expert in this field, I don't think it easily expands to a distributed system. And again, to perhaps belabour the obvious, I'm not insisting that any of those capabilities are always needed. I'm only trying to outline some of the trade-offs you should be aware of.

### Handle other listeners

`HandleOwnScrobblesObservable` objects publish `Scrobble` objects. What does the next 'filter' look like? It's another observable stream, implemented by a class called `HandleOtherListenersObservable`. It implements `IObservable<User>`, and its class declaration, constructor, and `Subscribe` implementation look a lot like what's already on display above. The main difference is the `Produce` method.

```csharp
private async Task Produce(IObserver<User> obs)
{
    // Impure
    var otherListeners = await _songService
        .GetTopListenersAsync(scrobble.Song.Id);
 
    // Pure
    var otherListenersSnapshot = otherListeners
        .Where(u => u.TotalScrobbleCount >= 10_000)
        .OrderByDescending(u => u.TotalScrobbleCount)
        .Take(20);
 
    // Impure
    foreach (var otherListener in otherListenersSnapshot)
        obs.OnNext(otherListener);
    obs.OnCompleted();
}
```

Compared to the reference architecture, this implementation is hardly surprising. The most important point to make is that, as was the goal all along, this is another small [Recawr Sandwich](/2025/01/13/recawr-sandwich).

A third observable, `HandleOtherScrobblesObservable`, handles the third step of the algorithm, and looks much like `HandleOtherListenersObservable`. You can see it in the Git repository.

### Composition

The three observable streams constitute most of the building blocks required to implement the song recommendations algorithm. Notice the 'fan-out' nature of the observables. The first step starts with a single `userName` and produces up to 100 scrobbles. To handle _each_ scrobble, a new instance of `HandleOtherListenersObservable` is required, and each of those produces up to twenty `User` notifications, and so on.

In the abstract, we may view the `HandleOwnScrobblesObservable` constructor as a function from `string` to `IObservable<Scrobble>`. Likewise, we may view the `HandleOtherListenersObservable` constructor as a function that takes a single `Scrobble` as input, and gives an `IObservable<User>` as return value. And finally, `HandleOtherScrobblesObservable` takes a single `User` as input to return `IObservable<Song>` as output.

Quick, what does that look like, and how do you compose them?

Indeed, those are [Kleisli arrows](/2022/04/04/kleisli-composition), but in practice, we use [monadic bind](/2022/03/28/monads) to compose them. In C# this usually means `SelectMany`.

```csharp
public async Task<IReadOnlyList<Song>> GetRecommendationsAsync(string userName)
{
    // 1. Get user's own top scrobbles
    // 2. Get other users who listened to the same songs
    // 3. Get top scrobbles of those users
    // 4. Aggregate the songs into recommendations
 
    var songs = await
        new HandleOwnScrobblesObservable(userName, _songService)
        .SelectMany(s => new HandleOtherListenersObservable(s, _songService))
        .SelectMany(u => new HandleOtherScrobblesObservable(u, _songService))
        .ToList();
    return songs
        .OrderByDescending(s => s.Rating)
        .Take(200)
        .ToArray();
}
```

The fourth step of the algorithm, you may notice, isn't implemented as an observable, but rather as a standard LINQ pipeline. This is because sorting is required, and if there's a way to sort an observable, I'm not aware of it. After all, given that observable objects may represent infinite data streams, I readily accept that there's no `OrderByDescending` method on `IObservable<T>`. (But then, the [System.Reactive](https://www.nuget.org/packages/System.Reactive/) library defines `Min` and `Max` operations, and exactly how those work when faced with infinite streams, I haven't investigated.)

While I could have created a helper function for that small `OrderByDescending`/`Take`/`ToArray` pipeline, I consider it under the [Fairbairn threshold](https://wiki.haskell.org/Fairbairn_threshold).

### Query syntax

You can also compose the algorithm using query syntax, which I personally find prettier.

```csharp
IObservable<Song> obs =
    from scr in new HandleOwnScrobblesObservable(userName, _songService)
    from usr in new HandleOtherListenersObservable(scr, _songService)
    from sng in new HandleOtherScrobblesObservable(usr, _songService)
    select sng;
IList<Song> songs = await obs.ToList();
return songs
    .OrderByDescending(s => s.Rating)
    .Take(200)
    .ToArray();
```

In this code snippet I've used explicit variable type declarations (instead of using the `var` keyword) for the sole purpose of making it easier to see which types are involved.

### Conclusion

This article shows an implementation example of refactoring the song recommendations problem to a pipes-and-filters architecture. It uses Reactive Extensions for .NET, since this showcases the (de)composition in the simplest possible way. Hopefully, you can extrapolate from this to a more elaborate, distributed asynchronous message-based system, if you need something like that.

The next example makes a small move in that direction.

**Next:** [Song recommendations with F# agents](/2025/07/28/song-recommendations-with-f-agents).

---

![A close-up portrait of Mark Seemann, a man with graying hair and a full gray beard, wearing glasses and a dark jacket. He is looking slightly to his left. The background is dark.](assets/themes/ploeh/images/Portrait22.jpg "Mark Seemann")