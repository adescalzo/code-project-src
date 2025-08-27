```yaml
---
title: "Linux process priorities for C# devs | by Olivier Coanet | Medium"
source: https://medium.com/@ocoanet/linux-process-priorities-for-c-devs-9ed9d9cc4ba1
date_published: 2024-12-28T13:39:47.388Z
date_captured: 2025-08-12T18:17:42.778Z
domain: medium.com
author: Olivier Coanet
category: frontend
technologies: [.NET, Linux, Windows, BenchmarkDotnet, systemd, htop, ps, nice, renice, chrt, Process.PriorityClass, Thread.Priority, ProcessThread.PriorityLevel, setpriority, sched_setparam, libc]
programming_languages: [C#]
tags: [linux, dotnet, process-management, thread-management, performance, scheduling, operating-system, csharp, systemd, kernel]
key_concepts: [process-priority, thread-priority, linux-scheduling, windows-scheduling, niceness, real-time-priorities, scheduling-policies, priority-inheritance, capabilities]
code_examples: false
difficulty_level: intermediate
summary: |
  This article delves into the complexities of managing process and thread priorities for C# applications running on Linux, highlighting the significant differences from Windows. It explains how the .NET priority API, designed for Windows, behaves unexpectedly on Linux due to the latter's thread-centric scheduling model, which utilizes niceness and real-time priorities. The author details the mapping of .NET API calls to underlying Linux system functions and the limitations encountered, such as the ineffectiveness of `Thread.Priority` without custom scheduling policies. The content also covers Linux-specific concepts like scheduling policies and required privileges. Ultimately, it provides practical guidelines for C# developers, recommending direct use of Linux commands or libc functions for more predictable priority control.
---
```

# Linux process priorities for C# devs | by Olivier Coanet | Medium

# Linux process priorities for C# devs

![Olivier Coanet](https://miro.medium.com/v2/resize:fill:64:64/0*Fs8yhMphtuPBRirt.)

Olivier Coanet

Follow

10 min read

·

Dec 27, 2024

15

Listen

Share

More

Press enter or click to view image in full size

![A terminal screenshot displaying process and thread information on a Linux system. The output shows columns for PID, CPU, PRI (priority), NI (niceness), VIRT (virtual memory), RES (resident memory), SHR (shared memory), S (status), CPU%, MEM%, TIME+, and Command. Several lines are highlighted, showing various .NET related threads such as ".NET DebugPipe", ".NET Debugger", ".NET EventPipe", ".NET Finalizer", ".NET SigHandler", ".NET SynchManag", "T1", "T2", and ".NET Server GC". Some entries in the 'NI' column show negative values like -11 and -6, indicating adjusted niceness levels for specific threads.](https://miro.medium.com/v2/resize:fit:700/1*bXxB65kJR8iFvdRzlvjeiw.png)

# Context

Although the .NET runtime can run on Linux since 2016, the support of the Linux priority model is minimal, and using the .NET priority API on Linux can produce surprising results. Also, the API behavior on Linux is poorly documented. The Microsoft documentation does not mention anything about Linux, and searching for _“dotnet thread priority linux”_ on Google returns only a handful of relevant results, which are almost exclusively SO questions or GitHub issues. I discovered these problems while migrating a high-performance application to Linux, and I decided to write this article as a compilation of the knowledge I wish I had before the migration.

# Why you should be concerned about priorities

Let’s start with a short example on Windows. I created a very [naive program](https://github.com/ocoanet/ProcessPrioritiesPlayground/blob/c4e903887ca38e2a32926cbb043be034a8a7c71b/PerfTester/Program.cs) that spawns two multi-threaded and CPU intensive sub-processes. These two sub-processes run the same code but one has a high priority while the other has a normal priority.

```
> PerfTester.exe
Normal priority - Process starting
High priority - Process starting
High priority - Process completed in 7.8s
Normal priority - Process completed in 15.3s
```

As you can see, in a loaded system, increasing a process or a thread priority can have a huge impact on the execution time. Of course it is not a silver bullet, and you cannot set a high priority on all processes and expect every process to run faster. It is only effective if you can identify a few critical processes in the machine or a few critical threads in the process.

In a quiet system, increasing a process priority is less impactful, but it can still help to reduce context switches or CPU migrations. In fact, [BenchmarkDotnet](https://github.com/dotnet/BenchmarkDotNet) always tries to increase the process and thread priorities before running benchmarks to increase results stability.

# The .NET priority API

.NET exposes process and thread level priorities through the [Process.PriorityClass](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process.priorityclass?view=net-9.0) and [Thread.Priority](https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.priority?view=net-9.0) properties. You can read or write these properties at runtime:

```csharp
var process = Process.GetCurrentProcess();
Console.WriteLine($"Current PriorityClass: {process.PriorityClass}");

process.PriorityClass = ProcessPriorityClass.High;
Console.WriteLine($"New PriorityClass: {process.PriorityClass}");

var thread = new Thread(SampleThreadProc)
{
  Name = "SampleThread",
  Priority = ThreadPriority.AboveNormal,
};

thread.Start();
```

The thread priority is also exposed in the [ProcessThread.PriorityLevel](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.processthread.prioritylevel?view=net-9.0) property, which surprisingly uses a different priority enum (`ThreadPriorityLevel` instead of `ThreadPriority`).

The .NET API matches the Windows scheduling priorities. On Windows, each process has a priority class, each thread has a priority level, and the resulting thread priority, also called [base priority](https://learn.microsoft.com/en-us/windows/win32/procthread/scheduling-priorities#base-priority), is a combination of both. The base priority is a number ranging from 1 (lowest priority) to 31 (highest priority).

> On Windows, the operating system can decide to [temporarily boost](https://learn.microsoft.com/en-us/windows/win32/procthread/priority-boosts) the priority of a thread. So, the effective thread priority can be different from the base priority.

The .NET API is so bound to the Windows API that the `ProcessPriorityClass` and `ThreadPriorityLevel` enum values are the same as the corresponding Win32 constants.

When using the .NET priority API on Windows, you could expect that:

*   The effective thread priority is a combination of the process priority class, the thread priority level, and an optional system-defined priority boost.
*   Changing the process priority class impacts all threads (previously created and future threads).
*   There is no priority inheritance, processes and threads always start with a “Normal” priority, regardless of the parent priority.

> There is an exception for process priorities. Although the Windows [documentation on inheritance](https://learn.microsoft.com/en-us/windows/win32/procthread/inheritance) specifies that “The child process does not inherit the following: Priority class (…)”, the Idle and BelowNormal priority classes are actually inherited. I suppose it prevents processes from “escaping” low priorities by starting child processes.

# Priority of the .NET runtime threads

Configuring the priority of the threads you create and own might be useful. However, you should probably avoid changing the .NET runtime threads priorities. The .NET runtime threads include management threads like the GC, finalizer, or EventPipe threads, but also obviously the thread-pool threads.

> You could potentially temporarily increase a thread-pool thread priority level by using a try / finally pattern that restores the original priority level at the end of your computation. Yet, I never applied this pattern myself.

Most of the threads created by the runtime have a normal priority, with a few exceptions like the finalizer thread or the server-GC threads, which have a higher priority on Windows.

# The Linux priority model

## Processes and threads

The Linux scheduler operates on threads, which are called tasks in the Linux terminology (not to be confused with .NET tasks of course). There is no concept of process level priorities on Linux. If a tool shows the priority of a process, it is most likely the priority of the main thread. However, the thread priority is inherited on Linux, so if you configure the startup priority of your process, all your application threads will have the same priority.

> Updating your “process” priority at runtime can be problematic in .NET. Even if you change the main thread priority at the very beginning of your program, the .NET runtime has already created many management threads which are left unaffected by the priority change. However, Linux commands and libc functions can target [process groups](https://en.wikipedia.org/wiki/Process_group). If you use a process group as the target of your priority change, then all the threads will be impacted.

## Linux priorities

The most common way to change a thread priority is to adjust its nice value (or niceness). The nice value is a user-space priority, ranging from -20 (highest priority) to 19 (lowest priority). You can set the niceness of your process before start using the [nice](https://man7.org/linux/man-pages/man1/nice.1.html) command:

```bash
$ nice -n priority program arguments
```

You can also change the niceness of your threads at runtime using [setpriority](https://man7.org/linux/man-pages/man2/setpriority.2.html) or the [renice](https://man7.org/linux/man-pages/man1/renice.1.html) command. In both cases, you can either:

*   Change the priority of a single thread by passing the thread ID: `setpriority(PRIO_PROCESS, threadId, priority)` or `renice -n priority threadId`
*   Change the priority of all threads by using process groups: `setpriority(PRIO_PGRP, processId, priority)` or `renice -n priority -g processId`

> Please note that although the process group option is interesting, it is clearly not equivalent to changing a process priority class on Windows, because it will override every thread priority that was previously set.

Under the hood, the Linux kernel uses a kernel-space priority ranging from 0 (highest priority) to 139 (lowest priority). The nice priorities from -20 to 19 are mapped to the kernel priorities from 100 to 139, and the range of 0 to 99 is used for real-time tasks.

You can also change the real-time priority using [sched\_setparam](https://man7.org/linux/man-pages/man2/sched_setparam.2.html) or the [chrt](https://man7.org/linux/man-pages/man1/chrt.1.html) command. However, changing the real-time priority of your threads is only effective with specific scheduling policies.

Theoretically, the operating system could also decide to adjust the kernel-space priority, just like Windows does with the priority boosts. However, I have never noticed that myself when looking at the priority values with `htop` or `ps`.

> `htop` displays the nice value normally, but the kernel priority is displayed as `20 + nice value` for user-space priorities, and simply `RT` for real-time priorities. `ps` can actually display the real-time priority with the `-l` option, but the value is shifted by 40 for historical reasons. This is very confusing to me.

## Scheduling policies

The Linux kernel supports multiple scheduling policies which have different ways to handle priorities.

The default scheduling policy is `SCHED_OTHER`, sometimes called `SCHED_NORMAL`. It is a non-real-time policy based on a [fair scheduler](https://lwn.net/Articles/925371/) and it only supports user-space priorities, i.e.: niceness.

Linux also provides real-time scheduling policies like `SCHED_FIFO` or `SCHED_RR`. These policies support real-time priorities, which are mapped to the kernel priorities from 0 to 99, and they ignore niceness.

Digging into the details of the scheduling policies is clearly out of the scope of this article. For now, you only need to remember that configuring priorities is not straightforward and even the ubiquitous nice command might have no effect on your environment.

## Permissions

Increasing the priorities on Linux, either by using the priority commands, or by using the related libc functions, requires specific privileges. Commands can be simply run with `sudo`. Yet, you should probably not run your applications with `sudo`, so you will need to set up your environment correctly to invoke functions like `setpriority` or to use the .NET priority API members that rely on `setpriority`. For example, if you use systemd services, you will need to configure `AmbientCapabilities=CAP_SYS_NICE` to allow your application to increase the nice priority.

On Linux, the .NET runtime finalizer and server-GC threads have a normal priority, whether your application runs with a non real-time or a real-time scheduling policy, and even if your application runs with the required privileges. I suppose that the vast majority of the .NET applications run without elevated privileges, so increasing the priority of the runtime threads would not be particularly useful.

# Effect of the .NET priority API on Linux

It should be already clear at this point that there is an important design mismatch between the Windows and Linux priority models. There is no obvious or right way to implement the .NET priority API on Linux and the runtime developers probably had a hard time carrying it out.

## Process-level priority

On Linux, the `Process.PriorityClass` property is implemented by calling `setpriority(PRIO_PROCESS, processId, priority)` ([source](https://github.com/dotnet/runtime/blob/f3d43efa514a87f04aa994749409297e8f3e94e2/src/libraries/System.Diagnostics.Process/src/System/Diagnostics/Process.Unix.cs#L298)). So, it will change the niceness of the main thread, which is quite different from changing a process-level priority. The priority of the previously created threads will remain unaffected (including .NET management threads), and new threads will only be affected if they are started from the main thread.

It might have been possible to call `setpriority(PRIO_PGRP, processId, priority)` instead. But then, all the program threads would be affected, which could override previously configured thread priorities. In my view, it might still have been slightly more appropriate, but it is obviously very debatable.

## Thread-level priority

Implementing the process-level priority API on an OS that does not support process-level priorities is clearly challenging. Implementing the thread-level priority API could possibly be more straightforward. Could the .NET API have used the ubiquitous, user-space priority type which is supported by the default scheduling policy? Well, it would have been nice (no pun intended). On Linux, `Thread.Priority` is not based on nicencess, it is implemented by calling `sched_setparam` ([source](https://github.com/dotnet/runtime/blob/072cbae7b4fc5198476a94fa6673b4076c65e7cc/src/coreclr/pal/src/thread/thread.cpp#L1043-L1221)), which is only supported by real-time scheduling policies. So, unless your program runs with a custom scheduling policy, setting the `Thread.Priority` property has no effect. There is an [open GitHub issue](https://github.com/dotnet/runtime/issues/91165) on this subject, so the implementation might change in the future.

## Priority mapping

Let me give you an easy riddle: is there any situation where this code could have an effect?

```csharp
var process = Process.GetCurrentProcess();
if (process.PriorityClass == ProcessPriorityClass.High)
  process.PriorityClass = ProcessPriorityClass.High;
```

The answer is indeed: on Linux, more precisely if your main thread nice priority is between -12 and -14. In this situation, the effect of this code will be to “normalize” your process main thread priority to -11. Naturally, as was previously discussed, all other threads will keep their initial priority.

The explanation is quite simple. Here is how niceness is mapped to `ProcessPriorityClass`:

```
return
  pri < -15 ? ProcessPriorityClass.RealTime :
  pri < -10 ? ProcessPriorityClass.High :
  pri < -5 ? ProcessPriorityClass.AboveNormal :
  pri == 0 ? ProcessPriorityClass.Normal :
  pri <= 10 ? ProcessPriorityClass.BelowNormal :
  ProcessPriorityClass.Idle;
```

And here is how the `ProcessPriorityClass` is mapped to niceness:

```csharp
switch (value)
{
    case ProcessPriorityClass.RealTime: pri = -19; break;
    case ProcessPriorityClass.High: pri = -11; break;
    case ProcessPriorityClass.AboveNormal: pri = -6; break;
    case ProcessPriorityClass.BelowNormal: pri = 10; break;
    case ProcessPriorityClass.Idle: pri = 19; break;
}
```

This kind of issue is inherent to the mismatch between the .NET process priority enum which has only 6 values and the niceness which has 40 possible values.

There is also a similar mismatch between the .NET thread priority enum which has 5 values and the Linux real-time priority which has 100 possible values.

There is nothing wrong in the way the priority values are converted in the .NET API. However, you should probably keep the mapping rules in mind when using the priority API on Linux.

## Recap

When using the .NET priority API on Linux, you could expect that:

*   Changing the current process priority class updates the priority of the main thread. It will not affect other threads, but it will affect future threads if they are created by the main thread.
*   Reading the current process priority class will give you an imprecise value because multiple niceness values are mapped to the same .NET priority level.
*   Changing the threads priorities has no effect, unless your program runs with a custom scheduling policy.
*   Your application needs specific privileges to increase priorities.
*   Configuring both process and thread level priorities is not possible because process-level priorities do not exist on Linux.
*   On Linux, if thread A starts thread B, then thread B will start with the same priority as thread A. On Windows, new threads always start with a Normal priority.
*   On Linux, all runtime threads have a normal priority, while on Windows the finalizer and server-GC threads have a higher priority.

# Opinionated thoughts and guidelines

I do not think there is anything inherently wrong in the way the .NET priority API is implemented on Linux. The .NET priority API was designed for Windows and there is no perfect way to implement it on Linux. However, I wonder if it might have been more explicit to mark the whole API as Windows-only, and to create a dedicated Linux-only API for Linux.

Anyway, if you need to configure priorities on Linux, here are a few personal guidelines:

*   Avoid updating the `Process.PriorityClass` property at runtime on Linux.
*   Avoid updating the `Thread.Priority` property at runtime on Linux.
*   Consider starting critical processes with a higher priority using nice.

> Of course, using the nice command directly might not be the recommended way depending on your setup. For example, if your application is deployed as a systemd service, you should use the NICE property instead.

*   Consider explicitly calling the libc functions to update your thread priorities at runtime on Linux.

# Final words

I dug into these issues because I had to migrate a high-performance application to Linux. I was relatively lucky because this application uses a very thin abstraction called “performance profiles”, which allows to tune performance settings like priorities and CPU affinities in a centralized way. So, in the end, I only had to write custom logic for Linux on the performance profile code. For now, I strongly believe that identifying the Linux platform and implementing custom logic is cleaner and more explicit than relying on the questionable effects of the .NET priority API.

Speaking of CPU affinities, I suppose it might be a good subject for another article!

Many thanks [@rbouallou](https://x.com/rbouallou) and [@Lucas\_Trz](https://x.com/Lucas_Trz) for the reviews.