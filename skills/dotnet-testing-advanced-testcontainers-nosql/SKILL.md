---
name: dotnet-testing-advanced-testcontainers-nosql
description: |
  Testcontainers NoSQL 整合測試完整指南。當需要對 MongoDB 或 Redis 進行容器化整合測試時使用。涵蓋 MongoDB 文件操作、Redis 五種資料結構、Collection Fixture 模式。包含 BSON 序列化、索引效能測試、資料隔離策略與容器生命週期管理。
  Make sure to use this skill whenever the user mentions Testcontainers MongoDB, Testcontainers Redis, NoSQL integration test, BSON serialization, or Redis data structure testing, even if they don't explicitly ask for NoSQL container testing.
  Keywords: testcontainers mongodb, testcontainers redis, mongodb integration test, redis integration test, nosql testing, MongoDbContainer, RedisContainer, IMongoDatabase, IConnectionMultiplexer, BSON serialization, BsonDocument, 文件模型測試, 快取測試, Collection Fixture
---

# Testcontainers NoSQL 整合測試指南

## 核心概念

### NoSQL 測試的挑戰

NoSQL 資料庫測試與關聯式資料庫有顯著差異：

1. **文件模型複雜度**：MongoDB 支援巢狀物件、陣列、字典等複雜結構
2. **無固定 Schema**：需要透過測試驗證資料結構的一致性
3. **多樣化資料結構**：Redis 有五種主要資料結構，各有不同使用場景
4. **序列化處理**：BSON (MongoDB) 與 JSON (Redis) 序列化行為需要驗證

### Testcontainers 優勢

- **真實環境模擬**：使用實際的 MongoDB 7.0 和 Redis 7.2 容器
- **一致性測試**：測試結果直接反映正式環境行為
- **隔離性保證**：每個測試環境完全獨立
- **效能驗證**：可進行真實的索引效能測試

## 環境需求

### 必要套件

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <!-- MongoDB 相關套件 -->
    <PackageReference Include="MongoDB.Driver" Version="3.0.0" />
    <PackageReference Include="MongoDB.Bson" Version="3.0.0" />

    <!-- Redis 相關套件 -->
    <PackageReference Include="StackExchange.Redis" Version="2.8.16" />

    <!-- Testcontainers -->
    <PackageReference Include="Testcontainers" Version="4.0.0" />
    <PackageReference Include="Testcontainers.MongoDb" Version="4.0.0" />
    <PackageReference Include="Testcontainers.Redis" Version="4.0.0" />

    <!-- 測試框架 -->
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="AwesomeAssertions" Version="9.1.0" />

    <!-- JSON 序列化與時間測試 -->
    <PackageReference Include="System.Text.Json" Version="9.0.0" />
    <PackageReference Include="Microsoft.Bcl.TimeProvider" Version="9.0.0" />
    <PackageReference Include="Microsoft.Extensions.TimeProvider.Testing" Version="9.0.0" />
  </ItemGroup>
</Project>
```

### 套件版本說明

| 套件                   | 版本   | 用途                               |
| ---------------------- | ------ | ---------------------------------- |
| MongoDB.Driver         | 3.0.0  | MongoDB 官方驅動程式，支援最新功能 |
| MongoDB.Bson           | 3.0.0  | BSON 序列化處理                    |
| StackExchange.Redis    | 2.8.16 | Redis 客戶端，支援 Redis 7.x       |
| Testcontainers.MongoDb | 4.0.0  | MongoDB 容器管理                   |
| Testcontainers.Redis   | 4.0.0  | Redis 容器管理                     |

---

## MongoDB 容器化測試

涵蓋 MongoDB Container Fixture 建立、複雜文件模型設計（巢狀物件、陣列、字典）、BSON 序列化測試、CRUD 操作測試（含樂觀鎖定）以及索引效能與唯一性約束測試。使用 Collection Fixture 模式共享容器，節省 80% 以上的測試時間。

> 完整程式碼範例請參考 [MongoDB 容器化測試詳細指南](references/mongodb-testing.md)

---

## Redis 容器化測試

涵蓋 Redis Container Fixture 建立、快取模型設計（CacheItem 泛型包裝器、UserSession、RecentView、LeaderboardEntry）以及 Redis 五種資料結構（String、Hash、List、Set、Sorted Set）的完整測試範例，包含 TTL 過期測試與資料隔離策略。

> 完整程式碼範例請參考 [Redis 容器化測試詳細指南](references/redis-testing.md)

---

## 最佳實踐

### 1. Collection Fixture 模式

使用 Collection Fixture 共享容器，避免每個測試重啟容器：

```csharp
// 定義集合
[CollectionDefinition("MongoDb Collection")]
public class MongoDbCollectionFixture : ICollectionFixture<MongoDbContainerFixture> { }

// 使用集合
[Collection("MongoDb Collection")]
public class MyMongoTests
{
    public MyMongoTests(MongoDbContainerFixture fixture)
    {
        // 使用共享的容器
    }
}
```

### 2. 資料隔離策略

確保測試間不互相干擾：

```csharp
// MongoDB：使用唯一的 Email/Username
var user = new UserDocument
{
    Username = $"testuser_{Guid.NewGuid():N}",
    Email = $"test_{Guid.NewGuid():N}@example.com"
};

// Redis：使用唯一的 Key 前綴
var testId = Guid.NewGuid().ToString("N")[..8];
var key = $"test:{testId}:mykey";
```

### 3. 清理策略

```csharp
// MongoDB：測試後清理
await fixture.ClearDatabaseAsync();

// Redis：使用 KeyDelete 而非 FLUSHDB（避免權限問題）
var keys = server.Keys(database.Database);
if (keys.Any())
{
    await database.KeyDeleteAsync(keys.ToArray());
}
```

### 4. 效能考量

| 策略               | 說明                                         |
| ------------------ | -------------------------------------------- |
| Collection Fixture | 容器只啟動一次，節省 80%+ 時間               |
| 資料隔離           | 使用唯一 Key/ID 而非清空資料庫               |
| 批次操作           | 使用 InsertManyAsync、SetMultipleStringAsync |
| 索引建立           | 在 Fixture 初始化時建立索引                  |

---

## 常見問題

### Redis FLUSHDB 權限問題

某些 Redis 容器映像檔預設不啟用 admin 模式：

```csharp
// ❌ 錯誤：可能失敗
await server.FlushDatabaseAsync();

// ✅ 正確：使用 KeyDelete
var keys = server.Keys(database.Database);
if (keys.Any())
{
    await database.KeyDeleteAsync(keys.ToArray());
}
```

### MongoDB 唯一索引重複插入

```csharp
// 測試時使用唯一的 Email 避免衝突
var uniqueEmail = $"test_{Guid.NewGuid():N}@example.com";
```

### 容器啟動超時

```csharp
// 增加等待時間
_container = new MongoDbBuilder()
    .WithImage("mongo:7.0")
    .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilPortIsAvailable(27017))
    .Build();
```

---

## 相關技能

- [testcontainers-database](../testcontainers-database/SKILL.md) - PostgreSQL/MSSQL 容器化測試
- [aspnet-integration-testing](../aspnet-integration-testing/SKILL.md) - ASP.NET Core 整合測試
- [nsubstitute-mocking](../../dotnet-testing/nsubstitute-mocking/SKILL.md) - 測試替身與 Mock

---

## 輸出格式

- 產生 Container Fixture 類別（MongoDB/Redis 容器管理）
- 產生測試類別含 Collection Fixture 設定
- 提供 .csproj 套件參考配置
- 包含資料隔離與容器生命週期管理程式碼

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 22 - Testcontainers 整合測試：MongoDB 及 Redis 基礎到進階**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10376740
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day22

### 官方文件

- [Testcontainers 官方網站](https://testcontainers.com/)
- [.NET Testcontainers 文件](https://dotnet.testcontainers.org/)
- [MongoDB.Driver 官方文件](https://www.mongodb.com/docs/drivers/csharp/)
- [StackExchange.Redis 官方文件](https://stackexchange.github.io/StackExchange.Redis/)
- [xUnit Collection Fixtures](https://xunit.net/docs/shared-context#collection-fixture)
