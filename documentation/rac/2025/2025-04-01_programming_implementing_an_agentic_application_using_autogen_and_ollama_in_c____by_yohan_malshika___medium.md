```yaml
---
title: "Implementing an Agentic Application Using AutoGen and Ollama in C# | by Yohan Malshika | Medium"
source: https://malshikay.medium.com/implementing-an-agentic-application-using-autogen-and-ollama-in-c-54367a3ac9d8
date_published: 2025-04-01T11:41:51.609Z
date_captured: 2025-08-27T11:17:42.313Z
domain: malshikay.medium.com
author: Yohan Malshika
category: programming
technologies: [AutoGen, Ollama, .NET, OpenAI (API concept), Qwen2.5]
programming_languages: [C#]
tags: [agentic-ai, llm, autogen, ollama, csharp, dotnet, ai-agents, local-llm, multi-agent-system, artificial-intelligence]
key_concepts: [agentic-applications, autonomous-ai-agents, local-llm-execution, multi-agent-collaboration, user-proxy-agent, chat-agent, openai-compatible-api, system-message]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a step-by-step guide on implementing agentic AI applications using Microsoft's AutoGen for .NET and Ollama for local Large Language Model (LLM) execution. It emphasizes the benefits of local execution, such as enhanced data privacy and reduced costs, compared to cloud-based AI services. The content introduces AutoGen's core concepts, including different types of AI agents like Chat Agents and User Proxy Agents, and demonstrates their practical implementation in C#. Code examples illustrate how to define an Ollama-powered agent, initiate a chat, and set up multi-agent collaboration for complex tasks. This guide offers a robust foundation for developing autonomous, privacy-focused AI solutions.]
---
```

# Implementing an Agentic Application Using AutoGen and Ollama in C# | by Yohan Malshika | Medium

# Implementing an Agentic Application Using AutoGen and Ollama in C#

![Four stylized white robots with blue glowing eyes and an "AI" logo on their chests, each sitting at a small, floating desk with a blue laptop. They are arranged in a row, suggesting collaboration or a team of AI agents working together.](https://miro.medium.com/v2/resize:fit:700/0*QP9PPhxQQLRoLE_B)

Photo by [Mohamed Nohassi](https://unsplash.com/@coopery?utm_source=medium&utm_medium=referral) on [Unsplash](https://unsplash.com/?utm_source=medium&utm_medium=referral)

Agentic applications leverage autonomous AI agents to perform complex tasks collaboratively. Microsoft’s AutoGen for .NET simplifies the creation of such applications, and when combined with Ollama, a local large language model (LLM) runtime, it provides a cost-effective and efficient AI solution without relying on cloud-based APIs.

This article provides a step-by-step guide to implementing an agentic application using AutoGen and Ollama in C#.

## Introduction to AutoGen

### What is AutoGen?

AutoGen is a framework that enables the creation of agent-based applications using AI models. Its introduced from the Microsoft. It provides tools to manage autonomous agents, allowing them to communicate, collaborate, and solve tasks efficiently.

### Why Use AutoGen with Ollama?

*   **Local Execution**: No reliance on cloud-based APIs, ensuring data privacy.
*   **Multi-Agent Collaboration**: Agents can communicate and work together on tasks.
*   **Customization**: You can fine-tune agent behaviors and interactions based on specific needs.

## Introduction to Agents in AutoGen

### What Are AI Agents?

AI agents are autonomous entities that can process inputs, make decisions, and perform tasks based on predefined logic and AI models. In AutoGen, agents interact with each other and users to solve problems dynamically.

### Types of Agents in AutoGen

1.  **Chat Agents** — Communicate with users or other agents to process queries.
2.  **User Proxy Agents** — Represent human users, allowing them to interact with AI agents.
3.  **Specialized Agents** — Designed for specific tasks, such as code review, analysis, or report generation.

By combining different agent types, you can create a powerful, interactive system tailored to your needs.

## Implementing an Ollama-Powered Agent with AutoGen

Unlike OpenAI-based agents, an Ollama agent interacts directly with a local model using an OpenAI-compatible API.

### Define the Ollama-Based Agent

Create a C# script (e.g., `Program.cs`) with the following code:

```csharp
using AutoGen;  
using AutoGen.Core;  
using AutoGen.Ollama;  
using AutoGen.Ollama.Extension;  
  
// Define Ollama endpoint and model  
var ollamaEndpoint = "http://localhost:11434/v1/chat/completions"; // Ollama API  
var model = "qwen2.5:1.5b"; // Using Qwen model for local processing  
  
var ollamaClient = new OllamaClient(ollamaEndpoint);  
  
// Create an Ollama agent  
var ollamaAgent = new OllamaChatAgent(  
    name: "assistant",  
    systemMessage: "You are an AI assistant that helps users with their tasks.",  
    chatClient: ollamaClient.GetChatClient(model))  
    .RegisterMessageConnector()  
    .RegisterPrintMessage();  
  
// Define user agent  
var userProxyAgent = new UserProxyAgent(  
    name: "user",  
    humanInputMode: HumanInputMode.ALWAYS)  
    .RegisterPrintMessage();  
  
// Start conversation  
await userProxyAgent.InitiateChatAsync(  
    receiver: ollamaAgent,  
    message: "Hello assistant, can you help me with my C# code?",  
    maxRound: 10);
```

### How This Code Works

1.  **Define Ollama Endpoint and Model**: The code specifies the Ollama API URL and uses the `qwen2.5:1.5b` model.
2.  **Create an Ollama Client**: `OllamaClient` is initialized to communicate with the Ollama server.
3.  **Create an AI Assistant Agent**: The `OllamaChatAgent` is set up to process user queries.
4.  **Define a User Agent**: The `UserProxyAgent` allows human interaction with the AI assistant.
5.  **Initiate Chat**: The `userProxyAgent` starts a conversation with the assistant.

## Running the Application

### Start Ollama

First, ensure Ollama is running before executing the application:

```bash
ollama serve
```

Then, execute your .NET application:

```bash
dotnet run
```

This will start a conversation between your user agent and the Ollama-powered assistant.

## Enhancing the Agent

### Implementing Multi-Agent Collaboration

AutoGen supports multiple agents working together. To enable collaboration, define multiple Ollama agents and orchestrate their interactions.

Example:

```csharp
var coderAgent = new OllamaChatAgent(  
    name: "coder",  
    systemMessage: "You write and review C# code.",  
    chatClient: ollamaClient.GetChatClient(model))  
    .RegisterMessageConnector()  
    .RegisterPrintMessage();  
  
var analystAgent = new OllamaChatAgent(  
    name: "analyst",  
    systemMessage: "You analyze code for best practices and efficiency.",  
    chatClient: ollamaClient.GetChatClient(model))  
    .RegisterMessageConnector()  
    .RegisterPrintMessage();  
  
// Agents communicate with each other  
await coderAgent.InitiateChatAsync(  
    receiver: analystAgent,  
    message: "Can you review this C# snippet for performance?",  
    maxRound: 5);
```

### Explanation of Multi-Agent Collaboration

*   The **Coder Agent** writes and reviews C# code.
*   The **Analyst Agent** evaluates code quality and suggests improvements.
*   The agents exchange messages, working together to analyze code snippets.

## Conclusion

By integrating `AutoGen` with `Ollama`, you can create fully autonomous AI-powered agents that operate locally. This setup ensures enhanced privacy, reduced costs, and improved performance for AI-driven applications.

## Next Steps

*   Experiment with different Ollama models.
*   Extend agents to collaborate on multi-step tasks.
*   Implement memory and context management for better interactions.
*   Deploy agents in cloud or containerized environments.

This guide provides a robust starting point, and you can further refine it based on your needs. Happy coding!