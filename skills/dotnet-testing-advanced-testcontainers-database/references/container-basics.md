# 基本容器操作模式 - 完整範例

## PostgreSQL 容器

```csharp
public class PostgreSqlTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;
    private UserDbContext _dbContext = null!;

    public PostgreSqlTests()
    {
        _postgres = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .WithPortBinding(5432, true)  // 自動分配主機埠號
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _dbContext = new UserDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
```

## SQL Server 容器

```csharp
public class SqlServerTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container;
    private UserDbContext _dbContext = null!;

    public SqlServerTests()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("YourStrong@Passw0rd")
            .WithCleanUp(true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<UserDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;

        _dbContext = new UserDbContext(options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        await _container.DisposeAsync();
    }
}
```
