```yaml
---
title: Easy API Testing in VS Code, Visual Studio, and Rider
source: https://okyrylchuk.dev/blog/easy-api-testing-in-vs-code-visual-studio-and-rider/
date_published: 2025-01-31T19:27:43.000Z
date_captured: 2025-08-11T16:15:59.953Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: testing
technologies: [Postman, Rest Client, VS Code, Visual Studio, Rider, ASP.NET Core Web API, Git, .NET]
programming_languages: [C#, HTTP, JSON]
tags: [api-testing, ide, developer-tools, web-api, http-client, version-control, dotnet, productivity, rest]
key_concepts: [API testing, HTTP request syntax, environment variables, IDE integration, version control for API requests, developer productivity]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores an efficient and lightweight approach to API testing directly within popular IDEs such as VS Code, Visual Studio, and Rider. It introduces the use of `.http` or `.rest` files as a simpler alternative to comprehensive tools like Postman. The guide demonstrates how to construct various HTTP requests (GET, DELETE, POST), include headers, and manage environment-specific variables. The author emphasizes the benefits of this integrated method, highlighting its lightweight nature, ease of version control, and human-readable syntax for streamlined API development workflows.]
---
```

# Easy API Testing in VS Code, Visual Studio, and Rider

# Easy API Testing in VS Code, Visual Studio, and Rider

I have used Postman for API tests for a long time. It’s a great tool. However, now it’s an entire API platform with too many features. 

I don’t need all of its features, so I was looking for something light and fast. I found the [Rest Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client#overview) extension for VS Code, which amazed me with its simplicity.

As a fan of the Rest Client extension, I was happy to see that **Visual Studio** and **Rider** started supporting the **.http** or **.rest** files. 

## **Getting Started**

ASP.NET Core Web API project template now includes the **.http** file for your requests. 

![Screenshot of a Visual Studio Solution Explorer showing an HttpFileExample project with an HttpFileExample.http file.](https://okyrylchuk.dev/wp-content/uploads/2025/01/solution-png.avif "solution")

**Note:** All examples above will be in Visual Studio. However, you write requests the same way in VS Code or Rider.

If you don’t have the **.http** file or want to add more files, add the file with **.http** or **.rest** extension. 

## **Writing Requests**

To create a request, start with the HTTP verb and put the URL afterward.  After adding the request, the link **“Send Request”** will appear above each request. That’s it.

```
@host = http://localhost:5150

GET {{host}}/users/
Accept: application/json
```

As you can see above, it’s a GET request to fetch Users. It contains one HTTP header _Accept: application/json_.

Add each header on its own line immediately after the request line to add one or more headers. Don’t include blank lines between the request line and the first header or subsequent header lines.

Don’t worry that you must remember all headers, IntelliSence will help you.

You can also create variables and use them in the URL, headers, or parameters. The **@host** variable is visible in the example above. You can reference the variable value in the entire file using double curly braces.

A file can contain multiple requests by using lines with **###** as delimiters. 

```
@host = http://localhost:5150

GET {{host}}/users/
Accept: application/json

###
DELETE {{host}}/users/1
```

The request body must be after a blank line.

```
###
POST {{host}}/users/
Content-type: application/json

{
  "id": 1,
  "name": "John Doe",
  "email": "jdoe@example.com"
}
```

## Sending Requests

Click the **“Send Request”** link above each request to send it. You’ll see the window with the response. 

![Screenshot showing a .http file in Visual Studio with multiple requests (GET, DELETE, POST) and the "Send Request" link. On the right, the response window displays the formatted JSON body of a successful GET request.](https://okyrylchuk.dev/wp-content/uploads/2025/01/send-request-1024x444.avif "send request")

## **Environment Files**

You can assign different values for your variables depending on the environment. To do that, create a **http-client.env.json** file in the same folder as the request **.http** file. 

Then, you can set values for different environments: 

```json
{
  "dev": {
    "host": "http://localhost:5150"
  },
  "remote": {
    "host": "https://contoso.com"
  }
}
```

Then, you’ll see the environments in the top right corner:

![Screenshot showing a dropdown menu in Visual Studio with environment options: <none>, dev, and remote. The 'dev' environment is currently selected.](https://okyrylchuk.dev/wp-content/uploads/2025/01/evns-png.avif "evns")

## **Key Advantages**

**Lightweight & Integrated**

*   No need for external tools like Postman or cURL.
*   Works directly inside your IDE (VS Code, Visual Studio, Rider).

**Version Control Friendly**

*   **.http** files can be committed to **Git**, making API requests shareable across teams.
*   Easier to track changes and maintain API request history.

**Quick & Readable Syntax**

*   Simple and human-readable format compared to JSON or XML-based configurations.
*   No need to learn a new scripting language or DSL.