// =============================================================================
// FakeTimeProvider 測試範例
// 展示如何使用 FakeTimeProvider 進行時間控制測試
// =============================================================================

using System;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace TimeProviderExamples.Tests;

#region FakeTimeProvider 擴充方法

/// <summary>
/// FakeTimeProvider 擴充方法，簡化時間設定
/// </summary>
public static class FakeTimeProviderExtensions
{
    /// <summary>
    /// 設定 FakeTimeProvider 的本地時間
    /// </summary>
    /// <param name="fakeTimeProvider">FakeTimeProvider 實例</param>
    /// <param name="localDateTime">要設定的本地時間</param>
    public static void SetLocalNow(this FakeTimeProvider fakeTimeProvider, DateTime localDateTime)
    {
        fakeTimeProvider.SetLocalTimeZone(TimeZoneInfo.Local);
        var utcTime = TimeZoneInfo.ConvertTimeToUtc(localDateTime, TimeZoneInfo.Local);
        fakeTimeProvider.SetUtcNow(utcTime);
    }
    
    /// <summary>
    /// 設定 FakeTimeProvider 為特定日期的特定小時
    /// </summary>
    public static void SetLocalNow(this FakeTimeProvider fakeTimeProvider, int year, int month, int day, int hour, int minute = 0, int second = 0)
    {
        var localDateTime = new DateTime(year, month, day, hour, minute, second);
        fakeTimeProvider.SetLocalNow(localDateTime);
    }
}

#endregion

#region 基礎時間控制測試

/// <summary>
/// OrderService 的單元測試 - 展示 FakeTimeProvider 基礎用法
/// </summary>
public class OrderServiceTests
{
    [Fact]
    public void CanPlaceOrder_在營業時間內_應回傳True()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        
        // 設定為下午 2 點（營業時間內）
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0));
        
        var sut = new OrderService(fakeTimeProvider);
        
        // Act
        var result = sut.CanPlaceOrder();
        
        // Assert
        result.Should().BeTrue();
    }
    
    [Fact]
    public void CanPlaceOrder_在營業時間外_應回傳False()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        
        // 設定為晚上 8 點（營業時間外）
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 20, 0, 0));
        
        var sut = new OrderService(fakeTimeProvider);
        
        // Act
        var result = sut.CanPlaceOrder();
        
        // Assert
        result.Should().BeFalse();
    }
    
    [Fact]
    public void GetTimeBasedDiscount_週五_應回傳九折優惠()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        
        // 2024/3/15 是週五
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 14, 0, 0));
        
        var sut = new OrderService(fakeTimeProvider);
        
        // Act
        var discount = sut.GetTimeBasedDiscount();
        
        // Assert
        discount.Should().Be("週五快樂：九折優惠");
    }
    
    [Fact]
    public void GetTimeBasedDiscount_聖誕節_應回傳八折優惠()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        
        // 聖誕節
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 12, 25, 10, 0, 0));
        
        var sut = new OrderService(fakeTimeProvider);
        
        // Act
        var discount = sut.GetTimeBasedDiscount();
        
        // Assert
        discount.Should().Be("聖誕特惠：八折優惠");
    }
    
    [Fact]
    public void GetTimeBasedDiscount_一般日期_應回傳無優惠()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        
        // 2024/3/11 是週一（一般日期）
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 11, 14, 0, 0));
        
        var sut = new OrderService(fakeTimeProvider);
        
        // Act
        var discount = sut.GetTimeBasedDiscount();
        
        // Assert
        discount.Should().Be("無優惠");
    }
}

#endregion

#region 參數化邊界測試

/// <summary>
/// 參數化測試 - 涵蓋所有邊界條件
/// </summary>
public class OrderServiceBoundaryTests
{
    [Theory]
    [InlineData(8, false)]   // 上午 8 點 - 營業時間前
    [InlineData(9, true)]    // 上午 9 點 - 剛開始營業（邊界）
    [InlineData(12, true)]   // 中午 12 點 - 營業時間內
    [InlineData(16, true)]   // 下午 4 點 - 營業時間內
    [InlineData(17, false)]  // 下午 5 點 - 剛結束營業（邊界）
    [InlineData(18, false)]  // 下午 6 點 - 營業時間後
    [InlineData(0, false)]   // 凌晨 0 點 - 深夜
    [InlineData(23, false)]  // 晚上 11 點 - 深夜
    public void CanPlaceOrder_不同時間點_應回傳正確結果(int hour, bool expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, hour, 0, 0));
        
        var sut = new OrderService(fakeTimeProvider);
        
        // Act
        var result = sut.CanPlaceOrder();
        
        // Assert
        result.Should().Be(expected);
    }
    
    [Theory]
    [InlineData("2024-03-15", "週五快樂：九折優惠")]  // 週五
    [InlineData("2024-03-11", "無優惠")]               // 週一
    [InlineData("2024-12-25", "聖誕特惠：八折優惠")]  // 聖誕節
    [InlineData("2024-03-16", "無優惠")]               // 週六（非週五）
    public void GetTimeBasedDiscount_不同日期_應回傳正確優惠(string dateStr, string expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var date = DateTime.Parse(dateStr);
        fakeTimeProvider.SetLocalNow(date.AddHours(12)); // 中午 12 點
        
        var sut = new OrderService(fakeTimeProvider);
        
        // Act
        var discount = sut.GetTimeBasedDiscount();
        
        // Assert
        discount.Should().Be(expected);
    }
}

#endregion

#region 時間凍結測試

/// <summary>
/// 時間凍結測試 - 驗證多個操作在同一時間點發生
/// </summary>
public class TimeFreezeTests
{
    [Fact]
    public void ProcessBatch_在固定時間點_應產生相同時間戳()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var fixedTime = new DateTime(2024, 12, 25, 10, 30, 0);
        fakeTimeProvider.SetLocalNow(fixedTime);
        
        var processor = new BatchProcessor(fakeTimeProvider);
        
        // Act - 連續執行多個操作
        var result1 = processor.ProcessItem("Item1");
        var result2 = processor.ProcessItem("Item2");
        var result3 = processor.ProcessItem("Item3");
        
        // Assert - 時間被凍結，所有時間戳應該相同
        result1.Timestamp.Should().Be(fixedTime);
        result2.Timestamp.Should().Be(fixedTime);
        result3.Timestamp.Should().Be(fixedTime);
    }
}

public class BatchProcessor
{
    private readonly TimeProvider _timeProvider;
    
    public BatchProcessor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    public ProcessResult ProcessItem(string item)
    {
        return new ProcessResult
        {
            Item = item,
            Timestamp = _timeProvider.GetLocalNow().DateTime
        };
    }
}

public class ProcessResult
{
    public string Item { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

#endregion

#region 時間快轉測試 (Advance)

/// <summary>
/// 時間快轉測試 - 使用 Advance() 方法
/// </summary>
public class TimeAdvanceTests
{
    [Fact]
    public void Cache_設定項目後快轉時間_應正確處理過期()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var startTime = new DateTime(2024, 3, 15, 10, 0, 0);
        fakeTimeProvider.SetLocalNow(startTime);
        
        var cache = new TimedCache<string>(fakeTimeProvider, TimeSpan.FromMinutes(5));
        
        // Act & Assert - 設定快取項目（時間點：10:00）
        cache.Set("key1", "value1");
        cache.Get("key1").Should().Be("value1");
        
        // 模擬時間前進 3 分鐘（時間點：10:03），快取尚未過期（5分鐘期限）
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(3));
        cache.Get("key1").Should().Be("value1"); // 3 < 5，仍在有效期內
        
        // 再次模擬時間前進 3 分鐘（時間點：10:06），快取已過期
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(3)); // 總計 6 分鐘 > 5 分鐘期限
        cache.Get("key1").Should().BeNull(); // 已過期，返回 null
    }
    
    [Fact]
    public void Cache_邊界測試_剛好過期的瞬間()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 10, 0, 0));
        
        var cache = new TimedCache<string>(fakeTimeProvider, TimeSpan.FromMinutes(5));
        cache.Set("key", "value");
        
        // Act & Assert - 4 分 59 秒後仍有效
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(4).Add(TimeSpan.FromSeconds(59)));
        cache.Get("key").Should().Be("value");
        
        // 再過 2 秒後過期
        fakeTimeProvider.Advance(TimeSpan.FromSeconds(2));
        cache.Get("key").Should().BeNull();
    }
    
    [Fact]
    public void Token_使用Advance測試有效期限()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 3, 15, 10, 0, 0));
        
        var tokenService = new TokenService(fakeTimeProvider);
        var token = tokenService.GenerateToken("user123", TimeSpan.FromHours(1));
        
        // Act & Assert - 立即驗證應該有效
        tokenService.ValidateToken(token).Should().BeTrue();
        
        // 30 分鐘後仍有效
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(30));
        tokenService.ValidateToken(token).Should().BeTrue();
        
        // 再過 31 分鐘後（共 61 分鐘）過期
        fakeTimeProvider.Advance(TimeSpan.FromMinutes(31));
        tokenService.ValidateToken(token).Should().BeFalse();
    }
}

/// <summary>
/// 泛型快取類別
/// </summary>
public class TimedCache<T>
{
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<string, CacheItem<T>> _cache = new();
    
    public TimeSpan DefaultExpiry { get; }
    
    public TimedCache(TimeProvider timeProvider, TimeSpan defaultExpiry)
    {
        _timeProvider = timeProvider;
        DefaultExpiry = defaultExpiry;
    }
    
    public void Set(string key, T value, TimeSpan? expiry = null)
    {
        var expiryTime = _timeProvider.GetUtcNow().Add(expiry ?? DefaultExpiry);
        _cache[key] = new CacheItem<T>(value, expiryTime);
    }
    
    public T? Get(string key)
    {
        if (!_cache.TryGetValue(key, out var item))
            return default;
            
        if (item.ExpiryTime <= _timeProvider.GetUtcNow())
        {
            _cache.Remove(key);
            return default;
        }
        
        return item.Value;
    }
}

public record CacheItem<T>(T Value, DateTimeOffset ExpiryTime);

/// <summary>
/// Token 服務範例
/// </summary>
public class TokenService
{
    private readonly TimeProvider _timeProvider;
    private readonly Dictionary<string, TokenInfo> _tokens = new();
    
    public TokenService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    public string GenerateToken(string userId, TimeSpan validity)
    {
        var token = Guid.NewGuid().ToString();
        var expiryTime = _timeProvider.GetUtcNow().Add(validity);
        
        _tokens[token] = new TokenInfo(userId, expiryTime);
        return token;
    }
    
    public bool ValidateToken(string token)
    {
        if (!_tokens.TryGetValue(token, out var info))
            return false;
            
        return info.ExpiryTime > _timeProvider.GetUtcNow();
    }
}

public record TokenInfo(string UserId, DateTimeOffset ExpiryTime);

#endregion

#region 時間倒轉測試

/// <summary>
/// 時間倒轉測試 - 歷史資料處理
/// </summary>
public class TimeRewindTests
{
    [Fact]
    public void HistoricalDataProcessor_回到過去時間_應正確處理歷史資料()
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        
        // 回到 2020 年的某一天
        var historicalTime = new DateTime(2020, 1, 15, 9, 0, 0);
        fakeTimeProvider.SetLocalNow(historicalTime);
        
        var processor = new HistoricalDataProcessor(fakeTimeProvider);
        
        // Act
        var result = processor.ProcessDataForDate(historicalTime.Date);
        
        // Assert
        result.Should().NotBeNull();
        result.ProcessedAt.Should().Be(historicalTime);
    }
}

public class HistoricalDataProcessor
{
    private readonly TimeProvider _timeProvider;
    
    public HistoricalDataProcessor(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    public HistoricalResult ProcessDataForDate(DateTime date)
    {
        return new HistoricalResult
        {
            Date = date,
            ProcessedAt = _timeProvider.GetLocalNow().DateTime
        };
    }
}

public class HistoricalResult
{
    public DateTime Date { get; set; }
    public DateTime ProcessedAt { get; set; }
}

#endregion

#region 排程與交易時間測試

/// <summary>
/// 排程服務測試
/// </summary>
public class ScheduleServiceTests
{
    [Theory]
    [InlineData("2024-03-15 14:30:00", "2024-03-15 14:00:00", true)]  // 已到執行時間
    [InlineData("2024-03-15 13:30:00", "2024-03-15 14:00:00", false)] // 尚未到執行時間
    [InlineData("2024-03-15 14:00:00", "2024-03-15 14:00:00", true)]  // 剛好到執行時間（邊界）
    public void ShouldExecuteJob_根據時間判斷_應回傳正確結果(
        string currentTimeStr, 
        string scheduledTimeStr, 
        bool expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var currentTime = DateTime.Parse(currentTimeStr);
        var scheduledTime = DateTime.Parse(scheduledTimeStr);
        
        fakeTimeProvider.SetLocalNow(currentTime);
        
        var schedule = new JobSchedule { NextExecutionTime = scheduledTime };
        var sut = new ScheduleService(fakeTimeProvider);
        
        // Act
        var result = sut.ShouldExecuteJob(schedule);
        
        // Assert
        result.Should().Be(expected);
    }
}

/// <summary>
/// 交易服務測試
/// </summary>
public class TradingServiceTests
{
    [Theory]
    [InlineData("09:30:00", true)]   // 上午交易時間
    [InlineData("11:15:00", true)]   // 上午交易時間結束前
    [InlineData("12:00:00", false)]  // 中午休息時間
    [InlineData("14:30:00", true)]   // 下午交易時間
    [InlineData("15:30:00", false)]  // 下午交易結束後
    [InlineData("09:00:00", true)]   // 上午交易開始（邊界）
    [InlineData("15:00:00", true)]   // 下午交易結束（邊界）
    public void IsInTradingHours_不同時間點_應回傳正確結果(string timeStr, bool expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var testTime = DateTime.Today.Add(TimeSpan.Parse(timeStr));
        fakeTimeProvider.SetLocalNow(testTime);
        
        var sut = new TradingService(fakeTimeProvider);
        
        // Act
        var result = sut.IsInTradingHours();
        
        // Assert
        result.Should().Be(expected);
    }
    
    [Theory]
    [InlineData(DayOfWeek.Saturday, 0)]     // 週六不交易
    [InlineData(DayOfWeek.Sunday, 0)]       // 週日不交易
    [InlineData(DayOfWeek.Monday, 1.0)]     // 週一正常
    [InlineData(DayOfWeek.Friday, 1.1)]     // 週五下午波動大（hour >= 14）
    public void GetMarketMultiplier_不同星期_應回傳正確乘數(DayOfWeek dayOfWeek, decimal expected)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        
        // 找到對應星期的日期，設定為下午 3 點
        var date = GetNextWeekday(new DateTime(2024, 3, 1), dayOfWeek);
        fakeTimeProvider.SetLocalNow(date.AddHours(15)); // 下午 3 點
        
        var sut = new TradingService(fakeTimeProvider);
        
        // Act
        var result = sut.GetMarketMultiplier();
        
        // Assert
        result.Should().Be(expected);
    }
    
    private static DateTime GetNextWeekday(DateTime start, DayOfWeek day)
    {
        int daysToAdd = ((int)day - (int)start.DayOfWeek + 7) % 7;
        return start.AddDays(daysToAdd == 0 && start.DayOfWeek != day ? 7 : daysToAdd);
    }
}

#endregion

#region 時區測試

/// <summary>
/// 全球時間服務測試 - 時區處理
/// </summary>
public class GlobalTimeServiceTests
{
    [Theory]
    [InlineData("UTC", "2024-03-15 10:00:00")]
    [InlineData("Tokyo Standard Time", "2024-03-15 19:00:00")]
    [InlineData("Eastern Standard Time", "2024-03-15 06:00:00")]
    public void GetTimeInTimeZone_不同時區_應回傳正確時間(string timeZoneId, string expectedTimeStr)
    {
        // Arrange
        var fakeTimeProvider = new FakeTimeProvider();
        var baseUtcTime = new DateTime(2024, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        fakeTimeProvider.SetUtcNow(baseUtcTime);
        
        var sut = new GlobalTimeService(fakeTimeProvider);
        var expectedTime = DateTime.Parse(expectedTimeStr);
        
        // Act
        var result = sut.GetTimeInTimeZone(timeZoneId);
        
        // Assert
        result.DateTime.Should().BeCloseTo(expectedTime, TimeSpan.FromSeconds(1));
    }
}

#endregion

#region 測試隔離策略

/// <summary>
/// 展示正確的測試隔離策略
/// </summary>
public class TimeServiceTestsWithIsolation : IDisposable
{
    private readonly FakeTimeProvider _fakeTimeProvider;
    private readonly GlobalTimeService _sut;
    
    public TimeServiceTestsWithIsolation()
    {
        // 每個測試實例都有獨立的 FakeTimeProvider
        _fakeTimeProvider = new FakeTimeProvider();
        _sut = new GlobalTimeService(_fakeTimeProvider);
    }
    
    public void Dispose()
    {
        // FakeTimeProvider 實作了 IDisposable
        _fakeTimeProvider?.Dispose();
    }
    
    [Fact]
    public void Test1_設定為2024年1月1日()
    {
        _fakeTimeProvider.SetLocalNow(new DateTime(2024, 1, 1, 12, 0, 0));
        
        var result = _sut.GetCurrentTimeString();
        
        result.Should().Contain("2024-01-01");
    }
    
    [Fact]
    public void Test2_設定為2024年12月31日()
    {
        _fakeTimeProvider.SetLocalNow(new DateTime(2024, 12, 31, 12, 0, 0));
        
        var result = _sut.GetCurrentTimeString();
        
        result.Should().Contain("2024-12-31");
    }
}

#endregion
