# TUnit + Testcontainers 基礎設施編排

> 本文件從 [SKILL.md](../SKILL.md) 提煉，提供 TUnit 與 Testcontainers 多服務編排的完整範例與細節。

## 使用 [Before(Assembly)] 和 [After(Assembly)] 管理容器

```csharp
public static class GlobalTestInfrastructureSetup
{
    public static PostgreSqlContainer? PostgreSqlContainer { get; private set; }
    public static RedisContainer? RedisContainer { get; private set; }
    public static KafkaContainer? KafkaContainer { get; private set; }
    public static INetwork? Network { get; private set; }

    [Before(Assembly)]
    public static async Task SetupGlobalInfrastructure()
    {
        Console.WriteLine("=== 開始設置全域測試基礎設施 ===");

        // 建立網路
        Network = new NetworkBuilder()
            .WithName("global-test-network")
            .Build();

        await Network.CreateAsync();

        // 建立 PostgreSQL 容器
        PostgreSqlContainer = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithNetwork(Network)
            .WithCleanUp(true)
            .Build();

        await PostgreSqlContainer.StartAsync();

        // 建立 Redis 容器
        RedisContainer = new RedisBuilder()
            .WithNetwork(Network)
            .WithCleanUp(true)
            .Build();

        await RedisContainer.StartAsync();

        // 建立 Kafka 容器
        KafkaContainer = new KafkaBuilder()
            .WithNetwork(Network)
            .WithCleanUp(true)
            .Build();

        await KafkaContainer.StartAsync();

        Console.WriteLine("=== 全域測試基礎設施設置完成 ===");
    }

    [After(Assembly)]
    public static async Task TeardownGlobalInfrastructure()
    {
        Console.WriteLine("=== 開始清理全域測試基礎設施 ===");

        if (KafkaContainer != null)
            await KafkaContainer.DisposeAsync();

        if (RedisContainer != null)
            await RedisContainer.DisposeAsync();

        if (PostgreSqlContainer != null)
            await PostgreSqlContainer.DisposeAsync();

        if (Network != null)
            await Network.DeleteAsync();

        Console.WriteLine("=== 全域測試基礎設施清理完成 ===");
    }
}
```

## 使用全域容器進行測試

```csharp
public class ComplexInfrastructureTests
{
    [Test]
    [Property("Category", "Integration")]
    [Property("Infrastructure", "Complex")]
    [DisplayName("多服務協作：PostgreSQL + Redis + Kafka 完整測試")]
    public async Task CompleteWorkflow_多服務協作_應正確執行()
    {
        var dbConnectionString = GlobalTestInfrastructureSetup.PostgreSqlContainer!.GetConnectionString();
        var redisConnectionString = GlobalTestInfrastructureSetup.RedisContainer!.GetConnectionString();
        var kafkaBootstrapServers = GlobalTestInfrastructureSetup.KafkaContainer!.GetBootstrapAddress();

        await Assert.That(dbConnectionString).IsNotNull();
        await Assert.That(dbConnectionString).Contains("test_db");

        await Assert.That(redisConnectionString).IsNotNull();
        await Assert.That(redisConnectionString).Contains("127.0.0.1");

        await Assert.That(kafkaBootstrapServers).IsNotNull();
        await Assert.That(kafkaBootstrapServers).Contains("127.0.0.1");
    }

    [Test]
    [Property("Category", "Database")]
    [DisplayName("PostgreSQL 資料庫連線驗證")]
    public async Task PostgreSqlDatabase_連線驗證_應成功建立連線()
    {
        var connectionString = GlobalTestInfrastructureSetup.PostgreSqlContainer!.GetConnectionString();

        await Assert.That(connectionString).Contains("test_db");
        await Assert.That(connectionString).Contains("test_user");
    }
}
```

**Assembly 級別容器共享的好處：**

1. **大幅減少啟動時間**：容器只在 Assembly 開始時啟動一次
2. **顯著降低資源消耗**：避免每個測試類別重複建立容器
3. **提升測試穩定性**：減少容器啟動失敗的風險
4. **保持測試隔離**：測試間仍然可以獨立清理資料
