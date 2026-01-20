// =============================================================================
// TimeProvider 基礎用法範例
// 展示如何將時間相依程式碼重構為可測試的設計
// =============================================================================

using System;

namespace TimeProviderExamples;

#region 問題：傳統 DateTime 無法測試

/// <summary>
/// ❌ 問題程式碼：直接使用 DateTime.Now，無法在測試中控制時間
/// </summary>
public class LegacyOrderService
{
    public bool CanPlaceOrder()
    {
        // 直接使用靜態時間 - 測試結果取決於執行時間
        var now = DateTime.Now;
        var currentHour = now.Hour;
        
        // 營業時間：上午9點到下午5點
        return currentHour >= 9 && currentHour < 17;
    }
    
    public string GetTimeBasedDiscount()
    {
        var today = DateTime.Today;
        
        if (today.DayOfWeek == DayOfWeek.Friday)
        {
            return "週五快樂：九折優惠";
        }
            
        if (today.Month == 12 && today.Day == 25)
        {
            return "聖誕特惠：八折優惠";
        }
            
        return "無優惠";
    }
}

#endregion

#region 解決方案：使用 TimeProvider 抽象化

/// <summary>
/// ✅ 可測試的程式碼：透過依賴注入接收 TimeProvider
/// </summary>
public class OrderService
{
    private readonly TimeProvider _timeProvider;
    
    /// <summary>
    /// 建構式注入 TimeProvider
    /// 生產環境傳入 TimeProvider.System
    /// 測試環境傳入 FakeTimeProvider
    /// </summary>
    public OrderService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }
    
    /// <summary>
    /// 判斷是否可以下單（營業時間內）
    /// </summary>
    public bool CanPlaceOrder()
    {
        // 使用注入的 TimeProvider 取得時間
        var now = _timeProvider.GetLocalNow();
        var currentHour = now.Hour;
        
        // 營業時間：上午9點到下午5點
        return currentHour >= 9 && currentHour < 17;
    }
    
    /// <summary>
    /// 根據日期取得優惠資訊
    /// </summary>
    public string GetTimeBasedDiscount()
    {
        var today = _timeProvider.GetLocalNow().Date;
        
        if (today.DayOfWeek == DayOfWeek.Friday)
        {
            return "週五快樂：九折優惠";
        }
            
        if (today.Month == 12 && today.Day == 25)
        {
            return "聖誕特惠：八折優惠";
        }
            
        return "無優惠";
    }
}

#endregion

#region TimeProvider 核心 API 說明

/// <summary>
/// TimeProvider 核心 API 參考
/// </summary>
public static class TimeProviderApiReference
{
    public static void ShowTimeProviderUsage()
    {
        // 1. 系統時間提供者（生產環境使用）
        TimeProvider systemProvider = TimeProvider.System;
        
        // 2. 取得 UTC 時間
        DateTimeOffset utcNow = systemProvider.GetUtcNow();
        Console.WriteLine($"UTC 時間: {utcNow}");
        
        // 3. 取得本地時間
        DateTimeOffset localNow = systemProvider.GetLocalNow();
        Console.WriteLine($"本地時間: {localNow}");
        
        // 4. 取得本地時區資訊
        TimeZoneInfo localTimeZone = systemProvider.LocalTimeZone;
        Console.WriteLine($"本地時區: {localTimeZone.DisplayName}");
        
        // 5. 高精度時間戳（用於效能測量）
        long timestamp = systemProvider.GetTimestamp();
        Console.WriteLine($"時間戳: {timestamp}");
        
        // 6. 計算時間差
        long startTimestamp = systemProvider.GetTimestamp();
        // ... 執行某些操作 ...
        long endTimestamp = systemProvider.GetTimestamp();
        TimeSpan elapsed = systemProvider.GetElapsedTime(startTimestamp, endTimestamp);
        Console.WriteLine($"經過時間: {elapsed}");
    }
}

#endregion

#region 依賴注入設定

/// <summary>
/// 依賴注入設定範例
/// </summary>
public static class DependencyInjectionSetup
{
    /*
    // Program.cs 或 Startup.cs
    
    // 生產環境 - 使用系統時間
    services.AddSingleton(TimeProvider.System);
    services.AddScoped<OrderService>();
    
    // 開發環境（如果需要特定時間測試）
    if (builder.Environment.IsDevelopment())
    {
        var fakeTimeProvider = new FakeTimeProvider();
        fakeTimeProvider.SetLocalNow(new DateTime(2024, 12, 25, 10, 0, 0)); // 測試聖誕節
        services.AddSingleton<TimeProvider>(fakeTimeProvider);
    }
    */
}

#endregion

#region 實際業務邏輯範例

/// <summary>
/// 排程服務 - 展示時間相依的排程邏輯
/// </summary>
public class ScheduleService
{
    private readonly TimeProvider _timeProvider;
    
    public ScheduleService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    /// <summary>
    /// 判斷工作是否應該執行
    /// </summary>
    public bool ShouldExecuteJob(JobSchedule schedule)
    {
        var now = _timeProvider.GetLocalNow();
        return schedule.NextExecutionTime <= now;
    }
    
    /// <summary>
    /// 計算下次執行時間
    /// </summary>
    public DateTime CalculateNextExecution(JobSchedule schedule)
    {
        var now = _timeProvider.GetLocalNow();
        
        return schedule.CronExpression switch
        {
            "0 0 * * *" => now.Date.AddDays(1),           // 每日午夜
            "0 0 * * 1" => GetNextMonday(now),             // 每週一午夜
            _ => now.DateTime.AddHours(1)                  // 預設每小時
        };
    }
    
    private DateTime GetNextMonday(DateTimeOffset now)
    {
        var daysUntilMonday = ((int)DayOfWeek.Monday - (int)now.DayOfWeek + 7) % 7;
        return now.Date.AddDays(daysUntilMonday == 0 ? 7 : daysUntilMonday);
    }
}

public class JobSchedule
{
    public DateTime NextExecutionTime { get; set; }
    public string CronExpression { get; set; } = string.Empty;
}

/// <summary>
/// 交易服務 - 展示時間窗口邏輯
/// </summary>
public class TradingService
{
    private readonly TimeProvider _timeProvider;
    
    public TradingService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    /// <summary>
    /// 判斷是否在交易時間內
    /// 交易時間：9:00-11:30, 13:00-15:00
    /// </summary>
    public bool IsInTradingHours()
    {
        var now = _timeProvider.GetLocalNow();
        var currentTime = now.TimeOfDay;
        
        return (currentTime >= TimeSpan.FromHours(9) && currentTime <= TimeSpan.FromHours(11.5)) ||
               (currentTime >= TimeSpan.FromHours(13) && currentTime <= TimeSpan.FromHours(15));
    }
    
    /// <summary>
    /// 取得市場乘數
    /// </summary>
    public decimal GetMarketMultiplier()
    {
        var now = _timeProvider.GetLocalNow();
        
        return now.DayOfWeek switch
        {
            DayOfWeek.Saturday or DayOfWeek.Sunday => 0m,      // 週末不交易
            DayOfWeek.Friday when now.Hour >= 14 => 1.1m,       // 週五下午波動較大
            _ => 1.0m
        };
    }
}

/// <summary>
/// 全球時間服務 - 展示時區處理
/// </summary>
public class GlobalTimeService
{
    private readonly TimeProvider _timeProvider;
    
    public GlobalTimeService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    /// <summary>
    /// 取得指定時區的當前時間
    /// </summary>
    public DateTimeOffset GetTimeInTimeZone(string timeZoneId)
    {
        var utcNow = _timeProvider.GetUtcNow();
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        
        return TimeZoneInfo.ConvertTime(utcNow, targetTimeZone);
    }
    
    /// <summary>
    /// 取得當前時間字串
    /// </summary>
    public string GetCurrentTimeString()
    {
        return _timeProvider.GetLocalNow().ToString("yyyy-MM-dd HH:mm:ss");
    }
    
    /// <summary>
    /// 取得當前時間
    /// </summary>
    public DateTime GetCurrentTime()
    {
        return _timeProvider.GetLocalNow().DateTime;
    }
}

/// <summary>
/// 審計日誌服務 - 展示 UTC 與本地時間轉換
/// </summary>
public class AuditLogger
{
    private readonly TimeProvider _timeProvider;
    
    public AuditLogger(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }
    
    /// <summary>
    /// 記錄活動（使用 UTC 時間儲存，顯示本地時間）
    /// </summary>
    public AuditLog LogActivity(string activity)
    {
        var utcTimestamp = _timeProvider.GetUtcNow();
        var localTime = _timeProvider.GetLocalNow();
        
        return new AuditLog
        {
            Activity = activity,
            UtcTimestamp = utcTimestamp,
            LocalTimeDisplay = localTime.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }
}

public class AuditLog
{
    public string Activity { get; set; } = string.Empty;
    public DateTimeOffset UtcTimestamp { get; set; }
    public string LocalTimeDisplay { get; set; } = string.Empty;
}

#endregion
