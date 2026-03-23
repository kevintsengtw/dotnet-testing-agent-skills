---
name: dotnet-testing-advanced-testcontainers-database
description: |
  使用 Testcontainers 進行容器化資料庫測試的專門技能。當需要測試真實資料庫行為、使用 SQL Server/PostgreSQL/MySQL 容器、測試 EF Core/Dapper 時使用。涵蓋容器啟動、資料庫遷移、測試隔離、容器共享等。
  Make sure to use this skill whenever the user mentions Testcontainers database, SQL Server container, PostgreSQL container, EF Core integration test, Dapper testing, or Collection Fixture, even if they don't explicitly ask for container-based database testing.
  Keywords: testcontainers, 容器測試, container testing, database testing, 資料庫測試, MsSqlContainer, PostgreSqlContainer, MySqlContainer, EF Core testing, Dapper testing, Testcontainers.MsSql, Testcontainers.PostgreSql, GetConnectionString, IAsyncLifetime, CollectionFixture
---

# Testcontainers 資料庫整合測試指南

## EF Core InMemory 的限制

在選擇測試策略前，必須了解 EF Core InMemory 資料庫的重大限制：

### 1. 交易行為與資料庫鎖定

- **不支援資料庫交易 (Transactions)**：`SaveChanges()` 後資料立即儲存，無法進行 Rollback
- **無資料庫鎖定機制**：無法模擬並發 (Concurrency) 情境下的行為

### 2. LINQ 查詢差異

- **查詢翻譯差異**：某些 LINQ 查詢（複雜 GroupBy、JOIN、自訂函數）在 InMemory 中可執行，但轉換成 SQL 時可能失敗
- **Case Sensitivity**：InMemory 預設不區分大小寫，但真實資料庫依賴校對規則 (Collation)
- **效能模擬不足**：無法模擬真實資料庫的效能瓶頸或索引問題

### 3. 資料庫特定功能

InMemory 模式無法測試：預存程序、Triggers、Views、外鍵約束、檢查約束、資料類型精確度、Concurrency Tokens 等。

**結論**：當需要驗證複雜交易邏輯、並發處理、資料庫特定行為時，應使用 Testcontainers 進行整合測試。

## Testcontainers 核心概念

Testcontainers 是一個測試函式庫，提供輕量好用的 API 來啟動 Docker 容器，專門用於整合測試。

### 核心優勢

1. **真實環境測試**：使用真實資料庫，測試實際 SQL 語法與資料庫限制
2. **環境一致性**：確保測試環境與正式環境使用相同服務版本
3. **清潔測試環境**：每個測試有獨立乾淨的環境，容器自動清理
4. **簡化開發環境**：開發者只需 Docker，不需安裝各種服務

## 必要套件

```xml
<ItemGroup>
  <!-- 測試框架 -->
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.3.0" />
  <PackageReference Include="xunit" Version="2.9.3" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.9.3" />
  <PackageReference Include="AwesomeAssertions" Version="9.4.0" />

  <!-- Testcontainers 核心套件 -->
  <PackageReference Include="Testcontainers" Version="4.11.0" />

  <!-- 資料庫容器 -->
  <PackageReference Include="Testcontainers.PostgreSql" Version="4.11.0" />
  <PackageReference Include="Testcontainers.MsSql" Version="4.11.0" />

  <!-- Entity Framework Core -->
  <PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
  <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.0" />

  <!-- Dapper (可選) -->
  <PackageReference Include="Dapper" Version="2.1.72" />
  <PackageReference Include="Microsoft.Data.SqlClient" Version="6.1.4" />
</ItemGroup>
```

> **重要**：使用 `Microsoft.Data.SqlClient` 而非舊版 `System.Data.SqlClient`，提供更好的效能與安全性。

## 環境需求

- Windows 10 版本 2004+，啟用 WSL 2，8GB RAM（建議 16GB+），64GB 磁碟空間
- Docker Desktop 建議設定：Memory 6GB、CPUs 4 cores、Swap 2GB、Disk 64GB

## 基本容器操作模式

使用 `IAsyncLifetime` 管理容器生命週期，在 `InitializeAsync` 中啟動容器並建立 DbContext，在 `DisposeAsync` 中釋放資源。支援 PostgreSQL（`PostgreSqlBuilder`）和 SQL Server（`MsSqlBuilder`）。

> 完整程式碼範例請參考 [references/container-basics.md](references/container-basics.md)

## Collection Fixture 模式：容器共享

在大型專案中，每個測試類別都建立新容器會遇到嚴重的效能瓶頸。Collection Fixture 讓所有測試類別共享同一個容器，**測試執行時間可減少約 67%**。

包含 SQL 腳本外部化策略（將 SQL 檔案從 C# 程式碼分離）與 Wait Strategy 最佳實務。

> 完整 Collection Fixture 實作、SQL 腳本外部化與 Wait Strategy 範例請參考 [references/collection-fixture-and-scripts.md](references/collection-fixture-and-scripts.md)

## EF Core 進階功能測試

涵蓋 Include/ThenInclude 多層關聯查詢、AsSplitQuery 避免笛卡兒積、N+1 查詢問題驗證、AsNoTracking 唯讀查詢最佳化等完整測試範例。

> 完整程式碼範例請參考 [references/orm-advanced-testing.md](references/orm-advanced-testing.md#ef-core-進階功能測試)

## Dapper 進階功能測試

涵蓋基本 CRUD 測試類別設置、QueryMultiple 一對多關聯處理、DynamicParameters 動態查詢建構等完整測試範例。

> 完整程式碼範例請參考 [references/orm-advanced-testing.md](references/orm-advanced-testing.md#dapper-進階功能測試)

## Repository Pattern 設計原則

### 介面分離原則 (ISP) 的應用

- `IProductRepository`：基礎 CRUD 操作介面（GetAll、GetById、Add、Update、Delete）
- `IProductByEFCoreRepository`：EF Core 特有進階功能（SplitQuery、BatchUpdate、NoTracking）
- `IProductByDapperRepository`：Dapper 特有進階功能（多表查詢、動態參數、預存程序）

### 設計優勢

1. **單一職責原則 (SRP)**：每個介面專注於特定職責
2. **介面隔離原則 (ISP)**：使用者只需依賴所需的介面
3. **依賴反轉原則 (DIP)**：高層模組依賴抽象而非具體實作
4. **測試隔離性**：可針對特定功能進行精準測試

## 常見問題處理

### Docker 容器啟動失敗

```bash
# 檢查連接埠是否被佔用
netstat -an | findstr :5432

# 清理未使用的映像檔
docker system prune -a
```

### ContainerNotRunningException（4.8.0+ 新增）

Testcontainers 4.8.0 起，預設等待策略改為等待容器進入 **Running** 狀態。若容器未能正常啟動，會拋出此例外。常見原因：映像檔名稱錯誤、Docker 資源不足、連接埠衝突。

### 記憶體不足問題

- 調整 Docker Desktop 記憶體配置
- 限制同時執行的容器數量
- 使用 Collection Fixture 共享容器

### 測試資料隔離

在 `Dispose` 中按照外鍵約束順序清理資料（OrderItems → Orders → Products → Categories）。

## 輸出格式

- 產生使用 Testcontainers 的 xUnit 整合測試類別（.cs 檔案）
- 包含 IAsyncLifetime 容器生命週期管理程式碼
- 包含 Collection Fixture 共享容器設定
- 產生外部化 SQL 腳本檔案（.sql）與對應的 .csproj 設定
- 包含 Repository Pattern 介面與實作範例

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 20 - Testcontainers 初探：使用 Docker 架設測試環境**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10376401
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day20

- **Day 21 - Testcontainers 整合測試：MSSQL + EF Core 以及 Dapper 基礎應用**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10376524
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day21

### 官方文件

- [Testcontainers 官方網站](https://testcontainers.com/)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [Testcontainers for .NET / Modules](https://dotnet.testcontainers.org/modules/)

### 相關技能

- `dotnet-testing-advanced-testcontainers-nosql` - NoSQL 容器測試（MongoDB、Redis）
- `dotnet-testing-advanced-webapi-integration-testing` - 完整 WebAPI 整合測試
- `dotnet-testing-advanced-aspnet-integration-testing` - ASP.NET Core 基礎整合測試
- `dotnet-testing-advanced-xunit-upgrade-guide` - xUnit v3 升級指南（含 Testcontainers.XunitV3 整合說明）

> **xUnit v3 使用者注意**：搭配 xUnit v3 時，可使用 `Testcontainers.XunitV3`（4.9.0）套件自動管理容器生命週期，取代手動實作 `IAsyncLifetime`。
