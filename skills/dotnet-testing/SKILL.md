---
name: dotnet-testing
description: |
  .NET 測試基礎技能總覽與引導中心。當使用者詢問「如何寫 .NET 測試」、「.NET 測試入門」、「需要哪些測試工具」、「測試最佳實踐」、「從零開始學測試」等一般性測試需求時觸發。會根據具體需求推薦適合的子技能組合，涵蓋測試基礎、測試資料、斷言、模擬、特殊場景等 19 個基礎技能。
  Make sure to use this skill whenever the user asks about .NET testing in general, wants to know which testing tool to use, or needs guidance on getting started with testing, even if they don't explicitly ask for an overview.
  Keywords: dotnet testing, .NET 測試, 測試入門, 如何寫測試, 測試最佳實踐, unit test, 單元測試, xunit, 3A pattern, FIRST 原則, assertion, 斷言, mock, stub, NSubstitute, test data, AutoFixture, Bogus, validator, FluentValidation, TimeProvider, IFileSystem, code coverage, ITestOutputHelper, test naming
---

# .NET 測試基礎技能總覽

本檔案是「導航中心」，幫助找到正確的子技能。子技能包含詳細的程式碼範例、最佳實踐和常見陷阱，載入正確的子技能能確保使用者獲得最完整的指引。

根據使用者需求匹配對應的子技能，使用 Skill tool 載入，讓子技能提供專業的測試指引。

---

## 快速技能對照表

**使用者提到的關鍵字 → 應載入的子技能**

### 最常用技能

| 使用者說... | 載入指令 | 用途說明 |
|------------|----------|----------|
| **Validator**、驗證器、CreateUserValidator | `/skill dotnet-testing-fluentvalidation-testing` | FluentValidation 測試 |
| **Mock**、模擬、IRepository、IService | `/skill dotnet-testing-nsubstitute-mocking` | 模擬外部依賴 |
| **AutoFixture**、測試資料生成 | `/skill dotnet-testing-autofixture-basics` | 自動產生測試資料 |
| **斷言**、Should()、BeEquivalentTo | `/skill dotnet-testing-awesome-assertions-guide` | 流暢斷言（必學） |
| **DateTime**、時間測試、TimeProvider | `/skill dotnet-testing-datetime-testing-timeprovider` | 時間相關測試 |
| **File**、檔案系統、IFileSystem | `/skill dotnet-testing-filesystem-testing-abstractions` | 檔案系統測試 |
| **Bogus**、假資料、Faker | `/skill dotnet-testing-bogus-fake-data` | 擬真資料生成 |
| **Builder Pattern**、WithXxx | `/skill dotnet-testing-test-data-builder-pattern` | Test Data Builder |
| **深層比對**、DTO 比對、Excluding | `/skill dotnet-testing-complex-object-comparison` | 複雜物件比對 |

### 基礎入門技能

| 使用者說... | 載入指令 | 用途說明 |
|------------|----------|----------|
| **從零開始**、測試基礎、FIRST 原則 | `/skill dotnet-testing-unit-test-fundamentals` | 單元測試基礎 |
| **測試命名**、如何命名測試 | `/skill dotnet-testing-test-naming-conventions` | 命名規範 |
| **建立測試專案**、xUnit 設定 | `/skill dotnet-testing-xunit-project-setup` | 專案建置 |

### 進階技能組合

| 使用者說... | 載入指令 | 用途說明 |
|------------|----------|----------|
| AutoFixture + Bogus | `/skill dotnet-testing-autofixture-bogus-integration` | 自動化+擬真資料 |
| AutoFixture + NSubstitute | `/skill dotnet-testing-autofixture-nsubstitute-integration` | 自動建立 Mock |
| AutoData、Theory 測試 | `/skill dotnet-testing-autodata-xunit-integration` | 參數化測試 |
| 測試輸出、ITestOutputHelper | `/skill dotnet-testing-test-output-logging` | 測試日誌 |
| 覆蓋率、Coverlet | `/skill dotnet-testing-code-coverage-analysis` | 程式碼覆蓋率 |

---

## 使用流程範例

```
使用者：請幫我建立 CreateUserValidator 的測試

AI：我注意到您需要測試 Validator。根據快速對照表，
    我應該載入 dotnet-testing-fluentvalidation-testing skill。

    [使用 Skill tool 載入子技能]

AI：現在按照 FluentValidation Testing skill 的指引為您建立測試...
```

---

## 完整技能清單

如需查看完整的 19 個基礎技能清單、詳細決策樹、學習路徑建議，請繼續閱讀本檔案後續內容。

**人類開發者參考**：如需快速查找，請查看 [SKILLS_QUICK_INDEX.md](/SKILLS_QUICK_INDEX.md)

---

## 快速決策樹

### 我應該從哪裡開始？

#### 情境 1：完全新手，從未寫過測試

**推薦學習路徑**：
1. `dotnet-testing-unit-test-fundamentals` - 理解 FIRST 原則與 3A Pattern
2. `dotnet-testing-test-naming-conventions` - 學習命名規範
3. `dotnet-testing-xunit-project-setup` - 建立第一個測試專案

**為什麼這樣學**：
- FIRST 原則是所有測試的基礎，先建立正確的觀念
- 命名規範讓測試易讀易維護
- 實際動手建立專案，將理論轉化為實踐

---

#### 情境 2：會寫基礎測試，但測試資料準備很麻煩

**推薦技能（擇一或組合）**：

**選項 A - 自動化優先**
→ `dotnet-testing-autofixture-basics`
適合：需要大量測試資料、減少樣板程式碼

**選項 B - 擬真資料優先**
→ `dotnet-testing-bogus-fake-data`
適合：需要真實感的測試資料（姓名、地址、Email 等）

**選項 C - 語意清晰優先**
→ `dotnet-testing-test-data-builder-pattern`
適合：需要高可讀性、明確表達測試意圖

**選項 D - 兩者兼具**
→ `dotnet-testing-autofixture-basics` + `dotnet-testing-autofixture-bogus-integration`
適合：同時需要自動化和擬真資料

---

#### 情境 3：有外部依賴（資料庫、API、第三方服務）需要模擬

**推薦技能組合**：
1. `dotnet-testing-nsubstitute-mocking` - NSubstitute Mock 框架基礎
2. `dotnet-testing-autofixture-nsubstitute-integration` - （可選）整合 AutoFixture 與 NSubstitute

**何時需要第二個技能**：
- 如果您已經在使用 AutoFixture 產生測試資料
- 想要自動建立 Mock 物件，減少手動設定

---

#### 情境 4：測試中有特殊場景

**時間相關測試**
→ `dotnet-testing-datetime-testing-timeprovider`
處理：DateTime.Now、時區轉換、時間計算

**檔案系統測試**
→ `dotnet-testing-filesystem-testing-abstractions`
處理：檔案讀寫、目錄操作、路徑處理

**私有/內部成員測試**
→ `dotnet-testing-private-internal-testing`
處理：需要測試 private、internal 成員（但應謹慎使用）

---

#### 情境 5：需要更好的斷言方式

**基礎需求 - 流暢斷言**
→ `dotnet-testing-awesome-assertions-guide`
所有專案都應該使用，提升測試可讀性

**進階需求 - 複雜物件比較**
→ `dotnet-testing-complex-object-comparison`
處理：深層物件比較、DTO 驗證、Entity 比對

**驗證規則測試**
→ `dotnet-testing-fluentvalidation-testing`
處理：測試 FluentValidation 驗證器

---

#### 情境 6：想了解測試覆蓋率

→ `dotnet-testing-code-coverage-analysis`
學習：使用 Coverlet 分析程式碼覆蓋率、產生報告

## 技能分類地圖

將 19 個基礎技能分為 7 大類別（測試基礎、測試資料生成、測試替身、斷言驗證、特殊場景、測試度量、框架整合），每類包含技能對照表、學習路徑與程式碼範例。

> 詳細內容請參閱 [references/skill-classification-map.md](references/skill-classification-map.md)

## 常見任務映射表

提供 7 個常見測試任務（從零建專案、服務依賴測試、時間邏輯測試等）的技能組合推薦、實施步驟與提示詞範例。

> 詳細內容請參閱 [references/task-mapping-table.md](references/task-mapping-table.md)

## 學習路徑建議

規劃新手路徑（1-2 週）與進階路徑（2-3 週）的每日學習計畫，包含技能、學習重點與實作練習。

> 詳細內容請參閱 [references/learning-paths.md](references/learning-paths.md)

## 引導對話範例

展示 AI 如何與您互動，幫助您選擇正確的技能，包含新手入門、處理依賴、特定問題、改善測試等 4 個常見對話場景。

> 詳細內容請參閱 [references/conversation-examples.md](references/conversation-examples.md)

## 與進階技能的關係

完成基礎技能後，如果您需要進行整合測試、API 測試、容器化測試或微服務測試，請參考：

**進階整合測試** → `dotnet-testing-advanced`
- ASP.NET Core 整合測試
- 容器化測試（Testcontainers）
- 微服務測試（.NET Aspire）
- 測試框架升級與遷移

## 輸出格式

- 根據使用者需求推薦具體的子技能名稱與載入指令
- 提供簡短的推薦理由說明為何選擇該技能
- 若涉及多個技能組合，列出建議的學習順序

## 相關資源

### 原始資料來源

- **iThome 鐵人賽系列文章**：[老派軟體工程師的測試修練 - 30 天挑戰](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)
  2025 iThome 鐵人賽 Software Development 組冠軍

- **完整範例程式碼**：[30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)
  包含所有範例專案的可執行程式碼

### 學習文檔

本技能集基於以下完整教材提煉而成：

- **Agent Skills：從架構設計到實戰應用** (docs/Agent_Skills_Mastery.pdf)
  完整涵蓋 Agent Skills 從理論到實踐

- **Claude Code Skills: 讓 AI 變身專業工匠** (docs/Agent_Skills_Architecture.pdf)
  深入解析 Agent Skills 的架構設計

- **.NET Testing：寫得更好、跑得更快** (docs/NET_Testing_Write_Better_Run_Faster.pdf)
  測試執行優化與除錯技巧

## 下一步

選擇符合您需求的技能開始學習，或告訴我您的具體情況，我會推薦最適合的學習路徑！

**快速開始**：
- 新手 → 從 `dotnet-testing-unit-test-fundamentals` 開始
- 有經驗 → 告訴我您遇到的具體問題
- 不確定 → 告訴我您的專案情況，我會幫您分析
