---
name: dotnet-testing-advanced-aspire-testing
description: |
  .NET Aspire Testing 整合測試框架完整指南。當需要測試 .NET Aspire 分散式應用程式、設定 AppHost 測試或從 Testcontainers 遷移至 Aspire 測試時使用。涵蓋 DistributedApplicationTestingBuilder、容器生命週期管理、多服務編排、Respawn 配置與時間可測試性設計。
  Keywords: aspire testing, .NET Aspire, DistributedApplicationTestingBuilder, AppHost testing, 分散式測試, AspireAppFixture, IAsyncLifetime, ContainerLifetime.Session, 雲原生測試, 多服務整合, Aspire.Hosting.Testing, Respawn
---

# .NET Aspire Testing 整合測試框架

## 前置需求

- .NET 9 SDK 或更高版本
- Docker Desktop（WSL 2 或 Hyper-V）
- AppHost 專案（.NET Aspire 應用編排）

## 核心概念

### .NET Aspire Testing 定位

**.NET Aspire Testing 是封閉式整合測試框架**，專為分散式應用設計：

- 在測試中重現與正式環境相同的服務架構
- 使用真實容器而非模擬服務
- 自動管理容器生命週期

### AppHost 專案的必要性

使用 .NET Aspire Testing 必須建立 AppHost 專案：

- 定義完整的應用架構和容器編排
- 測試重用 AppHost 配置建立環境
- 沒有 AppHost 就無法使用 Aspire Testing

### 與 Testcontainers 的差異

| 特性     | .NET Aspire Testing | Testcontainers |
| -------- | ------------------- | -------------- |
| 設計目標 | 雲原生分散式應用    | 通用容器測試   |
| 配置方式 | AppHost 宣告式定義  | 程式碼手動配置 |
| 服務編排 | 自動處理            | 手動管理       |
| 學習曲線 | 較高                | 中等           |
| 適用場景 | 已用 Aspire 的專案  | 傳統 Web API   |

## 專案結構

```text
MyApp/
├── src/
│   ├── MyApp.Api/                    # WebApi 層
│   ├── MyApp.Application/            # 應用服務層
│   ├── MyApp.Domain/                 # 領域模型
│   └── MyApp.Infrastructure/         # 基礎設施層
├── MyApp.AppHost/                    # Aspire 編排專案 ⭐
│   ├── MyApp.AppHost.csproj
│   └── Program.cs
└── tests/
    └── MyApp.Tests.Integration/      # Aspire Testing 整合測試
        ├── MyApp.Tests.Integration.csproj
        ├── Infrastructure/
        │   ├── AspireAppFixture.cs
        │   ├── IntegrationTestCollection.cs
        │   ├── IntegrationTestBase.cs
        │   └── DatabaseManager.cs
        └── Controllers/
            └── MyControllerTests.cs
```

## 必要套件

### AppHost 專案

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Sdk Name="Aspire.AppHost.Sdk" Version="9.0.0" />

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <IsAspireHost>true</IsAspireHost>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.AppHost" Version="9.1.0" />
    <PackageReference Include="Aspire.Hosting.PostgreSQL" Version="9.1.0" />
    <PackageReference Include="Aspire.Hosting.Redis" Version="9.1.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\src\MyApp.Api\MyApp.Api.csproj" />
  </ItemGroup>
</Project>
```

### 測試專案

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Aspire.Hosting.Testing" Version="13.1.3" />
    <PackageReference Include="AwesomeAssertions" Version="9.4.0" />
    <PackageReference Include="AwesomeAssertions.Web" Version="1.9.6" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
    <PackageReference Include="Respawn" Version="7.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.5" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\MyApp.AppHost\MyApp.AppHost.csproj" />
  </ItemGroup>
</Project>
```

## 容器生命週期管理

使用 `ContainerLifetime.Session` 確保測試資源自動清理：

```csharp
var postgres = builder.AddPostgres("postgres")
                     .WithLifetime(ContainerLifetime.Session);

var redis = builder.AddRedis("redis")
                  .WithLifetime(ContainerLifetime.Session);
```

- **Session**：測試會話結束後自動清理（推薦）
- **Persistent**：容器持續運行，需手動清理

## 等待服務就緒

容器啟動與服務就緒是兩個階段，需要等待機制：

```csharp
private async Task WaitForPostgreSqlReadyAsync()
{
    const int maxRetries = 30;
    const int delayMs = 1000;

    for (int i = 0; i < maxRetries; i++)
    {
        try
        {
            var connectionString = await GetConnectionStringAsync();
            await using var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync();
            return;
        }
        catch (Exception ex) when (i < maxRetries - 1)
        {
            await Task.Delay(delayMs);
        }
    }
    throw new InvalidOperationException("PostgreSQL 服務未能就緒");
}
```

> ⚠️ **不以固定延遲取代就緒探測**：等待必須是「探測 → 失敗 → 退避 → 再探測」的迴圈，
> 由**探測成功**決定何時繼續。`await Task.Delay(30_000)` 之後直接假設服務已就緒，
> 在慢機器上仍會失敗、在快機器上白等，是不穩定測試的常見來源。
>
> 上面示範中的 `Task.Delay(delayMs)` 是**重試之間的退避**，屬於探測迴圈的一部分，
> 與固定延遲不同。`Task.Delay` 這個 API 本身沒有問題，問題在於有沒有探測。

## 資料庫初始化

Aspire 啟動容器但不自動建立資料庫：

```csharp
private async Task EnsureDatabaseExistsAsync(string connectionString)
{
    var builder = new NpgsqlConnectionStringBuilder(connectionString);
    var databaseName = builder.Database;
    builder.Database = "postgres"; // 連到預設資料庫

    await using var connection = new NpgsqlConnection(builder.ToString());
    await connection.OpenAsync();

    var checkDbQuery = $"SELECT 1 FROM pg_database WHERE datname = '{databaseName}'";
    var dbExists = await new NpgsqlCommand(checkDbQuery, connection).ExecuteScalarAsync();

    if (dbExists == null)
    {
        await new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", connection)
            .ExecuteNonQueryAsync();
    }
}
```

## Respawn 配置

使用 PostgreSQL 時指定適配器：

```csharp
_respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
{
    TablesToIgnore = new Table[] { "__EFMigrationsHistory" },
    SchemasToInclude = new[] { "public" },
    DbAdapter = DbAdapter.Postgres  // 明確指定（7.0 起可從 DbConnection 自動推斷，但建議保留）
});
```

> **Respawn 7.0 升級提示**：
> - `DbAdapter` 現可從 `DbConnection` 型別自動推斷（使用 `NpgsqlConnection` 時自動選擇 Postgres），但明確指定仍建議以確保正確性
> - `Microsoft.Data.SqlClient` 不再作為傳遞依賴；若需重置 SQL Server 需自行加入該套件
> - 新增 SQLite、IBM DB2、Snowflake 適配器支援
> - 新增 `FormatDeleteStatement` 可自訂刪除語句

## Collection Fixture 最佳實踐

避免每個測試類別重複啟動容器：

```csharp
[CollectionDefinition(Name)]
public class IntegrationTestCollection : ICollectionFixture<AspireAppFixture>
{
    public const string Name = "Integration Tests";
}

[Collection(IntegrationTestCollection.Name)]
public class MyControllerTests : IntegrationTestBase
{
    public MyControllerTests(AspireAppFixture fixture) : base(fixture) { }
}
```

## 時間可測試性

使用 `TimeProvider` 抽象化時間依賴：

```csharp
// 服務實作
public class ProductService
{
    private readonly TimeProvider _timeProvider;

    public ProductService(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public async Task<Product> CreateAsync(ProductCreateRequest request)
    {
        var now = _timeProvider.GetUtcNow();
        var product = new Product
        {
            CreatedAt = now,
            UpdatedAt = now
        };
        // ...
    }
}

// DI 註冊
builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);
```

## HTTP 回應斷言

`AwesomeAssertions.Web` 的狀態碼專用擴充方法（**只用表列的方法**）：

| 狀態碼 | 方法 |
| --- | --- |
| 200 | `Be200Ok()` |
| 201 | `Be201Created()` |
| 204 | `Be204NoContent()` |
| 400 | `Be400BadRequest()` |
| 401 | `Be401Unauthorized()` |
| 403 | `Be403Forbidden()` |
| 404 | `Be404NotFound()` |
| 409 | `Be409Conflict()` |

以上方法已對照 AwesomeAssertions.Web 1.9.6 與 2.0.3 的 API 文件確認存在。

⛔ 不得使用 `.HaveStatusCode(HttpStatusCode.X)`，該方法在 AwesomeAssertions.Web 中不存在。

**表上沒有的狀態碼**：改用 `response.StatusCode.Should().Be(HttpStatusCode.XXX)`，
**不得為了確認某個 `BeNNN***()` 方法是否存在而搜尋檔案系統**。

範例見 `templates/controller-tests.cs` 的 `CreateProduct_名稱重複_應回傳409狀態衝突`。

## 選擇建議

### 選擇 .NET Aspire Testing

- 專案已使用 .NET Aspire
- 需要測試多服務互動
- 重視統一的開發測試體驗
- 雲原生應用架構

### 選擇 Testcontainers

- 傳統 .NET 專案
- 需要精細的容器控制
- 與非 .NET 服務整合
- 團隊對 Aspire 不熟悉

## 常見問題

### 端點配置衝突

不要手動配置已由 Aspire 自動處理的端點：

```csharp
// ❌ 錯誤：會造成衝突
builder.AddProject<Projects.MyApp_Api>("my-api")
       .WithHttpEndpoint(port: 8080, name: "http");

// ✅ 正確：讓 Aspire 自動處理
builder.AddProject<Projects.MyApp_Api>("my-api")
       .WithReference(postgresDb)
       .WithReference(redis);
```

### Dapper 欄位映射

PostgreSQL snake_case 與 C# PascalCase 的映射：

```csharp
// 在 Program.cs 初始化
DapperTypeMapping.Initialize();

// 或使用 SQL 別名
const string sql = @"
    SELECT id, name, price,
           created_at AS CreatedAt,
           updated_at AS UpdatedAt
    FROM products";
```

## 輸出格式

- 產生 AppHost 專案的 `Program.cs`，定義容器編排與服務參考
- 產生測試基礎設施檔案：`AspireAppFixture.cs`、`IntegrationTestCollection.cs`、`IntegrationTestBase.cs`
- 產生 `DatabaseManager.cs` 處理資料庫初始化與 Respawn 清理
- 產生控制器測試類別（`*ControllerTests.cs`），繼承 `IntegrationTestBase`
- 修改測試專案 `.csproj`，加入 Aspire.Hosting.Testing 與相關套件參考

## 參考資源

### 範例檔案

- [templates/apphost-program.cs](templates/apphost-program.cs) - AppHost 編排定義
- [templates/apphost-project.csproj](templates/apphost-project.csproj) - AppHost 專案設定
- [templates/aspire-app-fixture.cs](templates/aspire-app-fixture.cs) - 測試基礎設施：啟動應用、等待服務就緒，並持有整個 Collection 共用的 `DatabaseManager`
- [templates/integration-test-collection.cs](templates/integration-test-collection.cs) - Collection Fixture 設定
- [templates/integration-test-base.cs](templates/integration-test-base.cs) - 測試基底類別
- [templates/database-manager.cs](templates/database-manager.cs) - 資料庫管理員
- [templates/controller-tests.cs](templates/controller-tests.cs) - 控制器測試範例
- [templates/test-helpers.cs](templates/test-helpers.cs) - `TestHelpers` 靜態類別，提供測試資料的種入、清除與計數（`SeedProductsAsync`、`SeedSpecificProductAsync`、`CleanAllProductsAsync`、`GetProductCountAsync`）
- [templates/test-project.csproj](templates/test-project.csproj) - 測試專案設定

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 24 - .NET Aspire Testing 入門基礎介紹**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10377071
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day24

- **Day 25 - .NET Aspire 整合測試實戰：從 Testcontainers 到 .NET Aspire Testing**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10377197
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day25

### 官方文件

- [.NET Aspire 官方文件](https://learn.microsoft.com/dotnet/aspire/)
- [Aspire Testing 文件](https://learn.microsoft.com/dotnet/aspire/testing)

### 相關技能

- `dotnet-testing-advanced-testcontainers-database` - Testcontainers 資料庫測試
- `dotnet-testing-advanced-testcontainers-nosql` - Testcontainers NoSQL 測試
- `dotnet-testing-advanced-webapi-integration-testing` - 完整 WebAPI 整合測試
