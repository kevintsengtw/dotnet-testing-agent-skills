---
name: dotnet-testing-code-coverage-analysis
description: |
  程式碼覆蓋率分析完整指南。當需要分析程式碼覆蓋率、產生覆蓋率報告或設定 CI/CD 覆蓋率檢查時使用。涵蓋 Coverlet 設定、報告產生、指標解讀與循環複雜度整合。包含 Fine Code Coverage、VS Code 內建工具與最佳實踐。
  Make sure to use this skill whenever the user mentions code coverage, Coverlet, coverage report, branch coverage, cyclomatic complexity, or test quality metrics, even if they don't explicitly ask for coverage analysis.
  Keywords: code coverage, 程式碼覆蓋率, 覆蓋率分析, coverage report, Coverlet, Fine Code Coverage, dotnet-coverage, ReportGenerator, line coverage, branch coverage, 行覆蓋率, 分支覆蓋率, cyclomatic complexity, 循環複雜度, runsettings, cobertura
---

# 程式碼覆蓋率分析指南

## Code Coverage 核心概念

**程式碼覆蓋率 (Code Coverage)** 是一種測量指標，用來統計測試執行時實際執行了多少程式碼。

**實際價值：** 找出測試盲點、評估測試完整性、輔助重構決策、增加測試信心。

**常見誤解：**

- 涵蓋率 100% 不代表沒有 Bug — 只代表程式碼有被執行，不代表驗證了正確行為
- 涵蓋率數字越高不一定越好 — 重點是測試的有效性
- 把涵蓋率當作 KPI 會適得其反 — 開發者會為了衝數字而寫沒有 Assert 的測試

## 覆蓋率工具選擇

| 工具                         | 優點                           | 適用場景       |
| ---------------------------- | ------------------------------ | -------------- |
| Visual Studio Enterprise     | 內建整合，完整 UI              | 僅 Enterprise  |
| Fine Code Coverage（推薦）   | 免費，即時顯示，編輯器直接標示 | VS 所有版本    |
| .NET CLI + Coverlet          | 跨平台，CLI 自動化             | CI/CD 流程     |
| VS Code 內建                 | 跨平台，無需額外擴充套件       | VS Code 開發   |

### Fine Code Coverage 設定

- **安裝方式：** Visual Studio 延伸模組管理 → 搜尋 "Fine Code Coverage" → 安裝
- **必要設定：** 工具 → 選項 → Fine Code Coverage → Enable: `True`、Editor Colouring Line Highlighting: `True`

### VS Code 內建測試覆蓋率

1. 安裝 C# Dev Kit 擴充套件
2. 開啟測試總管（燒杯圖示）
3. 點選「執行涵蓋範圍測試」
4. 查看結果：測試涵蓋範圍視圖、編輯器內顯示、檔案總管顯示

## 執行覆蓋率分析

```powershell
# .NET CLI（推薦用於 CI/CD）
dotnet test --collect:"XPlat Code Coverage"
dotnet test --collect:"XPlat Code Coverage" --results-directory ./coverage

# Fine Code Coverage：在 VS 中執行測試後自動顯示
# VS Code：開啟測試總管 → 點選「執行涵蓋範圍測試」
```

### 方法說明

| 方法              | 工具                  | 操作步驟                                      |
| ----------------- | --------------------- | --------------------------------------------- |
| .NET CLI          | Coverlet + CLI        | 執行 `dotnet test --collect:"XPlat Code Coverage"` |
| Fine Code Coverage | VS 擴充套件          | 在 VS 中執行測試後自動顯示                    |
| VS Code           | C# Dev Kit            | 開啟測試總管 → 執行涵蓋範圍測試               |

## 設定 Coverlet

測試專案需安裝 `coverlet.collector` 套件，並可透過 `runsettings` 檔案進行進階設定（排除規則、閾值、報告格式）。

> 完整 csproj 配置範例請參考 [references/coverlet-csproj-config.md](references/coverlet-csproj-config.md)

## 解讀覆蓋率報告

**顏色標示：** 綠色（已覆蓋）、黃色（部分覆蓋）、紅色（未覆蓋）

**覆蓋率指標：**

1. **Line Coverage（行覆蓋率）** — 被執行的行數 / 總行數，最基本的指標
2. **Branch Coverage（分支覆蓋率）** — 被執行的分支數 / 總分支數，比行覆蓋率更準確
3. **Method Coverage（方法覆蓋率）** — 被執行的方法數 / 總方法數

**報告解讀策略：** 優先處理紅色區域（完全未測試）→ 檢查黃色區域（部分分支未測試）→ 評估必要性（簡單 getter/setter 可跳過）

## 結合複雜度指標

循環複雜度（Cyclomatic Complexity）代表程式中獨立邏輯路徑的數量，等於至少需要的測試案例數量。每個 if、for、while、case、&&、|| 都會增加複雜度。

**Visual Studio 擴充套件：**

- **CodeMaintainability** — 顯示可維護性指標、計算循環複雜度
- **CodeMaid** — Spade 功能視覺化程式碼結構、顯示每個方法的複雜度

> 完整範例與測試策略請參考 [references/cyclomatic-complexity-example.md](references/cyclomatic-complexity-example.md)

## 改善覆蓋率的策略

**漸進式改善流程：**

1. 第一階段：覆蓋核心業務邏輯（目標 60-70%）
2. 第二階段：補充邊界條件測試（目標 70-80%）
3. 第三階段：處理異常情境（目標 80-85%）
4. 維持階段：新增功能必須有測試

**優先順序：**

- **高優先級**：業務邏輯核心、金融計算、資料驗證、權限控制、異常處理
- **中優先級**：資料轉換、格式化邏輯、查詢邏輯
- **低優先級**：簡單 getter/setter、DTO 類別、自動產生的程式碼

**排除不必要的程式碼：** 使用 `[ExcludeFromCodeCoverage]` 屬性或在 runsettings 中排除。

## 最佳實踐

### 測試案例數量決策

1. **基於需求分析** — 列出使用案例、識別邊界條件和異常情況
2. **參考複雜度指標** — 循環複雜度提供測試案例下限
3. **平衡覆蓋率與品質** — 不以 100% 覆蓋率為唯一目標，專注於關鍵業務邏輯

### 四大測試類型

1. **邊界測試**：測試輸入值的上下限
2. **異常測試**：驗證錯誤處理邏輯
3. **主流程測試**：覆蓋正常的業務流程
4. **條件分支測試**：確保所有分支都有測試

### 持續改善

- 每次提交前檢查涵蓋率變化，Pull Request 時審查覆蓋率
- 關注未覆蓋的關鍵程式碼，優先處理高複雜度未測試區域
- 新功能必須包含測試，Code Review 包含測試檢查

## CI/CD 整合

覆蓋率分析可整合至 CI/CD Pipeline，在 GitHub Actions 中使用 `dotnet test --collect:"XPlat Code Coverage"` 搭配 `reportgenerator` 產生報告；在 Azure DevOps 中使用 `DotNetCoreCLI@2` 任務搭配 `PublishCodeCoverageResults@1`。

> 完整 YAML 設定範例請參閱 [references/cicd-integration.md](references/cicd-integration.md)

## 常見問題與解決方案

### Q1: 覆蓋率顯示 0%？

1. 確認已安裝 `coverlet.collector` 套件
2. 檢查 runsettings 設定是否正確
3. 確認測試有實際執行
4. 查看是否有排除設定過於廣泛

### Q2: Visual Studio 看不到覆蓋率？

- Community/Professional 版本：安裝 Fine Code Coverage 擴充套件
- Enterprise 版本：使用內建功能
- 確認已啟用覆蓋率收集

### Q3: VS Code 無法顯示覆蓋率？

1. 確認已安裝 C# Dev Kit
2. 重新執行「執行涵蓋範圍測試」
3. 檢查 lcov 檔案是否產生
4. 嘗試重新載入視窗

### Q4: 如何提升覆蓋率？

1. 識別未覆蓋的關鍵程式碼（紅色區域）
2. 補充邊界條件測試
3. 測試所有條件分支
4. 加入異常情境測試
5. 考慮重構過於複雜的方法

## 檢查清單

- [ ] 已安裝 `coverlet.collector` 套件
- [ ] 可以執行 `dotnet test --collect:"XPlat Code Coverage"`
- [ ] 工具可以正常顯示覆蓋率結果
- [ ] 了解覆蓋率數字的真正意義（不是 KPI）
- [ ] 已排除不必要的程式碼
- [ ] 專注於測試品質而非覆蓋率數字

## 範本檔案

- `templates/runsettings-template.xml` - 覆蓋率設定範本
- `templates/coverage-workflow.md` - 完整的工作流程說明

## 核心理念

> **程式碼覆蓋率是手段，不是目的。** 重點在於關鍵業務邏輯是否都有測試、測試是否真正驗證了預期行為、是否能在重構時提供信心。

## 輸出格式

- 產生 `coverage.runsettings` 設定檔，配置覆蓋率收集參數與排除規則
- 修改測試專案 `.csproj`，確保包含 `coverlet.collector` 套件參考
- 產生覆蓋率報告（cobertura XML 格式），可供 CI/CD 管線使用
- 提供覆蓋率分析摘要，標示未覆蓋的關鍵區域與改善建議

## 參考資源

### 原始文章

- **Day 06 - Code Coverage 程式碼涵蓋範圍實戰指南**
  - 鐵人賽文章：https://ithelp.ithome.com.tw/articles/10374467

### 官方文件

- [使用程式碼涵蓋範圍進行單元測試](https://learn.microsoft.com/dotnet/core/testing/unit-testing-code-coverage)
- [dotnet-coverage 工具](https://learn.microsoft.com/dotnet/core/additional-tools/dotnet-coverage)
- [VS Code Testing](https://code.visualstudio.com/docs/editor/testing)

### 工具

- [Fine Code Coverage](https://marketplace.visualstudio.com/items?itemName=FortuneNgwenya.FineCodeCoverage2022)
- [Coverlet](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator](https://github.com/danielpalme/ReportGenerator)

### 相關技能

- `unit-test-fundamentals` - 單元測試基礎與 FIRST 原則
- `xunit-project-setup` - xUnit 專案設定
