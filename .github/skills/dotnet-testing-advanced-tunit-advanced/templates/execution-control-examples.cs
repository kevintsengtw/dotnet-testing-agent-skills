// TUnit 執行控制範例 - Retry、Timeout、DisplayName

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using System.Diagnostics;

namespace TUnit.Advanced.ExecutionControl.Examples;

#region Domain Models

public enum CustomerLevel
{
    一般會員 = 0,
    VIP會員 = 1,
    白金會員 = 2,
    鑽石會員 = 3
}

#endregion

#region Retry Mechanism

/// <summary>
/// Retry 機制範例
/// 用於處理可能因外部因素而偶爾失敗的測試
/// </summary>
public class RetryMechanismExamples
{
    /// <summary>
    /// 基本 Retry 使用
    /// 如果失敗，重試最多 3 次
    /// </summary>
    [Test]
    [Retry(3)]
    [Property("Category", "Flaky")]
    public async Task NetworkCall_可能不穩定_使用重試機制()
    {
        // 模擬可能失敗的網路呼叫
        var random = new Random();
        var success = random.Next(1, 4) == 1; // 約 33% 的成功率

        if (!success)
        {
            throw new HttpRequestException("模擬網路錯誤");
        }

        await Assert.That(success).IsTrue();
    }

    /// <summary>
    /// 外部 API 呼叫的 Retry 最佳實踐
    /// </summary>
    [Test]
    [Retry(3)]
    [Property("Category", "ExternalDependency")]
    public async Task CallExternalApi_網路問題時重試_最終應成功()
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(10);

        try
        {
            // 實際的外部 API 呼叫
            var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
            
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
            
            var content = await response.Content.ReadAsStringAsync();
            await Assert.That(content).IsNotNull();
        }
        catch (TaskCanceledException)
        {
            // 超時也算是暫時性錯誤，可以重試
            throw new HttpRequestException("請求超時，將重試");
        }
    }

    /// <summary>
    /// 不應該使用 Retry 的情況：預期會失敗的測試
    /// </summary>
    [Test]
    // 不要對預期會失敗的測試使用 Retry
    public async Task Divide_被零除_應拋出例外()
    {
        await Assert.That(() => { var _ = 10 / int.Parse("0"); }).Throws<DivideByZeroException>();
    }
}

/// <summary>
/// Retry 使用場景指南
/// </summary>
public class RetryUsageGuide
{
    /*
     * ✅ 適合使用 Retry 的情況：
     * 
     * 1. 外部服務呼叫
     *    - API 請求可能因網路問題暫時失敗
     *    - 資料庫連線可能暫時中斷
     * 
     * 2. 檔案系統操作
     *    - 在 CI/CD 環境中，檔案鎖定可能導致暫時性失敗
     * 
     * 3. 並行測試競爭
     *    - 多個測試同時存取共享資源時的競爭條件
     * 
     * ❌ 不適合使用 Retry 的情況：
     * 
     * 1. 邏輯錯誤
     *    - 程式碼本身的錯誤重試多少次都不會成功
     * 
     * 2. 預期的例外
     *    - 測試本身就是要驗證例外情況
     * 
     * 3. 效能測試
     *    - 重試會影響效能測量的準確性
     */

    [Test]
    public async Task RetryGuidelines_最佳實踐文件()
    {
        await Assert.That(true).IsTrue();
    }
}

#endregion

#region Timeout Control

/// <summary>
/// Timeout 控制範例
/// 確保測試在合理時間內完成
/// </summary>
public class TimeoutControlExamples
{
    /// <summary>
    /// 基本 Timeout 使用
    /// 5 秒超時保護
    /// </summary>
    [Test]
    [Timeout(5000)]
    [Property("Category", "Performance")]
    public async Task LongRunningOperation_應在時限內完成()
    {
        // 模擬可能會很慢的操作
        await Task.Delay(1000); // 1 秒操作，應該在 5 秒限制內

        await Assert.That(true).IsTrue();
    }

    /// <summary>
    /// 較長的 Timeout 設定
    /// 適合較複雜的操作
    /// </summary>
    [Test]
    [Timeout(30000)] // 30 秒超時
    [Property("Category", "Integration")]
    public async Task DatabaseMigration_大量資料處理_應在合理時間內完成()
    {
        // 模擬資料庫遷移或大量資料處理
        var tasks = new List<Task>();
        
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(ProcessDataBatch(i));
        }
        
        await Task.WhenAll(tasks);
        await Assert.That(tasks.All(t => t.IsCompletedSuccessfully)).IsTrue();
    }

    private static async Task ProcessDataBatch(int batchNumber)
    {
        // 模擬批次處理
        await Task.Delay(50); // 每批次 50ms
    }

    /// <summary>
    /// 效能基準測試
    /// 結合 Timeout 和效能測量
    /// </summary>
    [Test]
    [Timeout(1000)] // 確保不會超過 1 秒
    [Property("Category", "Performance")]
    [Property("Baseline", "true")]
    public async Task SearchFunction_效能基準_應符合SLA要求()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // 模擬搜尋功能
        var searchResults = await PerformSearch("test query");
        
        stopwatch.Stop();
        
        // 功能性驗證
        await Assert.That(searchResults).IsNotNull();
        await Assert.That(searchResults.Count()).IsGreaterThan(0);
        
        // 效能驗證：99% 的查詢應在 500ms 內完成
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(500);
    }

    private static async Task<IEnumerable<string>> PerformSearch(string query)
    {
        // 模擬搜尋邏輯
        await Task.Delay(100);
        return new[] { "result1", "result2", "result3" };
    }
}

#endregion

#region DisplayName

/// <summary>
/// DisplayName 自訂測試名稱範例
/// 提升測試報告的可讀性
/// </summary>
public class DisplayNameExamples
{
    /// <summary>
    /// 基本 DisplayName 使用
    /// </summary>
    [Test]
    [DisplayName("自訂測試名稱：驗證使用者註冊流程")]
    public async Task UserRegistration_CustomDisplayName_測試名稱更易讀()
    {
        // 使用自訂顯示名稱讓測試報告更容易理解
        await Assert.That("user@example.com").Contains("@");
    }

    /// <summary>
    /// 參數化測試的動態顯示名稱
    /// DisplayName 會自動替換參數值
    /// </summary>
    [Test]
    [Arguments("valid@email.com", true)]
    [Arguments("invalid-email", false)]
    [Arguments("", false)]
    [Arguments("test@domain.co.uk", true)]
    [Arguments("user.name+tag@example.com", true)]
    [DisplayName("電子郵件驗證：{0} 應為 {1}")]
    public async Task EmailValidation_參數化顯示名稱(string email, bool expectedValid)
    {
        // 顯示名稱會自動替換參數
        // 產生的名稱如：「電子郵件驗證：valid@email.com 應為 True」
        var isValid = !string.IsNullOrEmpty(email) && email.Contains("@") && email.Contains(".");
        
        await Assert.That(isValid).IsEqualTo(expectedValid);
    }

    /// <summary>
    /// 業務場景驅動的顯示名稱
    /// 使用業務語言而非技術術語
    /// </summary>
    [Test]
    [Arguments(CustomerLevel.一般會員, 1000, 0)]
    [Arguments(CustomerLevel.VIP會員, 1000, 50)]
    [Arguments(CustomerLevel.白金會員, 1000, 100)]
    [Arguments(CustomerLevel.鑽石會員, 1000, 200)]
    [DisplayName("會員等級 {0} 購買 ${1} 應獲得 ${2} 折扣")]
    public async Task MemberDiscount_根據會員等級_計算正確折扣(
        CustomerLevel level, decimal amount, decimal expectedDiscount)
    {
        // 這樣的測試報告讀起來像業務需求
        var discount = CalculateDiscount(amount, level);
        
        await Assert.That(discount).IsEqualTo(expectedDiscount);
    }

    private static decimal CalculateDiscount(decimal amount, CustomerLevel level)
    {
        return level switch
        {
            CustomerLevel.鑽石會員 => amount * 0.2m,
            CustomerLevel.白金會員 => amount * 0.1m,
            CustomerLevel.VIP會員 => amount * 0.05m,
            _ => 0m
        };
    }

    /// <summary>
    /// 訂單狀態轉換的業務語言顯示名稱
    /// </summary>
    [Test]
    [Arguments("Created", "Paid", true)]
    [Arguments("Paid", "Shipped", true)]
    [Arguments("Shipped", "Delivered", true)]
    [Arguments("Cancelled", "Shipped", false)]
    [DisplayName("訂單狀態從 {0} 轉換到 {1} 應為 {2}")]
    public async Task OrderStatusTransition_狀態轉換驗證(
        string fromStatus, string toStatus, bool expectedValid)
    {
        var isValid = IsValidTransition(fromStatus, toStatus);
        await Assert.That(isValid).IsEqualTo(expectedValid);
    }

    private static bool IsValidTransition(string from, string to)
    {
        var validTransitions = new Dictionary<string, string[]>
        {
            ["Created"] = new[] { "Paid", "Cancelled" },
            ["Paid"] = new[] { "Shipped", "Refunded" },
            ["Shipped"] = new[] { "Delivered", "Returned" },
            ["Delivered"] = new[] { "Completed", "Returned" }
        };

        return validTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
    }
}

#endregion

#region Combined Examples

/// <summary>
/// 組合使用範例
/// 同時使用 Retry、Timeout、DisplayName
/// </summary>
public class CombinedExecutionControlExamples
{
    /// <summary>
    /// 完整的執行控制組合
    /// </summary>
    [Test]
    [Retry(2)]
    [Timeout(5000)]
    [Property("Category", "Integration")]
    [DisplayName("外部 API 整合測試：健康檢查")]
    public async Task ExternalApiHealthCheck_完整執行控制()
    {
        using var httpClient = new HttpClient();
        httpClient.Timeout = TimeSpan.FromSeconds(3);

        try
        {
            var response = await httpClient.GetAsync("https://jsonplaceholder.typicode.com/posts/1");
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // 重試時會拋出這個例外
            throw new HttpRequestException($"API 健康檢查失敗：{ex.Message}");
        }
    }

    /// <summary>
    /// 效能測試的執行控制組合
    /// </summary>
    [Test]
    [Timeout(10000)]
    [Property("Category", "Performance")]
    [Property("Priority", "High")]
    [DisplayName("效能基準：批次處理應在 10 秒內完成")]
    public async Task PerformanceBenchmark_批次處理效能()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // 模擬批次處理
        var tasks = Enumerable.Range(0, 50)
            .Select(async i =>
            {
                await Task.Delay(Random.Shared.Next(10, 50));
                return i;
            });

        var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        await Assert.That(results.Length).IsEqualTo(50);
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5000);
        
        Console.WriteLine($"批次處理完成，耗時: {stopwatch.ElapsedMilliseconds}ms");
    }
}

#endregion
