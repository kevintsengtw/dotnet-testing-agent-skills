using Aspire.Hosting;
using Aspire.Hosting.Testing;

namespace MyApp.Tests.Integration.Infrastructure;

/// <summary>
/// Aspire 應用測試 Fixture
/// 使用 .NET Aspire Testing 框架管理分散式應用測試
/// </summary>
public class AspireAppFixture : IAsyncLifetime
{
    private DistributedApplication? _app;
    private HttpClient? _httpClient;

    /// <summary>
    /// 應用程式實例
    /// </summary>
    public DistributedApplication App => _app ?? throw new InvalidOperationException("應用程式尚未初始化");

    /// <summary>
    /// HTTP 客戶端
    /// </summary>
    public HttpClient HttpClient => _httpClient ?? throw new InvalidOperationException("HTTP 客戶端尚未初始化");

    /// <summary>
    /// 初始化 Aspire 測試應用
    /// </summary>
    public async Task InitializeAsync()
    {
        // 建立 Aspire Testing 主機 - 使用 AppHost 定義的架構
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyApp_AppHost>();

        // 建置並啟動應用
        _app = await appHost.BuildAsync();
        await _app.StartAsync();

        // 確保所有服務完全就緒
        await WaitForServicesReadyAsync();

        // 建立 HTTP 客戶端，用於呼叫 API
        _httpClient = _app.CreateHttpClient("myapp-api", "http");
    }

    /// <summary>
    /// 等待所有服務完全就緒
    /// </summary>
    private async Task WaitForServicesReadyAsync()
    {
        await WaitForPostgreSqlReadyAsync();
        await WaitForRedisReadyAsync();
    }

    /// <summary>
    /// 等待 PostgreSQL 服務就緒
    /// </summary>
    private async Task WaitForPostgreSqlReadyAsync()
    {
        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var connectionString = await GetConnectionStringAsync();
                var builder = new Npgsql.NpgsqlConnectionStringBuilder(connectionString);
                builder.Database = "postgres"; // 使用預設資料庫測試連線
                
                await using var connection = new Npgsql.NpgsqlConnection(builder.ToString());
                await connection.OpenAsync();
                await connection.CloseAsync();
                Console.WriteLine("PostgreSQL 服務已就緒");
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"等待 PostgreSQL 就緒，嘗試 {i + 1}/{maxRetries}: {ex.Message}");
                await Task.Delay(delayMs);
            }
        }
        
        throw new InvalidOperationException("PostgreSQL 服務未能在預期時間內就緒");
    }

    /// <summary>
    /// 等待 Redis 服務就緒
    /// </summary>
    private async Task WaitForRedisReadyAsync()
    {
        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var connectionString = await GetRedisConnectionStringAsync();
                await using var connection = StackExchange.Redis.ConnectionMultiplexer.Connect(connectionString);
                var database = connection.GetDatabase();
                await database.PingAsync();
                await connection.DisposeAsync();
                Console.WriteLine("Redis 服務已就緒");
                return;
            }
            catch (Exception ex) when (i < maxRetries - 1)
            {
                Console.WriteLine($"等待 Redis 就緒，嘗試 {i + 1}/{maxRetries}: {ex.Message}");
                await Task.Delay(delayMs);
            }
        }
        
        throw new InvalidOperationException("Redis 服務未能在預期時間內就緒");
    }

    /// <summary>
    /// 清理資源
    /// </summary>
    public async Task DisposeAsync()
    {
        _httpClient?.Dispose();
        
        if (_app != null)
        {
            await _app.DisposeAsync();
        }
    }

    /// <summary>
    /// 取得 PostgreSQL 連線字串
    /// </summary>
    public async Task<string> GetConnectionStringAsync()
    {
        return await _app!.GetConnectionStringAsync("productdb") 
            ?? throw new InvalidOperationException("無法取得 PostgreSQL 連線字串");
    }

    /// <summary>
    /// 取得 Redis 連線字串  
    /// </summary>
    public async Task<string> GetRedisConnectionStringAsync()
    {
        return await _app!.GetConnectionStringAsync("redis")
            ?? throw new InvalidOperationException("無法取得 Redis 連線字串");
    }
}
