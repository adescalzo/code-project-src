```yaml
---
title: "Building a Real-Time Stock Market Bot with .NET and SignalR 📈💹 | by Justin Muench | CodeX | Medium"
source: https://medium.com/codex/building-a-real-time-stock-market-bot-with-net-and-signalr-0e67534eb74f
date_published: 2024-11-19T12:18:32.088Z
date_captured: 2025-08-08T13:50:16.871Z
domain: medium.com
author: Justin Muench
category: frontend
technologies: [.NET, SignalR, ASP.NET Core, .NET SDK, HttpClient, Finnhub, Alpha Vantage, Visual Studio Code, cdnjs]
programming_languages: [C#, HTML, JavaScript]
tags: [real-time, stock-market, signalr, dotnet, web-api, front-end, back-end, api-integration, background-service, javascript]
key_concepts: [real-time-applications, signalr-hub, background-services, dependency-injection, api-integration, client-server-communication, cross-origin-resource-sharing, web-sockets]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a step-by-step guide to building a real-time stock market bot using .NET and SignalR. It covers setting up a .NET Web API backend with a SignalR Hub to push live stock updates and a background service to fetch data from a stock API like Finnhub. A basic HTML and JavaScript frontend is demonstrated to consume and display these real-time updates. The tutorial highlights how SignalR enables seamless, dynamic user experiences without page refreshes, making it ideal for applications requiring up-to-the-second information. It also suggests potential enhancements like notifications and dynamic stock tracking.
---
```

# Building a Real-Time Stock Market Bot with .NET and SignalR 📈💹 | by Justin Muench | CodeX | Medium

# Building a Real-Time Stock Market Bot with .NET and SignalR 📈💹

![A computer monitor displaying a cryptocurrency trading interface, showing real-time price charts, order books, and various crypto pairs like BTC/USDT and ETH/USDT. The screen is dark with bright green and red numbers and graphs, indicating live market data.](https://miro.medium.com/v2/resize:fit:700/0*tuhJZqSNIQdb7OYs)

Photo by [Behnam Norouzi](https://unsplash.com/@behy_studio?utm_source=medium&utm_medium=referral) on [Unsplash](https://unsplash.com/?utm_source=medium&utm_medium=referral)

With the power of SignalR in .NET, you can build real-time applications that deliver updates as they happen. A perfect use case is a stock market bot that streams live stock prices to users in real time. This type of bot can display price changes, track specific stocks, and notify users when certain conditions are met — all without refreshing the page.

In this article, we’ll walk through how to build a basic real-time stock market bot using .NET and SignalR, with live updates that create a seamless experience for users.

# What We’ll Build 🛠️

Our bot will provide real-time updates for a selection of stocks. We’ll set it up:

1.  A **.NET back end** using SignalR to push live updates.
2.  A **front end** (we’ll use a basic HTML and JavaScript setup) to display real-time stock prices.
3.  A simple **API integration** with a stock market data provider to fetch and update stock prices periodically.

# Prerequisites

To follow along, you’ll need:

*   **.NET SDK (6.0 or newer)** installed
*   Basic understanding of C#, HTML, and JavaScript
*   An API key from a stock data provider (such as Alpha Vantage or Finnhub)

# Step 1: Setting Up the .NET Project and SignalR Hub

## 1.1 Create a New .NET Web API Project

Open your terminal and create a new Web API project for our stock market bot:

```bash
dotnet new webapi -n StockMarketBot  
cd StockMarketBot
```

## 1.2 Install SignalR Package

Install the SignalR package to add real-time functionality:

```bash
dotnet add package Microsoft.AspNetCore.SignalR
```

## 1.3 Create the StockHub

Next, we’ll create a SignalR hub that will handle client connections and push stock price updates to connected users.

*   In the `StockMarketBot` project, create a folder called `Hubs` and add a `StockHub.cs` file:

```csharp
using Microsoft.AspNetCore.SignalR;  
  
namespace StockMarketBot.Hubs  
{  
    public class StockHub : Hub  
    {  
        public async Task SendStockUpdate(string symbol, decimal price)  
        {  
            await Clients.All.SendAsync("ReceiveStockUpdate", symbol, price);  
        }  
    }  
}
```

This hub has a method `SendStockUpdate` that sends stock price updates to all connected clients. When a price update is received, it broadcasts the stock symbol and price to the clients via the `ReceiveStockUpdate` method.

## 1.4 Configure SignalR in the ASP.NET Middleware

In `Program.cs`, configure SignalR to listen for incoming connections.

```csharp
var builder = WebApplication.CreateBuilder(args);  
var app = builder.Build();  
  
app.UseHttpsRedirection();  
app.UseRouting();  
  
app.MapHub<StockHub>("/stockHub");  
  
app.Run();
```

This code maps our `StockHub` endpoint to `/stockHub`, so clients can connect to this path to receive real-time updates.

# Step 2: Setting Up a Stock Data Service

To get live stock quotes, we’ll need an API from a stock market data provider. For this example, we’ll assume you’re using Finnhub (but you can use any provider you prefer).

## 2.1 Create a Service to Fetch Stock Prices

Add a new service `StockService.cs` to handle API calls to the stock provider.

*   In the `StockMarketBot` project, create a `Services` folder and add a `StockService.cs` file:

```csharp
using System.Net.Http;  
using System.Text.Json;  
using System.Threading.Tasks;  
  
public class StockService  
{  
    private readonly HttpClient _httpClient;  
    private readonly string _apiKey = "YOUR_API_KEY";  
  
    public StockService(HttpClient httpClient)  
    {  
        _httpClient = httpClient;  
    }  
  
    public async Task<decimal> GetStockPriceAsync(string symbol)  
    {  
        var response = await _httpClient.GetStringAsync($"https://finnhub.io/api/v1/quote?symbol={symbol}&token={_apiKey}");  
        var json = JsonDocument.Parse(response);  
        var price = json.RootElement.GetProperty("c").GetDecimal(); // "c" represents current price in Finnhub API  
        return price;  
    }  
}
```

Replace `"YOUR_API_KEY"` with your actual API key from Finnhub or your preferred provider. The `GetStockPriceAsync` method fetches the current price for a given stock symbol.

## 2.2 Register the Service and Schedule Price Updates

In `Program.cs`, register `StockService` in the DI container and set up a periodic task to fetch and broadcast price updates.

```csharp
builder.Services.AddHttpClient();  
builder.Services.AddSingleton<StockService>();  
builder.Services.AddSignalR();  
builder.Services.AddHostedService<StockPriceUpdater>();  
builder.Services.AddCors(options =>  
{  
    options.AddDefaultPolicy(  
        builder =>  
        {  
            // Run your Index.html for example with live server in VSC  
            builder.WithOrigins("http://127.0.0.1:5500")  
                .AllowAnyHeader()  
                .WithMethods("GET", "POST")  
                .AllowCredentials();  
        });  
});  
  
var app = builder.Build();  
app.MapHub<StockHub>("/stockHub");  
app.Run();
```

*   Add a `StockPriceUpdater.cs` in the `Services` folder. This background service will periodically fetch stock prices and broadcast updates:

```csharp
using Microsoft.AspNetCore.SignalR;  
using System.Threading;  
using System.Threading.Tasks;  
  
public class StockPriceUpdater : BackgroundService  
{  
    private readonly StockService _stockService;  
    private readonly IHubContext<StockHub> _hubContext;  
  
    public StockPriceUpdater(StockService stockService, IHubContext<StockHub> hubContext)  
    {  
        _stockService = stockService;  
        _hubContext = hubContext;  
    }  
  
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)  
    {  
        var symbols = new[] { "AAPL", "GOOGL", "MSFT" }; // Track specific stocks  
  
        while (!stoppingToken.IsCancellationRequested)  
        {  
            foreach (var symbol in symbols)  
            {  
                var price = await _stockService.GetStockPriceAsync(symbol);  
                await _hubContext.Clients.All.SendAsync("ReceiveStockUpdate", symbol, price);  
            }  
  
            await Task.Delay(5000, stoppingToken); // Update every 5 seconds  
        }  
    }  
}
```

In this code, `StockPriceUpdater` loops through an array of stock symbols (e.g., AAPL, GOOGL, MSFT) every 5 seconds, fetching the latest prices and broadcasting them to all connected clients via `ReceiveStockUpdate`.

# Step 3: Building the Front-End for Real-Time Stock Updates

Now that the back end is set up, let’s build a simple front-end interface to display stock quotes.

## 3.1 Create an `index.html` File

In the project root, create an `index.html` file with the following HTML and JavaScript to connect to SignalR:

```html
<!DOCTYPE html>  
<html lang="en">  
  <head>  
    <meta charset="UTF-8" />  
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />  
    <title>Real-Time Stock Market Bot</title>  
  </head>  
  
  <body>  
    <h1>Real-Time Stock Prices</h1>  
    <div id="stocks"></div>  
  
    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/6.0.1/signalr.js"></script>  
    <script>      const connection = new signalR.HubConnectionBuilder()  
        .withUrl("http://localhost:5159/stockHub")  
        .configureLogging(signalR.LogLevel.Information)  
        .build();  
  
      connection.onclose(async (error) => {  
        console.error("Connection interrupted:", error);  
        await start();  
      });  
  
      async function start() {  
        try {  
          await connection.start();  
          console.log("SignalR Connected.");  
          connection.on("ReceiveStockUpdate", (symbol, price) => {  
            const stockElement =  
              document.getElementById(symbol) || createStockElement(symbol);  
            stockElement.innerText = `${symbol}: $${price.toFixed(2)}`;  
          });  
  
          function createStockElement(symbol) {  
            const element = document.createElement("div");  
            element.id = symbol;  
            document.getElementById("stocks").appendChild(element);  
            return element;  
          }  
        } catch (err) {  
          console.error("Connection error:", err);  
          setTimeout(start, 5000);  
        }  
      }  
  
      start();    </script>  
  </body>  
</html>
```

In this HTML:

*   We connect to SignalR at `http://localhost:5159/stockHub`.
*   When a `ReceiveStockUpdate` message is received, the stock’s price is updated on the page. If the stock symbol is new, a new HTML element is created for it.

# Step 4: Running and Testing the Application 🧪

1.  **Run the .NET Server**:

    ```bash
    dotnet run
    ```

2.  **Open** `**index.html**` **in a Browser**: Open `index.html` in your browser and watch the live stock prices update every few seconds. If everything is set up correctly, you should see real-time stock price updates for the symbols being tracked!

# Possible Enhancements 🔍

1.  **Notifications**: Add alerts if a stock crosses a certain price threshold.
2.  **Dynamic Symbols**: Allow users to add or remove stocks to track in real-time.
3.  **Historical Data**: Show historical price charts by integrating with chart libraries.
4.  **User Authentication**: Secure the SignalR connection to only allow authenticated users to access stock data.

# Wrapping Up

Congratulations! You’ve built a real-time stock market bot using .NET, SignalR, and a stock API. This setup is highly flexible and can be extended to support notifications, dynamic updates, and personalized features. By integrating SignalR, you can create real-time applications that provide users with up-to-the-second information, making it perfect for tracking stocks or any other data that changes frequently.

# Stay Connected! 🚀

Follow me on [LinkedIn](https://www.linkedin.com/in/justin-m%C3%BCnch-0b1087133/) and [X](https://x.com/muench_justin) for more insights and tips. Check out my other posts for more on .NET, software development, and life as a senior developer!