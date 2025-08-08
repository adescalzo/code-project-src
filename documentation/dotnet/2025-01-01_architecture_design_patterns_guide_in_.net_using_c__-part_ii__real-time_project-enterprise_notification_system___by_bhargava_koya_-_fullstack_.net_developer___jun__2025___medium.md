# Design Patterns Guide in .NET Using C# -Part II: Real-Time Project-Enterprise Notification System | by Bhargava Koya - Fullstack .NET Developer | Jun, 2025 | Medium

**Source:** https://medium.com/@bhargavkoya56/design-patterns-guide-in-net-using-c-part-ii-real-time-project-enterprise-notification-system-53e3d51427cc
**Date Captured:** 2025-07-28T16:03:11.081Z
**Domain:** medium.com
**Author:** Bhargava Koya - Fullstack .NET Developer
**Category:** architecture

---

# Design Patterns Guide in .NET Using C# -Part II: Real-Time Project-Enterprise Notification System

[

![Bhargava Koya - Fullstack .NET Developer](https://miro.medium.com/v2/resize:fill:64:64/1*mXaNHb5mqlRTxc-4SqZPyA.jpeg)





](/@bhargavkoya56?source=post_page---byline--53e3d51427cc---------------------------------------)

[Bhargava Koya - Fullstack .NET Developer](/@bhargavkoya56?source=post_page---byline--53e3d51427cc---------------------------------------)

Follow

7 min read

·

Jun 25, 2025

42

3

Listen

Share

More

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/0*AVsx0QpQZ9-9KfDq.png)

Before deep dive into the implementation details let’s check the functional requirements of an application that we are going to build in this blog.

## Functional Requirements:

The enterprise notification system must support the following functional requirements:

1.  **Multi-Channel Delivery:** Support email, SMS, and push notification channels with extensible architecture for future channels.
2.  **Message Prioritization:** Handle urgent, normal, and low-priority messages with appropriate delivery guarantees.
3.  **User Preferences:** Respect user notification preferences and opt-out settings.
4.  **Retry Mechanism:** Implement intelligent retry logic for failed deliveries with exponential backoff.
5.  **Audit Trail:** Maintain comprehensive logs of all notification activities for compliance and debugging.
6.  **Batch Processing:** Support bulk notification sending for marketing campaigns and announcements.
7.  **Template Management:** Provide templating system for consistent message formatting across channels.
8.  **Rate Limiting:** Implement rate limiting to prevent overwhelming external services and users.

## Step-by-Step Implementation:

**Step 1: Core Notification Infrastructure using Singleton Pattern  
**The notification manager serves as the central coordinator for all notification activities, implemented as a thread-safe singleton.

public sealed class NotificationManager  
{  
    private static readonly Lazy<NotificationManager> \_instance =   
        new Lazy<NotificationManager>(() => new NotificationManager());  
      
    private readonly Dictionary<NotificationType, INotificationStrategy> \_strategies;  
    private readonly List<INotificationObserver> \_observers;  
    private readonly object \_lock = new object();  
      
    private NotificationManager()  
    {  
        \_strategies = new Dictionary<NotificationType, INotificationStrategy>();  
        \_observers = new List<INotificationObserver>();  
        InitializeStrategies();  
    }  
      
    public static NotificationManager Instance => \_instance.Value;  
      
    private void InitializeStrategies()  
    {  
        RegisterStrategy(NotificationType.Email, new EmailNotificationStrategy());  
        RegisterStrategy(NotificationType.SMS, new SmsNotificationStrategy());  
        RegisterStrategy(NotificationType.Push, new PushNotificationStrategy());  
    }  
      
    public void RegisterStrategy(NotificationType type, INotificationStrategy strategy)  
    {  
        lock (\_lock)  
        {  
            \_strategies\[type\] = strategy;  
        }  
    }  
      
    public async Task<NotificationResult> SendNotificationAsync(NotificationRequest request)  
    {  
        if (\_strategies.TryGetValue(request.Type, out var strategy))  
        {  
            var result = await strategy.SendAsync(request);  
            NotifyObservers(request, result);  
            return result;  
        }  
          
        throw new NotSupportedException($"Notification type {request.Type} is not supported");  
    }  
      
    public void Subscribe(INotificationObserver observer)  
    {  
        lock (\_lock)  
        {  
            \_observers.Add(observer);  
        }  
    }  
      
    private void NotifyObservers(NotificationRequest request, NotificationResult result)  
    {  
        foreach (var observer in \_observers)  
        {  
            observer.OnNotificationProcessed(request, result);  
        }  
    }  
}

**Step 2: Notification Strategies using Strategy Pattern  
**Each notification channel is implemented as a separate strategy, enabling easy addition of new channels.

public enum NotificationType  
{  
    Email,  
    SMS,  
    Push  
}  
  
public enum Priority  
{  
    Low,  
    Normal,  
    High,  
    Critical  
}  
  
public class NotificationRequest  
{  
    public string Id { get; set; } = Guid.NewGuid().ToString();  
    public NotificationType Type { get; set; }  
    public string Recipient { get; set; }  
    public string Subject { get; set; }  
    public string Message { get; set; }  
    public Priority Priority { get; set; }  
    public Dictionary<string, object\> Metadata { get; set; } = new Dictionary<string, object\>();  
    public DateTime ScheduledAt { get; set; } = DateTime.UtcNow;  
}  
  
public class NotificationResult  
{  
    public bool IsSuccess { get; set; }  
    public string ErrorMessage { get; set; }  
    public DateTime SentAt { get; set; }  
    public string ExternalId { get; set; }  
}  
  
public interface INotificationStrategy  
{  
    Task<NotificationResult> SendAsync(NotificationRequest request);  
}  
  
public class EmailNotificationStrategy : INotificationStrategy  
{  
    public async Task<NotificationResult> SendAsync(NotificationRequest request)  
    {  
        try  
        {  
            // Simulate email sending delay  
            await Task.Delay(Random.Shared.Next(100, 500));  
              
            Console.WriteLine($"Email sent to {request.Recipient}: {request.Subject}");  
              
            return new NotificationResult  
            {  
                IsSuccess = true,  
                SentAt = DateTime.UtcNow,  
                ExternalId = $"email\_{Guid.NewGuid():N}"  
            };  
        }  
        catch (Exception ex)  
        {  
            return new NotificationResult  
            {  
                IsSuccess = false,  
                ErrorMessage = ex.Message,  
                SentAt = DateTime.UtcNow  
            };  
        }  
    }  
}  
  
public class SmsNotificationStrategy : INotificationStrategy  
{  
    public async Task<NotificationResult> SendAsync(NotificationRequest request)  
    {  
        try  
        {  
            await Task.Delay(Random.Shared.Next(50, 200));  
              
            Console.WriteLine($"SMS sent to {request.Recipient}: {request.Message}");  
              
            return new NotificationResult  
            {  
                IsSuccess = true,  
                SentAt = DateTime.UtcNow,  
                ExternalId = $"sms\_{Guid.NewGuid():N}"  
            };  
        }  
        catch (Exception ex)  
        {  
            return new NotificationResult  
            {  
                IsSuccess = false,  
                ErrorMessage = ex.Message,  
                SentAt = DateTime.UtcNow  
            };  
        }  
    }  
}

**Step 3: Event Handling using Observer Pattern  
**The system uses observers to handle cross-cutting concerns like logging, metrics, and audit trails.

public interface INotificationObserver  
{  
    void OnNotificationProcessed(NotificationRequest request, NotificationResult result);  
}  
  
public class AuditObserver : INotificationObserver  
{  
    private readonly ILogger \_logger;  
      
    public AuditObserver(ILogger logger)  
    {  
        \_logger = logger;  
    }  
      
    public void OnNotificationProcessed(NotificationRequest request, NotificationResult result)  
    {  
        var auditEntry = new  
        {  
            RequestId = request.Id,  
            Type = request.Type.ToString(),  
            Recipient = request.Recipient,  
            Priority = request.Priority.ToString(),  
            Success = result.IsSuccess,  
            SentAt = result.SentAt,  
            ErrorMessage = result.ErrorMessage  
        };  
          
        \_logger.Log($"Audit: {JsonSerializer.Serialize(auditEntry)}");  
    }  
}  
  
public class MetricsObserver : INotificationObserver  
{  
    private static readonly Dictionary<NotificationType, int\> \_successCounts =   
        new Dictionary<NotificationType, int\>();  
    private static readonly Dictionary<NotificationType, int\> \_failureCounts =   
        new Dictionary<NotificationType, int\>();  
      
    public void OnNotificationProcessed(NotificationRequest request, NotificationResult result)  
    {  
        var counters = result.IsSuccess ? \_successCounts : \_failureCounts;  
          
        if (counters.ContainsKey(request.Type))  
            counters\[request.Type\]++;  
        else  
            counters\[request.Type\] = 1;  
          
        Console.WriteLine($"Metrics - {request.Type}: Success={\_successCounts.GetValueOrDefault(request.Type, 0)}, " +  
                         $"Failures={\_failureCounts.GetValueOrDefault(request.Type, 0)}");  
    }  
}

**Step 4: Enhanced Functionality using Decorator Pattern  
**Decorators add cross-cutting concerns like retry logic, rate limiting, and logging without modifying the core strategies.

public class RetryNotificationDecorator : INotificationStrategy  
{  
    private readonly INotificationStrategy \_inner;  
    private readonly int \_maxRetries;  
    private readonly TimeSpan \_baseDelay;  
      
    public RetryNotificationDecorator(INotificationStrategy inner, int maxRetries = 3, TimeSpan? baseDelay = null)  
    {  
        \_inner = inner;  
        \_maxRetries = maxRetries;  
        \_baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);  
    }  
      
    public async Task<NotificationResult> SendAsync(NotificationRequest request)  
    {  
        for (int attempt = 1; attempt <= \_maxRetries; attempt++)  
        {  
            try  
            {  
                var result = await \_inner.SendAsync(request);  
                if (result.IsSuccess)  
                    return result;  
                  
                if (attempt == \_maxRetries)  
                    return result;  
            }  
            catch (Exception ex)  
            {  
                if (attempt == \_maxRetries)  
                {  
                    return new NotificationResult  
                    {  
                        IsSuccess = false,  
                        ErrorMessage = ex.Message,  
                        SentAt = DateTime.UtcNow  
                    };  
                }  
            }  
              
            //Exponential backoff  
            var delay = TimeSpan.FromMilliseconds(\_baseDelay.TotalMilliseconds \* Math.Pow(2, attempt - 1));  
            await Task.Delay(delay);  
        }  
          
        return new NotificationResult  
        {  
            IsSuccess = false,  
            ErrorMessage = "Max retries exceeded",  
            SentAt = DateTime.UtcNow  
        };  
    }  
}  
  
public class RateLimitingDecorator : INotificationStrategy  
{  
    private readonly INotificationStrategy \_inner;  
    private readonly SemaphoreSlim \_semaphore;  
    private readonly TimeSpan \_timeWindow;  
    private readonly Queue<DateTime> \_requestTimes;  
      
    public RateLimitingDecorator(INotificationStrategy inner, int maxRequestsPerWindow, TimeSpan timeWindow)  
    {  
        \_inner = inner;  
        \_semaphore = new SemaphoreSlim(maxRequestsPerWindow, maxRequestsPerWindow);  
        \_timeWindow = timeWindow;  
        \_requestTimes = new Queue<DateTime>();  
    }  
      
    public async Task<NotificationResult> SendAsync(NotificationRequest request)  
    {  
        await \_semaphore.WaitAsync();  
          
        try  
        {  
            //Clean up old requests outside the time window  
            var cutoff = DateTime.UtcNow - \_timeWindow;  
            while (\_requestTimes.Count > 0 && \_requestTimes.Peek() < cutoff)  
            {  
                \_requestTimes.Dequeue();  
            }  
              
            \_requestTimes.Enqueue(DateTime.UtcNow);  
            return await \_inner.SendAsync(request);  
        }  
        finally  
        {  
            \_semaphore.Release();  
        }  
    }  
}

**Step 5: Simplified API using Facade Pattern  
**The facade provides a clean, simple interface for clients while managing the complexity of the underlying system.

public class NotificationFacade  
{  
    private readonly NotificationManager \_manager;  
    private readonly IUserPreferenceService \_preferences;  
    private readonly ITemplateService \_templates;  
      
    public NotificationFacade()  
    {  
        \_manager = NotificationManager.Instance;  
        \_preferences = new UserPreferenceService();  
        \_templates = new TemplateService();  
          
        //Register observers  
        \_manager.Subscribe(new AuditObserver(Logger.Instance));  
        \_manager.Subscribe(new MetricsObserver());  
          
        //Configure decorated strategies  
        SetupDecoratedStrategies();  
    }  
      
    private void SetupDecoratedStrategies()  
    {  
        //Email with retry and rate limiting  
        var emailStrategy = new EmailNotificationStrategy();  
        var decoratedEmail = new RateLimitingDecorator(  
            new RetryNotificationDecorator(emailStrategy, 3),  
            maxRequestsPerWindow: 10,  
            timeWindow: TimeSpan.FromMinutes(1)  
        );  
        \_manager.RegisterStrategy(NotificationType.Email, decoratedEmail);  
          
        //SMS with retry only  
        var smsStrategy = new SmsNotificationStrategy();  
        var decoratedSms = new RetryNotificationDecorator(smsStrategy, 2);  
        \_manager.RegisterStrategy(NotificationType.SMS, decoratedSms);  
    }  
      
    public async Task<NotificationResult> SendNotificationAsync(string userId, NotificationType type,   
        string templateName, Dictionary<string, object\> templateData)  
    {  
        //Check user preferences  
        var preferences = await \_preferences.GetUserPreferencesAsync(userId);  
        if (!preferences.IsChannelEnabled(type))  
        {  
            return new NotificationResult  
            {  
                IsSuccess = false,  
                ErrorMessage = "User has disabled this notification channel",  
                SentAt = DateTime.UtcNow  
            };  
        }  
          
        //Process template  
        var template = await \_templates.GetTemplateAsync(templateName, type);  
        var processedContent = template.Process(templateData);  
          
        //Create request  
        var request = new NotificationRequest  
        {  
            Type = type,  
            Recipient = preferences.GetChannelAddress(type),  
            Subject = processedContent.Subject,  
            Message = processedContent.Body,  
            Priority = Priority.Normal  
        };  
          
        return await \_manager.SendNotificationAsync(request);  
    }  
      
    public async Task<List<NotificationResult>> SendBatchNotificationAsync(  
        List<string\> userIds, NotificationType type, string templateName,   
        Dictionary<string, object\> templateData)  
    {  
        var tasks = userIds.Select(userId =>   
            SendNotificationAsync(userId, type, templateName, templateData));  
          
        return (await Task.WhenAll(tasks)).ToList();  
    }  
}

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/0*FP-S6d4BYeyBQ2Az.png)

Notification System Architecture-Complete Design Pattern Implementation

**Step 6: Supporting Services and Integration  
**The complete system includes supporting services that work together to provide a comprehensive notification solution.

public interface IUserPreferenceService  
{  
    Task<UserPreferences> GetUserPreferencesAsync(string userId);  
}  
  
public class UserPreferences  
{  
    public string UserId { get; set; }  
    public Dictionary<NotificationType, bool\> EnabledChannels { get; set; }  
    public Dictionary<NotificationType, string\> ChannelAddresses { get; set; }  
      
    public bool IsChannelEnabled(NotificationType type)  
    {  
        return EnabledChannels.GetValueOrDefault(type, false);  
    }  
      
    public string GetChannelAddress(NotificationType type)  
    {  
        return ChannelAddresses.GetValueOrDefault(type, string.Empty);  
    }  
}  
  
public interface ITemplateService  
{  
    Task<NotificationTemplate> GetTemplateAsync(string name, NotificationType type);  
}  
  
public class NotificationTemplate  
{  
    public string Name { get; set; }  
    public NotificationType Type { get; set; }  
    public string SubjectTemplate { get; set; }  
    public string BodyTemplate { get; set; }  
      
    public ProcessedTemplate Process(Dictionary<string, object\> data)  
    {  
        return new ProcessedTemplate  
        {  
            Subject = ProcessTemplate(SubjectTemplate, data),  
            Body = ProcessTemplate(BodyTemplate, data)  
        };  
    }  
      
    private string ProcessTemplate(string template, Dictionary<string, object\> data)  
    {  
        var result = template;  
        foreach (var kvp in data)  
        {  
            result = result.Replace($"{{{kvp.Key}}}", kvp.Value?.ToString() ?? string.Empty);  
        }  
        return result;  
    }  
}  
  
public class ProcessedTemplate  
{  
    public string Subject { get; set; }  
    public string Body { get; set; }  
}  
  
//Example usage  
public class Program  
{  
    public static async Task Main(string\[\] args)  
    {  
        var facade = new NotificationFacade();  
          
        //Send single notification  
        var result = await facade.SendNotificationAsync(  
            userId: "user123",  
            type: NotificationType.Email,  
            templateName: "welcome",  
            templateData: new Dictionary<string, object\>  
            {  
                { "name", "John Doe" },  
                { "product", "Enterprise Suite" }  
            }  
        );  
          
        Console.WriteLine($"Notification sent: {result.IsSuccess}");  
          
        //Send batch notifications  
        var userIds = new List<string\> { "user1", "user2", "user3" };  
        var batchResults = await facade.SendBatchNotificationAsync(  
            userIds,   
            NotificationType.SMS,   
            "promotion",   
            new Dictionary<string, object\> { { "discount", "20%" } }  
        );  
          
        Console.WriteLine($"Batch notifications sent: {batchResults.Count(r => r.IsSuccess)}/{batchResults.Count}");  
    }  
}

This comprehensive notification system demonstrates how multiple design patterns work together to create a robust, scalable, and maintainable solution. The architecture supports extensibility through the Strategy pattern, maintains single responsibility through the Decorator pattern, provides centralized management through the Singleton pattern, enables event-driven functionality through the Observer pattern, and offers a clean interface through the Facade pattern.

The system can be easily extended with new notification channels, enhanced with additional cross-cutting concerns, and integrated into existing enterprise applications. This practical implementation showcases the power of design patterns in solving real-world software engineering challenges.

## What’s Next?

**Part 3: Best Practices, Anti-Patterns, and Advanced Guidance**  
In the final part, we’ll focus on:

*   Best practices for applying design patterns in modern .NET development.
*   How to recognize and avoid common anti-patterns that can undermine your architecture.
*   Testing strategies for pattern-based code.
*   Performance considerations and integration with advanced .NET features like dependency injection and async programming.
*   Guidelines for choosing the right pattern for your specific scenario.

I hope you enjoyed reading this blog!

I’d love to hear your thoughts. please share your feedback or questions in the comments below and let me know if you’d like any clarifications on the topics covered. If you enjoyed this blog, don’t forget to like it and subscribe for more technology insights.

**Thank you for joining me on this learning journey!**