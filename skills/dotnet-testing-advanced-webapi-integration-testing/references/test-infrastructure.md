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

    // 整個 Collection 只持有一個實例，Respawner 只建立一次
    public DatabaseManager DatabaseManager { get; private set; } = null!;

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

        // 建立資料庫結構與 Respawner（只做一次，不隨每個測試重建）
        DatabaseManager = new DatabaseManager(_postgresContainer.GetConnectionString());
        await DatabaseManager.InitializeDatabaseAsync();
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
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<TestWebApplicationFactory>
{
    public const string Name = "Integration Tests";
}

// 具體測試類別標記 Collection；抽象基底不標
[Collection(IntegrationTestCollection.Name)]
public class ProductsControllerTests : IntegrationTestBase
{
    public ProductsControllerTests(TestWebApplicationFactory factory) : base(factory) { }
}
```

## 測試基底類別

```csharp
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly TestWebApplicationFactory Factory;
    protected readonly HttpClient HttpClient;
    protected DatabaseManager DatabaseManager => Factory.DatabaseManager;
    protected readonly IFlurlClient FlurlClient;

    protected IntegrationTestBase(TestWebApplicationFactory factory)
    {
        Factory = factory;
        HttpClient = factory.CreateClient();
        FlurlClient = new FlurlClient(HttpClient);
    }

    // FakeTimeProvider 由整個 Collection 共用、只能往前推，這裡不重設時間
    public virtual Task InitializeAsync() => Task.CompletedTask;

    public virtual async Task DisposeAsync()
    {
        await DatabaseManager.CleanDatabaseAsync();
        FlurlClient.Dispose();
    }

    protected void AdvanceTime(TimeSpan timeSpan)
    {
        Factory.TimeProvider.Advance(timeSpan);
    }
}
```
