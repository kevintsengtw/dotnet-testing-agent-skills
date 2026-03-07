---
name: dotnet-testing-awesome-assertions-guide
description: |
  使用 AwesomeAssertions 進行流暢且可讀的測試斷言技能。當需要撰寫清晰的斷言、比對物件、驗證集合、處理複雜比對時使用。涵蓋 Should()、BeEquivalentTo()、Contain()、ThrowAsync() 等完整 API。
  Make sure to use this skill whenever the user mentions assertions, Should(), BeEquivalentTo, fluent assertions, AwesomeAssertions, or wants more readable test assertions, even if they don't explicitly ask for assertion guidance.
  Keywords: assertions, awesome assertions, fluent assertions, 斷言, 流暢斷言, Should(), Be(), BeEquivalentTo, Contain, ThrowAsync, NotBeNull, 物件比對, 集合驗證, 例外斷言, AwesomeAssertions, FluentAssertions, fluent syntax
---

# AwesomeAssertions 流暢斷言指南

本技能提供使用 AwesomeAssertions 進行高品質測試斷言的完整指南，涵蓋基礎語法、進階技巧與最佳實踐。

## 關於 AwesomeAssertions

**AwesomeAssertions** 是 FluentAssertions 的社群分支版本，使用 **Apache 2.0** 授權，完全免費且無商業使用限制。

### 核心特色

- **完全免費**：Apache 2.0 授權，適合商業專案使用
- **流暢語法**：支援方法鏈結的自然語言風格
- **豐富斷言**：涵蓋物件、集合、字串、數值、例外等各種類型
- **優秀錯誤訊息**：提供詳細且易理解的失敗資訊
- **高性能**：優化的實作確保測試執行效率
- **可擴展**：支援自訂 Assertions 方法

### 與 FluentAssertions 的關係

AwesomeAssertions 是 FluentAssertions 的社群 fork，主要差異：

| 項目           | FluentAssertions   | AwesomeAssertions      |
| -------------- | ------------------ | ---------------------- |
| **授權**       | 商業專案需付費     | Apache 2.0（完全免費） |
| **命名空間**   | `FluentAssertions` | `AwesomeAssertions`    |
| **API 相容性** | 原版               | 高度相容               |
| **社群支援**   | 官方維護           | 社群維護               |

---

## 安裝與設定

### NuGet 套件安裝

```bash
# .NET CLI
dotnet add package AwesomeAssertions

# Package Manager Console
Install-Package AwesomeAssertions
```

### csproj 設定（推薦）

```xml
<ItemGroup>
  <PackageReference Include="AwesomeAssertions" Version="9.1.0" PrivateAssets="all" />
</ItemGroup>
```

### 命名空間引用

```csharp
using AwesomeAssertions;
using Xunit;
```

---

## 核心 Assertions 語法

所有 Assertions 皆以 `.Should()` 開始，搭配流暢方法鏈結。

| 類別 | 常用方法 | 說明 |
|------|----------|------|
| **物件** | `NotBeNull()`, `BeOfType<T>()`, `BeEquivalentTo()` | 空值、類型、相等性檢查 |
| **字串** | `Contain()`, `StartWith()`, `MatchRegex()`, `BeEquivalentTo()` | 內容、模式、忽略大小寫比對 |
| **數值** | `BeGreaterThan()`, `BeInRange()`, `BeApproximately()` | 比較、範圍、浮點精度 |
| **集合** | `HaveCount()`, `Contain()`, `BeEquivalentTo()`, `AllSatisfy()` | 數量、內容、順序、條件 |
| **例外** | `Throw<T>()`, `NotThrow()`, `WithMessage()`, `WithInnerException()` | 例外類型、訊息、巢狀例外 |
| **非同步** | `ThrowAsync<T>()`, `CompleteWithinAsync()` | 非同步例外與完成驗證 |

> 完整語法範例與程式碼請參閱 [references/core-assertions-syntax.md](references/core-assertions-syntax.md)

---

## 進階技巧：複雜物件比對

使用 `BeEquivalentTo()` 搭配 `options` 進行深度物件比較：

- **排除屬性**：`options.Excluding(u => u.Id)` — 排除自動生成欄位
- **動態排除**：`options.Excluding(ctx => ctx.Path.EndsWith("At"))` — 按模式排除
- **循環參考**：`options.IgnoringCyclicReferences().WithMaxRecursionDepth(10)`

---

## 進階技巧：自訂 Assertions 擴展

建立領域特定擴展方法，如 `product.Should().BeValidProduct()`，以及可重用排除擴展如 `ExcludingAuditFields()`。

參考 [templates/custom-assertions-template.cs](templates/custom-assertions-template.cs) 瞭解完整實作。

> 完整範例請參閱 [references/complex-object-assertions.md](references/complex-object-assertions.md)

---

## 效能最佳化策略

- **大量資料**：先用 `HaveCount()` 快速檢查數量，再抽樣驗證（避免全量 `BeEquivalentTo`）
- **選擇性比對**：使用匿名物件 + `ExcludingMissingMembers()` 只驗證關鍵屬性

```csharp
// 選擇性屬性比對 — 只驗證關鍵欄位
order.Should().BeEquivalentTo(new
{
    CustomerId = 123,
    TotalAmount = 999.99m,
    Status = "Pending"
}, options => options.ExcludingMissingMembers());
```

---

## 最佳實踐與團隊標準

### 測試命名規範

遵循 `方法_情境_預期結果` 模式（如 `CreateUser_有效電子郵件_應回傳啟用的使用者`）。

### 錯誤訊息優化

在斷言中加入 `because` 字串，提供清晰的失敗上下文：

```csharp
result.IsSuccess.Should().BeFalse("because negative payment amounts are not allowed");
```

### AssertionScope 使用

使用 `AssertionScope` 收集多個失敗訊息，一次顯示所有問題：

```csharp
using (new AssertionScope())
{
    user.Should().NotBeNull("User creation should not fail");
    user.Id.Should().BeGreaterThan(0, "User should have valid ID");
    user.Email.Should().NotBeNullOrEmpty("Email is required");
}
```

---

## 常見情境與解決方案

| 情境 | 關鍵技巧 |
|------|----------|
| API 回應驗證 | `BeEquivalentTo()` + `Including()` 選擇性比對 |
| 資料庫實體驗證 | `BeEquivalentTo()` + `Excluding()` 排除自動生成欄位 |
| 事件驗證 | 訂閱捕獲事件後逐一驗證屬性 |

> 完整程式碼範例請參閱 [references/common-scenarios.md](references/common-scenarios.md)

---

## 疑難排解

### 問題 1：BeEquivalentTo 失敗但物件看起來相同

**原因**：可能包含自動生成欄位或時間戳記

**解決方案**：

```csharp
// 排除動態欄位
actual.Should().BeEquivalentTo(expected, options => options
    .Excluding(x => x.Id)
    .Excluding(x => x.CreatedAt)
    .Excluding(x => x.UpdatedAt)
);
```

### 問題 2：集合順序不同導致失敗

**原因**：集合順序不同

**解決方案**：

```csharp
// 使用 BeEquivalentTo 忽略順序
actual.Should().BeEquivalentTo(expected); // 不檢查順序

// 或明確指定需要檢查順序
actual.Should().Equal(expected); // 檢查順序
```

### 問題 3：浮點數比較失敗

**原因**：浮點數精度問題

**解決方案**：

```csharp
// 使用精度容差
actualValue.Should().BeApproximately(expectedValue, 0.001);
```

---

## 何時使用此技能

### 適用情境

撰寫單元測試或整合測試時
需要驗證複雜物件結構時
比對 API 回應或資料庫實體時
需要清晰的失敗訊息時
建立領域特定測試標準時

### 不適用情境

效能測試（使用專用 benchmarking 工具）
負載測試（使用 K6、JMeter 等）
UI 測試（使用 Playwright、Selenium）

---

## 與其他技能的配合

### 與 unit-test-fundamentals 搭配

先使用 `unit-test-fundamentals` 建立測試結構，再使用本技能撰寫斷言：

```csharp
[Fact]
public void Calculator_Add_兩個正數_應回傳總和()
{
    // Arrange - 遵循 3A Pattern
    var calculator = new Calculator();
    
    // Act
    var result = calculator.Add(2, 3);
    
    // Assert - 使用 AwesomeAssertions
    result.Should().Be(5);
}
```

### 與 test-naming-conventions 搭配

使用 `test-naming-conventions` 的命名規範，搭配本技能的斷言：

```csharp
[Fact]
public void CreateUser_有效資料_應回傳啟用使用者()
{
    var user = userService.CreateUser("test@example.com");
    
    user.Should().NotBeNull()
        .And.BeOfType<User>();
    user.IsActive.Should().BeTrue();
}
```

### 與 xunit-project-setup 搭配

在 `xunit-project-setup` 建立的專案中安裝並使用 AwesomeAssertions。

---

## 輸出格式

- 產生使用 AwesomeAssertions 流暢語法的測試斷言
- 使用 Should().Be/BeEquivalentTo/Contain 等方法鏈
- 包含物件比對、集合驗證、例外斷言範例
- 提供 .csproj 套件參考（AwesomeAssertions）

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 04 - AwesomeAssertions 基礎應用與實戰技巧**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10374188
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day04

- **Day 05 - AwesomeAssertions 進階技巧與複雜情境應用**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10374425
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day05

### 官方資源

- **AwesomeAssertions GitHub**：https://github.com/AwesomeAssertions/AwesomeAssertions
- **AwesomeAssertions 官方文件**：https://awesomeassertions.org/

### 相關文章

- **Fluent Assertions 授權變化討論**：https://www.dotblogs.com.tw/mrkt/2025/04/19/152408

---

## 總結

AwesomeAssertions 提供了強大且可讀的斷言語法，是撰寫高品質測試的重要工具。透過：

1. **流暢語法**：讓測試程式碼更易讀
2. **豐富斷言**：涵蓋各種資料類型
3. **自訂擴展**：建立領域特定斷言
4. **效能優化**：處理大量資料情境
5. **完全免費**：Apache 2.0 授權無商業限制

記住：好的斷言不僅能驗證結果，更能清楚表達預期行為，並在失敗時提供有用的診斷資訊。

參考 [templates/assertion-examples.cs](templates/assertion-examples.cs) 查看更多實用範例。
