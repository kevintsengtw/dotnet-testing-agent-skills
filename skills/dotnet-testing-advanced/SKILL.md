---
name: dotnet-testing-advanced
description: |
  .NET 進階測試技能總覽與引導中心。當使用者詢問「整合測試」、「API 測試」、「容器化測試」、「微服務測試」、「測試框架遷移」、「Testcontainers」、「Aspire 測試」等進階測試需求時觸發。會根據具體需求推薦適合的子技能組合，涵蓋整合測試、Testcontainers、Aspire 測試、框架升級等 8 個進階技能。
  Keywords: integration testing, 整合測試, API testing, advanced testing, 進階測試, testcontainers, aspire testing, WebApplicationFactory, TestServer, database test, 資料庫測試, EF Core test, MongoDB test, Redis test, Docker test, 容器測試, microservice test, 微服務測試, .NET Aspire, xUnit upgrade, TUnit, framework migration
license: MIT
metadata:
  author: Kevin Tseng
  version: "1.0.0"
  tags: ".NET, testing, integration, advanced, testcontainers, aspire"
  related_skills: "dotnet-testing"
  skill_count: 8
  skill_type: "overview"
---

# .NET 進階測試技能總覽

---

## 🤖 AI Agent 重要提示

**當您（AI Agent）被載入此進階入口 skill 時，請先閱讀以下指引**：

### 📋 本技能的定位

本檔案是「進階測試導航中心」，用於幫助找到正確的**進階子技能**。

#### 您的任務是

1. ✅ 根據使用者需求匹配對應的進階子技能
2. ✅ 使用 `Skill` tool 載入具體的子技能
3. ✅ 讓子技能提供專業的整合測試指引

#### 禁止行為

- ❌ 不要在本入口 skill 中直接提供整合測試程式碼
- ❌ 不要在沒有載入子技能的情況下開始實作測試
- ❌ 不要跳過子技能直接提供「一般性」整合測試建議

---

## 🎯 快速技能對照表（AI Agent 必讀）

**使用者提到的關鍵字 → 應載入的進階子技能**

### 整合測試技能

| 使用者說... | 載入指令 | 用途說明 |
|------------|----------|----------|
| **API 測試**、Controller 測試、端點測試 | `/skill dotnet-testing-advanced-aspnet-integration-testing` | 基礎 API 整合測試 |
| **完整 CRUD**、WebAPI 測試、業務流程測試 | `/skill dotnet-testing-advanced-webapi-integration-testing` | 完整 API 流程測試 |
| **WebApplicationFactory**、TestServer | `/skill dotnet-testing-advanced-aspnet-integration-testing` | WebApplicationFactory 使用 |

### 容器化測試技能

| 使用者說... | 載入指令 | 用途說明 |
|------------|----------|----------|
| **SQL Server 容器**、PostgreSQL、MySQL | `/skill dotnet-testing-advanced-testcontainers-database` | 關聯式資料庫容器測試 |
| **MongoDB**、Redis、Elasticsearch | `/skill dotnet-testing-advanced-testcontainers-nosql` | NoSQL 資料庫容器測試 |
| **真實資料庫**、EF Core 測試、Dapper 測試 | `/skill dotnet-testing-advanced-testcontainers-database` | 真實資料庫行為測試 |
| **Testcontainers**、容器測試、Docker 測試 | `/skill dotnet-testing-advanced-testcontainers-database` | Testcontainers 基礎 |

### 微服務測試技能

| 使用者說... | 載入指令 | 用途說明 |
|------------|----------|----------|
| **.NET Aspire**、微服務測試、分散式測試 | `/skill dotnet-testing-advanced-aspire-testing` | Aspire 微服務測試 |
| **DistributedApplication**、服務間通訊 | `/skill dotnet-testing-advanced-aspire-testing` | Aspire 應用測試 |

### 框架升級技能

| 使用者說... | 載入指令 | 用途說明 |
|------------|----------|----------|
| **xUnit 升級**、xUnit 3.x、版本升級 | `/skill dotnet-testing-advanced-xunit-upgrade-guide` | xUnit 2.x → 3.x 升級 |
| **TUnit**、新測試框架、TUnit 基礎 | `/skill dotnet-testing-advanced-tunit-fundamentals` | TUnit 基礎與遷移 |
| **TUnit 進階**、TUnit DI、平行執行 | `/skill dotnet-testing-advanced-tunit-advanced` | TUnit 進階功能 |

---

## ⚠️ 使用流程範例

### ✅ 正確流程

```
使用者：請幫我建立 ProductsController 的 API 整合測試

AI：我注意到您需要進行 API 整合測試。根據快速對照表，
    我應該載入 dotnet-testing-advanced-aspnet-integration-testing skill。

    [使用 Skill tool 載入子技能]

AI：現在按照 ASP.NET Core Integration Testing skill 的指引為您建立測試...
```

### ❌ 錯誤流程

```
使用者：請幫我建立 ProductsController 的 API 整合測試

AI：好的，我來寫整合測試...（直接開始寫程式碼，沒有載入子技能）
```

---

## 📚 完整技能清單

如需查看完整的 8 個進階技能清單、詳細決策樹、學習路徑建議，請繼續閱讀本檔案後續內容。

**人類開發者參考**：如需快速查找，請查看 [SKILLS_QUICK_INDEX.md](/SKILLS_QUICK_INDEX.md)

---

## 適用情境

當您需要以下進階測試能力時，我會協助您找到正確的技能：

- 測試完整的 Web API 端點與 HTTP 流程
- 在測試中使用真實的資料庫（容器化）
- 測試 NoSQL 資料庫（MongoDB、Redis 等）
- 測試微服務架構與分散式系統
- 升級測試框架版本（xUnit 2.x → 3.x）
- 遷移到新測試框架（TUnit）
- 建立端到端的整合測試

## 快速決策樹

根據測試情境（API 測試、真實資料庫、微服務、框架遷移）快速找到對應的進階子技能。涵蓋 4 大情境與多個選項分支。

> 📖 詳細內容請參閱 [references/decision-tree.md](references/decision-tree.md)

---

## 技能分類地圖

8 個進階技能分為三大類：整合測試（4 個）、微服務測試（1 個）、框架遷移（3 個），包含各技能的核心價值、適合情境、學習難度與前置技能。

> 📖 詳細內容請參閱 [references/skill-classification-map.md](references/skill-classification-map.md)

---

## 常見任務映射表

7 個常見任務的完整映射，包含情境描述、推薦技能、實施步驟、提示詞範例與預期程式碼結構。

> 📖 詳細內容請參閱 [references/task-mapping-table.md](references/task-mapping-table.md)

---

## 整合測試層級對應

根據專案複雜度，選擇適合的測試策略：

### Level 1：簡單的 WebApi 專案

**專案特徵**：
- 簡單的 CRUD API
- 無外部依賴或使用記憶體實作
- 業務邏輯簡單

**推薦技能**：
- `dotnet-testing-advanced-aspnet-integration-testing`

**測試重點**：
- 路由驗證
- 模型綁定
- HTTP 回應
- 基本業務邏輯

**範例專案**：
- TodoList API
- 簡單的產品目錄

---

### Level 2：相依 Service 的 WebApi 專案

**專案特徵**：
- 有業務邏輯層（Services）
- 依賴外部服務（可以 Mock）
- 中等複雜度

**推薦技能組合**：
1. `dotnet-testing-advanced-aspnet-integration-testing`（基礎）
2. `dotnet-testing-nsubstitute-mocking`（模擬依賴）

**測試策略**：
- 使用 NSubstitute 建立 Service stub
- 測試 Controller 與 Service 的互動
- 驗證錯誤處理

**範例專案**：
- 電商 API（有庫存、訂單服務）
- CMS 系統

---

### Level 3：完整的 WebApi 專案

**專案特徵**：
- 複雜的業務邏輯
- 需要真實資料庫
- 可能有外部 API 整合
- 完整的錯誤處理

**推薦技能組合**：
1. `dotnet-testing-advanced-webapi-integration-testing`（完整流程）
2. `dotnet-testing-advanced-testcontainers-database`（真實資料庫）
3. `dotnet-testing-advanced-testcontainers-nosql`（如有使用 NoSQL）

**測試策略**：
- 使用 Testcontainers 建立真實資料庫
- 完整的端到端測試
- 測試資料準備與清理
- 驗證所有錯誤情境

**範例專案**：
- 大型電商平台
- 企業級管理系統
- SaaS 應用

---

## 學習路徑建議

包含整合測試入門（1 週）、微服務測試專精（3-5 天）、框架遷移路徑等完整學習計畫與每日學習重點。

> 📖 詳細內容請參閱 [references/learning-paths.md](references/learning-paths.md)

---

## 技能組合建議

根據不同的專案需求，推薦以下技能組合：

### 組合 1：完整 API 測試專案

**適合**：建立正式專案的完整測試套件

**技能組合**：
1. `dotnet-testing-advanced-aspnet-integration-testing`（基礎）
2. `dotnet-testing-advanced-testcontainers-database`（真實資料庫）
3. `dotnet-testing-advanced-webapi-integration-testing`（完整流程）

**學習順序**：
1. 先學 aspnet-integration-testing 理解基礎
2. 再學 testcontainers-database 掌握資料庫測試
3. 最後學 webapi-integration-testing 整合應用

**預期成果**：
- 能為 Web API 專案建立完整測試
- 使用真實資料庫驗證行為
- 測試所有 CRUD 端點與錯誤處理

---

### 組合 2：微服務測試方案

**適合**：微服務架構、分散式系統

**技能組合**：
1. `dotnet-testing-advanced-aspire-testing`（核心）
2. `dotnet-testing-advanced-testcontainers-database`（資料庫）
3. `dotnet-testing-advanced-testcontainers-nosql`（NoSQL）

**學習順序**：
1. 先學 testcontainers（資料庫測試基礎）
2. 再學 aspire-testing（微服務測試）

**預期成果**：
- 測試 .NET Aspire 專案
- 驗證服務間通訊
- 使用容器化環境測試

---

### 組合 3：框架現代化

**適合**：測試框架升級或遷移

#### 選項 A：xUnit 升級
**技能**：
- `dotnet-testing-advanced-xunit-upgrade-guide`

**適合**：
- 現有專案使用 xUnit 2.x
- 想升級到最新版本

---

#### 選項 B：TUnit 遷移
**技能組合**：
1. `dotnet-testing-advanced-tunit-fundamentals`（基礎）
2. `dotnet-testing-advanced-tunit-advanced`（進階）

**適合**：
- 新專案選擇測試框架
- 考慮從 xUnit 遷移

**學習順序**：
1. 先學 fundamentals 了解基礎
2. 再學 advanced 掌握進階功能

---

## 前置技能要求

學習進階技能前，建議先掌握以下基礎技能（來自 `dotnet-testing` 基礎技能集）：

### 必備技能

#### 1. dotnet-testing-unit-test-fundamentals
**為什麼必須**：
- 整合測試也遵循 3A Pattern
- FIRST 原則同樣適用
- 需要理解測試基礎概念

---

#### 2. dotnet-testing-xunit-project-setup
**為什麼必須**：
- 需要建立測試專案
- 理解專案結構
- 了解套件管理

---

#### 3. dotnet-testing-awesome-assertions-guide
**為什麼必須**：
- 整合測試需要驗證 HTTP 回應
- FluentAssertions.Web 提供強大的 API 斷言
- 提升測試可讀性

---

### 推薦技能

#### 1. dotnet-testing-nsubstitute-mocking
**為什麼推薦**：
- 整合測試中可能需要 Mock 外部服務
- WebApplicationFactory 需要替換服務

---

#### 2. dotnet-testing-autofixture-basics
**為什麼推薦**：
- 快速產生測試資料
- 減少整合測試的樣板程式碼

---

## 引導對話範例

4 個完整的對話範例，展示 AI 如何引導選擇正確技能：API 測試、微服務測試、框架升級、TUnit 評估。

> 📖 詳細內容請參閱 [references/conversation-examples.md](references/conversation-examples.md)

---

## 與基礎技能的關係

進階技能建立在基礎技能之上：

**基礎測試能力** → `dotnet-testing`（基礎技能集）
- 單元測試基礎
- 測試資料生成
- 斷言與模擬
- 特殊場景處理

**↓ 進階應用**

**進階整合測試** → `dotnet-testing-advanced`（本技能集）
- Web API 整合測試
- 容器化測試
- 微服務測試
- 框架升級

**學習建議**：
先完成 `dotnet-testing` 基礎技能集的核心技能，再進入本進階技能集。

---

## 相關資源

### 原始資料來源

- **iThome 鐵人賽系列文章**：[老派軟體工程師的測試修練 - 30 天挑戰](https://ithelp.ithome.com.tw/users/20066083/ironman/8276)
  🏆 2025 iThome 鐵人賽 Software Development 組冠軍

- **完整範例程式碼**：[30Days_in_Testing_Samples](https://github.com/kevintsengtw/30Days_in_Testing_Samples)
  包含所有範例專案的可執行程式碼

### 技術需求

**整合測試技能**：
- .NET 8+
- Docker Desktop
- WSL2（Windows 環境）

**Aspire 測試技能**：
- .NET 8+
- .NET Aspire Workload
- Docker Desktop

---

## 下一步

選擇符合您需求的進階技能開始學習，或告訴我您的具體情況，我會推薦最適合的學習路徑！

**快速開始**：
- 想測試 API → 從 `dotnet-testing-advanced-aspnet-integration-testing` 開始
- 需要真實資料庫 → 從 `dotnet-testing-advanced-testcontainers-database` 開始
- 微服務專案 → 使用 `dotnet-testing-advanced-aspire-testing`
- 框架升級 → 使用對應的升級指南
- 不確定 → 告訴我您的專案情況，我會幫您分析
