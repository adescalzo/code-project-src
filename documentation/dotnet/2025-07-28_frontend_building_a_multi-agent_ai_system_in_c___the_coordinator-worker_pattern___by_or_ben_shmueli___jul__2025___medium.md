# Building a Multi-Agent AI System in C#: The Coordinator-Worker Pattern | by Or Ben Shmueli | Jul, 2025 | Medium

**Source:** https://medium.com/@orbens/building-a-multi-agent-ai-system-in-c-the-coordinator-worker-pattern-a2577a67fa2c
**Date Captured:** 2025-07-28T16:14:44.175Z
**Domain:** medium.com
**Author:** Or Ben Shmueli
**Category:** frontend

---

# **Building a Multi-Agent AI System in C#: The Coordinator-Worker Pattern**

[

![Or Ben Shmueli](https://miro.medium.com/v2/resize:fill:64:64/0*m8RgOBLHT5k3_hkb.jpg)





](/@orbens?source=post_page---byline--a2577a67fa2c---------------------------------------)

[Or Ben Shmueli](/@orbens?source=post_page---byline--a2577a67fa2c---------------------------------------)

Follow

4 min read

·

Jul 13, 2025

10

2

Listen

Share

More

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*XLzxSi-J_eL744XscX3cWQ.png)

In the modern AI ecosystem, multi-agent collaboration is no longer a novelty — it’s a necessity. When one agent isn’t enough to solve a problem, why not let two (or more) collaborate? Inspired by distributed systems, microservices, and real-world teams, we explore a powerful architecture: the **Coordinator–Worker AI Pattern**.

In this post, you’ll learn how to implement a system in C# where:

*   The **Coordinator AI Agent** receives input, analyzes the goal, and breaks it into actionable tasks.
*   The **Worker AI Agent** executes these tasks one by one and returns the results.

With this approach, you enable structured, modular reasoning and execution across complex workflows — whether you’re automating operations, managing DevOps incidents, or building decision-support systems.

This design pattern is especially valuable in high-pressure, time-sensitive environments like production incident management. It not only demonstrates how to orchestrate multiple AI services in C#, but also how to combine architectural discipline with intelligent behavior for real-world impact.

Let’s dive in.

# 🚨 Use Case: Automated Incident Response

Imagine you’re operating a production system and a service alert pops up:

**“High error rate on /api/auth endpoint”**

Instead of waking up a human at 2 AM, we let a two-agent system handle the triage:

*   The **Coordinator** receives the incident description, breaks it into investigative steps:
*   Check recent deployment history
*   Analyze logs from the affected service
*   Validate upstream/downstream dependencies
*   Propose a possible root cause
*   The **Worker** executes each step by prompting an AI model to simulate investigation, using known DevOps practices, even querying telemetry APIs or logs.

This mirrors how SRE/DevOps engineers approach an incident — and it’s the perfect candidate for agent orchestration.

By simulating reasoning and logic used in production scenarios, this system not only enhances productivity, but also ensures consistent incident investigation methodology across teams and time zones.

# 🧠 Step 1: Base AI Client

public class OpenAiClient  
{  
    private readonly HttpClient \_httpClient;  
    private readonly string \_apiKey;  
  
    public OpenAiClient(string apiKey)  
    {  
        \_apiKey = apiKey;  
        \_httpClient = new HttpClient();  
        \_httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {\_apiKey}");  
    }  
  
    public async Task<string\> SendMessageAsync(string prompt)  
    {  
        var body = new  
        {  
            model = "gpt-4",  
            messages = new\[\] {  
                new { role = "user", content = prompt }  
            },  
            temperature = 0.3  
        };  
        var json = JsonSerializer.Serialize(body);  
        var content = new StringContent(json, Encoding.UTF8, "application/json");  
        var response = await \_httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);  
        var result = await response.Content.ReadAsStringAsync();  
        using var doc = JsonDocument.Parse(result);  
        return doc.RootElement.GetProperty("choices")\[0\].GetProperty("message").GetProperty("content").GetString();  
    }  
}

# 🤖 Step 2: Coordinator Agent

public class CoordinatorAgent  
{  
    private readonly OpenAiClient \_client;  
   
    public CoordinatorAgent(OpenAiClient client)  
    {  
        \_client = client;  
    }  
    public async Task<List<string\>> GetTasksFromIncident(string incidentDescription)  
    {  
        var prompt = $"""  
Given the following incident:  
"{incidentDescription}"  
Break it down into 3–5 investigation steps a DevOps engineer would take. Return as a bullet list.  
""";  
        var response = await \_client.SendMessageAsync(prompt);  
        return response.Split('  
').Where(line => !string.IsNullOrWhiteSpace(line)).ToList();  
    }  
}

Beyond simple task decomposition, the Coordinator’s role can be extended with:

*   Dynamic task prioritization based on severity
*   Conditional branching (e.g., if task 2 fails, run fallback task 2b)
*   History or memory of previous incidents for smarter decomposition

# ⚙️ Step 3: Worker Agent

public class WorkerAgent  
{  
    private readonly OpenAiClient \_client;  
      
    public WorkerAgent(OpenAiClient client)  
    {  
        \_client = client;  
    }  
    public async Task<string\> ExecuteInvestigationStep(string step)  
    {  
        var prompt = $"You are a DevOps assistant. Perform the following investigation step logically:  
{step}  
Return the reasoning and findings.";  
        return await \_client.SendMessageAsync(prompt);  
    }  
}

The Worker Agent can also be extended to:

*   Execute external HTTP queries to observability platforms (e.g., Grafana, Prometheus, Datadog)
*   Interface with internal tools like Jenkins, Jira, or custom dashboards
*   Perform real-time alert enrichment before returning results

# 🧪 Step 4: Orchestration

public class IncidentResponseOrchestrator  
{  
    private readonly CoordinatorAgent \_coordinator;  
    private readonly WorkerAgent \_worker;  
  
    public IncidentResponseOrchestrator(CoordinatorAgent coordinator, WorkerAgent worker)  
    {  
        \_coordinator = coordinator;  
        \_worker = worker;  
    }  
    public async Task<string\> HandleIncidentAsync(string incident)  
    {  
        var steps = await \_coordinator.GetTasksFromIncident(incident);  
        var output = new List<string\>();  
        foreach (var step in steps)  
        {  
            var result = await \_worker.ExecuteInvestigationStep(step);  
            output.Add($"### {step}  
{result}  
");  
        }  
        return string.Join("  
", output);  
    }  
}

This orchestrator pattern is highly extensible:

*   You can easily introduce retry logic with exponential backoff
*   You can make Worker steps parallel with `Task.WhenAll()` for faster MTTR
*   You can even integrate logging for each step into a centralized audit system

# ✅ Final Execution

class Program  
{  
    static async Task Main()  
    {  
        var client = new OpenAiClient("your-api-key");  
        var coordinator = new CoordinatorAgent(client);  
        var worker = new WorkerAgent(client);  
        var orchestrator = new IncidentResponseOrchestrator(coordinator, worker);  
          
        Console.WriteLine("Describe the incident:");  
        var input = Console.ReadLine();  
        var report = await orchestrator.HandleIncidentAsync(input);  
        Console.WriteLine("  
\--- Incident Investigation Report ---  
");  
        Console.WriteLine(report);  
    }  
}

# 🔍 Final Thoughts

Multi-agent orchestration opens the door to intelligent, semi-autonomous tools that mimic human workflows. In a DevOps context, this means:

*   Faster MTTR (mean time to resolution)
*   Consistent incident handling
*   Round-the-clock triage without burnout
*   Reduced cognitive load on on-call engineers

By bringing AI into the DevOps lifecycle — not just as a chatbot or copilot, but as a reliable process partner — we get systems that are more resilient, scalable, and aligned with how human teams actually operate.

# Where to Go From Here

*   Add memory/context to your agents (Redis, vector DBs)
*   Build a front-end (chat, CLI, dashboard)
*   Introduce agent feedback and scoring
*   Use different models per agent (e.g., GPT-4 for Coordinator, GPT-3.5 for Workers)
*   Integrate your incident manager with Slack, PagerDuty, or ServiceNow

It’s not science fiction — it’s how AI-native systems are being built today.