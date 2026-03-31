# Collection Fixture 實作與 SQL 腳本外部化 - 完整範例

## Collection Fixture 實作

### Fixture 定義

```csharp
/// <summary>
/// MSSQL 容器的 Collection Fixture
/// </summary>
public class SqlServerContainerFixture : IAsyncLifetime
{
    private readonly MsSqlContainer _container;

    public SqlServerContainerFixture()
    {
        _container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Test123456!")
            .WithCleanUp(true)
            .Build();
    }

    public static string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();

        // 等待容器完全啟動
        await Task.Delay(2000);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}

/// <summary>
/// 定義測試集合
/// </summary>
[CollectionDefinition(nameof(SqlServerCollectionFixture))]
public class SqlServerCollectionFixture : ICollectionFixture<SqlServerContainerFixture>
{
    // 此類別只是用來定義 Collection，不需要實作內容
}
```

### 測試類別整合

```csharp
[Collection(nameof(SqlServerCollectionFixture))]
public class EfCoreTests : IDisposable
{
    private readonly ECommerceDbContext _dbContext;

    public EfCoreTests(ITestOutputHelper testOutputHelper)
    {
        var connectionString = SqlServerContainerFixture.ConnectionString;

        var options = new DbContextOptionsBuilder<ECommerceDbContext>()
            .UseSqlServer(connectionString)
            .EnableSensitiveDataLogging()
            .LogTo(testOutputHelper.WriteLine, LogLevel.Information)
            .Options;

        _dbContext = new ECommerceDbContext(options);
        _dbContext.Database.EnsureCreated();
    }

    public void Dispose()
    {
        // 按照外鍵約束順序清理資料
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM OrderItems");
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM Orders");
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM Products");
        _dbContext.Database.ExecuteSqlRaw("DELETE FROM Categories");
        _dbContext.Dispose();
    }
}
```

## SQL 腳本外部化策略

### 資料夾結構

```text
tests/DatabaseTesting.Tests/
├── SqlScripts/
│   ├── Tables/
│   │   ├── CreateCategoriesTable.sql
│   │   ├── CreateProductsTable.sql
│   │   ├── CreateOrdersTable.sql
│   │   └── CreateOrderItemsTable.sql
│   └── StoredProcedures/
│       └── GetProductSalesReport.sql
```

### .csproj 設定

```xml
<ItemGroup>
  <Content Include="SqlScripts\**\*.sql">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

### 腳本載入實作

```csharp
private void EnsureTablesExist()
{
    var scriptDirectory = Path.Combine(AppContext.BaseDirectory, "SqlScripts");
    if (!Directory.Exists(scriptDirectory)) return;

    // 按照依賴順序執行表格建立腳本
    var orderedScripts = new[]
    {
        "Tables/CreateCategoriesTable.sql",
        "Tables/CreateProductsTable.sql",
        "Tables/CreateOrdersTable.sql",
        "Tables/CreateOrderItemsTable.sql"
    };

    foreach (var scriptPath in orderedScripts)
    {
        var fullPath = Path.Combine(scriptDirectory, scriptPath);
        if (File.Exists(fullPath))
        {
            var script = File.ReadAllText(fullPath);
            _dbContext.Database.ExecuteSqlRaw(script);
        }
    }
}
```

## Wait Strategy 最佳實務

### 內建 Wait Strategy

```csharp
// 等待資料庫就緒命令成功
var postgres = new PostgreSqlBuilder()
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilCommandIsCompleted("pg_isready"))
    .Build();

// 等待日誌訊息出現
var sqlServer = new MsSqlBuilder()
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilMessageIsLogged("SQL Server is now ready for client connections"))
    .Build();
```
