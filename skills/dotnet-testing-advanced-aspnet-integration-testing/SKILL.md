---
name: dotnet-testing-advanced-aspnet-integration-testing
description: |
  ASP.NET Core 整合測試的專門技能。當需要測試 Web API 端點、HTTP 請求/回應、中介軟體、依賴注入時使用。涵蓋 WebApplicationFactory、TestServer、HttpClient 測試、記憶體資料庫配置等。
  Make sure to use this skill whenever the user mentions ASP.NET Core integration testing, WebApplicationFactory, TestServer, HTTP endpoint testing, or middleware testing, even if they don't explicitly ask for integration testing guidance.
  Keywords: integration testing, 整合測試, web api testing, WebApplicationFactory, TestServer, HttpClient testing, controller testing, endpoint testing, 端點測試, RESTful API testing, Microsoft.AspNetCore.Mvc.Testing, CreateClient, ConfigureWebHost, AwesomeAssertions.Web, Be200Ok, Be404NotFound, middleware testing, 中介軟體測試, dependency injection testing
---

# ASP.NET Core 整合測試指南

## 核心概念

### 整合測試的兩種定義

1. **多物件協作測試** — 將兩個以上的類別做整合，測試它們之間的運作是否正確
2. **外部資源整合測試** — 使用到資料庫、外部服務、檔案等外部資源的測試

### 為什麼需要整合測試？

- 確保多個模組整合後能正確工作
- 單元測試無法涵蓋的整合點：Routing、Middleware、Request/Response Pipeline
- 確認 WebApplication 的整合與設定是否正確
- 確認異常處理是否完善

### 測試金字塔定位

| 測試類型   | 測試範圍      | 執行速度 | 維護成本 | 建議比例 |
| ---------- | ------------- | -------- | -------- | -------- |
| 單元測試   | 單一類別/方法 | 很快     | 低       | 70%      |
| 整合測試   | 多個元件      | 中等     | 中等     | 20%      |
| 端對端測試 | 完整流程      | 慢       | 高       | 10%      |

## 原則一：使用 WebApplicationFactory 建立測試環境

透過 `WebApplicationFactory<Program>` 建立記憶體中的 TestServer，搭配 `IClassFixture` 在測試類別間共享。

### 基本流程

1. 測試類別實作 `IClassFixture<WebApplicationFactory<Program>>`
2. 透過建構函式注入 Factory
3. 使用 `factory.CreateClient()` 建立 HttpClient
4. 對 API 端點發送請求並驗證回應

### 自訂 Factory

繼承 `WebApplicationFactory<Program>` 並覆寫 `ConfigureWebHost`：

- 移除原本的 `DbContextOptions`，改用 `UseInMemoryDatabase`
- 使用 `services.Replace()` 替換外部服務為測試版本
- 設定 `builder.UseEnvironment("Testing")` 使用測試環境

### 測試基底類別

建立 `IntegrationTestBase` 封裝共用邏輯：

- Factory 與 HttpClient 初始化
- `SeedShipperAsync()` 等資料準備方法
- `CleanupDatabaseAsync()` 清理方法
- 實作 `IDisposable` 確保資源釋放

> 完整程式碼範例（基本使用、自訂 Factory、測試基底類別）請參考 [references/webapplicationfactory-examples.md](references/webapplicationfactory-examples.md)

## 原則二：使用 AwesomeAssertions.Web 驗證 HTTP 回應

提供流暢的 HTTP 狀態碼斷言與強型別回應驗證：

- **狀態碼斷言**：`Be200Ok()`、`Be201Created()`、`Be400BadRequest()`、`Be404NotFound()` 等
- **Satisfy<T> 驗證**：`response.Should().Be200Ok().And.Satisfy<T>(result => { ... })`
- **優勢**：取代手動 `ReadAsStringAsync()` + `JsonSerializer.Deserialize<T>()` 的冗長程式碼

## 原則三：使用 System.Net.Http.Json 簡化 JSON 操作

- **`PostAsJsonAsync(url, object)`** — 取代手動 `JsonSerializer.Serialize` + `new StringContent`
- **`ReadFromJsonAsync<T>()`** — 取代手動 `ReadAsStringAsync` + `JsonSerializer.Deserialize`

> 完整斷言與 JSON 操作範例請參考 [references/assertion-and-json-examples.md](references/assertion-and-json-examples.md)

## 三個層級的整合測試策略

| Level | 特色                     | 測試重點                         | Factory 設定              |
| ----- | ------------------------ | -------------------------------- | ------------------------- |
| 1     | 無資料庫、無 Service     | API 輸入輸出、路由、狀態碼       | 直接使用原始 Factory      |
| 2     | 有 Service（NSubstitute）| 依賴注入配置、服務互動           | ConfigureTestServices     |
| 3     | 完整架構 + 資料庫        | 真實 CRUD、資料完整性            | InMemoryDatabase 替換     |

### Level 1：簡單的 WebApi 專案

最簡單的形式，直接使用 `WebApplicationFactory<Program>` 測試各個 API 端點的輸入輸出。

### Level 2：相依 Service 的 WebApi 專案

使用 NSubstitute 建立 Service stub，透過自訂 Factory 的 `ConfigureTestServices` 注入測試用服務。

### Level 3：完整的 WebApi 專案

包含真實的資料庫操作，移除原本的 `DbContextOptions` 並改用 InMemoryDatabase，在測試中呼叫 `EnsureCreated()` 建立結構。

> 完整三個層級的程式碼範例請參考 [references/three-level-testing-strategy.md](references/three-level-testing-strategy.md)

## CRUD 操作測試

### 測試涵蓋範圍

| 操作   | 測試重點                                       |
| ------ | ---------------------------------------------- |
| GET    | 單一資源查詢、集合查詢、不存在資源回傳 404     |
| POST   | 建立成功回傳 201、驗證錯誤回傳 400             |
| PUT    | 更新成功、不存在資源處理                       |
| DELETE | 刪除成功回傳 204、不存在資源回傳 404           |

### 測試資料管理

- 每個測試開始前呼叫 `CleanupDatabaseAsync()` 確保乾淨狀態
- 使用 `SeedShipperAsync()` 等方法準備測試資料
- 使用 `Satisfy<T>()` 驗證回應內容的正確性

完整的 CRUD 操作測試程式碼請參考 **[references/crud-test-examples.md](references/crud-test-examples.md)**

## 專案結構建議

```text
tests/
├── Sample.WebApplication.UnitTests/           # 單元測試
├── Sample.WebApplication.Integration.Tests/   # 整合測試
│   ├── Controllers/                           # 控制器整合測試
│   ├── Infrastructure/                        # 測試基礎設施
│   │   └── CustomWebApplicationFactory.cs
│   ├── IntegrationTestBase.cs                 # 測試基底類別
│   └── GlobalUsings.cs
└── Sample.WebApplication.E2ETests/            # 端對端測試
```

## 常見錯誤排除

### `'ObjectAssertions' 未包含 'Be200Ok' 的定義`

此錯誤通常是因為安裝了不相容的斷言套件組合。請確認基礎斷言庫與 Web 擴充套件版本一致。

### `Program` 類別無法存取

確保主專案的 `Program.cs` 包含以下宣告，讓測試專案可以存取：

```csharp
public partial class Program { }
```

### 測試資料互相干擾

- 每個測試開始前清理資料庫
- 不要依賴其他測試的執行順序
- 使用唯一識別碼避免資料衝突

## 套件相容性

| 基礎斷言庫                 | 正確的套件              |
| -------------------------- | ----------------------- |
| FluentAssertions < 8.0.0   | FluentAssertions.Web    |
| FluentAssertions >= 8.0.0  | FluentAssertions.Web.v8 |
| AwesomeAssertions >= 8.0.0 | AwesomeAssertions.Web   |

```xml
<!-- 推薦組合 -->
<PackageReference Include="AwesomeAssertions" Version="9.4.0" />
<PackageReference Include="AwesomeAssertions.Web" Version="1.9.6" />
```

## 最佳實踐

### 應該做

1. **獨立測試專案** — 整合測試與單元測試分離
2. **測試資料隔離** — 每個測試案例有獨立的資料準備和清理
3. **使用基底類別** — 共用設定和輔助方法放在基底類別
4. **明確的命名** — 使用三段式命名法（方法_情境_預期）
5. **適當的測試範圍** — 專注於整合點，不過度測試

### 應該避免

1. **混合測試類型** — 不要將單元測試和整合測試放在同一專案
2. **測試相依性** — 每個測試應獨立，不依賴其他測試的執行順序
3. **過度模擬** — 整合測試應盡量使用真實元件
4. **忽略清理** — 測試完成後要清理測試資料
5. **硬編碼資料** — 使用工廠方法或 Builder 模式建立測試資料

## 輸出格式

- 產生 `CustomWebApplicationFactory.cs`，配置測試用 DI 容器與資料庫替換
- 產生 `IntegrationTestBase.cs` 測試基底類別，包含資料準備與清理方法
- 產生控制器測試類別（`*ControllerTests.cs`），涵蓋 CRUD 操作驗證
- 修改測試專案 `.csproj`，加入 `Microsoft.AspNetCore.Mvc.Testing` 與 `AwesomeAssertions.Web`
- 確保主專案 `Program.cs` 包含 `public partial class Program { }` 以支援測試存取

## 參考資源

### 原始文章

- **Day 19 - 整合測試入門：基礎架構與應用場景**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10376335
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day19

### 官方文件

- [ASP.NET Core 整合測試文件](https://docs.microsoft.com/aspnet/core/test/integration-tests)
- [WebApplicationFactory 使用指南](https://docs.microsoft.com/aspnet/core/test/integration-tests#basic-tests-with-the-default-webapplicationfactory)
- [AwesomeAssertions.Web GitHub](https://github.com/AwesomeAssertions/AwesomeAssertions.Web)

### 相關技能

- `unit-test-fundamentals` - 單元測試基礎
- `nsubstitute-mocking` - 使用 NSubstitute 進行模擬
- `awesome-assertions-guide` - AwesomeAssertions 流暢斷言
- `testcontainers-database` - 使用 Testcontainers 進行容器化資料庫測試
