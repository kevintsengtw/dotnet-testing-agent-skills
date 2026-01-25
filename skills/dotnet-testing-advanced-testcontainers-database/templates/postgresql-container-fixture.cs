// PostgreSQL 容器 Fixture 範本
// 用於單一測試類別的 PostgreSQL 容器配置
// 適用於不需要跨類別共享容器的測試場景

using Testcontainers.PostgreSql;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Tests.Fixtures;

/// <summary>
/// PostgreSQL 容器的 Fixture，實作 IAsyncLifetime 進行非同步生命週期管理
/// </summary>
/// <remarks>
/// 使用時機：
/// - 單一測試類別需要 PostgreSQL 容器
/// - 不需要跨測試類別共享容器
/// - 測試需要獨立的資料庫環境
/// </remarks>
public class PostgreSqlContainerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private YourDbContext _dbContext = null!;

    public PostgreSqlContainerTests()
    {
        _postgres = new PostgreSqlBuilder()
            // 使用 Alpine 版本以減少容器大小和啟動時間
            .WithImage("postgres:15-alpine")
            // 設定資料庫名稱
            .WithDatabase("testdb")
            // 設定使用者名稱
            .WithUsername("testuser")
            // 設定密碼
            .WithPassword("testpass")
            // 使用隨機埠號避免衝突（true = 自動分配）
            .WithPortBinding(5432, true)
            // 測試完成後自動清理容器
            .WithCleanUp(true)
            .Build();
    }

    /// <summary>
    /// 初始化容器和資料庫上下文
    /// </summary>
    public async Task InitializeAsync()
    {
        // 啟動 PostgreSQL 容器
        await _postgres.StartAsync();

        // 取得連線字串並建立 DbContext
        var options = new DbContextOptionsBuilder<YourDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            // 啟用敏感資料日誌（僅限開發/測試環境）
            .EnableSensitiveDataLogging()
            .Options;

        _dbContext = new YourDbContext(options);
        
        // 確保資料庫已建立
        await _dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// 清理容器和資料庫上下文
    /// </summary>
    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }

    // ===== 測試方法範例 =====

    [Fact]
    public async Task CreateEntity_WithValidData_ShouldPersistToDatabase()
    {
        // Arrange
        var entity = new YourEntity
        {
            Name = "Test Entity",
            Description = "Created in PostgreSQL container"
        };

        // Act
        _dbContext.YourEntities.Add(entity);
        await _dbContext.SaveChangesAsync();

        // Assert
        var saved = await _dbContext.YourEntities.FindAsync(entity.Id);
        saved.Should().NotBeNull();
        saved!.Name.Should().Be("Test Entity");
    }

    [Fact]
    public async Task QueryEntity_WithValidId_ShouldReturnCorrectData()
    {
        // Arrange
        var entity = new YourEntity { Name = "Query Test" };
        _dbContext.YourEntities.Add(entity);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _dbContext.YourEntities
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == entity.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Query Test");
    }

    [Fact]
    public void GetConnectionString_AfterContainerStarted_ShouldReturnValidConnectionString()
    {
        // Act
        var connectionString = _postgres.GetConnectionString();
        var mappedPort = _postgres.GetMappedPublicPort(5432);

        // Assert
        connectionString.Should().NotBeNullOrEmpty();
        connectionString.Should().Contain($"Port={mappedPort}");
        connectionString.Should().Contain("Database=testdb");
        connectionString.Should().Contain("Username=testuser");
    }
}

// ===== 進階配置範例 =====

/// <summary>
/// 帶有 Wait Strategy 的 PostgreSQL 容器配置
/// </summary>
public class PostgreSqlWithWaitStrategyTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public PostgreSqlWithWaitStrategyTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            // 使用 Wait Strategy 確保容器完全就緒
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilPortIsAvailable(5432)
                .UntilMessageIsLogged("database system is ready to accept connections"))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}

/// <summary>
/// 帶有資源限制的 PostgreSQL 容器配置
/// 適用於 CI/CD 環境或資源受限的開發機器
/// </summary>
public class PostgreSqlWithResourceLimitsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    public PostgreSqlWithResourceLimitsTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            // 使用記憶體檔案系統提升效能
            .WithTmpfsMount("/var/lib/postgresql/data")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
    }
}

// ===== 替換這些類別為您的實際實作 =====

public class YourDbContext : DbContext
{
    public YourDbContext(DbContextOptions<YourDbContext> options) : base(options) { }
    public DbSet<YourEntity> YourEntities { get; set; }
}

public class YourEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
