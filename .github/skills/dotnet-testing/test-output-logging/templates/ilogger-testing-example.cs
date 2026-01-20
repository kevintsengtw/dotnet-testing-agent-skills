using System;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

/// <summary>
/// ILogger 測試策略範例
/// 展示如何在測試中驗證 ILogger 的記錄行為
/// </summary>
public class ILoggerTestingExample
{
    // ===== 方法 1: 使用 AbstractLogger 簡化測試 =====

    [Fact]
    public void WithAbstractLogger_付款失敗_應記錄錯誤()
    {
        // Arrange
        var logger = Substitute.For<AbstractLogger<PaymentService>>();
        var paymentGateway = Substitute.For<IPaymentGateway>();
        paymentGateway.ProcessPayment(Arg.Any<decimal>()).Returns(new PaymentResult 
        { 
            Success = false, 
            ErrorMessage = "餘額不足" 
        });

        var service = new PaymentService(logger, paymentGateway);

        // Act
        service.ProcessPayment(1000);

        // Assert
        logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<Exception>(),
            Arg.Is<string>(msg => msg.Contains("付款失敗") && msg.Contains("餘額不足"))
        );
    }

    // ===== 方法 2: 直接攔截 Log<TState> 方法（複雜但完整） =====

    [Fact]
    public void WithStandardILogger_付款成功_應記錄資訊()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PaymentService>>();
        var paymentGateway = Substitute.For<IPaymentGateway>();
        paymentGateway.ProcessPayment(Arg.Any<decimal>()).Returns(new PaymentResult 
        { 
            Success = true 
        });

        var service = new PaymentService(logger, paymentGateway);

        // Act
        service.ProcessPayment(1000);

        // Assert - 攔截底層 Log<TState> 方法
        logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains("付款成功")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>()
        );
    }

    // ===== 方法 3: 錯誤示範 - 直接 Mock 擴充方法會失敗 =====

    [Fact]
    public void WrongApproach_這種方式會失敗()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PaymentService>>();
        var service = new PaymentService(logger, null);

        // Act
        // service.ProcessPayment(1000);

        // Assert - ❌ 這樣寫會失敗，因為 LogError 是擴充方法
        // logger.Received().LogError(Arg.Any<string>()); // ❌ 不可行
    }
}

// ===== AbstractLogger 抽象層實作 =====

/// <summary>
/// 簡化的 ILogger 抽象層，用於簡化測試
/// </summary>
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

    // 簡化的抽象方法，更容易測試
    public abstract void Log(LogLevel logLevel, Exception ex, string information);
}

// ===== 被測試的服務類別 =====

public class PaymentService
{
    private readonly ILogger<PaymentService> _logger;
    private readonly IPaymentGateway _paymentGateway;

    public PaymentService(ILogger<PaymentService> logger, IPaymentGateway paymentGateway)
    {
        _logger = logger;
        _paymentGateway = paymentGateway;
    }

    public void ProcessPayment(decimal amount)
    {
        _logger.LogInformation($"開始處理付款，金額：${amount}");

        var result = _paymentGateway.ProcessPayment(amount);

        if (result.Success)
        {
            _logger.LogInformation($"付款成功，金額：${amount}");
        }
        else
        {
            _logger.LogError($"付款失敗：{result.ErrorMessage}");
        }
    }
}

// ===== 相依介面定義 =====

public interface IPaymentGateway
{
    PaymentResult ProcessPayment(decimal amount);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
