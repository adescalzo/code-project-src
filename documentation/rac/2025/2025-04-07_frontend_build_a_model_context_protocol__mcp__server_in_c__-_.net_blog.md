```yaml
---
title: Build a Model Context Protocol (MCP) server in C# - .NET Blog
source: https://devblogs.microsoft.com/dotnet/build-a-model-context-protocol-mcp-server-in-csharp/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-177
date_published: 2025-04-07T17:05:00.000Z
date_captured: 2025-08-06T17:45:09.208Z
domain: devblogs.microsoft.com
author: James Montemagno
category: frontend
technologies: [Model Context Protocol (MCP), MCP C# SDK, .NET, .NET MAUI, ASP.NET Core, Visual Studio Code (VS Code), NuGet, Microsoft.Extensions.Hosting, Microsoft.Extensions.Logging, Microsoft.Extensions.AI, GitHub Copilot, Docker, HttpClient, System.Text.Json, Mermaid, HTTP, Server-Sent Events (SSE), Azure Functions, Azure App Service, Azure Container Apps]
programming_languages: [C#, Bash, JSON, XML, XAML]
tags: [ai, dotnet, csharp, model-context-protocol, sdk, server-development, vs-code, github-copilot, containerization, api-integration]
key_concepts: [model-context-protocol, ai-integration, tooling, dependency-injection, server-sent-events, containerization, api-integration, code-generation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Model Context Protocol (MCP) and demonstrates how to build an MCP server using the C# SDK. It guides developers through setting up a basic console application, defining custom tools with C# methods, and configuring the server for local testing with Visual Studio Code and GitHub Copilot. The post further illustrates integrating the MCP server with external data and APIs, showcasing how AI agents can leverage these tools for tasks like data retrieval and code generation. It also covers publishing the MCP server as a container image and discusses Server-Sent Events (SSE) for efficient data streaming.]
---
```

# Build a Model Context Protocol (MCP) server in C# - .NET Blog

# Build a Model Context Protocol (MCP) server in C#

In the rapidly evolving world of AI and machine learning, effective communication between models and applications is critical. The Model Context Protocol (MCP) is a standardized protocol designed to facilitate this communication by providing a structured way to exchange context and data between AI models and their clients. Whether you’re building AI-powered applications or integrating multiple models into a cohesive system, MCP ensures interoperability and scalability. For developers using tools like Visual Studio Code, you can now integrate and leverage MCP servers in your development flow and it makes it easy to build and test MCP servers on your local machine.

With the release of the MCP C# SDK, developers can now easily build both servers and clients that leverage this protocol. This SDK simplifies the implementation process, allowing you to focus on your application’s unique features rather than the complexities of protocol handling. Additionally, the SDK includes support for consuming MCP servers, enabling developers to create robust client applications that interact seamlessly with MCP servers.

In this blog post, we’ll explore how you can use the C# SDK to create your own MCP server and client applications.

**Note**

The MCP C# SDK is in preview and APIs may change. We will continuously update this blog as the SDK evolves.

## Getting Started Building an MCP server

The MCP C# SDK is distributed as NuGet packages that you can integrate into a simple console application. Let’s start out by creating our very first MCP server by creating a new console app:

```bash
dotnet new console -n MyFirstMCP
```

Now, let’s add a few basic NuGet packages for the MCP C# SDK and to host our server with `Microsoft.Extensions.Hosting`.

```bash
dotnet add package ModelContextProtocol --prerelease
dotnet add package Microsoft.Extensions.Hosting
```

The `ModelContextProtocol` package gives access to new APIs to create clients that connect to MCP servers, creation of MCP servers, and AI helper libraries to integrate with LLMs through `Microsoft.Extensions.AI`.

## Starting up our server

Let’s update our `Program.cs` with some basic scaffolding to create our MCP server, configure standard server transport, and tell our server to search for **Tools** (or available APIs) from the running assembly.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using System.ComponentModel;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
```

With this code we are now ready to build our first tool that we want to expose to our MCP server.

## Defining our first tool

Let’s create the most basic tool to just repeat back what we ask it to do. We first define our class that will contain functions that are exposed as tools.

```csharp
[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("Echoes the message back to the client.")]
    public static string Echo(string message) => $"Hello from C#: {message}";

    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}
```

In our startup code, the `WithToolsFromAssembly` will scan the assembly for classes with the `McpServerToolType` attribute and register all methods with the `McpServerTool` attribute. Notice that the `McpServerTool` has a **Description** which will be fed into any client connecting to the server. This description helps the client determine which tool to call.

## Configure and run in VS Code

With this minimal code, our MCP server is ready for testing! If you haven’t tried out MCP support in VS Code, check out [this video](https://www.youtube.com/watch?v=iS25RFups4A) for a guided tour. To run our project locally, we just need to add a new server in our **mcp.json** file in your **.vscode** folder or your user settings:

```json
{
    "inputs": [],
    "servers": {
        "MyFirstMCP": {
            "type": "stdio",
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "D:\\source\\MyFirstMCP\\MyFirstMCP\\MyFirstMCP.csproj"
            ]
        }
    }
}
```

When we go into GitHub Copilot and toggle on Agent mode, we will see our new tool configured:

![VS Code drop down showing 2 tools available for the MCP server, "Echo" and "ReverseEcho", indicating they are available for chat.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2025/04/echotools.png)

Opening GitHub Copilot’s Agent mode we can now ask it to reverse a message for us. We will be prompted for permission to execute the call to the tool:

![GitHub Copilot prompt asking for permission to run the "ReverseEcho" tool, showing the input message "The new C# SDK for MCP is awesome!".](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2025/04/reverseecho.png) Selecting **Continue** will run the tool and pass the message to our MCP server to execute:

![GitHub Copilot displaying the output of the "ReverseEcho" tool, showing the input message reversed.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2025/04/reversedmessage.png)

## Integrating with our own data and APIs

MCP servers show their power when they integrate into an existing API or service to query real data that can be used by the clients. There is a [growing list of servers](https://github.com/modelcontextprotocol/servers) available to use in clients including ones I use every day such as [Git](https://github.com/modelcontextprotocol/servers/blob/main/src/git), [GitHub](https://github.com/modelcontextprotocol/servers/blob/main/src/github), [Playwright](https://github.com/microsoft/playwright-mcp), and the [Filesystem](https://github.com/modelcontextprotocol/servers/blob/main/src/filesystem). So, let’s extend our MCP server to connect with an API, take query parameters, and respond back to data. If you have followed me at all, you know I love making demos about monkeys and I thought having a [Monkey MCP server](https://github.com/jamesmontemagno/MonkeyMCP) available to me at all times would be useful. So, the first thing I did was integrate a simple service that queries my monkey database to return a list of monkeys or information about a specific kind:

```csharp
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MyFirstMCP;

public class MonkeyService
{
    private readonly HttpClient httpClient;
    public MonkeyService(IHttpClientFactory httpClientFactory)
    {
        httpClient = httpClientFactory.CreateClient();
    }

    List<Monkey> monkeyList = new();
    public async Task<List<Monkey>> GetMonkeys()
    {
        if (monkeyList?.Count > 0)
            return monkeyList;

        var response = await httpClient.GetAsync("https://www.montemagno.com/monkeys.json");
        if (response.IsSuccessStatusCode)
        {
            monkeyList = await response.Content.ReadFromJsonAsync(MonkeyContext.Default.ListMonkey) ?? [];
        }

        monkeyList ??= [];

        return monkeyList;
    }

    public async Task<Monkey?> GetMonkey(string name)
    {
        var monkeys = await GetMonkeys();
        return monkeys.FirstOrDefault(m => m.Name?.Equals(name, StringComparison.OrdinalIgnoreCase) == true);
    }
}

public partial class Monkey
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Details { get; set; }
    public string? Image { get; set; }
    public int Population { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

[JsonSerializable(typeof(List<Monkey>))]
internal sealed partial class MonkeyContext : JsonSerializerContext {

}
```

I can then register this with the built in .NET dependency service so I can use it later:

```csharp
builder.Services.AddHttpClient();
builder.Services.AddSingleton<MonkeyService>();
```

This service could call your existing APIs, query a database, process data, or anything else. MCP servers are often configured to take in access tokens or additional parameters on startup so your code has the necessary information it needs to call services. In this case I am just reading from my sample monkey data. To make these available as tools, all I need to do is to define a new `McpServerToolType` and setup a few new `McpServerTool` methods that call into this service:

```csharp
using System;
using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MyFirstMCP;

[McpServerToolType]
public static class MonkeyTools
{
    [McpServerTool, Description("Get a list of monkeys.")]
    public static async Task<string> GetMonkeys(MonkeyService monkeyService)
    {
        var monkeys = await monkeyService.GetMonkeys();
        return JsonSerializer.Serialize(monkeys);
    }

    [McpServerTool, Description("Get a monkey by name.")]
    public static async Task<string> GetMonkey(MonkeyService monkeyService, [Description("The name of the monkey to get details for")] string name)
    {
        var monkey = await monkeyService.GetMonkey(name);
        return JsonSerializer.Serialize(monkey);
    }
}
```

In the above code, I am simply returning the data as JSON, however you can format it in different ways for the LLM to process.

We can now restart our MCP server inside of VS Code and start to try it out! The power is when combining the power of GitHub Copilot and the models it calls to transform the data and apply it to software development. For example I may ask it for a list of monkeys and display it as table.

![GitHub Copilot displaying a table of monkeys with columns for Name, Location, Population, and Details, generated from the MCP server.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2025/04/monkeylist.png)

Or we could ask for information on a specific monkey and to generate a Mermaid diagram for the data type:

![GitHub Copilot generating a Mermaid diagram representing the structure of a "Monkey" data type, including properties like Name, Location, Population, and Image.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2025/04/mermaiddiagram.png)

Better yet, if I was building a .NET MAUI app, I could have it generate the XAML and the code needed for the data I was exploring:

![GitHub Copilot generating XAML code for a .NET MAUI application, displaying details of a monkey including its name, location, and image.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2025/04/mauicode.png)

## Publish your MCP server

.NET makes it simple to easily create container images for any .NET app. All that needs to be done is add the necessary configuration into the project file:

```xml
<PropertyGroup>
    <EnableSdkContainerSupport>true</EnableSdkContainerSupport>
    <ContainerRepository>jamesmontemagno/monkeymcp</ContainerRepository>
    <ContainerFamily>alpine</ContainerFamily>
    <RuntimeIdentifiers>linux-x64;linux-arm64</RuntimeIdentifiers>
</PropertyGroup>
```

Here, we are going to use the **alpine** container family to create a nice small image and also specify multiple runtime identifiers so users have an optimized image regardless if they are on x64 or an arm64 based machine. The .NET SDK has the ability to create the images by running the `dotnet publish /t:PublishContainer` command. Since we have multiple runtime identifiers specified two different images locally will be created. If we want to take these images and upload them, we are able to do it all from the CLI by passing in the specific container register to push to:

```bash
dotnet publish /t:PublishContainer -p ContainerRegistry=docker.io
```

Now a combined image will be sent to docker.io in this case, and then can be configured in VS Code or other clients that work with MCP servers.

```json
{
    "inputs": [],
    "servers": {
        "monkeymcp": {
            "command": "docker",
            "args": [
                "run",
                "-i",
                "--rm",
                "jamesmontemagno/monkeymcp"
            ],
            "env": {}
        }
    }
}
```

The great part about this, is that automatically the correct image will be pulled based on the machine type the user is on. Learn more about this in the [containerizing .NET apps documentation](https://learn.microsoft.com/dotnet/core/containers/publish-configuration).

## Server-Sent Events (SSE)

SSE transport enables server-to-client streaming with HTTP POST requests for client-to-server communication. With the MCP C# SDK, implementing SSE in your MCP server is straightforward. The SDK supports configuring server transports to handle streaming data efficiently to connected clients. You can see an implementation of an [SSE MonkeyMCP server on GitHub](https://github.com/jamesmontemagno/monkeymcp) and on the [MCP C# SDK samples](https://github.com/modelcontextprotocol/csharp-sdk). You can go a step further and look at remote MCP servers with the new [Azure Functions support](https://github.com/Azure-Samples/remote-mcp-functions-dotnet/).

## Go even further with MCP

From here you can continue to build new functionality into your MCP server for your company, community, and your services today that can be used in GitHub Copilot or other clients. The [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) has great samples of creating MCP servers, clients, and advanced tutorials showing the power of MCP and how easily you can build them with C#.