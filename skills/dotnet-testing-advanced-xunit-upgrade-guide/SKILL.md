---
name: dotnet-testing-advanced-xunit-upgrade-guide
description: |
  xUnit 2.9.x 到 3.x 升級完整指南。當需要將 xUnit v2 升級至 v3 或了解 xUnit v3 新功能與破壞性變更時使用。涵蓋套件更新、async void 修正、IAsyncLifetime 調整。包含新功能介紹: Assert.Skip、Explicit Tests、Matrix Theory、Assembly Fixtures。
  Make sure to use this skill whenever the user mentions xUnit upgrade, xUnit v3, xUnit migration, xUnit breaking changes, or Assert.Skip, even if they don't explicitly ask for xUnit upgrade guidance.
  Keywords: xunit upgrade, xunit v3, xunit 3.x, xunit migration, xunit 升級, xunit.v3, OutputType Exe, IAsyncLifetime v3, Assert.Skip, SkipUnless, SkipWhen, Explicit attribute, MatrixTheoryData, AssemblyFixture, 破壞性變更
---

# xUnit 升級指南：從 2.9.x 到 3.x

## 核心概念

### 套件命名變革

xUnit v3 採用全新的套件命名策略：

| v1~v2 套件名稱              | v3 套件名稱                         | 說明         |
| --------------------------- | ----------------------------------- | ------------ |
| `xunit`                     | `xunit.v3`                          | 主要測試框架 |
| `xunit.assert`              | `xunit.v3.assert`                   | 斷言函式庫   |
| `xunit.core`                | `xunit.v3.core`                     | 核心元件     |
| `xunit.abstractions`        | (移除)                              | 不再需要     |
| `xunit.runner.visualstudio` | `xunit.runner.visualstudio` (3.x.y) | 測試執行器   |

**重要**：使用 `xunit.v3` 套件名稱，不是 `xunit`。

### 最低運行時需求

- **.NET Framework 4.7.2+** 或 **.NET 8.0+** (推薦)
- **不支援**：.NET Core 3.1、.NET 5/6/7

---

## 破壞性變更

涵蓋 OutputType 改為 Exe、async void 不再支援、IAsyncLifetime 繼承 IAsyncDisposable、SkippableFact 移除、僅支援 SDK-style 專案、自訂 DataAttribute 簽名變更等六項破壞性變更，每項皆附修正前後的程式碼對照。

> 完整破壞性變更清單與程式碼範例請參考 [references/breaking-changes.md](references/breaking-changes.md)

---

## 升級步驟

五步驟 SOP：建立升級分支 → 更新專案檔案（套件、OutputType）→ 修正 async void → 更新 using → 編譯與測試。另含 .NET 10 SDK MTP 模式遷移指南（global.json 設定、CLI 用法變更、VSTest 遷移步驟）。

> 完整升級步驟與 .NET 10 MTP 模式指南請參考 [references/upgrade-steps.md](references/upgrade-steps.md)

---

## xUnit 3.x 新功能

涵蓋 Assert.Skip / SkipUnless / SkipWhen 動態跳過、Explicit Tests、[Test] 屬性、MatrixTheoryData 矩陣測試、Assembly Fixtures、Test Pipeline Startup、多格式測試報告等新功能，每項皆附完整程式碼範例。

> 完整新功能範例請參考 [references/new-features.md](references/new-features.md)

### Testcontainers.XunitV3 整合套件

Testcontainers 官方推出 `Testcontainers.XunitV3`（4.9.0），專為 xUnit v3 設計的整合套件，自動管理容器的建立與銷毀，取代手動實作 `IAsyncLifetime` 的方式。

```xml
<PackageReference Include="Testcontainers.XunitV3" Version="4.9.0" />
```

> 若專案同時使用 Testcontainers 與 xUnit v3，建議採用此套件簡化容器生命週期管理。

---

## xunit.runner.json 設定

```json
{
  "$schema": "https://xunit.net/schema/v3/xunit.runner.schema.json",
  "parallelAlgorithm": "conservative",
  "maxParallelThreads": 4,
  "diagnosticMessages": true,
  "internalDiagnosticMessages": false,
  "methodDisplay": "classAndMethod",
  "preEnumerateTheories": true,
  "stopOnFail": false
}
```

---

## 常見問題與解決方案

### 問題 1：找不到 xunit.abstractions

移除 `using Xunit.Abstractions;`，相關類型已移到 `Xunit` 命名空間。

### 問題 2：IDE 無法發現測試

確認 IDE 版本：Visual Studio 2022 17.8+、Rider 2023.3+、VS Code 最新版。如仍有問題，可暫時停用 MTP：

```xml
<PropertyGroup>
  <EnableMicrosoftTestingPlatform>false</EnableMicrosoftTestingPlatform>
</PropertyGroup>
```

> **.NET 10 SDK 注意**：若使用 MTP 模式，停用 MTP 會導致 `dotnet test` 錯誤。應改為移除 `global.json` 中的 `test` 區段。

---

## 升級檢查清單

### 升級前

- [ ] 確認目標框架版本 (.NET 8+ 或 .NET Framework 4.7.2+)
- [ ] 檢查專案檔案格式 (SDK-style)
- [ ] 識別所有 async void 測試方法
- [ ] 檢查 IAsyncLifetime 實作
- [ ] 評估相依套件相容性
- [ ] 建立備份分支

### 升級過程

- [ ] 更新套件參考 (使用 `xunit.v3`)
- [ ] 移除 `xunit.abstractions` 參考
- [ ] 修改 OutputType 為 Exe
- [ ] 修正所有 async void 測試方法
- [ ] 更新 using 陳述式
- [ ] 重構自訂屬性 (如有)
- [ ] 驗證編譯成功
- [ ] 執行所有測試

### 升級後驗證

- [ ] 功能完整性測試
- [ ] 效能基準比較
- [ ] CI/CD Pipeline 驗證
- [ ] 文檔更新
- [ ] 團隊培訓
- [ ] （.NET 10+）設定 `global.json` 啟用 MTP 模式並驗證 `dotnet test` 正常運作

---

## IDE 與工具支援

| IDE           | 最低版本   |
| ------------- | ---------- |
| Visual Studio | 2022 17.8+ |
| VS Code       | 最新版     |
| Rider         | 2023.3+    |

xUnit 3.x 預設啟用 Microsoft Testing Platform（MTP），搭配 .NET 10 SDK 可使用原生 MTP 模式。

> .NET 10 MTP 模式詳細設定請參考 [references/upgrade-steps.md](references/upgrade-steps.md)

---

## 輸出格式

- 產生升級後的測試專案 .csproj 設定（xunit.v3 套件、OutputType Exe）
- 包含 async void 修正為 async Task 的測試程式碼
- 包含 IAsyncLifetime 調整與 SkippableFact 替換範例
- 產生 xunit.runner.json 設定檔

## 參考資源

### 原始文章

本技能內容提煉自「老派軟體工程師的測試修練 - 30 天挑戰」系列文章：

- **Day 26 - xUnit 升級指南：從 2.9.x 到 3.x 的轉換**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10377477
  - 範例程式碼：https://github.com/kevintsengtw/30Days_in_Testing_Samples/tree/main/day26

### 官方文件

- [xUnit.net 官方網站](https://xunit.net/)
- [xUnit v3 新功能文件](https://xunit.net/docs/getting-started/v3/whats-new)
- [xUnit 2.x → 3.x 官方遷移指南](https://xunit.net/docs/getting-started/v3/migration)
- [xunit.v3 NuGet 套件](https://www.nuget.org/packages/xunit.v3)

### 相關技能

- `dotnet-testing-xunit-project-setup` - xUnit 專案設定基礎
- `dotnet-testing-advanced-tunit-fundamentals` - TUnit 替代框架
- `dotnet-testing-advanced-testcontainers-database` - Testcontainers 資料庫整合測試（搭配 XunitV3 套件）
