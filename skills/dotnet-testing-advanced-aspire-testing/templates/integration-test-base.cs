namespace MyApp.Tests.Integration.Infrastructure;

/// <summary>
/// 整合測試基底類別 - 使用 Aspire Testing 框架
/// 所有整合測試類別應繼承此基底類別
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly AspireAppFixture Fixture;
    protected readonly HttpClient HttpClient;
    /// <summary>
    /// 資料庫管理器 - 由 Fixture 持有單一實例，避免每個測試重建 Respawner
    /// </summary>
    protected DatabaseManager DatabaseManager => Fixture.DatabaseManager;

    protected IntegrationTestBase(AspireAppFixture fixture)
    {
        Fixture = fixture;
        HttpClient = fixture.HttpClient;
    }

    /// <summary>
    /// 每個測試執行前的初始化（資料庫結構與 Respawner 已在 Fixture 初始化）
    /// </summary>
    public Task InitializeAsync() => Task.CompletedTask;

    /// <summary>
    /// 每個測試執行後的清理
    /// 使用 Respawn 清理測試資料，保持測試隔離
    /// </summary>
    public async Task DisposeAsync()
    {
        await DatabaseManager.CleanDatabaseAsync();
    }

    #region 時間控制輔助方法 (使用 FakeTimeProvider)

    // 如果測試需要控制時間，可在此加入 FakeTimeProvider 相關方法
    // 例如：
    // protected void AdvanceTime(TimeSpan timeSpan) { ... }
    // protected void SetTime(DateTimeOffset dateTime) { ... }

    #endregion
}
