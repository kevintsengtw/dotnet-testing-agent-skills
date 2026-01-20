using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

/// <summary>
/// ITestOutputHelper 使用範例
/// 展示如何在 xUnit 測試中正確使用 ITestOutputHelper 進行診斷輸出
/// </summary>
public class ITestOutputHelperExample
{
    private readonly ITestOutputHelper _output;

    // 正確的注入方式：透過建構式注入
    public ITestOutputHelperExample(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    [Fact]
    public void BasicOutputExample_展示基本輸出()
    {
        // Arrange
        var productName = "筆記型電腦";
        var price = 30000;

        // Act
        _output.WriteLine("=== 測試開始 ===");
        _output.WriteLine($"商品名稱: {productName}");
        _output.WriteLine($"價格: NT${price:N0}");

        var discountedPrice = price * 0.9m;
        _output.WriteLine($"折扣後價格: NT${discountedPrice:N0}");

        // Assert
        _output.WriteLine("=== 測試完成 ===");
        Assert.True(discountedPrice < price);
    }

    [Fact]
    public void StructuredOutputExample_展示結構化輸出()
    {
        // Arrange
        LogSection("測試設置");
        var customer = new { Name = "王小明", Level = "VIP" };
        LogKeyValue("客戶姓名", customer.Name);
        LogKeyValue("會員等級", customer.Level);

        // Act
        LogSection("執行測試");
        var discount = CalculateDiscount(customer.Level);
        LogKeyValue("計算折扣", $"{discount}%");

        // Assert
        LogSection("驗證結果");
        Assert.Equal(10, discount);
        _output.WriteLine("✅ 折扣計算正確");
    }

    [Fact]
    public async Task PerformanceTestExample_展示效能測試輸出()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();
        _output.WriteLine("=== 效能測試開始 ===");
        _output.WriteLine($"開始時間: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");

        // Act - Stage 1
        await Task.Delay(100); // 模擬資料載入
        var loadTime = stopwatch.Elapsed;
        _output.WriteLine($"資料載入完成: {loadTime.TotalMilliseconds:F2} ms");

        // Act - Stage 2
        await Task.Delay(50); // 模擬資料處理
        var processTime = stopwatch.Elapsed;
        _output.WriteLine($"資料處理完成: {processTime.TotalMilliseconds:F2} ms");

        stopwatch.Stop();

        // Assert & Report
        _output.WriteLine("\n=== 效能報告 ===");
        _output.WriteLine($"總執行時間: {stopwatch.Elapsed.TotalMilliseconds:F2} ms");
        _output.WriteLine($"是否符合效能要求（< 200ms）: {stopwatch.Elapsed.TotalMilliseconds < 200}");

        Assert.True(stopwatch.Elapsed.TotalMilliseconds < 200);
    }

    // 輔助方法：結構化輸出
    private void LogSection(string title)
    {
        _output.WriteLine($"\n=== {title} ===");
    }

    private void LogKeyValue(string key, object value)
    {
        _output.WriteLine($"{key}: {value}");
    }

    private int CalculateDiscount(string customerLevel)
    {
        return customerLevel == "VIP" ? 10 : 0;
    }
}

// ❌ 錯誤示範：不要這樣做
public class WrongITestOutputHelperUsage
{
    // 錯誤 1：使用靜態欄位
    private static ITestOutputHelper _staticOutput; // ❌ 不可行

    public WrongITestOutputHelperUsage(ITestOutputHelper output)
    {
        _staticOutput = output;
    }

    // 錯誤 2：嘗試在靜態方法中使用
    public static void StaticHelper()
    {
        // _staticOutput.WriteLine("這會失敗"); // ❌ 不可行
    }

    // 錯誤 3：嘗試在 Dispose 中使用
    public void Dispose()
    {
        // _staticOutput?.WriteLine("測試清理"); // ❌ 可能失敗
    }
}
