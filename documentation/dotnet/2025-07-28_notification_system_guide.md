# Design Patterns Guide in .NET Using C# - Part II: Real-Time Project-Enterprise Notification System

## Functional Requirements

The enterprise notification system must support the following functional requirements:

- **Multi-Channel Delivery**: Support email, SMS, and push notification channels with extensible architecture for future channels.
- **Message Prioritization**: Handle urgent, normal, and low-priority messages with appropriate delivery guarantees.
- **User Preferences**: Respect user notification preferences and opt-out settings.
- **Retry Mechanism**: Implement intelligent retry logic for failed deliveries with exponential backoff.
- **Audit Trail**: Maintain comprehensive logs of all notification activities for compliance and debugging.
- **Batch Processing**: Support bulk notification sending for marketing campaigns and announcements.
- **Template Management**: Provide templating system for consistent message formatting across channels.
- **Rate Limiting**: Implement rate limiting to prevent overwhelming external services and users.

## Step-by-Step Implementation

### Step 1: Core Notification Infrastructure using Singleton Pattern

The notification manager serves as the central coordinator for all notification activities, implemented as a thread-safe singleton.

```csharp
public sealed class NotificationManager
{
    private static readonly Lazy<NotificationManager> _instance =
        new Lazy<NotificationManager>(() => new NotificationManager());
    private readonly Dictionary<NotificationType, INotificationStrategy> _strategies;
    private readonly List<INotificationObserver> _observers;
    private readonly object _lock = new object();

    private NotificationManager()
    {
        _strategies = new Dictionary<NotificationType, INotificationStrategy>();
        _observers = new List<INotificationObserver>();
        InitializeStrategies();
    }

    public static NotificationManager Instance => _instance.Value;

    private void InitializeStrategies()
    {
        RegisterStrategy(NotificationType.Email, new EmailNotificationStrategy());
        RegisterStrategy(NotificationType.SMS, new SmsNotificationStrategy());
        RegisterStrategy(NotificationType.Push, new PushNotificationStrategy());
    }

    public void RegisterStrategy(NotificationType type, INotificationStrategy strategy)
    {
        lock (_lock)
        {
            _strategies[type] = strategy;
        }
    }

    public async Task<NotificationResult> SendNotificationAsync(NotificationRequest request)
    {
        if (_strategies.TryGetValue(request.Type, out var strategy))
        {
            var result = await strategy.SendAsync(request);
            NotifyObservers(request, result);
            return result;
        }
        throw new NotSupportedException($"Notification type {request.Type} is not supported");
    }

    public void Subscribe(INotificationObserver observer)
    {
        lock (_lock)
        {
            _observers.Add(observer);
        }
    }

    private void NotifyObservers(NotificationRequest request, NotificationResult result)
    {
        foreach (var observer in _observers)
        {
            observer.OnNotificationProcessed(request, result);
        }
    }
}
```

### Step 2: Notification Strategies using Strategy Pattern

Each notification channel is implemented as a separate strategy, enabling easy addition of new channels.

```csharp
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
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
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
                ExternalId = $"email_{Guid.NewGuid():N}"
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
                ExternalId = $"sms_{Guid.NewGuid():N}"
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
```

### Step 3: Event Handling using Observer Pattern

The system uses observers to handle cross-cutting concerns like logging, metrics, and audit trails.

```csharp
public interface INotificationObserver
{
    void OnNotificationProcessed(NotificationRequest request, NotificationResult result);
}

public class AuditObserver : INotificationObserver
{
    private readonly ILogger _logger;

    public AuditObserver(ILogger logger)
    {
        _logger = logger;
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

        _logger.Log($"Audit: {JsonSerializer.Serialize(auditEntry)}");
    }
}

public class MetricsObserver : INotificationObserver
{
    private static readonly Dictionary<NotificationType, int> _successCounts =
        new Dictionary<NotificationType, int>();
    private static readonly Dictionary<NotificationType, int> _failureCounts =
        new Dictionary<NotificationType, int>();

    public void OnNotificationProcessed(NotificationRequest request, NotificationResult result)
    {
        var counters = result.IsSuccess ? _successCounts : _failureCounts;
        if (counters.ContainsKey(request.Type))
            counters[request.Type]++;
        else
            counters[request.Type] = 1;

        Console.WriteLine($"Metrics - {request.Type}: Success={_successCounts.GetValueOrDefault(request.Type, 0)}, " +
                         $"Failures={_failureCounts.GetValueOrDefault(request.Type, 0)}");
    }
}
```

### Step 4: Enhanced Functionality using Decorator Pattern

Decorators add cross-cutting concerns like retry logic, rate limiting, and logging without modifying the core strategies.

```csharp
public class RetryNotificationDecorator : INotificationStrategy
{
    private readonly INotificationStrategy _inner;
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;

    public RetryNotificationDecorator(INotificationStrategy inner, int maxRetries = 3, TimeSpan? baseDelay = null)
    {
        _inner = inner;
        _maxRetries = maxRetries;
        _baseDelay = baseDelay ?? TimeSpan.FromSeconds(1);
    }

    public async Task<NotificationResult> SendAsync(NotificationRequest request)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                var result = await _inner.SendAsync(request);
                if (result.IsSuccess)
                    return result;

                if (attempt == _maxRetries)
                    return result;
            }
            catch (Exception ex)
            {
                if (attempt == _maxRetries)
                {
                    return new NotificationResult
                    {
                        IsSuccess = false,
                        ErrorMessage = ex.Message,
                        SentAt = DateTime.UtcNow
                    };
                }
            }

            // Exponential backoff
            var delay = TimeSpan.FromMilliseconds(_baseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
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
    private readonly INotificationStrategy _inner;
    private readonly SemaphoreSlim _semaphore;
    private readonly TimeSpan _timeWindow;
    private readonly Queue<DateTime> _requestTimes;

    public RateLimitingDecorator(INotificationStrategy inner, int maxRequestsPerWindow, TimeSpan timeWindow)
    {
        _inner = inner;
        _semaphore = new SemaphoreSlim(maxRequestsPerWindow, maxRequestsPerWindow);
        _timeWindow = timeWindow;
        _requestTimes = new Queue<DateTime>();
    }

    public async Task<NotificationResult> SendAsync(NotificationRequest request)
    {
        await _semaphore.WaitAsync();
        try
        {
            // Clean up old requests outside the time window
            var cutoff = DateTime.UtcNow - _timeWindow;
            while (_requestTimes.Count > 0 && _requestTimes.Peek() < cutoff)
            {
                _requestTimes.Dequeue();
            }

            _requestTimes.Enqueue(DateTime.UtcNow);
            return await _inner.SendAsync(request);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### Step 5: Simplified API using Facade Pattern

The facade provides a clean, simple interface for clients while managing the complexity of the underlying system.

```csharp
public class NotificationFacade
{
    private readonly NotificationManager _manager;
    private readonly IUserPreferenceService _preferences;
    private readonly ITemplateService _templates;

    public NotificationFacade()
    {
        _manager = NotificationManager.Instance;
        _preferences = new UserPreferenceService();
        _templates = new TemplateService();

        // Register observers
        _manager.Subscribe(new AuditObserver(Logger.Instance));
        _manager.Subscribe(new MetricsObserver());

        // Configure decorated strategies
        SetupDecoratedStrategies();
    }

    private void SetupDecoratedStrategies()
    {
        // Email with retry and rate limiting
        var emailStrategy = new EmailNotificationStrategy();
        var decoratedEmail = new RateLimitingDecorator(
            new RetryNotificationDecorator(emailStrategy, 3),
            maxRequestsPerWindow: 10,
            timeWindow: TimeSpan.FromMinutes(1)
        );
        _manager.RegisterStrategy(NotificationType.Email, decoratedEmail);

        // SMS with retry only
        var smsStrategy = new SmsNotificationStrategy();
        var decoratedSms = new RetryNotificationDecorator(smsStrategy, 2);
        _manager.RegisterStrategy(NotificationType.SMS, decoratedSms);
    }

    public async Task<NotificationResult> SendNotificationAsync(string userId, NotificationType type,
        string templateName, Dictionary<string, object> templateData)
    {
        // Check user preferences
        var preferences = await _preferences.GetUserPreferencesAsync(userId);
        if (!preferences.IsChannelEnabled(type))
        {
            return new NotificationResult
            {
                IsSuccess = false,
                ErrorMessage = "User has disabled this notification channel",
                SentAt = DateTime.UtcNow
            };
        }

        // Process template
        var template = await _templates.GetTemplateAsync(templateName, type);
        var processedContent = template.Process(templateData);

        // Create request
        var request = new NotificationRequest
        {
            Type = type,
            Recipient = preferences.GetChannelAddress(type),
            Subject = processedContent.Subject,
            Message = processedContent.Body,
            Priority = Priority.Normal
        };

        return await _manager.SendNotificationAsync(request);
    }

    public async Task<List<NotificationResult>> SendBatchNotificationAsync(
        List<string> userIds, NotificationType type, string templateName,
        Dictionary<string, object> templateData)
    {
        var tasks = userIds.Select(userId =>
            SendNotificationAsync(userId, type, templateName, templateData));
        return (await Task.WhenAll(tasks)).ToList();
    }
}
```

### Step 6: Supporting Services and Integration

The complete system includes supporting services that work together to provide a comprehensive notification solution.

```csharp
public interface IUserPreferenceService
{
    Task<UserPreferences> GetUserPreferencesAsync(string userId);
}

public class UserPreferences
{
    public string UserId { get; set; }
    public Dictionary<NotificationType, bool> EnabledChannels { get; set; }
    public Dictionary<NotificationType, string> ChannelAddresses { get; set; }

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

    public ProcessedTemplate Process(Dictionary<string, object> data)
    {
        return new ProcessedTemplate
        {
            Subject = ProcessTemplate(SubjectTemplate, data),
            Body = ProcessTemplate(BodyTemplate, data)
        };
    }

    private string ProcessTemplate(string template, Dictionary<string, object> data)
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

// Example usage
public class Program
{
    public static async Task Main(string[] args)
    {
        var facade = new NotificationFacade();

        // Send single notification
        var result = await facade.SendNotificationAsync(
            userId: "user123",
            type: NotificationType.Email,
            templateName: "welcome",
            templateData: new Dictionary<string, object>
            {
                { "name", "John Doe" },
                { "product", "Enterprise Suite" }
            }
        );

        Console.WriteLine($"Notification sent: {result.IsSuccess}");

        // Send batch notifications
        var userIds = new List<string> { "user1", "user2", "user3" };
        var batchResults = await facade.SendBatchNotificationAsync(
            userIds,
            NotificationType.SMS,
            "promotion",
            new Dictionary<string, object> { { "discount", "20%" } }
        );

        Console.WriteLine($"Batch notifications sent: {batchResults.Count(r => r.IsSuccess)}/{batchResults.Count}");
    }
}
```

## What's Next?

**Part 3: Best Practices, Anti-Patterns, and Advanced Guidance**

In the final part, we'll focus on:
- Best practices for applying design patterns in modern .NET development.
- How to recognize and avoid common anti-patterns that can undermine your architecture.
- Testing strategies for pattern-based code.
- Performance considerations and integration with advanced .NET features like dependency injection and async programming.
- Guidelines for choosing the right pattern for your specific scenario.

---

## Technical Analysis

### Design Patterns Implementation Quality

#### 1. **Singleton Pattern Analysis**
**Strengths:**
- Proper lazy initialization using `Lazy<T>` ensures thread safety
- Clean API with static `Instance` property
- Maintains state consistently across application lifecycle

**Potential Issues:**
- Singleton makes unit testing difficult due to global state
- Violates dependency injection principles
- Could create tight coupling throughout the application

**Recommendation:** Consider using dependency injection container with singleton lifetime instead of static singleton pattern.

#### 2. **Strategy Pattern Analysis**
**Strengths:**
- Excellent separation of concerns - each notification type has its own strategy
- Easy to extend with new notification channels
- Clean interface design with `INotificationStrategy`

**Implementation Quality:** ⭐⭐⭐⭐⭐ Excellent - This is a textbook implementation of the Strategy pattern.

#### 3. **Observer Pattern Analysis**
**Strengths:**
- Decouples cross-cutting concerns (auditing, metrics) from core business logic
- Easy to add new observers without modifying existing code
- Follows open/closed principle

**Potential Improvements:**
- Consider using .NET events or `IObservable<T>` for better integration with .NET ecosystem
- Add error handling for observer failures to prevent cascading failures

#### 4. **Decorator Pattern Analysis**
**Strengths:**
- Excellent implementation of cross-cutting concerns
- Composable decorators allow flexible combinations
- Follows single responsibility principle

**Technical Excellence:**
- Proper exponential backoff implementation in retry decorator
- Thread-safe rate limiting using `SemaphoreSlim`
- Clean composition in facade setup

#### 5. **Facade Pattern Analysis**
**Strengths:**
- Provides simplified API hiding complex orchestration
- Integrates user preferences and templates seamlessly
- Supports both single and batch operations

**Areas for Enhancement:**
- Could benefit from configuration injection
- Template processing could be more robust (error handling for missing variables)

### Architecture Assessment

#### **Scalability Considerations**
- **Rate Limiting:** Well-implemented sliding window approach
- **Batch Processing:** Supports concurrent processing with `Task.WhenAll`
- **Async/Await:** Proper async implementation throughout

#### **Reliability Features**
- **Retry Logic:** Exponential backoff prevents overwhelming failing services
- **Error Handling:** Comprehensive error propagation and logging
- **Audit Trail:** Complete observability for compliance requirements

#### **Maintainability Score: 8/10**
- Clear separation of concerns
- Consistent coding patterns
- Good abstraction layers
- Missing: comprehensive configuration management

### Production Readiness Assessment

#### **What's Missing for Production:**
1. **Configuration Management:** Hard-coded values should be externalized
2. **Dependency Injection:** Replace singleton with DI container
3. **Health Checks:** Add monitoring endpoints for system health
4. **Circuit Breaker:** Protect against cascade failures
5. **Dead Letter Queue:** Handle persistently failing messages
6. **Metrics Collection:** Integration with application performance monitoring
7. **Structured Logging:** Replace console logging with structured logging framework

#### **Security Considerations:**
- **PII Handling:** Ensure notification content doesn't expose sensitive data in logs
- **Rate Limiting:** Implement per-user rate limiting to prevent abuse
- **Input Validation:** Add validation for notification content and templates

### Code Quality Metrics

| Aspect | Score | Notes |
|--------|-------|-------|
| **Design Patterns Usage** | 9/10 | Excellent pattern implementation |
| **Code Organization** | 8/10 | Clear structure, good naming |
| **Error Handling** | 7/10 | Good but could be more comprehensive |
| **Performance** | 8/10 | Async throughout, efficient algorithms |
| **Testability** | 6/10 | Singleton makes testing challenging |
| **Extensibility** | 9/10 | Easy to add new channels and features |

### Recommended Improvements

#### **High Priority:**
1. **Replace Singleton with DI:** Use dependency injection container
2. **Add Configuration:** External configuration for timeouts, retry counts, etc.
3. **Implement Circuit Breaker:** Protect against cascade failures
4. **Add Unit Tests:** Comprehensive test coverage

#### **Medium Priority:**
1. **Dead Letter Queue:** Handle permanently failed messages
2. **Structured Logging:** Replace console output with proper logging
3. **Health Checks:** Add monitoring endpoints
4. **Input Validation:** Validate all inputs thoroughly

#### **Low Priority:**
1. **Performance Monitoring:** Add metrics collection
2. **Background Processing:** Consider using background services for non-urgent notifications
3. **Message Queuing:** Integration with message brokers for high volume

### Summary

This is an **excellent educational example** demonstrating multiple design patterns working together cohesively. The implementation quality is high, showing proper understanding of each pattern's purpose and benefits. However, for production use, several enterprise-grade features would need to be added, particularly around configuration management, dependency injection, and comprehensive error handling.

The code effectively demonstrates how design patterns solve real-world problems and can serve as a solid foundation for building production notification systems.