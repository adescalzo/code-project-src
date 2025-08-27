```yaml
---
title: Better Controlling Time with TimeProvider in .NET
source: https://okyrylchuk.dev/blog/better-controlling-time-in-dotnet/
date_published: 2024-07-19T21:21:55.000Z
date_captured: 2025-08-20T21:15:13.869Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET, .NET 8, Microsoft.Extensions.TimeProvider.Testing, Microsoft.Bcl.TimeProvider]
programming_languages: [C#]
tags: [dotnet, testing, unit-testing, time, abstraction, dependency-injection, development, time-management]
key_concepts: [TimeProvider, FakeTimeProvider, dependency-injection, unit-testing, time-abstraction, timer-testing, testability]
code_examples: false
difficulty_level: intermediate
summary: |
  The article introduces .NET 8's new `TimeProvider` abstraction, designed to overcome the limitations of static `DateTime` and `DateTimeOffset` properties, particularly for testing. It demonstrates how `TimeProvider` allows for easier mocking of time in unit and integration tests, providing methods like `GetUtcNow()` and `GetLocalNow()`, and supporting custom time zones. A significant part of the article focuses on `FakeTimeProvider` from the `Microsoft.Extensions.TimeProvider.Testing` package, showing how developers can precisely control and advance time within tests. This new feature simplifies the testing of time-dependent logic, including timers and `Task.Delay` operations. The author also notes that `Microsoft.Bcl.TimeProvider` can be used for backward compatibility with older .NET versions.
---
```

# Better Controlling Time with TimeProvider in .NET

# Better Controlling Time with TimeProvider in .NET

![](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)
*Image: Avatar of Oleg Kyrylchuk, the author of the article.*

By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/ "View all posts by Oleg Kyrylchuk") / July 19, 2024

Table Of Contents

1.  [TimeProvider](#timeprovider)
2.  [FakeTimeProvider](#faketimeprovider)
3.  [Summary](#summary)

The DateTime type has existed in .NET from the beginning. It represents an instant in time, typically a date and time of day.

However, it has some disadvantages. For example, it doesn’t support time zone handling. DateTimeOffset partially fixes this issue. 

Nevertheless, both DateTime and DateTimeOffset types have static properties. It makes it hard to test the time. 

The community solved this old issue by creating an abstraction for these types to be mocked in unit or integration tests. For example:

```csharp
public interface IDateTimeProvider
{
    DateTime DateTime { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime DateTime => DateTime.UtcNow;
}
```

.NET 8 solves this issue by introducing a new type, TimeProvider. But it has more features than the simple abstraction from the sample above. Let’s see what it can do. 

## TimeProvider

A new TimeProvider is an abstract class. Don’t worry; you don’t have to implement it if you don’t need it. The TimeProvider has a static property System, which returns the SystemTimeProvider implementation. 

```csharp
var timeProvider = TimeProvider.System;
```

Using dependency injection, you can easily register TimeProvider implementation with TimeProvider.System.

The TimeProvider is more powerful than the simple abstractions we created before. It contains LocalTimeZone. You can get DateTimeOffset in UTC and local time zone. 

```csharp
Console.WriteLine(timeProvider.LocalTimeZone);
// (UTC+01:00) Sarajevo, Skopje, Warsaw, Zagreb

Console.WriteLine(timeProvider.GetLocalNow());
// 7/18/2024 23:10:11 +02:00

Console.WriteLine(timeProvider.GetUtcNow());
// 7/18/2024 21:10:11 +00:00
```

By default, the TimeProvider has a local time zone. But you can easily override it. 

```csharp
var timeProvider = new TimeZonedTimeProvider(
    TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"));

Console.WriteLine(timeProvider.LocalTimeZone);
// (UTC-08:00) Pacific Time (US & Canada)

Console.WriteLine(timeProvider.GetLocalNow());
// 7/18/2024 12:24:13 -07:00

Console.WriteLine(timeProvider.GetUtcNow());
// 7/18/ 2024 19:24:13 + 00:00

public class TimeZonedTimeProvider(TimeZoneInfo timeZoneInfo) 
      : TimeProvider
  {
      public override TimeZoneInfo LocalTimeZone => timeZoneInfo;
  }
```

More importantly, the TimeProvider has a method for creating a timer. It returns ITimer, which was also introduced in .NET 8. 

Also, Task.Delay and Task.WaitAsync methods got the overloads to accept the TimeProvider. 

It allows you to test your timer! Let’s see how.

## FakeTimeProvider

Microsoft provides Microsoft.Extensions.TimeProvider.Testing package for testing. It contains FakeTimeProvider implementation for TimeProvider. 

To show FakeTimeProvider in action, let’s assume we have such code. 

```csharp
public class TimerSample : IAsyncDisposable
{
    private readonly TimeProvider _timeProvider;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public int Value { get; set; } = 0;

    public TimerSample(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
        _cancellationTokenSource = new CancellationTokenSource();

        Task.Run(RunInTheLoop);
    }

    private async Task RunInTheLoop()
    {
        var token = _cancellationTokenSource.Token;

        while (!token.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(1), _timeProvider);
            Value++;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync();
    }
}
```

The TimerSample class accepts TimeProvider in the constructor. When the instance is created, it starts the RunInTheLoop method with Task.Run. The RunInTheLoop method increases the Value property every second in the loop until the cancellation is requested. 

Notice that the FakeTimeProvider has been passed to the Task.Delay method! 

Let’s create a simple test. 

```csharp
[Fact]
public async Task TestTimer()
{
    FakeTimeProvider fakeTimeProvider = new(DateTimeOffset.Now);

    TimerSample timerSample = new(fakeTimeProvider);

    await Task.Delay(2000);

    Assert.Equal(0, timerSample.Value);

    await timerSample.DisposeAsync();
}
```

This test passes. It creates an instance of TimerSample and starts the RunInTheLoop method. Then, we wait for two seconds and assert the Value.

But the expected value is 0, not 2!

That’s because the time in the FakeTimeProvider is not ticking. **You control the time!**

```csharp
[Fact]
public async Task TestTimer()
{
    FakeTimeProvider fakeTimeProvider = new(DateTimeOffset.Now);

    TimerSample timerSample = new(fakeTimeProvider);

    var timeBeforeDelay = fakeTimeProvider.GetLocalNow();
    await Task.Delay(2000);
    var timeAfterDelay = fakeTimeProvider.GetLocalNow();

    Assert.Equal(timeBeforeDelay, timeAfterDelay); 
    Assert.Equal(0, timerSample.Value);

    await timerSample.DisposeAsync();
}
```

When you equal the time before the delay and after, you will find out that they are the same! 

You need to call the method Advance passing the time delta to change the time.  
  
So, the final test version is the following: 

```csharp
[Fact]
public async Task TestTimer()
{
    FakeTimeProvider fakeTimeProvider = new(DateTimeOffset.Now);

    TimerSample timerSample = new(fakeTimeProvider);

    Assert.Equal(0, timerSample.Value);

    fakeTimeProvider.Advance(TimeSpan.FromSeconds(1));

    Assert.Equal(1, timerSample.Value);

    await timerSample.DisposeAsync();
}
```

## Summary

The new time abstractions help you better manage time. You no longer need to create custom abstractions for testing purposes. Controlling time is super easy in the tests with FakeTimeProvider.

If you use older .NET versions or the .NET Framework, you can still use these abstractions by installing the NuGet package **Microsoft.Bcl.TimeProvider**. It implements netstandard 2.0. 

Post navigation

[← Previous Post](https://okyrylchuk.dev/blog/boost-your-dotnet-development-with-refit/ "Boost Your .NET Development with Refit")

[Next Post →](https://okyrylchuk.dev/blog/new-place-to-manage-packages-in-dotnet/ "A New Place To Manage Packages in .NET")

## Related Posts

[![Records in C#](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgwIiBoZWlnaHQ9IjcyMCIgdmlld0JveD0iMCAwIDEyODAgNzIwIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0iZmlsbDojY2ZkNGRiO2ZpbGwtb3BhY2l0eTogMC4xOyIvPjwvc3ZnPg==)](https://okyrylchuk.dev/blog/records-in-csharp/)
*Image: Placeholder image for "Records in C#" related post.*

### [Records in C#](https://okyrylchuk.dev/blog/records-in-csharp/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [C#](https://okyrylchuk.dev/blog/category/csharp/) / February 2, 2024

[![Pattern Matching in C#](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgwIiBoZWlnaHQ9IjcyMCIgdmlld0JveD0iMCAwIDEyODAgNzIwIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0iZmlsbDojY2ZkNGRiO2ZpbGwtb3BhY2l0eTogMC4xOyIvPjwvc3ZnPg==)](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/)
*Image: Placeholder image for "Pattern Matching in C#" related post.*

### [Pattern Matching in C#](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [C#](https://okyrylchuk.dev/blog/category/csharp/) / February 9, 2024