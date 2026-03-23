---
name: dotnet-testing-advanced-tunit-fundamentals
description: |
  TUnit 新世代測試框架入門完整指南。當需要使用 TUnit 建立測試專案或從 xUnit 遷移至 TUnit 時使用。涵蓋 Source Generator 驅動測試發現、AOT 編譯支援、流暢式非同步斷言。包含專案建立、[Test] 屬性、生命週期管理、並行控制與 xUnit 語法對照。
  Make sure to use this skill whenever the user mentions TUnit, Source Generator testing, AOT test framework, TUnit vs xUnit, or migrating to TUnit, even if they don't explicitly ask for TUnit fundamentals.
  Keywords: TUnit, tunit testing, source generator testing, AOT testing, 新世代測試框架, [Test], [Arguments], TUnit.Assertions, Assert.That, Before(Test), After(Test), NotInParallel, TUnit.Templates, Microsoft.Testing.Platform, TUnit vs xUnit, 並行執行
---

# TUnit 新世代測試框架入門基礎

## TUnit 框架核心特色

### 1. Source Generator 驅動的測試發現

TUnit 與傳統測試框架最大的差異在於使用 Source Generator 在**編譯時期**完成測試發現，避免反射成本、完全支援 Native AOT、更快的啟動時間。

```csharp
// TUnit 在編譯時期就透過 Source Generator 產生測試註冊程式碼
public class ModernTests
{
    [Test] // 編譯時期就被處理和最佳化
    public async Task TestMethod()
    {
        await Assert.That(true).IsTrue();
    }
}
```

### 2. AOT (Ahead-of-Time) 編譯支援

```text
傳統 JIT：C# 原始碼 → IL 中間碼 → 執行時期 JIT 編譯 → 機器碼 → 執行
AOT：    C# 原始碼 → 編譯時期直接產生 → 機器碼 → 直接執行
```

AOT 優勢：超快啟動時間、更小的記憶體占用、可預測的效能、更適合容器化部署。大型專案可達 10-30 倍啟動時間改善。

### 3. Microsoft.Testing.Platform 採用

TUnit 建構在微軟最新的 Microsoft.Testing.Platform 之上，而非傳統的 VSTest 平台。

**重要注意事項：** TUnit 專案**不需要**也**不應該**安裝 `Microsoft.NET.Test.Sdk` 套件。

### 4. 預設並行執行

TUnit 將並行執行設為預設，需要時可用 `[NotInParallel("GroupName")]` 控制特定測試群組依序執行。

## TUnit 專案建立

支援手動建立（console 模板 + TUnit 套件）或使用 TUnit Template（`dotnet new tunit`，推薦）。專案需設定 csproj（TUnit 套件、IsTestProject）與 GlobalUsings.cs。所有測試方法**必須是非同步的**（`async Task`）。

> 完整專案建立步驟與 csproj 範例請參閱 [references/project-setup.md](references/project-setup.md)

## 測試屬性與參數化

### 基本測試 [Test]

TUnit 統一使用 `[Test]` 屬性，不像 xUnit 區分 `[Fact]` 和 `[Theory]`：

```csharp
[Test]
public async Task Add_輸入1和2_應回傳3()
{
    var calculator = new Calculator();
    var result = calculator.Add(1, 2);
    await Assert.That(result).IsEqualTo(3);
}
```

### 參數化測試 [Arguments]

```csharp
[Test]
[Arguments(1, 2, 3)]
[Arguments(-1, 1, 0)]
[Arguments(0, 0, 0)]
public async Task Add_多組輸入_應回傳正確結果(int a, int b, int expected)
{
    var calculator = new Calculator();
    var result = calculator.Add(a, b);
    await Assert.That(result).IsEqualTo(expected);
}
```

## TUnit.Assertions 斷言系統

TUnit 採用流暢式（Fluent）斷言設計，所有斷言都是非同步的。支援相等性、布林值、數值比較、字串、集合、例外等多種斷言，並可透過 `And` / `Or` 組合條件。

```csharp
await Assert.That(actual).IsEqualTo(expected);
await Assert.That(email).Contains("@").And.EndsWith(".com");
await Assert.That(() => action()).Throws<InvalidOperationException>();
```

> 完整斷言類型與範例請參閱 [references/tunit-assertions-detail.md](references/tunit-assertions-detail.md)

## 測試生命週期管理

TUnit 支援建構式 / `Dispose` 模式，以及 `[Before(Test)]`、`[Before(Class)]`、`[After(Test)]`、`[After(Class)]` 等屬性。

```text
執行順序：Before(Class) → 建構式 → Before(Test) → 測試方法 → After(Test) → Dispose → After(Class)
```

> 完整生命週期範例與屬性對照表請參閱 [references/lifecycle-management.md](references/lifecycle-management.md)

## 並行執行控制與 xUnit 遷移

TUnit 預設並行執行所有測試，使用 `[NotInParallel]` 控制特定群組依序執行。提供完整的 xUnit → TUnit 語法對照表、遷移範例、CLI 執行指令與 IDE 整合設定。

> 完整並行控制、語法對照與遷移範例請參閱 [references/xunit-migration.md](references/xunit-migration.md)

## 效能比較

| 場景             | xUnit   | TUnit   | TUnit AOT | 效能提升  |
| ---------------- | ------- | ------- | --------- | --------- |
| **簡單測試執行** | 1,400ms | 1,000ms | 60ms      | 23x (AOT) |
| **非同步測試**   | 1,400ms | 930ms   | 26ms      | 54x (AOT) |
| **並行測試**     | 1,425ms | 999ms   | 54ms      | 26x (AOT) |

## 常見問題與解決方案

1. **套件相容性** — 移除 `Microsoft.NET.Test.Sdk`，TUnit 使用新的測試平台
2. **IDE 整合問題** — 確認 IDE 版本支援 Microsoft.Testing.Platform，啟用相關預覽功能
3. **非同步斷言遺忘** — 所有斷言都需要 `await`，測試方法必須是 `async Task`

## 適用場景評估

### 適合使用 TUnit

1. **全新專案**：沒有歷史包袱
2. **效能要求高**：大型測試套件（1000+ 測試）
3. **技術棧先進**：使用 .NET 8+，計劃採用 AOT
4. **CI/CD 重度使用**：測試執行時間直接影響部署頻率
5. **容器化部署**：快速啟動時間很重要

### 暫時不建議

1. **Legacy 專案**：已有大量 xUnit 測試
2. **保守團隊**：需要穩定性勝過創新性
3. **複雜測試生態**：大量使用 xUnit 特定套件
4. **舊版 .NET**：還在 .NET 6/7

## 輸出格式

- 產生 TUnit 測試類別（含 [Test]、[Arguments] 屬性）
- 產生 .csproj 設定（TUnit 套件參考）
- 提供 GlobalUsings.cs 設定
- 包含 xUnit 到 TUnit 的遷移程式碼對照

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 28 - TUnit 入門 - 下世代 .NET 測試框架探索**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10377828
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day28

### 官方資源

- [TUnit 官方網站](https://tunit.dev/)
- [TUnit GitHub](https://github.com/thomhurst/TUnit)
- [從 xUnit 遷移指南](https://tunit.dev/docs/migration/xunit)

### Microsoft 官方文件

- [Microsoft.Testing.Platform 介紹](https://learn.microsoft.com/dotnet/core/testing/microsoft-testing-platform-intro)
- [原生 AOT 部署](https://learn.microsoft.com/zh-tw/dotnet/core/deploying/native-aot)

### 相關技能

- `dotnet-testing-advanced-tunit-advanced` - TUnit 進階功能
- `dotnet-testing-advanced-xunit-upgrade-guide` - xUnit 升級指南
- `dotnet-testing-xunit-project-setup` - xUnit 專案設定（比較用）
