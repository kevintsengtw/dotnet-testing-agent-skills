---
name: dotnet-testing-autofixture-customization
description: |
  AutoFixture 進階自訂化技術完整指南。當需要自訂 AutoFixture 建構器或處理特殊型別的測試資料產生規則時使用。涵蓋 DataAnnotations 自動整合、ISpecimenBuilder 實作、優先順序管理。包含 DateTime/數值範圍建構器、泛型化設計與流暢式擴充方法。
  Make sure to use this skill whenever the user mentions AutoFixture customization, ISpecimenBuilder, DataAnnotations with AutoFixture, or custom builders for test data generation, even if they don't explicitly ask for customization guidance.
  Keywords: autofixture customization, autofixture customize, autofixture 自訂, specimen builder, ISpecimenBuilder, RandomDateTimeSequenceGenerator, NumericRangeBuilder, DataAnnotations autofixture, fixture.Customizations, Insert(0), 自訂建構器, NoSpecimen, 泛型化建構器
---

# AutoFixture 進階：自訂化測試資料生成策略

## 概述

本技能涵蓋 AutoFixture 的進階自訂化功能，讓您能根據業務需求精確控制測試資料的生成邏輯。

### 核心技術

1. **DataAnnotations 整合**：AutoFixture 自動識別 `[StringLength]`、`[Range]` 等驗證屬性
2. **屬性範圍控制**：使用 `.With()` 配合 `Random.Shared` 動態產生隨機值
3. **自訂 ISpecimenBuilder**：實作精確控制特定屬性的建構器
4. **優先順序管理**：理解 `Insert(0)` vs `Add()` 的差異
5. **泛型化設計**：建立支援多種數值型別的可重用建構器

## 安裝套件

```xml
<PackageReference Include="AutoFixture" Version="4.18.1" />
<PackageReference Include="AutoFixture.Xunit2" Version="4.18.1" />
```

## DataAnnotations 自動整合

AutoFixture 能自動識別 `System.ComponentModel.DataAnnotations` 的驗證屬性：

```csharp
public class Person
{
    public Guid Id { get; set; }
    [StringLength(10)]
    public string Name { get; set; } = string.Empty;
    [Range(10, 80)]
    public int Age { get; set; }
    public DateTime CreateTime { get; set; }
}

[Fact]
public void AutoFixture_應能識別DataAnnotations()
{
    var fixture = new Fixture();
    var person = fixture.Create<Person>();
    person.Name.Length.Should().Be(10);        // StringLength(10)
    person.Age.Should().BeInRange(10, 80);     // Range(10, 80)
}
```

## 使用 .With() 控制屬性範圍

### 固定值 vs 動態值

```csharp
// ❌ 固定值：只執行一次，所有物件相同值
.With(x => x.Age, Random.Shared.Next(30, 50))

// ✅ 動態值：每個物件都重新計算
.With(x => x.Age, () => Random.Shared.Next(30, 50))
```

### Random.Shared 的優點

| 特性       | `new Random()`             | `Random.Shared`      |
| ---------- | -------------------------- | -------------------- |
| 實例化方式 | 每次建立新實例             | 全域共用單一實例     |
| 執行緒安全 | 不是                    | 是                |
| 效能       | 多次建立有負擔，可能重複值 | 效能更佳，避免重複值 |

## 自訂 ISpecimenBuilder

涵蓋 RandomRangedDateTimeBuilder（精確控制特定 DateTime 屬性）、ImprovedRandomRangedNumericSequenceBuilder（改進版數值範圍建構器）、泛型化 NumericRangeBuilder&lt;TValue&gt;（支援 int/long/decimal/double/float 等多種型別），以及流暢介面擴充方法 AddRandomRange。每個建構器皆附完整實作與使用範例。

> 完整 ISpecimenBuilder 實作範例請參考 [references/specimen-builder-examples.md](references/specimen-builder-examples.md)

## 優先順序管理：Insert(0) vs Add()

AutoFixture 內建的 `RangeAttributeRelay`、`NumericSequenceGenerator` 可能比自訂建構器有更高優先順序：

```csharp
// ❌ 可能失效：被內建建構器攔截
fixture.Customizations.Add(new MyNumericBuilder(30, 50, "Age"));

// ✅ 正確：確保最高優先順序
fixture.Customizations.Insert(0, new MyNumericBuilder(30, 50, "Age"));
```

### int vs DateTime 處理差異

| 型別       | 內建建構器                                        | 優先順序影響               |
| ---------- | ------------------------------------------------- | -------------------------- |
| `int`      | `RangeAttributeRelay`、`NumericSequenceGenerator` | 會被攔截，需用 `Insert(0)` |
| `DateTime` | 無特定建構器                                      | 不會被攔截，`Add()` 即可   |

## 最佳實踐

### 應該做

1. **善用 DataAnnotations** — 充分利用現有模型驗證規則，AutoFixture 自動產生符合限制的資料
2. **使用 Random.Shared** — 避免重複值問題，執行緒安全、效能更好
3. **Insert(0) 確保優先順序** — 自訂數值建構器務必用 `Insert(0)`
4. **泛型化設計** — 建立可重用的泛型建構器，使用擴充方法提供流暢介面

### 應該避免

1. **忽略建構器優先順序** — 不要假設 `Add()` 一定生效
2. **過度複雜的邏輯** — 建構器保持單一職責
3. **使用 new Random()** — 可能產生重複值，非執行緒安全

## 程式碼範本

請參考 [templates](./templates) 資料夾中的範例檔案：

- [dataannotations-integration.cs](./templates/dataannotations-integration.cs) - DataAnnotations 自動整合
- [custom-specimen-builders.cs](./templates/custom-specimen-builders.cs) - 自訂 ISpecimenBuilder 實作
- [numeric-range-extensions.cs](./templates/numeric-range-extensions.cs) - 泛型化數值範圍建構器與擴充方法

## 與其他技能的關係

- **autofixture-basics**：本技能的前置知識，需先掌握基礎用法
- **autodata-xunit-integration**：下一步學習目標，將自訂化與 xUnit 整合
- **autofixture-nsubstitute-integration**：進階整合，結合 Mock 與自訂資料生成

## 輸出格式

- 產生自訂 ISpecimenBuilder 實作類別
- 產生 ICustomization 組合類別
- 提供 fixture.Customizations.Insert(0, ...) 設定範例
- 包含 DataAnnotations 整合與泛型化建構器程式碼

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 11 - AutoFixture 進階：自訂化測試資料生成策略**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10375153
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day11

### 官方文件

- [AutoFixture GitHub](https://github.com/AutoFixture/AutoFixture)
- [AutoFixture 官方文件](https://autofixture.github.io/)
- [ISpecimenBuilder 介面](https://autofixture.github.io/docs/fixture-customization/)
