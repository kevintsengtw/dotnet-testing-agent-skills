using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// XUnitLogger 與 CompositeLogger 診斷工具範例
/// 展示如何同時進行行為驗證與測試輸出診斷
/// </summary>
public class DiagnosticToolsExample
{
    private readonly ITestOutputHelper _output;

    public DiagnosticToolsExample(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void WithCompositeLogger_同時驗證與輸出()
    {
        // Arrange - 組合 Mock Logger 與 XUnit Logger
        var mockLogger = Substitute.For<AbstractLogger<OrderService>>();
        var xunitLogger = new XUnitLogger<OrderService>(_output);
        var compositeLogger = new CompositeLogger<OrderService>(mockLogger, xunitLogger);

        var service = new OrderService(compositeLogger);

        // Act
        service.ProcessOrder("ORD001", 1500);

        // Assert - 可以驗證 Mock Logger 的行為
        mockLogger.Received().Log(
            LogLevel.Information,
            Arg.Any<Exception>(),
            Arg.Is<string>(msg => msg.Contains("開始處理訂單"))
        );

        // 同時，測試輸出中會顯示實際的記錄訊息，便於診斷
    }
}

// ===== XUnitLogger 實作 =====

/// <summary>
/// 將 ILogger 輸出導向 xUnit 測試輸出
/// </summary>
public class XUnitLogger<T> : ILogger<T>
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly string _categoryName;

    public XUnitLogger(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        _categoryName = typeof(T).Name;
    }

    public IDisposable BeginScope<TState>(TState state) => new NoOpDisposable();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        if (formatter == null)
        {
            return;
        }

        var message = formatter(state, exception);
        
        // 格式化輸出：[時間] [層級] [類別] 訊息
        _testOutputHelper.WriteLine(
            $"[{DateTime.Now:HH:mm:ss.fff}] [{logLevel}] [{_categoryName}] {message}"
        );

        if (exception != null)
        {
            _testOutputHelper.WriteLine($"Exception: {exception}");
        }
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

// ===== CompositeLogger 實作 =====

/// <summary>
/// 組合多個 Logger，同時將記錄發送到所有內部 Logger
/// </summary>
public class CompositeLogger<T> : ILogger<T>
{
    private readonly ILogger<T>[] _loggers;

    public CompositeLogger(params ILogger<T>[] loggers)
    {
        _loggers = loggers ?? throw new ArgumentNullException(nameof(loggers));
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        var disposables = _loggers.Select(logger => logger.BeginScope(state)).ToArray();
        return new CompositeDisposable(disposables);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _loggers.Any(logger => logger.IsEnabled(logLevel));
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        foreach (var logger in _loggers)
        {
            if (logger.IsEnabled(logLevel))
            {
                logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}

/// <summary>
/// 組合多個 IDisposable 物件
/// </summary>
public class CompositeDisposable : IDisposable
{
    private readonly IDisposable[] _disposables;

    public CompositeDisposable(params IDisposable[] disposables)
    {
        _disposables = disposables;
    }

    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            disposable?.Dispose();
        }
    }
}

// ===== TestLogger 實作（用於收集記錄） =====

/// <summary>
/// 測試用 Logger，收集所有記錄以供驗證
/// </summary>
public class TestLogger<T> : ILogger<T>
{
    private readonly ConcurrentBag<LogEntry> _logs = new ConcurrentBag<LogEntry>();

    public IReadOnlyCollection<LogEntry> Logs => _logs.ToList();

    public IDisposable BeginScope<TState>(TState state) => new NoOpDisposable();

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        var message = formatter?.Invoke(state, exception) ?? state?.ToString();
        
        _logs.Add(new LogEntry
        {
            LogLevel = logLevel,
            Message = message,
            Exception = exception,
            Timestamp = DateTime.Now
        });
    }

    public bool HasLog(LogLevel level, string messageContains)
    {
        return _logs.Any(log => 
            log.LogLevel == level && 
            log.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}

/// <summary>
/// 記錄項目
/// </summary>
public class LogEntry
{
    public LogLevel LogLevel { get; set; }
    public string Message { get; set; }
    public Exception Exception { get; set; }
    public DateTime Timestamp { get; set; }
}

// ===== 被測試的服務類別 =====

public class OrderService
{
    private readonly ILogger<OrderService> _logger;

    public OrderService(ILogger<OrderService> logger)
    {
        _logger = logger;
    }

    public void ProcessOrder(string orderId, decimal amount)
    {
        _logger.LogInformation($"開始處理訂單 {orderId}，金額：${amount}");

        // 模擬處理邏輯
        if (amount > 0)
        {
            _logger.LogInformation($"訂單 {orderId} 處理完成");
        }
        else
        {
            _logger.LogError($"訂單 {orderId} 金額無效");
        }
    }
}

// ===== AbstractLogger（從前一個範例複製） =====

public abstract class AbstractLogger<T> : ILogger<T>
{
    public IDisposable BeginScope<TState>(TState state) => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception exception,
        Func<TState, Exception, string> formatter)
    {
        Log(logLevel, exception, state?.ToString() ?? string.Empty);
    }

    public abstract void Log(LogLevel logLevel, Exception ex, string information);
}
