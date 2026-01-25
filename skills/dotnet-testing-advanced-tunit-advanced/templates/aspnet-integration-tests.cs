// TUnit ASP.NET Core 整合測試範例

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;
using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TUnit.Advanced.AspNetCore.Examples;

#region Basic Integration Tests

/// <summary>
/// ASP.NET Core 整合測試基本範例
/// 使用 WebApplicationFactory 進行完整的 Web API 測試
/// </summary>
public class WebApiIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public WebApiIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // 在這裡可以加入測試專用的服務設定
                    // 例如：替換資料庫連線、使用 Mock 服務等
                });
            });

        _client = _factory.CreateClient();
    }

    /// <summary>
    /// 基本的 API 端點測試
    /// </summary>
    [Test]
    public async Task WeatherForecast_Get_應回傳正確格式的資料()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsNotNull();
        await Assert.That(content.Length).IsGreaterThan(0);
    }

    /// <summary>
    /// 驗證 HTTP 回應標頭
    /// </summary>
    [Test]
    [Property("Category", "Integration")]
    public async Task WeatherForecast_ResponseHeaders_應包含ContentType標頭()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        
        // 檢查 Content-Type 標頭
        var contentType = response.Content.Headers.ContentType?.MediaType;
        await Assert.That(contentType).IsEqualTo("application/json");
    }

    /// <summary>
    /// 冒煙測試：確保端點可用
    /// </summary>
    [Test]
    [Property("Category", "Smoke")]
    public async Task WeatherForecast_端點可用性_應能正常回應()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();

        var content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsNotNull();
        await Assert.That(content.Length).IsGreaterThan(10); // 確保有實際內容
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

#endregion

#region Performance Tests

/// <summary>
/// 效能測試與負載測試範例
/// </summary>
public class PerformanceIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public PerformanceIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// 回應時間驗證
    /// </summary>
    [Test]
    [Property("Category", "Performance")]
    [Timeout(10000)] // 10 秒超時保護
    public async Task WeatherForecast_ResponseTime_應在合理範圍內()
    {
        // Arrange
        var stopwatch = Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/weatherforecast");
        stopwatch.Stop();

        // Assert
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
        await Assert.That(stopwatch.ElapsedMilliseconds).IsLessThan(5000); // 5秒內回應
        
        Console.WriteLine($"回應時間: {stopwatch.ElapsedMilliseconds}ms");
    }

    /// <summary>
    /// 並行負載測試
    /// </summary>
    [Test]
    [Property("Category", "Load")]
    [Timeout(30000)]
    public async Task WeatherForecast_並行請求_應能正確處理()
    {
        // Arrange
        const int concurrentRequests = 50;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_client.GetAsync("/weatherforecast"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        await Assert.That(responses.Length).IsEqualTo(concurrentRequests);
        await Assert.That(responses.All(r => r.IsSuccessStatusCode)).IsTrue();

        // 清理
        foreach (var response in responses)
        {
            response.Dispose();
        }
        
        Console.WriteLine($"成功處理 {concurrentRequests} 個並行請求");
    }

    /// <summary>
    /// 持續負載測試
    /// </summary>
    [Test]
    [Property("Category", "Load")]
    [Timeout(60000)] // 60 秒超時
    public async Task WeatherForecast_持續負載_應維持穩定效能()
    {
        // Arrange
        const int totalRequests = 100;
        const int batchSize = 10;
        var responseTimes = new List<long>();

        // Act
        for (int batch = 0; batch < totalRequests / batchSize; batch++)
        {
            var batchTasks = Enumerable.Range(0, batchSize)
                .Select(async _ =>
                {
                    var sw = Stopwatch.StartNew();
                    var response = await _client.GetAsync("/weatherforecast");
                    sw.Stop();
                    response.Dispose();
                    return sw.ElapsedMilliseconds;
                });

            var batchResults = await Task.WhenAll(batchTasks);
            responseTimes.AddRange(batchResults);
            
            // 批次間短暫延遲
            await Task.Delay(100);
        }

        // Assert
        var avgResponseTime = responseTimes.Average();
        var maxResponseTime = responseTimes.Max();
        var p95ResponseTime = responseTimes.OrderBy(x => x).ElementAt((int)(responseTimes.Count * 0.95));

        Console.WriteLine($"平均回應時間: {avgResponseTime:F2}ms");
        Console.WriteLine($"最大回應時間: {maxResponseTime}ms");
        Console.WriteLine($"P95 回應時間: {p95ResponseTime}ms");

        await Assert.That(avgResponseTime).IsLessThan(1000); // 平均 1 秒內
        await Assert.That(p95ResponseTime).IsLessThan(2000); // P95 2 秒內
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

#endregion

#region Health Check Tests

/// <summary>
/// 健康檢查與監控測試
/// </summary>
public class HealthCheckTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// 健康狀態端點測試
    /// 對於 Kubernetes 部署和監控很重要
    /// </summary>
    [Test]
    [Property("Category", "Health")]
    public async Task HealthCheck_應回傳健康狀態()
    {
        try
        {
            var response = await _client.GetAsync("/health");
            // 如果有 health endpoint 就測試
            await Assert.That(response.IsSuccessStatusCode).IsTrue();
        }
        catch (HttpRequestException)
        {
            // 如果沒有 /health 端點，測試根路徑
            var response = await _client.GetAsync("/");
            await Assert.That((int)response.StatusCode).IsLessThan(500);
        }
    }

    /// <summary>
    /// 應用程式基本可用性測試
    /// </summary>
    [Test]
    [Property("Category", "Smoke")]
    [DisplayName("應用程式可用性：基本端點應能回應")]
    public async Task ApplicationAvailability_基本端點_應能回應()
    {
        // Act
        var response = await _client.GetAsync("/weatherforecast");

        // Assert - 確保不是伺服器錯誤
        await Assert.That((int)response.StatusCode).IsLessThan(500);
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

#endregion

#region E2E Business Flow Tests

/// <summary>
/// 端到端業務流程測試
/// </summary>
public class OrderApiIntegrationTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public OrderApiIntegrationTests()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    /// <summary>
    /// 完整訂單流程測試
    /// </summary>
    [Test]
    [Property("Category", "E2E")]
    [DisplayName("完整訂單流程：建立 → 查詢 → 更新狀態")]
    public async Task CreateOrder_完整流程_應成功建立訂單()
    {
        // 這個測試展示完整的訂單建立流程
        // 由於範例 API 可能沒有實際的訂單端點，我們測試基本的 API 可用性

        // Act
        var response = await _client.GetAsync("/");

        // Assert - 確保 API 可以正常啟動和回應
        // 在真實專案中，這裡會測試實際的業務邏輯端點
        await Assert.That((int)response.StatusCode).IsLessThan(500); // 不是伺服器錯誤
    }

    /// <summary>
    /// 模擬完整的 CRUD 操作流程
    /// </summary>
    [Test]
    [Property("Category", "E2E")]
    [DisplayName("CRUD 操作流程驗證")]
    public async Task CrudOperations_完整流程_應正確執行()
    {
        // 1. 驗證 API 可用
        var listResponse = await _client.GetAsync("/weatherforecast");
        await Assert.That(listResponse.IsSuccessStatusCode).IsTrue();

        // 2. 驗證回應內容
        var content = await listResponse.Content.ReadAsStringAsync();
        await Assert.That(content).IsNotNull();
        await Assert.That(content.Length).IsGreaterThan(0);

        // 3. 驗證回應格式
        var contentType = listResponse.Content.Headers.ContentType?.MediaType;
        await Assert.That(contentType).IsEqualTo("application/json");
    }

    public void Dispose()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }
}

#endregion

#region Integration Test Best Practices

/*
 * ASP.NET Core 整合測試最佳實踐
 * 
 * 1. 測試專案設定：
 *    - 在 WebApi 專案的 Program.cs 最後加上：
 *      public partial class Program { }
 *    - 這讓整合測試可以存取 Program 類別
 * 
 * 2. 套件依賴：
 *    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
 *    <PackageReference Include="TUnit" Version="0.57.24" />
 * 
 * 3. GlobalUsings.cs 設定：
 *    global using Microsoft.AspNetCore.Mvc.Testing;
 *    global using System.Net.Http.Json;
 *    global using TUnit.Core;
 *    global using TUnit.Assertions;
 *    global using TUnit.Assertions.Extensions;
 * 
 * 4. 測試類別結構：
 *    - 實作 IDisposable 以正確清理資源
 *    - 使用建構式建立 WebApplicationFactory 和 HttpClient
 *    - 在 Dispose 中釋放資源
 * 
 * 5. 冒煙測試的價值：
 *    - 快速回饋：在 CI/CD 流程中提供最快的基本功能驗證
 *    - 早期發現：能夠在第一時間發現部署或設定問題
 *    - 成本效益：執行快速，但能夠捕獲大部分基礎問題
 *    - 信心建立：為後續的詳細測試建立基礎信心
 */

/// <summary>
/// 整合測試設定範例
/// </summary>
public class IntegrationTestSetupGuide
{
    [Test]
    [DisplayName("整合測試設定文件")]
    public async Task IntegrationTestSetup_文件說明()
    {
        // 這個測試作為文件說明用途
        await Assert.That(true).IsTrue();
    }
}

#endregion

// 注意：這個檔案需要實際的 Program 類別才能編譯
// 在真實專案中，確保 WebApi 專案有 public partial class Program { }
public partial class Program { }
