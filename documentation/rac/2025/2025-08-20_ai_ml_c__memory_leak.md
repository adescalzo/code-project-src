```yaml
---
title: C# Memory Leak
source: https://www.c-sharpcorner.com/article/c-sharp-memory-leak/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-273
date_published: 2025-08-21T00:00:00.000Z
date_captured: 2025-08-29T12:26:48.682Z
domain: www.c-sharpcorner.com
author: Howher Michael
category: ai_ml
technologies: [C#, .NET, ASP.NET, dotMemory, PerfView, WeakEventManager]
programming_languages: [C#]
tags: [memory-leak, csharp, garbage-collection, events, performance, debugging, profiling, web-application, resource-management, .net]
key_concepts: [static-events, event-handler-leaks, IDisposable, garbage-collection, weak-references, memory-profiling, resource-management-patterns, object-lifetime]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details a challenging C# memory leak encountered in a web application, where memory usage escalated dramatically in production. The root cause was identified as static event handlers preventing `UserController` instances from being garbage collected due to strong references. The author explains the hidden trap and provides a solution involving explicit unsubscription in the `Dispose()` method and adopting a `SafeSubscriber` pattern. The post emphasizes the importance of understanding static events, using profiling tools like dotMemory and PerfView, and testing at scale to prevent such issues. This experience led to improved memory management practices, including memory audits in code reviews and automated memory profiling.]
---
```

# C# Memory Leak

# C# Memory Leak

By [Howher Michael](https://www.c-sharpcorner.com/members/howher-michael)

## The Problem That Kept Me Up for Three Nights

Last week, I was debugging the most frustrating issue I've encountered in my two years of C# development. Our web application was consuming memory at an alarming rate - starting at 200MB and climbing to 2GB within hours. Users were complaining about slowdowns, and our production servers were crashing every few hours.

I thought I knew C# pretty well. I'd read about garbage collection, understood reference types versus value types, and always disposed of my IDisposable objects. But this memory leak had me stumped.

The weird part? The leak only happened in production. Local development and our staging environment ran perfectly fine. I spent three sleepless nights running profilers, analyzing heap dumps, and questioning every line of code I'd written in the past month.

## The Discovery That Changed Everything

Here's what I found, and it completely changed how I think about C# memory management.

The culprit was something I never expected: static event handlers.

I had this innocent-looking piece of code in our notification service.

```csharp
public class NotificationService
{
    public static event Action<string> OnNotificationSent;

    public void SendNotification(string message)
    {
        // Send notification logic
        OnNotificationSent?.Invoke(message);
    }
}
```

And in various parts of our application, I was subscribing to this event.

```csharp
public class UserController : Controller
{
    public UserController()
    {
        NotificationService.OnNotificationSent += HandleNotification;
    }
    private void HandleNotification(string message)
    {
        // Handle the notification
    }
}
```

Looks harmless, right? Wrong.

## The Hidden Trap I Walked Into

Here's what I didn't realize: when you subscribe to a static event, the static event holds a reference to your instance. This means the garbage collector can't clean up your controller instances because the static event is keeping them alive.

In production, we were creating hundreds of controller instances per minute. Each one was subscribing to the static event, and none of them were being garbage collected. That's why our memory kept climbing.

The fix was embarrassingly simple once I understood the problem.

```csharp
public class UserController : Controller, IDisposable
{
    public UserController()
    {
        NotificationService.OnNotificationSent += HandleNotification;
    }
    public void Dispose()
    {
        NotificationService.OnNotificationSent -= HandleNotification;
    }
    private void HandleNotification(string message)
    {
        // Handle the notification
    }
}
```

## What I Learned About C# Memory Management?

This experience taught me several crucial lessons that I wish I'd known earlier.

*   **Static events are memory leak traps:** Any time you subscribe to a static event from an instance method, you're creating a potential memory leak. The static event holder keeps your object alive indefinitely.
*   **WeakEventManager is your friend:** For scenarios where you need static events, consider using WeakEventManager or implementing weak event patterns. They allow the garbage collector to clean up subscribers even if they forget to unsubscribe.
*   **Profiling tools are essential:** I used dotMemory and PerfView to track down this issue. These tools showed me exactly which objects weren't being collected and helped me trace the references keeping them alive.
*   **Production environments reveal hidden issues:** The reason this only happened in production was the scale. Development had maybe 10-20 controller instances total, but production was creating thousands. The leak was there all along - I just never noticed it at small scale.

### The Performance Impact Was Huge

After implementing the fix, the results were dramatic.

*   Memory usage dropped from 2GB back to 200MB
*   Response times improved by 40%
*   Server crashes have stopped completely
*   CPU usage decreased because the garbage collector wasn't working overtime

### My New C# Memory Rules

Since this experience, I follow these rules religiously.

*   **Always unsubscribe from events in Dispose():** If you subscribe in a constructor or method, unsubscribe in Dispose(). No exceptions.
*   **Be suspicious of static events:** Every time I see a static event, I ask myself if there's a better way. Sometimes the answer is yes, sometimes no, but I always think about the memory implications.
*   **Profile early and often:** I now run memory profilers on every central feature, even in development. Catching these issues early saves massive headaches later.
*   **Test at scale:** I've started load testing with realistic user volumes much earlier in development. Problems that don't appear with five users can be catastrophic with 500.

### The Code Pattern That Saved Me

I now use this pattern for any class that subscribes to static events.

```csharp
public class SafeSubscriber : IDisposable
{
    private bool _disposed = false;

    public SafeSubscriber()
    {
        StaticEventSource.SomeEvent += HandleEvent;
    }

    private void HandleEvent(object sender, EventArgs e)
    {
        if (_disposed) return;

        // Handle event logic here
    }
    public void Dispose()
    {
        if (!_disposed)
        {
            StaticEventSource.SomeEvent -= HandleEvent;
            _disposed = true;
        }
    }
}
```

### Why This Matters More than You Think?

Memory leaks in C# are sneaky because the garbage collector usually handles everything for you. When you're coming from languages like JavaScript or Python, you might think C# memory management is entirely automatic. It mostly is, but events are one of the major exceptions.

This isn't just about static events either. The same principle applies to:

*   Long-lived objects holding references to short-lived ones
*   Timers that aren't properly disposed of
*   Event subscriptions that outlive their subscribers
*   Closures that capture more context than necessary

## What I'm Doing Differently Now?

I've changed my development workflow based on this experience.

*   **Code reviews now include memory audits:** Every PR gets reviewed specifically for potential memory leaks, especially around event handling and IDisposable implementations.
*   **Automated testing includes memory profiling:** Our CI pipeline now runs basic memory profiling on integration tests to catch obvious leaks before they reach production.
*   **Documentation includes disposal patterns:** When I write classes that subscribe to events, I document the disposal requirements clearly so future developers (including future me) don't make the same mistakes.

This was a humbling experience that reminded me there's always more to learn, even in languages I thought I understood well. The silver lining? Our application is now more performant than ever, and I'm much more confident about memory management in my C# code.

Have you run into similar memory issues with C#? I'd love to hear about your experiences and how you solved them.

---

### Related Images/Ads

*   **Ebook Cover: Regular Expressions (Regex) in C#** - An ebook cover featuring "C# Corner" and "C#" logos, with the title "Regular Expressions" and author "Mahesh Chand" on a blue background.
*   **AI Trainings Ad (Woman with Laptop)** - An advertisement for "AI TRAININGS" offering courses like "Generative AI for Beginners", "Mastering Prompt Engineering", and "Mastering LLMs", with a smiling woman working on a laptop.
*   **Mastering Prompt Engineering Ad (Robot Head)** - An advertisement for "Instructor-led Trainings: MASTERING PROMPT ENGINEERING", featuring a stylized robotic head against a digital background.