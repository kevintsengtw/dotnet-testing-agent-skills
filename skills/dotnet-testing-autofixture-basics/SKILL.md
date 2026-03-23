---
name: dotnet-testing-autofixture-basics
description: |
  使用 AutoFixture 自動產生測試資料的基礎技能。當需要快速產生測試物件、減少樣板程式碼、實現匿名測試時使用。涵蓋 Fixture.Create、CreateMany、循環參考處理、與 xUnit 整合等。
  Make sure to use this skill whenever the user mentions AutoFixture, fixture.Create, anonymous testing, auto-generating test data, or reducing test boilerplate, even if they don't explicitly ask for AutoFixture guidance.
  Keywords: autofixture, fixture, 自動產生測試資料, test data generation, anonymous testing, 匿名測試, fixture.Create, CreateMany, fixture.Build, Create<T>, AutoFixture.Xunit2, OmitOnRecursionBehavior, IFixture, 產生測試資料, generate test data
---

# AutoFixture 基礎：自動產生測試資料

## 安裝套件

```xml
<PackageReference Include="AutoFixture" Version="4.18.1" />
<PackageReference Include="AutoFixture.Xunit2" Version="4.18.1" />
```

或透過命令列安裝：

```powershell
dotnet add package AutoFixture
dotnet add package AutoFixture.Xunit2
```

## 核心 API 摘要

| API                      | 用途                         | 預設行為              |
| ------------------------ | ---------------------------- | --------------------- |
| `fixture.Create<T>()`   | 產生單一物件                 | 自動填充所有屬性      |
| `fixture.CreateMany<T>()` | 產生集合                   | 預設 3 個元素         |
| `fixture.Build<T>()`    | 精確控制屬性                 | 搭配 With/Without     |
| `OmitAutoProperties()`  | 只設定必要屬性               | 其餘保持預設值        |

### 基本使用

`Fixture` 是核心類別，提供自動產生測試資料的能力：

- **基本型別**：`int`（隨機正整數）、`string`（GUID 格式）、`decimal`、`bool`、`DateTime`、`Guid`
- **複雜物件**：自動建構巢狀物件與集合屬性，所有層級的屬性都會自動填入值
- **集合產生**：`CreateMany<T>()` 預設產生 3 個元素，可指定數量如 `CreateMany<T>(10)`

> 完整程式碼範例請參考 [references/basic-usage-examples.md](references/basic-usage-examples.md)

### Build<T>() 精確控制

當需要對特定屬性進行控制時，使用 `Build<T>()` 模式：

| 方法                    | 用途                         | 範例                                       |
| ----------------------- | ---------------------------- | ------------------------------------------ |
| `.With(prop, value)`    | 指定固定值                   | `.With(x => x.Name, "測試客戶")`           |
| `.With(prop, factory)`  | 使用工廠方法產生值           | `.With(x => x.Price, () => random.Next())` |
| `.Without(prop)`        | 排除屬性，保持預設值         | `.Without(x => x.InternalId)`              |
| `.OmitAutoProperties()` | 不自動設定任何屬性          | 只設定關心的屬性                           |

### 循環參考處理

當物件包含循環參考時（如 Category 的 Parent 屬性指向自己），AutoFixture 預設會拋出 `ObjectCreationException`。

**解決方案：** 使用 `OmitOnRecursionBehavior`，移除預設的 `ThrowingRecursionBehavior` 並加入忽略循環參考的行為。

**建議：** 建立 `AutoFixtureTestBase` 基底類別統一處理循環參考，避免在每個測試中重複設定。

> 完整程式碼範例請參考 [references/build-and-customization.md](references/build-and-customization.md)

## xUnit 整合

### Fixture 共享客製化

在測試類別建構函式中使用 `Customize<T>()` 設定共用的資料產生規則，所有測試方法共享同一個 Fixture 設定。

### 結合 Theory 測試

搭配 `[Theory]` + `[InlineData]`，使用 `Build<T>().With()` 針對不同輸入情境設定關鍵屬性值，驗證各情境的預期行為。

### 匿名測試原則

測試應該關注「行為」而不是「資料」：

- **好的做法：** 使用 `fixture.Create<T>()` 產生任意有效資料，驗證操作結果
- **避免：** 依賴隨機產生值的具體內容（如假設年齡大於 18）
- **正確：** 使用 `Build<T>().With()` 明確設定影響測試結果的關鍵值

> 完整程式碼範例請參考 [references/xunit-integration-examples.md](references/xunit-integration-examples.md)

## 進化比較：Test Data Builder vs AutoFixture

| 層面       | Test Data Builder      | AutoFixture        |
| ---------- | ---------------------- | ------------------ |
| 程式碼行數 | 40+ 行 Builder + 測試  | 5 行測試           |
| 維護成本   | 物件改變需更新 Builder | 自動適應變化       |
| 開發時間   | 先寫 Builder 再寫測試  | 直接寫測試         |
| 大量資料   | 需要迴圈               | `CreateMany(100)`  |
| 可讀性     | 業務語意明確           | 需理解 AutoFixture |

> 完整比較程式碼與混合策略請參考 [references/xunit-integration-examples.md](references/xunit-integration-examples.md)

## 混合策略建議

結合 Test Data Builder 與 AutoFixture 的優點：

- 使用 AutoFixture 建立基礎資料，再用 Builder 加工
- 使用 `TestDataFactory` 封裝常用的資料產生模式
- 大量隨機資料產生使用 `CreateMany<T>(count)`

> 完整比較程式碼與混合策略請參考 [references/xunit-integration-examples.md](references/xunit-integration-examples.md)

## 實務應用場景

AutoFixture 在實務中常用於以下場景：

- **Entity 測試**：搭配 Theory 驗證不同輸入場景
- **DTO 驗證**：使用 `Build<T>()` 產生符合驗證規則的資料
- **大量資料測試**：使用 `CreateMany()` 產生批次資料
- **服務層測試**：搭配 NSubstitute 自動產生 Mock 參數

> 完整程式碼範例請參閱 [references/practical-scenarios.md](references/practical-scenarios.md)

## 常見問題

### Q1: 何時該用 AutoFixture，何時該用固定值？

- **用 AutoFixture**：不影響測試結果的「填充」資料，如使用者的 Email、地址等
- **用固定值**：影響測試邏輯的「關鍵」資料，如折扣計算中的客戶類型、年齡驗證中的年齡值

### Q2: CreateMany 預設產生幾個？如何調整？

預設產生 3 個元素，可透過參數指定數量：`fixture.CreateMany<T>(10)`。若要全域調整，使用 `fixture.RepeatCount = 5`。

### Q3: AutoFixture 產生的字串格式不符合需求？

使用 `Build<T>().With()` 指定格式，或搭配 `Customize<T>()` 設定全域規則。若需要真實感資料，考慮整合 Bogus。

## 最佳實踐

### 應該做

1. **使用匿名測試概念** — 專注於測試邏輯而非具體資料
2. **只在必要時固定特定值** — 使用 `Build<T>().With()` 設定關鍵屬性
3. **建立共用基底類別** — 統一處理循環參考等共同配置
4. **合理的集合大小** — 根據測試目的調整 `CreateMany()` 數量

### 應該避免

1. **過度依賴隨機值** — 不要假設隨機值的具體內容
2. **忽略邊界值** — 仍需要明確測試邊界情況
3. **濫用自動產生** — 簡單測試可能用固定值更清楚

## 程式碼範本

請參考 [templates](./templates) 資料夾中的範例檔案：

- [basic-autofixture-usage.cs](./templates/basic-autofixture-usage.cs) - 基本使用方式
- [complex-object-scenarios.cs](./templates/complex-object-scenarios.cs) - 複雜物件與循環參考
- [xunit-integration.cs](./templates/xunit-integration.cs) - xUnit 整合與 Theory 測試

## 輸出格式

- 產生使用 AutoFixture 的測試類別（.cs 檔案）
- 包含 Fixture.Create/CreateMany/Build 用法範例
- 提供 .csproj 套件參考（AutoFixture、AutoFixture.Xunit2）
- 包含循環參考處理與自訂行為設定

## 參考資源

### 原始文章

- **Day 10 - AutoFixture 基礎：自動產生測試資料**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10375018
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day10

### 官方文件

- [AutoFixture GitHub](https://github.com/AutoFixture/AutoFixture)
- [AutoFixture 官方網站](https://autofixture.github.io/)
- [AutoFixture 快速入門](https://autofixture.github.io/docs/quick-start/)

### 相關技能

- `dotnet-testing-autofixture-customization` - AutoFixture 進階自訂
- `dotnet-testing-autofixture-bogus-integration` - AutoFixture + Bogus 整合
- `dotnet-testing-autofixture-nsubstitute-integration` - AutoFixture + NSubstitute 整合
- `dotnet-testing-autodata-xunit-integration` - AutoData 與 xUnit 整合
- `dotnet-testing-test-data-builder-pattern` - Test Data Builder Pattern
