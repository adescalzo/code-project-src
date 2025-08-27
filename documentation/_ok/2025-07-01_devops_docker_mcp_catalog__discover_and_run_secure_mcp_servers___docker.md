```yaml
---
title: "Docker MCP Catalog: Discover and Run Secure MCP Servers | Docker"
source: https://www.docker.com/blog/docker-mcp-catalog-secure-way-to-discover-and-run-mcp-servers/?utm_campaign=2025-07-14-nl-july&utm_medium=email&utm_source=marketo&mkt_tok=NzkwLVNTQi0zNzUAAAGbqJa0uYu7HMBcoLI6yL3EavDyvonEOSCqn-u0nuB-S9vWWziHVeoNFf7GcaDvjwf4fupXPWRcllVEQMDZmsTGW4kVjuJhwYzsXKbY6bXuZ244
date_published: 2025-07-01T13:04:20.000Z
date_captured: 2025-08-06T17:53:09.186Z
domain: www.docker.com
author: Unknown
category: devops
technologies: [Docker, Docker Hub, Docker Desktop, Model Context Protocol (MCP), GitHub, ClickHouse, Gordon, Claude, Cursor, VSCode, Jira, Stripe, ElasticSearch, Loki, Grafana, Astra DB, Atlassian, DuckDuckGo]
programming_languages: [SQL, JavaScript, Python]
tags: [docker, mcp, security, ai, containers, marketplace, distribution, catalog, artificial-intelligence, web-services]
key_concepts: [containerization, security-by-design, software-bill-of-materials, cryptographic-signatures, host-isolation, dependency-management, AI-tool-distribution, open-submission-process]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Docker MCP Catalog has rapidly gained traction, addressing the critical security risks associated with running Model Context Protocol (MCP) servers directly on host systems via commands like npx or uvx. Docker leverages its decade-long expertise in containerization to provide a secure environment for MCP servers, offering cryptographic signatures, SBOMs, and complete host isolation. The enhanced catalog features improved discovery by use case and transparent security classifications (Docker-built vs. community-built). Docker has also opened its submission process, inviting developers to containerize and publish their MCP servers, establishing a new standard for secure AI tool distribution. This initiative aims to make security the default path for AI applications, with future plans for remote MCP servers and integration with the official MCP registry.]
---
```

# Docker MCP Catalog: Discover and Run Secure MCP Servers | Docker

# The Docker MCP Catalog: the Secure Way to Discover and Run MCP Servers

![](https://www.docker.com/app/uploads/2022/07/Nuno_Coracao_profile.jpg)
[Nuno Coracao](https://www.docker.com/author/nuno-coracao/ "Posts by Nuno Coracao")

![](https://www.docker.com/app/uploads/2025/06/headshot-Cody-Rigney.jpg)
[Cody Rigney](https://www.docker.com/author/cody-rigney/ "Posts by Cody Rigney")

The Model Context Protocol (MCP) ecosystem is exploding. In just weeks, our Docker MCP Catalog has surpassed **1 million pulls**, validating that developers are hungry for a [secure way to run MCP servers](https://www.docker.com/products/mcp-catalog-and-toolkit/). Today, we’re excited to share major updates to the Docker MCP Catalog, including enhanced discovery features and our new open submission process. With hundreds of developers already requesting to publish their MCP servers through Docker, we’re accelerating our mission to make [containerized MCP servers](https://hub.docker.com/mcp) the standard for secure AI tool distribution.

The rapid adoption of MCP servers also highlights a critical problem — the current practice of running them via npx or uvx commands exposes systems to unverified code with full host access, not to mention dependency management friction. In this post, we’ll explain why Docker is investing in the MCP ecosystem, showcase the new catalog capabilities, and share how you can contribute to building a more secure foundation for AI applications.

![Screenshot 2025 06 26 at 16 56 08 Docker MCP Marketplace](https://www.docker.com/app/uploads/2025/06/Screenshot-2025-06-26-at-16-56-08-Docker-MCP-Marketplace.png "- Screenshot 2025 06 26 at 16 56 08 Docker MCP Marketplace")
*Figure 1: The new Docker MCP Catalog, built for easier discovery. This image displays the main interface of the Docker MCP Catalog, featuring a prominent search bar and various categories for discovering MCP servers.*

**Figure 1: The new Docker MCP Catalog, built for easier discovery.**

## Why Docker is building the MCP Catalog

### The security issues in MCP distribution

Every time a developer runs npx -y @untrusted/mcp-server or uvx some-mcp-tool, they’re making a dangerous trade-off: convenience over security. These commands execute arbitrary code directly on the host system with full access to:

*   The entire file system
*   Network connections
*   Environment variables and secrets
*   System resources

Some MCP clients limit environment variable access, but even that is not a universal practice. This isn’t sustainable. As MCP moves from experimentation to production, we need a fundamentally different approach.

### Docker’s unique position

Docker has spent over a decade solving exactly these problems for cloud-native applications. We’ve built the infrastructure, tools, and trust that developers rely on to run billions of containers in production. Now, we’re applying these same principles to the MCP ecosystem.

When you run an MCP server from our Catalog, you get:

*   **Cryptographic signatures** verifying the image hasn’t been tampered with
*   **Software Bill of Materials (SBOMs)** documenting every component
*   **Complete isolation** from your host system
*   **Controlled access** to only what the server actually needs

This isn’t about making life harder for developers—it’s about making security the path of least resistance.

## Introducing the enhanced MCP Catalog

### Built for MCP discovery

We’ve reimagined the MCP Catalog to make it more accessible and easier to navigate. You can still access the MCP Catalog from Docker Hub and the MCP Toolkit in Docker Desktop just like before, or [go straight to the MCP catalog](https://hub.docker.com/mcp). We’ve gone beyond generic container image listings by building features that help you quickly find the right MCP servers for your AI applications.

**Browse by Use Case**: MCP servers are organized by what they actually do:

*   Data Integration (databases, APIs, file systems)
*   Development Tools (IDEs, code analysis, testing)
*   Communication (email, Slack, messaging platforms)
*   Productivity (task management, calendars, note-taking)
*   Analytics (data processing, visualization, reporting)

**Enhanced Search**: Find servers by capability, tools, GitHub tags, and categories — not just by name.

**Security Transparency**: Every catalog entry clearly shows whether it’s Docker-built (with transparent build signing and verification) or community-built (containerized and maintained by the publisher).

![Screenshot 2025 06 27 205820](https://www.docker.com/app/uploads/2025/06/Screenshot-2025-06-27-205820.png "- Screenshot 2025 06 27 205820")
*Figure 2: Discover MCP servers by use cases. This image shows the "Browse by Use Case" section of the Docker MCP Catalog, displaying various categories like Data Integration, Development Tools, and Communication.*

**Figure 2: Discover MCP servers by use cases.**

### How we classify MCP Servers: Built by Docker vs. community-built

**Docker-Built Servers**: When you see “Built by Docker,” you’re getting our complete security treatment. We control the entire build pipeline, providing cryptographic signatures, SBOMs, provenance attestations, and continuous vulnerability scanning.

**Community-Built Servers**: These servers are packaged as Docker images by their developers. While we don’t control their build process, they still benefit from container isolation, which is a massive security improvement over direct execution.

**Tiers serve important roles**: Docker-built servers demonstrate the gold standard for security, while community-built servers ensure we can scale rapidly to meet developer demand. Developers can change their mind after submitting a community-built server and opt to resubmit it as a Docker-built server.

![Screenshot 2025 06 26 105434](https://www.docker.com/app/uploads/2025/06/Screenshot-2025-06-26-105434.png "- Screenshot 2025 06 26 105434")
*Figure 3: An example of Built by Docker MCP Server. This image displays a detailed view of an MCP server entry (GitHub Official) in the Docker Catalog, highlighting its "Built by Docker" status and other security features.*

**Figure 3: An example of Built by Docker MCP Server.**

## Open for MCP server submission: Join the secure MCP movement

Starting today, we’re opening our submission process to the community. Whether you’re an individual developer or an enterprise team, you can feature your MCP servers on the Docker MCP Catalog. By publishing through our catalog, you’re not just distributing your MCP server — you’re helping establish a new security standard for the entire ecosystem while getting your MCP tools available to millions of developers already using Docker via Docker Hub and Docker Desktop. Your containerized server becomes part of the solution, demonstrating that production-ready AI tools don’t require compromising on security.

[![Github MCP Registry](https://opengraph.githubassets.com/0/docker/mcp-registry "- mcp registry")](https://github.com/docker/mcp-registry)
*This image shows the GitHub repository logo for `docker/mcp-registry`, indicating the official Docker MCP registry.*

### How to submit your MCP server

1.  **Containerize your server** – Package your MCP server as a Docker image
2.  **Submit via GitHub** – Create a pull request at [github.com/docker/mcp-registry](https://github.com/docker/mcp-registry)
3.  **Choose your tier** – Opt for Docker-built (we handle the build) or community-built (you build and maintain it)

We’re committed to a fast, transparent review process. Quality MCP servers that follow our security guidelines will be published quickly, helping you reach Docker’s 20+ million developer community.

ClickHouse is one of the first companies to take advantage of Docker’s MCP Catalog, and they opted for the Docker-built tier to ensure maximum security. Here’s why they chose to partner with Docker:

_“At_ [_ClickHouse_](https://clickhouse.com/)_, we deliver the fastest analytics database – open-source, and designed for real-time data processing and analytics at scale. As agentic AI becomes more embedded in modern applications, developers are using the ClickHouse MCP server to support intelligent, data-driven workflows that demand low latency, high concurrency, and cost efficiency._
_To make it easier for developers to deploy these workloads, we’re featuring_ [_ClickHouse MCP Server_](https://hub.docker.com/mcp/server/clickhouse/overview) _on Docker’s MCP Catalog, which provides_ **_a powerful way to reach 20M+ developers_** _and makes it easier for Docker users to discover and use our solution._ **_We opted for “Built by Docker” with the highest security standard_**_, including cryptographic signatures, SBOMs, provenance attestations, and continuous vulnerability scanning. Together with Docker, developers can run ClickHouse MCP Server with confidence, knowing it’s secured, verified, and ready for their agentic applications.” –_ Tanya Bragin, VP of Product and Marketing Clickhouse_._

## What’s coming next

### Remote MCP servers

We’re preparing for the future of cloud-native AI applications. Remote MCP servers will enable:

*   Managed MCP services that scale automatically
*   Shared capabilities across teams without distributing code
*   Stricter security boundaries for sensitive operations

### Integration with the official MCP registry

We’re actively collaborating with the MCP community on the upcoming official registry. Our vision is complementary:

*   The official registry provides centralized discovery – the “yellow pages” of available MCP servers
*   Docker provides the secure runtime and distribution for those listings
*   Together, we create a complete ecosystem where discovery and security work hand-in-hand

## The path forward

The explosive growth of our MCP Catalog, 1 million pulls and hundreds of publisher requests, tells us developers are ready for change. They want the power of MCP, but they need it delivered securely.

By establishing containers as the standard for MCP server distribution, we’re not trying to own the ecosystem — we’re trying to secure it. Every MCP server that moves from npx execution to containerized deployment is a win for the entire community.

## Start today

*   **Explore the enhanced MCP Catalog**: [Visit the MCP Catalog](https://hub.docker.com/mcp) [](http://hub.docker.com/mcp)to discover MCP servers that solve your specific needs securely.
*   **Use and test hundreds of MCP Servers**: [Download Docker Desktop](https://www.docker.com/products/docker-desktop/) [](http://hub.docker.com/mcp)to download and use any MCP server in our catalog with your favorite clients: Gordon, Claude, Cursor, VSCode, etc
*   **Submit your server**: Join the movement toward secure AI tool distribution. [Check our submission guidelines](https://github.com/docker/mcp-registry) for more.
*   **Follow our progress**: Star our repository and watch for updates on the MCP Gateway release and remote server capabilities.

Together, we’re building more than a catalog — we’re establishing the secure foundation that the MCP ecosystem needs to grow from experimental tool to production-ready platform. Because when it comes to AI applications, security isn’t optional. It’s fundamental.

### **Learn more**

*   Check out our [announcement blog](https://www.docker.com/blog/announcing-docker-mcp-catalog-and-toolkit-beta/)
*   Find documentation for[Docker MCP Catalog and Toolkit](https://docs.docker.com/ai/mcp-catalog-and-toolkit/).
*   Subscribe to the [Docker Navigator Newsletter](https://www.docker.com/newsletter-subscription/).
*   New to Docker? [Create an account](https://hub.docker.com/signup?_gl=1*1v81gq1*_gcl_au*MTQxNjU3MjYxNS4xNzQyMjI1MTk2*_ga*MTMxODI0ODQ4LjE3NDE4MTI3NTA.*_ga_XJWPQMJYHQ*czE3NDg0NTYyNzIkbzI2JGcxJHQxNzQ4NDU2MzI2JGo2JGwwJGgw).
*   Have questions? The [Docker community is here to help](https://www.docker.com/community/).

#### Posted

Jul 1, 2025

[](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fwww.docker.com%2Fblog%2Fdocker-mcp-catalog-secure-way-to-discover-and-run-mcp-servers%2F "Visit this Linkedin profile")[](https://twitter.com/intent/tweet?url=https%3A%2F%2Fwww.docker.com%2Fblog%2Fdocker-mcp-catalog-secure-way-to-discover-and-run-mcp-servers%2F "Visit this X profile")[](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fwww.docker.com%2Fblog%2Fdocker-mcp-catalog-secure-way-to-discover-and-run-mcp-servers%2F "Visit this Facebook profile")

#### Post Tags

*   [AI/ML](https://www.docker.com/blog/tag/artificial-intelligence-machine-learning/)
*   [Docker](https://www.docker.com/blog/tag/docker/)
*   [Docker Hub](https://www.docker.com/blog/tag/docker-hub/)
*   [MCP](https://www.docker.com/blog/tag/mcp/)

#### Post Categories

*   [Products](https://www.docker.com/blog/category/products/)

## Related Posts

*   [

    #### Docker Brings Compose to the Agent Era: Building AI Agents is Now Easy

    Define, run, and scale AI agents using Docker Compose and Docker Offload. Streamline agentic development across your stack.

    Mark Cavage & Tushar Jain

    Jul 10, 2025

    Read now

    ](https://www.docker.com/blog/build-ai-agents-with-docker-compose/)
*   [

    #### Everyone’s a Snowflake: Designing Hardened Image Processes for the Real World

    Why flexible hardened images drive real security. Learn how platform teams can balance security, usability, and developer happiness at scale.

    Christian Dupuis & Michael Donovan

    Aug 5, 2025

    Read now

    ](https://www.docker.com/blog/hardened-image-best-practices/)
*   [

    #### How Docker MCP Toolkit Works with VS Code Copilot Agent Mode

    Learn how to set up and use the Docker MCP Toolkit and Copilot Agent Mode in VS Code together with practical steps and examples.

    Hamida Rebaï

    Aug 4, 2025

    Read now

    ](https://www.docker.com/blog/mcp-toolkit-and-vs-code-copilot-agent/)
*   [

    #### Hard Questions: What You Should Really Be Asking Your Hardened Image Provider Before You Press the Buy Button

    Not all hardened images are secure. Ask these 15+ critical questions to evaluate providers’ patching, flexibility, transparency, and CI/CD compatibility

    Christian Dupuis & Michael Donovan

    Aug 4, 2025

    Read now

    ](https://www.docker.com/blog/container-security-hardened-images-questions/)

![mcp.hub](https://www.docker.com/app/uploads/2025/06/Screenshot-2025-06-26-at-16-56-08-Docker-MCP-Marketplace.png)
*This image shows the main page of the mcp.hub website, featuring a large banner for "Access the largest library of secure, containerized MCP servers," a search bar, and sections for "Use cases" and "Featured MCPs."*

![Use cases](https://www.docker.com/app/uploads/2025/06/Screenshot-2025-06-27-205820.png)
*This image is a close-up of the "Use cases" section from the Docker MCP Catalog, displaying various interactive buttons representing common tasks like "Create a new Jira issue," "Run a SQL query," and "Search Loki logs for elevated error patterns."*

![GitHub Official](https://www.docker.com/app/uploads/2025/06/Screenshot-2025-06-26-105434.png)
*This image displays a detailed view of the "GitHub Official" MCP server entry within the Docker Catalog, showing its logo, description, statistics (10k+ pulls, 39 tools), and badges indicating "Signed," "Built by Docker," and "Requires Secrets."*

![docker/mcp-registry](https://opengraph.githubassets.com/0/docker/mcp-registry)
*This image shows the GitHub repository page for `docker/mcp-registry`, displaying the repository name, description ("Official Docker MCP registry"), and key metrics such as contributors (52), issues (9), stars (128), and forks (189).*