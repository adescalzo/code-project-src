## Summary
This article addresses how to handle timeout exceptions when using Semantic Kernel in C#. It outlines two primary methods: either by explicitly configuring a custom `HttpClient` instance with an extended timeout duration when building the Semantic Kernel, or by implementing a `ResilienceHandler` to establish a robust retry policy. The retry approach is particularly recommended for scenarios where AI platforms might experience intermittent slow responses, enhancing the application's resilience.

---

# Semantic Kernel–Change Timeout Value in C#

```markdown
**Source:** https://bartwullems.blogspot.com/2024/06/semantic-kernelchange-timeout-value-in-c.html
**Date Captured:** 2025-07-28T19:28:18.838Z
**Domain:** bartwullems.blogspot.com
**Author:** Visit profile
**Category:** programming
**Published Date:** June 24, 2024
```markdown

---

If you are new to [Semantic Kernel](https://bartwullems.blogspot.com/2024/06/debugging-semantic-kernel-in-c.html), I would point you to one of my earlier posts. In this post I want to show how you can change the timeout values when using Semantic Kernel.

The power of Semantic Kernel is that it gives you the ability to interact with multiple (large language) models in a uniform way. You interact using C#, Java or Python with the Semantic Kernel SDK and behind the scenes it will do the necessary API calls to OpenAI, Azure OpenAI, Hugging Face or a local OpenAI compatible tool like [Ollama](https://bartwullems.blogspot.com/2024/04/running-large-language-models-locally.html).

![](https://blogger.googleusercontent.com/img/b/R29vZ2xl/AVvXsEiWFsLFdmE_01P_nO3GnheogldcvH7lrpFAVEAN9jdM15GKQ8jtZz-J4B6dsQ6b93C5uCSRQergmkRJ0XBLgNp30dxXFlK6DL8Z7KMHuQUtAlI3bxVb6h92Cg2eWOX8Wm6ZMh2yaJfXmWiDWkgmYwbQaHniw8YVU4px7vy1fvVn715K2r1i7aPoXYYlZIG/s16000/SemanticKernel1.png)

Of course, as we are interacting with an API behind the scenes, it can happen that the API doesn’t return any results in time and that we get a timeout exception.

> The operation was cancelled because it exceeded the configured timeout.

Let me share how I fixed it…

## Use a Custom HttpClient

One option you have is to explicitly pass an `HttpClient` instance when creating the Semantic Kernel instance:

```csharp
HttpClient client = new HttpClient();
client.Timeout = TimeSpan.FromMinutes(2);

var semanticKernelBuilder = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion( // We use Semantic Kernel OpenAI API
        modelId: "phi3",
        apiKey: null,
        endpoint: new Uri("http://localhost:11434"),
        httpClient: client); // With Ollama OpenAI API endpoint
```

## Retry When a Timeout Happens

If the timeout typically happens because the AI platform that you are targeting reacts slow sometimes, it is maybe a better idea to configure a retry policy instead of changing the timeout value. This can be easily done in .NET Core by adding a `ResilienceHandler`:

```csharp
//Found here: https://github.com/microsoft/semantic-kernel/discussions/3412
builder.Services.ConfigureHttpClientDefaults(client =>
{
    client
        .AddStandardResilienceHandler()
        .Configure(o =>
        {
            // Combine checks for status codes and timeout into one delegate to set as the ShouldHandle policy
            o.Retry.ShouldHandle = args =>
            {
                // Check if the response has a status code that we should handle
                var isStatusCodeToRetry = args.Outcome.Result?.StatusCode is HttpStatusCode.Unauthorized
                                          || args.Outcome.Result?.StatusCode is HttpStatusCode.BadGateway;

                // Check if the result was a timeout (typically when the result is null and an exception is a TaskCanceledException)
                var isTimeout = args.Outcome.Exception?.InnerException is TaskCanceledException;

                // Combine the checks
                return ValueTask.FromResult(isStatusCodeToRetry || isTimeout);
            };
        });
});
```

## More Information

*   [Build resilient HTTP apps: Key development patterns - .NET | Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli&WT.mc_id=DOP-MVP-5001942)

**Labels:** [C#](https://bartwullems.blogspot.com/search/label/C%23), [Semantic Kernel](https://bartwullems.blogspot.com/search/label/Semantic%20Kernel)