---
name: dotnet-testing-private-internal-testing
description: |
  Private 與 Internal 成員測試策略指南。當需要測試私有或內部成員、設定 InternalsVisibleTo 或評估可測試性設計時使用。涵蓋設計優先思維、反射測試、策略模式重構、AbstractLogger 模式與決策框架。
  Make sure to use this skill whenever the user mentions private method testing, internal testing, InternalsVisibleTo, reflection testing, or testability design, even if they don't explicitly ask for testing strategy guidance.
  Keywords: private method testing, internal testing, InternalsVisibleTo, 私有方法測試, 內部成員測試, 反射測試, reflection testing, GetMethod BindingFlags, Meziantou.MSBuild.InternalsVisibleTo, 可測試性設計, 策略模式重構, testability
---

# 私有與內部成員測試策略指南

本技能協助您在 .NET 測試中正確處理私有與內部成員的測試，強調設計優先的測試思維。

## 回覆策略

回覆時必須完整涵蓋以下三種路徑，讓使用者根據實際情境選擇最適合的方式：

1. **設計優先（重構）** — 將複雜的私有邏輯提取為獨立類別或策略模式，使其透過公開 API 可測試
2. **InternalsVisibleTo 設定** — 將 private 改為 internal，搭配 InternalsVisibleTo 開放給測試專案存取
3. **反射測試（過渡方案）** — 用反射直接測試私有方法，適合短期內難以重構的遺留系統

不要只推薦其中一種而忽略其他。即使首推重構，也必須說明 InternalsVisibleTo 和反射的做法與適用時機。

## 核心原則：設計優先思維

### 黃金法則

**好的設計自然就有好的可測試性。如果你發現自己經常需要測試私有方法，很可能是設計出了問題。** 但在實務上，並非所有情境都能立即重構，因此也需要了解 InternalsVisibleTo 和反射測試等替代方案。

### 設計問題的徵兆

當你想測試私有方法時，先檢查以下徵兆：

- 私有方法超過 10 行且包含複雜邏輯
- 私有方法包含重要的業務規則
- 私有方法難以透過公開方法間接測試
- 類別承擔多個職責

### 解決方案：重構而非測試

```csharp
// ❌ 有問題的設計
public class OrderProcessor
{
    public OrderResult ProcessOrder(Order order)
    {
        // 使用多個複雜的私有方法
        var discount = CalculateDiscount(order); // 20 行邏輯
        var tax = CalculateTax(order, discount);  // 15 行邏輯
        // ...
    }

    private decimal CalculateDiscount(Order order) { /* 複雜邏輯 */ }
    private decimal CalculateTax(Order order, decimal discount) { /* 複雜邏輯 */ }
}

// ✅ 改進的設計：責任分離
public class OrderProcessor
{
    private readonly IDiscountCalculator _discountCalculator;
    private readonly ITaxCalculator _taxCalculator;

    public OrderProcessor(
        IDiscountCalculator discountCalculator,
        ITaxCalculator taxCalculator)
    {
        _discountCalculator = discountCalculator;
        _taxCalculator = taxCalculator;
    }

    public OrderResult ProcessOrder(Order order)
    {
        var discount = _discountCalculator.Calculate(order);
        var tax = _taxCalculator.Calculate(order, discount);
        // ...
    }
}

// 現在可以獨立測試每個計算器
public class DiscountCalculator : IDiscountCalculator
{
    public decimal Calculate(Order order)
    {
        // 複雜邏輯現在是公開方法，容易測試
    }
}
```

## Internal 成員測試策略

### 何時需要測試 Internal 成員

**適合的情境：**

- 框架或類別庫開發
- 複雜的內部演算法驗證
- 效能關鍵的內部組件
- 安全相關的內部邏輯

**不適合的情境：**

- 應用層的業務邏輯（應該是 public）
- 簡單的輔助方法
- 可以透過公開 API 間接測試的邏輯

### 方法一：使用 InternalsVisibleTo 屬性

最直接的方法，適合簡單情況：

```csharp
// 在主專案中的 AssemblyInfo.cs 或任何類別檔案中
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("YourProject.Tests")]
[assembly: InternalsVisibleTo("YourProject.IntegrationTests")]
```

**優點：**

- 簡單直接
- 不需要額外套件

**缺點：**

- 需要硬編碼組件名稱
- 簽署組件時需要包含公鑰

### 方法二：在 csproj 中設定

透過 MSBuild 屬性設定：

```xml
<!-- YourProject.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <AssemblyAttribute Include="System.Runtime.CompilerServices.InternalsVisibleToAttribute">
      <_Parameter1>$(AssemblyName).Tests</_Parameter1>
    </AssemblyAttribute>
  </ItemGroup>
</Project>
```

**優點：**

- 可以使用 MSBuild 變數
- 集中管理

### 方法三：使用 Meziantou.MSBuild.InternalsVisibleTo（推薦）

對於複雜專案，推薦使用此 NuGet 套件：

```xml
<!-- YourProject.csproj -->
<ItemGroup>
  <PackageReference Include="Meziantou.MSBuild.InternalsVisibleTo" Version="1.0.2">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>

<ItemGroup>
  <InternalsVisibleTo Include="$(AssemblyName).Tests" />
  <InternalsVisibleTo Include="$(AssemblyName).IntegrationTests" />
  <InternalsVisibleTo Include="DynamicProxyGenAssembly2" Key="0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7" />
</ItemGroup>
```

**優點：**

- 自動處理簽署組件的公鑰
- 支援 DynamicProxyGenAssembly2（NSubstitute/Moq）
- 可讀性高

**參考資源：**

- [Declaring InternalsVisibleTo in the csproj - Meziantou's blog](https://www.meziantou.net/declaring-internalsvisibleto-in-the-csproj.htm)
- [GitHub - meziantou/Meziantou.MSBuild.InternalsVisibleTo](https://github.com/meziantou/Meziantou.MSBuild.InternalsVisibleTo)

### Internal 測試的風險評估

| 評估面向   | 風險程度 | 說明                             |
| :--------- | :------- | :------------------------------- |
| 封裝性破壞 | 中等     | 增加了測試對內部實作的依賴       |
| 重構阻力   | 高       | 改變 internal 成員會影響測試     |
| 維護成本   | 中等     | 需要同步維護生產代碼和測試代碼   |
| 設計品質   | 低       | 如果過度使用，可能表示設計有問題 |

## 私有方法測試技術

涵蓋決策樹（是否應測試私有方法）、反射測試私有實例方法與靜態方法、`ReflectionTestHelper` 輔助類別封裝，以及反射測試的風險與最佳實踐。

> 完整程式碼範例與技術細節請參考 [references/private-method-testing.md](references/private-method-testing.md)

## 測試友善的設計模式

### 策略模式改善可測試性

將複雜的私有邏輯重構為策略模式：

#### 重構前：難以測試的設計

```csharp
public class PricingService
{
    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;
        var discount = CalculateDiscount(customer, product); // 私有方法
        var tax = CalculateTax(product, customer.Location);   // 私有方法
        return basePrice - discount + tax;
    }

    private decimal CalculateDiscount(Customer customer, Product product)
    {
        // 20 行複雜的折扣計算邏輯
    }

    private decimal CalculateTax(Product product, Location location)
    {
        // 15 行複雜的稅率計算
    }
}
```

#### 重構後：使用策略模式

```csharp
// 策略介面
public interface IDiscountStrategy
{
    decimal Calculate(Customer customer, Product product);
}

public interface ITaxStrategy
{
    decimal Calculate(Product product, Location location);
}

// 具體策略實作
public class StandardDiscountStrategy : IDiscountStrategy
{
    public decimal Calculate(Customer customer, Product product)
    {
        // 折扣邏輯現在是公開方法，容易測試
        if (customer.IsVIP)
            return product.BasePrice * 0.1m;

        return 0;
    }
}

public class TaiwanTaxStrategy : ITaxStrategy
{
    public decimal Calculate(Product product, Location location)
    {
        // 稅率邏輯現在是公開方法，容易測試
        return product.BasePrice * 0.05m;
    }
}

// 改進的服務
public class PricingService
{
    private readonly IDiscountStrategy _discountStrategy;
    private readonly ITaxStrategy _taxStrategy;

    public PricingService(
        IDiscountStrategy discountStrategy,
        ITaxStrategy taxStrategy)
    {
        _discountStrategy = discountStrategy;
        _taxStrategy = taxStrategy;
    }

    public decimal CalculatePrice(Product product, Customer customer)
    {
        var basePrice = product.BasePrice;
        var discount = _discountStrategy.Calculate(customer, product);
        var tax = _taxStrategy.Calculate(product, customer.Location);
        return basePrice - discount + tax;
    }
}
```

**優點：**

- 每個策略可以獨立測試
- 符合開放封閉原則
- 易於擴展新的策略
- 減少對反射的依賴

### 部分模擬（Partial Mock）

有時需要模擬類別的部分行為：

```csharp
// 需要部分模擬的類別
public class DataProcessor
{
    public ProcessResult Process(string input)
    {
        var validated = ValidateInput(input);
        if (!validated)
            return ProcessResult.InvalidInput();

        var data = TransformData(input);
        var saved = SaveData(data); // 想模擬這個方法避免實際資料庫操作

        return saved
            ? ProcessResult.Success()
            : ProcessResult.Failed();
    }

    protected virtual bool SaveData(string data)
    {
        // 實際的資料庫操作
        return true;
    }

    private bool ValidateInput(string input) => !string.IsNullOrEmpty(input);
    private string TransformData(string input) => input.ToUpper();
}

// 測試用的子類別
public class TestableDataProcessor : DataProcessor
{
    protected override bool SaveData(string data)
    {
        // 模擬實作，避免實際資料庫操作
        return true;
    }
}

// 測試
[Fact]
public void Process_使用部分模擬_應成功處理()
{
    // Arrange
    var processor = new TestableDataProcessor();

    // Act
    var result = processor.Process("test");

    // Assert
    result.Success.Should().BeTrue();
}
```

## 實務決策框架

### 三層次風險評估法

#### 第一層：設計品質評估

**問題：這是設計問題還是測試問題？**

- 私有方法是否過於複雜？（> 10 行）
- 類別是否承擔多個職責？
- 是否可以提取為獨立類別？

**建議行動：**

- 優先考慮重構（提取類別、策略模式）
- 透過改善設計來改善可測試性

#### 第二層：維護成本評估

**問題：測試是否會成為重構的阻礙？**

- 測試是否依賴實作細節？
- 重構時測試是否需要大量修改？
- 測試失敗時是否難以定位問題？

**建議行動：**

- 如果維護成本高，重新考慮測試策略
- 考慮透過公開 API 的整合測試

#### 第三層：價值產出評估

**問題：測試帶來的價值是否超過成本？**

評估測試價值：

- 能抓到真實的業務邏輯錯誤
- 提供清楚的失敗訊息
- 在合理成本下長期穩定運行

**建議行動：**

- 如果價值不足，尋找替代測試策略
- 考慮效能測試、整合測試等其他方式

### 決策矩陣

| 情境                    | 建議做法                | 理由                 |
| :---------------------- | :---------------------- | :------------------- |
| 簡單私有方法（< 10 行） | 透過公開方法測試        | 維護成本低           |
| 複雜私有邏輯（> 10 行） | 重構為獨立類別          | 改善設計與可測試性   |
| 框架內部演算法          | 使用 InternalsVisibleTo | 需要精確測試內部行為 |
| 遺留系統私有方法        | 考慮使用反射測試        | 短期內難以重構       |
| 安全相關私有邏輯        | 重構或使用反射測試      | 需要獨立驗證正確性   |
| 頻繁變動的實作細節      | 避免直接測試            | 測試會變得脆弱       |

## 最佳實踐

1. **設計優先** — 需要測試私有方法通常意味著類別承擔了過多責任。優先考慮重構（依賴注入、策略模式、責任分離），讓邏輯透過公開 API 自然可測。

2. **測試公開行為** — 專注測試公開 API 的行為，透過公開方法間接覆蓋私有邏輯。這讓測試更穩定，不會因重構內部實作而失敗。

3. **明智使用 InternalsVisibleTo** — 僅用於框架或類別庫開發場景，因為這類程式碼的 internal 成員確實需要獨立驗證。使用 Meziantou.MSBuild.InternalsVisibleTo 簡化設定，並記錄開放的理由。

4. **謹慎使用反射** — 反射是最後手段，建立輔助方法封裝反射邏輯以降低維護成本，並在測試名稱中標示使用反射以提醒團隊。

## 常見誤區

1. **過度測試私有方法** — 為每個私有方法寫測試會讓測試套件變得脆弱，重構時大量測試會失敗。簡單的 getter/setter 和委派呼叫不需要獨立測試。

2. **忽略設計問題** — 用測試技巧繞過封裝（如大量使用反射）只是治標不治本，根本問題在於類別設計需要改善。測試應推動更好的設計，而非成為設計問題的遮蓋。

3. **依賴實作細節** — 測試私有方法的呼叫順序或驗證私有欄位的值，會讓測試與實作緊密耦合，任何內部重構都會導致測試失敗。

4. **濫用 InternalsVisibleTo** — 為應用層程式碼開放 internal 會破壞封裝邊界，讓測試依賴不該公開的內部結構。

## 範例參考

參考 `templates/` 目錄下的完整範例：

- `internals-visible-to-examples.cs` - InternalsVisibleTo 設定範例
- `reflection-testing-examples.cs` - 反射測試技術範例
- `strategy-pattern-refactoring.cs` - 策略模式重構範例

## 輸出格式

回覆必須包含以下三個區塊（依優先順序排列，但全部都要提及）：

1. **重構建議** — 說明如何透過設計改善（策略模式、責任分離）讓邏輯可測試，附程式碼範例
2. **InternalsVisibleTo 方案** — 說明如何設定 InternalsVisibleTo（含 AssemblyInfo.cs、csproj、Meziantou 三種方式），附設定範例
3. **反射測試方案** — 說明如何用反射測試私有方法（作為過渡方案），附程式碼範例

其他輸出要素：
- 產生測試類別檔案（`*Tests.cs`），包含私有或內部成員的測試方法
- 提供決策建議說明，標示選擇的測試策略與理由

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 09 - 測試私有與內部成員：Private 與 Internal 的測試策略**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10374866
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day09

### 官方文件

- [Meziantou's Blog - InternalsVisibleTo](https://www.meziantou.net/declaring-internalsvisibleto-in-the-csproj.htm)

### 相關技能

- `unit-test-fundamentals` - 單元測試基礎
- `nsubstitute-mocking` - 測試替身與模擬

## 測試清單

在處理私有與內部成員測試時，確認以下檢查項目：

- [ ] 已評估是否應該重構而非測試私有方法
- [ ] Internal 成員確實需要開放給測試專案
- [ ] 使用適當的 InternalsVisibleTo 設定方法
- [ ] 反射測試已使用輔助方法封裝
- [ ] 測試名稱清楚標示測試類型（如使用反射）
- [ ] 策略模式等設計模式已考慮用於複雜邏輯
- [ ] 測試不會成為重構的阻礙
- [ ] 測試提供的價值超過維護成本
- [ ] 沒有過度依賴實作細節
- [ ] 定期檢視測試策略的適切性
