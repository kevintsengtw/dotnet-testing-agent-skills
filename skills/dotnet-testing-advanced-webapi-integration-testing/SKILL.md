---
name: dotnet-testing-advanced-webapi-integration-testing
description: |
  ASP.NET Core WebApi 整合測試完整指南。當需要對 WebApi 端點進行整合測試或驗證 ProblemDetails 錯誤格式時使用。涵蓋 WebApplicationFactory、IExceptionHandler、Testcontainers 多容器編排、Flurl URL 建構與 AwesomeAssertions HTTP 驗證。
  Make sure to use this skill whenever the user mentions WebApi integration testing, ProblemDetails, IExceptionHandler, Flurl, Respawn, or multi-container test orchestration, even if they don't explicitly ask for WebApi testing guidance.
  Keywords: webapi integration testing, WebApplicationFactory, asp.net core integration test, webapi 整合測試, IExceptionHandler, ProblemDetails, ValidationProblemDetails, AwesomeAssertions, Flurl, Respawn, Be201Created, Be400BadRequest, 多容器測試, Collection Fixture, 全域例外處理
---

# WebApi 整合測試

## 學習目標

完成本技能學習後，您將能夠：

1. 建立完整的 WebApi 整合測試架構
2. 使用 `IExceptionHandler` 實作現代化異常處理
3. 驗證 `ProblemDetails` 和 `ValidationProblemDetails` 標準格式
4. 使用 Flurl 簡化 HTTP 測試的 URL 建構
5. 使用 AwesomeAssertions 進行精確的 HTTP 回應驗證
6. 建立多容器 (PostgreSQL + Redis) 測試環境

## 核心概念

### IExceptionHandler - 現代化異常處理

ASP.NET Core 8+ 引入的 `IExceptionHandler` 介面提供了比傳統 middleware 更優雅的錯誤處理方式。`GlobalExceptionHandler` 依據例外類型（KeyNotFoundException → 404、ArgumentException → 400、其他 → 500）產生對應的 `ProblemDetails` 回應。

### ProblemDetails 標準格式

RFC 7807 定義的統一錯誤回應格式：

| 欄位       | 說明               |
| ---------- | ------------------ |
| `type`     | 問題類型的 URI     |
| `title`    | 簡短的錯誤描述     |
| `status`   | HTTP 狀態碼        |
| `detail`   | 詳細的錯誤說明     |
| `instance` | 發生問題的實例 URI |

### ValidationProblemDetails - 驗證錯誤專用

繼承自 ProblemDetails，額外包含 `errors` 字典，記錄每個欄位的驗證錯誤訊息。

### FluentValidation 異常處理器

FluentValidation 異常處理器實作 `IExceptionHandler` 介面，專門處理 `ValidationException`，將驗證錯誤轉換為標準的 `ValidationProblemDetails` 格式回應。處理器之間按照註冊順序執行，特定處理器（如 FluentValidation）必須在全域處理器之前註冊。

> 完整實作程式碼請參閱 [references/exception-handler-details.md](references/exception-handler-details.md)

## 整合測試基礎設施

測試基礎設施由三個核心組件構成：

- **TestWebApplicationFactory**：繼承 `WebApplicationFactory<Program>`，配置多容器（PostgreSQL + Redis）與 DI 替換（如 FakeTimeProvider）
- **IntegrationTestCollection**：Collection Fixture 定義，確保容器共享
- **IntegrationTestBase**：測試基底類別，提供 HttpClient、DatabaseManager、FlurlClient 與時間控制方法

> 完整基礎設施程式碼請參閱 [references/test-infrastructure.md](references/test-infrastructure.md)

## Flurl 簡化 URL 建構

```csharp
// 傳統方式
var url = $"/products?pageSize={pageSize}&page={page}&keyword={keyword}";

// 使用 Flurl
var url = "/products"
    .SetQueryParam("pageSize", 5)
    .SetQueryParam("page", 2)
    .SetQueryParam("keyword", "特殊");
```

## 測試範例

涵蓋成功建立產品（201 Created）、驗證錯誤（400 BadRequest + ValidationProblemDetails）、資源不存在（404 NotFound + ProblemDetails）、分頁查詢（200 OK + PagedResult）等完整測試範例，以及 TestHelpers 資料管理策略。

> 完整測試範例與資料管理程式碼請參閱 [references/test-examples.md](references/test-examples.md)

## 最佳實務

### 1. 測試結構設計

- **單一職責**：每個測試專注於一個特定場景
- **3A 模式**：清楚區分 Arrange、Act、Assert
- **清晰命名**：方法名稱表達測試意圖

### 2. 錯誤處理驗證

- **ValidationProblemDetails**：驗證錯誤回應格式
- **ProblemDetails**：驗證業務異常回應
- **HTTP 狀態碼**：確認正確的狀態碼

### 3. 效能考量

- **容器共享**：使用 Collection Fixture
- **資料清理**：測試後清理資料，不重建容器
- **並行執行**：確保測試獨立性

## 相依套件

```xml
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="AwesomeAssertions" Version="9.4.0" />
<PackageReference Include="Testcontainers.PostgreSql" Version="4.11.0" />
<PackageReference Include="Testcontainers.Redis" Version="4.11.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="9.0.0" />
<PackageReference Include="Flurl" Version="4.0.0" />
<PackageReference Include="Respawn" Version="7.0.0" />
```

## 專案結構

```text
src/
├── Api/                          # WebApi 層
├── Application/                  # 應用服務層
├── Domain/                       # 領域模型
└── Infrastructure/               # 基礎設施層
tests/
└── Integration/
    ├── Fixtures/
    │   ├── TestWebApplicationFactory.cs
    │   ├── IntegrationTestCollection.cs
    │   └── IntegrationTestBase.cs
    ├── Handlers/
    │   ├── GlobalExceptionHandler.cs
    │   └── FluentValidationExceptionHandler.cs
    ├── Helpers/
    │   ├── DatabaseManager.cs
    │   └── TestHelpers.cs
    ├── SqlScripts/
    │   └── Tables/
    └── Controllers/
        └── ProductsControllerTests.cs
```

## 輸出格式

- 產生 `TestWebApplicationFactory.cs`，配置多容器（PostgreSQL + Redis）與 DI 替換
- 產生 `IntegrationTestCollection.cs` 與 `IntegrationTestBase.cs` 測試基礎設施
- 產生 `DatabaseManager.cs`，整合 Respawn 進行測試資料清理
- 產生控制器測試類別，驗證 CRUD、ProblemDetails 與 ValidationProblemDetails
- 產生 `GlobalExceptionHandler.cs` 與 `FluentValidationExceptionHandler.cs` 異常處理器

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 23 - 整合測試實戰：WebApi 服務的整合測試**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10376873
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day23

### 官方文件

- [ASP.NET Core 整合測試](https://docs.microsoft.com/aspnet/core/test/integration-tests)
- [IExceptionHandler 文件](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling)
- [ProblemDetails RFC 7807](https://tools.ietf.org/html/rfc7807)
- [Testcontainers for .NET](https://dotnet.testcontainers.org/)
- [AwesomeAssertions](https://awesomeassertions.org/)
- [Flurl HTTP Client](https://flurl.dev/)
- [Respawn](https://github.com/jbogard/Respawn)

### 相關技能

- `dotnet-testing-advanced-aspnet-integration-testing` - ASP.NET Core 基礎整合測試
- `dotnet-testing-advanced-testcontainers-database` - 資料庫容器測試
- `dotnet-testing-advanced-testcontainers-nosql` - NoSQL 容器測試
- `dotnet-testing-fluentvalidation-testing` - FluentValidation 測試
