using Flurl.Http;
using Microsoft.Extensions.Time.Testing;

namespace YourProject.Tests.Integration.Fixtures;

/// <summary>
/// 整合測試集合定義 - 所有測試共享同一個 TestWebApplicationFactory
/// </summary>
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
    /// <summary>
    /// 集合名稱常數
    /// </summary>
    public const string Name = "Integration Tests";

    // 這個類別不需要任何實作
    // 它只是用來定義 Collection Fixture
}

/// <summary>
/// 整合測試基底類別 - 使用 Collection Fixture 共享容器
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    /// <summary>
    /// WebApplicationFactory 實例
    /// </summary>
    protected readonly TestWebApplicationFactory Factory;

    /// <summary>
    /// HTTP 用戶端
    /// </summary>
    protected readonly HttpClient HttpClient;

    /// <summary>
    /// 資料庫管理器 - 由 Factory 持有單一實例，避免每個測試重建 Respawner
    /// </summary>
    protected DatabaseManager DatabaseManager => Factory.DatabaseManager;

    /// <summary>
    /// Flurl HTTP 用戶端
    /// </summary>
    protected readonly IFlurlClient FlurlClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();

        // 設定 Flurl 用戶端
        FlurlClient = new FlurlClient(HttpClient);
    }

    /// <summary>
    /// 每個測試前執行 - 重設時間（資料庫結構與 Respawner 已在 Factory 初始化）
    /// </summary>
    public virtual Task InitializeAsync()
    {
        ResetTime();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 每個測試後執行 - 清理資料庫資料
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await DatabaseManager.CleanDatabaseAsync();
        FlurlClient.Dispose();
    }

    /// <summary>
    /// 重設時間為測試開始時間 (2024-01-01 00:00:00 UTC)
    /// </summary>
    protected void ResetTime()
    {
        Factory.TimeProvider.SetUtcNow(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    /// <summary>
    /// 前進時間
    /// </summary>
    /// <param name="timeSpan">要前進的時間長度</param>
    protected void AdvanceTime(TimeSpan timeSpan)
    {
        Factory.TimeProvider.Advance(timeSpan);
    }

    /// <summary>
    /// 設定特定時間
    /// </summary>
    /// <param name="time">要設定的時間</param>
    protected void SetTime(DateTimeOffset time)
    {
        Factory.TimeProvider.SetUtcNow(time);
    }

    /// <summary>
    /// 取得當前測試時間
    /// </summary>
    protected DateTimeOffset GetCurrentTime()
    {
        return Factory.TimeProvider.GetUtcNow();
    }
}
