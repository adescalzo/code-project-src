```yaml
---
title: "Building Your First MCP Server with .NET: A Developer's Guide to Model Context Protocol"
source: https://engincanveske.substack.com/p/building-your-first-mcp-server-with?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2040
date_published: 2025-04-09T10:01:55.000Z
date_captured: 2025-08-12T12:19:23.875Z
domain: engincanveske.substack.com
author: Engincan Veske
category: frontend
technologies: [.NET, Model Context Protocol, MCP C# SDK, Microsoft.Extensions.Hosting, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.AI, Cursor Code Editor, Large Language Models, Standard I/O]
programming_languages: [C#]
tags: [ai, llm, model-context-protocol, dotnet, csharp, server, integration, developer-tools, extensibility, api]
key_concepts: [Model Context Protocol, Large Language Models, MCP Server, MCP Client, Dependency Injection, Tooling for AI, Standard I/O communication, AI integration]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to the Model Context Protocol (MCP), an open standard designed to enable Large Language Models (LLMs) to interact with external tools and data sources. It details the MCP architecture, which includes Hosts, Servers, and Clients, and explains how these components facilitate structured AI integrations. The tutorial offers a practical, step-by-step walkthrough on building a basic MCP server using .NET and the official MCP C# SDK. It further demonstrates how to integrate this custom server with the Cursor Code Editor, allowing the LLM within the IDE to query and receive real-time data from the server.
---
```

# Building Your First MCP Server with .NET: A Developer's Guide to Model Context Protocol

# Building Your First MCP Server with .NET: A Developer's Guide to Model Context Protocol

### What MCP (Model Context Protocol) is and how it works? How to build your first MCP Server using .NET? How to integrate it with Cursor Code Editor?

Connecting **Large Language Models (LLMs)** to external services is becoming a key part of modern application development. **Model Context Protocol (MCP)** offers a standard way to let LLMs interact with tools and APIs, making these integrations more structured and predictable.

The video version of this article is also available! [You can watch it on YouTube Now!](https://www.youtube.com/watch?v=ccAVySdFq58)

In this practical guide, I'll explain:

*   What MCP (Model Context Protocol) is and how it works
*   How to build your first MCP Server using .NET
*   How to integrate it with the Cursor code editor (- MCP Client -)

## **What is Model Context Protocol (MCP)?**

[Anthropic](https://www.anthropic.com/) (who coined the term and introduced the protocol), [describes](https://www.anthropic.com/news/model-context-protocol) it as:

> The **Model Context Protocol** is an open standard that enables developers to build secure, two-way connections between their data sources and AI-powered tools. The architecture is straightforward: developers can either expose their data through MCP servers or build AI applications (MCP clients) that connect to these servers.

**TL;DR:** MCP is a protocol that enables AI models to interact with internal/external tools, data, and APIs through standardized interfaces. The protocol's architecture consists of **MCP Servers** and **MCP Clients**, which we'll explore in the next sections.

## **MCP Architecture**

The following diagram illustrates the general architecture of MCP:

As shown in the diagram, MCP extends LLMs' capabilities by connecting them to external tools and data sources, providing specialized and up-to-date data for specific use cases.

MCP architecture consists of three main components:

1.  **MCP Hosts**: Applications or tools that consume MCP services (e.g., Claude Desktop, IDEs, AI tools)
2.  **MCP Servers**: Specialized programs that:
    *   Expose specific features or data
    *   Follow MCP specifications
    *   Provide standardized interfaces for tools and services

    Common examples include:
    *   GitHub
    *   Google Drive, Google Maps
    *   PostgreSQL and other data sources
3.  **MCP Clients**: Intermediaries that facilitate communication between _Hosts_ and _Servers_ by:
    *   Connecting to one specific MCP Server
    *   Handling message routing
    *   Managing communication protocols

## **Example: Getting Current Time from MCP Server**

Let's say we want to get the current time from a specific city or timezone as an example (you can use a different example for your use case, but for the sake of simplicity, we will use this example).

If you open the _Chat_ in Cursor IDE and ask about the current time in a specific city or timezone, you will see the following:

As you can see, since the LLMs don't have a connection to the internet by default, they are unable to provide the current time for a specific city or timezone. So, we need to extend the capabilities of LLMs by adding an MCP Server that provides the current time in a specific city or timezone. Since we are using Cursor IDE, we will build an MCP Server that provides the current time in a specific city or timezone and integrate it with Cursor IDE, which is the MCP Client in our case.

### **Building Your First MCP Server with .NET**

Let's create a simple MCP server that provides time-related functionality. We'll use the official [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) to build the MCP Server. Here are the steps to build the MCP Server:

#### **Step 1: Create a New Project**

First, let's create a new console application:

```bash
dotnet new console -n McpTimeServer
cd McpTimeServer
```

#### **Step 2: Install Required Packages**

Add the necessary NuGet packages:

```bash
dotnet add package ModelContextProtocol --prerelease
dotnet add package Microsoft.Extensions.Hosting
```

> The `ModelContextProtocol` package gives access to new APIs to create clients that connect to MCP servers, creation of MCP servers, and AI helper libraries to integrate with LLMs through `Microsoft.Extensions.AI`.

#### **Step 3: Implement the MCP Server**

Open your `Program.cs` and update it with the following code:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateEmptyApplicationBuilder(null);

// Add MCP server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

await app.RunAsync();
```

Let's break down the server configuration of our MCP server:

*   `AddMcpServer()`: Registers MCP server services to DI container
*   `WithStdioServerTransport()`: Uses standard I/O for communication
*   `WithToolsFromAssembly()`: Automatically discovers and registers MCP tools from the related attributes. (We will create the tools in the next step, and we will use the `McpServerToolType` attribute to mark the class as containing MCP tools.)

Now, let's create the tools that will be used by the MCP server. Create a class named `TimeTools` and add the following methods:

```csharp
[McpServerToolType]
public static class TimeTools
{
    [McpServerTool, Description("Gets the current time")]
    public static string GetCurrentTime()
    {
        return DateTimeOffset.Now.ToString();
    }

    [McpServerTool, Description("Gets time in specific timezone")]
    public static string GetTimeInTimezone(string timezone)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTime(DateTimeOffset.Now, tz).ToString();
        }
        catch
        {
            return "Invalid timezone specified";
        }
    }
}
```

Here;

*   The `McpServerToolType` attribute marks this class as containing MCP tools (`[McpServerToolType]`)
*   Each method marked with `McpServerTool` becomes available to MCP clients (`[McpServerTool]`)

After creating the tools, we are done developing the MCP server and we can directly use it in our MCP Client (Cursor IDE, in our example).

### **Integrating with a MCP Client (Cursor IDE)**

Now that we have our MCP server, let's configure it in Cursor IDE. Here are the steps to configure the MCP Server in Cursor IDE:

#### **Step 1: Configure MCP Server in Cursor**

To configure the MCP Server in Cursor IDE, we should go to _Cursor Settings_. You can do this from the top menu (_File -> Preferences -> Cursor Settings_) (or alternatively you can directly click the _Gear_ icon in the top right corner of the Cursor IDE):

After opening the _Cursor Settings_ window, you should see the _MCP_ section. Then you can directly click the _Add new global MCP server_ button and add the following MCP server configuration in the opening `mcp.json` file:

```json
{
  "mcpServers": {
    "timeServer": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "path/to/your/McpTimeServer", //absolute path to the MCP server project
        "--no-build"
      ]
    }
  }
}
```

#### **Step 2: Test Your MCP Server**

After adding the MCP server configuration, you can save the file and go back to the _MCP_ section in _Cursor Settings,_ and enable the MCP Server (click to the _Refresh_ icon, to run the MCP server):

Now, you can test your MCP server by asking about the time in a specific city or timezone:

You can clearly see that, now the LLMs can provide the current time in a specific city or timezone from our MCP Server. It even shows which tool is used to provide the current time and provided value while calling the related tool.

## **Conclusion**

The Model Context Protocol (MCP) represents a significant advancement in standardizing AI-tool interactions. Through this tutorial, you've learned how to create a basic MCP server and integrate it with Cursor IDE. This foundation can be extended to create more complex tools and services that interact seamlessly with AI models.

## **References**

*   [Model Context Protocol](https://www.anthropic.com/news/model-context-protocol)
*   [Official MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
*   [MCP Specification](https://github.com/modelcontextprotocol/specification)
*   [Available MCP Servers](https://github.com/modelcontextprotocol/servers)