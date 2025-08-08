```yaml
---
title: Building pipelines with IAsyncEnumerable in .NET
source: https://nikiforovall.blog/dotnet/2024/08/22/async-enumerable-pipelines.html?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=building-pipelines-with-iasyncenumerable&_bhlid=969da56f955149d6755538fe0012db36686586b2
date_published: 2024-08-22T03:00:00.000Z
date_captured: 2025-08-06T17:51:54.839Z
domain: nikiforovall.blog
author: Oleksii Nikiforov
category: frontend
technologies: [.NET, System.Threading.Channels, System.Linq.Async, IAsyncEnumerable, IObservable, Semantic Kernel, AnsiConsole, FileSystemWatcher, StreamWriter, System.Reactive]
programming_languages: [C#]
tags: [dotnet, async, pipelines, data-processing, streaming, linq, observable, ai, nlp, concurrency]
key_concepts: [asynchronous-programming, data-pipelines, reactive-programming, linq-operators, custom-operators, file-system-monitoring, text-summarization, cold-sequences, producer-consumer]
code_examples: true
difficulty_level: intermediate
summary: |
  This article demonstrates how to construct data processing pipelines in C# using `IAsyncEnumerable` and `System.Linq.Async`. It illustrates chaining asynchronous operations for streaming data, contrasting this approach with `System.Threading.Channels`. The content provides practical examples, including reading and processing files, integrating with `IObservable` for file system monitoring, and implementing custom operators like batching. It also showcases a more complex scenario of text summarization using the `Semantic Kernel` library. The article emphasizes the benefits of composable, readable, and maintainable pipeline stages for efficient data transformation.
---
```

# Building pipelines with IAsyncEnumerable in .NET

August 22nd, 2024

##### [Job Offloading Pattern with System.Threading.Channels. A way to deal with long-running tasks in .NET dotnet pipelines async](/dotnet/async/2024/04/21/job-offloading-pattern.html)

##### [Building pipelines with System.Threading.Channels dotnet async pipelines](/dotnet/async/2024/04/21/channels-composition.html)

## TL;DR

*   [TL;DR](#tldr)
*   [Introduction](#introduction)
*   [Examples](#examples)
    *   [Using `System.Linq.Async` operators to build a pipeline](#using-systemlinqasync-operators-to-build-a-pipeline)
    *   [Combining `IAsyncEnumerable` with `IObservable`](#combining-iasyncenumerable-with-iobservable)
    *   [Implementing reusable operators - `Batch`](#implementing-reusable-operators---batch)
    *   [Implementing reusable domain-specific operators - `TextSummarization` with Semantic Kernel](#implementing-reusable-domain-specific-operators---textsummarization-with-semantic-kernel)
*   [Conclusion](#conclusion)
*   [References](#references)

This article demonstrates how to use `IAsyncEnumerable` and `System.Linq.Async` to build pipelines in C#.

**Source code**: [https://github.com/NikiforovAll/async-enumerable-pipelines](https://github.com/NikiforovAll/async-enumerable-pipelines)

You can see all demos by running:

```
dotnet example --list
```

```
╭─────────────────────────────────────────┬────────────────────────────────────────────────────────────────────────────────────────────╮
│ Example                                 │ Description                                                                                │
├─────────────────────────────────────────┼────────────────────────────────────────────────────────────────────────────────────────────┤
│ CalculateWordCountPipeline              │ Demonstrates how to build async-enumerable pipelines based on standard LINQ operators      │
│ CalculateWordCountFileWatcherPipeline   │ Demonstrates how to combine async-enumerable pipelines with IObservable. E.g: file watcher │
│ CalculateWordCountBatchPipeline         │ Demonstrates how to use batching in async-enumerable pipelines                             │
│ TextSummarizationAndAggregationPipeline │ Demonstrates how to build custom async-enumerable operators                                │
╰─────────────────────────────────────────┴────────────────────────────────────────────────────────────────────────────────────────────╯
```

Here’s a sneak peek 👀:

```
// TextSummarizationAndAggregationPipeline
var pipeline = Directory
    .EnumerateFiles(path)
    .ToAsyncEnumerable()
    .ReportProgress()
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .SelectAwait(Summarize)
    .WriteResultToFile(path: Path.Combine(Path.GetTempPath(), "summaries.txt"))
    .ForEachAsync(x => AnsiConsole.MarkupLine($"Processed [green]{x.Name}[/]"));
```

## Introduction

Pipelines are a powerful way to process data in a streaming fashion. They are a series of stages that transform data from one form to another. In this article, we will explore how to build pipelines using `IAsyncEnumerable` and `System.Linq.Async`.

Pipelines are a common pattern in modern software development. They are used to process data in a streaming fashion, which can be more efficient than processing it all at once. Pipelines are also composable, meaning that you can combine multiple stages together to create more complex processing logic.

💡 I already describe an approach to building pipelines in my previous blog post, you might want to take a look at [Building pipelines with System.Threading.Channels](https://nikiforovall.github.io/dotnet/async/2024/04/21/channels-composition.html). Both `System.Threading.Channels` and `IAsyncEnumerable` provide powerful tools for managing asynchronous data streams in .NET. However, while `System.Threading.Channels` offers a more explicit approach to handling producer-consumer scenarios, `IAsyncEnumerable` brings a more integrated and LINQ-friendly way to work with asynchronous sequences. Understanding the strengths and nuances of each can help you choose the right tool for your specific use case.

## Examples

There are many interesting concepts that I’m going to cover in this article. Let’s start with the basics.

### Using `System.Linq.Async` operators to build a pipeline

Let’s say we want to build a pipeline that reads files from a directory, parses them, and counts the number of words in each file.

This can be illustrated as follows:

#mermaid-1754502422205{font-family:"trebuchet ms",verdana,arial,sans-serif;font-size:16px;fill:#000000;}#mermaid-1754502422205 .error-icon{fill:#552222;}#mermaid-1754502422205 .error-text{fill:#552222;stroke:#552222;}#mermaid-1754502422205 .edge-thickness-normal{stroke-width:2px;}#mermaid-1754502422205 .edge-thickness-thick{stroke-width:3.5px;}#mermaid-1754502422205 .edge-pattern-solid{stroke-dasharray:0;}#mermaid-1754502422205 .edge-pattern-dashed{stroke-dasharray:3;}#mermaid-1754502422205 .edge-pattern-dotted{stroke-dasharray:2;}#mermaid-1754502422205 .marker{fill:#000000;stroke:#000000;}#mermaid-1754502422205 .marker.cross{stroke:#000000;}#mermaid-1754502422205 svg{font-family:"trebuchet ms",verdana,arial,sans-serif;font-size:16px;}#mermaid-1754502422205 .label{font-family:"trebuchet ms",verdana,arial,sans-serif;color:#000000;}#mermaid-1754502422205 .cluster-label text{fill:#333;}#mermaid-1754502422205 .cluster-label span,#mermaid-1754502422205 p{color:#333;}#mermaid-1754502422205 .label text,#mermaid-1754502422205 span,#mermaid-1754502422205 p{fill:#000000;color:#000000;}#mermaid-1754502422205 .node rect,#mermaid-1754502422205 .node circle,#mermaid-1754502422205 .node ellipse,#mermaid-1754502422205 .node polygon,#mermaid-1754502422205 .node path{fill:#cde498;stroke:#13540c;stroke-width:1px;}#mermaid-1754502422205 .flowchart-label text{text-anchor:middle;}#mermaid-1754502422205 .node .katex path{fill:#000;stroke:#000;stroke-width:1px;}#mermaid-1754502422205 .node .label{text-align:center;}#mermaid-1754502422205 .node.clickable{cursor:pointer;}#mermaid-1754502422205 .arrowheadPath{fill:green;}#mermaid-1754502422205 .edgePath .path{stroke:#000000;stroke-width:2.0px;}#mermaid-1754502422205 .flowchart-link{stroke:#000000;fill:none;}#mermaid-1754502422205 .edgeLabel{background-color:#e8e8e8;text-align:center;}#mermaid-1754502422205 .edgeLabel rect{opacity:0.5;background-color:#e8e8e8;fill:#e8e8e8;}#mermaid-1754502422205 .labelBkg{background-color:rgba(232, 232, 232, 0.5);}#mermaid-1754502422205 .cluster rect{fill:#cdffb2;stroke:#6eaa49;stroke-width:1px;}#mermaid-1754502422205 .cluster text{fill:#333;}#mermaid-1754502422205 .cluster span,#mermaid-1754502422205 p{color:#333;}#mermaid-1754502422205 div.mermaidTooltip{position:absolute;text-align:center;max-width:200px;padding:2px;font-family:"trebuchet ms",verdana,arial,sans-serif;font-size:12px;background:hsl(78.1578947368, 58.4615384615%, 84.5098039216%);border:1px solid #6eaa49;border-radius:2px;pointer-events:none;z-index:100;}#mermaid-1754502422205 .flowchartTitleText{text-anchor:middle;font-size:18px;fill:#000000;}#mermaid-1754502422205 :root{--mermaid-font-family:"trebuchet ms",verdana,arial,sans-serif;}

Read files

Parse files

Count words

Output results

🎯 Our goal is to represent each stage of the pipeline in the code using `IAsyncEnumerable` and `System.Linq.Async`.

`System.Linq.Async` is a library that provides asynchronous versions of LINQ operators. It allows you to work with `IAsyncEnumerable` in a similar way to how you would work with `IEnumerable`. It makes it easy to build pipelines.

Basically, you have control-flow described as chain of methods calls, and you can implement each stage of the pipeline as a separate method. In my opinion, it makes the code more readable and maintainable. The benefit of this approach is once you determine the stages of the pipeline, you can implement them independently and focus on the logic of each stage.

The process of parsing files can be implemented as follows:

```
var path = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Data");

var pipeline = Directory
    .EnumerateFiles(path)
    .ToAsyncEnumerable()
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .Select(CalculateWordCount)
    .OrderByDescending(x => x.WordCount)
    .ForEachAsync(Console.WriteLine);

await pipeline;
```

Everything starts with the conversion of `IEnumerable<string>` file paths to `IAsyncEnumerable<string>`.

💡 Alternatively, we could write our own method that returns `IAsyncEnumerable`, for example, we could easily swap the local file system with Azure Blob Storage. It means we can reuse the same pipeline with different data sources.

💡 Later, we will see that not only `IEnumerable` can be converted to `IAsyncEnumerable`, but also `IObservable`.

As you can see `System.Linq.Async` provides a set of extension methods that allow you to work with `IAsyncEnumerable` in a similar way to how you would work with `IEnumerable`. The `SelectAwait` method is used to asynchronously project each element of the sequence. The `Where` method is used to filter elements based on a predicate. The `OrderByDescending` method is used to sort the elements of the sequence in descending order. The `ForEachAsync` method is used to asynchronously iterate over the sequence.

It worth to point out that `ForEachAsync` is a terminal operation that triggers the execution of the pipeline. It is important to remember that `IAsyncEnumerable` is a cold sequence, meaning that it does not start processing until you start iterating over it.

Here are the building blocks of the pipeline:

```
public static class Steps
{
    public static async ValueTask<FilePayload> ReadFile(string file)
    {
        var content = await File.ReadAllTextAsync(file);
        var name = Path.GetFileName(file);

        return new FilePayload(name, content);
    }

    public static bool IsValidFileForProcessing(FilePayload file) =>
        file is { Content.Length: > 0, Name: [.., 't', 'x', 't'] };

    public static WordCountPayload CalculateWordCount(FilePayload payload)
    {
        var words = payload.Content.Split(' ');

        return new(payload.Name, words.Length);
    }
}

public record FilePayload(string Name, string Content);
public record WordCountPayload(string Name, int WordCount);
```

Let’s see the pipeline in action:

### Combining `IAsyncEnumerable` with `IObservable`

Let’s say we want to use simple file watcher to monitor changes in the directory and trigger the pipeline when a new file is created or an existing file is modified.

```
var fileWatcher = CreateFileObservable(path);

var pipeline = fileWatcher
    .TakeUntil(DateTimeOffset.Now.AddSeconds(15))
    .ToAsyncEnumerable()
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .Select(CalculateWordCount)
    .ForEachAsync(Console.WriteLine);
```

In this example, we use `IObservable` to monitor changes in the directory. We create an observable sequence of file paths using the `CreateFileObservable` method. We then use the `TakeUntil` operator to limit the duration of the sequence to 15 seconds. We convert the observable sequence to an asynchronous enumerable sequence using the `ToAsyncEnumerable` method. We then apply the same pipeline as before to process the files.

The `CreateFileObservable` method is implemented as follows:

```
static IObservable<string> CreateFileObservable(string path) =>
    Observable.Create<string>(observer =>
    {
        var watcher = new FileSystemWatcher(path)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = "*.*",
            EnableRaisingEvents = true
        };

        void onChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                observer.OnNext(e.FullPath);
            }
            catch (Exception ex)
            {
                observer.OnError(ex);
            }
        }

        watcher.Created += onChanged;
        watcher.Changed += onChanged;

        return () =>
        {
            watcher.Created -= onChanged;
            watcher.Changed -= onChanged;
            watcher.Dispose();
        };
    });
```

Let’s see the pipeline in action:

In the demo below, I’m appending “ word” to the end of the file content to trigger the pipeline.

### Implementing reusable operators - `Batch`

Let’s say we want to batch the processing of files to improve performance. We can implement a custom operator called `Batch` that groups elements of the sequence into batches of a specified size.

In the example above, we are reading files in batches in parallel. We are using the `Batch` operator to group files into batches of size 2. We then process each batch in parallel using the `ProcessEachAsync` method.

```
const int batchSize = 2;

var pipeline = Directory
    .EnumerateFiles(path)
    .ToAsyncEnumerable()
    .Batch<string, FilePayload>(batchSize)
    .ProcessEachAsync(ReadFile)
    .Where(IsValidFileForProcessing)
    .Select(CalculateWordCount)
    .OrderByDescending(x => x.WordCount)
    .ForEachAsync(Console.WriteLine);

await pipeline;
```

In the example above, we are reading files in batches in parallel. We are using the `Batch` operator to group files into batches of size 2. We then process each batch in parallel using the `ProcessEachAsync` method.

💡 I will leave the implementation of the `Batch` operator as an exercise for the reader. Please check source code for the full implementation. [https://github.com/NikiforovAll/async-enumerable-pipelines/blob/main/Pipelines.Core/PipelineBuilderExtensions.cs](https://github.com/NikiforovAll/async-enumerable-pipelines/blob/main/Pipelines.Core/PipelineBuilderExtensions.cs)

Let’s see the pipeline in action:

### Implementing reusable domain-specific operators - `TextSummarization` with Semantic Kernel

To demonstrate something more complex, let’s say we want to summarize the content of the files using the [Semantic Kernel](https://github.com/microsoft/semantic-kernel) library. Summarization is a common task in natural language processing (NLP) that involves generating a concise representation of a text document.

```
var pipeline = Directory
    .EnumerateFiles(path)
    .ToAsyncEnumerable()
    .ReportProgress()
    .SelectAwait(ReadFile)
    .Where(IsValidFileForProcessing)
    .SelectAwait(Summarize)
    .WriteResultToFile(path: Path.Combine(Path.GetTempPath(), "summaries.txt"))
    .ForEachAsync(x => AnsiConsole.MarkupLine($"Processed [green]{x.Name}[/]"));
```

In the example above, we are reading files, summarizing their content, and writing the results to a file. We are using the `ReportProgress` operator to report progress as each file is processed. We are using the `Summarize` operator to summarize the content of each file. We are using the `WriteResultToFile` operator to write the results to a file.

Before we move forward, let’s see how the pipeline works in the demo below:

Now, we are ready to move forward and see the details of the implementation.

The `Summarize` method is implemented as follows:

```
async ValueTask<SummarizationPayload> Summarize(FilePayload file)
{
    var prompt = """
        
        Please summarize the content above in 20 words or less:

        The output format should be: [title]: [summary]
        """;

    var result = await kernel.InvokePromptAsync(prompt, new KernelArguments() { ["input"] = file.Content });

    return new(file.Name, result.ToString());
}
```

Than we want to write the results to a file:

```
public static async IAsyncEnumerable<SummarizationPayload> WriteResultToFile(
    this IAsyncEnumerable<SummarizationPayload> values,
    string path
)
{
    const int batchSize = 10;

    using var streamWriter = new StreamWriter(path, append: true);

    await foreach (var batch in values.Buffer(batchSize))
    {
        foreach (var value in batch)
        {
            await streamWriter.WriteLineAsync(value.Summary);

            yield return value;
        }

        await streamWriter.FlushAsync();
    }

    AnsiConsole.MarkupLine($"Results written to [green]{path}[/]");
}
```

💡 Note, `IAsyncEnumerable` is pull-based model. With this approach, each summary is read individually and appended to the end of the file. This means that results are continuously saved as each batch is processed by calling the `FlushAsync` method.

The `ReportProgress` method is quite interesting because it eagerly reads all elements of the sequence to determine the total count. It then reports progress as each element is processed.

```
public static async IAsyncEnumerable<string> ReportProgress(this IAsyncEnumerable<string> values)
{
    var totalCount = await values.CountAsync();

    await foreach (var (value, index) in values.Select((value, index) => (value, index)))
    {
        yield return value;

        AnsiConsole
            .Progress()
            .Start(ctx =>
            {
                var task = ctx.AddTask($"Processing - {Path.GetFileName(value)}", true, totalCount);
                task.Increment(index + 1);
                task.StopTask();
            });
    }
}
```

💡 This is a good demonstration of leaky abstractions. Not all data sources can provide the full sequence immediately, so we need to be careful.

## Conclusion

That is it! 🙌 We have seen how to build pipelines using `IAsyncEnumerable` and `System.Linq.Async`. I hope you found this article helpful. If you have any questions or comments, please feel free to leave them below.

## References

*   [https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8](https://learn.microsoft.com/en-us/archive/msdn-magazine/2019/november/csharp-iterating-with-async-enumerables-in-csharp-8)
*   [https://github.com/dotnet/reactive](https://github.com/dotnet/reactive)
*   [https://github.com/microsoft/semantic-kernel/tree/main/dotnet/notebooks](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/notebooks)

*   [dotnet (56)](https://nikiforovall.github.io/categories.html#dotnet-ref)

*   [dotnet (61) ,](https://nikiforovall.github.io/tags.html#dotnet-ref)
*   [async (5) ,](https://nikiforovall.github.io/tags.html#async-ref)
*   [pipelines (4)](https://nikiforovall.github.io/tags.html#pipelines-ref)

---

#### Share Post

[Twitter](http://twitter.com/share?&via=nikiforovall)

![](//www.gravatar.com/avatar/e86fa0938976c38907a302ecb208f011)

#### Oleksii Nikiforov

Jibber-jabbering about programming and IT.

*   [← Previous](https://nikiforovall.github.io/dotnet/2024/08/17/result-endpoints.html "Unlocking the Power of TypedResults in Endpoints: A Consistent Approach to Strongly Typed APIs in .NET")
*   [Next →](https://nikiforovall.github.io/dotnet/ai/2024/09/04/typical-rag-dotnet.html "Typical RAG Implementation Using Semantic Kernel, Kernel Memory, and Aspire in .NET")

---