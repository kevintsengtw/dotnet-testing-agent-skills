using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace YourProject.Integration.Tests.Fixtures;

/// <summary>
/// MongoDB 容器 Fixture - 使用 Collection Fixture 模式共享容器
/// 節省 80% 以上的測試執行時間
/// </summary>
public class MongoDbContainerFixture : IAsyncLifetime
{
    private MongoDbContainer? _container;

    /// <summary>
    /// MongoDB 資料庫實例 - 用於測試操作
    /// </summary>
    public IMongoDatabase Database { get; private set; } = null!;
    
    /// <summary>
    /// MongoDB 連線字串
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;
    
    /// <summary>
    /// 測試資料庫名稱
    /// </summary>
    public string DatabaseName { get; } = "testdb";

    /// <summary>
    /// 在測試集合開始時啟動 MongoDB 容器
    /// </summary>
    public async Task InitializeAsync()
    {
        // 使用 MongoDB 7.0 版本確保功能完整性
        _container = new MongoDbBuilder()
                     .WithImage("mongo:7.0")
                     .WithPortBinding(27017, true)  // 自動分配主機埠
                     .Build();

        await _container.StartAsync();

        // 取得連線字串並建立資料庫連線
        ConnectionString = _container.GetConnectionString();
        var client = new MongoClient(ConnectionString);
        Database = client.GetDatabase(DatabaseName);
    }

    /// <summary>
    /// 在測試集合結束時釋放容器資源
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// 清空資料庫中的所有集合 - 用於測試間隔離
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        var collections = await Database.ListCollectionNamesAsync();
        await collections.ForEachAsync(async collectionName =>
        {
            await Database.DropCollectionAsync(collectionName);
        });
    }

    /// <summary>
    /// 取得指定的集合
    /// </summary>
    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return Database.GetCollection<T>(collectionName);
    }
}

/// <summary>
/// 定義使用 MongoDB Fixture 的測試集合
/// 標記為此集合的測試類別將共享同一個容器實例
/// </summary>
[CollectionDefinition("MongoDb Collection")]
public class MongoDbCollectionFixture : ICollectionFixture<MongoDbContainerFixture>
{
    // 此類別不需要實作，僅用於標記集合
    // xUnit 會自動管理 Fixture 的生命週期
}
