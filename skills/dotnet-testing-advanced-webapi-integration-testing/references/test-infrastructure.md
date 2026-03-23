# 整合測試基礎設施 - 完整範例

## TestWebApplicationFactory

```csharp
public class TestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;
    private RedisContainer? _redisContainer;
    private FakeTimeProvider? _timeProvider;

    public PostgreSqlContainer PostgresContainer => _postgresContainer
        ?? throw new InvalidOperationException("PostgreSQL container 尚未初始化");

    public RedisContainer RedisContainer => _redisContainer
        ?? throw new InvalidOperationException("Redis container 尚未初始化");

    public FakeTimeProvider TimeProvider => _timeProvider
        ?? throw new InvalidOperationException("TimeProvider 尚未初始化");

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("test_db")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithCleanUp(true)
            .Build();

        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithCleanUp(true)
            .Build();

        _timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(config =>
        {
            config.Sources.Clear();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = PostgresContainer.GetConnectionString(),
                ["ConnectionStrings:Redis"] = RedisContainer.GetConnectionString(),
                ["Logging:LogLevel:Default"] = "Warning"
            });
        });

        builder.ConfigureServices(services =>
        {
            // 替換 TimeProvider
            services.Remove(services.Single(d => d.ServiceType == typeof(TimeProvider)));
            services.AddSingleton<TimeProvider>(TimeProvider);
        });

        builder.UseEnvironment("Testing");
    }

    public new async Task DisposeAsync()
    {
        if (_postgresContainer != null) await _postgresContainer.DisposeAsync();
        if (_redisContainer != null) await _redisContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
```

## Collection Fixture 模式

```csharp
[CollectionDefinition("Integration Tests")]
public class IntegrationTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "Integration Tests";
}
```

## 測試基底類別

```csharp
[Collection("Integration Tests")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;
    protected readonly DatabaseManager DatabaseManager;
    protected readonly IFlurlClient FlurlClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        DatabaseManager = new DatabaseManager(factory.PostgresContainer.GetConnectionString());
        FlurlClient = new FlurlClient(HttpClient);
    }

    public virtual async Task InitializeAsync()
    {
        await DatabaseManager.InitializeDatabaseAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await DatabaseManager.CleanDatabaseAsync();
        FlurlClient.Dispose();
    }

    protected void ResetTime()
    {
        Factory.TimeProvider.SetUtcNow(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    protected void AdvanceTime(TimeSpan timeSpan)
    {
        Factory.TimeProvider.Advance(timeSpan);
    }
}
```
