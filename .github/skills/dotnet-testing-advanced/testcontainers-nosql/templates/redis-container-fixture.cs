using StackExchange.Redis;
using Testcontainers.Redis;

namespace YourProject.Integration.Tests.Fixtures;

/// <summary>
/// Redis 容器 Fixture - 使用 Collection Fixture 模式共享容器
/// 支援 Redis 7.x 的所有功能，包含五種資料結構測試
/// </summary>
public class RedisContainerFixture : IAsyncLifetime
{
    private RedisContainer? _container;

    /// <summary>
    /// Redis 連線多工器 - 用於管理連線池
    /// </summary>
    public IConnectionMultiplexer Connection { get; private set; } = null!;
    
    /// <summary>
    /// Redis 資料庫實例 - 用於執行命令
    /// </summary>
    public IDatabase Database { get; private set; } = null!;
    
    /// <summary>
    /// Redis 連線字串
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// 在測試集合開始時啟動 Redis 容器
    /// </summary>
    public async Task InitializeAsync()
    {
        // 使用 Redis 7.2 版本，支援最新功能
        _container = new RedisBuilder()
                     .WithImage("redis:7.2")
                     .WithPortBinding(6379, true)  // 自動分配主機埠
                     .Build();

        await _container.StartAsync();

        // 建立 Redis 連線
        ConnectionString = _container.GetConnectionString();
        Connection = await ConnectionMultiplexer.ConnectAsync(ConnectionString);
        Database = Connection.GetDatabase();
    }

    /// <summary>
    /// 在測試集合結束時釋放資源
    /// </summary>
    public async Task DisposeAsync()
    {
        if (Connection != null)
        {
            await Connection.DisposeAsync();
        }

        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// 清空資料庫 - 使用 KeyDelete 而非 FLUSHDB
    /// 某些 Redis 容器映像檔預設不啟用 admin 模式，FLUSHDB 會失敗
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        var server = Connection.GetServer(Connection.GetEndPoints().First());
        var keys = server.Keys(Database.Database);
        if (keys.Any())
        {
            await Database.KeyDeleteAsync(keys.ToArray());
        }
    }

    /// <summary>
    /// 取得 Redis Server 實例 - 用於進階操作如 Keys 掃描
    /// </summary>
    public IServer GetServer()
    {
        return Connection.GetServer(Connection.GetEndPoints().First());
    }

    /// <summary>
    /// 刪除符合模式的所有 Key
    /// </summary>
    public async Task DeleteKeysByPatternAsync(string pattern)
    {
        var server = GetServer();
        var keys = server.Keys(Database.Database, pattern);
        if (keys.Any())
        {
            await Database.KeyDeleteAsync(keys.ToArray());
        }
    }
}

/// <summary>
/// 定義使用 Redis Fixture 的測試集合
/// 標記為此集合的測試類別將共享同一個容器實例
/// </summary>
[CollectionDefinition("Redis Collection")]
public class RedisCollectionFixture : ICollectionFixture<RedisContainerFixture>
{
    // 此類別不需要實作，僅用於標記集合
    // xUnit 會自動管理 Fixture 的生命週期
}
