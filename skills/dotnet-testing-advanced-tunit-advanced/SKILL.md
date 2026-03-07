---
name: dotnet-testing-advanced-tunit-advanced
description: |
  TUnit 進階應用完整指南。當需要使用 TUnit 進行資料驅動測試、依賴注入或整合測試時使用。涵蓋 MethodDataSource、ClassDataSource、Matrix Tests、Properties 過濾。包含 Retry/Timeout 控制、WebApplicationFactory 整合、Testcontainers 多服務編排。
  Make sure to use this skill whenever the user mentions TUnit advanced, MethodDataSource, ClassDataSource, Matrix Tests, TUnit dependency injection, TUnit Retry/Timeout, or TUnit WebApplicationFactory, even if they don't explicitly ask for TUnit advanced features.
  Keywords: TUnit advanced, TUnit 進階, MethodDataSource, ClassDataSource, Matrix Tests, MatrixDataSource, MicrosoftDependencyInjectionDataSource, Property, Retry, Timeout, 資料驅動測試, 測試過濾, WebApplicationFactory TUnit, 多容器編排
---

# TUnit 進階應用：資料驅動測試、依賴注入與整合測試實戰

## 資料驅動測試進階技巧

TUnit 提供 MethodDataSource、ClassDataSource、Matrix Tests 三種進階資料來源。MethodDataSource 最靈活，支援動態產生與外部檔案載入；ClassDataSource 適合跨測試類別共享資料與 AutoFixture 整合；Matrix Tests 自動產生所有參數組合（注意控制數量避免爆炸性增長）。

> 完整範例與比較表請參閱 [references/data-driven-testing.md](references/data-driven-testing.md)

---

## Properties 屬性標記與測試過濾

### 基本 Properties 使用

```csharp
[Test]
[Property("Category", "Database")]
[Property("Priority", "High")]
public async Task DatabaseTest_高優先級_應能透過屬性過濾()
{
    await Assert.That(true).IsTrue();
}

[Test]
[Property("Category", "Unit")]
[Property("Priority", "Medium")]
public async Task UnitTest_中等優先級_基本驗證()
{
    await Assert.That(1 + 1).IsEqualTo(2);
}

[Test]
[Property("Category", "Integration")]
[Property("Priority", "Low")]
[Property("Environment", "Development")]
public async Task IntegrationTest_低優先級_僅開發環境執行()
{
    await Assert.That("Hello World").Contains("World");
}
```

### 建立一致的屬性命名規範

```csharp
public static class TestProperties
{
    // 測試類別
    public const string CATEGORY_UNIT = "Unit";
    public const string CATEGORY_INTEGRATION = "Integration";
    public const string CATEGORY_E2E = "E2E";

    // 優先級
    public const string PRIORITY_CRITICAL = "Critical";
    public const string PRIORITY_HIGH = "High";
    public const string PRIORITY_MEDIUM = "Medium";
    public const string PRIORITY_LOW = "Low";

    // 環境
    public const string ENV_DEVELOPMENT = "Development";
    public const string ENV_STAGING = "Staging";
    public const string ENV_PRODUCTION = "Production";
}

[Test]
[Property("Category", TestProperties.CATEGORY_UNIT)]
[Property("Priority", TestProperties.PRIORITY_HIGH)]
public async Task ExampleTest_使用常數_確保一致性()
{
    await Assert.That(1 + 1).IsEqualTo(2);
}
```

### TUnit 測試過濾執行

TUnit 使用 `dotnet run` 而不是 `dotnet test`：

```bash
# 只執行單元測試
dotnet run --treenode-filter "/*/*/*/*[Category=Unit]"

# 只執行高優先級測試
dotnet run --treenode-filter "/*/*/*/*[Priority=High]"

# 組合條件：執行高優先級的單元測試
dotnet run --treenode-filter "/*/*/*/*[(Category=Unit)&(Priority=High)]"

# 或條件：執行單元測試或冒煙測試
dotnet run --treenode-filter "/*/*/*/*[(Category=Unit)|(Suite=Smoke)]"

# 執行特定功能的測試
dotnet run --treenode-filter "/*/*/*/*[Feature=OrderProcessing]"
```

**過濾語法注意事項：**
- 路徑模式 `/*/*/*/*` 代表 Assembly/Namespace/Class/Method 層級
- 屬性名稱大小寫敏感
- 組合條件必須用括號正確包圍

---

## 測試生命週期管理

TUnit 提供完整的生命週期鉤子：`[Before(Class)]` → 建構式 → `[Before(Test)]` → 測試方法 → `[After(Test)]` → Dispose → `[After(Class)]`。另有 Assembly/TestSession 層級與 `[BeforeEvery]`/`[AfterEvery]` 全域鉤子。建構式永遠最先執行，BeforeClass/AfterClass 各只執行一次。

> 完整屬性家族與範例請參閱 [references/lifecycle-management.md](references/lifecycle-management.md)

---

## 依賴注入模式

### TUnit 依賴注入核心概念

TUnit 的依賴注入建構在 Data Source Generators 基礎上：

```csharp
public class MicrosoftDependencyInjectionDataSourceAttribute : DependencyInjectionDataSourceAttribute<IServiceScope>
{
    private static readonly IServiceProvider ServiceProvider = CreateSharedServiceProvider();

    public override IServiceScope CreateScope(DataGeneratorMetadata dataGeneratorMetadata)
    {
        return ServiceProvider.CreateScope();
    }

    public override object? Create(IServiceScope scope, Type type)
    {
        return scope.ServiceProvider.GetService(type);
    }

    private static IServiceProvider CreateSharedServiceProvider()
    {
        return new ServiceCollection()
            .AddSingleton<IOrderRepository, MockOrderRepository>()
            .AddSingleton<IDiscountCalculator, MockDiscountCalculator>()
            .AddSingleton<IShippingCalculator, MockShippingCalculator>()
            .AddSingleton<ILogger<OrderService>, MockLogger<OrderService>>()
            .AddTransient<OrderService>()
            .BuildServiceProvider();
    }
}
```

### 使用 TUnit 依賴注入

```csharp
[MicrosoftDependencyInjectionDataSource]
public class DependencyInjectionTests(OrderService orderService)
{
    [Test]
    public async Task CreateOrder_使用TUnit依賴注入_應正確運作()
    {
        // Arrange - 依賴已經透過 TUnit DI 自動注入
        var items = new List<OrderItem>
        {
            new() { ProductId = "PROD001", ProductName = "測試商品", UnitPrice = 100m, Quantity = 2 }
        };

        // Act
        var order = await orderService.CreateOrderAsync("CUST001", CustomerLevel.VIP會員, items);

        // Assert
        await Assert.That(order).IsNotNull();
        await Assert.That(order.CustomerId).IsEqualTo("CUST001");
        await Assert.That(order.CustomerLevel).IsEqualTo(CustomerLevel.VIP會員);
    }

    [Test]
    public async Task TUnitDependencyInjection_驗證自動注入_服務應為正確類型()
    {
        await Assert.That(orderService).IsNotNull();
        await Assert.That(orderService.GetType().Name).IsEqualTo("OrderService");
    }
}
```

### TUnit DI vs 手動依賴建立比較

| 特性           | TUnit DI                 | 手動依賴建立               |
| :------------- | :----------------------- | :------------------------- |
| **設定複雜度** | 一次設定，重複使用       | 每個測試都需要手動建立     |
| **可維護性**   | 依賴變更只需修改一個地方 | 需要修改所有使用的測試     |
| **一致性**     | 與產品程式碼的 DI 一致   | 可能與實際應用程式不一致   |
| **測試可讀性** | 專注於測試邏輯           | 被依賴建立程式碼干擾       |
| **範圍管理**   | 自動管理服務範圍         | 需要手動管理物件生命週期   |
| **錯誤風險**   | 框架保證依賴正確注入     | 可能遺漏或錯誤建立某些依賴 |

---

## 執行控制與測試品質

- **`[Retry(n)]`**：僅用於外部依賴造成的不穩定測試（網路、檔案鎖定），不用於邏輯錯誤
- **`[Timeout(ms)]`**：為效能敏感測試設定合理上限，搭配 `Stopwatch` 驗證 SLA
- **`[DisplayName]`**：支援 `{0}` 參數插值，讓測試報告更貼近業務語言

> 完整範例（Retry/Timeout/DisplayName）請參閱 [references/execution-control.md](references/execution-control.md)

---

## ASP.NET Core 整合測試

在 TUnit 中使用 `WebApplicationFactory<Program>` 進行 ASP.NET Core 整合測試，透過實作 `IDisposable` 管理生命週期。涵蓋 API 回應驗證、Content-Type 標頭檢查，以及效能基準與並行負載測試。

> 完整 WebApplicationFactory 整合與負載測試範例請參閱 [references/aspnet-integration.md](references/aspnet-integration.md)

---

## TUnit + Testcontainers 基礎設施編排

使用 `[Before(Assembly)]` / `[After(Assembly)]` 在 Assembly 層級管理 PostgreSQL、Redis、Kafka 等多容器編排，搭配 `NetworkBuilder` 建立共用網路。容器僅啟動一次，大幅減少啟動時間與資源消耗，同時保持測試間的資料隔離。

> 完整多容器編排與全域共享範例請參閱 [references/tunit-testcontainers.md](references/tunit-testcontainers.md)

---

## TUnit Engine Modes

### Source Generation Mode（預設模式）

```text
████████╗██╗   ██╗███╗   ██╗██╗████████╗
╚══██╔══╝██║   ██║████╗  ██║██║╚══██╔══╝
   ██║   ██║   ██║██╔██╗ ██║██║   ██║
   ██║   ██║   ██║██║╚██╗██║██║   ██║
   ██║   ╚██████╔╝██║ ╚████║██║   ██║
   ╚═╝    ╚═════╝ ╚═╝  ╚═══╝╚═╝   ╚═╝

   Engine Mode: SourceGenerated
```

**特色與優勢：**

- **編譯時期產生**：所有測試發現邏輯在編譯時產生，不需要執行時反射
- **效能優異**：比反射模式快數倍
- **型別安全**：編譯時期驗證測試配置和資料來源
- **AOT 相容**：完全支援 Native AOT 編譯

### Reflection Mode（反射模式）

```bash
# 啟用反射模式
dotnet run -- --reflection

# 或設定環境變數
$env:TUNIT_EXECUTION_MODE = "reflection"
dotnet run
```

**適用場景：**

- 動態測試發現
- F# 和 VB.NET 專案（自動使用）
- 某些依賴反射的測試模式

### Native AOT 支援

```xml
<PropertyGroup>
    <PublishAot>true</PublishAot>
</PropertyGroup>
```

```bash
dotnet publish -c Release
```

---

## 常見問題與疑難排解

### 測試統計顯示異常問題

**問題現象：** `測試摘要: 總計: 0, 失敗: 0, 成功: 0`

**解決步驟：**

1. **確保專案檔設定正確：**

```xml
<PropertyGroup>
    <IsTestProject>true</IsTestProject>
</PropertyGroup>
```

2. **確保 GlobalUsings.cs 正確：**

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using TUnit.Core;
global using TUnit.Assertions;
global using TUnit.Assertions.Extensions;
```

3. **整合測試的特殊設定：**

```csharp
// 在 WebApi 專案的 Program.cs 最後加上
public partial class Program { }  // 讓整合測試可以存取
```

4. **清理和重建：**

```bash
dotnet clean; dotnet build
dotnet test --verbosity normal
```

### Source Generator 相關問題

**問題：測試類別無法被發現**

- **解決**：確保專案完全重建 (`dotnet clean; dotnet build`)

**問題：編譯時出現奇怪錯誤**

- **解決**：檢查是否有其他 Source Generator 套件，考慮更新到相容版本

### 診斷選項

```ini
# .editorconfig
tunit.enable_verbose_diagnostics = true
```

```xml
<PropertyGroup>
    <TUnitEnableVerboseDiagnostics>true</TUnitEnableVerboseDiagnostics>
</PropertyGroup>
```

---

## 實務建議

### 資料驅動測試的選擇策略

- **MethodDataSource**：適合動態資料、複雜物件、外部檔案載入
- **ClassDataSource**：適合共享資料、AutoFixture 整合、跨測試類別重用
- **Matrix Tests**：適合組合測試，但要注意參數數量避免爆炸性增長

### 執行控制最佳實踐

- **Retry**：只用於真正不穩定的外部依賴測試
- **Timeout**：為效能敏感的測試設定合理限制
- **DisplayName**：讓測試報告更符合業務語言

### 整合測試策略

- 使用 WebApplicationFactory 進行完整的 Web API 測試
- 運用 TUnit + Testcontainers 建立複雜多服務測試環境
- 透過屬性注入系統管理複雜的依賴關係
- 只測試實際存在的功能，避免測試不存在的端點

---

## 範本檔案

| 檔案名稱                                                                 | 說明                                   |
| ------------------------------------------------------------------------ | -------------------------------------- |
| [data-source-examples.cs](templates/data-source-examples.cs)             | MethodDataSource、ClassDataSource 範例 |
| [matrix-tests-examples.cs](templates/matrix-tests-examples.cs)           | Matrix Tests 組合測試範例              |
| [lifecycle-di-examples.cs](templates/lifecycle-di-examples.cs)           | 生命週期管理與依賴注入範例             |
| [execution-control-examples.cs](templates/execution-control-examples.cs) | Retry、Timeout、DisplayName 範例       |
| [aspnet-integration-tests.cs](templates/aspnet-integration-tests.cs)     | ASP.NET Core 整合測試範例              |
| [testcontainers-examples.cs](templates/testcontainers-examples.cs)       | Testcontainers 基礎設施編排範例        |

---

## 輸出格式

- 產生使用 MethodDataSource/ClassDataSource/Matrix Tests 的資料驅動測試類別（.cs 檔案）
- 包含 MicrosoftDependencyInjectionDataSource 依賴注入設定
- 包含 Retry/Timeout/DisplayName 執行控制範例
- 產生 WebApplicationFactory 整合測試與 Testcontainers 多容器編排程式碼

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 29 - TUnit 進階應用：資料驅動測試與依賴注入深度實戰**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10377970
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day29

- **Day 30 - TUnit 進階應用 - 執行控制與測試品質和 ASP.NET Core 整合測試實戰**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10378176
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day30

### TUnit 官方資源

- [TUnit 官方網站](https://tunit.dev/)
- [TUnit GitHub Repository](https://github.com/thomhurst/TUnit)

### 進階功能文件

- [TUnit Method Data Source 文件](https://tunit.dev/docs/test-authoring/method-data-source)
- [TUnit Class Data Source 文件](https://tunit.dev/docs/test-authoring/class-data-source)
- [TUnit Matrix Tests 文件](https://tunit.dev/docs/test-authoring/matrix-tests)
- [TUnit Properties 文件](https://tunit.dev/docs/test-lifecycle/properties)
- [TUnit Dependency Injection 文件](https://tunit.dev/docs/test-lifecycle/dependency-injection)
- [TUnit Retrying 文件](https://tunit.dev/docs/execution/retrying)
- [TUnit Timeouts 文件](https://tunit.dev/docs/execution/timeouts)
- [TUnit Engine Modes 文件](https://tunit.dev/docs/execution/engine-modes)
- [TUnit ASP.NET Core 文件](https://tunit.dev/docs/examples/aspnet)
- [TUnit Complex Test Infrastructure](https://tunit.dev/docs/examples/complex-test-infrastructure-orchestration)

### Testcontainers 相關資源

- [Testcontainers.NET 官方網站](https://dotnet.testcontainers.org/)
- [Testcontainers.NET GitHub](https://github.com/testcontainers/testcontainers-dotnet)

### 相關技能

- `dotnet-testing-advanced-tunit-fundamentals` - TUnit 基礎（前置技能）
- `dotnet-testing-advanced-aspnet-integration-testing` - ASP.NET Core 整合測試
- `dotnet-testing-advanced-testcontainers-database` - Testcontainers 資料庫測試
