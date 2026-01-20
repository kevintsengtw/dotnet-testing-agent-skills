using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace YourProject.Tests.Integration.Fixtures;

/// <summary>
/// 測試用 WebApplicationFactory - 管理 Testcontainers 與服務配置
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;
    private FakeTimeProvider? _timeProvider;

    /// <summary>
    /// PostgreSQL 容器實例
    /// </summary>
    public PostgreSqlContainer PostgresContainer => _postgresContainer
        ?? throw new InvalidOperationException("PostgreSQL container 尚未初始化");

    /// <summary>
    /// Redis 容器實例
    /// </summary>
    public RedisContainer RedisContainer => _redisContainer
        ?? throw new InvalidOperationException("Redis container 尚未初始化");

    /// <summary>
    /// 可控制的時間提供者
    /// </summary>
    public FakeTimeProvider TimeProvider => _timeProvider
        ?? throw new InvalidOperationException("TimeProvider 尚未初始化");

    /// <summary>
    /// 初始化 Testcontainers
    /// </summary>
    public async Task InitializeAsync()
    {
        // 建立 PostgreSQL container
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        // 建立 Redis container
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();

        // 建立 FakeTimeProvider - 固定起始時間
        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        // 啟動容器
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    /// <summary>
    /// 配置測試用 WebHost
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            // 移除現有的設定來源
            config.Sources.Clear();

            // 添加測試專用設定
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = PostgresContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = RedisContainer.GetConnectionString(),
                ["Logging:LogLevel:Default"] = "Warning",
                ["Logging:LogLevel:System"] = "Warning",
                ["Logging:LogLevel:Microsoft"] = "Warning"
            });
        });

        builder.ConfigureTestServices(services =>
        {
            // 移除原有的 TimeProvider
            var timeProviderDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(TimeProvider));
            if (timeProviderDescriptor != null)
            {
                services.Remove(timeProviderDescriptor);
            }

            // 註冊 FakeTimeProvider
            services.AddSingleton<TimeProvider>(TimeProvider);
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// 釋放資源
    /// </summary>
    public new async Task DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.DisposeAsync();
        }

        if (_redisContainer != null)
        {
            await _redisContainer.DisposeAsync();
        }

        await base.DisposeAsync();
    }
}
