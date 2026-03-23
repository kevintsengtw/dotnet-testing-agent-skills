---
name: dotnet-testing-test-data-builder-pattern
description: |
  Test Data Builder Pattern 完整實作指南。當需要使用建構者模式建立可維護的測試資料或簡化複雜物件的測試準備時使用。涵蓋流暢介面、語意化方法、預設值設計與 Builder 組合模式。
  Make sure to use this skill whenever the user mentions test data builder, builder pattern, fluent interface, Object Mother, or test data preparation for complex objects, even if they don't explicitly ask for the builder pattern.
  Keywords: test data builder, builder pattern test, 測試資料建構器, object mother, fluent interface, 流暢介面, UserBuilder, ProductBuilder, .With(), .Build(), AUser(), 測試資料準備, 複雜物件建立, 語意化測試
---

# Test Data Builder Pattern 測試資料建構器模式

## 核心概念

### 什麼是 Test Data Builder Pattern？

Test Data Builder Pattern 是 Object Mother Pattern 的改良版，主要解決以下問題：

1. **固定測試資料的問題**：Object Mother 提供固定的測試物件，難以針對特定測試情境調整
2. **測試意圖不明確**：直接建立物件時，測試的關注點容易被大量屬性設定所掩蓋
3. **重複程式碼**：相似的物件建立邏輯在多個測試中重複出現

### 為何需要 Builder Pattern？

```csharp
// ❌ 問題：過多參數設定，測試意圖不明確
var user = new User
{
    Name = "John Doe", Email = "john@example.com", Age = 30,
    Roles = new[] { "User" }, Settings = new UserSettings { Theme = "Dark", Language = "zh-TW" },
    IsActive = true, CreatedAt = DateTime.Now, ModifiedAt = DateTime.Now
};

// ✅ 改善：意圖明確，只設定測試關注的屬性
var user = UserBuilder.AUser().WithName("John Doe").WithValidEmail().Build();
```

## 實作指南

### 基本 Builder 結構

一個標準的 Test Data Builder 應包含：

1. **預設值**：為所有必要屬性提供合理的預設值
2. **流暢介面**：使用 `With*` 方法鏈來設定屬性
3. **語意化方法**：提供有意義的預設建立者（如 `AnAdminUser()`、`ARegularUser()`）
4. **Build 方法**：最終建立並回傳目標物件

涵蓋完整 UserBuilder 實作（預設值、With* 流暢方法、語意化靜態工廠方法、Build 方法）、單一測試情境與配合 Theory 使用的範例。

> 完整 Builder 實作與測試使用範例請參考 [references/builder-implementation.md](references/builder-implementation.md)

---

## 最佳實踐

涵蓋五項實踐：提供合理的預設值、使用語意化命名（UserScenarios）、Builder 之間的組合（OrderBuilder 組合 UserBuilder + ProductBuilder）、避免過度複雜化、統一管理測試資料（TestData 靜態類別）。

> 完整最佳實踐與進階模式請參考 [references/best-practices-and-patterns.md](references/best-practices-and-patterns.md)

---

## 與其他模式的比較

### Test Data Builder vs. Object Mother

| 特性     | Test Data Builder           | Object Mother         |
| -------- | --------------------------- | --------------------- |
| 彈性     | 高度彈性，可針對測試調整 | 固定的測試資料     |
| 可讀性   | 流暢介面，意圖明確       | 需要查看方法實作 |
| 維護性   | 集中管理，易於修改       | 變更影響所有測試   |
| 使用場景 | 單元測試、情境測試          | 簡單的整合測試        |

### Test Data Builder vs. AutoFixture

| 特性       | Test Data Builder       | AutoFixture               |
| ---------- | ----------------------- | ------------------------- |
| 控制度     | 完全控制物件建立     | 自動產生，控制度較低 |
| 設定複雜度 | 需手動建立 Builder | 幾乎零設定             |
| 測試意圖   | 非常明確             | 需額外說明           |
| 適用時機   | 需要精確控制的測試      | 大量資料產生、匿名測試    |

> **建議**：Test Data Builder 和 AutoFixture 可以相輔相成。簡單情境使用 AutoFixture，複雜情境或需明確意圖時使用 Builder Pattern。

## 實戰範例

請參考 `templates/` 目錄下的完整實作範例：

- `user-builder-example.cs` - 基本 User Builder 實作
- `advanced-builder-scenarios.cs` - 進階 Builder 組合與使用情境
- `builder-with-theory.cs` - Builder 配合 xUnit Theory 的實務範例

## 輸出格式

- 產生 Builder 類別檔案（如 `UserBuilder.cs`、`ProductBuilder.cs`），放置於測試專案的 `Builders/` 目錄
- 每個 Builder 包含預設值、`With*` 流暢方法、語意化靜態工廠方法與 `Build()` 方法
- 若需要跨測試共用，產生 `TestData.cs` 靜態類別集中管理常用測試資料
- 產生對應的測試類別檔案（`*Tests.cs`），示範 Builder 的使用方式

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 10 - AutoFixture 基礎：自動產生測試資料** (Builder Pattern 概念)
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10375018
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day10

### 延伸閱讀

- **Test Data Builder 原始文章**：[Test Data Builders: an alternative to the Object Mother pattern](http://www.natpryce.com/articles/000714.html) by Nat Pryce
- **Builder Pattern vs Object Mother**：[Test Data Builders and Object Mother: another look](https://www.javacodegeeks.com/2014/06/test-data-builders-and-object-mother-another-look.html)

### 相關技能

- `autofixture-basics` - 使用 AutoFixture 自動產生測試資料
- `xunit-project-setup` - xUnit 測試專案的基礎設定
- `test-naming-conventions` - 測試命名規範

## 總結

Test Data Builder Pattern 是撰寫可維護測試的重要技巧：

- **使用時機**：測試物件有多個屬性、需重複使用測試資料、希望表達清晰意圖
- **核心優勢**：提升可讀性、降低維護成本、增強表達力
- **注意事項**：保持 Builder 簡單、提供合理預設值、使用語意化方法名稱
