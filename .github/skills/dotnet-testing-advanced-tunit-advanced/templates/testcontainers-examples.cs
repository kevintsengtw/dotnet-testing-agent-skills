// TUnit + Testcontainers 基礎設施編排範例

using TUnit.Core;
using TUnit.Assertions;
using TUnit.Assertions.Extensions;

// 需要安裝以下套件：
// dotnet add package Testcontainers.PostgreSql
// dotnet add package Testcontainers.Redis
// dotnet add package Testcontainers.Kafka

/*
 * 以下範例使用 Testcontainers.NET 進行容器管理
 * 實際使用時需要取消註解並安裝相關套件
 */

namespace TUnit.Advanced.Testcontainers.Examples;

#region Global Test Infrastructure Setup

/// <summary>
/// 全域測試基礎設施設置
/// 使用 [Before(Assembly)] 和 [After(Assembly)] 管理容器生命週期
/// 
/// 這是最佳實踐：所有測試共享相同的基礎設施
/// </summary>
public static class GlobalTestInfrastructureSetup
{
    // 模擬的容器屬性（實際專案中使用 Testcontainers 類型）
    public static MockPostgreSqlContainer? PostgreSqlContainer { get; private set; }
    public static MockRedisContainer? RedisContainer { get; private set; }
    public static MockKafkaContainer? KafkaContainer { get; private set; }
    public static string? NetworkName { get; private set; }

    /// <summary>
    /// Assembly 層級的設置
    /// 在整個測試組件開始前執行一次
    /// </summary>
    [Before(Assembly)]
    public static async Task SetupGlobalInfrastructure()
    {
        Console.WriteLine("=== 開始設置全域測試基礎設施 ===");

        // 建立網路
        NetworkName = "global-test-network";
        Console.WriteLine($"測試網路已建立: {NetworkName}");

        // 建立 PostgreSQL 容器
        PostgreSqlContainer = new MockPostgreSqlContainer
        {
            ConnectionString = "Host=localhost;Database=test_db;Username=test_user;Password=test_password"
        };
        await PostgreSqlContainer.StartAsync();
        Console.WriteLine($"PostgreSQL 容器已啟動: {PostgreSqlContainer.ConnectionString}");

        // 建立 Redis 容器
        RedisContainer = new MockRedisContainer
        {
            ConnectionString = "127.0.0.1:6379"
        };
        await RedisContainer.StartAsync();
        Console.WriteLine($"Redis 容器已啟動: {RedisContainer.ConnectionString}");

        // 建立 Kafka 容器
        KafkaContainer = new MockKafkaContainer
        {
            BootstrapAddress = "127.0.0.1:9092"
        };
        await KafkaContainer.StartAsync();
        Console.WriteLine($"Kafka 容器已啟動: {KafkaContainer.BootstrapAddress}");

        Console.WriteLine("=== 全域測試基礎設施設置完成 ===");
    }

    /// <summary>
    /// Assembly 層級的清理
    /// 在整個測試組件結束後執行一次
    /// </summary>
    [After(Assembly)]
    public static async Task TeardownGlobalInfrastructure()
    {
        Console.WriteLine("=== 開始清理全域測試基礎設施 ===");

        if (KafkaContainer != null)
        {
            await KafkaContainer.DisposeAsync();
            Console.WriteLine("Kafka 容器已停止");
        }

        if (RedisContainer != null)
        {
            await RedisContainer.DisposeAsync();
            Console.WriteLine("Redis 容器已停止");
        }

        if (PostgreSqlContainer != null)
        {
            await PostgreSqlContainer.DisposeAsync();
            Console.WriteLine("PostgreSQL 容器已停止");
        }

        Console.WriteLine("=== 全域測試基礎設施清理完成 ===");
    }
}

#endregion

#region Mock Container Classes (for demonstration)

/// <summary>
/// 模擬的 PostgreSQL 容器（實際使用 Testcontainers.PostgreSql.PostgreSqlContainer）
/// </summary>
public class MockPostgreSqlContainer : IAsyncDisposable
{
    public string ConnectionString { get; set; } = string.Empty;
    public string State { get; private set; } = "Created";

    public Task StartAsync()
    {
        State = "Running";
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        State = "Stopped";
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// 模擬的 Redis 容器（實際使用 Testcontainers.Redis.RedisContainer）
/// </summary>
public class MockRedisContainer : IAsyncDisposable
{
    public string ConnectionString { get; set; } = string.Empty;
    public string State { get; private set; } = "Created";

    public Task StartAsync()
    {
        State = "Running";
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        State = "Stopped";
        return ValueTask.CompletedTask;
    }
}

/// <summary>
/// 模擬的 Kafka 容器（實際使用 Testcontainers.Kafka.KafkaContainer）
/// </summary>
public class MockKafkaContainer : IAsyncDisposable
{
    public string BootstrapAddress { get; set; } = string.Empty;
    public string State { get; private set; } = "Created";

    public Task StartAsync()
    {
        State = "Running";
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        State = "Stopped";
        return ValueTask.CompletedTask;
    }
}

#endregion

#region Infrastructure Verification Tests

/// <summary>
/// 基礎設施驗證測試
/// 確保測試環境的容器服務正常運作
/// </summary>
public class ComplexInfrastructureTests
{
    /// <summary>
    /// 多服務協作測試
    /// 驗證所有容器都正常啟動並可連線
    /// </summary>
    [Test]
    [Property("Category", "Integration")]
    [Property("Infrastructure", "Complex")]
    [DisplayName("多服務協作：PostgreSQL + Redis + Kafka 完整測試")]
    public async Task CompleteWorkflow_多服務協作_應正確執行()
    {
        // Arrange & Act
        var dbConnectionString = GlobalTestInfrastructureSetup.PostgreSqlContainer!.ConnectionString;
        var redisConnectionString = GlobalTestInfrastructureSetup.RedisContainer!.ConnectionString;
        var kafkaBootstrapServers = GlobalTestInfrastructureSetup.KafkaContainer!.BootstrapAddress;

        // Assert
        await Assert.That(dbConnectionString).IsNotNull();
        await Assert.That(dbConnectionString).Contains("test_db");
        await Assert.That(dbConnectionString).Contains("test_user");

        await Assert.That(redisConnectionString).IsNotNull();
        await Assert.That(redisConnectionString).Contains("127.0.0.1");

        await Assert.That(kafkaBootstrapServers).IsNotNull();
        await Assert.That(kafkaBootstrapServers).Contains("127.0.0.1");

        // 輸出驗證資訊
        Console.WriteLine("=== 多服務協作測試 ===");
        Console.WriteLine($"PostgreSQL: {dbConnectionString}");
        Console.WriteLine($"Redis: {redisConnectionString}");
        Console.WriteLine($"Kafka: {kafkaBootstrapServers}");
        Console.WriteLine("=====================");
    }

    /// <summary>
    /// PostgreSQL 連線驗證
    /// </summary>
    [Test]
    [Property("Category", "Database")]
    [DisplayName("PostgreSQL 資料庫連線驗證")]
    public async Task PostgreSqlDatabase_連線驗證_應成功建立連線()
    {
        // Arrange
        var connectionString = GlobalTestInfrastructureSetup.PostgreSqlContainer!.ConnectionString;

        // Act & Assert
        await Assert.That(connectionString).Contains("test_db");
        await Assert.That(connectionString).Contains("test_user");
        await Assert.That(connectionString).Contains("test_password");

        Console.WriteLine($"Database connection verified: {connectionString}");
    }

    /// <summary>
    /// Redis 服務驗證
    /// </summary>
    [Test]
    [Property("Category", "Cache")]
    [DisplayName("Redis 快取服務驗證")]
    public async Task RedisCache_快取服務_應正確啟動()
    {
        // Arrange
        var connectionString = GlobalTestInfrastructureSetup.RedisContainer!.ConnectionString;

        // Act & Assert
        await Assert.That(connectionString).IsNotNull();
        await Assert.That(connectionString).Contains("127.0.0.1");

        Console.WriteLine($"Redis connection verified: {connectionString}");
    }

    /// <summary>
    /// Kafka 服務驗證
    /// </summary>
    [Test]
    [Property("Category", "MessageQueue")]
    [DisplayName("Kafka 訊息佇列服務驗證")]
    public async Task KafkaMessageQueue_訊息佇列_應正確啟動()
    {
        // Arrange
        var bootstrapServers = GlobalTestInfrastructureSetup.KafkaContainer!.BootstrapAddress;

        // Act & Assert
        await Assert.That(bootstrapServers).IsNotNull();
        await Assert.That(bootstrapServers).Contains("127.0.0.1");

        Console.WriteLine($"Kafka connection verified: {bootstrapServers}");
    }
}

#endregion

#region Advanced Dependency Tests

/// <summary>
/// 進階依賴管理測試
/// 展示如何在測試中使用容器提供的服務
/// </summary>
public class AdvancedDependencyTests
{
    /// <summary>
    /// 網路基礎設施驗證
    /// </summary>
    [Test]
    [Property("Category", "Network")]
    [DisplayName("網路基礎設施驗證")]
    public async Task NetworkInfrastructure_網路設定_應正確建立()
    {
        // Arrange & Act
        var networkName = GlobalTestInfrastructureSetup.NetworkName;

        // Assert
        await Assert.That(networkName).IsEqualTo("global-test-network");

        Console.WriteLine($"Test network verified: {networkName}");
    }

    /// <summary>
    /// 容器狀態驗證
    /// </summary>
    [Test]
    [Property("Category", "Infrastructure")]
    [DisplayName("所有容器運行狀態驗證")]
    public async Task AllContainers_運行狀態_應為Running()
    {
        // Assert
        await Assert.That(GlobalTestInfrastructureSetup.PostgreSqlContainer!.State).IsEqualTo("Running");
        await Assert.That(GlobalTestInfrastructureSetup.RedisContainer!.State).IsEqualTo("Running");
        await Assert.That(GlobalTestInfrastructureSetup.KafkaContainer!.State).IsEqualTo("Running");

        Console.WriteLine("All containers are running");
    }
}

#endregion

#region Test Infrastructure Manager

/// <summary>
/// 測試基礎設施管理器
/// 提供統一的容器管理和設定產生
/// </summary>
public class TestInfrastructureManager
{
    /// <summary>
    /// 取得完整的應用程式設定
    /// </summary>
    private Dictionary<string, string> GetTestConfiguration()
    {
        return new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = GlobalTestInfrastructureSetup.PostgreSqlContainer!.ConnectionString,
            ["ConnectionStrings:Redis"] = GlobalTestInfrastructureSetup.RedisContainer!.ConnectionString,
            ["Kafka:BootstrapServers"] = GlobalTestInfrastructureSetup.KafkaContainer!.BootstrapAddress,
            ["Environment"] = "Testing"
        };
    }

    /// <summary>
    /// 設定產生驗證
    /// </summary>
    [Test]
    [Property("Category", "Infrastructure")]
    [DisplayName("基礎設施管理器：設定產生驗證")]
    public async Task InfrastructureManager_設定產生_應提供完整設定()
    {
        // Act
        var configuration = GetTestConfiguration();

        // Assert
        await Assert.That(configuration).IsNotNull();
        await Assert.That(configuration.ContainsKey("ConnectionStrings:DefaultConnection")).IsTrue();
        await Assert.That(configuration.ContainsKey("ConnectionStrings:Redis")).IsTrue();
        await Assert.That(configuration.ContainsKey("Kafka:BootstrapServers")).IsTrue();
        await Assert.That(configuration.ContainsKey("Environment")).IsTrue();

        await Assert.That(configuration["Environment"]).IsEqualTo("Testing");
        await Assert.That(configuration["ConnectionStrings:DefaultConnection"]).Contains("test_db");
        await Assert.That(configuration["ConnectionStrings:Redis"]).Contains("127.0.0.1");

        // 輸出設定資訊
        Console.WriteLine("Generated test configuration:");
        foreach (var kvp in configuration)
        {
            Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
        }
    }
}

#endregion

#region Real Testcontainers Example (Commented)

/*
 * 實際的 Testcontainers 使用範例
 * 需要安裝以下套件：
 * 
 * dotnet add package Testcontainers.PostgreSql
 * dotnet add package Testcontainers.Redis  
 * dotnet add package Testcontainers.Kafka
 * 
 * using Testcontainers.PostgreSql;
 * using Testcontainers.Redis;
 * using Testcontainers.Kafka;
 * using DotNet.Testcontainers.Builders;
 * using DotNet.Testcontainers.Containers;
 * using DotNet.Testcontainers.Networks;
 * 
 * public static class RealGlobalTestInfrastructureSetup
 * {
 *     public static PostgreSqlContainer? PostgreSqlContainer { get; private set; }
 *     public static RedisContainer? RedisContainer { get; private set; }
 *     public static KafkaContainer? KafkaContainer { get; private set; }
 *     public static INetwork? Network { get; private set; }
 * 
 *     [Before(Assembly)]
 *     public static async Task SetupGlobalInfrastructure()
 *     {
 *         // 建立網路
 *         Network = new NetworkBuilder()
 *             .WithName("global-test-network")
 *             .Build();
 * 
 *         await Network.CreateAsync();
 * 
 *         // 建立 PostgreSQL 容器
 *         PostgreSqlContainer = new PostgreSqlBuilder()
 *             .WithDatabase("test_db")
 *             .WithUsername("test_user")
 *             .WithPassword("test_password")
 *             .WithNetwork(Network)
 *             .WithCleanUp(true)
 *             .Build();
 * 
 *         await PostgreSqlContainer.StartAsync();
 * 
 *         // 建立 Redis 容器
 *         RedisContainer = new RedisBuilder()
 *             .WithNetwork(Network)
 *             .WithCleanUp(true)
 *             .Build();
 * 
 *         await RedisContainer.StartAsync();
 * 
 *         // 建立 Kafka 容器
 *         KafkaContainer = new KafkaBuilder()
 *             .WithNetwork(Network)
 *             .WithCleanUp(true)
 *             .Build();
 * 
 *         await KafkaContainer.StartAsync();
 *     }
 * 
 *     [After(Assembly)]
 *     public static async Task TeardownGlobalInfrastructure()
 *     {
 *         if (KafkaContainer != null)
 *             await KafkaContainer.DisposeAsync();
 * 
 *         if (RedisContainer != null)
 *             await RedisContainer.DisposeAsync();
 * 
 *         if (PostgreSqlContainer != null)
 *             await PostgreSqlContainer.DisposeAsync();
 * 
 *         if (Network != null)
 *             await Network.DeleteAsync();
 *     }
 * }
 */

#endregion

#region Performance Optimization Notes

/*
 * Assembly 級別容器共享的效能優勢：
 * 
 * 1. 大幅減少啟動時間
 *    - 容器只在 Assembly 開始時啟動一次
 *    - 避免每個測試類別重複建立容器
 * 
 * 2. 顯著降低資源消耗
 *    - 減少 Docker 容器數量
 *    - 降低記憶體和 CPU 使用
 * 
 * 3. 提升測試穩定性
 *    - 減少容器啟動失敗的風險
 *    - 容器狀態在測試間保持一致
 * 
 * 4. 保持測試隔離
 *    - 測試間仍然可以獨立清理資料
 *    - 容器狀態不會互相干擾
 * 
 * 最佳實踐：
 * - 使用 [Before(Assembly)] 啟動所有共享容器
 * - 使用 [After(Assembly)] 清理所有容器
 * - 在測試中使用靜態屬性存取容器
 * - 考慮使用 Testcontainers 的 WithCleanUp(true) 確保測試後清理
 */

#endregion
