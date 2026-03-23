---
name: dotnet-testing-autofixture-bogus-integration
description: |
  AutoFixture 與 Bogus 整合完整指南。當需要結合 AutoFixture 與 Bogus 產生兼具匿名性與真實感的測試資料時使用。涵蓋 SpecimenBuilder 整合、混合產生器、測試資料工廠與循環參考處理。
  Make sure to use this skill whenever the user mentions AutoFixture with Bogus, Faker integration, realistic test data, semantic test data, or HybridTestDataGenerator, even if they don't explicitly ask for integration guidance.
  Keywords: autofixture bogus integration, autofixture bogus, bogus integration, Faker, EmailSpecimenBuilder, PhoneSpecimenBuilder, NameSpecimenBuilder, 真實感測試資料, 語意化資料, 混合產生器, HybridTestDataGenerator, OmitOnRecursionBehavior, 循環參考
---

# AutoFixture 與 Bogus 整合應用指南

## 核心概念

### 為什麼需要整合？

| 面向           | AutoFixture              | Bogus                    | 整合方案         |
| -------------- | ------------------------ | ------------------------ | ---------------- |
| 資料真實感     | 低（GUID 格式字串）      | 高（真實 Email/Phone）   | 高               |
| 物件關聯處理   | 自動                     | 手動                     | 自動             |
| 循環參考處理   | 內建                     | 無                       | 整合             |
| 設定複雜度     | 低                       | 中                       | 中               |
| 適用場景       | 單元測試                 | 整合測試/原型            | 兩者皆可         |

**整合效果：** `user.Email` 從 `"Email1a2b3c4d"` 變為 `"john.doe@example.com"`，其他屬性仍由 AutoFixture 自動填充。

## 套件安裝

```xml
<PackageReference Include="AutoFixture" Version="4.18.1" />
<PackageReference Include="AutoFixture.Xunit2" Version="4.18.1" />
<PackageReference Include="Bogus" Version="35.6.5" />
<PackageReference Include="xunit" Version="2.9.3" />
<PackageReference Include="AwesomeAssertions" Version="9.4.0" />
```

## 整合方式總覽

| 整合方式                     | 適用場景           | 複雜度 |
| ---------------------------- | ------------------ | ------ |
| 屬性層級 SpecimenBuilder     | 特定屬性使用 Bogus | 低     |
| 類型層級 SpecimenBuilder     | 整個類型使用 Bogus | 中     |
| 混合產生器 (HybridGenerator) | 統一 API 整合      | 中     |
| 整合工廠 (IntegratedFactory) | 完整測試場景建構   | 高     |
| 自訂 AutoData 屬性           | xUnit 整合         | 低     |

## 核心整合技術

### ISpecimenBuilder 整合

透過 `ISpecimenBuilder` 介面實現屬性層級與類型層級的整合：

- **屬性層級**：根據屬性名稱判斷（如 `Email`、`Phone`、`FirstName`），使用 Bogus 產生對應的真實感資料
- **類型層級**：為整個類型（如 `User`、`Address`）註冊 Bogus Faker 產生器

### 常用 SpecimenBuilder

| SpecimenBuilder          | 產生資料類型       | Bogus API               |
| ------------------------ | ------------------ | ------------------------ |
| `EmailSpecimenBuilder`   | Email 地址         | `f.Internet.Email()`     |
| `PhoneSpecimenBuilder`   | 電話號碼           | `f.Phone.PhoneNumber()`  |
| `NameSpecimenBuilder`    | 人名               | `f.Name.FirstName()`     |
| `AddressSpecimenBuilder` | 地址               | `f.Address.FullAddress()`|

### 擴充方法

- `fixture.WithBogus()` — 註冊所有 Bogus SpecimenBuilder
- `fixture.WithOmitOnRecursion()` — 處理循環參考
- `fixture.WithSeed(seed)` — 設定隨機種子
- `fixture.WithRepeatCount(count)` — 設定 CreateMany 預設數量

> 完整內容請參閱 [references/core-integration-techniques.md](references/core-integration-techniques.md)

## 循環參考處理

當物件存在循環參考（如 User → Company → Employees(User)）時，使用 `OmitOnRecursionBehavior` 解決：

```csharp
var fixture = new Fixture();
fixture.Behaviors.OfType<ThrowingRecursionBehavior>()
    .ToList()
    .ForEach(b => fixture.Behaviors.Remove(b));
fixture.Behaviors.Add(new OmitOnRecursionBehavior());
```

**效果：** 避免 StackOverflowException，循環參考的屬性設為 null 或空集合。

## 自訂 AutoData 屬性

```csharp
public class BogusAutoDataAttribute : AutoDataAttribute
{
    public BogusAutoDataAttribute()
        : base(() => new Fixture().WithBogus())
    {
    }
}

// 使用方式
[Theory]
[BogusAutoData]
public void 使用整合資料測試(User user, Address address)
{
    user.Email.Should().Contain("@");
    address.City.Should().NotBeNullOrEmpty();
}
```

## 混合產生器與測試資料工廠

### HybridTestDataGenerator

統一的測試資料產生 API，實作 `ITestDataGenerator` 介面：

- `Generate<T>()` — 產生單一物件
- `Generate<T>(int count)` — 產生指定數量的集合
- `Generate<T>(Action<T> configure)` — 產生物件後進行自訂設定

### IntegratedTestDataFactory

完整場景建構工廠，支援進階功能：

- `CreateFresh<T>()` — 每次產生全新物件
- `CreateMany<T>(count)` — 批次建立
- `GetCached<T>()` — 快取機制，相同類型只產生一次
- `CreateTestScenario()` — 建立包含 Company、Users、Orders 的完整測試場景，自動建立關聯

### TestBase 基底類別

統一 Fixture、Generator、Factory 的初始化與 Seed 管理，提供 `Create<T>()`、`CreateMany<T>()`、`Create<T>(configure)` 等便捷方法。

> 完整程式碼範例請參考 [references/hybrid-generator-and-factory.md](references/hybrid-generator-and-factory.md)

## Seed 管理與可重現性

### 重要限制

由於 AutoFixture 和 Bogus 有不同的隨機數管理機制：

- Seed 確保測試行為穩定性
- Seed 確保資料格式一致性
- 無法保證所有屬性值完全相同

### 建議做法

- **一般場景**：使用 `IntegratedTestDataFactory(seed: 12345)` 確保穩定性
- **完全可重現**：使用單一工具，如 `Faker<User>().UseSeed(12345)`
- **不需重現**：不設定 Seed，每次產生不同的隨機資料

## 常見問題

### Q1: 什麼時候該用整合方案，什麼時候用單一工具？

- **用整合方案**：需要真實感資料（Email、Phone）且物件結構複雜的測試場景
- **用純 AutoFixture**：只需要匿名資料填充的單元測試
- **用純 Bogus**：需要完全可重現且資料格式嚴格的整合測試

### Q2: SpecimenBuilder 匹配優先順序？

AutoFixture 按照 `Customizations` 集合的順序匹配，先加入的 Builder 優先。使用 `fixture.Customizations.Insert(0, builder)` 可確保最高優先。

### Q3: 如何為新的實體類型加入 Bogus 支援？

1. 建立對應的 `ISpecimenBuilder` 實作
2. 在 `Create()` 方法中判斷屬性名稱或類型
3. 使用 Bogus Faker 產生對應的真實感資料
4. 在 `WithBogus()` 擴充方法中註冊

## 最佳實踐

### 建議做法

1. **永遠先處理循環參考** — `fixture.WithOmitOnRecursion().WithBogus()`
2. **為常用實體建立專用 SpecimenBuilder**
3. **使用 Seed 確保測試穩定性**
4. **建立測試基底類別統一資料產生邏輯**
5. **適當使用快取提升效能**

### 避免事項

1. 過度設計，保持簡單實用
2. 期望整合環境完全可重現
3. 忽略循環參考處理
4. 在每個測試中重新建立 Fixture

## 輸出格式

- 產生 `ISpecimenBuilder` 實作類別檔案（如 `EmailSpecimenBuilder.cs`）
- 產生 Fixture 擴充方法類別檔案（`FixtureBogusExtensions.cs`）
- 產生整合測試資料工廠或混合產生器類別
- 產生 `BogusAutoDataAttribute` 供 xUnit Theory 測試使用
- 搭配 `OmitOnRecursionBehavior` 處理循環參考

## 參考資源

### 原始文章

- **Day 15 - AutoFixture 與 Bogus 整合：結合兩者優勢**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10375620
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day15

### 官方文件

- [AutoFixture GitHub](https://github.com/AutoFixture/AutoFixture)
- [Bogus GitHub Repository](https://github.com/bchavez/Bogus)

### 相關技能

- [autofixture-basics](../autofixture-basics/) - AutoFixture 基礎使用
- [autofixture-customization](../autofixture-customization/) - AutoFixture 自訂化策略
- [autodata-xunit-integration](../autodata-xunit-integration/) - AutoData 屬性整合
- [bogus-fake-data](../bogus-fake-data/) - Bogus 假資料產生器
