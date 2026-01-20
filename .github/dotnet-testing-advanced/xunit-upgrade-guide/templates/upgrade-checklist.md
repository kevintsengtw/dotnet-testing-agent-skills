# xUnit 2.x → 3.x 升級檢查清單

## 升級前準備

### 環境確認

- [ ] **目標框架版本**
  - [ ] .NET 8.0 或更新版本 (推薦)
  - [ ] 或 .NET Framework 4.7.2 或更新版本
  - [ ] ⚠️ 注意：不支援 .NET Core 3.1、.NET 5/6/7

- [ ] **專案格式**
  - [ ] 確認為 SDK-style 專案格式
  - [ ] 專案檔開頭應為 `<Project Sdk="Microsoft.NET.Sdk">`

- [ ] **IDE 版本**
  - [ ] Visual Studio 2022 17.8+
  - [ ] 或 Rider 2023.3+
  - [ ] 或 VS Code (最新版)

### 程式碼盤點

- [ ] **識別 async void 測試方法**
  - [ ] 搜尋模式：`async void`
  - [ ] 正規表達式：`async\s+void.*\[(Fact|Theory)\]`
  - [ ] 記錄需要修改的檔案數量：______

- [ ] **檢查 IAsyncLifetime 實作**
  - [ ] 搜尋實作 `IAsyncLifetime` 的類別
  - [ ] 確認是否同時實作 `IDisposable`
  - [ ] 規劃將 `Dispose` 邏輯移至 `DisposeAsync`

- [ ] **識別 SkippableFact/SkippableTheory 使用**
  - [ ] 搜尋 `[SkippableFact]` 和 `[SkippableTheory]`
  - [ ] 規劃改用 `Assert.Skip` 或 `SkipUnless`

- [ ] **檢查自訂屬性**
  - [ ] 識別繼承自 `DataAttribute` 的自訂類別
  - [ ] 規劃更新為新的非同步 API

### 相依套件評估

- [ ] **記錄目前套件版本**
  - [ ] xunit: ______
  - [ ] xunit.runner.visualstudio: ______
  - [ ] Microsoft.NET.Test.Sdk: ______
  - [ ] AwesomeAssertions/FluentAssertions: ______
  - [ ] NSubstitute/Moq: ______
  - [ ] AutoFixture: ______

- [ ] **確認相容性**
  - [ ] 檢查各套件是否支援 xUnit 3.x
  - [ ] 特別注意 AutoFixture.Xunit3 套件

### 備份

- [ ] **建立升級分支**
  ```bash
  git checkout -b feature/upgrade-xunit-v3
  git push -u origin feature/upgrade-xunit-v3
  ```

---

## 升級執行

### 專案檔案修改

- [ ] **更新 OutputType**
  ```xml
  <OutputType>Exe</OutputType>
  ```

- [ ] **更新套件參考**
  - [ ] 移除 `xunit` → 新增 `xunit.v3`
  - [ ] 移除 `xunit.abstractions` (不再需要)
  - [ ] 更新 `xunit.runner.visualstudio` 到 3.x 版本
  - [ ] 更新 `Microsoft.NET.Test.Sdk` 到最新版本

- [ ] **加入 xunit.runner.json** (選用)
  ```json
  {
    "$schema": "https://xunit.net/schema/v3/xunit.runner.schema.json",
    "parallelAlgorithm": "conservative",
    "maxParallelThreads": 4
  }
  ```

### 程式碼修正

- [ ] **修正 async void 測試**
  - [ ] 將所有 `async void` 改為 `async Task`
  - [ ] 驗證修改數量：______

- [ ] **更新 using 陳述式**
  - [ ] 移除 `using Xunit.Abstractions;`

- [ ] **修正 IAsyncLifetime 實作**
  - [ ] 將 `Dispose` 邏輯整合到 `DisposeAsync`

- [ ] **修正 SkippableFact/SkippableTheory**
  - [ ] 改用 `Assert.Skip` 或 `SkipUnless/SkipWhen` 屬性

- [ ] **更新自訂 DataAttribute**
  - [ ] 實作新的 `GetDataAsync` 方法

### 編譯與測試

- [ ] **清理並還原**
  ```bash
  dotnet clean
  dotnet restore
  ```

- [ ] **編譯**
  ```bash
  dotnet build
  ```
  - [ ] 記錄編譯錯誤數量：______
  - [ ] 逐一解決編譯錯誤

- [ ] **執行測試**
  ```bash
  dotnet test --verbosity normal
  ```
  - [ ] 記錄測試結果：通過 ______ / 失敗 ______ / 跳過 ______

---

## 升級後驗證

### 功能驗證

- [ ] **所有測試通過**
  - [ ] 單元測試：______
  - [ ] 整合測試：______

- [ ] **效能比較**
  - [ ] 升級前執行時間：______
  - [ ] 升級後執行時間：______

### CI/CD 驗證

- [ ] **測試執行**
  ```bash
  dotnet test --configuration Release --logger trx
  ```

- [ ] **測試報告**
  - [ ] 確認報告格式正確解析
  - [ ] 驗證測試結果正確顯示

- [ ] **並行執行設定**
  - [ ] 根據 CI 環境調整 `maxParallelThreads`

### 文件與培訓

- [ ] **更新專案文件**
  - [ ] README.md
  - [ ] CONTRIBUTING.md

- [ ] **團隊知識轉移**
  - [ ] 分享升級經驗
  - [ ] 介紹新功能用法

---

## 新功能啟用 (選用)

- [ ] **動態跳過測試**
  - [ ] 使用 `Assert.Skip` 或 `SkipUnless/SkipWhen`

- [ ] **明確測試**
  - [ ] 標記 `[Fact(Explicit = true)]`

- [ ] **Assembly Fixtures**
  - [ ] 建立全域資源管理

- [ ] **Test Pipeline Startup**
  - [ ] 實作全域初始化

---

## 問題記錄

| 問題描述 | 解決方案 | 狀態 |
| -------- | -------- | ---- |
|          |          |      |
|          |          |      |
|          |          |      |

---

## 簽核

- [ ] 開發人員確認：______________ 日期：______________
- [ ] 程式碼審查：______________ 日期：______________
- [ ] 測試確認：______________ 日期：______________
